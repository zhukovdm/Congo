using Congo.Core;
using Congo.Server;
using Congo.Utils;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Congo.GUI
{
    internal enum MainState : int { INIT, FR, TO, AI, NET, END }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // wrappers
        private readonly BoardWrapper boardWrapper;
        private readonly UserPanelWrapper userPanelWrapper;
        private readonly AdvisePanelWrapper advisePanelWrapper;

        private readonly List<BaseWrapper> wrappers;

        // local game

        // network game

        private int moveFr;
        private int moveTo;
        private MainState state;
        private CongoGame game;
        private CongoUser whiteUser, blackUser;


        long gameId, moveId = -1;
        GrpcChannel channel;
        CongoGrpc.CongoGrpcClient client;

        private BackgroundWorker adviceWorker = null;
        private readonly ManualResetEventSlim adviceEvent;
        private readonly ManualResetEventSlim pauseEvent;

        private CongoUser activeUser
            => game.ActivePlayer.IsWhite() ? whiteUser : blackUser;

        private void appendMove(CongoMove move)
        {
            listBoxMoves.Items.Add(MovePresenter.GetMoveView(move));
            listBoxMoves.ScrollIntoView(listBoxMoves.Items[^1]);
        }

        private void adviceWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            pauseEvent.Wait();

            /* Avoid too fast switching between players. Safe to access 
             * variables, because only reset could remove users, but is
             * waiting for adviceEvent. */
            if (whiteUser is Ai && blackUser is Ai) { Thread.Sleep(1000); }

            var g = (CongoGame)e.Argument;
            var user = g.ActivePlayer.IsWhite() ? whiteUser : blackUser;
            e.Result = user.Advise(g);

            adviceEvent.Set();
        }

        private BackgroundWorker getAdviceWorker()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += adviceWorker_DoWork;

            return worker;
        }

        private void aiAdvice_Init()
        {
            adviceEvent.Wait();
            adviceEvent.Reset(); // ensure mutual exclusion

            buttonMenuCancel.IsEnabled = true;
            adviceWorker = getAdviceWorker();
            adviceWorker.RunWorkerCompleted += aiAdvice_Finalize;

            adviceWorker.RunWorkerAsync(argument: game);
        }

        private void aiAdvice_Finalize(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!Algorithm.IsDisabled()) {
                var move = (CongoMove)e.Result;
                if (move == null) { move = Algorithm.Random(game); }

                game = game.Transition(move);

                if (activeUser is Hi) { buttonAdvise.IsEnabled = true; }

                buttonMenuCancel.IsEnabled = false;
                appendMove(move);
                updateGame();
            }

            adviceWorker = null;
            Algorithm.Enable();
        }

        private void hiAdvice_Finalize(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!Algorithm.IsDisabled()) {
                var move = (CongoMove)e.Result;
                if (move == null) { move = Algorithm.Random(game); }

                buttonAdvise.IsEnabled = true;
                buttonMenuCancel.IsEnabled = false;
                textBlockAdvise.Text = MovePresenter.GetMoveView(move);
            }

            adviceWorker = null;
            Algorithm.Enable();
        }

        /// <summary>
        /// Event handler for clicks on all board tiles.
        /// </summary>
        private void tile_Click(object sender, RoutedEventArgs e)
        {
            switch (state) {

                case MainState.INIT:
                case MainState.AI:
                    break;

                case MainState.FR:

                    if (sender is Canvas tileFrom) {
                        moveFr = int.Parse((string)tileFrom.Tag);
                        updateGame();
                    }
                    break;

                case MainState.TO:

                    if (sender is Canvas tileTo) {
                        moveTo = int.Parse((string)tileTo.Tag);
                        updateGame();
                    }
                    break;

                case MainState.END:
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        #region Ui Buttons

        /// <summary>
        /// Dialog with game and both users always starts new game.
        /// </summary>
        private void buttonNewGameDialog<T>(object sender, EventArgs e)
            where T : Window, IPlayable, new()
        {
            var dialog = new T();
            if (dialog.ShowDialog() == true) {
                resetGame();

                game = dialog.Game;
                whiteUser = dialog.WhiteUser;
                blackUser = dialog.BlackUser;

                buttonMenuLocal.IsEnabled = false;
                buttonMenuNetwork.IsEnabled = false;
                buttonMenuSave.IsEnabled = true;
                buttonMenuPause.IsEnabled = (whiteUser is Ai) && (blackUser is Ai);

                initGame();
            }
        }

        private void buttonMenuLocal_Click(object sender, RoutedEventArgs e)
            => buttonNewGameDialog<MenuLocalPopup>(sender, e);

        private void buttonMenuNetwork_Click(object sender, RoutedEventArgs e)
            => buttonNewGameDialog<MenuNetworkPopup>(sender, e);

        private void buttonMenuSave_Click(object sender, RoutedEventArgs e)
        {
            var g = game;
            var r = (g is not null) ? CongoFen.ToFen(g) : string.Empty;
            Clipboard.SetText(r);
        }

        private void buttonMenuPause_Click(object sender, RoutedEventArgs e)
        {
            // .IsSet means no pause

            if (pauseEvent.IsSet) {
                pauseEvent.Reset();
                buttonMenuPause.Header = "Resu_me";
            }

            else {
                pauseEvent.Set();
                buttonMenuPause.Header = "_Pause";
            }
        }

        private void buttonMenuCancel_Click(object sender, RoutedEventArgs e)
            => Algorithm.Cancel();

        private void buttonMenuReset_Click(object sender, RoutedEventArgs e)
            => resetGame();

        private void buttonMenuExit_Click(object sender, RoutedEventArgs e)
            => exitGame();

        private void buttonAdvise_Click(object sender, RoutedEventArgs e)
        {
            adviceEvent.Wait();
            adviceEvent.Reset(); // ensure mutual exclusion

            buttonAdvise.IsEnabled = false;
            buttonMenuCancel.IsEnabled = true;

            adviceWorker = getAdviceWorker();
            adviceWorker.RunWorkerCompleted += hiAdvice_Finalize;

            adviceWorker.RunWorkerAsync(argument: game);
        }

        private void buttonMoveGenerator_Click(object sender, RoutedEventArgs e)
            => finalizeState();

        #endregion

        #region Draw Game

        private void cleanAdvice()
            => textBlockAdvise.Text = "";

        #endregion

        #region Update Game state

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

            else if (state == MainState.FR && game.Board.IsOccupied(moveFr)
                && game.Board.IsFriendlyPiece(game.ActivePlayer.Color, moveFr)) {
                state = MainState.TO;
            }

            // end of game | monkey jump | move ~> opponent
            else if (state == MainState.TO) {

                var move = game.ActivePlayer.Accept(new CongoMove(moveFr, moveTo));

                /* Move exists, also includes monkey jump interrupt (Fr == To)! */
                if (move != null) {
                    game = game.Transition(move);

                    if (game.HasEnded()) { state = MainState.END; }

                    else if (move is MonkeyJump) { moveFr = moveTo; /* State.TO remains */ }

                    // opponent will turn
                    else {
                        state = (activeUser is Ai) ? MainState.AI : MainState.FR;
                    }

                    cleanAdvice();
                    appendMove(move);
                    Algorithm.Cancel(); // maybe running advice
                }

                // not a move, but reset
                else if (moveFr == moveTo) { state = MainState.FR; }

                else { /* do nothing */ }
            }

            else { state = MainState.FR; }
        }

        private void finalizeState()
        {
            // maybe act on new state

            switch (state) {

                case MainState.AI:
                    buttonAdvise.IsEnabled = false;
                    aiAdvice_Init();
                    break;

                case MainState.FR:
                case MainState.TO:
                    buttonAdvise.IsEnabled = true;
                    break;

                case MainState.END:
                    buttonAdvise.IsEnabled = false;
                    borderWhitePlayer.BorderBrush = Brushes.Transparent;
                    borderBlackPlayer.BorderBrush = Brushes.Transparent;

                    var c = game.Opponent.Color.IsWhite() ? "White" : "Black";
                    MessageBox.Show($"{c} wins.");
                    buttonMenuLocal.IsEnabled = true;
                    buttonMenuNetwork.IsEnabled = true;
                    break;

                case MainState.INIT:
                default:
                    break;
            }
        }

        private void initGame()
        {
            initState();
            drawBoard();
            drawPanel();

            Dispatcher.BeginInvoke(new Action(() => {
                buttonMoveGenerator.PerformClick();
            }));
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

        #endregion

        #region Reset and Exit Game

        private void initGame()
        {

        }

        private void finalizeAdviceWorker()
        {
            /* Algorithm can be disabled __only if__ ongoing advice exist,
             * and will be re-enabled in the advice finalize procedure.
             */
            if (adviceWorker is not null) { Algorithm.Disable(); }

            pauseEvent.Set(); // maybe worker on a break
            adviceEvent.Wait();
        }

        /// <summary>
        /// Upon call, the window reaches predictable state. Remove all code
        /// entities, clean up the board, enable or disable buttons.
        /// </summary>
        private void resetGame()
        {
            /* Code entities */

            finalizeAdviceWorker();

            state = MainState.INIT;
            game = null;
            whiteUser = null;
            blackUser = null;

            /* Gui entities */

            // menu
            buttonMenuLocal.IsEnabled = true;
            buttonMenuNetwork.IsEnabled = true;
            buttonMenuSave.IsEnabled = false;
            buttonMenuCancel.IsEnabled = false;
            buttonMenuReset.IsEnabled = true;
            buttonMenuExit.IsEnabled = true;

            buttonMenuPause.IsEnabled = false;
            buttonMenuPause.Header = "_Pause";

            // board and pieces
            cleanBoard();

            // panel
            borderWhitePlayer.BorderBrush = Brushes.Transparent;
            borderBlackPlayer.BorderBrush = Brushes.Transparent;
            buttonAdvise.IsEnabled = false;
            cleanAdvice();
            listBoxMoves.Items.Clear();
            textBlockStatus.Text = "";
        }

        /// <summary>
        /// Exits the game. Ensure worker is finalized and resources are
        /// released, no further drawing actions are necessary.
        /// </summary>
        private void exitGame()
        {
            finalizeAdviceWorker();

            // TODO: Congo.GUI.WainWindow.exitGame() releases resources if game is network.

            Application.Current.Shutdown();
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            boardWrapper = new(panelCongoBoard, tile_Click);
            userPanelWrapper = new(borderWhitePlayer, borderBlackPlayer);
            advisePanelWrapper = new(buttonAdvise, textBlockAdvise);

            wrappers = new() { boardWrapper, userPanelWrapper, advisePanelWrapper, };

            adviceEvent = new ManualResetEventSlim(true);
            pauseEvent = new ManualResetEventSlim(true);

            resetGame();
        }
    }
}
