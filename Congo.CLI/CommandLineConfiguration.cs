using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

using Congo.Def;

namespace Congo.CLI {

	class CongoCommandLineConfiguration : ICommandLineConfiguration {

		private static readonly string greetView =
			File.ReadAllText("Resources\\CLI\\Congo\\greet.txt");

		private static readonly ImmutableDictionary<PieceCode, string> pieceView =
			new Dictionary<PieceCode, string>() {
				{ PieceCode.Lion, " l" }, { PieceCode.Zebra, " z" },
				{ PieceCode.Elephant, " e" }, { PieceCode.Giraffe, " g" },
				{ PieceCode.Crocodile, " c" }, { PieceCode.Pawn, " p" },
				{ PieceCode.Superpawn, " s" }, { PieceCode.Monkey, " m" },
				{ PieceCode.Empty, " -" }, { PieceCode.Captured, " x" }
			}.ToImmutableDictionary();

		private static readonly CongoCommandLineConfiguration clv =
			new CongoCommandLineConfiguration();
		
		public static ICommandLineConfiguration GetInstance() => clv;

		private CongoCommandLineConfiguration() { }

		public string GreetView { get => greetView; }
		public ImmutableDictionary<PieceCode, string> PieceView { get => pieceView; }

	}

}
