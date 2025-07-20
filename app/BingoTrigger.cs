using Croupier.Exceptions;
using Croupier.GameEvents;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using System.Windows.Controls;

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

		public void Load(JsonElement json, string propName) {
			if (!json.TryGetProperty(propName, out var prop))
				return;

			this.propName = prop.ValueKind switch {
				JsonValueKind.String => prop.GetString(),
				_ => throw new BingoTileConfigException("Expected string.")
			};
		}
	}

	public class BingoTriggerBool : BingoTriggerExpression {
		private bool? value;

		public override bool Test(object? value, BingoTileState state) {
			return this.value == null || (value is bool v && this.value == v);
		}

		public void Load(JsonElement json, string propName) {
			if (!json.TryGetProperty(propName, out var prop))
				return;

			value = prop.ValueKind switch {
				JsonValueKind.True => true,
				JsonValueKind.False => false,
				_ => throw new BingoTileConfigException("Expected true or false.")
			};
		}
	}

	public class BingoTriggerEnum<TEnum> : BingoTriggerExpression where TEnum : struct, Enum {
		private List<TEnum>? values = null;

		public override bool Test(object? value, BingoTileState state) {
			return values == null || (value is TEnum v && values.Contains(v));
		}

		public void Load(JsonElement json, string propName, Func<string, TEnum>? converter = null) {
			if (!json.TryGetProperty(propName, out var prop))
				return;

			converter ??= new(str => (TEnum)Enum.Parse(typeof(TEnum), str));
			values = prop.ValueKind switch {
				JsonValueKind.String => [converter(prop.GetString() ?? "")],
				JsonValueKind.Array => LoadEnumArray(prop, converter),
				_ => throw new BingoTileConfigException("Expected string or string array.")
			};
		}

		public static List<TEnum> LoadEnumArray(JsonElement prop, Func<string, TEnum> converter) {
			if (prop.ValueKind != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new List<TEnum>();
			foreach (var item in prop.EnumerateArray()) {
				if (item.ValueKind != JsonValueKind.String)
					throw new BingoTileConfigException($"Invalid enum value type '{item.ValueKind}' in array.");
				coll.Add(converter(item.GetString() ?? ""));
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

		public void Load(JsonElement json, string? propName = null) {
			JsonElement prop = json;
			if (propName != null) {
				if (!json.TryGetProperty(propName, out var p)) return;
				prop = p;
			}

			values = prop.ValueKind switch {
				JsonValueKind.String => [Int64.Parse(prop.GetString() ?? "")],
				JsonValueKind.Number => [prop.GetInt64()],
				JsonValueKind.Array => LoadIntArray(prop),
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

		public static List<Int64> LoadIntArray(JsonElement prop) {
			if (prop.ValueKind != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new List<Int64>();
			foreach (var item in prop.EnumerateArray()) {
				coll.Add(item.ValueKind switch {
					JsonValueKind.String => Int64.Parse(item.GetString() ?? ""),
					JsonValueKind.Number => item.GetInt64(),
					_ => throw new BingoTileConfigException("Expected number.")
				});
			}
			return coll;
		}
	}

	public class BingoTriggerUInt : BingoTriggerExpression {
		private List<UInt64>? values;

		public override bool Test(object? value, BingoTileState state) {
			return values == null || (
				TryConvertValue(value, out var v)
				&& v != null
				&& values.Contains((ulong)v)
			);
		}

		public void Load(JsonElement json, string? propName = null) {
			JsonElement prop = json;
			if (propName != null) {
				if (!json.TryGetProperty(propName, out var p)) return;
				prop = p;
			}

			values = prop.ValueKind switch {
				JsonValueKind.String => [UInt64.Parse(prop.GetString() ?? "")],
				JsonValueKind.Number => [prop.GetUInt64()],
				JsonValueKind.Array => LoadUIntArray(prop),
				_ => throw new BingoTileConfigException("Expected number or numeric array.")
			};
		}

		public static bool TryConvertValue(object? value, out UInt64? result) {
			if (value != null) {
				try {
					result = Convert.ToUInt64(value);
					return true;
				} catch { }
			}
			result = null;
			return false;
		}

		public static List<UInt64> LoadUIntArray(JsonElement prop) {
			if (prop.ValueKind != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new List<UInt64>();
			foreach (var item in prop.EnumerateArray()) {
				coll.Add(item.ValueKind switch {
					JsonValueKind.String => UInt64.Parse(item.GetString() ?? ""),
					JsonValueKind.Number => item.GetUInt64(),
					_ => throw new BingoTileConfigException("Expected number.")
				});
			}
			return coll;
		}
	}

	public class BingoTriggerCIString : BingoTriggerExpression {
		private StringCollection? values;

		public override bool Test(object? value, BingoTileState state) {
			return values == null || values.Count == 0 || (
				value is string v
				&& values.Contains(v.ToLower())
			);
		}

		public void Load(JsonElement json, string? propName = null) {
			JsonElement prop = json;
			if (propName != null) {
				if (!json.TryGetProperty(propName, out var p)) return;
				prop = p;
			}
			values = prop.ValueKind switch {
				JsonValueKind.String => [prop.GetString()?.ToLower()],
				JsonValueKind.Array => LoadStringArray(prop),
				_ => throw new BingoTileConfigException("Expected string or string array.")
			};
		}

		public static StringCollection LoadStringArray(JsonElement prop) {
			if (prop.ValueKind != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new StringCollection();
			foreach (var item in prop.EnumerateArray()) {
				coll.Add(item.GetString()?.ToLower());
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

		public void Load(JsonElement json, string? propName = null) {
			JsonElement prop = json;
			if (propName != null) {
				if (!json.TryGetProperty(propName, out var p)) return;
				prop = p;
			}
			values = prop.ValueKind switch {
				JsonValueKind.String => [prop.GetString()],
				JsonValueKind.Array => LoadStringArray(prop),
				_ => throw new BingoTileConfigException("Expected string or string array.")
			};
		}

		public static StringCollection LoadStringArray(JsonElement prop) {
			if (prop.ValueKind != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new StringCollection();
			foreach (var item in prop.EnumerateArray()) {
				coll.Add(item.GetString());
			}
			return coll;
		}
	}

	public class BingoTriggerArray<T>(JsonValueKind jsonKind) : BingoTriggerExpression {
		private readonly JsonValueKind jsonKind = jsonKind;
		private List<T>? values;

		public override bool Test(object? value, BingoTileState state) {
			return values == null || values.Count == 0 || (
				value is List<T> v
				&& values.All(val => v.Contains(val))
			);
		}

		public void Load(JsonElement json, string propName) {
			if (!json.TryGetProperty(propName, out var prop))
				return;
			values = prop.ValueKind switch {
				JsonValueKind.Array => LoadArray(prop),
				_ => [LoadNodeValue(prop)],
			};
		}

		public List<T> LoadArray(JsonElement prop) {
			if (prop.ValueKind != JsonValueKind.Array)
				throw new BingoTileConfigException("Expected array property.");
			var coll = new List<T>();
			foreach (var item in prop.EnumerateArray()) {
				coll.Add(LoadNodeValue(item));
			}
			return coll;
		}

		private T LoadNodeValue(JsonElement prop) {
			if (prop.ValueKind != jsonKind)
				throw new BingoTileConfigException("Invalid value type.");
			return prop.Deserialize<T>() ?? throw new BingoTileConfigException($"Invalid value type {prop.ValueKind} for BingoTriggerArray, expected {jsonKind}.");
		}
	}

	public class BingoTriggerPosition : BingoTriggerExpression {
		struct CoordRange {
			public double? From;
			public double? To;
		}
		CoordRange? RangeX = new();
		CoordRange? RangeY = new();
		CoordRange? RangeZ = new();
		double? X = null;
		double? Y = null;
		double? Z = null;

		public override bool Test(object? value, BingoTileState state) {
			if (value is not SVector3 v) return true;
			return TestAxis(X, RangeX, v.X)
				&& TestAxis(Y, RangeY, v.Y)
				&& TestAxis(Z, RangeZ, v.Z);
		}

		public void Load(JsonElement json, string propName) {
			if (!json.TryGetProperty(propName, out var prop))
				return;
			if (prop.ValueKind != JsonValueKind.Object)
				throw new BingoTileConfigException("Property 'Position' should be an object.");
			LoadObject(prop);
		}

		private void LoadObject(JsonElement json) {
			RangeX = LoadCoordRange("X", json);
			RangeY = LoadCoordRange("Y", json);
			RangeZ = LoadCoordRange("Z", json);
			X = LoadAxis("X", json);
			Y = LoadAxis("Y", json);
			Z = LoadAxis("Z", json);
		}

		private static bool TestAxis(double? exact, CoordRange? range, double value) {
			if (exact.HasValue) {
				return Math.Abs(exact.Value - value) <= 0.001;
			}
			else if (range.HasValue) {
				if (range.Value.From.HasValue && range.Value.From.Value > value)
					return false;
				if (range.Value.To.HasValue && range.Value.To.Value < value)
					return false;
			}
			return true;
		}

		private static double? LoadAxis(string axis, JsonElement json) {
			if (!json.TryGetProperty(axis, out var prop))
				return null;
			if (prop.ValueKind != JsonValueKind.Number)
				return null;
			return prop.GetDouble();
		}

		private static CoordRange? LoadCoordRange(string axis, JsonElement json) {
			if (!json.TryGetProperty(axis, out var prop))
				return null;
			if (prop.ValueKind == JsonValueKind.Number)
				return null;
			if (prop.ValueKind != JsonValueKind.Object)
				throw new BingoTileConfigException($"Expected object for {axis} axis.");
			JsonElement? from = prop.TryGetProperty("From", out var fromProp) ? fromProp : null;
			JsonElement? to = prop.TryGetProperty("To", out var toProp) ? toProp : null;
			if (!from.HasValue && !to.HasValue)
				throw new BingoTileConfigException($"Expected 'From' and/or 'To' property for {axis} axis.");
			return new() {
				From = from?.ValueKind switch {
					JsonValueKind.Number => from.Value.GetDouble(),
					null => null,
					_ => throw new BingoTileConfigException($"Propery 'From' must be a number for {axis} axis."),
				},
				To = to?.ValueKind switch {
					JsonValueKind.Number => to.Value.GetDouble(),
					null => null,
					_ => throw new BingoTileConfigException($"Propery 'To' must be a number for {axis} axis."),
				},
			};
		}
	}

	public interface IBingoTrigger {
		bool Test(EventValue ev, BingoTileState state);
	}

	public class BingoTrigger : IBingoTrigger {
		// Have to wrap the struct in a class to identify it in order to prevent a recursion loop.
		class JsonHolder(JsonElement json) {
			public readonly JsonElement json = json;
		}

		public int? Count { get; set; }

		protected BingoTrigger() {}
		protected BingoTrigger(JsonElement json) {
			if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty(nameof(Count), out var countProp))
				Count = countProp.GetInt32();
		}

		public virtual bool Test(EventValue ev, BingoTileState state) {
			return true;
		}

		public virtual void Advance(BingoTileState state) {
			if (Count == null) state.Complete = true;
			else state.Complete = ++state.Counter >= Count;
		}

		public virtual object[] GetFormatArgs(BingoTileState state) {
			return [Count ?? 1, (Count ?? 1) - (state.Complete ? 0 : state.Counter), state.Counter];
		}

		private static BingoTrigger? Load(JsonHolder jsonHolder, Func<JsonHolder, BingoTrigger?> create) {
			var json = jsonHolder.json;
			var kind = json.ValueKind;

			if (kind == JsonValueKind.Object) {
				var triggers = new List<IBingoTrigger>();
				if (json.TryGetProperty("NOT", out var NOT)) {
					var t = Load(new(NOT), json => create(json));
					if (t != null)
						triggers.Add(new BingoTriggerNOT(t));
				}
				if (json.TryGetProperty("OR", out var OR)) {
					if (OR.ValueKind != JsonValueKind.Array)
						throw new BingoConfigException("Expected array for 'OR' trigger group.");
					var orTriggers = new List<IBingoTrigger?>();
					foreach (var j in OR.EnumerateArray()) {
						var t = Load(new(j), json => create(json));
						if (t != null)
							orTriggers.Add(t);
					}
					if (orTriggers.Count > 0)
						triggers.Add(new BingoTriggerOR(orTriggers));
				}
				if (triggers.Count > 0)
					return new BingoTriggerAND([create(jsonHolder), ..triggers], json);
				//else if (triggers.Count == 1)
				//	return triggers[0];
			}

			return create(jsonHolder);
		}

		public static BingoTrigger? FromJson(BingoTileType type, JsonElement json) {
			var jsonHolder = new JsonHolder(json);
			var trigger = FromJson_Trigger(type, jsonHolder);
			if (trigger != null && type == BingoTileType.Complication)
				return new BingoTriggerComplication(trigger);
			return trigger;
		}

		private static Func<JsonHolder, BingoTrigger>? FromJson_TriggerProp(string propName) {
			return propName switch {
				"ActorSick" => jh => new BingoTriggerActorSick(jh.json),
				"BodyBagged" => jh => new BingoTriggerBodyBagged(jh.json),
				"BodyFound" => jh => new BingoTriggerBodyFound(jh.json),
				"BodyHidden" => jh => new BingoTriggerBodyHidden(jh.json),
				"CarExploded" => jh => new BingoTriggerCarExploded(jh.json),
				"Collect" => jh => new BingoTriggerCollect(jh.json),
				"DartHit" => jh => new BingoTriggerDartHit(jh.json),
				"Disguise" => jh => new BingoTriggerDisguise(jh.json),
				"DoorBroken" => jh => new BingoTriggerDoorBroken(jh.json),
				"DoorUnlocked" => jh => new BingoTriggerDoorUnlocked(jh.json),
				"DragBodyMove" => jh => new BingoTriggerDragBodyMove(jh.json),
				"DrainPipeClimbed" => jh => new BingoTriggerDrainPipeClimbed(jh.json),
				"EnterArea" => jh => new BingoTriggerEnterArea(jh.json),
				"EnterRoom" => jh => new BingoTriggerEnterRoom(jh.json),
				"Explosion" => jh => new BingoTriggerExplosion(jh.json),
				"FriskedSuccess" => jh => new BingoTriggerFriskedSuccess(jh.json),
				"InstinctActive" => jh => new BingoTriggerInstinctActive(jh.json),
				"ItemDestroyed" => jh => new BingoTriggerItemDestroyed(jh.json),
				"ItemDropped" => jh => new BingoTriggerItemDropped(jh.json),
				"ItemStashed" => jh => new BingoTriggerItemStashed(jh.json),
				"ItemThrown" => jh => new BingoTriggerItemThrown(jh.json),
				"Kill" => jh => new BingoTriggerKill(jh.json),
				"LevelSetupEvent" => jh => new BingoTriggerLevelSetupEvent(jh.json),
				"Movement" => jh => new BingoTriggerMovement(jh.json),
				"OnDestroy" => jh => new BingoTriggerOnDestroy(jh.json),
				"OnEvacuationStarted" => jh => new BingoTriggerOnEvacuationStarted(jh.json),
				"OnIsFullyInCrowd" => jh => new BingoTriggerOnIsFullyInCrowd(jh.json),
				"OnIsFullyInVegetation" => jh => new BingoTriggerOnIsFullyInVegetation(jh.json),
				"OnPickup" => jh => new BingoTriggerOnPickup(jh.json),
				"OnTakeDamage" => jh => new BingoTriggerOnTakeDamage(jh.json),
				"OnTurnOn" => jh => new BingoTriggerOnTurnOn(jh.json),
				"OnTurnOff" => jh => new BingoTriggerOnTurnOff(jh.json),
				"OnWeaponReload" => jh => new BingoTriggerOnWeaponReload(jh.json),
				"OpenDoor" => jh => new BingoTriggerOpenDoor(jh.json),
				"OpportunityEvent" => jh => new BingoTriggerOpportunityEvent(jh.json),
				"Pacify" => jh => new BingoTriggerPacify(jh.json),
				"PlayerShot" => jh => new BingoTriggerPlayerShot(jh.json),
				"ProjectileBodyShot" => jh => new BingoTriggerProjectileBodyShot(jh.json),
				"SecuritySystemRecorder" => jh => new BingoTriggerSecuritySystemRecorder(jh.json),
				"Setpiece" => jh => new BingoTriggerSetpiece(jh.json),
				"StartingSuit" => jh => new BingoTriggerStartingSuit(jh.json),
				"Trespassing" => jh => new BingoTriggerTrespassing(jh.json),
				_ => null,
			};
		}

		private static BingoTrigger? FromJson_Trigger(BingoTileType type, JsonHolder jsonHolder) {
			var json = jsonHolder.json;
			if (json.ValueKind != JsonValueKind.Object)
				throw new BingoConfigException("Expected object for parent of trigger.");

			foreach (var prop in json.EnumerateObject()) {
				var fn = FromJson_TriggerProp(prop.Name);
				if (fn == null) continue;
				return Load(new(prop.Value), fn);
			}
			return Load(jsonHolder, j => j != jsonHolder ? FromJson_Trigger(type, j) : null);
		}
	}

	public class BingoTriggerActorInfoImbued : IBingoTrigger {
		readonly BingoTriggerString ActorName = new();
		readonly BingoTriggerString ActorRepositoryId = new();
		readonly BingoTriggerString ActorOutfitRepositoryId = new();
		readonly BingoTriggerEnum<EActorType> ActorType = new();
		readonly BingoTriggerInt ActorWeaponIndex = new();
		readonly BingoTriggerBool ActorHasDisguise = new();
		readonly BingoTriggerBool ActorIsAuthorityFigure = new();
		readonly BingoTriggerBool ActorIsDead = new();
		readonly BingoTriggerBool ActorIsFemale = new();
		readonly BingoTriggerBool ActorIsPacified = new();
		readonly BingoTriggerBool ActorIsTarget = new();
		readonly BingoTriggerBool ActorOutfitAllowsWeapons = new();
		readonly BingoTriggerBool ActorWeaponUnholstered = new();
		//readonly BingoTriggerString ActorOutfitType = new();

		public BingoTriggerActorInfoImbued(JsonElement json) : base() {
			if (json.ValueKind != JsonValueKind.Object) return;
			ActorName.Load(json, nameof(ActorName));
			ActorRepositoryId.Load(json, nameof(ActorRepositoryId));
			ActorOutfitRepositoryId.Load(json, nameof(ActorOutfitRepositoryId));
			ActorType.Load(json, "ActorType", str => str switch {
				"Guard" => EActorType.eAT_Guard,
				"Civilian" => EActorType.eAT_Civilian,
				_ => throw new BingoConfigException("Invalid ActorType - expected 'Guard' or 'Civilian'.")
			});
			ActorWeaponIndex.Load(json, nameof(ActorWeaponIndex));
			ActorHasDisguise.Load(json, nameof(ActorHasDisguise));
			ActorIsAuthorityFigure.Load(json, nameof(ActorIsAuthorityFigure));
			ActorIsDead.Load(json, nameof(ActorIsDead));
			ActorIsFemale.Load(json, nameof(ActorIsFemale));
			ActorIsPacified.Load(json, nameof(ActorIsPacified));
			ActorIsTarget.Load(json, nameof(ActorIsTarget));
			ActorOutfitAllowsWeapons.Load(json, nameof(ActorOutfitAllowsWeapons));
			ActorWeaponUnholstered.Load(json, nameof(ActorWeaponUnholstered));
		}

		public bool Test(EventValue ev, BingoTileState state) {
			return ev is ActorInfoImbuedEventValue v
				&& ActorName.Test(v.ActorName, state)
				&& ActorRepositoryId.Test(v.ActorRepositoryId, state)
				&& ActorOutfitRepositoryId.Test(v.ActorOutfitRepositoryId, state)
				&& ActorType.Test(v.ActorType, state)
				&& ActorWeaponIndex.Test(v.ActorWeaponIndex, state)
				&& ActorHasDisguise.Test(v.ActorHasDisguise, state)
				&& ActorIsAuthorityFigure.Test(v.ActorIsAuthorityFigure, state)
				&& ActorIsDead.Test(v.ActorIsDead, state)
				&& ActorIsFemale.Test(v.ActorIsFemale, state)
				&& ActorIsPacified.Test(v.ActorIsPacified, state)
				&& ActorIsTarget.Test(v.ActorIsTarget, state)
				&& ActorOutfitAllowsWeapons.Test(v.ActorOutfitAllowsWeapons, state)
				&& ActorWeaponUnholstered.Test(v.ActorWeaponUnholstered, state);
		}
	}

	public class BingoTriggerItemInfoImbued : IBingoTrigger {
		readonly BingoTriggerString ItemName = new();
		readonly BingoTriggerUInt ItemInstanceId = new();
		readonly BingoTriggerString ItemRepositoryId = new();
		readonly BingoTriggerString WeaponAnimFrontSide = new();
		readonly BingoTriggerString WeaponAnimBack = new();
		readonly BingoTriggerString IsScopedWeapon = new();
		readonly BingoTriggerEnum<EWeaponAnimationCategory> WeaponAnimationCategory = new();
		readonly BingoTriggerEnum<EWeaponType> WeaponType = new();
		readonly BingoTriggerBool IsCloseCombatWeapon = new();
		readonly BingoTriggerBool IsFiberWire = new();
		readonly BingoTriggerBool IsFirearm = new();
		readonly BingoTriggerString RepositoryItemType = new();
		readonly BingoTriggerString RepositoryItemSize = new();
		readonly BingoTriggerArray<string> RepositoryPerks = new(JsonValueKind.String);

		public BingoTriggerItemInfoImbued(JsonElement json) {
			if (json.ValueKind != JsonValueKind.Object) return;
			ItemName.Load(json, nameof(ItemName));
			ItemInstanceId.Load(json, nameof(ItemInstanceId));
			ItemRepositoryId.Load(json, nameof(ItemRepositoryId));
			WeaponAnimFrontSide.Load(json, nameof(WeaponAnimFrontSide));
			WeaponAnimBack.Load(json, nameof(WeaponAnimBack));
			IsScopedWeapon.Load(json, nameof(IsScopedWeapon));
			WeaponAnimationCategory.Load(json, nameof(WeaponAnimationCategory), ParseWeaponAnimationCategory);
			WeaponType.Load(json, nameof(WeaponType), ParseWeaponType);
			IsCloseCombatWeapon.Load(json, nameof(IsCloseCombatWeapon));
			IsFiberWire.Load(json, nameof(IsFiberWire));
			IsFirearm.Load(json, nameof(IsFirearm));
			RepositoryItemType.Load(json, nameof(RepositoryItemType));
			RepositoryItemSize.Load(json, nameof(RepositoryItemSize));
			RepositoryPerks.Load(json, nameof(RepositoryPerks));
		}

		public bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemInfoImbuedEventValue v
				&& ItemName.Test(v.ItemName, state)
				&& ItemInstanceId.Test(v.ItemInstanceId, state)
				&& ItemRepositoryId.Test(v.ItemRepositoryId, state)
				&& WeaponAnimFrontSide.Test(v.WeaponAnimFrontSide, state)
				&& WeaponAnimBack.Test(v.WeaponAnimBack, state)
				&& IsScopedWeapon.Test(v.IsScopedWeapon, state)
				&& WeaponAnimationCategory.Test(v.WeaponAnimationCategory, state)
				&& WeaponType.Test(v.WeaponType, state)
				&& IsCloseCombatWeapon.Test(v.IsCloseCombatWeapon, state)
				&& IsFiberWire.Test(v.IsFiberWire, state)
				&& IsFirearm.Test(v.IsFirearm, state)
				&& RepositoryItemType.Test(v.RepositoryItemType, state)
				&& RepositoryItemSize.Test(v.RepositoryItemSize, state)
				&& RepositoryPerks.Test(v.RepositoryPerks, state);
		}

		private static EWeaponAnimationCategory ParseWeaponAnimationCategory(string str) {
			return str switch {
				"Pistol" => EWeaponAnimationCategory.eWAC_Pistol,
				"Revolver" => EWeaponAnimationCategory.eWAC_Revolver,
				"SMG_2H" => EWeaponAnimationCategory.eWAC_SMG_2H,
				"SMG_1H" => EWeaponAnimationCategory.eWAC_SMG_1H,
				"Rifle" => EWeaponAnimationCategory.eWAC_Rifle,
				"Sniper" => EWeaponAnimationCategory.eWAC_Sniper,
				"Shotgun_Pump" => EWeaponAnimationCategory.eWAC_Shotgun_Pump,
				"Shotgun_Semi" => EWeaponAnimationCategory.eWAC_Shotgun_Semi,
				_ => throw new BingoConfigException("Invalid WeaponAnimationCategory - expected one of: Pistol, Revolver, SMG_2H, SMG_1H, Rifle, Sniper, Shotgun_Pump, Shotgun_Semi."),
			};
		}

		private static EWeaponType ParseWeaponType(string str) {
			return str switch {
				"Handgun" => EWeaponType.WT_HANDGUN,
				"Slowgun" => EWeaponType.WT_SLOWGUN,
				"AssaultRifle" => EWeaponType.WT_ASSAULTRIFLE,
				"SMG" => EWeaponType.WT_SMG,
				"Sniper" => EWeaponType.WT_SNIPER,
				"Knife" => EWeaponType.WT_KNIFE,
				"RPG" => EWeaponType.WT_RPG,
				"Shotgun" => EWeaponType.WT_SHOTGUN,
				"Spotter" => EWeaponType.WT_SPOTTER,
				_ => throw new BingoConfigException("Invalid WeaponType - expected one of: Handgun, Slowgun, AssaultRifle, SMG, Sniper, Knife, RPG, Shotgun, Spotter."),
			};
		}
	}

	public class BingoTriggerLocationImbued : IBingoTrigger {
		readonly BingoTriggerString Area = new();
		readonly BingoTriggerString ActorArea = new();
		readonly BingoTriggerPosition ActorPosition = new();
		readonly BingoTriggerInt ActorRoom = new();
		readonly BingoTriggerString HeroArea = new();
		readonly BingoTriggerPosition HeroPosition = new();
		readonly BingoTriggerInt HeroRoom = new();
		readonly BingoTriggerString ItemArea = new();
		readonly BingoTriggerPosition ItemPosition = new();
		readonly BingoTriggerInt ItemRoom = new();
		readonly BingoTriggerBool IsCrouching = new();
		readonly BingoTriggerBool IsIdle = new();
		readonly BingoTriggerBool IsRunning = new();
		readonly BingoTriggerBool IsTrespassing = new();
		readonly BingoTriggerBool IsWalking = new();
		readonly BingoTriggerPosition Position = new();
		readonly BingoTriggerInt Room = new();

		public BingoTriggerLocationImbued(JsonElement json) : base() {
			if (json.ValueKind != JsonValueKind.Object) return;
			ActorArea.Load(json, "ActorArea");
			ActorRoom.Load(json, "ActorRoom");
			ActorPosition.Load(json, "ActorPosition");
			Area.Load(json, "Area");
			HeroArea.Load(json, "HeroArea");
			HeroPosition.Load(json, "HeroPosition");
			HeroRoom.Load(json, "HeroRoom");
			ItemArea.Load(json, "ItemArea");
			ItemPosition.Load(json, "ItemPosition");
			ItemRoom.Load(json, "ItemRoom");
			IsCrouching.Load(json, "IsCrouching");
			IsIdle.Load(json, "IsLoad");
			IsRunning.Load(json, "IsRunning");
			IsTrespassing.Load(json, "IsTrepassing");
			IsWalking.Load(json, "IsWalking");
			Room.Load(json, "Room");
			Position.Load(json, "Position");
		}

		public bool Test(EventValue ev, BingoTileState state) {
			return ev is LocationImbuedEventValue v
				&& Area.Test(v.Area, state)
				&& ActorArea.Test(v.ActorArea, state)
				&& ActorPosition.Test(v.ActorPosition, state)
				&& ActorRoom.Test(v.ActorRoom, state)
				&& HeroArea.Test(v.HeroArea, state)
				&& HeroPosition.Test(v.HeroPosition, state)
				&& HeroRoom.Test(v.HeroRoom, state)
				&& ItemArea.Test(v.ItemArea, state)
				&& ItemPosition.Test(v.ItemPosition, state)
				&& ItemRoom.Test(v.ItemRoom, state)
				&& IsCrouching.Test(v.IsCrouching, state)
				&& IsIdle.Test(v.IsIdle, state)
				&& IsRunning.Test(v.IsRunning, state)
				&& IsTrespassing.Test(v.IsTrespassing, state)
				&& IsWalking.Test(v.IsWalking, state)
				&& Position.Test(v.Position, state)
				&& Room.Test(v.Room, state);
		}
	}

	public class BingoTriggerComplication(IBingoTrigger? trigger) : BingoTrigger {
		public readonly IBingoTrigger? Trigger = trigger;

		public override bool Test(EventValue ev, BingoTileState state) {
			return Trigger == null || Trigger.Test(ev, state);
		}

		public override void Advance(BingoTileState state) {
			state.Complete = false;
		}
	}

	public class BingoTriggerNOT(IBingoTrigger? trigger) : IBingoTrigger {
		public readonly IBingoTrigger? Trigger = trigger;

		public bool Test(EventValue ev, BingoTileState state) {
			return Trigger == null || !Trigger.Test(ev, state);
		}
	}

	public class BingoTriggerOR(List<IBingoTrigger?> triggers) : IBingoTrigger {
		public readonly List<IBingoTrigger?> Triggers = triggers;

		public bool Test(EventValue ev, BingoTileState state) {
			var res = Triggers.Any(t => t?.Test(ev, state) ?? true);
			return res;
		}
	}

	public class BingoTriggerAND : BingoTrigger {
		public readonly List<IBingoTrigger?> Triggers;

		public BingoTriggerAND(List<IBingoTrigger?> triggers) {
			Triggers = triggers;
		}

		public BingoTriggerAND(List<IBingoTrigger?> triggers, JsonElement node) : base(node) {
			Triggers = triggers;
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			var res = Triggers.All(t => t?.Test(ev, state) ?? true);
			return res;
		}
	}

	public class BingoTriggerPacify : BingoTrigger {
		readonly BingoTriggerBool Accident = new();
		readonly BingoTriggerBool ActorHasDisguise = new();
		readonly BingoTriggerBool ActorHasSameOutfit = new();
		readonly BingoTriggerString ActorName = new();
		readonly BingoTriggerBool ActorOutfitIsUnique = new();
		readonly BingoTriggerEnum<EOutfitType> ActorOutfitType = new();
		readonly BingoTriggerPosition ActorPosition = new();
		readonly BingoTriggerEnum<EActorType> ActorType = new();
		readonly BingoTriggerArray<string> DamageEvents = new(JsonValueKind.String);
		readonly BingoTriggerInt ExplosionType = new();
		readonly BingoTriggerBool Explosive = new();
		readonly BingoTriggerBool IsFemale = new();
		readonly BingoTriggerBool IsHeadshot = new();
		readonly BingoTriggerBool IsTarget = new();
		readonly BingoTriggerPosition HeroPosition = new();
		readonly BingoTriggerString KillClass = new();
		readonly BingoTriggerString KillContext = new();
		readonly BingoTriggerCIString KillItemRepositoryId = new();
		readonly BingoTriggerString KillItemCategory = new();
		readonly BingoTriggerString KillMethodBroad = new();
		readonly BingoTriggerString KillMethodStrict = new();
		readonly BingoTriggerEnum<EKillType> KillType = new();
		readonly BingoTriggerBool OutfitIsHitmanSuit = new();
		readonly BingoTriggerBool OutfitIsUnique = new();
		readonly BingoTriggerCIString OutfitRepositoryId = new();
		readonly BingoTriggerBool Projectile = new();
		readonly BingoTriggerCIString RepositoryId = new();
		readonly BingoTriggerInt RoomId = new();
		readonly BingoTriggerString SetPieceId = new();
		readonly BingoTriggerString SetPieceType = new();
		readonly BingoTriggerBool Sniper = new();
		readonly BingoTriggerUnique Unique = new();
		readonly BingoTriggerBool WeaponSilenced = new();

		public BingoTriggerPacify(JsonElement json) : base(json) {
			Accident.Load(json, "Accident");
			ActorHasDisguise.Load(json, "ActorHasDisguise");
			ActorHasSameOutfit.Load(json, "ActorHasSameOutfit");
			ActorName.Load(json, "ActorName");
			ActorOutfitIsUnique.Load(json, "ActorOutfitIsUnique");
			ActorOutfitType.Load(json, "OutfitType", str => str switch {
				"Suit" => EOutfitType.eOT_Suit,
				"Guard" => EOutfitType.eOT_Guard,
				"Worker" => EOutfitType.eOT_Worker,
				"Waiter" => EOutfitType.eOT_Waiter,
				"LucasGrey" => EOutfitType.eOT_LucasGrey,
				_ => throw new BingoConfigException("Invalid OutfitType value.")
			});
			ActorPosition.Load(json, nameof(ActorPosition));
			ActorType.Load(json, "ActorType", str => str switch {
				"Guard" => EActorType.eAT_Guard,
				"Civilian" => EActorType.eAT_Civilian,
				_ => throw new BingoConfigException("Invalid ActorType - expected 'Guard' or 'Civilian'.")
			});
			DamageEvents.Load(json, "DamageEvents");
			ExplosionType.Load(json, "ExplosionType");
			Explosive.Load(json, "Explosive");
			HeroPosition.Load(json, nameof(HeroPosition));
			IsHeadshot.Load(json, "IsHeadshot");
			IsFemale.Load(json, "IsFemale");
			IsTarget.Load(json, "IsTarget");
			KillClass.Load(json, "KillClass");
			KillContext.Load(json, "KillContext");
			KillItemRepositoryId.Load(json, "KillItemRepositoryId");
			KillItemCategory.Load(json, "KillItemCategory");
			KillMethodBroad.Load(json, "KillMethodBroad");
			KillMethodStrict.Load(json, "KillMethodStrict"	);
			KillType.Load(json, "KillType", str => str switch {
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
			OutfitIsHitmanSuit.Load(json, "OutfitIsHitmanSuit");
			OutfitIsUnique.Load(json, "OutfitIsUnique");
			OutfitRepositoryId.Load(json, "OutfitRepositoryId");
			Projectile.Load(json, "Projectile");
			RepositoryId.Load(json, "RepositoryId");
			RoomId.Load(json, "RoomId");
			SetPieceId.Load(json, "SetPieceId");
			SetPieceType.Load(json, "SetPieceType");
			Sniper.Load(json, "Sniper");
			Unique.Load(json, "Unique");
			WeaponSilenced.Load(json, "WeaponSilenced");
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			if (ev is PacifyEventValue v && (ev is not KillEventValue || this is BingoTriggerKill)) {
				if (!base.Test(ev, state)) 
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
				if (!ActorPosition.Test(v.ActorPosition, state))
					return false;
				if (!ActorType.Test(v.ActorType, state)) 
					return false;
				if (!DamageEvents.Test(v.DamageEvents, state)) 
					return false;
				if (!ExplosionType.Test(v.ExplosionType, state)) 
					return false;
				if (!Explosive.Test(v.Explosive, state)) 
					return false;
				if (!HeroPosition.Test(v.HeroPosition, state))
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
				if (!OutfitIsUnique.Test(v.OutfitIsUnique, state))
					return false;
				if (!OutfitIsHitmanSuit.Test(v.OutfitIsHitmanSuit, state))
					return false;
				if (!OutfitRepositoryId.Test(v.OutfitRepositoryId, state)) 
					return false;
				if (!Projectile.Test(v.Projectile, state))
					return false;
				if (!RepositoryId.Test(v.RepositoryId, state)) 
					return false;
				if (!RoomId.Test(v.RoomId, state))
					return false;
				if (!SetPieceId.Test(v.SetPieceId, state)) 
					return false;
				if (!SetPieceType.Test(v.SetPieceType, state)) 
					return false;
				if (!Sniper.Test(v.Sniper, state))
					return false;
				if (!WeaponSilenced.Test(v.WeaponSilenced, state))
					return false;
				return Unique.Test(v, state);
			}
			return false;
		}
	}

	public class BingoTriggerKill(JsonElement json) : BingoTriggerPacify(json) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is KillEventValue && base.Test(ev, state);
		}
	}

	public class BingoTriggerActorSick : BingoTrigger {
		readonly BingoTriggerString ActorName = new();
		readonly BingoTriggerCIString ActorRepositoryId = new();
		readonly BingoTriggerBool IsTarget	 = new();
		readonly BingoTriggerString ItemRepositoryId = new();
		readonly BingoTriggerCIString SetPieceRepositoryId = new();
		readonly BingoTriggerEnum<EActorType> ActorType = new();
		readonly BingoTriggerUnique Unique = new();

		public BingoTriggerActorSick(JsonElement json) : base(json) {
			Unique.Load(json, "Unique");
			ActorName.Load(json, "ActorName");
			ActorRepositoryId.Load(json, "ActorRepositoryId");
			ActorType.Load(json, "ActorType", str => str switch {
				"Guard" => EActorType.eAT_Guard,
				"Civilian" => EActorType.eAT_Civilian,
				_ => throw new BingoConfigException("Invalid ActorType - expected 'Guard' or 'Civilian'.")
			});
			IsTarget.Load(json, "IsTarget");
			ItemRepositoryId.Load(json, "ItemRepositoryId");
			SetPieceRepositoryId.Load(json, "SetPieceRepositoryId");
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ActorSickEventValue v
				&& ActorName.Test(v.ActorName, state)
				&& ActorRepositoryId.Test(v.ActorRepositoryId, state)
				&& ActorType.Test(v.ActorType, state)
				&& IsTarget.Test(v.IsTarget, state)
				&& ItemRepositoryId.Test(v.ItemRepositoryId, state)
				&& SetPieceRepositoryId.Test(v.SetpieceRepositoryId, state)
				&& Unique.Test(v, state);
		}
	}

	public class BingoTriggerItemStashed(JsonElement json) : BingoTrigger(json) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemStashedEventValue;
		}
	}

	public class BingoTriggerPlayerShot(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);
		readonly BingoTriggerItemInfoImbued itemInfoTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is PlayerShotEventValue v
				&& locationTrigger.Test(v.Location, state)
				&& itemInfoTrigger.Test(v.Weapon, state);
		}
	}

	public class BingoTriggerCarExploded : BingoTrigger {
		readonly BingoTriggerString CarArea = new();
		readonly BingoTriggerPosition CarPosition = new();
		readonly BingoTriggerInt CarSize = new();

		public BingoTriggerCarExploded(JsonElement json) : base(json) {
			CarArea.Load(json, "CarArea");
			CarPosition.Load(json, "CarPosition");
			CarSize.Load(json, "CarSize");
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is CarExplodedEventValue v
				&& base.Test(ev, state)
				&& CarArea.Test(v.CarArea, state)
				&& CarPosition.Test(v.CarPosition, state)
				&& CarSize.Test(v.CarSize, state);
		}
	}

	public class BingoTriggerDartHit : BingoTrigger {
		readonly BingoTriggerCIString RepositoryId = new();
		readonly BingoTriggerEnum<EActorType> ActorType = new();
		readonly BingoTriggerBool IsTarget = new();
		readonly BingoTriggerBool Blind = new();
		readonly BingoTriggerBool Sedative = new();
		readonly BingoTriggerBool Sick = new();
		readonly BingoTriggerUnique Unique = new();

		public BingoTriggerDartHit(JsonElement json) : base(json) {
			RepositoryId.Load(json, "RepositoryId");
			ActorType.Load(json, "ActorType", str => str switch {
				"Guard" => EActorType.eAT_Guard,
				"Civilian" => EActorType.eAT_Civilian,
				_ => throw new BingoConfigException("Invalid ActorType - expected 'Guard' or 'Civilian'."),
			});
			IsTarget.Load(json, "IsTarget");
			Blind.Load(json, "Blind");
			Sedative.Load(json, "Sedative");
			Sick.Load(json, "Sick");
			Unique.Load(json, "Unique");
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DartHitEventValue v
				&& RepositoryId.Test(v.RepositoryId, state)
				&& IsTarget.Test(v.IsTarget, state)
				&& Blind.Test(v.Blind, state)
				&& Sedative.Test(v.Sedative, state)
				&& Sick.Test(v.Sick, state)
				&& ActorType.Test(v.ActorType, state)
				&& Unique.Test(v, state);
		}
	}

	public class BingoTriggerDoorBroken(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DoorBrokenEventValue v
				&& base.Test(ev, state)
				&& locationTrigger.Test(v, state);
		}
	}

	public class BingoTriggerDoorUnlocked(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DoorUnlockedEventValue v
				&& base.Test(ev, state)
				&& locationTrigger.Test(v, state);
		}
	}

	public class BingoTriggerExplosion(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ExplosionEventValue v
				&& base.Test(ev, state)
				&& locationTrigger.Test(v, state);
		}
	}

	public class BingoTriggerItemThrown : BingoTrigger {
		readonly BingoTriggerString ItemName = new();
		readonly BingoTriggerString ItemType = new();
		readonly BingoTriggerCIString RepositoryId = new();

		public BingoTriggerItemThrown(JsonElement json) : base(json) {
			ItemName.Load(json, nameof(ItemName));
			ItemType.Load(json, nameof(ItemType));
			RepositoryId.Load(json, nameof(RepositoryId));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemThrownEventValue v
				&& ItemName.Test(v.ItemName, state)
				&& ItemType.Test(v.ItemType, state)
				&& RepositoryId.Test(v.RepositoryId, state);
		}
	}

	public class BingoTriggerItemDestroyed : BingoTrigger {
		readonly BingoTriggerCIString RepositoryId = new();
		readonly BingoTriggerLocationImbued locationTrigger;

		public BingoTriggerItemDestroyed(JsonElement json) : base(json) {
			locationTrigger = new(json);
			RepositoryId.Load(json, nameof(RepositoryId));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemDestroyedEventValue v
				&& locationTrigger.Test(ev, state)
				&& RepositoryId.Test(v.RepositoryId, state);
		}
	}

	public class BingoTriggerItemDropped : BingoTrigger {
		readonly BingoTriggerString ItemName = new();
		readonly BingoTriggerString ItemType = new();
		readonly BingoTriggerCIString RepositoryId = new();
		readonly BingoTriggerLocationImbued locationTrigger;

		public BingoTriggerItemDropped(JsonElement json) : base(json) {
			locationTrigger = new(json);
			ItemName.Load(json, nameof(ItemName));
			ItemType.Load(json, nameof(ItemType));
			RepositoryId.Load(json, nameof(RepositoryId));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemDroppedEventValue v
				&& locationTrigger.Test(ev, state)
				&& ItemName.Test(v.ItemName, state)
				&& ItemType.Test(v.ItemType, state)
				&& RepositoryId.Test(v.RepositoryId, state);
		}
	}

	public class BingoTriggerSetpiece : BingoTrigger {
		readonly BingoTriggerCIString RepositoryId = new();
		readonly BingoTriggerString Name = new();
		readonly BingoTriggerString Type = new();
		readonly BingoTriggerString Helper = new();
		readonly BingoTriggerString ItemTriggered = new();
		readonly BingoTriggerString ToolUsed = new();
		readonly BingoTriggerString Position = new();
		readonly BingoTriggerUnique Unique = new();

		public BingoTriggerSetpiece(JsonElement json) : base(json) {
			Name.Load(json, nameof(Name));
			Type.Load(json, nameof(Type));
			Helper.Load(json, nameof(Helper));
			ItemTriggered.Load(json, nameof(ItemTriggered));
			ToolUsed.Load(json, nameof(ToolUsed));
			RepositoryId.Load(json, nameof(RepositoryId));
			Position.Load(json, nameof(Position));
			Unique.Load(json, nameof(Unique));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is SetpiecesEventValue v
				&& Name.Test(v.Name, state)
				&& Type.Test(v.Type, state)
				&& Helper.Test(v.Helper, state)
				&& ItemTriggered.Test(v.ItemTriggered, state)
				&& ToolUsed.Test(v.ToolUsed, state)
				&& RepositoryId.Test(v.RepositoryId, state)
				&& Position.Test(v.Position, state)
				&& Unique.Test(v, state);
		}
	}

	public class BingoTriggerLevelSetupEvent : BingoTrigger {
		readonly BingoTriggerString Event = new();
		readonly BingoTriggerString Location = new();
		readonly BingoTriggerString ContractName = new();
		readonly BingoTriggerUnique Unique = new();

		public BingoTriggerLevelSetupEvent(JsonElement json) : base(json) {
			Event.Load(json, "Event");
			Location.Load(json, "Location");
			ContractName.Load(json, "ContractName");
			Unique.Load(json, "Unique");
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is LevelSetupEventValue v
				&& Event.Test(v.Event_metricvalue, state)
				&& Location.Test(v.Location_MetricValue, state)
				&& ContractName.Test(v.Contract_Name_metricvalue, state)
				&& Unique.Test(v, state);
		}
	}

	public class BingoTriggerBodyBagged : BingoTrigger {
		readonly BingoTriggerCIString RepositoryId = new();

		public BingoTriggerBodyBagged(JsonElement obj) : base(obj) {
			RepositoryId.Load(obj, nameof(RepositoryId));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is BodyBaggedEventValue v
				&& RepositoryId.Test(v.RepositoryId, state);
		}
	}

	public class BingoTriggerBodyFound : BingoTrigger {
		readonly BingoTriggerCIString RepositoryId = new();
		readonly BingoTriggerEnum<EDeathContext> DeathContext = new();
		readonly BingoTriggerEnum<EDeathType> DeathType = new();

		public BingoTriggerBodyFound(JsonElement obj) : base(obj) {
			RepositoryId.Load(obj, "RepositoryId");
			DeathContext.Load(obj, "DeathContext", s => s switch {
				"NotHero" => EDeathContext.eDC_NOT_HERO,
				"Hidden" => EDeathContext.eDC_HIDDEN,
				"Accident" => EDeathContext.eDC_ACCIDENT,
				"Murder" => EDeathContext.eDC_MURDER,
				_ => EDeathContext.eDC_UNDEFINED
			});
			DeathType.Load(obj, "DeathType", s => s switch {
				"Pacify" => EDeathType.eDT_PACIFY,
				"Kill" => EDeathType.eDT_KILL,
				"BloodyKill" => EDeathType.eDT_BLOODY_KILL,
				_ => EDeathType.eDT_UNDEFINED
			});
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is BodyFoundEventValue v
				&& RepositoryId.Test(v.RepositoryId, state)
				&& DeathContext.Test(v.DeathContext, state)
				&& DeathType.Test(v.DeathType, state);
		}
	}

	public class BingoTriggerBodyHidden : BingoTrigger {
		readonly BingoTriggerString ActorName = new();
		readonly BingoTriggerCIString RepositoryId = new();
		readonly BingoTriggerUnique Unique = new();

		public BingoTriggerBodyHidden(JsonElement obj) : base(obj) {
			ActorName.Load(obj, "ActorName");
			RepositoryId.Load(obj, "RepositoryId");
			Unique.Load(obj, "Load");
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is BodyHiddenEventValue v
				&& ActorName.Test(v.ActorName, state)
				&& RepositoryId.Test(v.RepositoryId, state)
				&& Unique.Test(v, state);
		}
	}

	public class BingoTriggerProjectileBodyShot(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ProjectileBodyShotEventValue
				&& base.Test(ev, state)
				&& locationTrigger.Test(ev, state);
		}
	}

	public class BingoTriggerSecuritySystemRecorder : BingoTrigger {
		readonly BingoTriggerString Event = new();
		readonly BingoTriggerInt Camera = new();
		readonly BingoTriggerInt Recorder = new();
		readonly BingoTriggerUnique Unique = new();

		public BingoTriggerSecuritySystemRecorder(JsonElement json) : base(json) {
			if (json.ValueKind != JsonValueKind.Object)
				throw new BingoTileConfigException("Expected object value.");
			Event.Load(json, "Event");
			Camera.Load(json, "Camera");
			Recorder.Load(json, "Recorder");
			Unique.Load(json, "Load");
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			var res = ev is SecuritySystemRecorderEventValue v
				&& Event.Test(v.Event, state)
				&& Camera.Test(v.Camera, state)
				&& Recorder.Test(v.Recorder, state)
				&& Unique.Test(v, state);
			return res;
		}
	}

	public class BingoTriggerCollect : BingoTrigger {
		readonly BingoTriggerCIString Items = new();

		public BingoTriggerCollect(JsonElement json) : base(json) {
			Items.Load(json, "Items");
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is ItemPickedUpEventValue v
				&& base.Test(ev, state)
				&& (v.InstanceId != null && v.InstanceId.Length != 0)
				&& Items.Test(v.RepositoryId, state);
		}
	}

	public class BingoTriggerDisguise : BingoTrigger {
		readonly BingoTriggerEnum<EActorType> ActorType = new();
		readonly BingoTriggerBool IsBundle = new();
		readonly BingoTriggerBool IsSuit = new();
		readonly BingoTriggerBool IsUnique = new();
		readonly BingoTriggerEnum<EOutfitType> OutfitType = new();
		readonly BingoTriggerCIString RepositoryId = new();
		readonly BingoTriggerString Title = new();
		readonly BingoTriggerUnique Unique = new();
		readonly BingoTriggerLocationImbued locationTrigger;

		public BingoTriggerDisguise(JsonElement obj) : base(obj) {
			locationTrigger = new(obj);
			switch (obj.ValueKind) {
				case JsonValueKind.Array:
				case JsonValueKind.String:
					RepositoryId.Load(obj);
					return;
				case JsonValueKind.Object:
					ActorType.Load(obj, "ActorType", s => s switch {
						"Civilian" => EActorType.eAT_Civilian,
						"Guard" => EActorType.eAT_Guard,
						"Hitman" => EActorType.eAT_Hitman,
						_ => throw new BingoConfigException("ActorType must be one of: Civilian, Guard, Hitman")
					});
					IsBundle.Load(obj, "IsBundle");
					IsSuit.Load(obj, "IsSuit");
					IsUnique.Load(obj, "IsUnique");
					OutfitType.Load(obj, "OutfitType", s => s switch {
						"Suit" => EOutfitType.eOT_Suit,
						"Guard" => EOutfitType.eOT_Guard,
						"Worker" => EOutfitType.eOT_Worker,
						"Waiter" => EOutfitType.eOT_Waiter,
						"LucasGrey" => EOutfitType.eOT_LucasGrey,
						_ => throw new BingoConfigException("OutfitType must be one of: Suit, Guard, Worker, Waiter, LucasGrey")
					});
					RepositoryId.Load(obj, "RepositoryId");
					Title.Load(obj, "Title");
					Unique.Load(obj, "Unique");
					return;
				default:
					throw new BingoConfigException("Expected string, object or string array for disguise.");
			}
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DisguiseEventValue v
				&& (ev is not StartingSuitEventValue || this is BingoTriggerStartingSuit)
				&& base.Test(ev, state)
				&& locationTrigger.Test(ev, state)
				&& ActorType.Test(v.ActorType, state)
				&& IsBundle.Test(v.IsBundle, state)
				&& IsSuit.Test(v.IsSuit, state)
				&& IsUnique.Test(v.IsUnique, state)
				&& OutfitType.Test(v.OutfitType, state)
				&& RepositoryId.Test(v.RepositoryId, state)
				&& Title.Test(v.Title, state)
				&& Unique.Test(v, state);
		}
	}

	public class BingoTriggerEnterArea : BingoTrigger {
		readonly BingoTriggerString ID = new();
		readonly BingoTriggerLocationImbued locationTrigger;

		public BingoTriggerEnterArea(JsonElement json) : base(json) {
			locationTrigger = new(json);
			if (json.ValueKind == JsonValueKind.String)
				ID.Load(json);
			else if (json.ValueKind == JsonValueKind.Object)
				ID.Load(json, "ID");
			else throw new BingoConfigException($"Expected string or object with 'ID' property but got {json.ValueKind}.");
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is EnterAreaEventValue v
				&& base.Test(ev, state)
				&& ID.Test(v.Area, state)
				&& locationTrigger.Test(v, state);
		}
	}

	public class BingoTriggerEnterRoom : BingoTrigger {
		readonly BingoTriggerInt Room = new();
		readonly BingoTriggerLocationImbued locationTrigger;

		public BingoTriggerEnterRoom(JsonElement json) : base(json) {
			locationTrigger = new(json);
			switch (json.ValueKind) {
				case JsonValueKind.Array:
				case JsonValueKind.Number:
				case JsonValueKind.String:
					Room.Load(json);
					return;
				case JsonValueKind.Object:
					Room.Load(json, "Room");
					return;
				default:
					throw new BingoConfigException($"Unexpected {json.ValueKind} for room trigger, expected object with 'Room' property.");
			}
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is EnterRoomEventValue v
				&& base.Test(ev, state)
				&& Room.Test(v.Room, state)
				&& locationTrigger.Test(v, state);
		}
	}

	public class BingoTriggerFriskedSuccess(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is FriskedSuccessEventValue
				&& base.Test(ev, state)
				&& locationTrigger.Test(ev, state);
		}
	}

	public class BingoTriggerTrespassing(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is TrespassingEventValue
				&& base.Test(ev, state)
				&& locationTrigger.Test(ev, state);
		}
	}

	public class BingoTriggerStartingSuit(JsonElement json) : BingoTriggerDisguise(json) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is StartingSuitEventValue && base.Test(ev, state);
		}
	}

	public class BingoTriggerOnIsFullyInCrowd(JsonElement json) : BingoTrigger(json) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnIsFullyInCrowdEventValue;
		}
	}

	public class BingoTriggerOnIsFullyInVegetation(JsonElement json) : BingoTrigger(json) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnIsFullyInVegetationEventValue;
		}
	}

	public class BingoTriggerOnDestroy : BingoTrigger {
		readonly BingoTriggerUInt EntityID = new ();
		readonly BingoTriggerBool InitialStateOn = new();
		readonly BingoTriggerLocationImbued location;

		public BingoTriggerOnDestroy(JsonElement json) {
			location = new(json);
			EntityID.Load(json, nameof(EntityID));
			InitialStateOn.Load(json, nameof(InitialStateOn));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnDestroyEventValue v
				&& base.Test(ev, state)
				&& location.Test(ev, state)
				&& EntityID.Test(v.EntityID, state)
				&& InitialStateOn.Test(v.InitialStateOn, state);
		}
	}

	public class BingoTriggerOnEvacuationStarted : BingoTrigger {
		readonly BingoTriggerString ActorName = new();
		readonly BingoTriggerBool IsTarget = new();
		readonly BingoTriggerLocationImbued locationTrigger;
		readonly BingoTriggerActorInfoImbued actorInfoTrigger;

		public BingoTriggerOnEvacuationStarted(JsonElement json) {
			locationTrigger = new(json);
			actorInfoTrigger = new(json);
			ActorName.Load(json, nameof(ActorName));
			IsTarget.Load(json, nameof(IsTarget));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnEvacuationStartedEventValue v
				&& base.Test(ev, state)
				&& locationTrigger.Test(v, state)
				&& actorInfoTrigger.Test(v, state)
				&& ActorName.Test(v.ActorName, state)
				&& IsTarget.Test(v.IsTarget, state);
		}
	}

	public class BingoTriggerOnPickup(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued location = new(json);
		readonly BingoTriggerItemInfoImbued item = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnPickupEventValue v
				&& base.Test(ev, state)
				&& location.Test(v.Location, state)
				&& item.Test(v.Item, state);
		}
	}

	public class BingoTriggerOnTakeDamage(JsonElement json) : BingoTrigger(json) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnTakeDamageEventValue;
		}
	}

	public class BingoTriggerOnTurnOn : BingoTrigger {
		readonly BingoTriggerLocationImbued location;
		readonly BingoTriggerString RepositoryId = new();
		readonly BingoTriggerUInt EntityID = new();
		readonly BingoTriggerBool InitialStateOn = new();

		public BingoTriggerOnTurnOn(JsonElement json) : base(json) {
			location = new(json);
			EntityID.Load(json, nameof(EntityID));
			RepositoryId.Load(json, nameof(RepositoryId));
			InitialStateOn.Load(json, nameof(InitialStateOn));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnTurnOnEventValue v
				&& base.Test(ev, state)
				&& EntityID.Test(v.EntityID, state)
				&& RepositoryId.Test(v.RepositoryId, state)
				&& InitialStateOn.Test(v.InitialStateOn, state);
		}
	}

	public class BingoTriggerOnTurnOff : BingoTrigger {
		readonly BingoTriggerLocationImbued location;
		readonly BingoTriggerString RepositoryId = new();
		readonly BingoTriggerUInt EntityID = new();
		readonly BingoTriggerBool InitialStateOn = new();

		public BingoTriggerOnTurnOff(JsonElement json) : base(json) {
			location = new(json);
			EntityID.Load(json, nameof(EntityID));
			RepositoryId.Load(json, nameof(RepositoryId));
			InitialStateOn.Load(json, nameof(InitialStateOn));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnTurnOffEventValue v
				&& base.Test(ev, state)
				&& EntityID.Test(v.EntityID, state)
				&& RepositoryId.Test(v.RepositoryId, state)
				&& InitialStateOn.Test(v.InitialStateOn, state);
		}
	}

	public class BingoTriggerOnWeaponReload(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);
		readonly BingoTriggerItemInfoImbued itemInfoTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OnWeaponReloadEventValue
				&& base.Test(ev, state)
				&& locationTrigger.Test(ev, state)
				&& itemInfoTrigger.Test(ev, state);
		}
	}

	public class BingoTriggerOpenDoor : BingoTrigger {
		readonly BingoTriggerString Type = new();
		readonly BingoTriggerLocationImbued locationTrigger;

		public BingoTriggerOpenDoor(JsonElement json) : base(json) {
			locationTrigger = new(json);
			Type.Load(json, nameof(Type));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OpenDoorEventValue v
				&& base.Test(ev, state)
				&& locationTrigger.Test(ev, state)
				&& Type.Test(v.Type, state);
		}
	}

	public class BingoTriggerOpportunityEvent : BingoTrigger {
		readonly BingoTriggerString RepositoryId = new();
		readonly BingoTriggerString Event = new();

		public BingoTriggerOpportunityEvent(JsonElement json) : base(json) {
			RepositoryId.Load(json, nameof(RepositoryId));
			Event.Load(json, nameof(Event));
		}

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is OpportunityEventValue v
				&& base.Test(ev, state)
				&& RepositoryId.Test(v.RepositoryId, state)
				&& Event.Test(v.Event, state);
		}
	}

	public class BingoTriggerInstinctActive(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is InstinctActiveEventValue
				&& base.Test(ev, state)
				&& locationTrigger.Test(ev, state);
		}
	}

	public class BingoTriggerDrainPipeClimbed(JsonElement json) : BingoTrigger(json) {
		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DrainPipeClimbedEventValue;
		}
	}

	public class BingoTriggerMovement(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is MovementEventValue
				&& base.Test(ev, state)
				&& locationTrigger.Test(ev, state);
		}
	}

	public class BingoTriggerDragBodyMove(JsonElement json) : BingoTrigger(json) {
		readonly BingoTriggerLocationImbued locationTrigger = new(json);
		readonly BingoTriggerActorInfoImbued actorInfoTrigger = new(json);

		public override bool Test(EventValue ev, BingoTileState state) {
			return ev is DragBodyMoveEventValue v
				&& base.Test(ev, state)
				&& actorInfoTrigger.Test(v.Actor, state)
				&& locationTrigger.Test(v.Location, state);
		}
	}
}
