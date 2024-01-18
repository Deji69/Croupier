using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Croupier.UI {
	public enum KillMethodType {
		Standard,
		Firearm,
		Specific,
	}

	public enum KillType {
		Any,
		Silenced,
		Loud,
		Melee,
		Thrown
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
	}

	class KillMethodInfo(string name, string image) {
		public string Name { get; set; } = name;
		public string Image { get; set; } = image;
	}

	public partial class KillMethod(KillMethodType type) {
		private static readonly Regex TokenCharacterRegex = InvalidTokenCharacterRegex();

		public static readonly List<StandardKillMethod> StandardList = [
			StandardKillMethod.ConsumedPoison,
			StandardKillMethod.Drowning,
			StandardKillMethod.Electrocution,
			StandardKillMethod.Explosion,
			StandardKillMethod.Fall,
			StandardKillMethod.FallingObject,
			StandardKillMethod.FiberWire,
			StandardKillMethod.Fire,
			StandardKillMethod.InjectedPoison,
			StandardKillMethod.NeckSnap,
		];
		public static readonly List<FirearmKillMethod> FirearmList = [
			FirearmKillMethod.AssaultRifle,
			FirearmKillMethod.Pistol,
			FirearmKillMethod.Shotgun,
			FirearmKillMethod.SMG,
			FirearmKillMethod.Sniper,
		];
		public static readonly List<FirearmKillMethod> WeaponList = [
			FirearmKillMethod.AssaultRifle,
			FirearmKillMethod.Pistol,
			FirearmKillMethod.Shotgun,
			FirearmKillMethod.SMG,
			FirearmKillMethod.Sniper,
			FirearmKillMethod.Explosive,
		];
		public static readonly List<SpecificKillMethod> SodersKillsList = [
			SpecificKillMethod.Soders_Electrocution,
			SpecificKillMethod.Soders_Explosion,
			SpecificKillMethod.Soders_PoisonStemCells,
			SpecificKillMethod.Soders_RobotArms,
			SpecificKillMethod.Soders_ShootHeart,
			SpecificKillMethod.Soders_TrashHeart,
		];

		public KillMethodType Type { get; set; } = type;
		public StandardKillMethod? Standard { get; set; } = null;
		public FirearmKillMethod? Firearm { get; set; } = null;
		public SpecificKillMethod? Specific { get; set; } = null;
		public KillComplication? Complication { get; set; } = null;
		public KillType KillType { get; set; } = KillType.Any;

		public string DisplayText {
			get {
				string prefix = "";
				if (Complication != null)
					prefix = GetComplicationName((KillComplication)Complication) + " ";
				if (KillType != KillType.Any)
					prefix += GetKillTypeName(KillType) + " ";
				return prefix + Name;
			}
		}

		public string Serialized {
			get {
				string str = "";
				if (Complication != null)
					str = string.Join("", GetComplicationName((KillComplication)Complication).Split("")) + " ";
				if (KillType != KillType.Any)
					str += string.Join("", GetKillTypeName(KillType).Split("")) + " ";
				str += new string(Name.Where(c => Char.IsLetterOrDigit(c) || c == ' ').ToArray());
				return str;
			}
		}

		public string Name {
			get {
				return GetMethodInfo().Name;
			}
		}

		public string Image {
			get {
				return GetMethodInfo().Image;
			}
		}

		public string ImagePath {
			get {
				return GetMethodInfo().Image;
			}
		}

		public Uri ImageUri {
			get {
				if (Type != KillMethodType.Specific)
					return new Uri(Path.Combine(Environment.CurrentDirectory, "methods", ImagePath));
				return new Uri(Path.Combine(Environment.CurrentDirectory, "weapons", ImagePath));
			}
		}

		public bool IsRemote {
			get {
				if (Type == KillMethodType.Specific) return false;
				if (Type == KillMethodType.Firearm)
					return Firearm == FirearmKillMethod.Explosive && KillType != KillType.Loud;
				if (Type == KillMethodType.Standard) {
					switch (Standard) {
						case StandardKillMethod.ConsumedPoison:
						case StandardKillMethod.Electrocution:
						case StandardKillMethod.Explosion:
						case StandardKillMethod.Fire:
							return true;
					}
				}
				return false;
			}
		}

		public bool IsLargeFirearm {
			get {
				if (Type == KillMethodType.Firearm) {
					switch (Firearm) {
						case FirearmKillMethod.Shotgun:
						case FirearmKillMethod.AssaultRifle:
						case FirearmKillMethod.Sniper:
							return true;
						case FirearmKillMethod.SMG:
							// Brine SMG cannot be brought in concealed weapon slot
							return KillType == KillType.Loud;
					}
				}
				return false;
			}
		}

		public bool IsLiveFirearm {
			get {
				return Type == KillMethodType.Firearm
					&& Complication == KillComplication.Live;
			}
		}

		public bool IsLoudWeapon {
			get {
				return Type == KillMethodType.Firearm
					&& KillType == KillType.Loud;
			}
		}

		public bool IsSilencedWeapon {
			get {
				return Type == KillMethodType.Firearm
					&& KillType == KillType.Silenced;
			}
		}

		public bool IsSameMethod(KillMethod method) {
			if (Type == method.Type) {
				switch (Type) {
					case KillMethodType.Standard:
						return Standard.Value == method.Standard.Value;
					case KillMethodType.Specific:
						return Specific.Value == method.Specific.Value;
					case KillMethodType.Firearm:
						return Firearm.Value == method.Firearm.Value;
				}
			}
			return false;
		}

		private KillMethodInfo GetMethodInfo() {
			switch (Type) {
				case KillMethodType.Standard:
					switch (Standard) {
						case StandardKillMethod.NeckSnap: return new KillMethodInfo("Neck Snap", "condition_killmethod_unarmed.jpg");
						case StandardKillMethod.InjectedPoison: return new KillMethodInfo("Injected Poison", "condition_killmethod_injected_poison.jpg");
						case StandardKillMethod.FiberWire: return new KillMethodInfo("Fiber Wire", "condition_killmethod_fiberwire.jpg");
						case StandardKillMethod.Fall: return new KillMethodInfo("Fall", "condition_killmethod_accident_push.jpg");
						case StandardKillMethod.FallingObject: return new KillMethodInfo("Falling Object", "condition_killmethod_accident_suspended_object.jpg");
						case StandardKillMethod.Drowning: return new KillMethodInfo("Drowning", "condition_killmethod_accident_drown.jpg");
						case StandardKillMethod.Fire: return new KillMethodInfo("Fire", "condition_killmethod_accident_burn.jpg");
						case StandardKillMethod.Electrocution: return new KillMethodInfo("Electrocution", "condition_killmethod_accident_electric.jpg");
						case StandardKillMethod.Explosion: return new KillMethodInfo("Explosion (Accident)", "condition_killmethod_accident_explosion.jpg");
						case StandardKillMethod.ConsumedPoison: return new KillMethodInfo("Consumed Poison", "condition_killmethod_consumed_poison.jpg");
					}
					break;
				case KillMethodType.Firearm:
					switch (Firearm) {
						case FirearmKillMethod.Pistol: return new KillMethodInfo("Pistol", "condition_killmethod_pistol.jpg");
						case FirearmKillMethod.SMG: return new KillMethodInfo("SMG", "condition_killmethod_smg.jpg");
						case FirearmKillMethod.Shotgun: return new KillMethodInfo("Shotgun", "condition_killmethod_shotgun.jpg");
						case FirearmKillMethod.AssaultRifle: return new KillMethodInfo("Assault Rifle", "condition_killmethod_assaultrifle.jpg");
						case FirearmKillMethod.Sniper: return new KillMethodInfo("Sniper", "condition_killmethod_sniperrifle.jpg");
						//case FirearmKillMethod.Elimination: return new KillMethodInfo("Elimination", "condition_killmethod_close_combat_pistol_elimination.jpg");
						//case FirearmKillMethod.PistolElimination: return new KillMethodInfo("Pistol Elimination", "condition_killmethod_close_combat_pistol_elimination.jpg");
						//case FirearmKillMethod.SMGElimination: return new KillMethodInfo("SMG Elimination", "condition_killmethod_close_combat_smg_elimination.jpg");
						case FirearmKillMethod.Explosive: return new KillMethodInfo("Explosive", "condition_killmethod_explosive.jpg");
					}
					break;
				case KillMethodType.Specific:
					switch (Specific) {
						case SpecificKillMethod.AmputationKnife: return new KillMethodInfo("Amputation Knife", "item_perspective_62c2ac2e-329e-4648-822a-e45a29a93cd0_0.jpg");
						case SpecificKillMethod.AntiqueCurvedKnife: return new KillMethodInfo("Antique Curved Knife", "item_perspective_5c211971-235a-4856-9eea-fe890940f63a_0.jpg");
						case SpecificKillMethod.BarberRazor: return new KillMethodInfo("Barber Razor", "item_perspective_5ce2f842-e091-4ead-a51c-1cc406309c8d_0.jpg");
						case SpecificKillMethod.BattleAxe: return new KillMethodInfo("Battle Axe", "item_perspective_58dceb1c-d7db-41dc-9750-55e3ab87fdf0_0.jpg");
						case SpecificKillMethod.BeakStaff: return new KillMethodInfo("Beak Staff", "item_perspective_b153112f-9cd1-4a49-a9c6-ba1a34f443ab_0.jpg");
						case SpecificKillMethod.Broadsword: return new KillMethodInfo("Broadsword", "item_perspective_12200bd8-9605-4111-8b26-4e73cb07d816_0.jpg");
						case SpecificKillMethod.BurialKnife: return new KillMethodInfo("Burial Knife", "item_perspective_6d4c88f3-9a09-453c-9a6e-a081f1136bf3_0.jpg");
						case SpecificKillMethod.CircumcisionKnife: return new KillMethodInfo("Circumcision Knife", "item_perspective_e312a416-5b56-4cb5-8994-1d4bc82fbb84_0.jpg");
						case SpecificKillMethod.Cleaver: return new KillMethodInfo("Cleaver", "item_perspective_1bbf0ed5-0515-4599-a4c9-454ce59cff44_0.jpg");
						case SpecificKillMethod.CombatKnife: return new KillMethodInfo("Combat Knife", "item_perspective_2c037ef5-a01b-4532-8216-1d535193a837_0.jpg");
						case SpecificKillMethod.ConcealableKnife: return new KillMethodInfo("Concealable Knife", "item_perspective_e30a5b15-ce4d-41d5-a2a5-08dec9c4fe79_0.jpg");
						case SpecificKillMethod.FireAxe: return new KillMethodInfo("Fire Axe", "item_perspective_a8bc4325-718e-45ba-b0e4-000729c70ce4_0.jpg");
						case SpecificKillMethod.FoldingKnife: return new KillMethodInfo("Folding Knife", "item_perspective_a2c56798-026f-4d0b-9480-de0d2525a119_0.jpg");
						case SpecificKillMethod.GardenFork: return new KillMethodInfo("Garden Fork", "item_perspective_1a105af8-fd30-447f-8b2c-f908f702e81c_0.jpg");
						case SpecificKillMethod.GrapeKnife: return new KillMethodInfo("Grape Knife", "item_perspective_2b1bd2af-554e-4ea7-a717-3f6d0eb0215f_0.jpg");
						case SpecificKillMethod.Hatchet: return new KillMethodInfo("Hatchet", "item_perspective_3a8207bb-84f5-438f-8f30-5c83aef2af80_0.jpg");
						case SpecificKillMethod.HobbyKnife: return new KillMethodInfo("Hobby Knife", "item_perspective_9e728dc1-3344-4615-be7a-1bcbdd7ad4aa_0.jpg");
						case SpecificKillMethod.Hook: return new KillMethodInfo("Hook", "item_perspective_58a036dc-79d4-4d64-8bf5-3faafa3cfead_0.jpg");
						case SpecificKillMethod.Icicle: return new KillMethodInfo("Icicle", "item_perspective_d689f87e-c3b1-4018-8e78-2f0025cde2a9_0.jpg");
						case SpecificKillMethod.JarlsPirateSaber: return new KillMethodInfo("Jarl's Pirate Saber", "item_perspective_fba6e133-78d1-4af1-8450-1ff30466c553_0.jpg");
						case SpecificKillMethod.Katana: return new KillMethodInfo("Katana", "item_perspective_5631dace-7f4a-4df8-8e97-b47373b815ff_0.jpg");
						case SpecificKillMethod.KitchenKnife: return new KillMethodInfo("Kitchen Knife", "item_perspective_e17172cc-bf70-4df6-9828-d9856b1a24fd_0.jpg");
						case SpecificKillMethod.KukriMachete: return new KillMethodInfo("Kukri Machete", "item_perspective_5db9cefd-391e-4c35-a4c4-bb672ac9b996_0.jpg");
						case SpecificKillMethod.LetterOpener: return new KillMethodInfo("Letter Opener", "item_perspective_f1f89faf-a441-4492-b442-9a923b5ecfd1_0.jpg");
						case SpecificKillMethod.Machete: return new KillMethodInfo("Machete", "item_perspective_3e3819ca-4d19-4e0a-a238-4bd16c730e61_0.jpg");
						case SpecificKillMethod.MeatFork: return new KillMethodInfo("Meat Fork", "item_perspective_66024572-7838-42d3-8c7b-c651e259438e_0.jpg");
						case SpecificKillMethod.OldAxe: return new KillMethodInfo("Old Axe", "item_perspective_369c68f7-cbef-4e45-83c7-8acd0dc2d237_0.jpg");
						case SpecificKillMethod.OrnateScimitar: return new KillMethodInfo("Ornate Scimitar", "item_perspective_b4d4ed1a-0687-48a9-a731-0e3b99494eb6_0.jpg");
						case SpecificKillMethod.RustyScrewdriver: return new KillMethodInfo("Rusty Screwdriver", "item_perspective_ecf022db-ecfd-48c0-97b5-2258e4e89a65_0.jpg");
						case SpecificKillMethod.Saber: return new KillMethodInfo("Saber", "item_perspective_94f52181-b9ec-4363-baef-d53b4e424b74_0.jpg");
						case SpecificKillMethod.SacrificialKnife: return new KillMethodInfo("Sacrificial Knife", "item_perspective_b2321154-4520-4911-9d94-9256b85e0983_0.jpg");
						case SpecificKillMethod.SappersAxe: return new KillMethodInfo("Sapper's Axe", "item_perspective_d2a7fa04-2cac-45d8-b696-47c566bb95ff_0.jpg");
						case SpecificKillMethod.Seashell: return new KillMethodInfo("Seashell", "item_perspective_7bc45270-83fe-4cf6-ad10-7d1b0cf3a3fd_0.jpg");
						case SpecificKillMethod.Scalpel: return new KillMethodInfo("Scalpel", "item_perspective_5d8ca32a-fe4c-4597-b074-51e36c3de898_0.jpg");
						case SpecificKillMethod.Scissors: return new KillMethodInfo("Scissors", "item_perspective_6ecf1f15-453c-4783-9c70-8777c83934d7_0.jpg");
						case SpecificKillMethod.ScrapSword: return new KillMethodInfo("Scrap Sword", "item_perspective_d73251b4-4860-4b5b-8376-7c9cf2a054a2_0.jpg");
						case SpecificKillMethod.Screwdriver: return new KillMethodInfo("Screwdriver", "item_perspective_12cb6b51-a6dd-4bf5-9653-0ab727820cac_0.jpg");
						case SpecificKillMethod.Shears: return new KillMethodInfo("Shears", "item_perspective_42c7bb52-a71b-489c-8a74-7db0c09ba313_0.jpg");
						case SpecificKillMethod.Shuriken: return new KillMethodInfo("Shuriken", "item_perspective_e55eb9a4-e79c-43c7-970b-79e94e7683b7_0.jpg");
						case SpecificKillMethod.Starfish: return new KillMethodInfo("Starfish", "item_perspective_cad726d7-331d-4601-9723-6b8a17e5f91b_0.jpg");
						case SpecificKillMethod.Tanto: return new KillMethodInfo("Tanto", "item_perspective_9488fa1e-10e1-49c9-bb24-6635d2e5bd49_0.jpg");
						case SpecificKillMethod.UnicornHorn: return new KillMethodInfo("Unicorn Horn", "item_perspective_58769c58-3e70-4746-be8e-4c7114f8c2bb_0.jpg");
						case SpecificKillMethod.VikingAxe: return new KillMethodInfo("Viking Axe", "item_perspective_9a7711c7-ede9-4230-853e-ab94c65fc0c9_0.jpg");
						case SpecificKillMethod.HolidayFireAxe: return new KillMethodInfo("Holiday Fire Axe", "item_perspective_2add9602-cda7-43fd-9758-6269c8fbb233_0.jpg");
						case SpecificKillMethod.XmasStar: return new KillMethodInfo("Xmas Star", "item_perspective_1a852006-e632-401f-aedc-d0cf76521b1f_0.jpg");
						case SpecificKillMethod.Soders_Electrocution: return new KillMethodInfo("Electrocution", "snowcrane_sign_soders_electrocute.jpg");
						case SpecificKillMethod.Soders_Explosion: return new KillMethodInfo("Explosion", "condition_killmethod_accident_explosion.jpg");
						case SpecificKillMethod.Soders_PoisonStemCells: return new KillMethodInfo("Poison Stem Cells", "snowcrane_soders_poison.jpg");
						case SpecificKillMethod.Soders_RobotArms: return new KillMethodInfo("Robot Arms", "snowcrane_soders_spidermachine.jpg");
						case SpecificKillMethod.Soders_ShootHeart: return new KillMethodInfo("Shoot Heart", "snowcrane_sign_soders_heart.jpg");
						case SpecificKillMethod.Soders_TrashHeart: return new KillMethodInfo("Trash Heart", "snowcrane_throw_away_heart.jpg");
					}
					break;
			}
			throw new NotImplementedException("No info found for kill method");
		}

		public static bool IsSpecificKillMethodMelee(SpecificKillMethod method)
		{
			return method switch {
				SpecificKillMethod.Soders_Electrocution
				or SpecificKillMethod.Soders_Explosion
				or SpecificKillMethod.Soders_PoisonStemCells
				or SpecificKillMethod.Soders_RobotArms
				or SpecificKillMethod.Soders_ShootHeart
				or SpecificKillMethod.Soders_TrashHeart
				or SpecificKillMethod.Yuki_SabotageCableCar => false,
				_ => true,
			};
		}

		public static bool IsStandardMethodLiveOnly(StandardKillMethod method)
		{
			return method switch {
				StandardKillMethod.Drowning
				or StandardKillMethod.InjectedPoison
				or StandardKillMethod.FiberWire => true,
				_ => false,
			};
		}

		public static string GetComplicationName(KillComplication complication) {
			return complication switch {
				KillComplication.Live => "(Live)",
				_ => "",
			};
		}

		public static string GetKillTypeName(KillType type) {
			return type switch {
				KillType.Silenced => "Silenced",
				KillType.Loud => "Loud",
				KillType.Melee => "Melee",
				KillType.Thrown => "Thrown",
				_ => "",
			};
		}

		public static bool ParseComplication(string input, out KillComplication complication) {
			complication = TokenCharacterRegex.Replace(input, "").ToLower() switch {
				"live" or "ntko" or "notargetko" or "noko" or "nko" or "nonko" => KillComplication.Live,
				_ => KillComplication.None,
			};
			return complication != KillComplication.None;
		}

		public static bool ParseStandardMethod(string input, out StandardKillMethod method)
		{
			method = TokenCharacterRegex.Replace(input, "").ToLower() switch {
				"necksnap" or "unarmed" or "snapneck" => StandardKillMethod.NeckSnap,
				"injected" or "injectedpoison" or "needle" or "syringe" => StandardKillMethod.InjectedPoison,
				"drown" or "drowning" => StandardKillMethod.Drowning,
				"electro" or "electrocution" or "electrocute" or "electric" or "electricity" => StandardKillMethod.Electrocution,
				"explosion" or "explode" or "explosionaccident" or "accidentexplosion" or "accexplosion" or "explosionacc" or "propane" => StandardKillMethod.Explosion,
				"fallingobj" or "fo" or "fallingobject" or "suspendedobject" => StandardKillMethod.FallingObject,
				"fall" or "gravity" => StandardKillMethod.Fall,
				"fire" or "burn" => StandardKillMethod.Fire,
				"fiberwire" or "fw" or "fiber" or "wire" => StandardKillMethod.FiberWire,
				"consumed" or "consumedpoison" or "poison" or "lethalpoison" => StandardKillMethod.ConsumedPoison,
				_ => StandardKillMethod.None,
			};
			return method != StandardKillMethod.None;
		}

		public static bool ParseFirearmMethod(string input, out FirearmKillMethod method)
		{
			method = TokenCharacterRegex.Replace(input, "").ToLower() switch {
				"ar" or "assaultrifle" => FirearmKillMethod.AssaultRifle,
				"bomb" or "explosiveweapon" or "explosive" => FirearmKillMethod.Explosive,
				"pistol" => FirearmKillMethod.Pistol,
				"shotgun" or "shotty" => FirearmKillMethod.Shotgun,
				"smg" => FirearmKillMethod.SMG,
				"sniper" or "sniperrifle" or "snipe" => FirearmKillMethod.Sniper,
				_ => FirearmKillMethod.None,
			};
			return method != FirearmKillMethod.None;
		}

		static readonly Dictionary<string, SpecificKillMethod> specificKillMethodDictionary = new(){
			{"amputation", SpecificKillMethod.AmputationKnife},
			{"ampknife", SpecificKillMethod.AmputationKnife},
			{"amputationknife", SpecificKillMethod.AmputationKnife},
			{"antiquecurvedknife", SpecificKillMethod.AntiqueCurvedKnife},
			{"antiquecurveknife", SpecificKillMethod.AntiqueCurvedKnife},
			{"antiqueknife", SpecificKillMethod.AntiqueCurvedKnife},
			{"curvedknife", SpecificKillMethod.AntiqueCurvedKnife},
			{"curveknife", SpecificKillMethod.AntiqueCurvedKnife},
			{"acknife", SpecificKillMethod.AntiqueCurvedKnife},
			{"barberrazor", SpecificKillMethod.BarberRazor},
			{"razor", SpecificKillMethod.BarberRazor},
			{"battleaxe", SpecificKillMethod.BattleAxe},
			{"bataxe", SpecificKillMethod.BattleAxe},
			{"baxe", SpecificKillMethod.BattleAxe},
			{"beakstaff", SpecificKillMethod.BeakStaff},
			{"broadsword", SpecificKillMethod.Broadsword},
			{"burialknife", SpecificKillMethod.BurialKnife},
			{"burialdagger", SpecificKillMethod.BurialKnife},
			{"circumcision", SpecificKillMethod.CircumcisionKnife},
			{"circumcisionknife", SpecificKillMethod.CircumcisionKnife},
			{"circknife", SpecificKillMethod.CircumcisionKnife},
			{"cleaver", SpecificKillMethod.Cleaver},
			{"meatcleaver", SpecificKillMethod.Cleaver},
			{"combatknife", SpecificKillMethod.CombatKnife},
			{"concealknife", SpecificKillMethod.ConcealableKnife},
			{"concealableknife", SpecificKillMethod.ConcealableKnife},
			{"fireaxe", SpecificKillMethod.FireAxe},
			{"foldingknife", SpecificKillMethod.FoldingKnife},
			{"foldknife", SpecificKillMethod.FoldingKnife},
			{"gardenfork", SpecificKillMethod.GardenFork},
			{"grapeknife", SpecificKillMethod.GrapeKnife},
			{"hatchet", SpecificKillMethod.Hatchet},
			{"hobby", SpecificKillMethod.HobbyKnife},
			{"hobbyknife", SpecificKillMethod.HobbyKnife},
			{"boxcutter", SpecificKillMethod.HobbyKnife},
			{"holidayfireaxe", SpecificKillMethod.HolidayFireAxe},
			{"festivefireaxe", SpecificKillMethod.HolidayFireAxe},
			{"christmasfireaxe", SpecificKillMethod.HolidayFireAxe},
			{"xmasfireaxe", SpecificKillMethod.HolidayFireAxe},
			{"hook", SpecificKillMethod.Hook},
			{"icicle", SpecificKillMethod.Icicle},
			{"ice", SpecificKillMethod.Icicle},
			{"jarlspiratesaber", SpecificKillMethod.JarlsPirateSaber},
			{"piratesaber", SpecificKillMethod.JarlsPirateSaber},
			{"jarlssaber", SpecificKillMethod.JarlsPirateSaber},
			{"piratesword", SpecificKillMethod.JarlsPirateSaber},
			{"jarlspiratesword", SpecificKillMethod.JarlsPirateSaber},
			{"jarlssword", SpecificKillMethod.JarlsPirateSaber},
			{"katana", SpecificKillMethod.Katana},
			{"katanasword", SpecificKillMethod.Katana},
			{"kitchenknife", SpecificKillMethod.KitchenKnife},
			{"kknife", SpecificKillMethod.KitchenKnife},
			{"knife", SpecificKillMethod.KitchenKnife},
			{"kukrimachete", SpecificKillMethod.KukriMachete},
			{"kukri", SpecificKillMethod.KukriMachete},
			{"letteropener", SpecificKillMethod.LetterOpener},
			{"letopener", SpecificKillMethod.LetterOpener},
			{"machete", SpecificKillMethod.Machete},
			{"meatfork", SpecificKillMethod.MeatFork},
			{"fork", SpecificKillMethod.MeatFork},
			{"oldaxe", SpecificKillMethod.OldAxe},
			{"ornatescimitar", SpecificKillMethod.OrnateScimitar},
			{"ornate", SpecificKillMethod.OrnateScimitar},
			{"scimitar", SpecificKillMethod.OrnateScimitar},
			{"rustyscrewdriver", SpecificKillMethod.RustyScrewdriver},
			{"rustedscrewdriver", SpecificKillMethod.RustyScrewdriver},
			{"rustscrewdriver", SpecificKillMethod.RustyScrewdriver},
			{"saber", SpecificKillMethod.Saber},
			{"sacknife", SpecificKillMethod.SacrificialKnife},
			{"sacrificial", SpecificKillMethod.SacrificialKnife},
			{"sacraficial", SpecificKillMethod.SacrificialKnife},
			{"sacrificialknife", SpecificKillMethod.SacrificialKnife},
			{"sacraficialknife", SpecificKillMethod.SacrificialKnife},
			{"sappersaxe", SpecificKillMethod.SappersAxe},
			{"sappers", SpecificKillMethod.SappersAxe},
			{"sapperaxe", SpecificKillMethod.SappersAxe},
			{"sapaxe", SpecificKillMethod.SappersAxe},
			{"scalpel", SpecificKillMethod.Scalpel},
			{"scissors", SpecificKillMethod.Scissors},
			{"scissor", SpecificKillMethod.Scissors},
			{"scrapsword", SpecificKillMethod.ScrapSword},
			{"scrap", SpecificKillMethod.ScrapSword},
			{"screwdriver", SpecificKillMethod.Screwdriver},
			{"seashell", SpecificKillMethod.Seashell},
			{"shell", SpecificKillMethod.Seashell},
			{"shears", SpecificKillMethod.Shears},
			{"shear", SpecificKillMethod.Shears},
			{"shuriken", SpecificKillMethod.Shuriken},
			{"electrocution", SpecificKillMethod.Soders_Electrocution},
			{"electro", SpecificKillMethod.Soders_Electrocution},
			{"electrocute", SpecificKillMethod.Soders_Electrocution},
			{"explosion", SpecificKillMethod.Soders_Explosion},
			{"explosive", SpecificKillMethod.Soders_Explosion},
			{"stemcells", SpecificKillMethod.Soders_PoisonStemCells},
			{"poisonstemcells", SpecificKillMethod.Soders_PoisonStemCells},
			{"poison", SpecificKillMethod.Soders_PoisonStemCells},
			{"poisoncells", SpecificKillMethod.Soders_PoisonStemCells},
			{"robotarms", SpecificKillMethod.Soders_RobotArms},
			{"arms", SpecificKillMethod.Soders_RobotArms},
			{"robot", SpecificKillMethod.Soders_RobotArms},
			{"kai", SpecificKillMethod.Soders_RobotArms},
			{"shootheart", SpecificKillMethod.Soders_ShootHeart},
			{"shoottheheart", SpecificKillMethod.Soders_ShootHeart},
			{"heartshot", SpecificKillMethod.Soders_ShootHeart},
			{"trashheart", SpecificKillMethod.Soders_TrashHeart},
			{"trashcan", SpecificKillMethod.Soders_TrashHeart},
			{"throwheartinthetrash", SpecificKillMethod.Soders_TrashHeart},
			{"throwtheheartinthetrash", SpecificKillMethod.Soders_TrashHeart},
			{"throwheartintrash", SpecificKillMethod.Soders_TrashHeart},
			{"starfish", SpecificKillMethod.Starfish},
			{"tanto", SpecificKillMethod.Tanto},
			{"unicornhorn", SpecificKillMethod.UnicornHorn},
			{"unicorn", SpecificKillMethod.UnicornHorn},
			{"vikingaxe", SpecificKillMethod.VikingAxe},
			{"viking", SpecificKillMethod.VikingAxe},
			{"xmasstar", SpecificKillMethod.XmasStar},
			{"christmasstar", SpecificKillMethod.XmasStar},
			{"festivestar", SpecificKillMethod.XmasStar},
		};

		public static bool ParseSpecificMethod(string input, out SpecificKillMethod method)
		{
			return specificKillMethodDictionary.TryGetValue(TokenCharacterRegex.Replace(input, "").ToLower(), out method);
		}

		public static bool ParseKillType(string input, out KillType? type)
		{
			type = TokenCharacterRegex.Replace(input.ToLower(), "") switch {
				"sil" or "silence" or "silenced" => KillType.Silenced,
				"ld" or "loud" => KillType.Loud,
				"melee" => KillType.Melee,
				"throw" or "thrown" => KillType.Thrown,
				"any" => KillType.Any,
				"" => KillType.Any,
				_ => null,
			};
			return type != null;
		}

		public static bool Parse(string input, out KillMethod method)
		{
			input = input.Trim();
			var toks = input.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var idx = 0;
			KillType? killType = null;
			KillComplication? complication = null;
			method = null;

			for (; idx < toks.Length; ++idx) {
				if (ParseKillType(toks[idx], out var kt))
					killType ??= kt;
				else if (ParseComplication(toks[idx], out var newComplication))
					complication ??= newComplication;
				else break;
			}

			if (idx < toks.Length) {
				var methodToks = toks[idx..];
				if (methodToks.Length > 0) {
					var methodToken = string.Join("", methodToks);
					if (ParseFirearmMethod(methodToken, out FirearmKillMethod firearm))
						method = new(KillMethodType.Firearm){Complication = complication, Firearm = firearm, KillType = killType ?? KillType.Any};
					else if (ParseStandardMethod(methodToken, out StandardKillMethod standard))
						method = new(KillMethodType.Standard){Complication = complication, Standard = standard, KillType = killType ?? KillType.Any};
					else if (ParseSpecificMethod(methodToken, out SpecificKillMethod specific))
						method = new(KillMethodType.Specific){Complication = complication, Specific = specific, KillType = killType ?? KillType.Any};
				}
			}

			return method != null;
		}

		[GeneratedRegex("[^a-zA-Z0-9]")]
		private static partial Regex InvalidTokenCharacterRegex();
	}
}
