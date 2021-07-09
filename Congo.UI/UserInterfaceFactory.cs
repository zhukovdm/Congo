using System.Collections.Generic;

using Congo.CLI;
using Congo.Def;

namespace Congo.UI {

	public static class UserInterfaceFactory {

		private delegate IUserInterface D(GameCode gc);

		private static Dictionary<UICode, D> delegateTable =
			new Dictionary<UICode, D>() {
				{
					UICode.CommandLineInterface,
					new D(CommandLineInterface.GetParametrizedInstance)
				}
			};

		public static IUserInterface GetInstance(UICode uiCode, GameCode gameCode) {
			return delegateTable[uiCode].Invoke(gameCode);
		}
	}

}
