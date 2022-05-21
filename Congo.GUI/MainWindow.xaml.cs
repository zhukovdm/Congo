using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using Congo.Core;

namespace Congo.GUI
{
    public static class TileExtensions
    {
        private static readonly double tileSize = Congo.GUI.MainWindow.tileSize;
        private static readonly double accentBoardThickness = 5.0;
        private static readonly double standardBoardThickness = 1.0;

        private static readonly ImmutableDictionary<Type, string> type2suffix = new Dictionary<Type, string>
        {
            { typeof(Superpawn), "super-pawn" }, { typeof(Monkey), "monkey" },
            { typeof(Crocodile), "crocodile"  }, { typeof(Zebra),  "zebra"  },
            { typeof(Elephant),  "elephant"   }, { typeof(Lion),   "lion"   },
            { typeof(Giraffe),   "giraffe"    }, { typeof(Pawn),   "pawn"   }
        }.ToImmutableDictionary();

        private static Canvas WithBoard(this Canvas tile, Brush brush, double thickness)
        {
            var border = new Border
            {
                Width = tileSize,
                Height = tileSize,
                BorderBrush = brush,
                BorderThickness = new Thickness(thickness)
            };
            _ = tile.Children.Add(border);

            return tile;
        }

        public static Canvas WithMoveFrBorder(this Canvas tile)
            => tile.WithBoard(Brushes.White, accentBoardThickness);

        public static Canvas WithMoveToBorder(this Canvas tile)
            => tile.WithBoard(Brushes.Red, accentBoardThickness);

        public static Canvas WithStandardBorder(this Canvas tile)
            => tile.WithBoard(Brushes.Black, standardBoardThickness);

        public static Canvas WithPiece(this Canvas tile, CongoColor color, Type type)
        {
            var pfx = color.IsWhite() ? "white" : "black";
            var sfx = type2suffix[type];
            var ext = ".png";

            var image = new Image
            {
                Width = tileSize,
                Height = tileSize,
                Source = new BitmapImage(new Uri("/Congo.GUI;component/Resources/" + pfx + "-" + sfx + ext, UriKind.Relative))
            };
            _ = tile.Children.Add(image);

            return tile;
        }
    }

    public static class ButtonExtensions
    {
        public static void PerformClick(this Button button)
            => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly double tileSize = 82.0;
        private static readonly int boardSize = CongoBoard.Empty.Size * CongoBoard.Empty.Size;

        private static readonly string whiteColorCode = "#914800";
        private static readonly string blackColorCode = "#000000";
        private static readonly string riverColorCode = "#65b9f8";
        private static readonly string groundColorCode = "#67de79";
        private static readonly string castleColorCode = "#f2d377";

        private static readonly ImmutableList<string> moveViews = new List<string>
        {
            "a7", "b7", "c7", "d7", "e7", "f7", "g7",
            "a6", "b6", "c6", "d6", "e6", "f6", "g6",
            "a5", "b5", "c5", "d5", "e5", "f5", "g5",
            "a4", "b4", "c4", "d4", "e4", "f4", "g4",
            "a3", "b3", "c3", "d3", "e3", "f3", "g3",
            "a2", "b2", "c2", "d2", "e2", "f2", "g2",
            "a1", "b1", "c1", "d1", "e1", "f1", "g1"
        }.ToImmutableList();

        private enum State : int { INIT, AI, FR, TO, END }

        private int moveFr;
        private int moveTo;
        private State state;
        private CongoGame game;
        private CongoUser white;
        private CongoUser black;

        private volatile bool advice = false;
        private readonly object adviceLock = new object();
        private BackgroundWorker adviceWorker;
        private readonly ManualResetEventSlim adviceEvent;
        private readonly ManualResetEventSlim pauseEvent;

        private IEnumerable<CongoMove> getMovesFr(int fr)
        {
            return from move in game.ActivePlayer.Moves
                   where move.Fr == fr
                   select move;
        }

        private void replaceTile(int idx, Canvas tile)
        {
            panelCongoBoard.Children.RemoveAt(idx);
            panelCongoBoard.Children.Insert(idx, tile);
        }

        /// <summary>
        /// Constructs empty tile with background and <b>event handler</b>.
        /// @note Event handler is a reason the method cannot be static.
        /// </summary>
        private Canvas getEmptyTile(int idx)
        {
            string code;

            if (idx >= 21 && idx < 28) {
                code = riverColorCode;
            }

            else {
                switch (idx % 7) {
                    case 0:
                    case 1:
                    case 5:
                    case 6:
                        code = groundColorCode;
                        break;
                    default:
                        code = castleColorCode;
                        break;
                }
            }

            var canvas = new Canvas
            {
                Width = tileSize,
                Height = tileSize,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString(code)
            };
            canvas.Tag = idx.ToString();
            canvas.MouseUp += tile_Click;

            return canvas;
        }

        private string getMoveView(CongoMove move)
        {
            if (move == null) { return null; }

            else if (move is MonkeyJump jump) {
                return "(" + moveViews[jump.Fr] + ", " + moveViews[jump.Bt] + ", " + moveViews[jump.To] + ")";
            }

            else { return "(" + moveViews[move.Fr] + ", " + moveViews[move.To] + ")"; }
        }

        private CongoUser getActiveUser()
            => game.ActivePlayer.Color.IsWhite() ? white : black;

        private void appendMove(CongoMove move)
        {
            listBoxMoves.Items.Add(getMoveView(move));
            listBoxMoves.ScrollIntoView(listBoxMoves.Items[listBoxMoves.Items.Count - 1]);
        }

        private void adviceWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            pauseEvent.Wait();

            /* Avoid too fast switching between players. Safe to access 
             * variables, because only reset could remove users, but is
             * waiting for adviceEvent. */
            if (white is Ai && black is Ai) { Thread.Sleep(1000); }

            var g = (CongoGame)e.Argument;
            var user = g.ActivePlayer.Color.IsWhite() ? white : black;
            e.Result = user.Advice(g);

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

            lock(adviceLock) { advice = true; Debug.WriteLine("+ai"); Debug.Flush(); }

            adviceWorker.RunWorkerAsync(argument: game);
        }

        private void aiAdvice_Finalize(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!Algorithm.IsBlocked()) {
                var move = (CongoMove)e.Result;
                if (move == null) { move = Algorithm.Rnd(game); }

                game = game.Transition(move);

                if (getActiveUser() is Hi) { buttonAdvice.IsEnabled = true; }

                buttonMenuCancel.IsEnabled = false;
                appendMove(move);
                updateGame();
            }

            lock (adviceLock) { advice = false; Debug.WriteLine("-ai"); Debug.Flush(); }

            Algorithm.Unblock();
        }

        private void hiAdvice_Finalize(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!Algorithm.IsBlocked()) {
                var move = (CongoMove)e.Result;
                if (move == null) { move = Algorithm.Rnd(game); }

                buttonAdvice.IsEnabled = true;
                buttonMenuCancel.IsEnabled = false;
                textBlockAdvice.Text = getMoveView(move);
            }

            lock (adviceLock) { advice = false; Debug.WriteLine("-hi"); Debug.Flush(); }

            Algorithm.Unblock();
        }

        /// <summary>
        /// Event handler for clicks on all board tiles.
        /// </summary>
        private void tile_Click(object sender, RoutedEventArgs e)
        {
            switch (state) {

                case State.INIT:
                case State.AI:
                    break;

                case State.FR:

                    if (sender is Canvas tileFrom) {
                        moveFr = int.Parse((string)tileFrom.Tag);
                        updateGame();
                    }
                    break;

                case State.TO:

                    if (sender is Canvas tileTo) {
                        moveTo = int.Parse((string)tileTo.Tag);
                        updateGame();
                    }
                    break;

                case State.END:
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

                game = dialog.Game; white = dialog.White; black = dialog.Black;

                buttonMenuLocal.IsEnabled = false;
                buttonMenuNetwork.IsEnabled = false;
                buttonMenuSave.IsEnabled = true;
                buttonMenuPause.IsEnabled = (white is Ai) && (black is Ai);

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
            if (g != null) { Clipboard.SetText(CongoFen.ToFen(g)); }
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

        private void buttonAdvice_Click(object sender, RoutedEventArgs e)
        {
            adviceEvent.Wait();
            adviceEvent.Reset(); // ensure mutual exclusion

            buttonAdvice.IsEnabled = false;
            buttonMenuCancel.IsEnabled = true;

            adviceWorker = getAdviceWorker();
            adviceWorker.RunWorkerCompleted += hiAdvice_Finalize;

            lock (adviceLock) { advice = true; Debug.WriteLine("+hi"); Debug.Flush(); }

            adviceWorker.RunWorkerAsync(argument: game);
        }

        private void buttonMoveGenerator_Click(object sender, RoutedEventArgs e)
            => finalizeState();

        #endregion

        #region Draw Game

        private void cleanAdvice()
            => textBlockAdvice.Text = "";

        private void cleanBoard()
        {
            panelCongoBoard.Children.RemoveRange(0, panelCongoBoard.Children.Count);

            for (int i = 0; i < boardSize; ++i) {
                panelCongoBoard.Children.Add(getEmptyTile(i).WithStandardBorder());
            }
        }

        private void drawPieces(CongoColor color)
        {
            var e = game.Board.GetEnumerator(color);

            while (e.MoveNext()) {
                var tile = getEmptyTile(e.Current)
                    .WithPiece(color, game.Board.GetPiece(e.Current).GetType())
                    .WithStandardBorder();
                replaceTile(e.Current, tile);
            }
        }

        /// <summary>
        /// Draw selections on the tiles based on game state.
        /// @note Selections are possible only if <b>state == State.TO</b>!
        /// </summary>
        private void drawSelect()
        {
            if (state == State.TO) {
                foreach (var move in getMovesFr(moveFr)) {

                    // excludes monkey interrupt vs. ordinary selection
                    if (move.Fr != move.To) {
                        panelCongoBoard.Children[move.To] = ((Canvas)panelCongoBoard.Children[move.To]).WithMoveToBorder();
                    }
                }

                var tile = (Canvas)panelCongoBoard.Children[moveFr];

                panelCongoBoard.Children[moveFr] = (game.FirstMonkeyJump == null)
                    ? tile.WithMoveFrBorder()
                    : tile.WithMoveToBorder();
            }
        }

        private void drawBoard()
        {
            cleanBoard();
            drawPieces(White.Color);
            drawPieces(Black.Color);
            drawSelect();
        }

        private void drawPanel()
        {
            var w = game.ActivePlayer.Color.IsWhite();

            borderWhitePlayer.BorderBrush = w ? Brushes.Red : Brushes.Transparent;
            borderBlackPlayer.BorderBrush = w ? Brushes.Transparent : Brushes.Red;

            cleanAdvice();
        }

        #endregion

        #region Update Game state

        private void initState()
        {
            if (game.HasEnded()) { state = State.END; }

            else if (getActiveUser() is Ai) { state = State.AI; }

            else if (game.FirstMonkeyJump != null) { state = State.TO; buttonAdvice.IsEnabled = true; }

            else { state = State.FR; buttonAdvice.IsEnabled = true; }
        }

        private void updateState()
        {
            // determine new state

            if (game.HasEnded()) { state = State.END; }

            else if (getActiveUser() is Ai) { state = State.AI; }

            else if (state == State.FR && game.Board.IsOccupied(moveFr)
                && game.Board.IsFriendlyPiece(game.ActivePlayer.Color, moveFr)) {
                state = State.TO;
            }

            // end of game | monkey jump | move ~> opponent
            else if (state == State.TO) {

                var move = game.ActivePlayer.Accept(new CongoMove(moveFr, moveTo));

                /* Move exists, also includes monkey jump interrupt (Fr == To)! */
                if (move != null) {
                    game = game.Transition(move);

                    if (game.HasEnded()) { state = State.END; }

                    else if (move is MonkeyJump) { moveFr = moveTo; /* State.TO remains */ }

                    // opponent will turn
                    else {
                        state = (getActiveUser() is Ai) ? State.AI : State.FR;
                    }

                    cleanAdvice();
                    appendMove(move);
                    Algorithm.Cancel(); // maybe running advice
                }

                // not a move, but reset
                else if (moveFr == moveTo) { state = State.FR; }

                else { /* do nothing */ }
            }

            else { state = State.FR; }
        }

        private void finalizeState()
        {
            // maybe act on new state

            switch (state) {

                case State.AI:
                    aiAdvice_Init();
                    break;

                case State.FR:
                case State.TO:
                    break;

                case State.END:
                    borderWhitePlayer.BorderBrush = Brushes.Transparent;
                    borderBlackPlayer.BorderBrush = Brushes.Transparent;

                    var c = game.Opponent.Color.IsWhite() ? "White" : "Black";
                    MessageBox.Show($"{c} wins.");
                    buttonMenuLocal.IsEnabled = true;
                    buttonMenuNetwork.IsEnabled = true;
                    break;

                case State.INIT:
                default:
                    break;
            }
        }

        #endregion

        private void initGame()
        {
            initState();
            drawBoard();
            drawPanel();

            buttonMoveGenerator.PerformClick();
        }

        private void updateGame()
        {
            updateState();
            drawBoard();
            drawPanel();

            buttonMoveGenerator.PerformClick();
        }

        #region Reset and Exit Game

        private void finalizeAdviceWorker()
        {
            lock (adviceLock) {
                if (advice) { Algorithm.Block(); }
            }

            pauseEvent.Set(); // maybe worker on a break
            adviceEvent.Wait();
        }

        /// <summary>
        /// Upon call, the window reaches predictable state. Remove all code
        /// entities, clean up the board, enable or disable buttons.
        /// </summary>
        private void resetGame()
        {
            /* code entities */

            finalizeAdviceWorker();

            state = State.INIT;
            game = null;
            white = null;
            black = null;

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
            buttonAdvice.IsEnabled = false;
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

            adviceEvent = new ManualResetEventSlim(true);
            pauseEvent = new ManualResetEventSlim(true);

            panelWhitePlayer.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(whiteColorCode);
            panelBlackPlayer.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(blackColorCode);

            resetGame();
        }
    }
}
