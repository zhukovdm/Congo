using Congo.Core;
using Congo.GUI.Wrappers;
using Congo.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Congo.GUI
{
    internal enum MainState { INIT, FR, TO, AI, NET, END, EXIT }

    internal sealed record Step
    {
        public MainState State;
        public CongoGame Game;
        public CongoMove Move;
        public int MoveFr;
    }

    public sealed partial class MainWindow : Window
    {
        // state
        private int moveFr;
        private CongoGame game;
        private MainState state;
        private CongoUser whiteUser, blackUser;
        private CongoNetworkPack networkPack;

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
        private readonly AiAdviseWrapper aiAdviseWrapper;
        private readonly HiAdviseWrapper hiAdviseWrapper;

        private CongoUser activeUser
            => game.ActivePlayer.IsWhite() ? whiteUser : blackUser;

        private MainState nextState(CongoGame g)
        {
            MainState s;

            var u = g.ActivePlayer.IsWhite() ? whiteUser : blackUser;

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

                aiAdviseWrapper.End();
                applyStep(nextState(g), g, m, -1);
            }

            else {
                applyStep(step.State, step.Game, step.Move, step.MoveFr);
            }
        }

        private void aiAdvise_Init()
        {
            if (job is null) {
                job = new AsyncAdvise(game, activeUser, whiteUser is Ai && blackUser is Ai);
                aiAdviseWrapper.Begin();
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
        }

        private void hiAdvise_Init()
        {
            if (job is null) {
                job = new AsyncAdvise(game, activeUser, false);
                hiAdviseWrapper.Begin();
                AsyncPrimitives.GetWorker(worker_DoWork, hiAdvise_Complete)
                               .RunWorkerAsync(argument: job);
            }
        }

        private void netMove_Complete(object sender, RunWorkerCompletedEventArgs e)
            => throw new NotImplementedException();

        private void netMove_Init()
            => throw new NotImplementedException();

        #endregion

        #region window control

        private void click()
        {
            Dispatcher.BeginInvoke(new Action(() => {
                buttonMoveGenerator.PerformClick();
            }));
        }

        private void maybeStep()
        {
            switch (state) {

                case MainState.AI:
                    aiAdvise_Init();
                    break;

                case MainState.NET:
                    netMove_Init();
                    break;

                default:
                    break;
            }
        }

        private void applyStep(MainState s, CongoGame g, CongoMove m, int f)
        {
            if (job is not null && step is null) {
                job.Cancel();
                step = new() { State = s, Game = g, Move = m, MoveFr = f, };
            }

            else if (job is null) {

                if (m is not null) { statusPanelWrapper.AppendMove(m); }

                switch (s) {

                    case MainState.INIT:
                        state = s;
                        networkPack = null;
                        foreach (var wrapper in wrappers) { wrapper.Init(); }
                        GC.Collect();
                        break;

                    case MainState.FR:
                        game = g;
                        state = s;
                        boardWrapper.Draw(g, s, -1);
                        break;

                    case MainState.TO:
                        state = s;
                        moveFr = f;
                        boardWrapper.Draw(game, s, f);
                        break;

                    case MainState.AI:
                    case MainState.NET:
                        game = g;
                        state = s;
                        boardWrapper.Draw(g, s, -1);
                        click();
                        break;

                    case MainState.END:
                        state = s;
                        game = g;
                        boardWrapper.Draw(g, s, -1);

                        gameMenuWrapper.Init();
                        controlMenuWrapper.Init();
                        userPanelWrapper.Init();
                        advisePanelWrapper.Init();

                        var message = game.Opponent.Color.IsWhite()
                            ? "white"
                            : "black";

                        statusPanelWrapper.SetStatus(message + " wins");
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

        private void resetGame()
        {
            if (job is not null) {
                step = new Step()
                {
                    State = MainState.INIT,
                    Game = null,
                    Move = null,
                    MoveFr = -1,
                };
            }

            else {
                applyStep(MainState.INIT, null, null, -1);
            }
        }

        private void stepTileFr(int f)
        {
            if (game.Board.IsFriendlyPiece(game.ActivePlayer.Color, f)) {
                applyStep(MainState.TO, null, null, f);
            }
        }

        private void stepTileTo(int to)
        {
            var g = game; var s = state; var f = moveFr;
            var m = game.ActivePlayer.Accept(new CongoMove(moveFr, to));

            // also covers monkey jump termination, when moveFr == to
            if (m is not null) {

                g = game.Transition(m);

                if (g.HasEnded()) { s = MainState.END; }

                else if (m is MonkeyJump) { f = to; }

                else {
                    if (activeUser is Ai) { s = MainState.AI; }
                    if (activeUser is Hi) { s = MainState.FR; }
                    if (activeUser is Net) { s = MainState.NET; }
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

                game = dialog.Game;
                whiteUser = dialog.WhiteUser;
                blackUser = dialog.BlackUser;
                networkPack = dialog.NetworkPack;

                gameMenuWrapper.Disable();
                menuItemPause.IsEnabled = (whiteUser is Ai) && (blackUser is Ai);

                click();
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
            => applyStep(MainState.EXIT, null, null, -1);

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

        private void buttonStepGenerator_Click(object sender, RoutedEventArgs e)
            => maybeStep();

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            advisePanelWrapper = new(buttonAdvise, textBlockAdvise);
            boardWrapper = new(panelCongoBoard, tile_Click);
            controlMenuWrapper = new(menuItemPause, menuItemCancel);
            gameMenuWrapper = new(menuItemLocal, menuItemNetwork);
            statusPanelWrapper = new(textBlockGameId, listBoxMoves, textBlockStatus);
            userPanelWrapper = new(borderWhitePlayer, borderBlackPlayer);

            wrappers = new() {
                advisePanelWrapper, boardWrapper, controlMenuWrapper,
                gameMenuWrapper, statusPanelWrapper, userPanelWrapper,
            };

            aiAdviseWrapper = new(menuItemCancel);
            hiAdviseWrapper = new(menuItemCancel, buttonAdvise, textBlockAdvise);

            applyStep(MainState.INIT, null, null, -1);
        }
    }
}
