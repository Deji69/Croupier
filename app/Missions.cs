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
	public class MissionComboBoxItem {
		public MissionID ID { get; set; }
		public required string Name { get; set; }
		public string? Location { get; set; }
		public Uri? Image { get; set; }
		public bool IsSeparator { get; set; }
	}

	public partial class Mission(string name, string image, string location, MissionGroup group, string codename, bool major = false, StringCollection? keywords = null) : IComparable<Mission> {
		public static readonly List<Mission> All = [];

		public MissionID ID => MissionIDMethods.FromName(Name);

		public string Name { get; set; } = name;
		public string Location { get; set; } = location;
		public MissionGroup Group { get; set; } = group;
		public string Codename { get; set; } = codename;
		public string Image { get; private set; } = image;
		public StringCollection Keywords { get; private set; } = keywords ?? [];
		public bool Major { get; set; } = major;
		public Uri ImagePath {
			get => new(Path.Combine(Environment.CurrentDirectory, "missions", this.Image));
		}

		public List<Target> Targets { get; private set; } = [];
		public List<Target> SpecificTargets { get; private set; } = [];
		public List<MissionKillMethod> Methods { get; private set; } = [];

		public List<Disguise> Disguises { get; private set; } = [];

		public Disguise? SuitDisguise => Disguises.FirstOrDefault(d => d.Suit);

		public static Mission Get(MissionID id) => All.First(m => m.ID == id);
		public static Mission? TryGet(MissionID id) => All.FirstOrDefault(m => m.ID == id);

		public static MissionID GetRandomMissionID() => All[random.Next(All.Count)].ID;

		public static MissionID GetRandomMajorMissionID() {
			var missions = All.Where(m => m.Major).ToList();
			return missions[random.Next(missions.Count)].ID;
		}

		public static Mission LoadMissionFromJson(JsonNode json, List<KillMethod> methods) {
			var name = json["Name"]?.GetValue<string>() ?? throw new Exception("Config error: missing Name.");
			var image = json["Image"]?.GetValue<string>() ?? throw new Exception("Config error: missing Image.");
			var location = json["Location"]?.GetValue<string>() ?? throw new Exception("Config error: missing Location.");
			var group = MissionGroupMethods.FromName(json["Group"]?.GetValue<string>() ?? throw new Exception("Config error: missing Group."));
			var major = json["IsMajor"]?.GetValue<bool>() ?? false;
			var codename = json["Codename"]?.GetValue<string>() ?? "";
			var jsonTargets = json["Targets"]?.AsArray() ?? throw new Exception("Config error: missing Targets.");
			var jsonSpecificTargets = json["SpecificTargets"]?.AsArray() ?? [];
			var jsonDisguises = json["Disguises"]?.AsArray() ?? [];
			var jsonKillMethods = json["KillMethods"]?.AsArray() ?? [];

			StringCollection keywords = [];
			foreach (var kw in json["Keywords"]?.AsArray() ?? []) {
				if (kw == null) continue;
				keywords.Add(kw.GetValue<string>().ToLower().RemoveDiacritics());
			}

			var mission = new Mission(name, image, location, group, codename, major, keywords);
			
			foreach (var target in jsonTargets) {
				if (target == null) continue;
				mission.Targets.Add(Target.FromJson(target, mission));
			}

			foreach (var target in jsonSpecificTargets) {
				if (target == null)
					continue;
				mission.SpecificTargets.Add(Target.FromJson(target, mission));
			}

			foreach (var km in jsonKillMethods) {
				if (km == null) continue;

				StringCollection tags = [];
				string? methodName = null;

				if (km.GetValueKind() == JsonValueKind.String)
					methodName = km.GetValue<string>();
				else {
					foreach (var (prop, value) in km.AsObject()) {
						if (value == null) continue;
						switch (prop) {
							case "Name":
								methodName = value.GetValue<string>();
								break;
							case "Tags":
								foreach (var tag in value.AsArray()) {
									if (tag == null) continue;
									tags.Add(tag.GetValue<string>());
								}
								break;
						}
					}
				}

				if (methodName == null) continue;

				var killMethod = methods.FirstOrDefault(a => a.Name == methodName);
				if (killMethod == null) continue;

				mission.Methods.Add(new MissionKillMethod(mission, killMethod, tags));
			}

			foreach (var d in jsonDisguises) {
				if (d == null) continue;
				var disguiseName = d["Name"]?.GetValue<string>();
				var disguiseImage = d["Image"]?.GetValue<string>();
				var suit = d["Suit"]?.GetValue<bool>() ?? false;
				var hostile = d["Hostile"]?.GetValue<bool>() ?? false;
				StringCollection kws = [];
				
				if (disguiseName == null) continue;
				if (disguiseImage == null) continue;
				
				foreach (var kw in d["Keywords"]?.AsArray() ?? []) {
					if (kw == null) continue;
					kws.Add(kw.GetValue<string>()); 
				}

				mission.Disguises.Add(new(mission, disguiseName, disguiseImage, suit, false, hostile, kws));
			}

			All.Add(mission);
			return mission;
		}

		public int CompareTo(Mission? other) {
			if (other == null) return 0;
			return ID.GetGroupOrder() - other.ID.GetGroupOrder();
		}

		private static readonly Random random = new();
	}
}
