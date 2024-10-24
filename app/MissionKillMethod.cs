using System.Collections.Specialized;

namespace Croupier {
	public class MissionKillMethod(Mission mission, KillMethod method, StringCollection tags) : KillMethod(method, tags) {
		public readonly Mission Mission = mission;
	}
}
