using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace Croupier
{
	public class Spin(List<SpinCondition> conditions) {
		public readonly List<SpinCondition> Conditions = conditions;

		public Spin() : this([]) { }

		public MissionID Mission {
			get {
				if (Conditions.Count == 0) return MissionID.NONE;
				return Conditions.First().Target.Mission?.ID ?? MissionID.NONE;
			}
		}

		public bool HasDisguise(Disguise disguise) {
			if (disguise == null) return false;
			return Conditions.Exists(cond => cond.Disguise.Name == disguise.Name);
		}

		public bool HasMethod(KillMethod method) {
			return Conditions.Exists(cond => cond.Kill.IsSameMethod(method));
		}

		public int LargeFirearmCount { 
			get => Conditions.Count(c => c.Kill.IsLargeFirearm);
		}

		public int LoudWeaponCount {
			get => Conditions.Count(c => c.Kill.IsLoudWeapon);
		}

		public bool IsLegal() {
			var ruleset = Ruleset.Current ?? throw new Exception("No ruleset.");
			if (LargeFirearmCount > ruleset.Rules.MaxLargeFirearms)
				return false;
			var mission = Croupier.Mission.Get(Mission);
			foreach (var cond in Conditions) {
				if (cond.Target.Mission?.ID != mission.ID)
					return false;
				if (!cond.IsLegal())
					return false;
			}
			return true;
		}

		public override string ToString() {
			var str = "";

			foreach (var cond in Conditions) {
				if (str.Length > 0) str += ", ";
				str += cond.ToString();
			}

			return str;
		}
	}
}
