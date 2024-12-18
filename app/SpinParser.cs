using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Automation.Peers;

namespace Croupier {
	public class MapTokenFrequency() : IComparable<MapTokenFrequency> {
		public required Mission Item { get; set; }
		public int Frequency { get; set; } = 0;

		public int CompareTo(MapTokenFrequency? other) {
			if (other == null) return 0;
			if (Frequency < other.Frequency) return 1;
			if (Frequency == other.Frequency) return 0;
			return -1;
		}
	}

	public class ParseContext() {
		public KillComplication Complication { get; set; } = KillComplication.None;
		public KillType KillType { get; set; } = KillType.Any;
		public Target? Target { get; set; } = null;
		public Target? AutoTarget { get; set; } = null;
		public KillMethod? Method { get; set; } = null;
		public string? MethodToken { get; set; } = null;
		public Disguise? Disguise { get; set; } = null;
	}

	public class ConditionKeywords(HashSet<MissionID>? missions = null, Dictionary<string, string>? keywords = null) {
		public readonly HashSet<MissionID> missions = missions ?? [];
		public readonly Dictionary<string, string> keywords = keywords ?? [];
	}

	public class SpinParser {
		public static readonly List<string> IgnoreKeywords = ["in", "with", "target", "using", "eliminate", "wear"];
		public static readonly List<string> SuitKeywords = ["suit"];
		public static readonly List<string> AnyDisguiseKeywords = ["anydisg", "anydisguise"];
		public static SpinParser? Main { get; private set; }

		private readonly Dictionary<string, List<Target>> targetKeywordMap = [];
		private readonly Dictionary<string, List<Disguise>> disguiseKeywordMap = [];
		private readonly Dictionary<string, KillMethod> methodKeywordMap = [];
		private readonly Dictionary<string, List<MissionKillMethod>> missionMethodKeywordMap = [];
		private readonly Dictionary<string, List<Mission>> missionIdentifyingKeywordMap = [];
		private KillMethod? neckSnap = null;
		private Roulette roulette;

		public static readonly Dictionary<string, string> Substitutions = new() {
			{"bg", "bodyguard"},
			{"bgd", "bodyguard"},
			{"bdgd", "bodyguard"},
			{"bdygd", "bodyguard"},
			{"bdygrd", "bodyguard"},
			{"bgrd", "bodyguard"},
			{"bdgrd", "bodyguard"},
			{"cocoa", "coca"},
			{"drvr", "driver"},
			{"mech", "mechanic"},
			{"shiek", "sheikh"},
			{"sheik", "sheikh"},
			{"shiekh", "sheikh"},
			{"shake", "sheikh"},
			{"vamp", "vampire"},
			{"dracula", "vampire"},
			{"hippy", "hippie"},
			{"bicyclist", "cyclist"},
			{"gardner", "gardener"},
			{"guardener", "gardener"},
			{"housekpr", "housekeeper"},
			{"housekper", "housekeeper"},
			{"housekeper", "housekeeper"},
			{"housekeep", "housekeeper"},
			{"groundkeeper", "groundskeeper"},
			{"miltia", "militia"},
			{"kron", "kronstadt"},
			{"kronst", "kronstadt"},
			{"kronstad", "kronstadt"},
			{"kronstat", "kronstadt"},
			{"krondstat", "kronstadt"},
			{"krondstadt", "kronstadt"},
			{"bollywd", "bollywood"},
			{"bllywd", "bollywood"},
			{"blywd", "bollywood"},
			{"bollywod", "bollywood"},
			{"bolly", "bollywood"},
			{"servent", "servant"},
			{"armour", "armor"},
			{"famos", "famous"},
			{"maintenence", "maintenance"},
			{"maintainance", "maintenance"},
			{"maintainence", "maintenance"},
			{"fac", "facility"},
			{"facil", "facility"},
			{"analist", "analyst"},
			{"guacho", "gaucho"},
			{"somelier", "sommelier"},
			{"sacraficial", "sacrificial"},
			{"millitary", "military"},
			{"prison", "prisoner"},
		};
		
		public static readonly Dictionary<string, KillComplication> ComplicationKeywords = new() {
			{"live", KillComplication.Live},
			{"nko", KillComplication.Live},
			{"noko", KillComplication.Live},
			{"nonko", KillComplication.Live},
			{"ntko", KillComplication.Live},
			{"notargetko", KillComplication.Live},
			{"notargetpacification", KillComplication.Live},
			{"nopacification", KillComplication.Live},
			{"nopacify", KillComplication.Live},
			{"notargetknockout", KillComplication.Live},
			{"noknockout", KillComplication.Live},
			{"notargetpacify", KillComplication.Live},
			{"donotko", KillComplication.Live},
			{"donotpacify", KillComplication.Live},
			{"nonpacify", KillComplication.Live},
			{"nonpacification", KillComplication.Live},
		};
		public static readonly Dictionary<string, KillType> KillTypeKeywords = new() {
			{"loud", KillType.Loud},
			{"ld", KillType.Loud},
			{"silenced", KillType.Silenced},
			{"silence", KillType.Silenced},
			{"sil", KillType.Silenced},
			{"silent", KillType.Silenced},
			{"melee", KillType.Melee},
			{"mel", KillType.Melee},
			{"thrown", KillType.Thrown},
			{"throw", KillType.Thrown},
			{"remote", KillType.Remote},
			{"impact", KillType.Impact},
			{"imp", KillType.Impact},
			{"rem", KillType.Remote},
			{"loudremote", KillType.LoudRemote},
			{"ldremote", KillType.LoudRemote},
			{"loudrem", KillType.LoudRemote},
			{"ldrem", KillType.LoudRemote},
		};

		public SpinParser(Roulette roulette) {
			this.roulette = roulette;

			AddMethodKeywords(roulette.WeaponMethods);
			AddMethodKeywords(roulette.StandardMethods);

			foreach (var mission in roulette.Missions) {
				AddTargetKeywords(mission);
				AddDisguiseKeywords(mission);
				AddMissionMethodKeywords(mission);
			}
		}

		private List<Target> GetTargetListForKeyword(string keyword) {
			if (targetKeywordMap.TryGetValue(keyword, out var targets))
				return targets;
			List<Target> list = [];
			targetKeywordMap.Add(keyword, list);
			return list;
		}

		private List<Mission> GetMissionListForKeyword(string keyword) {
			if (missionIdentifyingKeywordMap.TryGetValue(keyword, out var missions))
				return missions;
			List<Mission> list = [];
			missionIdentifyingKeywordMap.Add(keyword, list);
			return list;
		}

		private List<Disguise> GetDisguiseListForKeyword(string keyword) {
			if (disguiseKeywordMap.TryGetValue(keyword, out var disguises))
				return disguises;
			List<Disguise> list = [];
			disguiseKeywordMap.Add(keyword, list);
			return list;
		}

		private List<MissionKillMethod> GetMissionMethodListForKeyword(string keyword) {
			if (missionMethodKeywordMap.TryGetValue(keyword, out var methods))
				return methods;
			List<MissionKillMethod> list = [];
			missionMethodKeywordMap.Add(keyword, list);
			return list;
		}

		private void AddMissionIdentifyingKeyword(string keyword, Mission mission) {
			var list = GetMissionListForKeyword(keyword);
			if (!list.Contains(mission))
				list.Add(mission);
		}

		private void AddTargetKeywords(Mission mission) {
			foreach (var target in mission.Targets) {
				foreach (var keyword in target.Keywords) {
					if (keyword == null) continue;
					GetTargetListForKeyword(keyword).Add(target);
					AddMissionIdentifyingKeyword(keyword, mission);
				}
			}
		}

		private void AddDisguiseKeyword(string keyword, Disguise disguise) {
			var list = GetDisguiseListForKeyword(keyword);
			if (!list.Contains(disguise))
				list.Add(disguise);
		}

		private void AddDisguiseKeywords(Mission mission) {
			foreach (var disguise in mission.Disguises) {
				foreach (var keyword in disguise.Keywords) {
					if (keyword == null) continue;
					AddDisguiseKeyword(keyword, disguise);
					AddMissionIdentifyingKeyword(keyword, mission);
				}
			}
		}

		private void AddMissionMethodKeyword(string keyword, MissionKillMethod method) {
			var list = GetMissionMethodListForKeyword(keyword);
			if (!list.Contains(method))
				list.Add(method);
		}

		private void AddMissionMethodKeywords(Mission mission) {
			foreach (var method in mission.Methods) {
				if (neckSnap == null && method.Name == "Neck Snap")
					neckSnap = method;

				foreach (var keyword in method.Keywords) {
					if (keyword == null) continue;
					AddMissionMethodKeyword(keyword, method);
					AddMissionIdentifyingKeyword(keyword, mission);
				}
			}
			foreach (var target in mission.Targets) {
				var uniqueMethods = roulette.GetUniqueMethods(target);
				foreach (var method in uniqueMethods) {
					foreach (var keyword in method.Keywords) {
						if (keyword == null) continue;
						AddMissionMethodKeyword(keyword, new(mission, method, []));
						AddMissionIdentifyingKeyword(keyword, mission);
					}
				}
			}
		}

		private void AddMethodKeywords(List<KillMethod> methods) {
			foreach (var method in methods) {
				foreach (var keyword in method.Keywords) {
					if (keyword == null)
						continue;
					methodKeywordMap.Add(keyword, method);
				}
			}
		}
		

		public Spin Parse(string input) {
			var tokens = ProcessInput(input).Where(t => !IgnoreKeywords.Contains(t)).ToArray();

			var missionTokenFreqs = AnalyseMapTokenFrequency(missionIdentifyingKeywordMap, tokens, 3);
			missionTokenFreqs.Sort();
			var mostEligibleMissions = missionTokenFreqs.Take(3).ToList();
			if (mostEligibleMissions.Count == 0) throw new Exception("Could not detect mission in spin.");

			var missionHint = mostEligibleMissions.First().Item;
			List<ParseContext> contexts = [];
			ParseContext? context = null;

			var anyDisguise = missionHint.Disguises.FirstOrDefault(d => d.Any);
			var suitDisguise = missionHint.Disguises.FirstOrDefault(d => d.Suit);

			var j = 1;
			var maxLength = 4;
			var numTokens = tokens.Length;
			var targets = missionHint.Targets.ToList();

			for (var i = 0; i < numTokens; i += j > 0 ? j : 1) {
				var maxTokens = maxLength > (numTokens - i) ? numTokens - i : maxLength;

				for (j = maxTokens; j >= 1; --j) {
					var token = "";
					
					for (var k = 0; k < j; ++k)
						token += tokens[k + i];

					context ??= new();

					var isSuit = suitDisguise != null && SuitKeywords.Contains(token);
					var isAnyDisguise = anyDisguise != null && AnyDisguiseKeywords.Contains(token);
					var complication = context.Complication == KillComplication.None ? ParseComplication(token) : null;
					var killType = context.KillType == KillType.Any ? ParseKillType(token) : null;

					if (!isSuit && !isAnyDisguise && context.Target == null && targetKeywordMap.TryGetValue(token, out var targetList)) {
						foreach (var target in targetList) {
							if (target.Mission == missionHint) {
								context.Target = target;
								break;
							}
						}

						context.Target ??= targetList.First();
						break;
					}
					else if (complication != null) {
						context.Complication = complication.Value;
						break;
					}
					else if (killType != null) {
						context.KillType = killType.Value;
						break;
					}

					var disguises = disguiseKeywordMap.GetValueOrDefault(token);

					if (context.Disguise == null && (isSuit || isAnyDisguise || disguises != null)) {
						if (isSuit) context.Disguise = suitDisguise;
						else if (isAnyDisguise) context.Disguise = anyDisguise;
						else if (disguises != null) context.Disguise = disguises.FirstOrDefault(d => d.Mission == missionHint);
						break;
					}
					else if (context.Method == null) {
						if (methodKeywordMap.TryGetValue(token, out var method)) {
							context.Method = method;
							context.MethodToken = token;
							break;
						}
						else if (missionMethodKeywordMap.TryGetValue(token, out var methods)) {
							context.Method = methods.FirstOrDefault(m => m.Mission == missionHint);
							context.MethodToken = token;
							break;
						}
					}
				}

				if (context == null) continue;

				var firstTarget = targets.FirstOrDefault();

				if (context.Method != null && context.Disguise != null && firstTarget != null && firstTarget.Generic) {
					targets.RemoveAt(0);
				}

				if ((context.Target != null || context.AutoTarget != null) && context.Method != null && context.Disguise != null) {
					contexts.Add(context);
					context = null;
				}
			}

			if (contexts.Count < missionHint.Targets.Count) {
				if (context?.Method != null && context.Disguise != null && context.Target == null) {
					while (missionHint != null && missionHint.Targets.Count > 1) {
						missionHint = mostEligibleMissions.FirstOrDefault()?.Item;
						if (missionHint != null)
							mostEligibleMissions.RemoveAt(0);
					}
					if (missionHint != null) {
						context.Target = missionHint.Targets.FirstOrDefault();
						contexts.Add(context);
					}
				}
				else if (context?.Target != null) {
					context.Method ??= neckSnap;
					context.Disguise ??= missionHint.Disguises.First(d => d.Suit);
					contexts.Add(context);
				}
			}

			var spin = new Spin();

			foreach (var target in missionHint?.Targets ?? []) {
				var ctx = contexts.First(c => c.Target == target);
				KillMethod km = ctx.Method!;

				var uniqueMethods = roulette.GetUniqueMethods(target);
				if (uniqueMethods.Any() && ctx.MethodToken != null) {
					var uniqueMethod = uniqueMethods.FirstOrDefault(m => m.Keywords.Contains(ctx.MethodToken));
					if (uniqueMethod != null) km = uniqueMethod;
				}

				if (ctx.KillType != KillType.Any)
					km = km.GetVariantMatchingKillType(ctx.KillType) ?? km;

				spin.Conditions.Add(new(target, ctx.Disguise!, new(km, ctx.Complication)));
			}

			return spin;
		}

		public static SpinParser Get() {
			return Main ??= new(Roulette.Main);
		}

		public static bool TryParse(string input, out Spin? spin) {
			try {
				var parser = Get();
				spin = parser.Parse(input);
			} catch (Exception) {
				spin = null;
				return false;
			}
			return true;
		}

		public static KillComplication? ParseComplication(string token) {
			return token switch {
				"live" or "nko" or "noko" or "nonko" or "ntko" or "notargetko" or
				"notargetpacification" or "nopacification" or "nopacify" or
				"notargetknockout" or "noknockout" or "notargetpacify" or
				"donotko" or "donotpacify" or "nonpacify" or "nonpacification" or
				"elimination" or "elim"
					=> KillComplication.Live,
				_ => null,
			};
		}

		public static KillType? ParseKillType(string token) {
			return token switch {
				"loud" or "ld" => KillType.Loud,
				"s" or "silenced" or "silence" or" sil" or "silent" => KillType.Silenced,
				"melee" or "mel" => KillType.Melee,
				"thrown" or "throw" => KillType.Thrown,
				"remote" or "rem" => KillType.Remote,
				"impact" or "imp" => KillType.Impact,
				"loudremote" or "ldremote" or "loudrem" or "ldrem" => KillType.LoudRemote,
				_ => null
			};
		}

		private static List<MapTokenFrequency> AnalyseMapTokenFrequency(Dictionary<string, List<Mission>> map, string[] tokens, int maxLength = 4) {
			List<MapTokenFrequency> results = [];
			var numTokens = tokens.Length;
			var j = 1;

			for (var i = 0; i < numTokens; i += j > 0 ? j : 1) {
				var maxTokens = maxLength > (numTokens - i) ? numTokens - i : maxLength;
				for (j = maxTokens; j >= 1; --j) {
					var token = "";
					for (var k = 0; k < j; ++k)
						token += tokens[k + i];

					var items = map.GetValueOrDefault(token);
					if (items == null || items.Count == 0)
						continue;
					var uniqueHit = items.Count == 1;

					foreach (var item in items) {
						var keys = FindAllIndexes(results, (r, k) => r.Item == item);
						var pts = uniqueHit ? 2 : 1;

						foreach (var key in keys) {
							results[key].Frequency += pts;
						}

						if (keys.Count == 0)
							results.Add(new MapTokenFrequency() { Item = item, Frequency = pts });
					}

					break;
				}
			}
			return results;
		}

		private static List<int> FindAllIndexes(List<MapTokenFrequency> items, Func<MapTokenFrequency, int, bool> fn) {
			List<int> keys = [];
			for (var i = 0; i < items.Count; ++i) {
				if (fn(items[i], i))
					keys.Add(i);
			}
			return keys;
		}

		private static string[] ProcessInput(string input) {
			input = Strings.TokenCharacterRegex.Replace(input.RemoveDiacritics().ToLower(), " ");
			return input.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
