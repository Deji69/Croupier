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
	}

	public class ActorInfoImbuedEventValue : EventValue {
		public string? ActorName { get; set; }
		public string? ActorRepositoryId { get; set; }
		public string? ActorOutfitRepositoryId { get; set; }
		public EActorType? ActorType { get; set; }
		public int? ActorWeaponIndex { get; set; }
		public bool? ActorHasDisguise { get; set; }
		public bool? ActorIsAuthorityFigure { get; set; }
		public bool? ActorIsDead { get; set; }
		public bool? ActorIsFemale { get; set; }
		public bool? ActorIsPacified { get; set; }
		public bool? ActorIsTarget { get; set; }
		public bool? ActorOutfitAllowsWeapons { get; set; }
		public bool? ActorWeaponUnholstered { get; set; }
	}

	public class ItemInfoImbuedEventValue : EventValue {
		public string? ItemName { get; set; }
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
	}

	public class LocationImbuedEventValue : EventValue {
		public int? Room { get; set; }
		public string? Area { get; set; }
		public string? ActorArea { get; set; }
		public SVector3? ActorPosition { get; set; }
		public int? ActorRoom { get; set; }
		public string? HeroArea { get; set; }
		public SVector3? HeroPosition { get; set; }
		public int? HeroRoom { get; set; }
		public string? ItemArea { get; set; }
		public SVector3? ItemPosition { get; set; }
		public int? ItemRoom { get; set; }
		public bool? IsCrouching { get; set; }
		public bool? IsIdle { get; set; }
		public bool? IsRunning { get; set; }
		public bool? IsWalking { get; set; }
		public bool? IsTrespassing { get; set; }
		public SVector3? Position { get; set; }
	}

	public class DoorUnlockedEventValue : LocationImbuedEventValue {
	}

	public class DoorBrokenEventValue : LocationImbuedEventValue {
	}

	public class LoadoutItemEventValue {
		public required string RepositoryId { get; set; }
		public required string InstanceId { get; set; }
		public required List<string> OnlineTraits { get; set; }
	}

	public class DamageHistoryEventValue : EventValue {
		public required bool Explosive { get; set; }
		public required bool Headshot { get; set; }
		public required bool Accident { get; set; }
		public required bool WeaponSilenced { get; set; }
		public required bool Projectile { get; set; }
		public required bool Sniper { get; set; }
		public required bool ThroughWall { get; set; }
		public required string InstanceId { get; set; }
		public required string RepositoryId { get; set; }
		public required int BodyPartId { get; set; }
		public required int TotalDamage { get; set; }
	}

	public class PacifyEventValue : EventValue {
		public required string RepositoryId { get; set; }
		public required string ActorName { get; set; }
		public required EActorType ActorType { get; set; }
		public required EKillType KillType { get; set; }
		public required EDeathContext KillContext { get; set; }
		public required string KillClass { get; set; }
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

	public class TakedownCleannessEventValue : EventValue {
		public required string RepositoryId { get; set; }
		public bool IsTarget { get; set; } = false;
	}

	public class ActorIdentityEventValue : EventValue {
		public required double ActorId { get; set; }
		public required string RepositoryId { get; set; }
		public bool? IsCrowdActor { get; set; }
	}

	public class BodyBaggedEventValue : ActorIdentityEventValue {

	}

	public class InvestigateCuriousEventValue : EventValue {
		public required double ActorId { get; set; }
		public required string RepositoryId { get; set; }
		public required string SituationType { get; set; }
		public required string EventType { get; set; }
		public required double InvestigationType { get; set; }
	}

	public class OpportunityEventValue : EventValue {
		public required string RepositoryId { get; set; }
		public required string Event { get; set; }
	}

	public class LevelSetupEventValue : EventValue {
		public required string Contract_Name_metricvalue { get; set; }
		public required string Location_MetricValue { get; set; }
		public required string Event_metricvalue { get; set; }
	}

	public class BodyHiddenEventValue : EventValue {
		public string? RepositoryId { get; set; }
		public string? ActorName { get; set; }
	}

	public class BodyEventValue : EventValue {
		public required string RepositoryId { get; set; }
		public bool? IsCrowdActor { get; set; }
	}

	public class BodyKillInfoEventValue : EventValue {
		public string? RepositoryId { get; set; }
		public EDeathContext? DeathContext { get; set; }
		public EDeathType? DeathType { get; set; }
	}

	public class BodyFoundEventValue : BodyKillInfoEventValue {
	}

	public class ExplosionEventValue : LocationImbuedEventValue {
	}

	public class CarExplodedEventValue : EventValue {
		public int CarSize { get; set; }
		public SVector3? CarPosition { get; set; }
		public string? CarArea { get; set; }
	}

	public abstract class ItemEventValue : LocationImbuedEventValue {
		public string? RepositoryId { get; set; }
		public string? InstanceId { get; set; }
		public string? ItemType { get; set; }
		public string? ItemName { get; set; }
	}

	public class ItemDestroyedEventValue : ItemEventValue { }
	public class ItemDroppedEventValue : ItemEventValue { }
	public class ItemPickedUpEventValue : ItemEventValue { }
	public class ItemRemovedFromInventoryEventValue : ItemEventValue { }
	public class ItemThrownEventValue : ItemEventValue { }

	public class SetpiecesEventValue : EventValue {
		public required string RepositoryId { get; set; }
		[JsonPropertyName("name_metricvalue")]
		public string? Name { get; set; }
		[JsonPropertyName("setpieceHelper_metricvalue")]
		public string? Helper { get; set; }
		[JsonPropertyName("setpieceType_metricvalue")]
		public string? Type { get; set; }
		[JsonPropertyName("toolUsed_metricvalue")]
		public string? ToolUsed { get; set; }
		[JsonPropertyName("Item_triggered_metricvalue")]
		public string? ItemTriggered { get; set; }
		public string? Position { get; set; }
	}

	public class ActorSickEventValue : EventValue {
		//public required SVector3 ActorPosition { get; set; }
		//public required uint ActorId { get; set; }
		public required string ActorName { get; set; }
		[JsonPropertyName("actor_R_ID")]
		public required string ActorRepositoryId { get; set; }
		public required bool IsTarget { get; set; }
		[JsonPropertyName("item_R_ID")]
		public required string ItemRepositoryId { get; set; }
		[JsonPropertyName("setpiece_R_ID")]
		public required string SetpieceRepositoryId { get; set; }
		public required EActorType ActorType { get; set; }
	}

	public class AgilityStartEventValue : EventValue {
		public required LocationImbuedEventValue Location;

		public static AgilityStartEventValue? Load(JsonElement json) {
			var location = json.Deserialize<LocationImbuedEventValue>();
			return location != null ? new() {
				Location = location,
			} : null;
		}
	}

	public class PlayerShotEventValue : EventValue {
		public required ItemInfoImbuedEventValue Weapon;
		public required LocationImbuedEventValue Location;

		public static PlayerShotEventValue? Load(JsonElement json) {
			var weapon = json.Deserialize<ItemInfoImbuedEventValue>();
			var location = json.Deserialize<LocationImbuedEventValue>();
			return weapon != null && location != null ? new() {
				Weapon = weapon,
				Location = location,
			} : null;
		}
	}

	public class DartHitEventValue : LocationImbuedEventValue {
		public required string RepositoryId { get; set; }
		public required EActorType ActorType { get; set; }
		public required bool IsTarget { get; set; }
		public bool? Blind { get; set; }
		public bool? Sedative { get; set; }
		public bool? Sick { get; set; }
	}

	public class TrespassingEventValue : LocationImbuedEventValue { }

	public class DisguiseEventValue : LocationImbuedEventValue {
		public required string RepositoryId { get; set; }
		public string? Title { get; set; }
		public EActorType? ActorType { get; set; }
		public bool? IsSuit { get; set; }
		public EOutfitType? OutfitType { get; set; }

		// Imbued
		public bool? IsBundle { get; set; }

		// Self Imbued
		public bool? IsUnique { get; set; }
	}

	public class StartingSuitEventValue : DisguiseEventValue {
	}

	public class SecuritySystemRecorderEventValue : EventValue {
		[JsonPropertyName("event")]
		public required string Event { get; set; }
		[JsonPropertyName("camera")]
		public double? Camera { get; set; }
		[JsonPropertyName("recorder")]
		public double? Recorder { get; set; }
	}

	public class OnTurnOnEventValue : LocationImbuedEventValue {
		public UInt64? EntityID { get; set; }
		public string? RepositoryId { get; set; }
		public bool? InitialStateOn { get; set; }
	}

	public class OnTurnOffEventValue : LocationImbuedEventValue {
		public UInt64? EntityID { get; set; }
		public string? RepositoryId { get; set; }
		public bool? InitialStateOn { get; set; }
	}

	public class OnDestroyEventValue : LocationImbuedEventValue {
		public UInt64? EntityID { get; set; }
		public bool? InitialStateOn { get; set; }
	}

	public class OnEvacuationStartedEventValue : LocationImbuedEventValue {
		public string? ActorName { get; set; }
		public bool? IsTarget { get; set; }
	}

	public class OnIsFullyInCrowdEventValue : EventValue {
	}

	public class OnIsFullyInVegetationEventValue : EventValue {
	}

	public class OnPickupEventValue : EventValue {
		public required LocationImbuedEventValue Location { get; set; }
		public required ItemInfoImbuedEventValue Item { get; set; }

		public static OnPickupEventValue? Load(JsonElement json) {
			var item = json.Deserialize<ItemInfoImbuedEventValue>();
			var location = json.Deserialize<LocationImbuedEventValue>();
			return item != null && location != null ? new() {
				Item = item,
				Location = location,
			} : null;
		}
	}

	public class OpenDoorEventValue : LocationImbuedEventValue {
		public string? Type { get; set; }
	}

	public class OnTakeDamageEventValue : EventValue {
	}

	public class OnWeaponReloadEventValue : EventValue {
		public required LocationImbuedEventValue Location;
		public required ItemInfoImbuedEventValue Item;

		public static OnWeaponReloadEventValue? Load(JsonElement json) {
			var item = json.Deserialize<ItemInfoImbuedEventValue>();
			var location = json.Deserialize<LocationImbuedEventValue>();
			return item != null && location != null ? new() {
				Item = item,
				Location = location,
			} : null;
		}
	}

	public class InstinctActiveEventValue : LocationImbuedEventValue {
	}

	public class ProjectileBodyShotEventValue : EventValue {
	}

	public class DrainPipeClimbedEventValue : EventValue {
	}

	public class ItemStashedEventValue : EventValue {
	}

	public class EnterRoomEventValue : LocationImbuedEventValue {
	}

	public class FriskedSuccessEventValue : LocationImbuedEventValue {
	}

	public class EnterAreaEventValue : LocationImbuedEventValue {
	}

	public class MovementEventValue : LocationImbuedEventValue {
	}

	public class DragBodyMoveEventValue : EventValue {
		public required ActorInfoImbuedEventValue Actor { get; set; }
		public required LocationImbuedEventValue Location { get; set; }

		public static DragBodyMoveEventValue? Load(JsonElement json) {
			var actor = json.Deserialize<ActorInfoImbuedEventValue>();
			var location = json.Deserialize<LocationImbuedEventValue>();
			return actor != null && location != null ? new() {
				Actor = actor,
				Location = location,
			} : null;
		}
	}
}
