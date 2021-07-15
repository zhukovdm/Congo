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
		PieceCode GetPieceCode(int square);
		bool IsFirstMovePiece(int square);
	}

	public interface IParametrizedEnumerator<T> {
		T Current { get; }
		bool MoveNext();
	}

	public interface IParametrizedEnumerable<T, U> {
		IParametrizedEnumerator<U> GetEnumerator(T param);
	}

	public interface ICommandLineConfiguration {
		string GreetView { get; }
		ImmutableDictionary<PieceCode, string> PieceView { get; }
	}

}
