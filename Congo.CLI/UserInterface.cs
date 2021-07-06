using System;
using System.Collections.Generic;
using System.IO;

using Congo.Core;

namespace Congo.CLI {
	
	abstract class UserInterface {
		public abstract void Greet();
		public abstract void Show(Board board);
	}

	sealed class CommandLineInterface : UserInterface {

		private static Dictionary<Type, string> pieceView;
		private static TextReader reader = Console.In;
		private static TextWriter writer = Console.Out;

		static CommandLineInterface() {
			pieceView = new Dictionary<Type, string>() {
				{ typeof(Lion),      " l" }, { typeof(Zebra),   " z" },
				{ typeof(Elephant),  " e" }, { typeof(Giraffe), " g" },
				{ typeof(Crocodile), " c" }, { typeof(Pawn),    " p" },
				{ typeof(Superpawn), " s" }, { typeof(Monkey),  " m" },
				{ typeof(Empty),     " -" }
			};
		}

		public override void Greet() {
			Console.WriteLine(
				File.ReadAllText("Resources\\text\\greet.txt")
			);
		}

		public override void Show(Board board) {
			for (int rank = 0; rank < 7; rank++) {
				writer.Write($" {7 - rank} ");
				for (int file = 0; file < 7; file++) {
					var position = rank * 7 + file;
					var pv = pieceView[board.GetPieceType(position)];
					if (board.IsPieceWhite(position)) pv = pv.ToUpper();
					writer.Write(pv);
				}
				writer.WriteLine();
			}
			writer.WriteLine();
			writer.WriteLine(" /  A B C D E F G");
		}

	}

}
