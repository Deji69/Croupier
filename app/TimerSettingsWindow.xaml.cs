using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Croupier {
	public class TimerSettingsWindowViewModel : ViewModel {
		private MissionID resetMission = MissionID.NONE;
		private int autoSpinCountdown = 0;

		public MissionID ResetMission {
			get => resetMission;
			set => SetProperty(ref resetMission, value);
		}

		public int AutoSpinCountdown {
			get => autoSpinCountdown;
			set {
				SetProperty(ref autoSpinCountdown, value);
				UpdateProperty(nameof(AutoSpinCountdownStatus));
			}
		}

		public string AutoSpinCountdownStatus {
			get => "Adjust the slider to set a delay on auto spins after entering the planning screen, to give time for pre-planning.\n" + (autoSpinCountdown == 0
				? "Spins will generate instantly on planning screen detection."
				: $"Spins will generate {autoSpinCountdown} second{(autoSpinCountdown > 1 ? "s" : "")} after planning screen detection.");
		}
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

		public int ReplanWindow {
			get => Config.Default.StreakReplanWindow;
			set {
				Config.Default.StreakReplanWindow = value;
				UpdateProperty(nameof(ReplanWindow));
			}
		}
	}
	
	public partial class TimerSettingsWindow : Window {
		public event EventHandler<int> ResetStreak;
		public event EventHandler<int> ResetStreakPB;

		private readonly TimerSettingsWindowViewModel viewModel = new();
		private readonly List<Mission> missions = [
			new Mission(MissionID.NONE),
			new Mission(MissionID.ICAFACILITY_FREEFORM),
			new Mission(MissionID.PARIS_SHOWSTOPPER),
			new Mission(MissionID.HAWKESBAY_NIGHTCALL),
			new Mission(MissionID.MIAMI_FINISHLINE),
			new Mission(MissionID.DUBAI_ONTOPOFTHEWORLD),
		];

		private ObservableCollection<MissionComboBoxItem> ResetMissionListItems {
			get {
				var items = new ObservableCollection<MissionComboBoxItem>();
				var group = MissionGroup.None;
				missions.ForEach(mission => {
					if (mission.Group != group) {
						items.Add(new() {
							Name = mission.Group.GetName(),
							IsSeparator = true,
						});
						group = mission.Group;
					}
					items.Add(new() {
						ID = mission.ID,
						Name = mission.ID != MissionID.NONE ? mission.Name : "(NONE)",
						Location = mission.Location,
						IsSeparator = false
					});
				});
				return items;
			}
		}

		public TimerSettingsWindow() {
			DataContext = viewModel;
			InitializeComponent();

			viewModel.PropertyChanged += OnPropertyChanged;
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			ResetMissionSelect.ItemsSource = ResetMissionListItems;
			var idx = ResetMissionListItems.ToList().FindIndex(item => item.ID == Config.Default.TimerResetMission);
			ResetMissionSelect.SelectedIndex = idx != -1 ? idx : 0;
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			Config.Default.TimerResetMission = viewModel.ResetMission;
			Config.Default.AutoSpinCountdown = viewModel.AutoSpinCountdown;
			Config.Save();
		}

		private void ResetOnMissionSelect_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var items = e.AddedItems;
			if (items.Count == 0) return;
			var item = (MissionComboBoxItem)items[0];
			viewModel.ResetMission = item.ID;
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
