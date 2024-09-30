using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Croupier
{
	public class Generator(Ruleset ruleset, Mission mission)
	{
		private static readonly Random random = new();
		private readonly Ruleset ruleset = ruleset;
		private readonly Mission mission = mission;

		public Spin GenerateSpin() {
			Spin spin = new();
			mission.Targets.ForEach(target => {
				spin.Conditions.Add(GenerateCondition(ref target, spin));
			});
			return spin;
		}

		public SpinCondition GenerateCondition(ref Target target, Spin spin = null) {
			if (!mission.Targets.Contains(target))
				throw new ArgumentException("target is not in the selected mission");

			Disguise disguise;
			KillMethod method;

			// In suit only mode, force the suit disguise for every target
			if (ruleset.Rules.SuitOnly)
				disguise = Mission.GetSuitDisguise(mission.ID);
			else {
				// Generate disguises, regenerate if necessary based on duplication rule
				do disguise = GenerateDisguise();
				while (spin != null && !ruleset.Rules.AllowDuplicateDisguise && spin.HasDisguise(disguise));
			}

			// Generate methods until something legal pops up
			do method = GenerateKillMethod(target);
			while (!IsLegalForSpin(spin, target, disguise, method));

			return new SpinCondition() {
				Target = target,
				Disguise = disguise,
				Method = method,
			};
		}

		public Disguise GenerateDisguise() {
			if (Config.Default.Ruleset_EnableAnyDisguise) {
				var disguises = new List<Disguise>(mission.Disguises) {
					new("Any", "condition_disguise_any.jpg", false, true)
				};
				return disguises[random.Next(disguises.Count)];
			}
			return mission.Disguises[random.Next(mission.Disguises.Count)];
		}

		public static KillMethod CreateKillMethod(TargetType target = TargetType.Normal) {
			return new KillMethod(StandardKillMethod.NeckSnap);
		}

		public KillMethod GenerateKillMethod(Target target) {
			if (target.Type == TargetType.Soders) return GenerateSodersKillMethod();

			// Pick random type of kill method, skip map-specific type if the mission has no such items
			KillMethodType type;
			var types = Enum.GetValues(typeof(KillMethodType));
			do type = (KillMethodType)types.GetValue(random.Next(types.Length));
			while (type == KillMethodType.Specific && mission.Methods.Count == 0);

			// Pick a random method of the generated type
			var method = new KillMethod(type);
			var shouldGenerateKillType = random.Next(101) > ruleset.Rules.KillTypeChance;

			switch (method.Type) {
				case KillMethodType.Standard:
					method.Standard = KillMethod.StandardList[random.Next(KillMethod.StandardList.Count)];
					break;
				case KillMethodType.Firearm:
					method.Firearm = KillMethod.WeaponList[random.Next(KillMethod.WeaponList.Count)];

					// Randomly apply kill types to explosive
					if (method.Firearm == FirearmKillMethod.Explosive) {
						if (!shouldGenerateKillType && ruleset.Rules.AnyExplosives)
							break;

						List<KillType> explosiveKillTypes = [ KillType.Loud ];
						if (ruleset.Rules.ImpactExplosives)
							explosiveKillTypes.Add(KillType.Impact);
						if (ruleset.Rules.RemoteExplosives)
							explosiveKillTypes.Add(KillType.Remote);
						if (ruleset.Rules.LoudRemoteExplosives)
							explosiveKillTypes.Add(KillType.LoudRemote);
						method.KillType = explosiveKillTypes[random.Next(explosiveKillTypes.Count)];
					}
					// Randomly apply loud or silenced to other firearms
					else if (shouldGenerateKillType)
						method.KillType = (new[]{ KillType.Loud, KillType.Silenced })[random.Next(2)];
					break;
				case KillMethodType.Specific:
					List<SpecificKillMethod> methods = [..mission.Methods, ..(target.ID == TargetID.SierraKnox ? KillMethod.SierraKillsList : [])];

					while (true) {
						method.Specific = methods[random.Next(methods.Count)];

						// Skip easter egg conditions unless enabled
						if (!ruleset.Rules.Banned.Contains("EasterEgg")) {
							var easterEggMethods = Mission.GetEasterEggMethods(mission.ID);
							if (easterEggMethods.Contains(method.Specific.Value))
								continue;
						}

						break;
					}

					// Randomly apply thrown or melee to specific melee kills if enabled
					if (shouldGenerateKillType && KillMethod.IsSpecificKillMethodMelee((SpecificKillMethod)method.Specific)) {
						List<KillType> killTypes = [ ];
						if (ruleset.Rules.MeleeKillTypes) killTypes.Add(KillType.Melee);
						if (ruleset.Rules.ThrownKillTypes) killTypes.Add(KillType.Thrown);
						method.KillType = killTypes.Count > 0 ? killTypes[random.Next(killTypes.Count)] : KillType.Any;
					}
					break;
			}

			// If live complications enabled and valid for the kill method, randomly apply it
			if (ruleset.Rules.LiveComplications && CanMethodHaveLiveComplication(method)) {
				if (random.Next(100) + 1 <= ruleset.Rules.LiveComplicationChance)
					method.Complication = KillComplication.Live;
			}

			return method;
		}

		private static KillMethod GenerateSodersKillMethod() {
			// Pick random type of kill method, skip map-specific type if the mission has no such items
			var types = new KillMethodType[] { KillMethodType.Firearm, KillMethodType.Specific };
			KillMethod method = new((KillMethodType)types.GetValue(random.Next(types.Length)));
			
			var shouldGenerateKillType = random.Next(2) != 0;

			// Pick a random method of the generated type
			switch (method.Type) {
				case KillMethodType.Firearm:
					method.Firearm = KillMethod.FirearmList[random.Next(KillMethod.FirearmList.Count)];

					if (!shouldGenerateKillType)
						break;

					// Randomly apply loud or silenced
					method.KillType = (new []{ KillType.Loud, KillType.Silenced })[random.Next(2)];
					break;
				case KillMethodType.Specific:
					method.Specific = KillMethod.SodersKillsList[random.Next(KillMethod.SodersKillsList.Count)];
					break;
			}

			return method;
		}

		public bool IsLegalForSpin(Spin spin, Target target, Disguise disguise, KillMethod method) {
			if (IsLargeFirearm(method) && CountLargeFirearms(spin) > 0) return false;
			if (spin.HasMethod(method)) return false;
			var tags = ruleset.GetMethodTags(target.ID, method);
			if (tags.Contains("OnlyLoud") && method.IsSilencedWeapon) return false;
			if (DoTagsViolateRules(tags)) return false;
			tags = ruleset.TestRules(target.ID, disguise, method, mission);
			if (DoTagsViolateRules(tags)) return false;
			return true;
		}

		private bool IsLargeFirearm(KillMethod method) {
			if (ruleset.Rules.LoudSMGIsLargeFirearm && method.IsLoudWeapon && method.Firearm == FirearmKillMethod.SMG)
				return true;
			return method.IsLargeFirearm;
		}

		private int CountLargeFirearms(Spin spin) {
			return spin.Conditions.Count(c => IsLargeFirearm(c.Method));
		}
		
		private bool DoTagsViolateRules(List<string> tags) {
			if (tags.Count == 0 || ruleset.Rules.Banned.Count == 0) return false;
			foreach (var tag in tags) {
				if (ruleset.Rules.Banned.Contains(tag))
					return true;
			}
			return false;
		}
		
		private bool CanMethodHaveLiveComplication(KillMethod method) {
			switch (method.Type) {
				case KillMethodType.Firearm:
					// Allow all firearm kills except explosive (unless enabled)
					if (method.Firearm == FirearmKillMethod.Explosive)
						return !ruleset.Rules.LiveComplicationsExcludeStandard;
					return true;
				case KillMethodType.Specific:
					// Allow all specific melee kills
					return KillMethod.IsSpecificKillMethodMelee((SpecificKillMethod)method.Specific);
				case KillMethodType.Standard:
					// Disallow for standard kills for which it doesn't make sense
					if (KillMethod.IsStandardMethodLiveOnly((StandardKillMethod)method.Standard)) break;
					// Exception for neck snaps, allow based on being 'melee' kills
					if (method.Standard == StandardKillMethod.NeckSnap) return true;
					// Allow all other standard kills if exclude option not enabled
					return !ruleset.Rules.LiveComplicationsExcludeStandard;
			}

			return false;
		}
	}
}
