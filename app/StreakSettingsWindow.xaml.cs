using System;
using System.ComponentModel;
using System.Windows;

namespace Croupier {
	public class StreakSettingsWindowViewModel : ViewModel {
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
	}
	
	public partial class StreakSettingsWindow : Window {
		public event EventHandler<int> ResetStreak;
		public event EventHandler<int> ResetStreakPB;
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
			ResetStreak?.Invoke(this, 0);
		}

		private void ResetStreakPB_Click(object sender, RoutedEventArgs e) {
			viewModel.StreakPB = 0;
			ResetStreakPB?.Invoke(this, 0);
		}
	}
}
