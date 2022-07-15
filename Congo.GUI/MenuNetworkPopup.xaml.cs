using Congo.Core;
using Congo.Server;
using Congo.Utils;
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
                if (pred.Invoke()) { (err ??= new StringWriter()).WriteLine(msg); }
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
            NetworkPack = new()
            {
                Channel = GrpcPrimitives.CreateRpcChannel(textBoxHost.Text, textBoxPort.Text)
            };
            NetworkPack.Client = new CongoGrpc.CongoGrpcClient(NetworkPack.Channel);

            return err;
        }

        private StringWriter determineGameId(StringWriter err)
        {
            try {
                if (radioButtonStandard.IsChecked == true) {
                    NetworkPack.GameId = GrpcRoutines.PostFen(NetworkPack.Client, CongoFen.ToFen(CongoGame.Standard()));
                }

                if (radioButtonFen.IsChecked == true) {
                    NetworkPack.GameId = GrpcRoutines.PostFen(NetworkPack.Client, textBoxFen.Text);
                }
            }
            catch (Exception) {
                (err ??= new StringWriter())
                    .WriteLine("New board cannot be created.");
            }
            if (radioButtonId.IsChecked == true) {
                NetworkPack.GameId = long.Parse(textBoxId.Text);
            }

            return err;
        }

        private StringWriter checkGameIdExist(StringWriter err)
        {
            try {
                if (!GrpcRoutines.CheckGameId(NetworkPack.Client, NetworkPack.GameId)) {
                    (err ??= new StringWriter())
                        .WriteLine($"gameId {NetworkPack.GameId} does not exist.");
                }
            }
            catch (Exception) {
                (err ??= new StringWriter())
                    .WriteLine($"gameId {NetworkPack.GameId} cannot be checked.");
            }

            return err;
        }

        private StringWriter getKnownMoves(StringWriter err)
        {
            try {
                NetworkPack.Moves = GrpcRoutines.GetMovesAfter(NetworkPack.Client, NetworkPack.GameId, -1);
            }
            catch (Exception) {
                (err ??= new StringWriter())
                    .WriteLine($"Known moves for gameId {NetworkPack.GameId} cannot be obtained.");
            }

            return err;
        }

        private StringWriter getLatestGame(StringWriter err)
        {
            try {
                Game = GrpcRoutines.GetLatestGame(NetworkPack.Client, NetworkPack.GameId);
            }
            catch (Exception) {
                (err ??= new StringWriter())
                    .WriteLine($"Game for gameId {NetworkPack.GameId} cannot be obtained.");
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
                .AndThen(checkGameIdExist)
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
