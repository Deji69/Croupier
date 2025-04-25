using System.Collections.Generic;

namespace Croupier {
	public class RulesetRules {
		public bool GenericEliminations { get; set; } = false;
		public bool MeleeKillTypes { get; set; } = false;
		public bool ThrownKillTypes { get; set; } = false;
		public bool AnyExplosives { get; set; } = true;
		public bool ImpactExplosives { get; set; } = true;
		public bool RemoteExplosives { get; set; } = true;
		public bool LoudRemoteExplosives { get; set; } = false;
		public bool LiveComplications { get; set; } = true;
		public bool LiveComplicationsExcludeStandard { get; set; } = true;
		public int LiveComplicationChance { get; set; } = 25;
		public int KillTypeChance { get; set; } = 50;
		public bool LoudSMGIsLargeFirearm { get; set; } = true;
		public int MaxLargeFirearms { get; set; } = 1;
		public bool AllowDuplicateDisguise { get; set; } = false;
		public bool AllowDuplicateMethod { get; set; } = false;
		public bool SuitOnly { get; set; } = false;
		public bool AnyDisguise { get; set; } = false;
		public List<string> Banned { get; set; } = ["Slow", "Hard", "Extreme", "Impossible", "Buggy"];
	}
}
