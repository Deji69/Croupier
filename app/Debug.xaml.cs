using System;
using System.ComponentModel;
using System.Windows;

namespace Croupier {
	public class DebugWindowViewModel : ViewModel {
		private string legalSpinText = "";

		public string LegalSpinText {
			get => legalSpinText;
			set {
				legalSpinText = value;
				UpdateProperty(nameof(LegalSpinText));
			}
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
			main.SpinChanged += OnSpinChanged;
		}

		private void OnSpinChanged(object? sender, Spin e) {
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

		private void ValidKills_Click(object sender, RoutedEventArgs e) {
			var msg = "";
			foreach (var cond in mainWindow.CurrentSpin?.Conditions ?? []) {
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
