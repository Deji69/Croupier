using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Croupier
{
	public partial class KillMethodTags(string name, StringCollection? tags) {
		public string Name { get; set; } = WhitespaceRegex().Replace(name, "");
		public StringCollection Tags { get; set; } = tags ?? [];

		public bool MatchesKillMethod(KillMethod km) {
			if (km is KillMethodVariant v)
				return Name == WhitespaceRegex().Replace(v.Name + v.Method.Name, "")
					|| Name == WhitespaceRegex().Replace(v.Method.Name, "");
			return WhitespaceRegex().Replace(km.Name, "") == Name;
		}

		[GeneratedRegex("\\s")]
		private static partial Regex WhitespaceRegex();
	}

	public class RulesetRule(Func<Disguise, KillMethod, Mission, KillComplication, bool> func, StringCollection? tags = null) {
		public Func<Disguise, KillMethod, Mission, KillComplication, bool> Func { get; private set; } = func;
		public StringCollection Tags { get; private set; } = tags ?? [];
	}

	public class RulesetTargetTags(List<KillMethodTags> tags, List<RulesetRule> rules) {
		public List<KillMethodTags> Tags { get; private set; } = tags;
		public List<RulesetRule> Rules { get; private set; } = rules;
	}

	public class Ruleset(string name, RulesetRules rules, Dictionary<string, RulesetTargetTags> tags)
	{
		public static Ruleset? Current { get; set; }

		public string Name { get; set; } = name;
		public RulesetRules Rules { get; private set; } = rules;
		public Dictionary<string, RulesetTargetTags> Tags { get; private set; } = tags;

		public StringCollection GetMethodTags(Target target, KillMethod method) {
			if (!Tags.TryGetValue(target.Initials, out var tags))
				return method.Tags;
			var methodTags = tags.Tags.FirstOrDefault(t => t.MatchesKillMethod(method));
			return methodTags != null ? [..method.Tags, ..methodTags.Tags] : method.Tags;
		}

		public StringCollection TestRules(Target target, Disguise disguise, KillMethod method, Mission mission, KillComplication complication) {
			if (!Tags.TryGetValue(target.Initials, out var rules))
				return [];
			StringCollection broken = [];
			foreach (var rule in rules.Rules) {
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

		public static Ruleset FromJson(Roulette roulette, JsonNode json) {
			var name = json["Name"]?.GetValue<string>() ?? throw new Exception("Missing property 'Name'.");
			var rulesJson = json["Rules"]?.AsObject() ?? throw new Exception("Missing property 'Rules'.");
			var tagsJson = json["Tags"]?.AsObject() ?? [];
			var rules = new RulesetRules();
			Dictionary<string, RulesetTargetTags> rulesetTags = [];

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

				List<KillMethodTags> methodTags = [];
				List<RulesetRule> tagRules = [];
				Dictionary<string, StringCollection> rulesConfig = [];

				foreach (var (tag, entry) in v.AsObject()) {
					if (tag == null) continue;
					if (entry == null) continue;
					foreach (var cond in entry.AsArray()) {
						if (cond == null) continue;
						if (cond.GetValueKind() == JsonValueKind.Object) {
							var condJson = cond.AsObject();
							var methodName = condJson["Method"]?.GetValue<string>();
							if (methodName == null) continue;
							StringCollection disguises = [];
							foreach (var disguise in condJson["Disguises"]?.AsArray() ?? []) {
								if (disguise == null) continue;
								disguises.Add(disguise.GetValue<string>());
							}
							tagRules.Add(new(
								(Disguise d, KillMethod k, Mission m, KillComplication c) => k.Name == methodName && disguises.Contains(d.Name),
								[tag]
							));
							continue;
						}

						var condStr = cond.GetValue<string>();
						var method = SpinKillMethod.Parse(roulette, condStr, target);
						if (method != null) {
							var methodTag = methodTags.FirstOrDefault(t => t.MatchesKillMethod(method.Method));
							if (methodTag != null) {
								methodTag.Tags.Add(tag);
								continue;
							}

							methodTags.Add(new(method.Method.Name, [tag]));
							continue;
						}

						if (rulesConfig.TryGetValue(condStr, out var res)) {
							if (!res.Contains(tag)) res.Add(tag);
						}
						else rulesConfig.Add(condStr, [tag]);
					}
				}

				foreach (var (key, tags) in rulesConfig) {
					Func<Disguise, KillMethod, Mission, KillComplication, bool>? func = key switch {
						"LoudLive" => (Disguise d, KillMethod k, Mission m, KillComplication c) => c == KillComplication.Live && k.IsLoud && k.IsFirearm,
						"HostileNonRemote" => (Disguise d, KillMethod k, Mission m, KillComplication c) => d.Hostile && !k.IsRemote,
						"RemoteExplosive" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsExplosive && k.IsRemote,
						"ImpactExplosive" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsExplosive && k.IsImpact,
						"Explosive" => (Disguise d, KillMethod k, Mission m, KillComplication c) => k.IsExplosive,
						_ => null,
					};
					if (func == null) continue;
					tagRules.Add(new(func, tags));
				}

				rulesetTags.Add(k, new(methodTags, tagRules));
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
