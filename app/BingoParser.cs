using Croupier.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Croupier {
	public class BingoParser {
		public static BingoParser? Main { get; private set; }

		private readonly Dictionary<string, List<Mission>> missionKeywordMap = [];
		private readonly Dictionary<string, List<BingoTile>> bingoTileMap = [];


		public BingoParser(Bingo bingo, Roulette roulette) {
			foreach (var tile in bingo.Tiles) {
				if (bingoTileMap.TryGetValue(tile.Key!.ToLower(), out var tiles))
					tiles.Add(tile);
				else
					bingoTileMap.Add(tile.Key!.ToLower(), [tile]);
			}

			foreach (var mission in roulette.Missions) {
				AddMissionKeywords(mission);
			}
		}

		private List<Mission> GetMissionListForKeyword(string keyword) {
			if (missionKeywordMap.TryGetValue(keyword, out var missions))
				return missions;
			List<Mission> list = [];
			missionKeywordMap.Add(keyword, list);
			return list;
		}

		private void AddMissionKeyword(string keyword, Mission mission) {
			var list = GetMissionListForKeyword(keyword);
			if (!list.Contains(mission))
				list.Add(mission);
		}

		private void AddMissionKeywords(Mission mission) {
			foreach (var kwd in mission.Keywords) {
				if (kwd == null) continue;
				AddMissionKeyword(kwd, mission);
			}
		}
		

		public BingoCard Parse(string input) {
			input = input.RemoveDiacritics();
			
			// Check for a mission keyword separated by colon before the tile list
			var mission = MissionID.NONE;
			var colonSplitInput = input.Split(":", 2, StringSplitOptions.TrimEntries);
			
			if (colonSplitInput.Length > 1 && !colonSplitInput[0].Contains(',')) {
				input = colonSplitInput.Last();
				var missionTokens = Strings.TokenCharacterRegex.Replace(colonSplitInput.First(), "").ToLower();
				var missionTokenFreqs = AnalyseMapTokenFrequency(missionKeywordMap, [missionTokens], 3);
				if (missionTokenFreqs.Count > 0) {
					missionTokenFreqs.Sort();
					mission = missionTokenFreqs.First().Item.ID;
				}
			}

			var tokens = ProcessInput(input.ToLower());
			var card = new BingoCard(mission);

			foreach (var token in tokens) {
				var tok = token;
				var colonSplitToken = tok.Split(":", 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
				
				if (colonSplitToken.Length > 1)
					tok = colonSplitToken.First();

				if (!bingoTileMap.TryGetValue(tok, out var matchingTiles) || matchingTiles.Count == 0)
					throw new ParserException($"No matching tile found for token \"{tok}\".");
				
				var missionMatchingTiles = card.Mission != MissionID.NONE ? matchingTiles
					: matchingTiles.Where(mt => (mt.Missions.Count == 0 || mt.Missions.Contains(card.Mission)) && (mt.ExcludeMissions.Count == 0 || !mt.ExcludeMissions.Contains(card.Mission)));
				var tile = missionMatchingTiles.Any() ? missionMatchingTiles.First() : matchingTiles.First();
				
				if (card.Mission == MissionID.NONE && tile.Missions.Count == 1)
					card.Mission = mission;
				
				if (colonSplitToken.Length > 1 && int.TryParse(colonSplitToken[1], out var value))
					card.Add(tile, value);
				else
					card.Add(tile);
			}

			return card;
		}

		public static BingoParser Get() {
			return Main ??= new(Bingo.Main, Roulette.Main);
		}

		public static bool TryParse(string input, out BingoCard? card) {
			try {
				var parser = Get();
				card = parser.Parse(input);
			} catch (Exception) {
				card = null;
				return false;
			}
			return true;
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
			return input.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
