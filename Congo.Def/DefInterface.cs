using System.Collections.Immutable;

namespace Congo.Def {

	public interface IUserCommand { }

	public interface IUserInterface {
		void Greet();
		void ShowBoard(IBoard board);
		IUserCommand GetUserCommand();
		void Report(string message);
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

	public interface IParametrizedEnumerator<U> {
		U Current { get; }
		bool MoveNext();
	}

	public interface IParametrizedEnumerable<T, U> {
		IParametrizedEnumerator<U> GetEnumerator(T param);
	}

	public interface ICommandLineConfiguration {
		
		string GreetView { get; }
		ImmutableDictionary<PieceCode, string> PieceView { get; }
		IUserCommand TryPlayCommand(string[] input, IUserInterface ui);
		IUserCommand TryHelpCommand(string[] input, IUserInterface ui);
		IUserCommand TryAdviseCommand(string[] input, IUserInterface ui);
		IUserCommand TryMoveCommand(string[] input, IUserInterface ui);

	}

}
