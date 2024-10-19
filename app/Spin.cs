using System;
using System.Collections.Generic;
using System.Linq;

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
