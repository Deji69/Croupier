using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Croupier {
	public enum KillMethodType {
		Standard,
		Firearm,
		Specific,
	}

	public enum KillComplication {
		None,
		Live,
	}

	public enum StandardKillMethod {
		None,
		Drowning,
		FallingObject,
		Fall,
		Fire,
		Electrocution,
		Explosion,
		ConsumedPoison,
		InjectedPoison,
		FiberWire,
		NeckSnap,
	}

	public enum FirearmKillMethod {
		None,
		Pistol,
		SMG,
		Sniper,
		Shotgun,
		AssaultRifle,
		Explosive,
	}

	public enum SpecificKillMethod {
		None,
		AmputationKnife,
		AntiqueCurvedKnife,
		BarberRazor,
		BattleAxe,
		BeakStaff,
		Broadsword,
		BurialKnife,
		CircumcisionKnife,
		CombatKnife,
		ConcealableKnife,
		Cleaver,
		FireAxe,
		FoldingKnife,
		GardenFork,
		GrapeKnife,
		Hatchet,
		HobbyKnife,
		Hook,
		Icicle,
		JarlsPirateSaber,
		Katana,
		KitchenKnife,
		KukriMachete,
		LetterOpener,
		Machete,
		MeatFork,
		OldAxe,
		OrnateScimitar,
		RustyScrewdriver,
		Saber,
		SacrificialKnife,
		SappersAxe,
		Scalpel,
		Scissors,
		ScrapSword,
		Screwdriver,
		Seashell,
		Shears,
		Shuriken,
		Starfish,
		Tanto,
		UnicornHorn,
		VikingAxe,

		HolidayFireAxe,
		XmasStar,

		Soders_Electrocution,
		Soders_Explosion,
		Soders_PoisonStemCells,
		Soders_RobotArms,
		Soders_ShootHeart,
		Soders_TrashHeart,
		Yuki_SabotageCableCar,
		Sierra_ShootCar,
		Sierra_DestroyCarExplosive,
	}

	class KillMethodInfo(string name, string image) {
		public string Name { get; set; } = name;
		public string Image { get; set; } = image;
	}

	public partial class KillMethod(string name, string image, KillMethodCategory category, string? target = null, StringCollection? tags = null, StringCollection? keywords = null) : IComparable<KillMethod> {
		public KillMethod(KillMethod method, StringCollection tags) : this(method.Name, method.Image, method.Category, method.Target, [..method.Tags, ..tags], method.Keywords) {
			Variants = method.Variants;
		}

		public string Name { get; set; } = name;
		public string Image { get; set; } = image;
		public KillMethodCategory Category { get; set; } = category;
		public string? Target { get; set; } = target;
		public StringCollection Tags { get; set; } = tags ?? [];
		public StringCollection Keywords { get; set; } = keywords ?? [];
		public List<KillMethodVariant> Variants { get; set; } = [];

		public Uri ImageUri => new(Path.Combine(Environment.CurrentDirectory, "methods", Image));

		public bool IsLive => Tags.Contains("IsLive");

		public bool IsLarge => Tags.Contains("IsLarge");

		public bool IsFirearm => Category == KillMethodCategory.Weapon && !IsExplosive;

		public bool IsLargeFirearm => IsLarge && IsFirearm;

		public bool IsLoud => Tags.Contains("IsLoud");

		public bool IsSilenced => Tags.Contains("IsSilenced");

		public bool IsLoudWeapon => Category == KillMethodCategory.Weapon && IsLoud;

		public bool IsSilencedWeapon => Category == KillMethodCategory.Weapon && IsSilenced;

		public bool IsExplosive => Tags.Contains("IsExplosive");

		public bool IsRemote => IsRemoteTechnically && !IsLoud && !IsImpact;

		public bool IsRemoteOnly => Tags.Contains("IsRemoteOnly");

		public bool IsRemoteTechnically => Tags.Contains("IsRemote");

		public bool IsImpact => Tags.Contains("IsImpact");

		public bool IsMelee => Tags.Contains("IsMelee");

		public bool IsThrown => Tags.Contains("IsThrown");

		public bool CanHaveLiveComplication(Ruleset ruleset) {
			return Category switch {
				KillMethodCategory.Weapon => !IsExplosive || !ruleset.Rules.LiveComplicationsExcludeStandard,
				KillMethodCategory.Melee => true,
				KillMethodCategory.Unique => false,
				KillMethodCategory.Standard => !IsLive && (!ruleset.Rules.LiveComplicationsExcludeStandard || Name == "Neck Snap"),
				_ => throw new NotImplementedException(),
			};
		}

		public KillMethodVariant? GetVariantMatchingKillType(KillType type) {
			if (type == KillType.Any)
				return null;
			return Variants.First(v => v.IsKillType(type));
		}

		public virtual KillMethod GetBasicMethod() {
			return this;
		}

		public virtual bool IsSameMethod(KillMethod other) {
			if (other is KillMethodVariant variant)
				return IsSameMethod(variant.Method);
			return Name == other.Name;
		}

		public bool IsKillType(KillType type) {
			return type switch {
				KillType.Loud => IsLoud && !IsImpact && !IsRemoteOnly,
				KillType.Silenced => IsSilenced,
				KillType.Impact => IsImpact,
				KillType.LoudRemote => IsLoud && IsRemoteTechnically && IsRemoteOnly,
				KillType.Remote => IsRemote && IsRemoteOnly,
				KillType.Melee => IsMelee,
				KillType.Thrown => IsThrown,
				KillType.Any => this is not KillMethodVariant,
				_ => false
			};
		}

		public static KillMethod FromJson(JsonNode json) {
			var name = (json["Name"]?.GetValue<string>()) ?? throw new Exception("Invalid property 'Name'.");
			var image = (json["Image"]?.GetValue<string>()) ?? throw new Exception("Invalid property 'Image'.");
			var category = KillMethodCategoryMethods.FromString((json["Category"]?.GetValue<string>()) ?? throw new Exception("Invalid property 'Category'."));
			var target = json["Target"]?.GetValue<string>();
			StringCollection tags = [];
			StringCollection keywords = [];
			
			foreach (var node in json["Tags"]?.AsArray() ?? []) {
				if (node == null) continue;
				var v = node.GetValue<string>();
				if (v == null || v.Length == 0) continue;
				tags.Add(v);
			}
			
			foreach (var node in json["Keywords"]?.AsArray() ?? []) {
				if (node == null) continue;
				var v = node.GetValue<string>();
				if (v == null || v.Length == 0) continue;
				keywords.Add(v);
			}

			var km = new KillMethod(name, image, category, target, tags, keywords);

			var types = json["Types"]?.AsArray();

			if (types != null && types.Count > 0) {
				foreach (var type in types) {
					if (type == null) continue;
					KillMethodVariant? variant = null;
					if (type.GetValueKind() == JsonValueKind.String) {
						var typeStr = type.GetValue<string>();
						variant = typeStr switch {
							"Loud" => new KillMethodVariant(km, typeStr, image, [.. tags, "IsLoud"]),
							"Impact" => new KillMethodVariant(km, typeStr, image, [.. tags, "IsImpact"]),
							"Silenced" => new KillMethodVariant(km, typeStr, image, [.. tags, "IsSilenced"]),
							"Remote" => new KillMethodVariant(km, typeStr, image, [.. tags, "IsRemote"]),
							"Loud Remote" => new KillMethodVariant(km, typeStr, image, [.. tags, "IsLoud", "IsRemote"]),
							_ => null
						};
					}
					else variant = KillMethodVariant.FromJsonVariant(km, type.AsObject());
					if (variant != null) km.Variants.Add(variant);
				}
			}
			return km;
		}

		public int CompareTo(KillMethod? other) {
			if (other == null) return 0;
			return Name.CompareTo(other.Name);
		}
	}
}
