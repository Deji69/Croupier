using Croupier.Exceptions;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Croupier.GameEvents {
	public class SVector3 {
		public required double X { get; set; }
		public required double Y { get; set; }
		public required double Z { get; set; }
	}

	public enum EActorType {
		eAT_Civilian = 0,
		eAT_Guard    = 1,
		eAT_Hitman   = 2,
		eAT_Last     = 3,
	}
	public enum EOutfitType {
		eOT_None      = 0,
		eOT_Suit      = 1,
		eOT_Guard     = 2,
		eOT_Worker    = 3,
		eOT_Waiter    = 4,
		eOT_LucasGrey = 5,
	}
	public enum EKillType {
		EKillType_Undefined = 0,
		EKillType_Throw = 1,
		EKillType_Fiberwire = 2,
		EKillType_PistolExecute = 3,
		EKillType_ItemTakeOutFront = 4,
		EKillType_ItemTakeOutBack = 5,
		EKillType_ChokeOut = 6,
		EKillType_SnapNeck = 7,
		EKillType_KnockOut = 8,
		EKillType_Push = 9,
		EKillType_Pull = 10,
	}

	public enum EDeathContext {
		eDC_UNDEFINED = 0,
		eDC_NOT_HERO  = 1,
		eDC_HIDDEN    = 2,
		eDC_ACCIDENT  = 3,
		eDC_MURDER    = 4,
	}

	public enum EDeathType {
		eDT_UNDEFINED   = 0,
		eDT_PACIFY      = 1,
		eDT_KILL        = 2,
		eDT_BLOODY_KILL = 3,
	}

	public enum EWeaponAnimationCategory {
		eWAC_Undefined = 0,
		eWAC_Pistol = 1,
		eWAC_Revolver = 2,
		eWAC_SMG_2H = 3,
		eWAC_SMG_1H = 4,
		eWAC_Rifle = 5,
		eWAC_Sniper = 6,
		eWAC_Shotgun_Pump = 7,
		eWAC_Shotgun_Semi = 8,
	};

	public enum EWeaponType {
		WT_HANDGUN      = 0,
		WT_SLOWGUN      = 1,
		WT_ASSAULTRIFLE = 2,
		WT_SMG          = 3,
		WT_SNIPER       = 4,
		WT_RPG          = 5,
		WT_KNIFE        = 6,
		WT_SHOTGUN      = 7,
		WT_SPOTTER      = 8,
	};

	public enum SecuritySystemRecorderEvent {
		Undefined,
		Spotted,
		Erased,
		Destroyed,
		CameraDestroyed,
	}

	public class Event {
		public required string Name { get; set; }
		public double Timestamp { get; set; }
		public object? Value { get; set; }
	}

	public class EventValue {
		protected static bool? TryLoadBool(JsonElement json, string propName) {
			try {
				if (json.TryGetProperty(propName, out var prop))
					return prop.GetBoolean();
			}
			catch { }
			return null;
		}

		protected static Int64? TryLoadInt64(JsonElement json, string propName) {
			try {
				if (json.TryGetProperty(propName, out var prop))
					return prop.GetInt64();
			}
			catch { }
			return null;
		}


		protected static UInt64? TryLoadUInt64(JsonElement json, string propName) {
			try {
				if (json.TryGetProperty(propName, out var prop))
					return prop.GetUInt64();
			}
			catch { }
			return null;
		}
		protected static double? TryLoadDouble(JsonElement json, string propName) {
			try {
				if (json.TryGetProperty(propName, out var prop))
					return prop.GetDouble();
			}
			catch { }
			return null;
		}

		protected static string? TryLoadString(JsonElement json, string propName) {
			try {
				if (json.TryGetProperty(propName, out var prop))
					return prop.GetString();
			}
			catch { }
			return null;
		}

		protected static SVector3? TryLoadSVector3(JsonElement json, string propName) {
			try {
				if (json.TryGetProperty(propName, out var prop)) {
					var x = TryLoadDouble(prop, nameof(SVector3.X));
					var y = TryLoadDouble(prop, nameof(SVector3.Y));
					var z = TryLoadDouble(prop, nameof(SVector3.Z));
					return x != null && y != null && z != null ? new SVector3() {
						X = (double)x,
						Y = (double)y,
						Z = (double)z,
					} : null;
				}
			}
			catch { }
			return null;
		}

		protected static T? TryLoad<T>(JsonElement json) {
			try {
				var res = json.Deserialize<T>(jsonSerializerOptions);
				return res;
			} catch (Exception) {}
			return default;
		}

		protected static T? TryLoad<T>(JsonElement json, string propName) {
			try {
				if (json.TryGetProperty(propName, out var prop)) {
					var res = prop.Deserialize<T>(jsonSerializerOptions);
					return res;
				}
			} catch (Exception) {}
			return default;
		}

		public static T Load<T>(JsonElement json) {
			return TryLoad<T>(json) ?? throw new BingoException($"Failed to load event message.");
		}

		public static T Load<T>(JsonElement json, string propName) {
			return TryLoad<T>(json, propName) ?? throw new BingoException($"Failed to load event message.");
		}

		protected static bool LoadBool(JsonElement json, string propName) {
			return TryLoadBool(json, propName) ?? throw new BingoException($"Failed to load property {propName} in event message.");
		}

		protected static Int64 LoadInt64(JsonElement json, string propName) {
			return TryLoadInt64(json, propName) ?? throw new BingoException($"Failed to load property {propName} in event message.");
		}

		protected static string LoadString(JsonElement json, string propName) {
			var result = TryLoadString(json, propName);
			if (result == null)
				throw new BingoException($"Failed to load property {propName} in event message.");
			return result;
		}

		private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};
	}

	public class ActorInfoImbuedEventValue : EventValue {
		public string? ActorName { get; set; }
		public string? ActorRepositoryId { get; set; }
		public string? ActorOutfitRepositoryId { get; set; }
		public EActorType? ActorType { get; set; }
		public Int64? ActorWeaponIndex { get; set; }
		public bool? ActorHasDisguise { get; set; }
		public bool? ActorIsAuthorityFigure { get; set; }
		public bool? ActorIsDead { get; set; }
		public bool? ActorIsFemale { get; set; }
		public bool? ActorIsPacified { get; set; }
		public bool? ActorIsTarget { get; set; }
		public bool? ActorOutfitAllowsWeapons { get; set; }
		public bool? ActorWeaponUnholstered { get; set; }

		public static ActorInfoImbuedEventValue Load(JsonElement json) {
			return new() {
				ActorName = TryLoadString(json, nameof(ActorName)),
				ActorRepositoryId = TryLoadString(json, nameof(ActorRepositoryId)),
				ActorOutfitRepositoryId = TryLoadString(json, nameof(ActorOutfitRepositoryId)),
				ActorType = TryLoad<EActorType>(json, nameof(ActorType)),
				ActorWeaponIndex = TryLoadInt64(json, nameof(ActorWeaponIndex)),
				ActorHasDisguise = TryLoadBool(json, nameof(ActorHasDisguise)),
				ActorIsAuthorityFigure = TryLoadBool(json, nameof(ActorIsAuthorityFigure)),
				ActorIsDead = TryLoadBool(json, nameof(ActorIsDead)),
				ActorIsFemale = TryLoadBool(json, nameof(ActorIsFemale)),
				ActorIsPacified = TryLoadBool(json, nameof(ActorIsPacified)),
				ActorIsTarget = TryLoadBool(json, nameof(ActorIsTarget)),
				ActorOutfitAllowsWeapons = TryLoadBool(json, nameof(ActorOutfitAllowsWeapons)),
				ActorWeaponUnholstered = TryLoadBool(json, nameof(ActorWeaponUnholstered)),
			};
		}
	}

	public class ItemEventValue : EventValue {
		public string? ItemName { get; set; }
		public string? ItemType { get; set; }
		public UInt64? ItemInstanceId { get; set; }
		public string? ItemRepositoryId { get; set; }
		public string? WeaponAnimFrontSide { get; set; }
		public string? WeaponAnimBack { get; set; }
		public bool? IsScopedWeapon { get; set; }
		public EWeaponAnimationCategory? WeaponAnimationCategory { get; set; }
		public EWeaponType? WeaponType { get; set; }
		public bool? IsCloseCombatWeapon { get; set; }
		public bool? IsFiberWire { get; set; }
		public bool? IsFirearm { get; set; }
		public string? RepositoryItemType { get; set; }
		public string? RepositoryItemSize { get; set; }
		public List<string>? RepositoryPerks { get; set; }

		public static ItemEventValue Load(JsonElement json) {
			return new() {
				ItemName = TryLoadString(json, nameof(ItemName)),
				ItemType = TryLoadString(json, nameof(ItemType)),
				ItemInstanceId = TryLoadUInt64(json, nameof(ItemInstanceId)),
				ItemRepositoryId = TryLoadString(json, nameof(ItemRepositoryId)),
				WeaponAnimFrontSide = TryLoadString(json, nameof(WeaponAnimFrontSide)),
				WeaponAnimBack = TryLoadString(json, nameof(WeaponAnimBack)),
				IsScopedWeapon = TryLoadBool(json, nameof(IsScopedWeapon)),
				WeaponAnimationCategory = TryLoad<EWeaponAnimationCategory>(json, nameof(WeaponAnimationCategory)),
				WeaponType = TryLoad<EWeaponType>(json, nameof(WeaponType)),
				IsCloseCombatWeapon = TryLoadBool(json, nameof(IsCloseCombatWeapon)),
				IsFiberWire = TryLoadBool(json, nameof(IsFiberWire)),
				IsFirearm = TryLoadBool(json, nameof(IsFirearm)),
				RepositoryItemType = TryLoadString(json, nameof(RepositoryItemType)),
				RepositoryItemSize = TryLoadString(json, nameof(RepositoryItemSize)),
				RepositoryPerks = TryLoad<List<string>>(json, nameof(RepositoryPerks)),
			};
		}
	}

	public class LocationEventValue : EventValue {
		public Int64? Room { get; set; }
		public string? Area { get; set; }
		public SVector3? Position { get; set; }
		public string? ActorArea { get; set; }
		public SVector3? ActorPosition { get; set; }
		public Int64? ActorRoom { get; set; }
		public string? HeroArea { get; set; }
		public SVector3? HeroPosition { get; set; }
		public Int64? HeroRoom { get; set; }
		public string? ItemArea { get; set; }
		public SVector3? ItemPosition { get; set; }
		public Int64? ItemRoom { get; set; }
		public bool? IsCrouching { get; set; }
		public bool? IsIdle { get; set; }
		public bool? IsRunning { get; set; }
		public bool? IsWalking { get; set; }
		public bool? IsTrespassing { get; set; }

		public static LocationEventValue Load(JsonElement json) {
			return new() {
				Room = TryLoadInt64(json, nameof(Room)),
				Area = TryLoadString(json, nameof(Area)),
				Position = TryLoadSVector3(json, nameof(Position)),
				ActorArea = TryLoadString(json, nameof(ActorArea)),
				ActorPosition = TryLoadSVector3(json, nameof(ActorPosition)),
				ActorRoom = TryLoadInt64(json, nameof(ActorRoom)),
				HeroArea = TryLoadString(json, nameof(HeroArea)),
				HeroPosition = TryLoadSVector3(json, nameof(HeroPosition)),
				HeroRoom = TryLoadInt64(json, nameof(HeroRoom)),
				ItemArea = TryLoadString(json, nameof(ItemArea)),
				ItemPosition = TryLoadSVector3(json, nameof(ItemPosition)),
				ItemRoom = TryLoadInt64(json, nameof(ItemRoom)),
				IsCrouching = TryLoadBool(json, nameof(IsCrouching)),
				IsIdle = TryLoadBool(json, nameof(IsIdle)),
				IsRunning = TryLoadBool(json, nameof(IsRunning)),
				IsWalking = TryLoadBool(json, nameof(IsWalking)),
				IsTrespassing = TryLoadBool(json, nameof(IsTrespassing)),
			};
		}
	}

	public class PlayerEventValue : EventValue {
		public string? HeroOutfit { get; set; }
		public bool? HeroOutfitIsHitmanSuit { get; set; }
		public string? Outfit { get; set; }
		public bool? OutfitIsHitmanSuit { get; set; }

		public static PlayerEventValue Load(JsonElement json) {
			return new() {
				HeroOutfit = TryLoadString(json, nameof(HeroOutfit)),
				Outfit = TryLoadString(json, nameof(Outfit)),
				HeroOutfitIsHitmanSuit = TryLoadBool(json, nameof(HeroOutfitIsHitmanSuit)),
				OutfitIsHitmanSuit = TryLoadBool(json, nameof(OutfitIsHitmanSuit)),
			};
		}
	}

	public class DoorUnlockedEventValue : EventValue {
		public required PlayerEventValue Player;
		public required LocationEventValue Location;

		public static DoorUnlockedEventValue? Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class DoorBrokenEventValue : EventValue {
		public required PlayerEventValue Player;
		public required LocationEventValue Location;

		public static DoorBrokenEventValue? Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class LoadoutItemEventValue {
		public string? RepositoryId { get; set; }
		public string? InstanceId { get; set; }
		public List<string>? OnlineTraits { get; set; }
	}

	public class DamageHistoryEventValue : EventValue {
		public bool? Explosive { get; set; }
		public bool? Headshot { get; set; }
		public bool? Accident { get; set; }
		public bool? WeaponSilenced { get; set; }
		public bool? Projectile { get; set; }
		public bool? Sniper { get; set; }
		public bool? ThroughWall { get; set; }
		public string? InstanceId { get; set; }
		public string? RepositoryId { get; set; }
		public int? BodyPartId { get; set; }
		public int? TotalDamage { get; set; }
	}

	public class PacifyEventValue : EventValue {
		public string? RepositoryId { get; set; }
		public string? ActorName { get; set; }
		public EActorType? ActorType { get; set; }
		public EKillType? KillType { get; set; }
		public EDeathContext? KillContext { get; set; }
		public string? KillClass { get; set; }
		public bool? Accident { get; set; }
		public bool? WeaponSilenced { get; set; }
		public bool? Explosive { get; set; }
		public double? ExplosionType { get; set; }
		public bool? Projectile { get; set; }
		public bool? Sniper { get; set; }
		public bool? IsHeadshot { get; set; }
		public bool? IsTarget { get; set; }
		public bool? ThroughWall { get; set; }
		public double? BodyPartId { get; set; }
		public double? TotalDamage { get; set; }
		public bool? IsMoving { get; set; }
		public double? RoomId { get; set; }
		public List<string>? DamageEvents { get; set; }
		public double? PlayerId { get; set; }
		public string? OutfitRepositoryId { get; set; }
		public string? SetPieceId { get; set; }
		public string? SetPieceType { get; set; }
		public bool? OutfitIsHitmanSuit { get; set; }
		public string? KillMethodBroad { get; set; }
		public string? KillMethodStrict { get; set; }
		public double? EvergreenRarity { get; set; }
		public string? KillItemRepositoryId { get; set; }
		//public string? KillItemInstanceId { get; set; }
		public string? KillItemCategory { get; set; }
		public List<DamageHistoryEventValue>? History { get; set; }

		// Imbued
		public bool? ActorHasDisguise { get; set; }
		public EOutfitType? ActorOutfitType { get; set; }
		public bool? IsFemale { get; set; }
		public bool? ActorHasSameOutfit { get; set; }
		public string? ActorOutfitRepositoryId { get; set; }
		public SVector3? ActorPosition { get; set; }
		public SVector3? HeroPosition { get; set; }

		// Self Imbued
		public bool? OutfitIsUnique { get; set; }
		public bool? ActorOutfitIsUnique { get; set; }
	}

	public class KillEventValue : PacifyEventValue {
	}

	public class BodyBaggedEventValue : EventValue {
		public double? ActorId;
		public string? RepositoryId;
		public bool? IsCrowdActor;

		public static BodyBaggedEventValue Load(JsonElement json) {
			return new() {
				ActorId = TryLoadDouble(json, nameof(ActorId)),
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				IsCrowdActor = TryLoadBool(json, nameof(IsCrowdActor)),
			};
		}
	}

	public class InvestigateCuriousEventValue : EventValue {
		public double? ActorId;
		public string? RepositoryId;
		public string? SituationType;
		public string? EventType;
		public double? InvestigationType;

		public static InvestigateCuriousEventValue Load(JsonElement json) {
			return new() {
				ActorId = TryLoadDouble(json, nameof(ActorId)),
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				SituationType = TryLoadString(json, nameof(SituationType)),
				EventType = TryLoadString(json, nameof(EventType)),
				InvestigationType = TryLoadDouble(json, nameof(InvestigationType)),
			};
		}
	}

	public class OpportunityEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public string? RepositoryId;
		public string? Event;

		public static OpportunityEventValue Load(JsonElement json) {
			return new() {
				Location = LocationEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				Event = TryLoadString(json, nameof(Event)),
			};
		}
	}

	public class LevelSetupEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public string? Contract_Name_metricvalue;
		public string? Location_MetricValue;
		public string? Event_metricvalue;

		public static LevelSetupEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				Contract_Name_metricvalue = TryLoadString(json, nameof(Contract_Name_metricvalue)),
				Location_MetricValue = TryLoadString(json, nameof(Location_MetricValue)),
				Event_metricvalue = TryLoadString(json, nameof(Event_metricvalue)),
			};
		}
	}

	public class BodyHiddenEventValue : EventValue {
		public string? RepositoryId;
		public string? ActorName;

		public static BodyHiddenEventValue Load(JsonElement json) {
			return new() {
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				ActorName = TryLoadString(json, nameof(ActorName)),
			};
		}
	}

	public class BodyFoundEventValue : EventValue {
		public string? RepositoryId { get; set; }
		public EDeathContext? DeathContext { get; set; }
		public EDeathType? DeathType { get; set; }

		public static BodyFoundEventValue? Load(JsonElement json) {
			return new() {
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				DeathContext = TryLoad<EDeathContext>(json, nameof(DeathContext)),
				DeathType = TryLoad<EDeathType>(json, nameof(DeathType)),
			};
		}
	}

	public class ExplosionEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static ExplosionEventValue? Load(JsonElement json) {
			return new() {
				Location = LocationEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
			};
		}
	}

	public class CarExplodedEventValue : EventValue {
		public required PlayerEventValue Player;
		public required LocationEventValue Location;

		public Int64? CarSize;
		public SVector3? CarPosition;
		public string? CarArea;

		public static CarExplodedEventValue? Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				CarSize = TryLoadInt64(json, nameof(CarSize)),
				CarPosition = TryLoadSVector3(json, nameof(CarPosition)),
				CarArea = TryLoadString(json, nameof(CarArea)),
			};
		}
	}

	// It was really important that this be its own unique event
	public class CrocodileEventValue : EventValue {
		// We even get a repository ID for the exact crocodile, so cool!
		public string? RepositoryId { get; set; }

		public static CrocodileEventValue? Load(JsonElement json) {
			return new() {
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
			};
		}
	}

	public abstract class BaseItemEventValue : EventValue {
		public required ItemEventValue Item;
		public required PlayerEventValue Player;
		public required LocationEventValue Location;
		public string? RepositoryId;
		//public string? InstanceId { get; set; }
		//public string? ItemType { get; set; }
		//public string? ItemName { get; set; }
	}

	public class ItemDestroyedEventValue : BaseItemEventValue {
		public static ItemDestroyedEventValue Load(JsonElement json) {
			return new() {
				Item = ItemEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				RepositoryId = TryLoadString(json, "RepositoryId") ?? TryLoadString(json, "ItemRepositoryId"),
			};
		}
	}
	public class ItemDroppedEventValue : BaseItemEventValue {
		public static ItemDroppedEventValue Load(JsonElement json) {
			return new() {
				Item = ItemEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				RepositoryId = TryLoadString(json, "RepositoryId") ?? TryLoadString(json, "ItemRepositoryId"),
			};
		}
	}
	public class ItemPickedUpEventValue : BaseItemEventValue {
		public static ItemPickedUpEventValue Load(JsonElement json) {
			return new() {
				Item = ItemEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				RepositoryId = TryLoadString(json, "RepositoryId") ?? TryLoadString(json, "ItemRepositoryId"),
			};
		}
	}
	public class ItemRemovedFromInventoryEventValue : BaseItemEventValue {
		public static ItemRemovedFromInventoryEventValue Load(JsonElement json) {
			return new() {
				Item = ItemEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				RepositoryId = TryLoadString(json, "RepositoryId") ?? TryLoadString(json, "ItemRepositoryId"),
			};
		}
	}
	public class ItemThrownEventValue : BaseItemEventValue {
		public static ItemThrownEventValue Load(JsonElement json) {
			return new() {
				Item = ItemEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				RepositoryId = TryLoadString(json, "RepositoryId") ?? TryLoadString(json, "ItemRepositoryId"),
			};
		}
	}

	public class SetpiecesEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public string? RepositoryId;
		public string? Name;
		public string? Helper;
		public string? Type;
		public string? ToolUsed;
		public string? ItemTriggered;
		public SVector3? Position;

		public static SetpiecesEventValue? Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				Name = TryLoadString(json, nameof(Name)),
				Helper = TryLoadString(json, nameof(Helper)),
				Type = TryLoadString(json, nameof(Type)),
				ToolUsed = TryLoadString(json, nameof(ToolUsed)),
				ItemTriggered = TryLoadString(json, nameof(ItemTriggered)),
				Position = TryLoadSVector3(json, nameof(Position)),
			};
		}
	}

	public class ActorSickEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public required ActorInfoImbuedEventValue Actor;

		public UInt64? ActorID;
		public bool? IsTarget;
		public string? ItemRepositoryId;
		public string? SetpieceRepositoryId;

		public static ActorSickEventValue? Load(JsonElement json) {
			return new() {
				Actor = ActorInfoImbuedEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				ActorID = TryLoadUInt64(json, nameof(ActorID)),
				IsTarget = TryLoadBool(json, nameof(IsTarget)),
				ItemRepositoryId = TryLoadString(json, nameof(ItemRepositoryId)),
				SetpieceRepositoryId = TryLoadString(json, nameof(SetpieceRepositoryId)),
			};
		}
	}

	public class AgilityStartEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static AgilityStartEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class PlayerShotEventValue : EventValue {
		public required ItemEventValue Weapon;
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static PlayerShotEventValue Load(JsonElement json) {
			return new() {
				Weapon = ItemEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
			};
		}
	}

	public class DartHitEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public string? RepositoryId { get; set; }
		public EActorType? ActorType { get; set; }
		public bool? IsTarget { get; set; }
		public bool? Blind { get; set; }
		public bool? Sedative { get; set; }
		public bool? Sick { get; set; }

		public static DartHitEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				ActorType = TryLoad<EActorType>(json, nameof(ActorType)),
				IsTarget = TryLoadBool(json, nameof(IsTarget)),
				Blind = TryLoadBool(json, nameof(Blind)),
				Sedative = TryLoadBool(json, nameof(Sedative)),
				Sick = TryLoadBool(json, nameof(Sick)),
			};
		}
	}

	public class TrespassingEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static TrespassingEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class DisguiseEventValue : EventValue {
		public required PlayerEventValue Player;
		public required LocationEventValue Location;

		public string? RepositoryId { get; set; }
		public string? Title { get; set; }
		public EActorType? ActorType { get; set; }
		public bool? IsSuit { get; set; }
		public EOutfitType? OutfitType { get; set; }

		// Imbued
		public bool? IsBundle { get; set; }

		// Self Imbued
		public bool? IsUnique { get; set; }

		public static DisguiseEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				Title = TryLoadString(json, nameof(Title)),
				ActorType = TryLoad<EActorType>(json, nameof(ActorType)),
				IsSuit = TryLoadBool(json, nameof(IsSuit)),
				OutfitType = TryLoad<EOutfitType>(json, nameof(OutfitType)),
				IsBundle = TryLoadBool(json, nameof(IsBundle)),
			};
		}
	}

	public class StartingSuitEventValue : DisguiseEventValue {
	}

	public class SecuritySystemRecorderEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public SecuritySystemRecorderEvent? Event;
		public double? Camera;
		public double? Recorder;

		public static SecuritySystemRecorderEventValue Load(JsonElement json) {
			return new() {
				Location = LocationEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Event = TryLoad<SecuritySystemRecorderEvent>(json, nameof(Event)),
				Camera = TryLoadDouble(json, nameof(Camera)),
				Recorder = TryLoadDouble(json, nameof(Recorder)),
			};
		}
	}

	public class OnTurnOnEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public UInt64? EntityID;
		public string? RepositoryId;

		public static OnTurnOnEventValue Load(JsonElement json) {
			return new() {
				EntityID = TryLoadUInt64(json, nameof(EntityID)),
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnTurnOffEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public UInt64? EntityID;
		public string? RepositoryId;

		public static OnTurnOffEventValue Load(JsonElement json) {
			return new() {
				EntityID = TryLoadUInt64(json, nameof(EntityID)),
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnAttachToHitmanEventValue : EventValue {
		public required ItemEventValue Item;
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public string? RepositoryId;

		public static OnAttachToHitmanEventValue Load(JsonElement json) {
			return new() {
				Item = ItemEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				RepositoryId = TryLoadString(json, "ItemRepositoryId"),
			};
		}
	}

	public class OnBrokenEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public UInt64? EntityID { get; set; }

		public static OnBrokenEventValue Load(JsonElement json) {
			return new() {
				EntityID = TryLoadUInt64(json, nameof(EntityID)),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnDestroyEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public UInt64? EntityID { get; set; }
		public bool? InitialStateOn { get; set; }

		public static OnDestroyEventValue Load(JsonElement json) {
			return new() {
				EntityID = TryLoadUInt64(json, nameof(EntityID)),
				InitialStateOn = TryLoadBool(json, nameof(InitialStateOn)),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnDestroyedEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public UInt64? EntityID { get; set; }

		public static OnDestroyedEventValue Load(JsonElement json) {
			return new() {
				EntityID = TryLoadUInt64(json, nameof(EntityID)),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnEvacuationStartedEventValue : EventValue {
		public required LocationEventValue Location;
		public required ActorInfoImbuedEventValue Actor;
		public required PlayerEventValue Player;

		public static OnEvacuationStartedEventValue Load(JsonElement json) {
			return new() {
				Location = LocationEventValue.Load(json),
				Actor = ActorInfoImbuedEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
			};
		}
	}

	public class OnIsFullyInCrowdEventValue : EventValue {
		public required LocationEventValue Location { get; set; }
		public required PlayerEventValue Player { get; set; }

		public static OnIsFullyInCrowdEventValue? Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnIsFullyInVegetationEventValue : EventValue {
		public required LocationEventValue Location { get; set; }
		public required PlayerEventValue Player { get; set; }

		public static OnIsFullyInVegetationEventValue? Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnPickupEventValue : EventValue {
		public required LocationEventValue Location { get; set; }
		public required ItemEventValue Item { get; set; }
		public required PlayerEventValue Player { get; set; }
		public string? RepositoryId;

		public static OnPickupEventValue? Load(JsonElement json) {
			return new() {
				Item = ItemEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
				RepositoryId = TryLoadString(json, "ItemRepositoryId"),
			};
		}
	}

	public class OnPutInContainerEventValue : EventValue {
		public required LocationEventValue Location { get; set; }
		public required ItemEventValue Item { get; set; }
		public required PlayerEventValue Player { get; set; }

		public static OnPutInContainerEventValue? Load(JsonElement json) {
			return new() {
				Item = ItemEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnInitialFractureEventValue : EventValue {
		public required LocationEventValue Location { get; set; }
		public required PlayerEventValue Player { get; set; }
		public UInt64? EntityID { get; set; }

		public static OnInitialFractureEventValue Load(JsonElement json) {
			return new() {
				Location = LocationEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				EntityID = TryLoadUInt64(json, nameof(EntityID)),
			};
		}
	}

	public class OpenDoorEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public required string? Type;

		public static OpenDoorEventValue Load(JsonElement json) {
			var typeProp = json.GetProperty(nameof(Type));
			return new() {
				Type = typeProp.GetString(),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnTakeDamageEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static OnTakeDamageEventValue? Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class OnWeaponReloadEventValue : EventValue {
		public required LocationEventValue Location;
		public required ItemEventValue Item;
		public required PlayerEventValue Player;

		public static OnWeaponReloadEventValue Load(JsonElement json) {
			return new() {
				Item = ItemEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class InstinctActiveEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static InstinctActiveEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class ProjectileBodyShotEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static ProjectileBodyShotEventValue? Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class DrainPipeClimbedEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static DrainPipeClimbedEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class ItemStashedEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;
		public UInt64? ActorId;
		public string? ActorName;
		public string? ItemId;
		public string? ItemTypeId;
		public string? RepositoryId;

		public static ItemStashedEventValue Load(JsonElement json) {
			return new() {
				Location = LocationEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				ActorId = TryLoadUInt64(json, nameof(ActorId)),
				ActorName = TryLoadString(json, nameof(ActorName)),
				ItemId = TryLoadString(json, nameof(ItemId)),
				ItemTypeId = TryLoadString(json, nameof(ItemTypeId)),
				RepositoryId = TryLoadString(json, nameof(RepositoryId)),
			};
		}
	}

	public class EnterRoomEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static EnterRoomEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class FriskedSuccessEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static FriskedSuccessEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class EnterAreaEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static EnterAreaEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}

	}

	public class MovementEventValue : EventValue {
		public required LocationEventValue Location;
		public required PlayerEventValue Player;

		public static MovementEventValue Load(JsonElement json) {
			return new() {
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}

	public class DragBodyMoveEventValue : EventValue {
		public required ActorInfoImbuedEventValue Actor { get; set; }
		public required PlayerEventValue Player { get; set; }
		public required LocationEventValue Location { get; set; }

		public static DragBodyMoveEventValue Load(JsonElement json) {
			return new() {
				Actor = ActorInfoImbuedEventValue.Load(json),
				Player = PlayerEventValue.Load(json),
				Location = LocationEventValue.Load(json),
			};
		}
	}
}
