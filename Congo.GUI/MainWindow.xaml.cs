using Congo.Core;
using Congo.GUI.Wrappers;
using Congo.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Congo.GUI
{
    internal enum MainState { INIT, FR, TO, AI, NET, END }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // wrappers
        private readonly AdvisePanelWrapper advisePanelWrapper;
        private readonly BoardWrapper boardWrapper;
        private readonly ControlMenuWrapper controlMenuWrapper;
        private readonly GameMenuWrapper gameMenuWrapper;
        private readonly StatusPanelWrapper statusPanelWrapper;
        private readonly UserPanelWrapper userPanelWrapper;
        private readonly List<IPanelWrapper> wrappers;

        // state
        private int moveFr;
        private CongoGame game;
        private MainState state;
        private CongoUser whiteUser, blackUser;
        private CongoNetworkPack networkPack;

        // async
        private AsyncJob job = null;

        private CongoUser activeUser
            => game.ActivePlayer.IsWhite() ? whiteUser : blackUser;

        #region async control

        private void worker_DoWork(object sender, DoWorkEventArgs e)
            => e.Result = ((AsyncJob)e.Argument).Run();

        private BackgroundWorker getWorker()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;

            return worker;
        }

        private void aiAdvise_Init()
        {
            AsyncJob.Acquire();
            job = new AsyncAdvise(game, whiteUser, blackUser);

            menuItemCancel.IsEnabled = true;

            var worker = getWorker();
            worker.RunWorkerCompleted += aiAdvise_Finalize;

            worker.RunWorkerAsync(argument: job);
        }

        private void aiAdvise_Finalize(object sender, RunWorkerCompletedEventArgs e)
        {
            var advise = (AsyncAdvise)e.Result;
            var move = advise.Move;

            if (!advise.IsAbandoned()) {
                game = advise.Game;

                if (move is null) { move = Algorithm.Random(game); }
                game = game.Transition(move);

                menuItemCancel.IsEnabled = false;
                statusPanelWrapper.AppendMove(move);

                updateGame();
            }

            job = null;
            AsyncJob.Release();
        }

        private void hiAdvise_Init()
        {
            job = AsyncAdvise.GetInstance(game, whiteUser, blackUser);

            buttonAdvise.IsEnabled = false;
            menuItemCancel.IsEnabled = true;

            var worker = getWorker();
            worker.RunWorkerCompleted += hiAdvise_Finalize;

            worker.RunWorkerAsync(argument: job);
        }

        private void hiAdvise_Finalize(object sender, RunWorkerCompletedEventArgs e)
        {
            job = null;

            if (!Algorithm.IsAbandoned()) {
                var move = (CongoMove)e.Result;
                if (move == null) { move = Algorithm.Random(game); }

                buttonAdvise.IsEnabled = true;
                menuItemCancel.IsEnabled = false;
                textBlockAdvise.Text = MovePresenter.GetMoveView(move);
            }

            job = null;
        }

        private void netMove_Init()
        {

        }

        private void netMove_Finalize(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        #endregion

        #region window control

        private void cleanAdvice()
            => textBlockAdvise.Text = "";

        private void initState()
        {
            if (game.HasEnded()) { state = MainState.END; }

            else if (activeUser is Ai) { state = MainState.AI; }

            else if (game.FirstMonkeyJump != null) { state = MainState.TO; buttonAdvise.IsEnabled = true; }

            else { state = MainState.FR; buttonAdvise.IsEnabled = true; }
        }

        private void updateState()
        {
            // determine new state

            if (game.HasEnded()) { state = MainState.END; }

            else if (activeUser is Ai) { state = MainState.AI; }

            else { state = MainState.FR; }
        }

        private void finalizeState()
        {
            // maybe act on new state

            switch (state) {

                case MainState.AI:
                    buttonAdvise.IsEnabled = false;
                    aiAdvise_Init();
                    break;

                case MainState.FR:
                    buttonAdvise.IsEnabled = true;
                    break;

                case MainState.END:
                    buttonAdvise.IsEnabled = false;
                    borderWhitePlayer.BorderBrush = Brushes.Transparent;
                    borderBlackPlayer.BorderBrush = Brushes.Transparent;

                    var c = game.Opponent.Color.IsWhite() ? "White" : "Black";
                    MessageBox.Show($"{c} wins.");
                    menuItemLocal.IsEnabled = true;
                    menuItemNetwork.IsEnabled = true;
                    break;

                default:
                    break;
            }
        }

        private void updateGame()
        {
            updateState();
            drawBoard();
            drawPanel();

            Dispatcher.BeginInvoke(new Action(() => {
                buttonMoveGenerator.PerformClick();
            }));
        }

        private void finalizeWorkers()
        {
            if (worker is not null || networkWorker is not null) { syncPrimitive.Disable(); }

            syncPrimitive.Wait();

            pauseEvent.Set(); // maybe paused worker
            adviceEvent.Wait();
        }

        private void initGame()
        {
            Dispatcher.BeginInvoke(new Action(() => {
                buttonMoveGenerator.PerformClick();
            }));
        }

        private void resetGame()
        {
            finalizeWorkers();

            foreach (var wrapper in wrappers) { wrapper.Reset(); }

            state = MainState.INIT;
            networkPack = null;

            GC.Collect();

            // panel
            borderWhitePlayer.BorderBrush = Brushes.Transparent;
            borderBlackPlayer.BorderBrush = Brushes.Transparent;
            buttonAdvise.IsEnabled = false;
            cleanAdvice();
            listBoxMoves.Items.Clear();
            textBlockStatus.Text = "";
        }

        private void exitGame()
        {
            finalizeWorkers();
            Application.Current.Shutdown();
        }

        private void updateGameTileFr(int fr)
        {
            if (game.Board.IsFriendlyPiece(game.ActivePlayer.Color, fr)) {
                moveFr = fr;
                state = MainState.TO;
                boardWrapper.Draw(game, state, fr);
            }
        }

        private void updateGameTileTo(int to)
        {
            var move = game.ActivePlayer.Accept(new CongoMove(moveFr, to));

            // also covers monkey jump termination, when moveFr == to
            if (move is not null) {

                game = game.Transition(move); // global game is replaced!

                if (game.HasEnded()) { state = MainState.END; }

                else if (move is MonkeyJump) { moveFr = to; }

                else {
                    if (activeUser is Ai) { state = MainState.AI; }
                    if (activeUser is Hi) { state = MainState.FR; }
                    if (activeUser is Net) { state = MainState.NET; }
                }

                Algorithm.Cancel(); // maybe running advise
                cleanAdvice();
                statusPanelWrapper.AppendMove(move);
            }

            // not a move, but reset
            else if (moveFr == to) { state = MainState.FR; }

            else { /* do nothing */ }

            boardWrapper.Draw(game, state, moveFr);
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

                initGame();
            }
        }

        private void menuItemLocal_Click(object sender, RoutedEventArgs e)
            => spawnNewGameDialog<MenuLocalPopup>(sender, e);

        private void menuItemNetwork_Click(object sender, RoutedEventArgs e)
            => spawnNewGameDialog<MenuNetworkPopup>(sender, e);

        private void menuItemSave_Click(object sender, RoutedEventArgs e)
        {
            var g = game;
            var r = (g is not null) ? CongoFen.ToFen(g) : string.Empty;
            Clipboard.SetText(r);
        }

        private void menuItemPause_Click(object sender, RoutedEventArgs e)
        {
            // .IsSet means no pause

            if (pauseEvent.IsSet) {
                pauseEvent.Reset();
                menuItemPause.Header = "Resu_me";
            }

            else {
                pauseEvent.Set();
                menuItemPause.Header = "_Pause";
            }
        }

        private void menuItemCancel_Click(object sender, RoutedEventArgs e)
            => Algorithm.Cancel();

        private void menuItemReset_Click(object sender, RoutedEventArgs e)
            => resetGame();

        private void menuItemExit_Click(object sender, RoutedEventArgs e)
            => exitGame();

        private void tile_Click(object sender, RoutedEventArgs e)
        {
            switch (state) {

                case MainState.FR:
                    if (sender is Canvas tileFr) {
                        updateGameTileFr(int.Parse((string)tileFr.Tag));
                    }
                    break;

                case MainState.TO:
                    if (sender is Canvas tileTo) {
                        updateGameTileTo(int.Parse((string)tileTo.Tag));
                    }
                    break;

                default:
                    break;
            }
        }

        private void buttonAdvise_Click(object sender, RoutedEventArgs e)
            => hiAdvise_Init();

        private void buttonMoveGenerator_Click(object sender, RoutedEventArgs e)
            => finalizeState();

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
            foreach (var wrapper in wrappers) { wrapper.Init(); }

            state = MainState.INIT;
        }
    }
}
