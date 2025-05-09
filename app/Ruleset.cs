using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Croupier
{
	public class RulesetRule(Func<Disguise, KillMethod, Mission, KillComplication, bool> func, StringCollection tags) {
		public Func<Disguise, KillMethod, Mission, KillComplication, bool> Func { get; private set; } = func;
		public StringCollection Tags { get; private set; } = tags ?? [];
	}

	public class Ruleset(string name, RulesetRules rules, Dictionary<string, List<RulesetRule>> tags)
	{
		public static Ruleset? Current { get; set; }

		public string Name { get; set; } = name;
		public RulesetRules Rules { get; private set; } = rules;
		public Dictionary<string, List<RulesetRule>> Tags { get; private set; } = tags;

		public StringCollection TestRules(Target target, Disguise disguise, KillMethod method, Mission mission, KillComplication complication) {
			if (!Tags.TryGetValue(target.Initials, out var rules) || rules == null)
				return [];
			StringCollection broken = [];
			foreach (var rule in rules) {
				if (rule.Func(disguise, method, mission, complication))
					broken.AddRange([..rule.Tags]);
			}
			return broken;
		}

		public bool AreAnyOfTheseTagsBanned(StringCollection tags) {
			if (tags.Count == 0 || Rules.Banned.Count == 0)
				return false;
			foreach (var tag in tags) {
				if (tag != null && Rules.Banned.Contains(tag))
					return true;
			}
			return false;
		}

		private static Func<Disguise, KillMethod, Mission, KillComplication, bool> GetRuleFunc(string? key) {
			return key switch {
				"Live" => (Disguise d, KillMethod k, Mission m, KillComplication c) => c == KillComplication.Live,
				"LoudLive" => (Disguise d, KillMethod k, Mission m, KillComplication c) => c == KillComplication.Live && k.IsLoud && k.IsFirearm,
				"IsLoud" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsLoud,
				"IsSilenced" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsSilenced,
				"OnlyLoud" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsLoud,
				"OnlySilenced" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsSilenced,
				"HostileNonRemote" => (Disguise d, KillMethod k, Mission m, KillComplication c) => d.Hostile && !k.IsRemote,
				"RemoteExplosive" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsExplosive && k.IsRemote,
				"ImpactExplosive" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsExplosive && k.IsImpact,
				"IsExplosive" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsExplosive,
				_ => (Disguise d, KillMethod k, Mission m, KillComplication c) => key == null || k.Name == key || k.Tags.Contains(key),
			};
		}

		public static Ruleset FromJson(Roulette roulette, JsonNode json) {
			var name = json["Name"]?.GetValue<string>() ?? throw new Exception("Missing property 'Name'.");
			var rulesJson = json["Rules"]?.AsObject() ?? throw new Exception("Missing property 'Rules'.");
			var tagsJson = json["Tags"]?.AsObject() ?? [];
			var rules = new RulesetRules();
			Dictionary<string, List<RulesetRule>> rulesetTags = [];

			foreach (var (k, v) in rulesJson) {
				if (k == null) continue;
				if (v == null) continue;
				var prop = rules.GetType().GetProperty(k);
				if (prop == null) continue;
				if (prop.PropertyType == typeof(string))
					prop.SetValue(rules, v.GetValue<string>());
				else if (prop.PropertyType == typeof(int))
					prop.SetValue(rules, v.GetValue<int>());
				else if (prop.PropertyType == typeof(bool))
					prop.SetValue(rules, v.GetValue<bool>());
				else if (prop.PropertyType == typeof(List<string>)) {
					List<string> items = [];
					foreach (var item in v.AsArray()) {
						if (item == null) continue;
						items.Add(item.GetValue<string>());
					}
					prop.SetValue(rules, items);
				}
			}

			foreach (var (k, v) in tagsJson) {
				if (k == null) continue;
				if (v == null) continue;
				var target = roulette.GetTargetByInitials(k);
				if (target == null) continue;

				List<RulesetRule> tagRules = [];
				Dictionary<string, StringCollection> rulesConfig = [];

				foreach (var (tag, entry) in v.AsObject()) {
					if (tag == null) continue;
					if (entry == null) continue;
					foreach (var cond in entry.AsArray()) {
						if (cond == null) continue;
						if (cond.GetValueKind() == JsonValueKind.Object) {
							var condJson = cond.AsObject();
							var ruleFn = GetRuleFunc(condJson["Method"]?.GetValue<string>());
							StringCollection disguises = [];
							foreach (var disguise in condJson["Disguises"]?.AsArray() ?? []) {
								if (disguise == null) continue;
								disguises.Add(disguise.GetValue<string>());
							}
							if (disguises.Count == 0)
								continue;
							tagRules.Add(new(
								(Disguise d, KillMethod k, Mission m, KillComplication c) =>
									ruleFn(d, k, m, c)
									&& (
										disguises.Count == 0
										|| disguises.Contains(d.Name)
									),
								[tag]
							));
						} else if (cond.GetValueKind() == JsonValueKind.String) {
							var condStr = cond.GetValue<string>();
							var disguise = target.Mission?.Disguises.Find(d => d.Name == condStr);
							var ruleFn = disguise == null ? GetRuleFunc(condStr) : (Disguise d, KillMethod k, Mission m, KillComplication c) => d.Name == disguise.Name;
							tagRules.Add(new(ruleFn, [tag]));
						}
					}
				}

				rulesetTags.Add(k, tagRules);
			}
			
			return new(name, rules, rulesetTags);
		}

		public static Ruleset LoadFromFile(Roulette roulette, string path) {
			var json = JsonNode.Parse(File.ReadAllText(path), null, new() {
				AllowTrailingCommas = true,
				CommentHandling = JsonCommentHandling.Skip
			}) ?? throw new Exception($"Unable to read file '{path}'.");
			return FromJson(roulette, json);
		}
	}
}
