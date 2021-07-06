using Congo.Core;

namespace Congo.CLI {

	class Game {

		private UserInterface userInterface;
		private Board board;

		public Game(UserInterface userInterface, Board board) {
			this.userInterface = userInterface;
			this.board = board;
		}

		public void Play() {
			userInterface.Greet();
			userInterface.Show(board);
			/*/
			while (!board.IsFinal()) {
				board = board.ActivePlayer.MakeMove(userInterface);
				userInterface.Show(board);
			}
			userInterface.ReportResult(board);
			/**/
		}

	}

}
