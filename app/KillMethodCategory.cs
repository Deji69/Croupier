using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Croupier {
	public enum KillMethodCategory {
		Standard,
		Weapon,
		Melee,
		Unique,
	}

	static partial class KillMethodCategoryMethods {
		public static string ToString(this KillMethodCategory c) {
			return c switch {
				KillMethodCategory.Standard => "Standard",
				KillMethodCategory.Weapon => "Weapon",
				KillMethodCategory.Melee => "Melee",
				KillMethodCategory.Unique => "Unique",
				_ => throw new NotImplementedException()
			};
		}

		public static KillMethodCategory FromString(string str) {
			return str.ToLower() switch {
				"standard" => KillMethodCategory.Standard,
				"weapon" => KillMethodCategory.Weapon,
				"melee" => KillMethodCategory.Melee,
				"unique" => KillMethodCategory.Unique,
				_ => throw new NotImplementedException()
			};
		}
	}
}
