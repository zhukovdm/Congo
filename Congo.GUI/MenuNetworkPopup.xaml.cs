using Congo.Core;
using Congo.Server;
using Congo.Utils;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Congo.GUI
{
    public partial class MenuNetworkPopup : MenuBasePopup
    {
        #region acceptors

        private StringWriter checkUserInput(StringWriter err)
        {
            var checks = new List<(Func<bool>, string)>()
                {
                    ( () => !UserInput.IsValidPort(textBoxPort.Text)
                    , "Entered port is invalid."
                    ),
                    ( () => !UserInput.IsValidHost(textBoxHost.Text)
                    , "Entered host address is invalid."
                    ),
                    ( () => radioButtonId.IsChecked == true && !UserInput.IsValidGameId(textBoxId.Text)
                    ,"Id is selected, but valid gameId is not provided."
                    ),
                    ( () => radioButtonFen.IsChecked == true && CongoFen.FromFen(textBoxFen.Text) == null
                    , "Fen is selected, but valid CongoFen string is not provided."
                    ),
                };

            foreach (var (pred, msg) in checks) {
                if (pred.Invoke()) { (err ??= new()).WriteLine(msg); }
            }

            return err;
        }

        private StringWriter createUsers(StringWriter err)
        {
            var algo = radioButtonRandom.IsChecked == true
                    ? Algorithm.Random
                    : (AlgorithmDelegate)Algorithm.Negamax;

            WhiteUser = radioButtonWhite.IsChecked == true
                ? (radioButtonHi.IsChecked == true ? new Hi(algo) : new Ai(algo))
                : new Net(Algorithm.Random);

            BlackUser = radioButtonWhite.IsChecked != true
                ? (radioButtonHi.IsChecked == true ? new Hi(algo) : new Ai(algo))
                : new Net(Algorithm.Random);

            return err;
        }

        private StringWriter createPopupPack(StringWriter err)
        {
            PopupPack = new();
            PopupPack.NetPack = new();

            return err;
        }

        private StringWriter createRpcPrimitives(StringWriter err)
        {
            PopupPack.NetPack.Channel = GrpcPrimitives.CreateGrpcChannel(textBoxHost.Text, textBoxPort.Text);
            PopupPack.NetPack.Client = new CongoGrpc.CongoGrpcClient(PopupPack.NetPack.Channel);

            return err;
        }

        private StringWriter determineGameId(StringWriter err)
        {
            try {
                if (radioButtonStandard.IsChecked == true) {
                    PopupPack.NetPack.GameId = GrpcRoutines.PostFen(PopupPack.NetPack.Client, CongoFen.ToFen(CongoGame.Standard()));
                }

                if (radioButtonFen.IsChecked == true) {
                    PopupPack.NetPack.GameId = GrpcRoutines.PostFen(PopupPack.NetPack.Client, textBoxFen.Text);
                }
            }
            catch (CongoServerResponseException ex) {
                (err ??= new()).WriteLine($"{GrpcRoutinesGui.ServerResponseErrorPrefix}{ex.Message}");
            }
            catch (RpcException ex) {
                (err ??= new()).WriteLine($"{GrpcRoutinesGui.GrpcErrorPrefix}{ex.StatusCode}");
            }

            if (radioButtonId.IsChecked == true) {
                PopupPack.NetPack.GameId = long.Parse(textBoxId.Text);
            }

            return err;
        }

        private StringWriter confirmGameId(StringWriter err)
        {
            try {
                GrpcRoutines.ConfirmGameId(PopupPack.NetPack.Client, PopupPack.NetPack.GameId);
            }
            catch (CongoServerResponseException ex) {
                (err ??= new()).WriteLine($"{GrpcRoutinesGui.ServerResponseErrorPrefix}{ex.Message}");
            }
            catch (RpcException ex) {
                (err ??= new()).WriteLine($"{GrpcRoutinesGui.GrpcErrorPrefix}{ex.StatusCode}");
            }

            return err;
        }

        private StringWriter initMoveId(StringWriter err)
        {
            PopupPack.NetPack.MoveId = -1;
            return err;
        }

        private StringWriter getKnownMoves(StringWriter err)
        {
            try {
                PopupPack.Moves = GrpcRoutines.GetDbMovesAfter(PopupPack.NetPack.Client, PopupPack.NetPack.GameId, -1);
            }
            catch (RpcException ex) {
                (err ??= new()).WriteLine($"{GrpcRoutinesGui.GrpcErrorPrefix}{ex.StatusCode}");
            }

            return err;
        }

        private StringWriter getLatestGame(StringWriter err)
        {
            try {
                Game = GrpcRoutines.GetLatestGame(PopupPack.NetPack.Client, PopupPack.NetPack.GameId);
            }
            catch (RpcException ex) {
                (err ??= new()).WriteLine($"{GrpcRoutinesGui.GrpcErrorPrefix}{ex.StatusCode}");
            }

            return err;
        }

        #endregion

        #region event control

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(buttonConfirm);
        }

        private void radioButtonFen_Checked(object sender, RoutedEventArgs e)
        {
            textBoxFen.Visibility = Visibility.Visible;
            textBoxFen.Focus();
        }

        private void radioButtonFen_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxFen.Visibility = Visibility.Hidden;
        }

        private void radioButtonId_Checked(object sender, RoutedEventArgs e)
        {
            textBoxId.Visibility = Visibility.Visible;
            textBoxId.Focus();
        }

        private void radioButtonId_Unchecked(object sender, RoutedEventArgs e)
        {
            textBoxId.Visibility = Visibility.Hidden;
        }

        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            var err = checkUserInput(null);

            if (err is not null) {
                reportError(err.ToString(), "Input Error");
                return;
            }

            err = err
                .AndThen(createUsers)
                .AndThen(createPopupPack)
                .AndThen(createRpcPrimitives)
                .AndThen(determineGameId)
                .AndThen(confirmGameId)
                .AndThen(initMoveId)
                .AndThen(getKnownMoves)
                .AndThen(getLatestGame);

            if (err is not null) {
                reportError(err.ToString(), "Communication Error");
                return;
            }

            DialogResult = true;
            Close();
        }

        #endregion

        public MenuNetworkPopup()
        {
            InitializeComponent();
        }
    }
}
