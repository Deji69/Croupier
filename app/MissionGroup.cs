using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Croupier {
	public enum MissionGroup {
		None,
		Prologue,
		Season1,
		Season1Bonus,
		PatientZero,
		Season2,
		SpecialAssignments,
		Season3,
	}

	static partial class MissionGroupMethods {
		public static string GetName(this MissionGroup g) {
			return g switch {
				MissionGroup.None => "",
				MissionGroup.Prologue => "Prologue",
				MissionGroup.Season1 => "Season 1",
				MissionGroup.Season2 => "Season 2",
				MissionGroup.Season3 => "Season 3",
				MissionGroup.Season1Bonus => "Season 1 Bonus",
				MissionGroup.PatientZero => "Patient Zero",
				MissionGroup.SpecialAssignments => "Special Assignments",
				_ => throw new NotImplementedException(),
			};
		}

		public static MissionGroup FromName(string name) {
			return name switch {
				"Prologue" => MissionGroup.Prologue,
				"Season 1" => MissionGroup.Season1,
				"Season 2" => MissionGroup.Season2,
				"Season 3" => MissionGroup.Season3,
				"Season 1 Bonus" => MissionGroup.Season1Bonus,
				"Patient Zero" => MissionGroup.PatientZero,
				"Special Assignments" => MissionGroup.SpecialAssignments,
				_ => throw new NotImplementedException(),
			};
		}
	}
}
