using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace Croupier {
	public class DebugWindowViewModel : ViewModel {
		private string legalSpinText = "";
		private int totalBingoTiles = 0;
		private int totalBingoObjectiveTiles = 0;
		private int totalBingoComplicationTiles = 0;
		private int totalBingoTilesCurrentMap = 0;
		private int totalBingoObjectiveTilesCurrentMap = 0;
		private int totalBingoComplicationTilesCurrentMap = 0;

		public string LegalSpinText {
			get => legalSpinText;
			set {
				legalSpinText = value;
				UpdateProperty(nameof(LegalSpinText));
			}
		}

		public int TotalBingoTiles {
			get => totalBingoTiles;
			set => SetProperty(ref totalBingoTiles, value);
		}

		public int TotalBingoObjectiveTiles {
			get => totalBingoObjectiveTiles;
			set => SetProperty(ref totalBingoObjectiveTiles, value);
		}

		public int TotalBingoComplicationTiles {
			get => totalBingoComplicationTiles;
			set => SetProperty(ref totalBingoComplicationTiles, value);
		}

		public int TotalBingoTilesCurrentMap {
			get => totalBingoTilesCurrentMap;
			set => SetProperty(ref totalBingoTilesCurrentMap, value);
		}

		public int TotalBingoObjectiveTilesCurrentMap {
			get => totalBingoObjectiveTilesCurrentMap;
			set => SetProperty(ref totalBingoObjectiveTilesCurrentMap, value);
		}

		public int TotalBingoComplicationTilesCurrentMap {
			get => totalBingoComplicationTilesCurrentMap;
			set => SetProperty(ref totalBingoComplicationTilesCurrentMap, value);
		}
	}
	
	public partial class DebugWindow : Window {
		protected MainWindow mainWindow;
		private readonly DebugWindowViewModel viewModel = new();

		public DebugWindow(MainWindow main) {
			DataContext = viewModel;
			mainWindow = main;
			InitializeComponent();

			viewModel.PropertyChanged += OnPropertyChanged;
			GameController.Main.Roulette.SpinEdited += OnSpinChanged;

			var bingo = Bingo.Main;
			var mission = GameController.Main.MissionID;
			var enabledTiles = bingo.Tiles.Where(t => !t.Disabled);
			var currentMapTiles = enabledTiles.Where(t => t.Missions.Count == 0 || t.Missions.Contains(mission));
			viewModel.TotalBingoTiles = enabledTiles.Count();
			viewModel.TotalBingoObjectiveTiles = enabledTiles.Count(t => t.Type == BingoTileType.Objective);
			viewModel.TotalBingoComplicationTiles = enabledTiles.Count(t => t.Type == BingoTileType.Complication);
			viewModel.TotalBingoTilesCurrentMap = currentMapTiles.Count();
			viewModel.TotalBingoObjectiveTilesCurrentMap = currentMapTiles.Count(t => t.Type == BingoTileType.Objective);
			viewModel.TotalBingoComplicationTilesCurrentMap = currentMapTiles.Count(t => t.Type == BingoTileType.Complication);
		}

		private void OnSpinChanged(object? sender, Spin? e) {
			viewModel.LegalSpinText = "";
			if (e == null) return;
			if (e.IsLegal())
				viewModel.LegalSpinText = "Spin is Legal";
			else
				viewModel.LegalSpinText = "Spin is Illegal";
		}

		private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
			Config.Save();
		}

		private void StartLoad_Click(object sender, RoutedEventArgs e) {
			CroupierSocketServer.SpoofMessage("LoadStarted:0");
		}

		private void StopLoad_Click(object sender, RoutedEventArgs e) {
			CroupierSocketServer.SpoofMessage("LoadFinished:0");
		}

		private void Attempt_Click(object sender, RoutedEventArgs e) {
			CroupierSocketServer.SpoofMessage($"MissionStart:bc531204-8e82-4550-b2a7-829b047dc6cc\t[]");
		}

		private void WinSpin_Click(object sender, RoutedEventArgs e) {
			CroupierSocketServer.SpoofMessage($"MissionComplete:1\t{(double)(Random.Shared.Next(50 * 10, 400 * 10)) / 10.0}");
		}

		private void RIPSpin_Click(object sender, RoutedEventArgs e) {
			CroupierSocketServer.SpoofMessage("MissionFailed:1");
		}

		private void CopyBingoTexts_Click(object sender, RoutedEventArgs e) {
			var tiles = Bingo.Main.GetTilesForMission(GameController.Main.Bingo.Mission);
			var str = "";
			foreach (var tile in tiles) {
				if (str.Length > 0) str += ", ";
				str += tile.ToString();
			}
			for (var i = 0; i < 10; ++i) {
				try {
					Clipboard.SetText($"{Mission.Get(GameController.Main.Bingo.Mission).Name}: {str}");
					MessageBox.Show(
						"Tile texts copied!",
						"Copy Bingo Debug Texts"
					);
					return;
				}
				catch {
				}
				System.Threading.Thread.Sleep(10);
			}
			MessageBox.Show(
				"Clipboard access failed. Another process may have attempted to access the clipboard at the same time. Please try again.",
				"Copy Bingo Debug Texts Failed"
			);
		}

		private void ValidKills_Click(object sender, RoutedEventArgs e) {
			var msg = "";
			foreach (var cond in GameController.Main.Roulette.Spin?.Conditions ?? []) {
				if (msg.Length > 0) msg += ",";
				msg += $"{cond.Target.Initials}:2:1:{cond.Target.Initials}";
			}

			CroupierSocketServer.SpoofMessage($"KillValidation:{msg}");
		}

		private void AutoSpin_Click(object sender, RoutedEventArgs e) {
			var mission = Mission.Get(Mission.GetRandomMajorMissionID());
			if (mission != null)
				CroupierSocketServer.SpoofMessage($"AutoSpin:{mission.Codename}");
		}
	}
}
