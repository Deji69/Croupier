using System;
using System.ComponentModel;
using System.Windows;

namespace Croupier {
	public class DebugWindowViewModel : ViewModel {
	}
	
	public partial class DebugWindow : Window {
		protected MainWindow mainWindow;
		private readonly StreakSettingsWindowViewModel viewModel = new();

		public DebugWindow(MainWindow main) {
			mainWindow = main;
			DataContext = viewModel;
			InitializeComponent();

			viewModel.PropertyChanged += OnPropertyChanged;
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			Config.Save();
		}

		private void StartLoad_Click(object sender, RoutedEventArgs e) {
			CroupierSocketServer.SpoofMessage("LoadStarted:0");
		}

		private void StopLoad_Click(object sender, RoutedEventArgs e) {
			CroupierSocketServer.SpoofMessage("LoadFinished:0");
		}

		private void WinSpin_Click(object sender, RoutedEventArgs e) {
			CroupierSocketServer.SpoofMessage("MissionComplete:1");
		}

		private void RIPSpin_Click(object sender, RoutedEventArgs e) {
			CroupierSocketServer.SpoofMessage("MissionFailed:1");
		}

		private void AutoSpin_Click(object sender, RoutedEventArgs e) {
			if (Mission.GetMissionCodename(Mission.GetRandomMainMissionID(), out var codename))
				CroupierSocketServer.SpoofMessage($"AutoSpin:{codename}");
		}
	}
}
