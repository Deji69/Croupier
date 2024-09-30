using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Croupier
{
	public class RulesetRulesConfig {
		public bool GenericEliminations { get; set; } = false;
		public bool MeleeKillTypes { get; set; } = false;
		public bool ThrownKillTypes { get; set; } = false;
		public bool AnyExplosives { get; set; } = true;
		public bool ImpactExplosives { get; set; } = true;
		public bool RemoteExplosives { get; set; } = true;
		public bool LoudRemoteExplosives { get; set; } = false;
		public bool LiveComplications { get; set; } = true;
		public bool LiveComplicationsExcludeStandard { get; set; } = true;
		public int LiveComplicationChance { get; set; } = 25;
		public int KillTypeChance { get; set; } = 50;
		public bool LoudSMGIsLargeFirearm { get; set; } = true;
		public bool AllowDuplicateDisguise { get; set; } = false;
		public bool AllowDuplicateMethod { get; set; } = false;
		public bool SuitOnly { get; set; } = false;
		public bool AnyDisguise { get; set; } = false;
		public List<string> Banned { get; set; } = ["Slow", "Hard", "Extreme", "Impossible", "Buggy"];
	}

	public class RulesetTargetConfig : Dictionary<string, List<string>> {}

	public class RulesetTagsConfig : Dictionary<string, RulesetTargetConfig> {}

	public class RulesetConfig {
		public string Name { get; set; }
		public RulesetRulesConfig Rules { get; set; }
		public RulesetTagsConfig Tags { get; set; }
	}

	public class Ruleset
	{
		public static Ruleset Current { get; set; }

		public readonly RulesetRulesConfig Rules;
		public readonly RulesetTagsConfig Tags;

		public string Name { get; set; }
		public string Filepath { get; set; }

		private readonly Dictionary<TargetID, List<MethodRule>> targetRules = [];
		private readonly Dictionary<TargetID, List<TargetKillMethodTags>> targetMethodTags = [];
		
		public Ruleset(string name, RulesetRulesConfig rules = null, RulesetTagsConfig tags = null) {
			Name = name;
			Rules = rules ?? new();
			Tags = tags ?? [];
			targetRules = [];
			targetMethodTags = [];

			foreach (var entry in Tags) {
				if (!Target.GetTargetFromKey(entry.Key, out var target))
					continue;

				List<MethodRule> methodRules = [];
				List<TargetKillMethodTags> methodTags = [];
				Dictionary<string, List<string>> rulesConfig = [];

				foreach (var tagEntry in entry.Value) {
					foreach (var cond in tagEntry.Value) {
						if (KillMethod.Parse(cond, out var method)) {
							var methodTag = methodTags.Find(t => t.MatchesKillMethod(method));

							if (methodTag != null) {
								methodTag.tags.Add(tagEntry.Key);
								continue;
							}

							methodTag = method.Type switch {
								KillMethodType.Standard => new(method.Standard, [tagEntry.Key]),
								KillMethodType.Firearm => new(method.Firearm, [tagEntry.Key]),
								KillMethodType.Specific => new(method.Specific, [tagEntry.Key]),
								_ => null,
							};
							if (methodTag != null)
								methodTags.Add(methodTag);

							continue;
						}

						if (rulesConfig.TryGetValue(cond, out var rulesConfigTags)) {
							if (rulesConfigTags.Find(t => t == tagEntry.Key) == null)
								rulesConfigTags.Add(tagEntry.Key);
						}
						else
							rulesConfig[cond] = [tagEntry.Key];
					}
				}

				foreach (var ruleConfig in rulesConfig) {
					Func<Disguise, KillMethod, Mission, bool> func = ruleConfig.Key switch {
						"LoudLive" => (Disguise disguise, KillMethod method, Mission mission) =>
							method.IsLiveFirearm && method.IsLoudWeapon,
						"HostileNonRemote" => (Disguise disguise, KillMethod method, Mission mission) =>
							disguise.Hostile && !method.IsRemote,
						"RemoteExplosive" => (Disguise disguise, KillMethod method, Mission mission) =>
							method.Type == KillMethodType.Firearm && method.Firearm == FirearmKillMethod.Explosive
								&& (method.KillType == KillType.Remote || method.KillType == KillType.LoudRemote),
						"ImpactExplosive" => (Disguise disguise, KillMethod method, Mission mission) =>
							method.Type == KillMethodType.Firearm && method.Firearm == FirearmKillMethod.Explosive && method.KillType == KillType.Impact,
						"ShootCarAsMoses" => (Disguise disguise, KillMethod method, Mission mission) =>
							method.Type == KillMethodType.Specific && method.Specific == SpecificKillMethod.Sierra_ShootCar && disguise.Name == "Moses Lee",
						_ => null,
					};
					if (func == null)
						continue;
					methodRules.Add(new(func, ruleConfig.Value));
				}

				targetRules.Add(target.ID, methodRules);
				targetMethodTags.Add(target.ID, methodTags);
			}
		}

		public List<string> TestRules(TargetID target, Disguise disguise, KillMethod method, Mission mission) {
			if (!targetRules.TryGetValue(target, out var rules))
				return [];

			var broken = new List<string>();
			rules.ForEach(rule => {
				if (rule.Func(disguise, method, mission))
					broken.AddRange(rule.Tags);
			});

			return broken;
		}

		public List<string> GetMethodTags(TargetID target, KillMethod method) {
			if (!targetMethodTags.TryGetValue(target, out var methodTags)) return [];
			var methodTagsList = methodTags?.Find((TargetKillMethodTags methodTags) => {
				return method.Type switch {
					KillMethodType.Standard => method.Standard == methodTags.standard,
					KillMethodType.Specific => method.Specific == methodTags.specific,
					KillMethodType.Firearm => method.Firearm == methodTags.firearm,
					_ => false,
				};
			});
			return methodTagsList?.tags ?? [];
		}

		public RulesetConfig ToConfig() {
			return new() {
				Name = Name,
				Rules = Rules,
				Tags = Tags
			};
		}

		public void Save() {
			var json = JsonSerializer.Serialize(ToConfig(), jsonSerializerOptions);
			File.WriteAllText(Filepath ?? $"rulesets/{Name}.json", json);
		}

		public static Ruleset LoadConfig(string path) {
			var json = File.ReadAllText(path);
			var config = JsonSerializer.Deserialize<RulesetConfig>(json, jsonSerializerOptions);
			return new Ruleset(config.Name, config.Rules, config.Tags) { Filepath = path };
		}

		private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
			AllowTrailingCommas = true,
			WriteIndented = true,
			IncludeFields = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};
	}
}
