using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
		public List<KillMethod> KillMethods { get; } = [];
		public List<KillMethod> StandardMethods { get; } = [];
		public List<KillMethod> WeaponMethods { get; } = [];
		public List<KillMethod> UniqueMethods { get; } = [];
		public Dictionary<string, List<KillMethod>> MethodKeywordMap { get; } = [];
		public Dictionary<string, List<KillType>> KillTypeKeywordMap { get; } = [];
		public Dictionary<string, List<KillComplication>> KillComplicationKeywordMap { get; } = [];
		public List<Mission> Missions { get; } = [];

		public List<KillMethod> LoadKillMethodsFromFile(string file) {

			var json = File.ReadAllText(file);
			var config = JsonSerializer.Deserialize<List<RulesetConfig>>(json, jsonSerializerOptions);
			return [];
		}

		public static Roulette Load() {
			var r = new Roulette();
			r.LoadKillMethodsFromFile("rulesets/kill-methods.json");
			return r;
		}

		private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
			AllowTrailingCommas = true,
			WriteIndented = true,
			IncludeFields = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};
	}
}