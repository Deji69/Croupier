using System;
using System.Collections.Generic;
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
	};
	public enum EOutfitType {
		eOT_None      = 0,
		eOT_Suit      = 1,
		eOT_Guard     = 2,
		eOT_Worker    = 3,
		eOT_Waiter    = 4,
		eOT_LucasGrey = 5,
	};
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

	public enum SecuritySystemRecorderEvent {
		Undefined,
		Spotted,
		Erased,
		Destroyed,
		CameraDestroyed,
	};

	public class Event {
		public required string Name { get; set; }
		public double Timestamp { get; set; }
		public object? Value { get; set; }
	}

	public class EventValue {

	}

	public class LocationImbuedEventValue : EventValue {
		public int? Room { get; set; }
		public string? Area { get; set; }
		public SVector3? Position { get; set; }
	}

	public class DoorUnlockedEventValue : EventValue {

	}

	public class DoorBrokenEventValue : EventValue {

	}

	public class LoadoutItemEventValue {
		public required string RepositoryId { get; set; }
		public required string InstanceId { get; set; }
		public required List<string> OnlineTraits { get; set; }
	};

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

	public class StringEventValue(string val) : EventValue {
		public string Value { get; set; } = val;
	}

	public class StringArrayEventValue(List<string> val) : EventValue {
		public List<string> Value { get; set; } = val;
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
		public required bool IsCrowdActor { get; set; }
	}

	public class BodyKillInfoEventValue : EventValue {
		public string? RepositoryId { get; set; }
		public EDeathContext? DeathContext { get; set; }
		public EDeathType? DeathType { get; set; }
	}

	public class BodyFoundEventValue : EventValue {
		public required BodyKillInfoEventValue DeadBody { get; set; }
	}

	public abstract class ItemEventValue : LocationImbuedEventValue {
		public required string RepositoryId { get; set; }
		public required string InstanceId { get; set; }
		public required string ItemType { get; set; }
		public required string ItemName { get; set; }
	}

	public class ItemPickedUpEventValue : ItemEventValue { }
	public class ItemDroppedEventValue : ItemEventValue { }
	public class ItemThrownEventValue : ItemEventValue { }
	public class ItemRemovedFromInventoryEventValue : ItemEventValue { }

	public class SetpiecesEventValue : EventValue {
		public required string RepositoryId { get; set; }
		public required string name_metricvalue { get; set; }
		public required string setpieceHelper_metricvalue { get; set; }
		public required string setpieceType_metricvalue { get; set; }
		public required string toolUsed_metricvalue { get; set; }
		public required string Item_triggered_metricvalue { get; set; }
		public required string Position { get; set; }
	}

	public class ActorSickEventValue : EventValue {
		//public required SVector3 ActorPosition { get; set; }
		//public required uint ActorId { get; set; }
		public required string ActorName { get; set; }
		public required string actor_R_ID { get; set; }
		public required bool IsTarget { get; set; }
		public required string item_R_ID { get; set; }
		public required string setpiece_R_ID { get; set; }
		public required EActorType ActorType { get; set; }
	}

	public class PlayerShotEventValue : LocationImbuedEventValue {
	}

	public class DartHitEventValue : EventValue {
		public required string RepositoryId { get; set; }
		public required EActorType ActorType { get; set; }
		public required bool IsTarget { get; set; }
		public bool? Blind { get; set; }
		public bool? Sedative { get; set; }
		public bool? Sick { get; set; }
	}

	public class TrespassingEventValue : LocationImbuedEventValue {
		public required bool IsTrespassing { get; set; }
	}

	public class DisguiseEventValue : LocationImbuedEventValue {
		public required string RepositoryId { get; set; }
		public string? Title { get; set; }
		public EActorType? ActorType { get; set; }
		public bool? IsSuit { get; set; }
		public EOutfitType? OutfitType { get; set; }

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

	public class OnIsFullyInCrowdEventValue : EventValue {

	}

	public class OnIsFullyInVegetationEventValue : EventValue {

	}

	public class OnTakeDamageEventValue : EventValue {

	}

	public class OnWeaponReloadEventValue : LocationImbuedEventValue {

	}

	public class InstinctActiveEventValue : EventValue {

	}

	public class ProjectileBodyShotEventValue : EventValue {

	}

	public class IsCrouchWalkingSlowlyEventValue : EventValue {

	}

	public class IsCrouchWalkingEventValue : EventValue {

	}

	public class IsCrouchRunningEventValue : EventValue {

	}

	public class IsRunningEventValue : EventValue {

	}

	public class DrainPipeClimbedEventValue : EventValue {

	}

	public class ItemStashedEventValue : EventValue {

	}

	public class EnterRoomEventValue : EventValue {
		public required int Room { get; set; }
	}

	public class EnterAreaEventValue : EventValue {
		public required string Area { get; set; }
	}
}
