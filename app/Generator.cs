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
			if (ruleset.suitOnlyMode)
				disguise = Mission.GetSuitDisguise(mission.ID);
			else {
				// Generate disguises, regenerate if necessary based on duplication rule
				do disguise = GenerateDisguise();
				while (spin != null && !ruleset.allowDuplicateDisguise && spin.HasDisguise(disguise));
			}

			// Generate methods until something legal pops up
			do method = GenerateKillMethod(target.Type);
			while (!IsLegalForSpin(spin, target, disguise, method));

			return new SpinCondition() {
				Target = target,
				Disguise = disguise,
				Method = method,
			};
		}

		public Disguise GenerateDisguise() {
			return mission.Disguises[random.Next(mission.Disguises.Count)];
		}

		public KillMethod GenerateKillMethod(TargetType targetType = TargetType.Normal) {
			if (targetType == TargetType.Soders) return GenerateSodersKillMethod();

			// Pick random type of kill method, skip map-specific type if the mission has no such items
			var types = Enum.GetValues(typeof(KillMethodType));
			KillMethod method;
			do {
				method = new KillMethod((KillMethodType)types.GetValue(random.Next(types.Length)));
			} while (
				(method.Type == KillMethodType.Specific && mission.Methods.Count == 0)
			);

			// Pick a random method of the generated type
			var shouldGenerateKillType = random.Next(2) != 0;
			switch (method.Type) {
				case KillMethodType.Standard:
					method.Standard = KillMethod.StandardList[random.Next(KillMethod.StandardList.Count)];
					break;
				case KillMethodType.Firearm:
					method.Firearm = KillMethod.WeaponList[random.Next(KillMethod.WeaponList.Count)];

					// Randomly apply kill types to explosive
					if (method.Firearm == FirearmKillMethod.Explosive) {
						if (!shouldGenerateKillType && ruleset.enableAnyExplosives)
							break;

						List<KillType> explosiveKillTypes = [ KillType.Loud ];
						if (ruleset.enableImpactExplosives)
							explosiveKillTypes.Add(KillType.Impact);
						if (ruleset.enableRemoteExplosives)
							explosiveKillTypes.Add(KillType.Remote);
						if (ruleset.enableLoudRemoteExplosives)
							explosiveKillTypes.Add(KillType.LoudRemote);
						method.KillType = explosiveKillTypes[random.Next(explosiveKillTypes.Count)];
					}
					else {
						if (!shouldGenerateKillType)
							break;

						// Randomly apply loud or silenced to other firearms
						method.KillType = (new[]{ KillType.Loud, KillType.Silenced })[random.Next(2)];
					}
					break;
				case KillMethodType.Specific:
					while (true) {
						method.Specific = mission.Methods[random.Next(mission.Methods.Count)];

						// Skip easter egg conditions unless enabled
						if (!ruleset.enableEasterEggConditions) {
							var easterEggMethods = Mission.GetEasterEggMethods(mission.ID);
							if (easterEggMethods.Contains(method.Specific.Value))
								continue;
						}

						break;
					}

					// Randomly apply thrown or melee to specific melee kills if enabled
					if (shouldGenerateKillType && KillMethod.IsSpecificKillMethodMelee((SpecificKillMethod)method.Specific)) {
						List<KillType> killTypes = [ ];
						if (ruleset.meleeKillTypes) killTypes.Add(KillType.Melee);
						if (ruleset.thrownKillTypes) killTypes.Add(KillType.Thrown);
						method.KillType = killTypes.Count > 0 ? killTypes[random.Next(killTypes.Count)] : KillType.Any;
					}
					break;
			}

			// If live complications enabled and valid for the kill method, randomly apply it
			if (ruleset.liveComplications && CanMethodHaveLiveComplication(method)) {
				if (random.Next(100) + 1 <= ruleset.liveComplicationChance)
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

		private bool IsLegalForSpin(Spin spin, Target target, Disguise disguise, KillMethod method) {
			if (method.IsLargeFirearm && spin.LargeFirearmCount > 0) return false;
			if (spin.HasMethod(method)) return false;
			var tags = target.GetMethodTags(method);
			if (tags.Contains(MethodTag.LoudOnly) && method.IsSilencedWeapon) return false;
			if (DoTagsViolateRules(tags)) return false;
			tags = target.TestRules(disguise, method);
			if (DoTagsViolateRules(tags)) return false;
			return true;
		}
		
		private bool DoTagsViolateRules(List<MethodTag> tags) {
			if (tags.Count == 0) return false;
			return (tags.Contains(MethodTag.Buggy) && !ruleset.enableBuggy)
				|| (tags.Contains(MethodTag.BannedInRR) && !ruleset.enableMedium)
				|| (tags.Contains(MethodTag.Hard) && !ruleset.enableHard)
				|| (tags.Contains(MethodTag.Extreme) && !ruleset.enableExtreme)
				|| (tags.Contains(MethodTag.Impossible) && !ruleset.enableImpossible);
		}
		
		private bool CanMethodHaveLiveComplication(KillMethod method) {
			switch (method.Type)
			{
				case KillMethodType.Firearm:
					// Allow all firearm kills except explosive (unless enabled)
					if (method.Firearm == FirearmKillMethod.Explosive)
						return !ruleset.liveComplicationsExcludeStandard;
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
					return !ruleset.liveComplicationsExcludeStandard;
			}

			return false;
		}
	}
}
