using System.Collections.Generic;

namespace Congo.Core {

	abstract class Player {
	}

	sealed class AIPlayer : Player {
		public AIPlayer() { }
	}

	sealed class HIPlayer : Player {
		public HIPlayer() { }
	}

}
