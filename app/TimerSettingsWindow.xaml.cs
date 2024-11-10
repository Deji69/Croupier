using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Croupier {
	public enum TimingMode {
		RTA,
		IGT,
		LRT,
		Spin,
	}

	public class TimerSettingsWindowViewModel : ViewModel {
		private MissionID resetMission = MissionID.NONE;
		private TimingMode timingMode = TimingMode.LRT;
		private int autoSpinCountdown = 0;

		public MissionID ResetMission {
			get => resetMission;
			set => SetProperty(ref resetMission, value);
		}

		public TimingMode TimingMode {
			get => timingMode;
			set => SetProperty(ref timingMode, value);
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

		public bool StreakRequireValidKills {
			get => Config.Default.StreakRequireValidKills;
			set {
				Config.Default.StreakRequireValidKills = value;
				UpdateProperty(nameof(StreakRequireValidKills));
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
	public class TimingModeComboBoxItem {
		public TimingMode TimingMode { get; set; }
		public required string Name { get; set; }
	}

	public partial class TimerSettingsWindow : Window {
		public event EventHandler<int>? ResetStreak;
		public event EventHandler<int>? ResetStreakPB;

		private readonly TimerSettingsWindowViewModel viewModel = new();
		private readonly List<MissionID> missions = [
			MissionID.NONE,
			MissionID.ICAFACILITY_FREEFORM,
			MissionID.PARIS_SHOWSTOPPER,
			MissionID.HAWKESBAY_NIGHTCALL,
			MissionID.MIAMI_FINISHLINE,
			MissionID.DUBAI_ONTOPOFTHEWORLD,
		];

		private ObservableCollection<TimingModeComboBoxItem> TimingModeItems {
			get => [
				new() { Name = "LRT (Real Time, Loads Removed)", TimingMode = TimingMode.LRT },
				new() { Name = "RTA (Real Time, Loads Included)", TimingMode = TimingMode.RTA },
				new() { Name = "IGT (In-Game Time, Accumulative)", TimingMode = TimingMode.IGT },
				new() { Name = "Spin (Real Time, Loads + Post-Mission Removed)", TimingMode = TimingMode.Spin },
			];
		}

		private ObservableCollection<MissionComboBoxItem> ResetMissionListItems {
			get {
				var items = new ObservableCollection<MissionComboBoxItem>();
				var group = MissionGroup.None;
				missions.ForEach(id => {
					if (id == MissionID.NONE) {
						items.Add(new() {
							ID = MissionID.NONE,
							Name = "(NONE)",
							Location = "",
							IsSeparator = false
						});
						return;
					}
					var mission = Mission.Get(id);
					if (mission.Group != group) {
						items.Add(new() {
							Name = mission.Group.GetName(),
							IsSeparator = true,
						});
						group = mission.Group;
					}
					items.Add(new() {
						ID = mission.ID,
						Name = mission.Name,
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
			{
				TimingModeSelect.ItemsSource = TimingModeItems;
				var idx = TimingModeItems.ToList().FindIndex(item => item.TimingMode == Config.Default.TimingMode);
				TimingModeSelect.SelectedIndex = idx != -1 ? idx : 0;
			}
			{
				ResetMissionSelect.ItemsSource = ResetMissionListItems;
				var idx = ResetMissionListItems.ToList().FindIndex(item => item.ID == Config.Default.TimerResetMission);
				ResetMissionSelect.SelectedIndex = idx != -1 ? idx : 0;
			}
		}

		private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
			Config.Default.TimerResetMission = viewModel.ResetMission;
			Config.Default.AutoSpinCountdown = viewModel.AutoSpinCountdown;
			Config.Default.TimingMode = viewModel.TimingMode;
			Config.Save();
		}

		private void ResetOnMissionSelect_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
			var items = e.AddedItems;
			if (items.Count == 0) return;
			var item = (MissionComboBoxItem)items[0]!;
			viewModel.ResetMission = item.ID;
		}

		private void TimingModeSelect_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
			var items = e.AddedItems;
			if (items.Count == 0)
				return;
			var item = (TimingModeComboBoxItem)items[0]!;
			viewModel.TimingMode = item.TimingMode;
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
