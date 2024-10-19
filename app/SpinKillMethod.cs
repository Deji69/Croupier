using System;
using System.Collections.Generic;

namespace Croupier {
	public class SpinKillMethod(KillMethod method, KillComplication complication = KillComplication.None) {
		public KillMethod Method { get; set; } = method;
		public KillComplication Complication { get; set; } = complication;

		public bool IsLive => Complication == KillComplication.Live;

		public bool IsLargeFirearm => Method.IsLargeFirearm;

		public bool IsLiveFirearm => IsLive && Method.IsFirearm;

		public bool IsSilencedWeapon => Method.IsSilencedWeapon;

		public bool IsLoudWeapon => Method.IsLoudWeapon;

		public bool IsSameMethod(KillMethod method) => Method.IsSameMethod(method);

		public bool IsKillType(KillType type) => Method.IsKillType(type);

		public override string ToString() => (IsLive ? "(Live) " : "") + Method.Name;

		public static SpinKillMethod? Parse(Roulette roulette, string input, Target? target = null) {
			input = input.ToLower().Trim();
			List<string> toks = [];
			toks.AddRange(input.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

			var idx = 0;

			var type = KillType.Any;
			var complication = KillComplication.None;
			KillMethod? method = null;

			for (; idx < toks.Count; ++idx) {
				for (var i = 0; i < 3 && i + idx < toks.Count; ++i) {
					var tok = toks[idx];
					if (i > 0) tok += toks[idx+1];
					if (i > 1) tok += toks[idx+2];
					if (i > 2) tok += toks[idx+3];
					var result = roulette.GetByKeyword(tok, target);
					if (result is KillType t)
						type = t;
					if (result is KillComplication comp)
						complication = comp;
					if (result is KillMethod km)
						method = km;
				}
			}

			if (method == null) return null;
			KillMethodVariant? variant = null;
			if (type != KillType.Any) {
				foreach (var v in method.Variants) {
					if (!v.IsKillType(type)) continue;
					variant = v;
					break;
				}
			}
			return new(variant ?? method, complication);
		}
	}
}
