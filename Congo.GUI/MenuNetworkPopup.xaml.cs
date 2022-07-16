using Congo.Core;
using Congo.Server;
using Congo.Utils;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

using CongoClient = Congo.Server.CongoGrpc.CongoGrpcClient;

namespace Congo.GUI
{
    public partial class MenuNetworkPopup : MenuBasePopup
    {
        private GrpcChannel channel;
        private CongoClient client;
        private long gameId;
        private long moveId;
        private ICollection<DbMove> dbMoves;

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

        private StringWriter createRpcPrimitives(StringWriter err)
        {
            channel = GrpcPrimitives.CreateGrpcChannel(textBoxHost.Text, textBoxPort.Text);
            client = new(PopupPack.NetPack.Channel);

            return err;
        }

        private StringWriter determineGameId(StringWriter err)
        {
            try {
                if (radioButtonStandard.IsChecked == true) {
                    gameId = GrpcRoutines.PostFen(PopupPack.NetPack.Client, CongoFen.ToFen(CongoGame.Standard()));
                }

                if (radioButtonFen.IsChecked == true) {
                    gameId = GrpcRoutines.PostFen(PopupPack.NetPack.Client, textBoxFen.Text);
                }
            }
            catch (CongoServerResponseException ex) {
                (err ??= new()).WriteLine($"{GrpcRoutinesGui.ServerResponseErrorPrefix}{ex.Message}");
            }
            catch (RpcException ex) {
                (err ??= new()).WriteLine($"{GrpcRoutinesGui.GrpcErrorPrefix}{ex.StatusCode}");
            }

            if (radioButtonId.IsChecked == true) {
                gameId = long.Parse(textBoxId.Text);
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
            moveId = -1;
            return err;
        }

        private StringWriter getKnownMoves(StringWriter err)
        {
            try {
                dbMoves = GrpcRoutines.GetDbMovesAfter(PopupPack.NetPack.Client, PopupPack.NetPack.GameId, -1);
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

            PopupPack = new(new(gameId, moveId, channel, client), dbMoves);

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
