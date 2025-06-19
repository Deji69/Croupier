using System;
using System.Collections.Generic;

namespace Croupier.GameEvents {
	public class SVector3 {
		public required double x { get; set; }
		public required double y { get; set; }
		public required double z { get; set; }
	}

	public enum EActorType {
		eAT_Civilian = 0,
		eAT_Guard    = 1,
		eAT_Hitman   = 2,
		eAT_Last     = 3,
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

	public class Event {
		public required string Name { get; set; }
		public required double Timestamp { get; set; }
		public object? Value { get; set; }
	}

	public class EventValue {

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
		public required uint ActorId { get; set; }
		public required string ActorName { get; set; }
		public required EActorType ActorType { get; set; }
		public required EKillType KillType { get; set; }
		public required EDeathContext KillContext { get; set; }
		public required string KillClass { get; set; }
		public required bool Accident { get; set; }
		public required bool WeaponSilenced { get; set; }
		public required bool Explosive { get; set; }
		public required int ExplosionType { get; set; }
		public required bool Projectile { get; set; }
		public required bool Sniper { get; set; }
		public required bool IsHeadshot { get; set; }
		public required bool IsTarget { get; set; }
		public required bool ThroughWall { get; set; }
		public required int BodyPartId { get; set; }
		public required double TotalDamage { get; set; }
		public required bool IsMoving { get; set; }
		public required int RoomId { get; set; }
		public required string ActorPosition { get; set; }
		public required string HeroPosition { get; set; }
		public required List<string> DamageEvents { get; set; }
		public required int PlayerId { get; set; }
		public required string OutfitRepositoryId { get; set; }
		public required string SetPieceId { get; set; }
		public required string SetPieceType { get; set; }
		public required bool OutfitIsHitmanSuit { get; set; }
		public required string KillMethodBroad { get; set; }
		public required string KillMethodStrict { get; set; }
		public required int EvergreenRarity { get; set; }
		public required List<DamageHistoryEventValue> History { get; set; }
	}

	public class KillEventValue : PacifyEventValue {
		public required string KillItemRepositoryId { get; set; }
		public required string KillItemInstanceId { get; set; }
		public required string KillItemCategory { get; set; }
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
		public required uint ActorId { get; set; }
		public required string RepositoryId { get; set; }
		public required bool IsCrowdActor { get; set; }
	}

	public class InvestigateCuriousEventValue : EventValue {
		public required uint ActorId { get; set; }
		public required string RepositoryId { get; set; }
		public required string SituationType { get; set; }
		public required string EventType { get; set; }
		public required uint InvestigationType { get; set; }
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

	public class BodyEventValue : EventValue {
		public required string RepositoryId { get; set; }
		public required bool IsCrowdActor { get; set; }
	}

	public class BodyKillInfoEventValue : EventValue {
		public required EDeathContext DeathContext { get; set; }
		public required EDeathType DeathType { get; set; }
	}

	public abstract class ItemEventValue : EventValue {
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
		public required SVector3 Position { get; set; }
	}

	public class ActorSickEventValue : EventValue {
		public required SVector3 ActorPosition { get; set; }
		public required uint ActorId { get; set; }
		public required string ActorName { get; set; }
		public required string actor_R_ID { get; set; }
		public required bool IsTarget { get; set; }
		public required string item_R_ID { get; set; }
		public required string setpiece_R_ID { get; set; }
		public required EActorType ActorType { get; set; }
	}

	public class DartHitEventValue : EventValue {
		public required string RepositoryId { get; set; }
		public required EActorType ActorType { get; set; }
		public required bool IsTarget { get; set; }
		public required bool Blind { get; set; }
		public required bool Sedative { get; set; }
		public required bool Sick { get; set; }
	}

	public class TrespassingEventValue : EventValue {
		public required bool IsTrespassing { get; set; }
		public required int RoomId { get; set; }
	}

	public class SecuritySystemRecorderEventValue : EventValue {
		public required string event_ { get; set; }
		public required uint camera { get; set; }
		public required uint recorder { get; set; }
	}
}
