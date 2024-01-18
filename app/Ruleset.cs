using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Croupier.UI
{
	public enum RulesetPreset {
		Custom,
		RRWC2023,
		RR12,
		RR11,
		Croupier,
	}

	public class Ruleset
	{
		public string Name { get; set; }
		public RulesetPreset Preset { get; set; } = RulesetPreset.Custom;

		public Ruleset(string name, RulesetPreset preset = RulesetPreset.Custom) {
			Name = name;
			Preset = preset;
			ApplyPresetDefaults();
		}

		public void ApplyPresetDefaults() {
			switch (Preset) {
				case RulesetPreset.RRWC2023:
					genericEliminations = false;
					meleeKillTypes = false;
					thrownKillTypes = false;
					liveComplications = true;
					liveComplicationsExcludeStandard = true;
					liveComplicationChance = 25;
					break;
				case RulesetPreset.RR12:
					genericEliminations = false;
					meleeKillTypes = false;
					thrownKillTypes = false;
					liveComplications = true;
					liveComplicationsExcludeStandard = true;
					liveComplicationChance = 20;
					break;
				case RulesetPreset.RR11:
					genericEliminations = true;
					meleeKillTypes = true;
					thrownKillTypes = true;
					break;
				case RulesetPreset.Croupier:
					genericEliminations = false;
					liveComplications = true;
					liveComplicationChance = 25;
					enableMedium = true;
					break;
			}
		}

		public bool genericEliminations = false;
		public bool meleeKillTypes = false;
		public bool thrownKillTypes = false;
		public bool liveComplications = false;
		public bool liveComplicationsExcludeStandard = false;
		public bool enableMedium = false;
		public bool enableHard = false;
		public bool enableExtreme = false;
		public bool enableBuggy = false;
		public bool enableImpossible = false;
		public bool allowDuplicateDisguise = false;
		public bool allowDuplicateMethod = false;
		public bool suitOnlyMode = false;
		public int liveComplicationChance = 30;
	}
}
