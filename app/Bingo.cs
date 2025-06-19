using Croupier.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;

namespace Croupier {
	public class BingoGroup(string name) {
		public string Name { get; set; } = name;
	}

	public class Bingo {
		public static readonly Bingo Main = new();

		public List<BingoTile> Tiles { get; } = [];

		public void Load() {
			Tiles.Clear();
			foreach (var file in Directory.GetFiles("config/bingo", "*.json", SearchOption.TopDirectoryOnly))
				LoadTilesFromFile(file);
		}

		public void LoadTilesFromFile(string file) {
			try {
				var json = JsonNode.Parse(File.ReadAllText(file)) ?? throw new BingoTileConfigException($"Failed to parse JSON file {file}.");
				LoadTilesFromJson(json);
			}
			catch (BingoConfigException e) {
				MessageBox.Show($"File: {file}\nException: {e.Message}", "Config Error - Croupier", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		public void LoadTilesFromJson(JsonNode json) {
			var jsonArray = json.AsArray();
			foreach (var item in jsonArray) {
				var obj = item?.AsObject() ?? throw new BingoTileConfigException($"Invalid bingo tile entry.");
				var tile = BingoTile.FromJson(obj);
				Tiles.Add(tile);
			}
		}
	}
}
