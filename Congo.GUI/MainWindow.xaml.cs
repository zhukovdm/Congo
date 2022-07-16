using Congo.Core;
using Congo.GUI.Wrappers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Congo.GUI
{
    internal enum MainState { INIT, FR, TO, AI, NET, END, ERR, EXIT }

    internal sealed record Step
    {
        public int MoveFr;
        public CongoGame Game;
        public CongoMove Move;
        public MainState State;

        public Step(MainState s, CongoGame g, CongoMove m, int f)
        {
            State = s; Game = g; Move = m; MoveFr = f;
        }
    }

    public sealed partial class MainWindow : Window
    {
        private const string whiteLiteral = "white";
        private const string blackLiteral = "black";

        // state
        private int moveFr;
        private CongoGame game;
        private MainState state;
        private CongoUser whiteUser, blackUser;
        private NetPack netPack = null;

        // async
        private Step step = null;
        private AsyncJob job = null;

        // general wrappers
        private readonly List<IBaseWrapper> wrappers;
        private readonly AdvisePanelWrapper advisePanelWrapper;
        private readonly BoardWrapper boardWrapper;
        private readonly ControlMenuWrapper controlMenuWrapper;
        private readonly GameMenuWrapper gameMenuWrapper;
        private readonly StatusPanelWrapper statusPanelWrapper;
        private readonly UserPanelWrapper userPanelWrapper;

        // specific wrappers
        private readonly HiAdviseWrapper hiAdviseWrapper;

        private MainState gameToState(CongoGame g)
        {
            MainState s;
            var u = g.GetActiveUser(whiteUser, blackUser);

            if (g.HasEnded()) { s = MainState.END; }
            else if (u is Ai) { s = MainState.AI; }
            else if (u is Hi) { s = MainState.FR; }
            else { s = MainState.NET; }

            return s;
        }

        #region async control

        private void worker_DoWork(object sender, DoWorkEventArgs e)
            => e.Result = ((AsyncJob)e.Argument).Run();

        private void aiAdvise_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            job = null;

            if (step is null) {

                var advise = (AsyncAdvise)e.Result;
                var g = advise.Game;
                var m = advise.Move;

                if (m is null) { m = Algorithm.Random(g); }
                g = g.Transition(m);

                applyStep(gameToState(g), g, m, -1);
            }

            else {
                applyStep(step.State, step.Game, step.Move, step.MoveFr);
            }

            step = null;
        }

        private void aiAdvise_Init()
        {
            if (job is null) {

                job = new AsyncAdvise(game, game.GetActiveUser(whiteUser, blackUser), whiteUser is Ai && blackUser is Ai);
                AsyncPrimitives.GetWorker(worker_DoWork, aiAdvise_Complete)
                               .RunWorkerAsync(argument: job);
            }
        }

        private void hiAdvise_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            job = null;

            if (step is null) {

                var advise = (AsyncAdvise)e.Result;
                var m = advise.Move;

                if (m is null) { m = Algorithm.Random(advise.Game); }
                hiAdviseWrapper.End(m);
            }

            else {
                applyStep(step.State, step.Game, step.Move, step.MoveFr);
            }

            step = null;
        }

        private void hiAdvise_Init()
        {
            if (job is null) {

                job = new AsyncAdvise(game, game.GetActiveUser(whiteUser, blackUser), false);
                hiAdviseWrapper.Begin();
                AsyncPrimitives.GetWorker(worker_DoWork, hiAdvise_Complete)
                               .RunWorkerAsync(argument: job);
            }
        }

        private void netMove_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            job = null;

            if (step is null) {

                var netMove = (AsyncNetMove)e.Result;
                switch(netMove.Status) {

                    case NetStatus.Ok:
                        netPack = netMove.NetPack;
                        statusPanelWrapper.AppendMoves(netMove.Moves);
                        applyStep(gameToState(netMove.Game), netMove.Game, null, -1);
                        break;

                    default:
                        statusPanelWrapper.SetErrorMessage(netMove.ErrorMessage);
                        applyStep(MainState.ERR, null, null, -1);
                        break;
                }
            }

            else {
                applyStep(step.State, step.Game, step.Move, step.MoveFr);
            }

            step = null;
        }

        private void netMove_Init(CongoMove move)
        {
            if (job is null) {

                job = new AsyncNetMove(netPack.Clone(), move, whiteUser, blackUser);
                AsyncPrimitives.GetWorker(worker_DoWork, netMove_Complete)
                               .RunWorkerAsync(argument: job);
            }
        }

        #endregion

        #region window control

        private void applyStep(MainState s, CongoGame g, CongoMove m, int f)
        {
            if (job is not null && step is null) {
                job.Abandon();
                step = new(s, g, m, f);
            }

            else if (job is null) {

                var oldState = state;
                state = s; game = g; moveFr = f;

                if (m is not null) { statusPanelWrapper.AppendMove(m); }

                switch (s) {

                    case MainState.INIT:
                        netPack = null;
                        foreach (var wrapper in wrappers) { wrapper.Init(); }
                        GC.Collect();
                        break;

                    case MainState.FR:
                        userPanelWrapper.Draw(g);
                        boardWrapper.Draw(g, s, f);
                        advisePanelWrapper.Activate(oldState, s);
                        break;

                    case MainState.TO:
                        boardWrapper.Draw(g, s, f);
                        advisePanelWrapper.Activate(oldState, s);
                        break;

                    case MainState.AI:
                        userPanelWrapper.Draw(g);
                        boardWrapper.Draw(g, s, f);
                        advisePanelWrapper.Deactivate();
                        aiAdvise_Init();
                        break;

                    case MainState.NET:
                        userPanelWrapper.Draw(g);
                        boardWrapper.Draw(g, s, f);
                        advisePanelWrapper.Deactivate();
                        netMove_Init(m);
                        break;

                    case MainState.END:
                        gameMenuWrapper.Init();
                        controlMenuWrapper.Init();
                        userPanelWrapper.Init();
                        advisePanelWrapper.Init();
                        boardWrapper.Draw(g, s, f);

                        var message = g.WhitePlayer.HasLion
                            ? whiteLiteral
                            : blackLiteral;
                        statusPanelWrapper.SetStatus(message + " wins");

                        // special case, local winning step
                        if (g.GetActiveUser(whiteUser, blackUser) is Net && m is not null) {
                            netMove_Init(m);
                        }

                        break;

                    case MainState.ERR:
                        gameMenuWrapper.Init();
                        controlMenuWrapper.Init();
                        userPanelWrapper.Init();
                        advisePanelWrapper.Init();

                        break;

                    case MainState.EXIT:
                        Application.Current.Shutdown();
                        break;

                    default:
                        break;
                }
            }

            else { /* do nothing, step is rejected. */ }
        }

        private void cancelGameWithState(MainState s)
        {
            AsyncJob.Resume(menuItemPause);
            applyStep(s, null, null, -1);
        }

        private void initGame() => cancelGameWithState(MainState.INIT);

        private void resetGame() => initGame();

        private void exitGame() => cancelGameWithState(MainState.EXIT);

        private void stepTileFr(int fr)
        {
            if (game.Board.IsFriendlyPiece(game.ActivePlayer.Color, fr)) {
                applyStep(MainState.TO, game, null, fr);
            }
        }

        private void stepTileTo(int to)
        {
            var s = state; var g = game; var f = moveFr;
            var m = game.ActivePlayer.Accept(new CongoMove(moveFr, to));

            // also covers monkey jump termination, when moveFr == to
            if (m is not null) {

                g = game.Transition(m);

                if (g.HasEnded()) { s = MainState.END; }

                else if (m is MonkeyJump) { f = to; }

                else {
                    var u = g.ActivePlayer.IsWhite() ? whiteUser : blackUser;

                    if (u is Ai) { s = MainState.AI; }
                    if (u is Hi) { s = MainState.FR; }
                    if (u is Net) { s = MainState.NET; }
                }
            }

            // not a move, but reset
            else if (moveFr == to) { s = MainState.FR; }

            else { /* do nothing */ return; }

            applyStep(s, g, m, f);
        }

        #endregion

        #region event control

        private void spawnNewGameDialog<T>(object sender, EventArgs e)
            where T : Window, IPlayable, new()
        {
            var dialog = new T();
            if (dialog.ShowDialog() == true) {

                resetGame();

                whiteUser = dialog.WhiteUser;
                blackUser = dialog.BlackUser;

                if (dialog.PopupPack is not null) {
                    netPack = dialog.PopupPack.NetPack;
                    statusPanelWrapper.SetId(netPack.GameId);
                    statusPanelWrapper.AppendMoves(dialog.PopupPack.Moves);
                    netPack.MoveId += dialog.PopupPack.Moves.Count;
                }

                gameMenuWrapper.Disable();
                menuItemCancel.IsEnabled = true;
                menuItemPause.IsEnabled = (whiteUser is Ai) && (blackUser is Ai);

                applyStep(gameToState(dialog.Game), dialog.Game, null, -1);
            }
        }

        private void menuItemLocal_Click(object sender, RoutedEventArgs e)
            => spawnNewGameDialog<MenuLocalPopup>(sender, e);

        private void menuItemNetwork_Click(object sender, RoutedEventArgs e)
            => spawnNewGameDialog<MenuNetworkPopup>(sender, e);

        private void menuItemSave_Click(object sender, RoutedEventArgs e)
        {
            var r = (game is not null) ? CongoFen.ToFen(game) : string.Empty;
            Clipboard.SetText(r);
        }

        private void menuItemPause_Click(object sender, RoutedEventArgs e)
            => AsyncJob.Invert(menuItemPause);

        private void menuItemCancel_Click(object sender, RoutedEventArgs e)
            => job?.Cancel();

        private void menuItemReset_Click(object sender, RoutedEventArgs e)
            => resetGame();

        private void menuItemExit_Click(object sender, RoutedEventArgs e)
            => exitGame();

        private void tile_Click(object sender, RoutedEventArgs e)
        {
            switch (state) {

                case MainState.FR:
                    if (sender is Canvas tileFr) {
                        stepTileFr(int.Parse((string)tileFr.Tag));
                    }
                    break;

                case MainState.TO:
                    if (sender is Canvas tileTo) {
                        stepTileTo(int.Parse((string)tileTo.Tag));
                    }
                    break;

                default:
                    break;
            }
        }

        private void buttonAdvise_Click(object sender, RoutedEventArgs e)
            => hiAdvise_Init();

        private void textBlockGameId_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => Clipboard.SetText(textBlockGameId.Text);

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            advisePanelWrapper = new(buttonAdvise, textBlockAdvise);
            boardWrapper = new(panelCongoBoard, tile_Click);
            controlMenuWrapper = new(menuItemPause, menuItemCancel);
            gameMenuWrapper = new(menuItemLocal, menuItemNetwork);
            statusPanelWrapper = new(textBlockGameId, listBoxMoves, textBlockStatus, textBlockErrorMessage);
            userPanelWrapper = new(borderWhitePlayer, borderBlackPlayer);

            wrappers = new() {
                advisePanelWrapper, boardWrapper, controlMenuWrapper,
                gameMenuWrapper, statusPanelWrapper, userPanelWrapper,
            };

            hiAdviseWrapper = new(buttonAdvise, textBlockAdvise);

            initGame();
        }
    }
}
