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
        public static MainState GameToState(CongoGame game, CongoUser activeUser, CongoMove move)
        {
            MainState state;

            if (game.HasEnded()) { state = MainState.END; }

            else if (activeUser is Ai) { state = MainState.AI; }

            else if (activeUser is Hi) { state = (move is MonkeyJump) ? MainState.TO : MainState.FR; }

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

        public Step(MainState state, CongoGame game, CongoMove move, int moveFr)
        {
            State = state; Game = game; Move = move; MoveFr = moveFr;
        }
    }

    public sealed partial class MainWindow : Window
    {
        private const string whiteLiteral = "white";
        private const string blackLiteral = "black";

        // state
        private int _moveFr;
        private CongoGame _game;
        private MainState _state;
        private CongoUser _whiteUser, _blackUser;

        // network
        private NetPack _netPack = null;
        private ImmutableList<CongoMove> _movesOut = new List<CongoMove>().ToImmutableList();

        // async
        private Step _step = null;
        private AsyncJob _job = null;

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
            _job = null;

            if (_step is null) {
                var advise = (AsyncAdvise)e.Result;

                var game = advise.NewGame; var move = advise.AdvisedMove; int moveFr = -1;
                var state = MainStatePrimitives.GameToState(game, game.GetActiveUser(_whiteUser, _blackUser), move);

                applyStep(state, game, move, moveFr);
            }

            else {
                applyStep(_step.State, _step.Game, _step.Move, _step.MoveFr);
            }

            _step = null;
        }

        private void aiAdvise_Init(CongoGame game, CongoUser activeUser, bool sleep)
        {
            if (_job is null) {

                _job = new AsyncAdvise(game, activeUser, sleep);
                AsyncPrimitives.GetWorker(worker_DoWork, aiAdvise_Complete)
                               .RunWorkerAsync(argument: _job);
            }
        }

        private void hiAdvise_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            _job = null;

            if (_step is null) {
                var advise = (AsyncAdvise)e.Result;
                hiAdviseWrapper.End(advise.AdvisedMove);
            }

            else {
                applyStep(_step.State, _step.Game, _step.Move, _step.MoveFr);
            }

            _step = null;
        }

        private void hiAdvise_Init(CongoGame game, CongoUser activeUser)
        {
            if (_job is null) {

                _job = new AsyncAdvise(game, activeUser, false);
                hiAdviseWrapper.Begin();
                AsyncPrimitives.GetWorker(worker_DoWork, hiAdvise_Complete)
                               .RunWorkerAsync(argument: _job);
            }
        }

        private void netMove_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            _job = null;

            if (_step is null) {
                var netMove = (AsyncNetMove)e.Result;

                switch(netMove.Status) {

                    case NetStatus.Ok:
                        _netPack = netMove.NewNetPack;
                        var game = netMove.NewGame; CongoMove move = null; int moveFr = -1;
                        var state = MainStatePrimitives.GameToState(game, game.GetActiveUser(_whiteUser, _blackUser), move);

                        statusPanelWrapper.AppendMoves(netMove.DbMoves);

                        applyStep(state, game, move, moveFr);
                        break;

                    default:
                        statusPanelWrapper.SetErrorMessage(netMove.ErrorMessage);
                        applyStep(MainState.ERR, null, null, -1);
                        break;
                }
            }

            else {
                applyStep(_step.State, _step.Game, _step.Move, _step.MoveFr);
            }

            _step = null;
        }

        private void netMove_Init(NetPack netPack, CongoUser white, CongoUser black, ImmutableList<CongoMove> movesOut)
        {
            if (_job is null) {

                _job = new AsyncNetMove(netPack, white, black, movesOut);
                AsyncPrimitives.GetWorker(worker_DoWork, netMove_Complete)
                               .RunWorkerAsync(argument: _job);
            }
        }

        #endregion

        #region window control

        private void applyStep(MainState state, CongoGame game, CongoMove move, int moveFr)
        {
            if (_job is not null && _step is null) {
                _job.Abandon();
                _step = new(state, game, move, moveFr);
            }

            else if (_job is null) {

                var oldState = _state;
                _state = state; _game = game; _moveFr = moveFr;

                if (move is not null) {
                    _movesOut = _movesOut.Add(move);
                    statusPanelWrapper.AppendMove(move);
                }

                switch (state) {

                    case MainState.INIT:
                        _netPack = null;
                        _movesOut = _movesOut.Clear();
                        foreach (var wrapper in wrappers) { wrapper.Init(); }
                        GC.Collect();
                        break;

                    case MainState.FR:
                        userPanelWrapper.Draw(game);
                        boardWrapper.Draw(game, state, moveFr);
                        advisePanelWrapper.Activate(oldState, state);
                        break;

                    case MainState.TO:
                        boardWrapper.Draw(game, state, moveFr);
                        advisePanelWrapper.Activate(oldState, state);
                        break;

                    case MainState.AI:
                        userPanelWrapper.Draw(game);
                        boardWrapper.Draw(game, state, moveFr);
                        advisePanelWrapper.Deactivate();
                        aiAdvise_Init(game, game.GetActiveUser(_whiteUser, _blackUser), _whiteUser is Ai && _blackUser is Ai);
                        break;

                    case MainState.NET:
                        userPanelWrapper.Draw(game);
                        boardWrapper.Draw(game, state, moveFr);
                        advisePanelWrapper.Deactivate();
                        netMove_Init(_netPack, _whiteUser, _blackUser, _movesOut);
                        _movesOut = _movesOut.Clear();
                        break;

                    case MainState.END:

                        // special case, local winning step with Net user
                        if (_movesOut.Count > 0 && (_whiteUser is Net || _blackUser is Net)) {
                            netMove_Init(_netPack, _whiteUser, _blackUser, _movesOut);
                            _movesOut = _movesOut.Clear();
                            _step = new Step(state, game, null, -1);
                        }

                        else {
                            gameMenuWrapper.Init();
                            controlMenuWrapper.Init();
                            userPanelWrapper.Init();
                            advisePanelWrapper.Init();
                            boardWrapper.Draw(game, state, moveFr);

                            var message = game.WhitePlayer.HasLion
                                ? whiteLiteral
                                : blackLiteral;
                            statusPanelWrapper.SetStatus(message + " wins");
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
            var s = MainState.TO; var g = _game; CongoMove m = null;

            if (g.Board.IsFriendlyPiece(g.ActivePlayer.Color, fr)) {
                applyStep(s, g, m, fr);
            }
        }

        private void stepTileTo(int to)
        {
            MainState state; var game = _game; var moveFr = _moveFr;
            var move = game.ActivePlayer.Accept(new CongoMove(moveFr, to));

            // also covers monkey jump termination, when fr == to is a move
            if (move is not null) {
                game = game.Transition(move);
                state = MainStatePrimitives.GameToState(game, game.GetActiveUser(_whiteUser, _blackUser), move);
                if (move is MonkeyJump) { moveFr = to; }
            }

            else if (moveFr == to) { state = MainState.FR; } // selection reset

            else { /* do nothing */ return; }

            applyStep(state, game, move, moveFr);
        }

        #endregion

        #region event control

        private void spawnNewGameDialog<T>(object sender, EventArgs e)
            where T : Window, IPlayablePopup, new()
        {
            var dialog = new T();
            if (dialog.ShowDialog() == true) {

                resetGame();

                var game = dialog.Game; CongoMove move = null; int moveFr = -1;
                var state = MainStatePrimitives.GameToState(game, game.GetActiveUser(dialog.WhiteUser, dialog.BlackUser), move);

                _whiteUser = dialog.WhiteUser;
                _blackUser = dialog.BlackUser;

                gameMenuWrapper.Disable();
                menuItemCancel.IsEnabled = true;
                menuItemPause.IsEnabled = (dialog.WhiteUser is Ai) && (dialog.BlackUser is Ai);

                if (dialog.PopupPack is not null) {
                    _netPack = dialog.PopupPack.NetPack;
                    statusPanelWrapper.SetId(dialog.PopupPack.NetPack.GameId);
                    statusPanelWrapper.AppendMoves(dialog.PopupPack.DbMoves);
                }

                applyStep(state, game, move, moveFr);
            }
        }

        private void menuItemLocal_Click(object sender, RoutedEventArgs e)
            => spawnNewGameDialog<MenuLocalPopup>(sender, e);

        private void menuItemNetwork_Click(object sender, RoutedEventArgs e)
            => spawnNewGameDialog<MenuNetworkPopup>(sender, e);

        private void menuItemSave_Click(object sender, RoutedEventArgs e)
        {
            var r = (_game is not null) ? CongoFen.ToFen(_game) : string.Empty;
            Clipboard.SetText(r);
        }

        private void menuItemPause_Click(object sender, RoutedEventArgs e)
            => AsyncJob.Invert(menuItemPause);

        private void menuItemCancel_Click(object sender, RoutedEventArgs e)
            => _job?.Cancel();

        private void menuItemReset_Click(object sender, RoutedEventArgs e)
            => resetGame();

        private void menuItemExit_Click(object sender, RoutedEventArgs e)
            => exitGame();

        private void Window_Closing(object sender, CancelEventArgs e)
            => exitGame();

        private void tile_Click(object sender, RoutedEventArgs e)
        {
            switch (_state) {

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
            => hiAdvise_Init(_game, _game.GetActiveUser(_whiteUser, _blackUser));

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
