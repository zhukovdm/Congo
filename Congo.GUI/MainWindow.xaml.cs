using Congo.Core;
using Congo.GUI.Wrappers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Congo.GUI
{
    internal enum MainState { INIT, FR, TO, AI, NET, END, ERR, EXIT }

    internal static class MainStatePrimitives
    {
        public static MainState GameToState(CongoGame game, CongoUser white, CongoUser black, CongoMove move)
        {
            MainState state;
            var user = game.GetActiveUser(white, black);

            if (game.HasEnded()) { state = MainState.END; }

            else if (user is Ai) { state = MainState.AI; }

            else if (user is Hi) { state = (move is MonkeyJump) ? MainState.TO : MainState.FR; }

            else { state = MainState.NET; }

            return state;
        }
    }

    internal sealed record Step
    {
        public int MoveFr { get; }
        public CongoGame Game { get; }
        public CongoMove Move { get; }
        public MainState State { get; }

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

        // network
        private NetPack netPack = null;
        private ImmutableList<CongoMove> movesOut = new List<CongoMove>().ToImmutableList();

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

        #region async control

        private void worker_DoWork(object sender, DoWorkEventArgs e)
            => e.Result = ((AsyncJob)e.Argument).Run();

        private void aiAdvise_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            job = null;

            if (step is null) {
                var advise = (AsyncAdvise)e.Result;
                var g = advise.Game; var m = advise.Move; var fr = -1;

                if (m is null) { m = Algorithm.Random(g); }
                g = g.Transition(m);

                var s = MainStatePrimitives.GameToState(g, whiteUser, blackUser, m);

                applyStep(s, g, m, fr);
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
                        var g = netMove.Game; CongoMove m = null; var fr = -1;
                        var s = MainStatePrimitives.GameToState(g, whiteUser, blackUser, m);

                        netPack = netMove.NetPack.WithMoveId(netMove.NetPack.MoveId + netMove.DbMoves.Count); // state update!
                        statusPanelWrapper.AppendMoves(netMove.DbMoves);

                        applyStep(s, g, m, fr);
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

        private void netMove_Init(ImmutableList<CongoMove> moves)
        {
            if (job is null) {

                job = new AsyncNetMove(netPack.Clone(), moves, whiteUser, blackUser);
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

                if (m is not null) {
                    statusPanelWrapper.AppendMove(m);
                    movesOut = movesOut.Add(m);
                }

                switch (s) {

                    case MainState.INIT:
                        netPack = null;
                        movesOut = movesOut.Clear();
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
                        netMove_Init(movesOut);
                        movesOut = movesOut.Clear();
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
                        if (movesOut.Count > 0) {
                            netMove_Init(movesOut);
                            movesOut = movesOut.Clear();
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
            var s = MainState.TO; var g = game; CongoMove m = null;

            if (g.Board.IsFriendlyPiece(g.ActivePlayer.Color, fr)) {
                applyStep(s, g, m, fr);
            }
        }

        private void stepTileTo(int to)
        {
            MainState s; var g = game; var fr = moveFr;
            var m = g.ActivePlayer.Accept(new CongoMove(fr, to));

            // also covers monkey jump termination, when fr == to is a move
            if (m is not null) {
                g = g.Transition(m);
                s = MainStatePrimitives.GameToState(g, whiteUser, blackUser, m);
                if (m is MonkeyJump) { fr = to; }
            }

            else if (fr == to) { s = MainState.FR; } // selection reset

            else { /* do nothing */ return; }

            applyStep(s, g, m, fr);
        }

        #endregion

        #region event control

        private void spawnNewGameDialog<T>(object sender, EventArgs e)
            where T : Window, IPlayablePopup, new()
        {
            var dialog = new T();
            if (dialog.ShowDialog() == true) {

                resetGame();

                whiteUser = dialog.WhiteUser;
                blackUser = dialog.BlackUser;

                if (dialog.PopupPack is not null) {
                    netPack = dialog.PopupPack.NetPack.WithMoveId(dialog.PopupPack.NetPack.MoveId + dialog.PopupPack.DbMoves.Count);
                    statusPanelWrapper.SetId(netPack.GameId);
                    statusPanelWrapper.AppendMoves(dialog.PopupPack.DbMoves);
                }

                gameMenuWrapper.Disable();
                menuItemCancel.IsEnabled = true;
                menuItemPause.IsEnabled = (whiteUser is Ai) && (blackUser is Ai);

                applyStep(MainStatePrimitives.GameToState(dialog.Game, whiteUser, blackUser, null), dialog.Game, null, -1);
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
