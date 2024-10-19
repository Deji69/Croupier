using System.Collections.Specialized;

namespace Croupier {
	public class MissionKillMethod(Mission mission, KillMethod method, StringCollection tags) : KillMethod(method.Name, method.Image, method.Category, method.Target, [..method.Tags, ..tags], method.Keywords) {
		public readonly Mission Mission = mission;
	}
}
