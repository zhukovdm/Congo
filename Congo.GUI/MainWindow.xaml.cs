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
        private readonly HiAdviseWrapper hiAdviseWrapper;

        private MainState gameToState(CongoGame g)
        {
            MainState s;

            var u = g.ActivePlayer.IsWhite() ? whiteUser : blackUser;

            if (g.HasEnded()) { s = MainState.END; }
            else if (u is Ai) { s = MainState.AI; }
            else if (u is Hi) { s = MainState.FR; }
            else { s = MainState.NET; }

            return s;
        }

        private CongoUser gameToUser(CongoGame g)
            => g.ActivePlayer.IsWhite() ? whiteUser : blackUser;

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
                job = new AsyncAdvise(game, gameToUser(game), whiteUser is Ai && blackUser is Ai);
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
                job = new AsyncAdvise(game, gameToUser(game), false);
                hiAdviseWrapper.Begin();
                AsyncPrimitives.GetWorker(worker_DoWork, hiAdvise_Complete)
                               .RunWorkerAsync(argument: job);
            }
        }

        private void netMove_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            job = null;

            if (step is null) {
                // TODO: network move completer
            }

            else {
                applyStep(step.State, step.Game, step.Move, step.MoveFr);
            }

            step = null;
        }

        private void netMove_Init()
        {
            if (job is null) {
                // TODO: network move initializer
            }
        }

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
                step = new(s, g, m, f);
            }

            else if (job is null) {

                var oldState = state;
                state = s; game = g; moveFr = f;

                if (m is not null) { statusPanelWrapper.AppendMove(m); }

                switch (s) {

                    case MainState.INIT:
                        networkPack = null;
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
                    case MainState.NET:
                        userPanelWrapper.Draw(g);
                        boardWrapper.Draw(g, s, f);
                        advisePanelWrapper.Deactivate();
                        click();
                        break;

                    case MainState.END:
                        boardWrapper.Draw(g, s, f);

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
            int f = -1;
            CongoGame g = null;
            CongoMove m = null;
            var s = MainState.INIT;

            if (job is not null) { step = new Step(s, g, m, f); }

            else {
                applyStep(MainState.INIT, null, null, -1);
            }
        }

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
                networkPack = dialog.NetworkPack;

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

            hiAdviseWrapper = new(buttonAdvise, textBlockAdvise);

            applyStep(MainState.INIT, null, null, -1);
        }
    }
}
