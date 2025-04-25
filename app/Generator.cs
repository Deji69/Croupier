using System;
using System.Collections.Generic;
using System.Linq;

namespace Croupier
{
	public class Generator(Ruleset ruleset, List<KillMethod>? standardKills = null, List<KillMethod>? weaponKills = null, List<KillMethod>? uniqueKills = null)
	{
		private static readonly Random random = new();
		private readonly Ruleset ruleset = ruleset;
		private readonly List<KillMethod> standardKills = standardKills ?? [];
		private readonly List<KillMethod> weaponKills = weaponKills ?? [];
		private readonly List<KillMethod> uniqueKills = uniqueKills ?? [];

		public Spin Spin(Mission? mission = null) {
			mission ??= Mission.Get(Mission.GetRandomMissionID());
			Spin spin = new();
			mission.Targets.ForEach(target => {
				spin.Conditions.Add(SpinCondition(spin, mission, target));
			});
			return spin;
		}

		public SpinCondition SpinCondition(Spin spin, Mission mission, Target target) {
			if (!mission.Targets.Contains(target))
				throw new ArgumentException($"Target '{target.Name}' is not in the mission '{mission.Name}'.");

			Disguise? disguise = null;
			SpinKillMethod? kill = null;

			if (ruleset.Rules.SuitOnly) {
				// In suit only mode, force the suit disguise for every target
				disguise = mission.SuitDisguise;
				
				if (disguise == null)
					throw new Exception($"Failed to determine suit disguise for mission '{mission.Name}'.");
			}
			else {
				// Generate disguises, regenerate if necessary based on duplication rule
				do disguise = GenerateDisguise(mission);
				while (!ruleset.Rules.AllowDuplicateDisguise && spin.HasDisguise(disguise));
			}
			
			kill = GenerateKill(spin, mission, target, disguise);
			return new SpinCondition(target, disguise, kill);
		}

		public Disguise GenerateDisguise(Mission mission) {
			if (ruleset.Rules.AnyDisguise) {
				var disguises = new List<Disguise>(mission.Disguises) {
					new(mission, "Any", "condition_disguise_any.jpg", false, true)
				};
				return disguises[random.Next(disguises.Count)];
			}
			return mission.Disguises[random.Next(mission.Disguises.Count)];
		}

		public KillMethod? GenerateKillFromSet(List<KillMethod> kills, Spin spin, Mission mission, Target target, Disguise disguise, bool variant = false) {
			var legalKills = kills.Where(k => Croupier.SpinCondition.IsLegalForSpin(spin, mission, target, disguise, k)).ToList();
			if (legalKills.Count == 0) return null;
			var kill = legalKills[random.Next(legalKills.Count)];
			if (kill.Category == KillMethodCategory.Weapon) {
				if (kill.IsExplosive && !ruleset.Rules.AnyExplosives)
					variant = true;
			}
			if (variant) {
				return kill.Category switch {
					KillMethodCategory.Melee => GenerateMeleeKillVariant(spin, mission, target, disguise, kill) ?? kill,
					KillMethodCategory.Weapon => GenerateWeaponKillVariant(spin, mission, target, disguise, kill) ?? kill,
					_ => kill,
				};
			}
			return kill;
		}

		public static KillMethodVariant? GenerateWeaponKillVariant(Spin spin, Mission mission, Target target, Disguise disguise, KillMethod method) {
			var variants = method.Variants.Where(v =>
				Croupier.SpinCondition.IsLegalForSpin(spin, mission, target, disguise, v)
			).ToList();
			if (variants.Count == 0) return null;
			return variants[random.Next(variants.Count)];
		}

		public static KillMethodVariant? GenerateMeleeKillVariant(Spin spin, Mission mission, Target target, Disguise disguise, KillMethod method) {
			var variants = method.Variants.Where(v => 
				Croupier.SpinCondition.IsLegalForSpin(spin, mission, target, disguise, v)
			).ToList();
			if (variants.Count == 0) return null;
			return variants[random.Next(variants.Count)];
		}

		public SpinKillMethod GenerateKill(Spin spin, Mission mission, Target target, Disguise disguise) {
			// Pick random type of kill method, skip map-specific type if the mission has no such items
			var uniqueKills = this.uniqueKills.Where(k => k.Target == target.Name).ToList();
			var hasUniqueKills = uniqueKills.Count > 0;
			var category = KillMethodCategory.Standard;
			List<KillMethodCategory> categories = [KillMethodCategory.Standard, KillMethodCategory.Weapon];

			if (hasUniqueKills && target.Type == TargetType.Unique)
				categories = [KillMethodCategory.Unique, KillMethodCategory.Weapon];

			if (target.Type != TargetType.Unique && mission.Methods.Count > 0)
				categories.Add(KillMethodCategory.Melee);

			var standardKills = this.standardKills.ToList();
			if (hasUniqueKills && target.Type != TargetType.Unique)
				standardKills = [..standardKills, ..uniqueKills];

			KillMethod? method = null;

			do {
				// Pick a random type
				category = categories[random.Next(categories.Count)];

				// Pick a random method of the generated type
				var shouldGenerateType = random.Next(1, 101) <= ruleset.Rules.KillTypeChance;

				method = category switch {
					KillMethodCategory.Standard => GenerateKillFromSet(standardKills, spin, mission, target, disguise),
					KillMethodCategory.Weapon => GenerateKillFromSet(weaponKills, spin, mission, target, disguise, shouldGenerateType),
					KillMethodCategory.Melee => GenerateKillFromSet([..mission.Methods.Select(m => m as KillMethod)], spin, mission, target, disguise, shouldGenerateType),
					KillMethodCategory.Unique => GenerateKillFromSet(uniqueKills, spin, mission, target, disguise, shouldGenerateType),
					_ => throw new NotImplementedException()
				};
			} while (method == null);

			// If live complications enabled and valid for the kill method, randomly apply it
			var complication = KillComplication.None;
			var tryGenerateLive = ruleset.Rules.LiveComplications && random.Next(1, 101) <= ruleset.Rules.LiveComplicationChance;

			if (tryGenerateLive
				&& target.Type != TargetType.Unique && method.CanHaveLiveComplication(ruleset)
				&& Croupier.SpinCondition.IsLegalForSpin(spin, mission, target, disguise, method, KillComplication.Live))
					complication = KillComplication.Live;

			return new SpinKillMethod(method, complication);
		}
	}
}
