using System.Collections.Immutable;

namespace Congo.Def {

	public interface IUserInterface {
		void Greet();
		void ShowBoard(IBoard board);
	}

	public interface IGame {
		void Show();
		bool InProgress();
		IGame MakeProgress();
		void ReportResult();
		bool Repeat();
	}

	public interface IBoard {
		int Size { get; }
		PieceCode GetPieceCode(int rank, int file);
		bool IsFirstMovePiece(int rank, int file);
	}

	public interface ICommandLineConfiguration {
		string GreetView { get; }
		ImmutableDictionary<PieceCode, string> PieceView { get; }
	}

}
