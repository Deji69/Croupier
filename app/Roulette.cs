using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;

namespace Croupier {
	public interface IKillMethodTypeConfig {
		public string Name { get; set; }
		public string Image { get; set; }
		public List<string> Tags { get; set; }
	}

	public interface IKillMethodConfig {
		public string Name { get; set; }
		public string Image { get; set; }
		public string Category { get; set; }
		public List<string> Tags { get; set; }
		public List<string> Keywords { get; set; }
	}

	public class Roulette {
		public static readonly Roulette Main = new();

		public List<KillMethod> KillMethods { get; } = [];
		public List<KillMethod> StandardMethods { get; } = [];
		public List<KillMethod> WeaponMethods { get; } = [];
		public List<KillMethod> UniqueMethods { get; } = [];
		public Dictionary<string, List<object>> MethodKeywordMap { get; private set; } = new() {
			{ "live", [KillComplication.Live] },
			{ "ntk", [KillComplication.Live] },
			{ "ntp", [KillComplication.Live] },
			{ "ntko", [KillComplication.Live] },
			{ "noko", [KillComplication.Live] },
			{ "nko", [KillComplication.Live] },
			{ "nonko", [KillComplication.Live] },
			{ "notargetko", [KillComplication.Live] },
			{ "notargetpacification", [KillComplication.Live] },
			{ "nopacification", [KillComplication.Live] },
			{ "notargetpacif", [KillComplication.Live] },
			{ "nopacif", [KillComplication.Live] },
			{ "sil", [KillType.Silenced] },
			{ "silence", [KillType.Silenced] },
			{ "silenced", [KillType.Silenced] },
			{ "ld", [KillType.Loud] },
			{ "loud", [KillType.Loud] },
			{ "melee", [KillType.Melee] },
			{ "mel", [KillType.Melee] },
			{ "throw", [KillType.Thrown] },
			{ "thrown", [KillType.Thrown] },
			{ "remote", [KillType.Remote] },
			{ "rem", [KillType.Remote] },
			{ "impact", [KillType.Impact] },
			{ "imp", [KillType.Impact] },
			{ "loudremote", [KillType.LoudRemote] },
			{ "ldremote", [KillType.LoudRemote] },
			{ "loudrem", [KillType.LoudRemote] },
			{ "ldrem", [KillType.LoudRemote] },
			{ "any", [KillType.Any] },
		};
		public List<Mission> Missions { get; } = [];

		public IEnumerable<KillMethod> GetUniqueMethods(Target target) {
			return UniqueMethods.Where(m => m.Target == target.Name);
		}

		public Mission GetRandomMission() {
			if (Missions.Count < 1) throw new Exception("No missions loaded.");
			return Missions[Random.Shared.Next(Missions.Count)];
		}

		public object? GetByKeyword(string kw, Target? target = null) {
			if (!MethodKeywordMap.TryGetValue(kw, out var result))
				return null;
			var items = result.Where(i => i is not KillMethod m || m.Category != KillMethodCategory.Unique || (target != null && target.Name == m.Target));
			var numItems = items.Count();
			if (numItems == 0) return null;
			if (numItems == 1) return items.First();
			var unique = items.FirstOrDefault(i => i is KillMethod m && m.Category == KillMethodCategory.Unique);
			if (unique != null && target != null) return unique;
			return items.First();
		}

		public Target? GetTargetByInitials(string initials) {
			foreach (var m in Missions) {
				var t = m.Targets.FirstOrDefault(t => t.Initials == initials);
				t ??= m.SpecificTargets.FirstOrDefault(t => t.Initials == initials);
				if (t != null) return  t;
			}
			return null;
		}

		public void LoadKillMethodsFromFile(string file) {
			var json = JsonNode.Parse(File.ReadAllText(file)) ?? throw new Exception($"Failed to parse JSON file {file}.");
			LoadKillMethodsFromJson(json);
		}

		public void LoadKillMethodsFromJson(JsonNode json) {
			var jsonArray = json.AsArray();
			foreach (var item in jsonArray) {
				var obj = item?.AsObject() ?? throw new Exception($"Invalid kill method entry.");
				var km = KillMethod.FromJson(obj);

				switch (km.Category) {
					case KillMethodCategory.Standard:
						StandardMethods.Add(km);
						break;
					case KillMethodCategory.Weapon:
						WeaponMethods.Add(km);
						break;
					case KillMethodCategory.Melee:
						km.Variants.Add(new(km, "Melee", null, ["IsMelee", "IsLive"]));
						km.Variants.Add(new(km, "Thrown", null, ["IsThrown"]));
						break;
					case KillMethodCategory.Unique:
						UniqueMethods.Add(km);
						break;
				}

				foreach (var kw in km.Keywords) {
					if (kw == null) continue;
					if (MethodKeywordMap.TryGetValue(kw, out var list))
						list.Add(km);
					else
						MethodKeywordMap.Add(kw, [km]);
				}

				KillMethods.Add(km);
			}
		}

		public Mission LoadMissionFromFile(string file) {
			var json = JsonNode.Parse(File.ReadAllText(file)) ?? throw new Exception($"Failed to parse JSON file {file}.");
			return LoadMissionFromJson(json);
		}

		public Mission LoadMissionFromJson(JsonNode json) {
			var mission = Mission.LoadMissionFromJson(json, KillMethods);
			Missions.Add(mission);
			return mission;
		}


		public void Load() {
			LoadKillMethodsFromFile("config/kill-methods.json");
			foreach (var file in Directory.GetFiles("config/missions", "*.json", SearchOption.TopDirectoryOnly))
				LoadMissionFromFile(file);
			Mission.All.Sort();
		}


		public static Roulette Get() {
			var r = new Roulette();
			r.Load();
			return r;
		}

		public Generator CreateGenerator(Ruleset ruleset) {
			if (StandardMethods.Count == 0) throw new Exception("No Standard kill methods loaded.");
			if (WeaponMethods.Count == 0) throw new Exception("No Weapon kill methods loaded.");
			return new(ruleset, StandardMethods, WeaponMethods, UniqueMethods, Missions);
		}

		private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
			AllowTrailingCommas = true,
			WriteIndented = true,
			IncludeFields = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};
	}
}