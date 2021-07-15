using System.Collections.Generic;

namespace Congo.Def {
    
	public enum SquareCode : int {
		A7, B7, C7, D7, E7, F7, G7,
		A6, B6, C6, D6, E6, F6, G6,
		A5, B5, C5, D5, E5, F5, G5,
		A4, B4, C4, D4, E4, F4, G4,
		A3, B3, C3, D3, E3, F3, G3,
		A2, B2, C2, D2, E2, F2, G2,
		A1, B1, C1, D1, E1, F1, G1
	}

	public static class SquareCodeExtensions {

		private static List<string> squareRepr = new List<string> {
			"a7", "b7", "c7", "d7", "e7", "f7", "g7",
			"a6", "b6", "c6", "d6", "e6", "f6", "g6",
			"a5", "b5", "c5", "d5", "e5", "f5", "g5",
			"a4", "b4", "c4", "d4", "e4", "f4", "g4",
			"a3", "b3", "c3", "d3", "e3", "f3", "g3",
			"a2", "b2", "c2", "d2", "e2", "f2", "g2",
			"a1", "b1", "c1", "d1", "e1", "f1", "g1"
		};

		public static string ToString(this SquareCode square) {
			var s = (int)square;
			return (s >= 0 && s < squareRepr.Count) ? squareRepr[s] : "--";
		}

		public static int IndexOf(string square) => squareRepr.IndexOf(square);

	}

	public enum ColorCode : int {
		White, Black
	}

	public static class ColorCodeExtensions {

		public static bool IsWhite(this ColorCode color) => color == ColorCode.White;
		public static bool IsBlack(this ColorCode color) => color == ColorCode.Black;
		public static ColorCode Opponent(this ColorCode color)
			=> color.IsWhite() ? ColorCode.Black : ColorCode.White;

	}

	public enum PieceCode : uint {
		Empty, Lion, Zebra, Elephant, Giraffe,
		Crocodile, Pawn, Superpawn, Monkey, Captured
	}

	public enum UICode : int {
		CommandLineInterface
	}

	public enum GameCode : int {
		Congo
	}

	public enum UserCommandCode : int {
		Help, Play, Advise, Move
	}

	public static class UserCommandExtensions {

		public static bool IsHelp(this UserCommandCode code) => code == UserCommandCode.Help;
		public static bool IsPlay(this UserCommandCode code) => code == UserCommandCode.Play;
		public static bool IsAdvise(this UserCommandCode code) => code == UserCommandCode.Advise;
		public static bool IsMove(this UserCommandCode code) => code == UserCommandCode.Move;
	
	}

}
