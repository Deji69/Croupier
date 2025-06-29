using Croupier.Exceptions;
using Croupier.GameEvents;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Croupier {
	public abstract class BingoTriggerExpression {
		public abstract bool Test(object? value, BingoTileState state);
	}

	public class BingoTriggerUnique : BingoTriggerExpression {
		private string? propName;

		public override bool Test(object? value, BingoTileState state) {
			if (propName == null) return true;
			state.History ??= new List<object?>();
			var history = (state.History as List<object?>);
			if (history == null) return false;
			if (value == null) return false;

			var prop = value.GetType().GetProperty(propName);
			if (prop == null) return false;
			var propVal = prop.GetValue(value);
			var exists = history.Contains(propVal);
			if (exists) return false;
			history.Add(propVal);
			return true;
		}

		public void Load(JsonNode? prop) {
			propName = prop?.GetValueKind() switch {
				JsonValueKind.String => prop.GetValue<string>(),
				null => null,
				_ => throw new BingoTileConfigException("Expected string.")
			};
		}
	}

	public class BingoTriggerBool : BingoTriggerExpression {
		private bool? value;

		public override bool Test(object? value, BingoTileState state) {
			return this.value == null || (value is bool v && this.value == v);
		}

		public void Load(JsonNode? prop) {
			value = prop?.GetValueKind() switch {
				JsonValueKind.True => true,
				JsonValueKind.False => false,
				null => null,
				_ => throw new BingoTileConfigException("Expected true or false.")
			};
		}
	}

	public class BingoTriggerEnum<TEnum> : BingoTriggerExpression where TEnum : struct, Enum {
		private List<TEnum>? values = null;

		public override bool Test(object? value, BingoTileState state) {
			return values == null || (value is TEnum v && values.Contains(v));
		}

		public void Load(JsonNode? prop, Func<string, TEnum>? converter = null) {
			converter ??= new(str => (TEnum)Enum.Parse(typeof(TEnum), str));
			values = prop?.GetValueKind() switch {
				JsonValueKind.String => [converter(prop.GetValue<string>())],
				JsonValueKind.Array => LoadEnumArray(prop, converter),
				null => null,
				_ => throw new BingoTileConfigException("Expected string or string array.")
			};
		}

		public static List<TEnum> LoadEnumArray(JsonNode prop, Func<string, TEnum> converter) {
			if (prop.GetValueKind() != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new List<TEnum>();
			foreach (var item in prop.AsArray()) {
				if (item == null) continue;
				coll.Add(converter(item.GetValue<string>()));
			}
			return coll;
		}
	}

	public class BingoTriggerInt : BingoTriggerExpression {
		private List<Int64>? values;

		public override bool Test(object? value, BingoTileState state) {
			return values == null || (
				TryConvertValue(value, out var v)
				&& v != null
				&& values.Contains((long)v)
			);
		}

		public void Load(JsonNode? prop) {
			values = prop?.GetValueKind() switch {
				JsonValueKind.String => [Int64.Parse(prop.GetValue<string>())],
				JsonValueKind.Number => [prop.GetValue<Int64>()],
				JsonValueKind.Array => LoadIntArray(prop),
				null => null,
				_ => throw new BingoTileConfigException("Expected number or numeric array.")
			};
		}

		public static bool TryConvertValue(object? value, out Int64? result) {
			if (value != null) {
				try {
					result = Convert.ToInt64(value);
					return true;
				} catch { }
			}
			result = null;
			return false;
		}

		public static List<Int64> LoadIntArray(JsonNode prop) {
			if (prop.GetValueKind() != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new List<Int64>();
			foreach (var item in prop.AsArray()) {
				if (item == null) continue;
				coll.Add(item.GetValueKind() switch {
					JsonValueKind.String => Int64.Parse(item.GetValue<string>()),
					JsonValueKind.Number => item.GetValue<Int64>(),
					_ => throw new BingoTileConfigException("Expected number.")
				});
			}
			return coll;
		}
	}

	public class BingoTriggerString : BingoTriggerExpression {
		private StringCollection? values;

		public override bool Test(object? value, BingoTileState state) {
			return values == null || values.Count == 0 || (
				value is string v
				&& values.Contains(v)
			);
		}

		public void Load(JsonNode? prop) {
			values = prop?.GetValueKind() switch {
				JsonValueKind.String => [prop.GetValue<string>()],
				JsonValueKind.Array => LoadStringArray(prop),
				null => null,
				_ => throw new BingoTileConfigException("Expected string or string array.")
			};
		}

		public static StringCollection LoadStringArray(JsonNode prop) {
			if (prop.GetValueKind() != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new StringCollection();
			foreach (var item in prop.AsArray()) {
				if (item == null) continue;
				coll.Add(item.GetValue<string>());
			}
			return coll;
		}
	}

	public class BingoTriggerArray<T>(JsonValueKind jsonKind) : BingoTriggerExpression {
		private JsonValueKind jsonKind = jsonKind;
		private List<T>? values;

		public override bool Test(object? value, BingoTileState state) {
			return values == null || values.Count == 0 || (
				value is List<T> v
				&& values.All(val => v.Contains(val))
			);
		}

		public void Load(JsonNode? prop) {
			values = prop?.GetValueKind() switch {
				null => null,
				JsonValueKind.Array => LoadArray(prop),
				_ => [LoadNodeValue(prop)],
			};
		}

		public List<T> LoadArray(JsonNode prop) {
			if (prop.GetValueKind() != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new List<T>();
			foreach (var item in prop.AsArray()) {
				if (item == null) continue;
				coll.Add(LoadNodeValue(item));
			}
			return coll;
		}

		private T LoadNodeValue(JsonNode prop) {
			var kind = prop.GetValueKind();
			if (kind == jsonKind) return prop.GetValue<T>();
			throw new BingoTileConfigException("Invalid value type.");
		}
	}

	public class BingoTrigger {
		public int? Count { get; set; }

		protected BingoTrigger() {}
		protected BingoTrigger(JsonNode json) {
			if (json.GetValueKind() == JsonValueKind.Object)
				Count = json["Count"]?.GetValue<int>();
		}

		public virtual bool Test(EventValue ev, BingoTileState state) {
			return true;
		}

		public virtual void Advance(BingoTileState state) {
			if (Count == null) state.Complete = true;
			else state.Complete = ++state.Counter >= Count;
		}

		public virtual object[] GetFormatArgs(BingoTileState state) {
			return [state.Counter, (Count ?? 1) - (state.Complete ? 0 : state.Counter)];
		}

		public static BingoTrigger? Load(JsonNode json, Func<JsonNode, BingoTrigger?> create) {
			var kind = json.GetValueKind();

			if (kind == JsonValueKind.Object) {
				var triggers = new List<BingoTrigger>();
				var NOT = json["NOT"];
				if (NOT != null) {
					var t = Load(NOT, json => create(json));
					if (t != null)
						triggers.Add(new BingoTriggerNOT(t));
				}
				var OR = json["OR"];
				if (OR != null) {
					if (OR.GetValueKind() != JsonValueKind.Array)
						throw new BingoConfigException("Expected array for 'OR' trigger group.");
					var orTriggers = new List<BingoTrigger?>();
					foreach (var j in OR.AsArray()) {
						if (j == null) continue;
						var t = Load(j, json => create(json));
						if (t != null)
							orTriggers.Add(t);
					}
					triggers.Add(new BingoTriggerOR(orTriggers));
				}
				if (triggers.Count > 0)
					return new BingoTriggerAND([create(json), ..triggers]);
			}

			return create(json);
		}

		public static BingoTrigger? FromJson(BingoTileType type, JsonNode json) {
			var trigger = FromJson_Trigger(type, json);
			if (trigger != null && type == BingoTileType.Complication)
				return new BingoTriggerComplication(trigger);
			return trigger;
		}

		private static BingoTrigger? FromJson_Trigger(BingoTileType type, JsonNode json) {
			if (json["Kill"] != null)
				return Load(json["Kill"]!, json => new BingoTriggerKill(json.AsObject()));
			if (json["Pacify"] != null)
				return Load(json["Pacify"]!, json => new BingoTriggerPacify(json.AsObject()));
			if (json["ActorSick"] != null)
				return Load(json["ActorSick"]!, json => new BingoTriggerActorSick(json.AsObject()));
			if (json["Setpiece"] != null)
				return Load(json["Setpiece"]!, json => new BingoTriggerSetpiece(json.AsObject()));
			if (json["Collect"] != null)
				return Load(json["Collect"]!, json => new BingoTriggerCollect(json.AsObject()));
			if (json["Disguise"] != null)
				return Load(json["Disguise"]!, json => new BingoTriggerDisguise(json));
			if (json["StartingSuit"] != null)
				return Load(json["StartingSuit"]!, json => new BingoTriggerStartingSuit(json));
			if (json["LevelSetupEvent"] != null)
				return Load(json["LevelSetupEvent"]!, json => new BingoTriggerLevelSetupEvent(json.AsObject()));
			if (json["BodyHidden"] != null)
				return Load(json["BodyHidden"]!, json => new BingoTriggerBodyHidden(json.AsObject()));
			if (json["SecuritySystemRecorder"] != null)
				return Load(json["SecuritySystemRecorder"]!, json => new BingoTriggerSecuritySystemRecorder(json));
			if (json["ItemStashed"] != null)
				return Load(json["ItemStashed"]!, json => new BingoTriggerItemStashed(json));
			if (json["ShotFired"] != null)
				return Load(json["ShotFired"]!, json => new BingoTriggerShotFired(json));
			if (json["DartHit"] != null)
				return Load(json["DartHit"]!, json => new BingoTriggerDartHit(json.AsObject()));
			if (json["DoorBroken"] != null)
				return Load(json["DoorBroken"]!, json => new BingoTriggerDoorBroken(json));
			if (json["DoorUnlocked"] != null)
				return Load(json["DoorUnlocked"]!, json => new BingoTriggerDoorUnlocked(json));
			if (json["ItemThrown"] != null)
				return Load(json["ItemThrown"]!, json => new BingoTriggerItemThrown(json));
			if (json["ItemDropped"] != null)
				return Load(json["ItemDropped"]!, json => new BingoTriggerItemDropped(json));
			if (json["OnIsFullyInCrowd"] != null)
				return Load(json["OnIsFullyInCrowd"]!, json => new BingoTriggerOnIsFullyInCrowd(json));
			if (json["OnIsFullyInVegetation"] != null)
				return Load(json["OnIsFullyInVegetation"]!, json => new BingoTriggerOnIsFullyInVegetation(json));
			if (json["OnTakeDamage"] != null)
				return Load(json["OnTakeDamage"]!, json => new BingoTriggerOnTakeDamage(json));
			if (json["InstinctActive"] != null)
				return Load(json["InstinctActive"]!, json => new BingoTriggerInstinctActive(json));
			if (json["IsCrouchWalkingSlowly"] != null)
				return Load(json["IsCrouchWalkingSlowly"]!, json => new BingoTriggerIsCrouchWalkingSlowly(json));
			if (json["IsCrouchWalking"] != null)
				return Load(json["IsCrouchWalking"]!, json => new BingoTriggerIsCrouchWalking(json));
			if (json["IsCrouchRunning"] != null)
				return Load(json["IsCrouchRunning"]!, json => new BingoTriggerIsCrouchRunning(json));
			if (json["IsRunning"] != null)
				return Load(json["IsRunning"]!, json => new BingoTriggerIsRunning(json));
			if (json["DrainPipeClimbed"] != null)
				return Load(json["DrainPipeClimbed"]!, json => new BingoTriggerDrainPipeClimbed(json));
			return Load(json!, j => j != json ? FromJson_Trigger(type, j) : null);
		}
	}

	public class BingoTriggerComplication(BingoTrigger? trigger) : BingoTrigger {
		readonly BingoTrigger? trigger = trigger;

		public override bool Test(EventValue ev, BingoTileState state) {
			return trigger == null || trigger.Test(ev, state);
		}

		public override void Advance(BingoTileState state) {
			state.Complete = false;
		}
	}

	public class BingoTriggerNOT(BingoTrigger? trigger) : BingoTrigger {
		readonly BingoTrigger? trigger = trigger;

		public override bool Test(EventValue ev, BingoTileState state) {
			return trigger == null || !trigger.Test(ev, state);
		}
	}

	public class BingoTriggerOR(List<BingoTrigger?> triggers) : BingoTrigger {
		readonly List<BingoTrigger?> triggers = triggers;

		public override bool Test(EventValue ev, BingoTileState state) {
			return triggers.Any(t => t?.Test(ev, state) ?? true);
		}
	}

	public class BingoTriggerAND(List<BingoTrigger?> triggers) : BingoTrigger {
		readonly List<BingoTrigger?> triggers = triggers;

		public override bool Test(EventValue ev, BingoTileState state) {
			return triggers.All(t => t?.Test(ev, state) ?? true);
		}
	}

	public class BingoTriggerPacify : BingoTrigger {
		public BingoTriggerBool Accident { get; set; } = new();
		public BingoTriggerBool ActorHasDisguise { get; set; } = new();
		public BingoTriggerBool ActorHasSameOutfit { get; set; } = new();
		public BingoTriggerString ActorName { get; set; } = new();
		public BingoTriggerBool ActorOutfitIsUnique { get; set; } = new();
		public BingoTriggerEnum<EOutfitType> ActorOutfitType { get; set; } = new();
		public BingoTriggerEnum<EActorType> ActorType { get; set; } = new();
		public BingoTriggerArray<string> DamageEvents { get; set; } = new(JsonValueKind.String);
		public BingoTriggerInt ExplosionType { get; set; } = new();
		public BingoTriggerBool Explosive { get; set; } = new();
		public BingoTriggerBool IsFemale { get; set; } = new();
		public BingoTriggerBool IsHeadshot { get; set; } = new();
		public BingoTriggerBool IsTarget { get; set; } = new();
		public BingoTriggerString KillClass { get; set; } = new();
		public BingoTriggerString KillContext { get; set; } = new();
		public BingoTriggerString KillItemRepositoryId { get; set; } = new();
		public BingoTriggerString KillItemCategory { get; set; } = new();
		public BingoTriggerString KillMethodBroad { get; set; } = new();
		public BingoTriggerString KillMethodStrict { get; set; } = new();
		public BingoTriggerEnum<EKillType> KillType { get; set; } = new();
		public BingoTriggerString OutfitRepositoryId { get; set; } = new();
		public BingoTriggerString RepositoryId { get; set; } = new();
		public BingoTriggerString SetPieceId { get; set; } = new();
		public BingoTriggerString SetPieceType { get; set; } = new();
		public BingoTriggerUnique Unique { get; set; } = new();

		public BingoTriggerPacify(JsonObject json) : base(json) {
			Accident.Load(json["Accident"]);
			ActorHasDisguise.Load(json["ActorHasDisguise"]);
			ActorHasSameOutfit.Load(json["ActorHasSameOutfit"]);
			ActorName.Load(json["ActorName"]);
			ActorOutfitIsUnique.Load(json["ActorOutfitIsUnique"]);
			ActorOutfitType.Load(json["OutfitType"], str => str switch {
				"Suit" => EOutfitType.eOT_Suit,
				"Guard" => EOutfitType.eOT_Guard,
				"Worker" => EOutfitType.eOT_Worker,
				"Waiter" => EOutfitType.eOT_Waiter,
				"LucasGrey" => EOutfitType.eOT_LucasGrey,
				_ => throw new BingoConfigException("Invalid OutfitType value.")
			});
			ActorType.Load(json["ActorType"], str => str switch {
				"Guard" => EActorType.eAT_Guard,
				"Civilian" => EActorType.eAT_Civilian,
				_ => throw new BingoConfigException("Invalid ActorType - expected 'Guard' or 'Civilian'.")
			});
			DamageEvents.Load(json["DamageEvents"]);
			ExplosionType.Load(json["ExplosionType"]);
			Explosive.Load(json["Explosive"]);
			IsHeadshot.Load(json["IsHeadshot"]);
			IsFemale.Load(json["IsFemale"]);
			IsTarget.Load(json["IsTarget"]);
			KillClass.Load(json["KillClass"]);
			KillContext.Load(json["KillContext"]);
			KillItemRepositoryId.Load(json["KillItemRepositoryId"]);
			KillItemCategory.Load(json["KillItemCategory"]);
			KillMethodBroad.Load(json["KillMethodBroad"]);
			KillMethodStrict.Load(json["KillMethodStrict"]);
			KillType.Load(json["KillType"], str => str switch {
				"Throw" => EKillType.EKillType_Throw,
				"Fiberwire" => EKillType.EKillType_Fiberwire,
				"PistolExecute" => EKillType.EKillType_PistolExecute,
				"ItemTakeOutFront" => EKillType.EKillType_ItemTakeOutFront,
				"ItemTakeOutBack" => EKillType.EKillType_ItemTakeOutBack,
				"ChokeOut" => EKillType.EKillType_ChokeOut,
				"SnapNeck" => EKillType.EKillType_SnapNeck,
				"KnockOut" => EKillType.EKillType_KnockOut,
				"Push" => EKillType.EKillType_Push,
				"Pull" => EKillType.EKillType_Pull,
				_ => throw new BingoConfigException("Invalid KillType value.")
			});
			OutfitRepositoryId.Load(json["OutfitRepositoryId"]);
			RepositoryId.Load(json["RepositoryId"]);
			SetPieceId.Load(json["SetPieceId"]);
			SetPieceType.Load(json["SetPieceType"]);
			Unique.Load(json["Unique"]);
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			if (ev is PacifyEventValue v && (ev is not KillEventValue || this is BingoTriggerKill)) {
				if (!base.Test(ev, state)) 
					return false;
				if (!Unique.Test(v, state)) 
					return false;
				if (!Accident.Test(v.Accident, state)) 
					return false;
				if (!ActorHasDisguise.Test(v.ActorHasDisguise, state)) 
					return false;
				if (!ActorHasSameOutfit.Test(v.ActorHasSameOutfit, state))
					return false;
				if (!ActorName.Test(v.ActorName, state)) 
					return false;
				if (!ActorOutfitType.Test(v.ActorOutfitType, state)) 
					return false;
				if (!ActorOutfitIsUnique.Test(v.ActorOutfitIsUnique, state))
					return false;
				if (!ActorType.Test(v.ActorType, state)) 
					return false;
				if (!DamageEvents.Test(v.DamageEvents, state)) 
					return false;
				if (!ExplosionType.Test(v.ExplosionType, state)) 
					return false;
				if (!Explosive.Test(v.Explosive, state)) 
					return false;
				if (!IsFemale.Test(v.IsFemale, state)) 
					return false;
				if (!IsHeadshot.Test(v.IsHeadshot, state)) 
					return false;
				if (!IsTarget.Test(v.IsTarget, state)) 
					return false;
				if (!KillClass.Test(v.KillClass, state)) 
					return false;
				if (!KillContext.Test(v.KillContext, state)) 
					return false;
				if (!KillItemRepositoryId.Test(v.KillItemRepositoryId, state)) 
					return false;
				if (!KillItemCategory.Test(v.KillItemCategory, state)) 
					return false;
				if (!KillMethodBroad.Test(v.KillMethodBroad, state)) 
					return false;
				if (!KillMethodStrict.Test(v.KillMethodStrict, state)) 
					return false;
				if (!KillType.Test(v.KillType, state)) 
					return false;
				if (!OutfitRepositoryId.Test(v.OutfitRepositoryId, state)) 
					return false;
				if (!RepositoryId.Test(v.RepositoryId, state)) 
					return false;
				if (!SetPieceId.Test(v.SetPieceId, state)) 
					return false;
				if (!SetPieceType.Test(v.SetPieceType, state)) 
					return false;
				return true;
			}
			return false;
		}
	}

	public class BingoTriggerKill(JsonObject json) : BingoTriggerPacify(json) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is KillEventValue && base.Test(ev, state);
		}
	}

	public class BingoTriggerActorSick : BingoTrigger {
		public BingoTriggerString ActorName { get; set; } = new();
		public BingoTriggerString ActorRepositoryId { get; set; } = new();
		public BingoTriggerBool IsTarget { get; set; } = new();
		public BingoTriggerString ItemRepositoryId { get; set; } = new();
		public BingoTriggerString SetPieceRepositoryId { get; set; } = new();
		public BingoTriggerEnum<EActorType> ActorType { get; set; } = new();
		public BingoTriggerUnique Unique { get; set; } = new();

		public BingoTriggerActorSick(JsonObject json) : base(json) {
			Unique.Load(json["Unique"]);
			ActorName.Load(json["ActorName"]);
			ActorRepositoryId.Load(json["ActorRepositoryId"]);
			ActorType.Load(json["ActorType"], str => str switch {
				"Guard" => EActorType.eAT_Guard,
				"Civilian" => EActorType.eAT_Civilian,
				_ => throw new BingoConfigException("Invalid ActorType - expected 'Guard' or 'Civilian'.")
			});
			IsTarget.Load(json["IsTarget"]);
			ItemRepositoryId.Load(json["ItemRepositoryId"]);
			SetPieceRepositoryId.Load(json["SetPieceRepositoryId"]);
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ActorSickEventValue v
				&& Unique.Test(v, state)
				&& ActorName.Test(v.ActorName, state)
				&& ActorRepositoryId.Test(v.actor_R_ID, state)
				&& ActorType.Test(v.ActorType, state)
				&& IsTarget.Test(v.IsTarget, state)
				&& ItemRepositoryId.Test(v.item_R_ID, state)
				&& SetPieceRepositoryId.Test(v.setpiece_R_ID, state);
		}
	}

	public class BingoTriggerItemStashed : BingoTrigger {
		public BingoTriggerItemStashed(JsonNode json) : base(json) {

		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemStashedEventValue;
		}
	}

	public class BingoTriggerShotFired(JsonNode json) : BingoTrigger(json) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ShotFiredEventValue;
		}
	}

	public class BingoTriggerDartHit : BingoTrigger {
		public BingoTriggerString RepositoryId { get; set; } = new();
		public BingoTriggerEnum<EActorType> ActorType { get; set; } = new();
		public BingoTriggerBool IsTarget { get; set; } = new();
		public BingoTriggerBool Blind { get; set; } = new();
		public BingoTriggerBool Sedative { get; set; } = new();
		public BingoTriggerBool Sick { get; set; } = new();
		public BingoTriggerUnique Unique { get; set; } = new();

		public BingoTriggerDartHit(JsonObject json) : base(json) {
			RepositoryId.Load(json["RepositoryId"]);
			ActorType.Load(json["ActorType"], str => str switch {
				"Guard" => EActorType.eAT_Guard,
				"Civilian" => EActorType.eAT_Civilian,
				_ => throw new BingoConfigException("Invalid ActorType - expected 'Guard' or 'Civilian'.")
			});
			IsTarget.Load(json["IsTarget"]);
			Blind.Load(json["Blind"]);
			Sedative.Load(json["Sedative"]);
			Sick.Load(json["Sick"]);
			Unique.Load(json["Unique"]);
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DartHitEventValue v
				&& Unique.Test(v, state)
				&& RepositoryId.Test(v.RepositoryId, state)
				&& IsTarget.Test(v.IsTarget, state)
				&& Blind.Test(v.Blind, state)
				&& Sedative.Test(v.Sedative, state)
				&& Sick.Test(v.Sick, state)
				&& ActorType.Test(v.ActorType, state);
		}
	}

	public class BingoTriggerDoorBroken : BingoTrigger {
		public BingoTriggerDoorBroken(JsonNode json) : base(json) {

		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DoorBrokenEventValue;
		}
	}

	public class BingoTriggerDoorUnlocked : BingoTrigger {
		public BingoTriggerDoorUnlocked(JsonNode json) : base(json) {

		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DoorUnlockedEventValue;
		}
	}

	public class BingoTriggerItemThrown : BingoTrigger {
		public BingoTriggerString ItemName { get; set; } = new();
		public BingoTriggerString ItemType { get; set; } = new();
		public BingoTriggerString RepositoryId { get; set; } = new();
		public BingoTriggerItemThrown(JsonNode json) : base(json) {

		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemThrownEventValue v
				&& ItemName.Test(v.ItemName, state)
				&& ItemType.Test(v.ItemType, state)
				&& RepositoryId.Test(v.RepositoryId, state);
		}
	}

	public class BingoTriggerItemDropped : BingoTrigger {
		public BingoTriggerString ItemName { get; set; } = new();
		public BingoTriggerString ItemType { get; set; } = new();
		public BingoTriggerString RepositoryId { get; set; } = new();
		public BingoTriggerItemDropped(JsonNode json) : base(json) {

		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemDroppedEventValue v
				&& ItemName.Test(v.ItemName, state)
				&& ItemType.Test(v.ItemType, state)
				&& RepositoryId.Test(v.RepositoryId, state);
		}
	}

	public class BingoTriggerSetpiece : BingoTrigger {
		public BingoTriggerString RepositoryId { get; set; } = new();
		public BingoTriggerString Name { get; set; } = new();
		public BingoTriggerString Type { get; set; } = new();
		public BingoTriggerString Helper { get; set; } = new();
		public BingoTriggerString ToolUsed { get; set; } = new();
		public BingoTriggerString Position { get; set; } = new();
		public BingoTriggerUnique Unique { get; set; } = new();

		public BingoTriggerSetpiece(JsonObject obj) : base(obj) {
			Name.Load(obj["Name"]);
			Type.Load(obj["Type"]);
			Helper.Load(obj["Helper"]);
			ToolUsed.Load(obj["ToolUsed"]);
			RepositoryId.Load(obj["RepositoryId"]);
			Position.Load(obj["Position"]);
			Unique.Load(obj["Unique"]);
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is SetpiecesEventValue v
				&& Unique.Test(v, state)
				&& Name.Test(v.name_metricvalue, state)
				&& Type.Test(v.setpieceType_metricvalue, state)
				&& Helper.Test(v.setpieceHelper_metricvalue, state)
				&& ToolUsed.Test(v.toolUsed_metricvalue, state)
				&& RepositoryId.Test(v.RepositoryId, state)
				&& Position.Test(v.Position, state);
		}
	}

	public class BingoTriggerLevelSetupEvent : BingoTrigger {
		public BingoTriggerString Event { get; set; } = new();
		public BingoTriggerString Location { get; set; } = new();
		public BingoTriggerString ContractName { get; set; } = new();
		public BingoTriggerUnique Unique { get; set; } = new();

		public BingoTriggerLevelSetupEvent(JsonObject obj) : base(obj) {
			Event.Load(obj["Event"]);
			Location.Load(obj["Location"]);
			ContractName.Load(obj["ContractName"]);
			Unique.Load(obj["Unique"]);
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is LevelSetupEventValue v
				&& Unique.Test(v, state)
				&& Event.Test(v.Event_metricvalue, state)
				&& Location.Test(v.Location_MetricValue, state)
				&& ContractName.Test(v.Contract_Name_metricvalue, state);
		}
	}

	public class BingoTriggerBodyHidden : BingoTrigger {
		public BingoTriggerString ActorName { get; set; } = new();
		public BingoTriggerString RepositoryId { get; set; } = new();
		public BingoTriggerUnique Unique { get; set; } = new();

		public BingoTriggerBodyHidden(JsonObject obj) : base(obj) {
			ActorName.Load(obj["ActorName"]);
			RepositoryId.Load(obj["RepositoryId"]);
			Unique.Load(obj["Load"]);
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is BodyHiddenEventValue v
				&& Unique.Test(v, state)
				&& ActorName.Test(v.ActorName, state)
				&& RepositoryId.Test(v.RepositoryId, state);
		}
	}

	public class BingoTriggerSecuritySystemRecorder : BingoTrigger {
		public BingoTriggerString Event { get; set; } = new();
		public BingoTriggerInt Camera { get; set; } = new();
		public BingoTriggerInt Recorder { get; set; } = new();
		public BingoTriggerUnique Unique { get; set; } = new();

		public BingoTriggerSecuritySystemRecorder(JsonNode json) : base(json) {
			if (json.GetValueKind() != JsonValueKind.Object)
				throw new BingoTileConfigException("Expected object value.");
			Event.Load(json["Event"]);
			Camera.Load(json["Camera"]);
			Recorder.Load(json["Recorder"]);
			Unique.Load(json["Load"]);
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is SecuritySystemRecorderEventValue v
				&& Unique.Test(v, state)
				&& Event.Test(v.Event, state)
				&& Camera.Test(v.Camera, state)
				&& Recorder.Test(v.Recorder, state);
		}
	}

	public class BingoTriggerCollect : BingoTrigger {
		public BingoTriggerString Items { get; set; } = new();
		public int? Max { get; set; }

		public BingoTriggerCollect(JsonObject obj) : base(obj) {
			Items.Load(obj["Items"]);
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemPickedUpEventValue v
				&& v.InstanceId.Length != 0
				&& Items.Test(v.RepositoryId, state);
		}
	}

	public class BingoTriggerDisguise : BingoTrigger {
		public BingoTriggerString RepositoryId { get; set; } = new();
		public BingoTriggerString Title { get; set; } = new();
		public BingoTriggerEnum<EActorType> ActorType { get; set; } = new();
		public BingoTriggerBool IsSuit { get; set; } = new();
		public BingoTriggerUnique Unique { get; set; } = new();
		public BingoTriggerEnum<EOutfitType> OutfitType { get; set; } = new();

		public BingoTriggerDisguise(JsonNode obj) : base() {
			switch (obj.GetValueKind()) {
				case JsonValueKind.Array:
				case JsonValueKind.String:
					RepositoryId.Load(obj);
					return;
				case JsonValueKind.Object:
					Unique.Load(obj["Unique"]);
					RepositoryId.Load(obj["RepositoryId"]);
					Title.Load(obj["Title"]);
					ActorType.Load(obj["ActorType"], s => s switch {
						"Civilian" => EActorType.eAT_Civilian,
						"Guard" => EActorType.eAT_Guard,
						"Hitman" => EActorType.eAT_Hitman,
						_ => throw new BingoConfigException("ActorType must be one of: Civilian, Guard, Hitman")
					});
					IsSuit.Load(obj["IsSuit"]);
					OutfitType.Load(obj["OutfitType"], s => s switch {
						"Suit" => EOutfitType.eOT_Suit,
						"Guard" => EOutfitType.eOT_Guard,
						"Worker" => EOutfitType.eOT_Worker,
						"Waiter" => EOutfitType.eOT_Waiter,
						"LucasGrey" => EOutfitType.eOT_LucasGrey,
						_ => throw new BingoConfigException("OutfitType must be one of: Suit, Guard, Worker, Waiter, LucasGrey")
					});
					return;
				default:
					throw new BingoConfigException("Expected string, object or string array for disguise.");
			}
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DisguiseEventValue v
				&& (ev is not StartingSuitEventValue || this is BingoTriggerStartingSuit)
				&& Unique.Test(v, state)
				&& RepositoryId.Test(v.RepositoryId, state)
				&& Title.Test(v.Title, state)
				&& ActorType.Test(v.ActorType, state)
				&& IsSuit.Test(v.IsSuit, state)
				&& OutfitType.Test(v.OutfitType, state);
		}
	}

	public class BingoTriggerStartingSuit(JsonNode obj) : BingoTriggerDisguise(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is StartingSuitEventValue && base.Test(ev, state);
		}
	}

	public class BingoTriggerOnIsFullyInCrowd(JsonNode obj) : BingoTrigger(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnIsFullyInCrowdEventValue;
		}
	}

	public class BingoTriggerOnIsFullyInVegetation(JsonNode obj) : BingoTrigger(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnIsFullyInVegetationEventValue;
		}
	}

	public class BingoTriggerOnTakeDamage(JsonNode obj) : BingoTrigger(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnTakeDamageEventValue;
		}
	}

	public class BingoTriggerInstinctActive(JsonNode obj) : BingoTrigger(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is InstinctActiveEventValue;
		}
	}

	public class BingoTriggerIsCrouchWalkingSlowly(JsonNode obj) : BingoTrigger(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is IsCrouchWalkingSlowlyEventValue;
		}
	}

	public class BingoTriggerIsCrouchWalking(JsonNode obj) : BingoTrigger(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is IsCrouchWalkingEventValue;
		}
	}

	public class BingoTriggerIsCrouchRunning(JsonNode obj) : BingoTrigger(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is IsCrouchRunningEventValue;
		}
	}

	public class BingoTriggerIsRunning(JsonNode obj) : BingoTrigger(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is IsRunningEventValue;
		}
	}

	public class BingoTriggerDrainPipeClimbed(JsonNode obj) : BingoTrigger(obj) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DrainPipeClimbedEventValue;
		}
	}
}
