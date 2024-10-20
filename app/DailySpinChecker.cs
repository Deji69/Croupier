using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Croupier {
	public class DailySpinData {
		public required int id { get; set; }
		public required string date { get; set; }
		public required string spin { get; set; }
	}

	public class DailySpinChecker {
		public static async Task<DailySpinData[]> CheckForDailySpinsAsync() {
			var client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("User-Agent", "Croupier");
			var json = await client.GetStringAsync("https://croupier.nbeatz.net/api/daily");
			return JsonSerializer.Deserialize<DailySpinData[]>(json, jsonSerializerOptions)!;
		}

		private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
			AllowTrailingCommas = true,
			WriteIndented = true,
			IncludeFields = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};
	}
}
