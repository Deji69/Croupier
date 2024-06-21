using System.ComponentModel;
using System.Windows;

namespace Croupier {
	public class StreakSettingsWindowViewModel : ViewModel {
		private int autoSpinCountdown = 0;

		public int StreakCurrent {
			get => Config.Default.StreakCurrent;
			set {
				Config.Default.StreakCurrent = 0;
				UpdateProperty(nameof(StreakCurrent));
			}
		}

		public int StreakPB {
			get => Config.Default.StreakPB;
			set {
				Config.Default.StreakPB = 0;
				UpdateProperty(nameof(StreakPB));
			}
		}

		public int AutoSpinCountdown {
			get => autoSpinCountdown;
			set {
				SetProperty(ref autoSpinCountdown, value);
			}
		}
	}
	
	public partial class StreakSettingsWindow : Window {
		private readonly StreakSettingsWindowViewModel viewModel = new();

		public StreakSettingsWindow() {
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

		private void ResetCurrentStreak_Click(object sender, RoutedEventArgs e) {
			viewModel.StreakCurrent = 0;
		}

		private void ResetStreakPB_Click(object sender, RoutedEventArgs e) {
			viewModel.StreakPB = 0;
		}
	}
}
