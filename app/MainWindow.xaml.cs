using Croupier.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace Croupier
{
	public class GameModeEntry(GameMode mode, string name) : INotifyPropertyChanged {
		public GameMode Mode { get; set; } = mode;
		public int Index => (int)Mode;
		public string Name { get; set; } = name;
		public bool IsSelected {
			get => Config.Default.Mode == Mode;
			set {
				Config.Default.Mode = Mode;
				OnPropertyChanged(nameof(IsSelected));
			}
        }

        public void Refresh() {
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(Name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

	public class TargetNameFormatEntry(TargetNameFormat id, string name) : INotifyPropertyChanged {
		public TargetNameFormat ID { get; set; } = id;
		public int Index => (int)ID;
		public string Name { get; set; } = name;
		public bool IsSelected {
			get => TargetNameFormatMethods.FromString(Config.Default.TargetNameFormat) == ID;
			set {
				Config.Default.TargetNameFormat = value.ToString();
				OnPropertyChanged(nameof(IsSelected));
			}
		}

		public void Refresh() {
			OnPropertyChanged(nameof(IsSelected));
			OnPropertyChanged(nameof(Name));
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class ContextSubmenuEntry(string name, int index) : INotifyPropertyChanged
	{
		private string _name = name;
		private bool isSelected = false;
		private int _index = index;

		public string Name {
			get => _name;
			set {
				_name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public int Index {
			get => _index;
			set {
				_index = value;
				OnPropertyChanged(nameof(Index));
			}
		}
		public bool IsSelected {
			get => isSelected;
			set {
				isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged {

		private const int MAX_HISTORY_ENTRIES = 30;
		public event PropertyChangedEventHandler? PropertyChanged;

		public static readonly RoutedUICommand CheckUpdateCommand = new("Check Update", "CheckUpdate", typeof(MainWindow));
		public static readonly RoutedUICommand CheckDailySpinsCommand = new("Check Daily Spins", "CheckDailySpins", typeof(MainWindow));
		public static readonly RoutedUICommand DailySpin1Command = new("Daily Spin 1", "DailySpin1", typeof(MainWindow));
		public static readonly RoutedUICommand DailySpin2Command = new("Daily Spin 2", "DailySpin2", typeof(MainWindow));
		public static readonly RoutedUICommand DailySpin3Command = new("Daily Spin 3", "DailySpin3", typeof(MainWindow));
		public static readonly RoutedUICommand CopySpinCommand = new("Copy Spin", "CopySpin", typeof(MainWindow), [
			new KeyGesture(Key.C, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand CopySpinLinkCommand = new("Copy Spin Link", "CopySpinLink", typeof(MainWindow));
		public static readonly RoutedUICommand PasteSpinCommand = new("Paste Spin", "PasteSpin", typeof(MainWindow), [
			new KeyGesture(Key.V, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand EditMapPoolCommand = new("Edit Map Pool", "EditMapPool", typeof(MainWindow), [
			new KeyGesture(Key.M, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand ShowHitmapsWindowCommand = new("Show Hitmaps Window", "ShowHitmapsWindow", typeof(MainWindow), [
			new KeyGesture(Key.H, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand ShowLiveSplitWindowCommand = new("Show LiveSplit Window", "ShowLiveSplitWindow", typeof(MainWindow), [
			new KeyGesture(Key.L, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand DebugWindowCommand = new("Debug Window", "DebugWindow", typeof(MainWindow), [
			new KeyGesture(Key.D, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand EditRulesetsCommand = new("Edit Rulesets", "EditRulesets", typeof(MainWindow), [
			new KeyGesture(Key.R, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand EditHotkeysCommand = new("Edit Hotkeys", "EditHotkeys", typeof(MainWindow));
		public static readonly RoutedUICommand ShowStatisticsWindowCommand = new("Show Statistics Window", "ShowStatisticsWindow", typeof(MainWindow), [
			new KeyGesture(Key.S, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand StreakSettingsCommand = new("Streak Settings", "StreakSettings", typeof(MainWindow), [
			new KeyGesture(Key.T, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand ResetStreakCommand = new("Reset Streak", "ResetStreak", typeof(MainWindow), [
			new KeyGesture(Key.S, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand TimerSettingsCommand = new("Timer Settings", "TimerSettings", typeof(MainWindow), [
			new KeyGesture(Key.T, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand EditSpinCommand = new("Edit Spin", "EditSpin", typeof(MainWindow), [
			new KeyGesture(Key.E, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand PrevSpinCommand = new("Previous Spin", "PrevSpin", typeof(MainWindow), [
			new KeyGesture(Key.Left, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand NextSpinCommand = new("Next Spin", "NextSpin", typeof(MainWindow), [
			new KeyGesture(Key.Right, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand PrevMapCommand = new("Previous Map", "PrevMap", typeof(MainWindow), [
			new KeyGesture(Key.Up, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand NextMapCommand = new("Next Map", "NextMap", typeof(MainWindow), [
			new KeyGesture(Key.Down, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand ShuffleCommand = new("Shuffle", "Shuffle", typeof(MainWindow), [
			new KeyGesture(Key.Space, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand RespinCommand = new("Respin", "Respin", typeof(MainWindow), [
			new KeyGesture(Key.R, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand ResetTimerCommand = new("Reset Timer", "ResetTimer", typeof(MainWindow), [
			new KeyGesture(Key.T, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand StartTimerCommand = new("Start Timer", "StartTimer", typeof(MainWindow));
		public static readonly RoutedUICommand StopTimerCommand = new("Stop Timer", "StopTimer", typeof(MainWindow));
		public static readonly RoutedUICommand SetBoardSizeCommand = new("Set Board Size", "SetBoardSize", typeof(MainWindow));

		private static readonly Random random = new();
		private static readonly List<Spin> spinHistory = [];
		private static int spinHistoryIndex = 1;

		private DebugWindow? DebugWindowInst;
		private EditMapPoolWindow? EditMapPoolWindowInst;
		private EditRulesetWindow? EditRulesetWindowInst;
		private EditHotkeys? EditHotkeysWindowInst;
		private TimerSettingsWindow? TimerSettingsWindowInst;
		private StatisticsWindow? StatisticsWindowInst;
		private EditSpinWindow? EditSpinWindowInst;
		private HitmapsWindow? HitmapsWindowInst;
		private LiveSplitWindow? LiveSplitWindowInst;
		private TargetNameFormat _targetNameFormat = TargetNameFormat.Initials;
		private DailySpinData? dailySpin1 = null;
		private DailySpinData? dailySpin2 = null;
		private DailySpinData? dailySpin3 = null;
		private double _spinFontSize = 16;
		private bool _rightToLeft = false;
		private bool _topmostEnabled = false;
		private bool _useNoKOBanner = true;
		private bool _verticalDisplay = false;
		private bool _spinLock = false;
		private bool _editMode = false;
		private bool _staticSize = false;
		private bool _staticSizeLHS = false;
		private bool _killValidations = false;
		private bool _showStreak = false;
		private bool _showStreakPB = false;
		private bool _showTimer = false;
		private bool _timerMultiSpin = false;
		private bool _timerFractions = true;
		private GameMode _gameMode = GameMode.Roulette;

		public GameMode Mode {
			get => _gameMode;
			set {
				_gameMode = value;
				Config.Default.Mode = value;
				OnPropertyChanged(nameof(Mode));
                foreach (var entry in GameModeEntries)
                    entry.Refresh();
				SetupGameMode();
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
				OnPropertyChanged(nameof(ContentGridFlowDir));
			}
		}

		public bool BingoSize4x4 => Config.Default.BingoCardSize == 4 * 4;
		public bool BingoSize5x5 => Config.Default.BingoCardSize == 5 * 5;
		public bool BingoSize6x6 => Config.Default.BingoCardSize == 6 * 6;

		public TargetNameFormat TargetNameFormat {
			get => _targetNameFormat;
			set {
				_targetNameFormat = value;
				Config.Default.TargetNameFormat = value.ToString();
				OnPropertyChanged(nameof(TargetNameFormat));
				SyncHistoryEntries();
				foreach (var entry in TargetNameFormatEntries)
					entry.Refresh();
			}
		}
		public bool AutoUpdateCheck {
			get => Config.Default.CheckUpdate;
			set {
				Config.Default.CheckUpdate = value;
				OnPropertyChanged(nameof(AutoUpdateCheck));
			}
		}

		public double SpinSmallFontSize {
			get => _spinFontSize * 0.8;
		}

		public double SpinNTKOHeight {
			get => _useNoKOBanner ? _spinFontSize * 1.1 : 0;
		}

		public double SpinFontSize {
			get => _spinFontSize;
			private set {
				if (_spinFontSize != value) {
					_spinFontSize = value;
					OnPropertyChanged(nameof(SpinFontSize));
					OnPropertyChanged(nameof(SpinSmallFontSize));
					OnPropertyChanged(nameof(SpinNTKOHeight));
					OnPropertyChanged(nameof(BingoGroupFontSize));
				}
			}
		}

		public double BingoGroupFontSize => SpinFontSize * .85;

		public bool ShuffleButtonEnabled => missionPool.Count > 0;

		public bool UseNoKOBanner {
			get => _useNoKOBanner;
			set {
				_useNoKOBanner = value;
				Config.Default.UseNoKOBanner = value;
				OnPropertyChanged(nameof(UseNoKOBanner));
				OnPropertyChanged(nameof(SpinNTKOHeight));
				RefitWindow();
			}
		}

		public bool TopmostEnabled {
			get => _topmostEnabled;
			set {
				_topmostEnabled = value;
				Topmost = value;
				Config.Default.AlwaysOnTop = value;
				OnPropertyChanged(nameof(TopmostEnabled));
			}
		}
		public bool VerticalDisplay {
			get => _verticalDisplay;
			set {
				_verticalDisplay = value;
				Config.Default.VerticalDisplay = value;
				OnPropertyChanged(nameof(VerticalDisplay));
			}
		}
		public bool SpinLock {
			get => _spinLock;
			set {
				if (value == _spinLock) return;
				
				// Clear any active spin countdown timers.
				if (value) CancelScheduledAutoSpin();

				_spinLock = value;
				OnPropertyChanged(nameof(SpinLock));
				SendSpinLockToClient();
			}
		}
		public bool HorizontalSpinDisplay { get; set; } = false;
		public bool RightToLeft {
			get => _rightToLeft;
			set {
				if (value == _rightToLeft) return;
				_rightToLeft = value;
				if (value != Config.Default.RightToLeft)
					Config.Default.RightToLeft = value;
				OnPropertyChanged(nameof(RightToLeft));
				OnPropertyChanged(nameof(RightToLeftFlowDir));
				OnPropertyChanged(nameof(ContentGridFlowDir));
				OnPropertyChanged(nameof(SpinTextAlignment));
				OnPropertyChanged(nameof(StatusAlignLeft));
			}
		}
		public bool StaticSize {
			get => _staticSize;
			set {
				if (value == _staticSize) return;
				_staticSize = value;

				if (value != Config.Default.StaticSize)
					Config.Default.StaticSize = value;

				OnPropertyChanged(nameof(ContentGridAlign));
				OnPropertyChanged(nameof(StaticSize));
				OnPropertyChanged(nameof(SpinAlignHorz));
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
				OnPropertyChanged(nameof(ContentGridFlowDir));
				OnPropertyChanged(nameof(RightToLeftFlowDir));
				OnPropertyChanged(nameof(StatusAlignLeft));
			}
		}
		public bool StaticSizeLHS {
			get => _staticSizeLHS;
			set {
				if (value == _staticSizeLHS) return;
				_staticSizeLHS = value;
				if (value != Config.Default.StaticSizeLHS)
					Config.Default.StaticSizeLHS = value;
				OnPropertyChanged(nameof(ContentGridAlign));
				OnPropertyChanged(nameof(StaticSize));
				OnPropertyChanged(nameof(SpinAlignHorz));
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
				OnPropertyChanged(nameof(ContentGridFlowDir));
				OnPropertyChanged(nameof(RightToLeftFlowDir));
				OnPropertyChanged(nameof(StatusAlignLeft));
			}
		}
		public bool KillValidations {
			get => _killValidations;
			set {
				if (value == _killValidations) return;
				_killValidations = value;
				if (value != Config.Default.KillValidations)
					Config.Default.KillValidations = value;

				OnPropertyChanged(nameof(KillValidations));

				foreach (var cond in conditions)
					cond.ForceUpdate();
			}
		}
		public bool ShowSpinLabels => !_editMode;
		public bool ShowStatusBar => ShowTimer || ShowStreak;
		public bool EditMode {
			get => _editMode;
			set {
				if (value == _editMode) return;
				_editMode = value;
				OnPropertyChanged(nameof(EditMode));
				OnPropertyChanged(nameof(ShowSpinLabels));
			}
		}
		public bool ShowTimer {
			get => _showTimer;
			set {
				_showTimer = value;
				if (value != Config.Default.Timer)
					Config.Default.Timer = value;
				OnPropertyChanged(nameof(ShowStatusBar));
				OnPropertyChanged(nameof(ShowTimer));
				RefitWindow();
			}
		}
		public bool ShowStreak {
			get => _showStreak;
			set {
				_showStreak = value;
				if (value != Config.Default.Streak)
					Config.Default.Streak = value;
				OnPropertyChanged(nameof(ShowStatusBar));
				OnPropertyChanged(nameof(ShowStreak));
				RefitWindow();
			}
		}
		public bool ShowStreakPB {
			get => _showStreakPB;
			set {
				_showStreakPB = value;
				if (value != Config.Default.ShowStreakPB)
					Config.Default.ShowStreakPB = value;
				OnPropertyChanged(nameof(ShowStreakPB));
				UpdateStreakStatus(false);
			}
		}
		public bool TimerMultiSpin {
			get => _timerMultiSpin;
			set {
				_timerMultiSpin = value;
				if (value != Config.Default.TimerMultiSpin)
					Config.Default.TimerMultiSpin = value;
				OnPropertyChanged(nameof(TimerMultiSpin));
			}
		}
		public bool TimerFractions {
			get => _timerFractions;
			set {
				_timerFractions = value;
				if (value != Config.Default.TimerFractions)
					Config.Default.TimerFractions = value;
				OnPropertyChanged(nameof(TimerFractions));
			}
		}
		public FlowDirection RightToLeftFlowDir
			=> ContentGridFlowDir == FlowDirection.RightToLeft
				? (RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight)
				: (RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
		public HorizontalAlignment ContentGridAlign
			=> !StaticSize ? HorizontalAlignment.Stretch : (StaticSizeLHS ? HorizontalAlignment.Left : HorizontalAlignment.Right);
		public TextAlignment SpinTextAlignment => RightToLeft ? TextAlignment.Right : TextAlignment.Left;
		public FlowDirection ContentGridFlowDir
			=> Mode == GameMode.Bingo
				? FlowDirection.LeftToRight
				: (!StaticSize ? FlowDirection.LeftToRight : (StaticSizeLHS ? FlowDirection.LeftToRight : FlowDirection.RightToLeft));
		public HorizontalAlignment SpinAlignHorz
			=> !StaticSize ? HorizontalAlignment.Stretch : (StaticSizeLHS ? HorizontalAlignment.Left : HorizontalAlignment.Right);
		public bool StatusAlignLeft => (StaticSize ? StaticSizeLHS : !RightToLeft);
		public double SpinGridWidth => Mode == GameMode.Roulette
			? (!StaticSize ? double.NaN : (VerticalDisplay ? Width : Width / 2) - 6)
			: Width / Math.Floor(Math.Sqrt(card?.Tiles.Count ?? 0));
		public double SpinGridHeight => Mode == GameMode.Roulette
			? !StaticSize ? double.NaN : SpinGridWidth * 0.33
			: Width / (Math.Floor(Math.Sqrt(card?.Tiles.Count ?? 0)) * 1.328);

		public string DailySpin1Label {
			get {
				if (dailySpin1 == null) return "Spin #1";
				if (!SpinParser.TryParse(dailySpin1.spin, out var spin)) return "Spin #1";
				var stats = Config.Default.Stats.GetSpinStats(spin!);
				var completion = stats.GetFastestIGTCompletion();
				var location = Mission.Get(spin!.Mission).Location;
				if (completion == null) return $"Spin #{dailySpin1.id}: {location}";
				return $"Spin #{dailySpin1.id}: {location} ({TimeFormatter.FormatSecondsTime(completion.IGT)})";
			}
		}

		public string DailySpin1Tooltip => dailySpin1 != null ? dailySpin1.spin : "";

		public bool DailySpin1Completed {
			get {
				if (dailySpin1 == null) return false;
				if (!SpinParser.TryParse(dailySpin1.spin, out var spin)) return false;
				var stats = Config.Default.Stats.GetSpinStats(spin!);
				return stats.Completions.Count > 0;
			}
		}

		public string DailySpin2Label {
			get {
				if (dailySpin2 == null) return "Spin #2";
				if (!SpinParser.TryParse(dailySpin2.spin, out var spin)) return "Spin #2";
				var stats = Config.Default.Stats.GetSpinStats(spin!);
				var completion = stats.GetFastestIGTCompletion();
				var location = Mission.Get(spin!.Mission).Location;
				if (completion == null) return $"Spin #{dailySpin2.id}: {location}";
				return $"Spin #{dailySpin2.id}: {location} ({TimeFormatter.FormatSecondsTime(completion.IGT)})";
			}
		}

		public string DailySpin2Tooltip => dailySpin2 != null ? dailySpin2.spin : "";

		public bool DailySpin2Completed {
			get {
				if (dailySpin2 == null) return false;
				if (!SpinParser.TryParse(dailySpin2.spin, out var spin)) return false;
				var stats = Config.Default.Stats.GetSpinStats(spin!);
				return stats.Completions.Count > 0;
			}
		}

		public string DailySpin3Label {
			get {
				if (dailySpin3 == null) return "Spin #3";
				if (!SpinParser.TryParse(dailySpin3.spin, out var spin)) return "Spin #3";
				var stats = Config.Default.Stats.GetSpinStats(spin!);
				var completion = stats.GetFastestIGTCompletion();
				var location = Mission.Get(spin!.Mission).Location;
				if (completion == null) return $"Spin #{dailySpin3.id}: {location}";
				return $"Spin #{dailySpin3.id}: {location} ({TimeFormatter.FormatSecondsTime(completion.IGT)})";
			}
		}

		public string DailySpin3Tooltip => dailySpin3 != null ? dailySpin3.spin : "";

		public bool DailySpin3Completed {
			get {
				if (dailySpin3 == null) return false;
				if (!SpinParser.TryParse(dailySpin3.spin, out var spin)) return false;
				var stats = Config.Default.Stats.GetSpinStats(spin!);
				return stats.Completions.Count > 0;
			}
		}

		public Spin? CurrentSpin => spin;

		public ObservableCollection<GameModeEntry> GameModeEntries = [
			new(GameMode.Roulette, "Roulette"),
            new(GameMode.Bingo, "Bingo"),
        ];
		public ObservableCollection<ContextSubmenuEntry> HistoryEntries = [];
		public ObservableCollection<ContextSubmenuEntry> BookmarkEntries = [
			new("<Add Current>", 0),
			new("<Remove Current>", 1),
		];
		public ObservableCollection<TargetNameFormatEntry> TargetNameFormatEntries = [
			new(TargetNameFormat.Initials, "Initials"),
			new(TargetNameFormat.Full, "Full Name"),
			new(TargetNameFormat.Short, "Short Name"),
		];

		private BingoCard? card = null;
		private readonly List<SpinCondition> conditions = [];
		private readonly ObservableCollection<Ruleset> rulesets = [];

		private readonly List<MissionID> missionPool = [];
		private Mission? currentMission = null;
		private Entrance? usedEntrance = null;
		private string[] loadout = [];
		private bool disableClientUpdate = false;
		private bool timerStopped = true;
		private bool timerManuallyStopped = false;
		private DateTime timerStart = DateTime.Now;
		private DateTime spinTimerStart = DateTime.Now;
		private TimeSpan? timeElapsed = null;
		private TimeSpan? spinTimeElapsed = null;
		private int streak = 0;
		private Spin? spin = null;
		private bool spinCompleted = false;
		private bool hasRestartedSinceSpin = false;
		private List<Target> trackedValidKills = [];
		private DateTime? autoSpinSchedule = null;
		private MissionID autoSpinMission = MissionID.NONE;
		private readonly LiveSplitClient liveSplit;
		private readonly DispatcherTimer timer;
		private readonly ObservableCollection<MissionComboBoxItem> missionListItems = [];
		public event EventHandler<Spin>? SpinChanged;

		private ObservableCollection<MissionComboBoxItem> MissionListItems {
			get {
				if (missionListItems.Count == 0) {
					var group = MissionGroup.None;
					Mission.All.ForEach(mission => {
						if (mission.Group != group) {
							missionListItems.Add(new() {
								Name = mission.Group.GetName(),
								IsSeparator = true,
							});
							group = mission.Group;
						}
						missionListItems.Add(new() {
							ID = mission.ID,
							Name = mission.Name,
							Image = mission.ImagePath,
							Location = mission.Location,
							IsSeparator = false
						});
					});
				}
				return missionListItems;
			}
		}

        private void RegisterWindowPlace() {
            if (OperatingSystem.IsWindowsVersionAtLeast(7))
                ((App)Application.Current).WindowPlace.Register(this);
        }

        public MainWindow() {
			liveSplit = ((App)Application.Current).LiveSplitClient;
			DataContext = this;
			InitializeComponent();
			MainContextMenu.DataContext = this;
			SizeToContent = SizeToContent.Height;

			RegisterWindowPlace();
			Focus();

			var ver = Assembly.GetExecutingAssembly().GetName().Version;
			if (ver != null) {
				var logoVer = Version.Parse(ver.ToString());
				var logoVerStr = logoVer.Major + "." + logoVer.Minor;
				if (logoVer.Build != 0)
					logoVerStr += "." + logoVer.Build;
				Logo.ToolTip = "Croupier v" + logoVerStr;
			}

			SetupGameMode();

			var idx = MissionListItems.ToList().FindIndex(item => item.ID == currentMission?.ID);
			MissionSelect.SelectedIndex = idx;

			PropertyChanged += MainWindow_PropertyChanged;

			timer = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(1)
			};
			timer.Tick += (object? sender, EventArgs e) => {
				var diff = timeElapsed ?? DateTime.Now - timerStart;

				if (autoSpinSchedule.HasValue) {
					Timer.Text = (DateTime.Now - autoSpinSchedule.Value).ToString(@"\-s");
					if (DateTime.Now > autoSpinSchedule.Value) {
						CancelScheduledAutoSpin();
						AutoSpin(autoSpinMission);
					}
				}
				else if (timerStopped && !timeElapsed.HasValue)
					Timer.Text = TimerFractions ? "00:00.00" : "00:00";
				else if (diff.TotalHours > 1)
					Timer.Text = diff.ToString(TimerFractions ? @"hh\:mm\:ss\.ff" : @"hh\:mm\:ss");
				else// if (diff.TotalMinutes > 1)
					Timer.Text = diff.ToString(TimerFractions ? @"mm\:ss\.ff" : @"mm\:ss");
				//else
				//	Timer.Text = diff.ToString(@"ss\.ff");
			};
			timer.Start();


			var respinHotkey = Hotkeys.Add("Respin", () => RespinCommand.Execute(this, this));
			var shuffleHotkey = Hotkeys.Add("Random Map", () => ShuffleCommand.Execute(this, this));
			var nextSpinHotkey = Hotkeys.Add("Next Spin", () => {
				if (NextSpinCommand.CanExecute(this, this))
					NextSpinCommand.Execute(this, this);
			});
			var prevSpinHotkey = Hotkeys.Add("Previous Spin", () => {
				if (PrevSpinCommand.CanExecute(this, this))
					PrevSpinCommand.Execute(this, this);
			});
			var nextMapHotkey = Hotkeys.Add("Next Map", () => {
				if (NextMapCommand.CanExecute(this, this))
					NextMapCommand.Execute(this, this);
			});
			var prevMapHotkey = Hotkeys.Add("Previous Map", () => {
				if (PrevMapCommand.CanExecute(this, this))
					PrevMapCommand.Execute(this, this);
			});

			respinHotkey.Keybind = Config.Default.RespinKeybind;
			shuffleHotkey.Keybind = Config.Default.ShuffleKeybind;
			nextSpinHotkey.Keybind = Config.Default.NextSpinKeybind;
			prevSpinHotkey.Keybind = Config.Default.PrevSpinKeybind;
			nextMapHotkey.Keybind = Config.Default.NextMapKeybind;
			prevMapHotkey.Keybind = Config.Default.PrevMapKeybind;

			Hotkeys.RegisterAll();

			respinHotkey.RebindAction = (Keybind bind) => {
				Config.Default.RespinKeybind = bind;
				Config.Save();
			};
			shuffleHotkey.RebindAction = (Keybind bind) => {
				Config.Default.ShuffleKeybind = bind;
				Config.Save();
			};
			nextSpinHotkey.RebindAction = (Keybind bind) => {
				Config.Default.NextSpinKeybind = bind;
				Config.Save();
			};
			prevSpinHotkey.RebindAction = (Keybind bind) => {
				Config.Default.PrevSpinKeybind = bind;
				Config.Save();
			};
			nextMapHotkey.RebindAction = (Keybind bind) => {
				Config.Default.NextMapKeybind = bind;
				Config.Save();
			};
			prevMapHotkey.RebindAction = (Keybind bind) => {
				Config.Default.PrevMapKeybind = bind;
				Config.Save();
			};

			CroupierSocketServer.Connected += (object? sender, int _) => {
				SendMissionsToClient();
				SendSpinToClient();
				SendSpinLockToClient();
				SendStreakToClient();
				SendTimerToClient();
			};
			CroupierSocketServer.Respin += (object? sender, MissionID id) => Spin(id);
			CroupierSocketServer.AutoSpin += (object? sender, MissionID id) => {
				if (SpinLock) return;
				if (Config.Default.AutoSpinCountdown > 0)
					ScheduleAutoSpin(TimeSpan.FromSeconds(Config.Default.AutoSpinCountdown), id);
				else
					AutoSpin(id);
			};
			CroupierSocketServer.Random += (object? sender, int _) => Shuffle();
			CroupierSocketServer.ToggleSpinLock += (object? sender, int _) => {
				SpinLock = !SpinLock;
			};
			CroupierSocketServer.ToggleTimer += (object? sender, bool enable) => {
				ShowTimer = enable;
			};
			CroupierSocketServer.PauseTimer += (object? sender, bool pause) => {
				if (pause) StopTimer();
				else ResumeTimer();
				isLoadRemoving = false;
				timerManuallyStopped = pause;
			};
			CroupierSocketServer.ResetTimer += (object? sender, int _) => {
				StopTimer();
				ResetTimer();
			};
			CroupierSocketServer.SplitTimer += (object? sender, int _) => {
				SplitTimer();
			};
			CroupierSocketServer.ResetStreak += (object? sender, int _) => {
				ResetStreak();
			};
			CroupierSocketServer.MissionStart += (object? sender, MissionStart start) => {
				if (card != null) card.Reset();
				TrackGameMissionAttempt(start);
			};
			CroupierSocketServer.MissionComplete += (object? sender, MissionCompletion arg) => {
				if (spinCompleted)
					return;
				spinCompleted = true;
				arg.KillsValidated = CheckSpinKillsValid();
				if (arg.SA && (arg.KillsValidated || Config.Default.StreakRequireValidKills == false)) IncrementStreak();
				else ResetStreak();
				
				HandleTimingOnSpinComplete(arg);
				TrackGameMissionCompletion(arg);
			};
			CroupierSocketServer.MissionOutroBegin += (object? sender, int _) => {
				if (Config.Default.TimerPauseDuringOutro)
					StopTimer();
			};
			CroupierSocketServer.MissionFailed += (object? sender, int _) => {
				if (spinCompleted) return;

				if (hasRestartedSinceSpin || (DateTime.Now - spinTimerStart).TotalSeconds > Config.Default.StreakReplanWindow)
					ResetStreak();

				hasRestartedSinceSpin = true;
			};
			CroupierSocketServer.Missions += (object? sender, List<MissionID> missions) => {
				missionPool.Clear();
				missionPool.AddRange(missions);
				EditMapPoolWindowInst?.UpdateMissionPool(missionPool);
			};
			CroupierSocketServer.Prev += (object? sender, int _) => {
				disableClientUpdate = true;
				PreviousSpin();
				disableClientUpdate = false;
				SendSpinToClient();
			};
			CroupierSocketServer.Next += (object? sender, int _) => {
				disableClientUpdate = true;
				NextSpin();
				disableClientUpdate = false;
				SendSpinToClient();
			};
			CroupierSocketServer.SpinData += (object? sender, string data) => {
				if (SpinParser.TryParse(data, out var spin)) {
					disableClientUpdate = true;
					SetSpin(spin!);
					UpdateSpinHistory();
					PostConditionUpdate();
					ResetCurrentSpinProgress();
					StartTimer();
					disableClientUpdate = false;
				}
			};
			CroupierSocketServer.LoadStarted += (object? sender, int _) => {
				HandleTimingOnLoadStart();
			};
			CroupierSocketServer.LoadFinished += (object? sender, int _) => {
				HandleTimingOnLoadFinish();
			};
			CroupierSocketServer.KillValidation += (object? sender, string data) => {
				if (data.Length == 0) return;

				var firstLine = data.Split("\n")[0];
				var validationStrings = firstLine.Split(",");

				foreach (var v in validationStrings) {
					var segments = v.Split(":");
					if (segments.Length != 4) return;
					var target = Roulette.Main.GetTargetByInitials(segments[0]);
					var specificTarget = Roulette.Main.GetTargetByInitials(segments[3]);
					var kv = new KillValidation {
						target = target,
						killValidation = (KillValidationType)int.Parse(segments[1]),
						disguiseValidation = int.Parse(segments[2]) != 0,
						specificTarget = specificTarget,
					};
					for (var i = 0; i < conditions.Count; ++i) {
						var cond = conditions[i];
						if (cond.Target.Initials != kv.target?.Initials) continue;
						cond.KillValidation = kv;
					}
				}

				TrackKillValidation();
			};
			CroupierSocketServer.Event += CroupierSocketServer_Event;
			HitmapsSpinLink.ReceiveNewSpinData += (object? sender, string data) => {
				if (SpinParser.TryParse(data, out var spin)) {
					SetSpin(spin!);
					UpdateSpinHistory();
					PostConditionUpdate();
					ResetCurrentSpinProgress();
					StartTimer(true);
					TrackNewSpin();
				}
			};

			LoadSpinHistory();

			if (spinHistory.Count > 0) {
				SetSpinHistory(1);
				if (Mode == GameMode.Bingo)
					Spin(MissionID.PARIS_SHOWSTOPPER);
			}
			else {
				SetMission(MissionID.PARIS_SHOWSTOPPER);
				Spin(MissionID.PARIS_SHOWSTOPPER);
			}


			SendMissionsToClient();
			SendSpinToClient();
		}

		private static readonly JsonSerializerOptions jsonGameEventSerializerOptions = new() {
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
		};

		private void CroupierSocketServer_Event(object? sender, string evData) {
			try {
				if (card == null) return;
				var json = JsonDocument.Parse(evData);
				var ev = json.Deserialize<GameEvents.Event>(jsonGameEventSerializerOptions);
				if (ev?.Value is JsonElement value) {
					var val = DeserializeEventValue(ev.Name, value);
					if (val != null) {
						card.TryAdvance(val);
						var result = card.CheckWin();
						Debug.WriteLine(ev.Name, val.ToString());
						if (result != null) {
							Debug.WriteLine($"WIN!!! {JsonSerializer.Serialize(result)}");
						}
					}
				}
			} catch (JsonException e) {
					Debug.WriteLine(e);
			}
		}

		private static GameEvents.EventValue? DeserializeEventValue(string name, JsonElement json) {
			return name switch {
				"setpieces" => json.Deserialize<GameEvents.SetpiecesEventValue>(jsonGameEventSerializerOptions),
				"ItemPickedUp" => json.Deserialize<GameEvents.ItemPickedUpEventValue>(jsonGameEventSerializerOptions),
				"ItemRemovedFromInventory" => json.Deserialize<GameEvents.ItemRemovedFromInventoryEventValue>(jsonGameEventSerializerOptions),
				"ItemDropped" => json.Deserialize<GameEvents.ItemDroppedEventValue>(jsonGameEventSerializerOptions),
				"ItemThrown" => json.Deserialize<GameEvents.ItemThrownEventValue>(jsonGameEventSerializerOptions),
				"Disguise" => new GameEvents.StringEventValue(json.Deserialize<string>(jsonGameEventSerializerOptions) ?? throw new JsonException()),
				"Actorsick" => json.Deserialize<GameEvents.ActorSickEventValue>(jsonGameEventSerializerOptions),
				"Dart_Hit" => json.Deserialize<GameEvents.DartHitEventValue>(jsonGameEventSerializerOptions),
				"Trespassing" => json.Deserialize<GameEvents.TrespassingEventValue>(jsonGameEventSerializerOptions),
				"SecuritySystemRecorder" => json.Deserialize<GameEvents.SecuritySystemRecorderEventValue>(jsonGameEventSerializerOptions),
				"BodyBagged" => json.Deserialize<GameEvents.ActorIdentityEventValue>(jsonGameEventSerializerOptions),
				"BodyHidden" => json.Deserialize<GameEvents.ActorIdentityEventValue>(jsonGameEventSerializerOptions),
				"Door_Unlocked" => json.Deserialize<GameEvents.EventValue>(jsonGameEventSerializerOptions),
				"Investigate_Curious" => json.Deserialize<GameEvents.InvestigateCuriousEventValue>(jsonGameEventSerializerOptions),
				"OpportunityEvents" => json.Deserialize<GameEvents.OpportunityEventValue>(jsonGameEventSerializerOptions),
				"Level_Setup_Events" => json.Deserialize<GameEvents.LevelSetupEventValue>(jsonGameEventSerializerOptions),
				_ => null,
			};
		}

		private void MainWindow_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(VerticalDisplay)
				|| e.PropertyName == nameof(StaticSize)
				|| e.PropertyName == nameof(UseNoKOBanner)) {
				DoHackyWindowSizeFix();
				return;
			}
		}

		private void DoHackyWindowSizeFix(bool skipSetupCall = false, int time = 40) {
			RefitWindow();
			Task.Delay(time).ContinueWith(task => {
				if (!skipSetupCall) SetupGameUI();
				RefitWindow();
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void SetupGameMode() {
			LoadSettings();
			
			if (Mode == GameMode.Roulette) LoadRouletteConfiguration();
			else LoadBingoConfiguration();

			UpdateStreakStatus(false);

			MissionSelect.ItemsSource = MissionListItems;
			ContextMenuGameMode.ItemsSource = GameModeEntries;
			ContextMenuGameMode.DataContext = this;
            ContextMenuTargetNameFormat.ItemsSource = TargetNameFormatEntries;
            ContextMenuTargetNameFormat.DataContext = this;
            ContextMenuHistory.ItemsSource = HistoryEntries;
            ContextMenuHistory.DataContext = this;
            ContextMenuBookmarks.ItemsSource = BookmarkEntries;
            ContextMenuBookmarks.DataContext = this;

            ContextMenuHistory.Visibility = Mode == GameMode.Roulette ? Visibility.Visible : Visibility.Collapsed;
            ContextMenuCopySpin.Visibility = Mode == GameMode.Roulette ? Visibility.Visible : Visibility.Collapsed;
            ContextMenuPasteSpin.Visibility = Mode == GameMode.Roulette ? Visibility.Visible : Visibility.Collapsed;
            ContextMenuBookmarks.Visibility = Mode == GameMode.Roulette ? Visibility.Visible : Visibility.Collapsed;
            ContextMenuTargetNameFormat.Visibility = Mode == GameMode.Roulette ? Visibility.Visible : Visibility.Collapsed;
            ContextMenuRouletteKillConfirmations.Visibility = Mode == GameMode.Roulette ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuTopSeparator.Visibility = Mode == GameMode.Roulette ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuSpinSeparator.Visibility = Mode == GameMode.Roulette ? Visibility.Visible : Visibility.Collapsed;
		}

		private void LoadMainConfiguration() {
			try {
				Roulette.Main.Load();
			}
			catch (Exception e) {
				MessageBox.Show(
					this,
					$"Config error: {e.Message}",
					"Config Error - Croupier",
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation
				);
			}
		}

		private void LoadRulesetConfiguration() {
			var files = Directory.GetFiles("rulesets", "*.json");

			rulesets.Clear();

			foreach (var file in files.Reverse()) {
				var ruleset = Ruleset.LoadFromFile(Roulette.Main, file);
				if (rulesets.Any(r => r.Name == ruleset.Name))
					continue;
				if (ruleset.Name == "Custom")
					rulesets.Insert(0, ruleset);
				else
					rulesets.Add(ruleset);
			}

			var activeRuleset = rulesets.FirstOrDefault(r => r.Name == Config.Default.Ruleset)
				?? rulesets.FirstOrDefault(r => r.Name == "RR17")
				?? rulesets.FirstOrDefault();

			if (activeRuleset != null) {
				Ruleset.Current = activeRuleset;
				Config.Default.Ruleset = Ruleset.Current.Name;
			}
		}

		private void LoadRouletteConfiguration() {
			LoadMainConfiguration();
			LoadRulesetConfiguration();
		}

		private void LoadBingoConfiguration() {
			LoadMainConfiguration();
			Bingo.Main.Load();
		}

		private void LoadSpinHistory() {
			if (Config.Default.SpinHistory.Count > 0) {
				string[] history = new string[Config.Default.SpinHistory.Count];
				Config.Default.SpinHistory.CopyTo(history, 0);

				foreach (var item in history.Reverse()) {
					if (SpinParser.TryParse(item, out var spin))
						PushSpinToHistory(spin!, true);
				}

				SyncHistoryEntries();
			}
		}

		private void LoadSettings() {
			if (!Enum.IsDefined(typeof(MissionPoolPresetID), Config.Default.MissionPool))
				Config.Default.MissionPool = MissionPoolPresetID.MainMissions;

			_gameMode = Config.Default.Mode;
			VerticalDisplay = Config.Default.VerticalDisplay;
			TopmostEnabled = Config.Default.AlwaysOnTop;
			UseNoKOBanner = Config.Default.UseNoKOBanner;
			RightToLeft = Config.Default.RightToLeft;
			StaticSize = Config.Default.StaticSize;
			StaticSizeLHS = Config.Default.StaticSizeLHS;
			ShowTimer = Config.Default.Timer;
			ShowStreak = Config.Default.Streak;
			ShowStreakPB = Config.Default.ShowStreakPB;
			streak = Config.Default.StreakCurrent;
			TimerMultiSpin = Config.Default.TimerMultiSpin;
			TimerFractions = Config.Default.TimerFractions;
			KillValidations = Config.Default.KillValidations;
			TargetNameFormat = TargetNameFormatMethods.FromString(Config.Default.TargetNameFormat);
			LoadMissionPool();

			foreach (var bookmark in Config.Default.Bookmarks) {
				BookmarkEntries.Add(new(bookmark, BookmarkEntries.Count));
			}
		}

		private bool CheckSpinKillsValid() {
			if (spin == null) return true;
			foreach (var cond in spin.Conditions) {
				if (!cond.KillValidation.IsValid) return false;
			}
			return true;
		}

		private void EditSpinWindow_SetCondition(object? _sender, SpinCondition condition) {
			if (currentMission != condition.Target.Mission) return;
			for (var i = 0; i < conditions.Count; i++) {
				if (conditions[i].Target != condition.Target) continue;
				conditions[i] = condition;
				break;
			}

			UpdateSpinHistory();
			PostConditionUpdate();
		}

		private void EditMapPoolWindow_AddMissionToPool(object? sender, MissionID e) {
			if (missionPool.Contains(e)) return;
			missionPool.Add(e);
			SendMissionsToClient();
			OnPropertyChanged(nameof(ShuffleButtonEnabled));
			SaveCustomMissionPool();
		}

		private void EditMapPoolWindow_RemoveMissionFromPool(object? sender, MissionID e) {
			missionPool.Remove(e);
			SendMissionsToClient();
			OnPropertyChanged(nameof(ShuffleButtonEnabled));
			SaveCustomMissionPool();
		}

		private void SaveCustomMissionPool() {
			if (Config.Default.MissionPool != MissionPoolPresetID.Custom)
				return;
			
			Config.Default.CustomMissionPool ??= [];
			Config.Default.CustomMissionPool.Clear();

			foreach (var missionID in missionPool) {
				var mission = Mission.TryGet(missionID);
				if (mission == null)
					continue;
				Config.Default.CustomMissionPool.Add(mission.Codename);
			}

			Config.Save();
		}

		private void LoadMissionPool()
		{
			missionPool.Clear();
			missionPool.AddRange(Config.Default.MissionPool.GetMissions());
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			ThemeManager.SetCurrentTheme(this, new Uri("/Croupier;component/Resources/DarkTheme.xaml", UriKind.Relative));

			if (Config.Default.CheckUpdate) {
				DoUpdateCheck();
				CheckDailySpinsAsync();
			}
		}

		public void AutoSpin(MissionID id = MissionID.NONE) {
			if (id != MissionID.NONE && currentMission != null && currentMission.ID == id && !spinCompleted)
				return;

			Spin(id);
		}

		public void ScheduleAutoSpin(TimeSpan time, MissionID id = MissionID.NONE) {
			autoSpinSchedule = DateTime.Now + time;
			autoSpinMission = id;
		}

		public void CancelScheduledAutoSpin() {
			autoSpinSchedule = null;
		}

		public void DrawBingo(MissionID id = MissionID.NONE) {
			try {
				card = BingoGenerator.Generate(Config.Default.BingoCardSize, id);
			}
			catch (BingoGeneratorException e) {
				MessageBox.Show($"Error: {e.Message}", "Error - Croupier", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		public void Spin(MissionID id = MissionID.NONE) {
			if (Mode == GameMode.Bingo) {
				if (id == MissionID.NONE && card != null)
					id = card?.Mission ?? MissionID.NONE;
				if (id != MissionID.NONE && !SetMission(id))
					return;
				spin = null;
				DrawBingo(id);
			}
			else {
				if (Ruleset.Current == null) {
					MessageBox.Show("No ruleset active. Please select a ruleset from the ruleset window.");
					return;
				}

				if (id == MissionID.NONE && spin != null)
					id = spin.Mission;
				if (id != MissionID.NONE && !SetMission(id))
					return;

				if (!spinCompleted)
					ResetStreak();

				CancelScheduledAutoSpin();

				var gen = Roulette.Main.CreateGenerator(Ruleset.Current);
				spin = gen.Spin(Mission.Get(currentMission!.ID));
				SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
			}

			PostConditionUpdate();

			HandleTimingOnNewSpin(id);

			Config.Default.SpinIsRandom = true;
			Config.Save();

			TrackNewSpin();
		}

		public void SetBoardSizeCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
			Config.Default.BingoCardSize = e.Parameter switch {
				"4x4" => 4 * 4,
				"5x5" => 5 * 5,
				_ => 5 * 5,
			};

			if (card != null && card.Tiles.Count != Config.Default.BingoCardSize)
				Spin();

			OnPropertyChanged(nameof(BingoSize4x4));
			OnPropertyChanged(nameof(BingoSize5x5));
		}

		public void ResetCurrentSpinProgress() {
			spinCompleted = false;
			hasRestartedSinceSpin = false;
		}

		public bool SetMission(MissionID id) {
			if (id == MissionID.NONE)
				return false;
			currentMission = Mission.All.Find(m => m.ID == id);
			if (currentMission == null) return false;
			var idx = MissionListItems.ToList().FindIndex(item => item.ID == id);
			MissionSelect.SelectedIndex = idx;
			return idx != -1;
		}

		public bool PushSpinToHistory(Spin spin, bool skipSync = false) {
			if (spin == null || spin.Conditions.Count == 0) return false;
			if (spin.Conditions == conditions) return false;
			if (spinHistory.Count > 0 && spinHistory[^1].ToString() == spin.ToString())
				return true;

			spinHistory.Add(spin);
			if (HistoryEntries.Count < MAX_HISTORY_ENTRIES)
				HistoryEntries.Insert(0, new(spin.ToString(), spinHistory.Count - 1));
			while (spinHistory.Count > MAX_HISTORY_ENTRIES)
				spinHistory.RemoveAt(0);
			if (!skipSync) SyncHistoryEntries();
			return true;
		}

		public bool UpdateSpinHistory()
		{
			if (spinHistory.Count == 0) return false;
			var spin = spinHistory[^1];
			spin.Conditions.Clear();
			spin.Conditions.AddRange(conditions);
			SyncHistoryEntries();
			return true;
		}

		public bool PushCurrentSpinToHistory() {
			return PushSpinToHistory(new Spin([..conditions]));
		}

		public void PreviousSpin() {
			if (spinHistoryIndex < 1) spinHistoryIndex = 1;
			if (spinHistoryIndex >= spinHistory.Count) {
				spinHistoryIndex = spinHistory.Count;
				return;
			}

			SetSpinHistory(spinHistoryIndex + 1);
			ResetCurrentSpinProgress();
			ResetTimer();
		}

		public void NextSpin() {
			if (spinHistoryIndex > spinHistory.Count)
				spinHistoryIndex = spinHistory.Count;
			if (spinHistoryIndex <= 1) {
				spinHistoryIndex = 1;
				return;
			}

			SetSpinHistory(spinHistoryIndex - 1);
			ResetCurrentSpinProgress();
			ResetTimer();
		}

		public void SetSpinHistory(int idx) {
			spinHistoryIndex = idx;
			SetSpin(spinHistory[^spinHistoryIndex]);
			SyncHistoryEntries();
			SendSpinToClient();
		}

		public void SyncHistoryEntries() {
			if (spinHistory.Count == 0) return;

			var fwdIndex = spinHistory.Count - spinHistoryIndex;

			Config.Default.SpinHistory.Clear();

			for (var i = 0; i < spinHistory.Count && i < HistoryEntries.Count; ++i) {
				var absIdx = spinHistory.Count - i - 1;
				var entry = HistoryEntries[i];
				entry.Name = spinHistory[absIdx].ToString();
				entry.Index = absIdx;
				entry.IsSelected = absIdx == fwdIndex;
				Config.Default.SpinHistory.Add(entry.Name);
			}

			Config.Save();
			OnPropertyChanged(nameof(HistoryEntries));
		}

		public void SyncBookmarks()
		{
			Config.Default.Bookmarks.Clear();
			
			for (var i = 2; i < BookmarkEntries.Count; ++i) {
				Config.Default.Bookmarks.Add(BookmarkEntries[i].Name);
			}

			Config.Save();
		}

		public void SetSpin(Spin spin) {
			if (spin == null) return;
			conditions.Clear();
			conditions.AddRange(spin.Conditions);
			this.spin = spin;
			SetMission(spin.Mission);
			EditSpinWindowInst?.UpdateConditions(conditions);
			PostConditionUpdate();
		}

		private void PostConditionUpdate() {
			Config.Default.SpinIsRandom = false;
			Config.Save();
			SendSpinToClient();
			SetupGameUI();
			RefitWindow();
			if (this.spin != null)
				this.SpinChanged?.Invoke(this, this.spin);
		}

		private void UpdateStreakStatus(bool save = true) {
			SendStreakToClient();

			Config.Default.StreakCurrent = streak;

			if (streak > Config.Default.StreakPB) {
				Config.Default.StreakPB = streak;
				if (Config.Default.StreakPB > Config.Default.Stats.TopStreak)
					Config.Default.Stats.TopStreak = Config.Default.StreakPB;
			}

			if (Config.Default.ShowStreakPB)
				Streak.Text = "Streak: " + streak + " (PB: " + Config.Default.StreakPB + ")";
			else
				Streak.Text = "Streak: " + streak;

			if (save) Config.Save();
		}

		private void IncrementStreak() {
			++streak;
			UpdateStreakStatus();
		}

		private void ResetStreak() {
			streak = 0;
			UpdateStreakStatus();
		}

		private void TrackNewSpin() {
			if (spin == null) return;

			var stats = Config.Default.Stats;
			var spinStats = stats.GetSpinStats(spin);
			var missionStats = stats.GetMissionStats(spin.Mission);
			spinStats.IsCustom = Config.Default.SpinIsRandom;

			var spinStr = spin.ToString();
			if (Config.Default.SpinIsRandom)
				++stats.NumRandomSpins;

			if (spinStats.DailyID == 0) {
				if (dailySpin1 != null && spinStr == dailySpin1.spin)
					spinStats.DailyID = dailySpin1.id;
				else if (dailySpin2 != null && spinStr == dailySpin2.spin)
					spinStats.DailyID = dailySpin2.id;
				else if (dailySpin3 != null && spinStr == dailySpin3.spin)
					spinStats.DailyID = dailySpin3.id;

				if (spinStats.DailyID != 0)
					++stats.NumDailySpins;
				
				++missionStats.NumSpins;
			}

			loadout = [];
			usedEntrance = null;
			trackedValidKills = [];

			Config.Save();
		}

		private void TrackKillValidation() {
			if (spin == null) return;

			var stats = Config.Default.Stats;

			foreach (var cond in spin.Conditions) {
				if (!cond.KillValidation.IsValid)
					continue;
				if (trackedValidKills.Contains(cond.Target))
					continue;

				trackedValidKills.Add(cond.Target);
				++stats.NumValidKills;
			}

			Config.Save();
		}

		private void TrackGameMissionAttempt(MissionStart start) {
			if (spin == null) return;

			var stats = Config.Default.Stats;
			var locationId = start.Location.ToLower();
			loadout = start.Loadout;
			usedEntrance = Locations.Entrances.Find(e => e.ID == locationId);

			var spinStats = stats.GetSpinStats(spin);
			var missionStats = stats.GetMissionStats(spin.Mission);

			spinStats.IsCustom = !Config.Default.SpinIsRandom;
			if (spinStats.IsCustom) {
				++stats.NumCustomSpins;
				++missionStats.NumSpins;
			}
			
			++missionStats.NumAttempts;

			if (spinStats.Attempts == 0)
				++stats.NumUniqueAttempts;

			++spinStats.Attempts;
			++stats.NumAttempts;

			Config.Save();
		}

		private void TrackGameMissionCompletion(MissionCompletion mc) {
			var stats = Config.Default.Stats;
			if (!mc.SA) {
				if (mc.KillsValidated)
					++stats.NumNonSAWins;
				Config.Save();
				return;
			}

			if (!mc.KillsValidated)
				return;

			if (spin == null)
				return;

			var spinStats = stats.GetSpinStats(spin);
			var missionStats = stats.GetMissionStats(spin.Mission);
			++stats.NumWins;
			++missionStats.NumWins;

			if (spinStats.Completions.Count == 0)
				++stats.NumUniqueWins;

			spinStats.Completions.Add(new() {
				IGT = mc.IGT > 0 ? mc.IGT : 0,
				RTA = (DateTime.Now - spinTimerStart).TotalSeconds,
				Mission = spin.Mission,
				StartLocation = usedEntrance?.ID ?? "",
				Loadout = [..loadout],
			});

			Config.Save();

			OnPropertyChanged(nameof(DailySpin1Completed));
			OnPropertyChanged(nameof(DailySpin2Completed));
			OnPropertyChanged(nameof(DailySpin3Completed));

			OnPropertyChanged(nameof(DailySpin1Label));
			OnPropertyChanged(nameof(DailySpin2Label));
			OnPropertyChanged(nameof(DailySpin3Label));
		}

		private async void StartTimer(bool overrideManual = false) {
			if (!overrideManual && timerManuallyStopped) return;
			else timerManuallyStopped = false;

			var liveSplitState = await liveSplit.GetSplitIndex();
			if (liveSplitState == -1)
				liveSplit.StartTimer();
			else
				liveSplit.Resume();

			if (TimerMultiSpin && !timerStopped) {
				switch (Config.Default.TimingMode) {
					case TimingMode.LRT:
					case TimingMode.RTA:
					case TimingMode.Spin:
						timeElapsed = DateTime.Now - timerStart;
						spinTimeElapsed = DateTime.Now - spinTimerStart;
						break;
				}
			}

			timerStopped = false;
			timerStart = timeElapsed.HasValue ? DateTime.Now - timeElapsed.Value : DateTime.Now;
			spinTimerStart = spinTimeElapsed.HasValue ? DateTime.Now - spinTimeElapsed.Value : DateTime.Now;
			spinTimeElapsed = null;

			if (Config.Default.TimingMode != TimingMode.IGT) {
				timeElapsed = null;
			}

			SendTimerToClient();
		}

		private void StopTimer() {
			liveSplit.Pause();

			if (timerStopped) return;

			if (Config.Default.TimingMode != TimingMode.IGT) {
				timeElapsed = DateTime.Now - timerStart;
				spinTimeElapsed = DateTime.Now - spinTimerStart;
			}

			timerStopped = true;
		}

		private async void ResumeTimer() {
			var splitIdx = await liveSplit.GetSplitIndex();
			if (splitIdx == -1)
				liveSplit.StartTimer();
			else
				liveSplit.Resume();

			if (!timerStopped) return;
			timerStart = timeElapsed.HasValue ? DateTime.Now - timeElapsed.Value : DateTime.Now;
			spinTimerStart = spinTimeElapsed.HasValue ? DateTime.Now - spinTimeElapsed.Value : DateTime.Now;
			timeElapsed = null;
			spinTimeElapsed = null;
			timerStopped = false;
		}

		private void ResetTimer() {
			liveSplit.Reset();
			if (!timerStopped)
				liveSplit.StartOrSplit();

			timeElapsed = null;
			spinTimeElapsed = null;
			timerStart = DateTime.Now;
			spinTimerStart = DateTime.Now;
		}

		private void SplitTimer() {
			liveSplit.Split();
		}

		private bool isLoadRemoving = false;

		private void HandleTimingOnLoadStart() {
			if (timerStopped)
				return;
			isLoadRemoving = true;

			switch (Config.Default.TimingMode) {
				case TimingMode.LRT:
				case TimingMode.Spin:
					StopTimer();
					break;
			}

			SendTimerToClient();
		}

		private void HandleTimingOnLoadFinish() {
			if (!isLoadRemoving)
				return;
			isLoadRemoving = false;

			switch (Config.Default.TimingMode) {
				case TimingMode.LRT:
					ResumeTimer();
					break;
				case TimingMode.Spin:
					if (!spinCompleted)
						ResumeTimer();
					break;
			}
			
			SendTimerToClient();
		}

		private void HandleTimingOnNewSpin(MissionID mission) {
			if (!TimerMultiSpin || Config.Default.TimerResetMission == mission) {
				ResetTimer();
				StopTimer();
			}

			ResetCurrentSpinProgress();

			StartTimer();

			SendTimerToClient();
		}

		private void HandleTimingOnSpinComplete(MissionCompletion completion) {
			if (!Config.Default.SplitRequiresSA || completion.SA)
				SplitTimer();

			//if (Config.Default.TimingMode != TimingMode.IGT && Config.Default.TimingMode != TimingMode.Spin) {
			//	if (timerStopped && !timerManuallyStopped)
			//		ResumeTimer();
			//}

			switch (Config.Default.TimingMode) {
				case TimingMode.IGT:
					timeElapsed = (timeElapsed ?? TimeSpan.Zero) + TimeSpan.FromSeconds(completion.IGT);
					SendTimerToClient();
					break;
				case TimingMode.Spin:
					StopTimer();
					break;
			}
			
			spinTimeElapsed = null;

			SendTimerToClient();
		}

		public void SendSpinLockToClient() {
			if (disableClientUpdate) return;
			CroupierSocketServer.Send("SpinLock:" + (SpinLock ? "1" : "0"));
		}

		public void SendTimerToClient() {
			if (disableClientUpdate) return;
			double elapsed = (timeElapsed ?? DateTime.Now - timerStart).TotalMilliseconds;
			CroupierSocketServer.Send($"Timer:{(timerStopped ? 0 : 1)}," + elapsed);
		}

		public void SendSpinToClient() {
			if (disableClientUpdate) return;
			var spinData = "";
			conditions.ForEach(condition => {
				if (spinData.Length > 0) spinData += ", ";
				spinData += $"{condition.ToString(TargetNameFormat.Initials)}";
			});
			CroupierSocketServer.Send("SpinData:" + spinData);
		}

		public void SendMissionsToClient() {
			if (disableClientUpdate) return;
			var missions = "";
			missionPool.ForEach(mission => {
				var miss = Mission.TryGet(mission);
				if (miss == null) return;
				missions += missions.Length > 0 ? $",{miss.Codename}" : miss.Codename;
			});
			CroupierSocketServer.Send("Missions:" + missions);
		}

		public void SendStreakToClient() {
			if (disableClientUpdate) return;
			CroupierSocketServer.Send($"Streak:{streak}");
		}

		private void OnMouseDown(object? sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void MissionSelect_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count == 0) return;
			var item = (MissionComboBoxItem)e.AddedItems[0]!;
			if (currentMission == null || currentMission.ID != item.ID)
				Spin(item.ID);
		}

		private void Shuffle() {
			if (missionPool.Count == 0) return;
			Spin(missionPool[random.Next(missionPool.Count)]);
		}

		private ICommand? _gameModeSelectCommand;
		private ICommand? _historyEntrySelectCommand;
		private ICommand? _bookmarkEntrySelectCommand;
		private ICommand? _targetNameFormatSelectCommand;

		public ICommand HistoryEntrySelectCommand {
			get => _historyEntrySelectCommand ??= new RelayCommand(param => this.HistoryEntrySelected(param));
		}

		public ICommand BookmarkEntrySelectCommand {
			get => _bookmarkEntrySelectCommand ??= new RelayCommand(param => this.BookmarkEntrySelected(param));
		}

		public ICommand TargetNameFormatSelectCommand {
			get => _targetNameFormatSelectCommand ??= new RelayCommand(param => this.TargetNameFormatSelected(param));
		}

		public ICommand GameModeSelectCommand {
			get => _gameModeSelectCommand ??= new RelayCommand(param => this.GameModeSelected(param));
		}

		private void HistoryEntrySelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= spinHistory.Count) return;
			SetSpinHistory(Math.Abs(index.Value - spinHistory.Count));
		}

		private void BookmarkEntrySelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= BookmarkEntries.Count) return;
			if (index == 0) {
				BookmarkEntries.Add(new(new Spin(conditions).ToString(), BookmarkEntries.Count));
				SyncBookmarks();
			}
			else if (index == 1) {
				var currentSpin = new Spin(conditions);
				var currentSpinStr = currentSpin.ToString();
				for (int i = 2; i < BookmarkEntries.Count; ++i) {
					if (currentSpinStr != BookmarkEntries[i].Name) continue;
					BookmarkEntries.RemoveAt(i);
					for (; i < BookmarkEntries.Count; ++i)
						--BookmarkEntries[i].Index;
					break;
				}
				SyncBookmarks();
			}
			else if (SpinParser.TryParse(BookmarkEntries[index.Value].Name, out var spin)) {
				SetSpin(spin!);
				SyncHistoryEntries();
				SendSpinToClient();
			}
		}

		private void GameModeSelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= GameModeEntries.Count)
				return;
			Mode = GameModeEntries[index.Value].Mode;
			if (Mode == GameMode.Bingo && card == null)
				Spin();
			else if (Mode == GameMode.Roulette && conditions.Count == 0)
				Spin();
		}

		private void TargetNameFormatSelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= TargetNameFormatEntries.Count)
				return;
			TargetNameFormat = TargetNameFormatEntries[index.Value].ID;
		}

		private void ContextMenu_Exit(object? sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void Window_Deactivated(object? sender, EventArgs e) {
			Window window = (Window)sender!;
			window.Topmost = TopmostEnabled;
		}

		private void OnSizeChange(object? sender, SizeChangedEventArgs e) {
			if (e.WidthChanged) {
				var numColumns = GetNumColumns();
				if (!StaticSize) {
					if (numColumns == 2) {
						Config.Default.Width2Column = e.NewSize.Width;
						Config.Save();
					}
					else if (numColumns == 1) {
						Config.Default.Width1Column = e.NewSize.Width;
						Config.Save();
					}
				}
				
				RefitWindow(true);
			}
		}

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Window_Loaded(object? sender, RoutedEventArgs e) {
			RefitWindow();
		}

		private int GetNumColumns() {
			if (VerticalDisplay) return 1;
			if (HorizontalSpinDisplay) return conditions.Count;
			return conditions.Count switch {
				3 or 4 or 5 => 2,
				_ => 1,
			};
		}

		private int GetNumRows() {
			if (HorizontalSpinDisplay) return 1;
			if (VerticalDisplay) return conditions.Count;
			return conditions.Count switch {
				2 or 3 or 4 => 2,
				5 => 3,
				_ => 1,
			};
		}

		private void SetupGameUI() {
			if (Mode == GameMode.Bingo)
				SetupBingoUI();
			else
				SetupSpinUI();
			DoHackyWindowSizeFix(true, 40);
		}

		private void SetupSpinUI() {
			var numColumns = GetNumColumns();
			ContentGrid.Columns = numColumns;
			ContentGrid.Rows = 0;
			ContentGrid.Children.Clear();
			var thisCodeIsStupid = false;

			if (StaticSize && !StaticSizeLHS && !VerticalDisplay) {
				thisCodeIsStupid = true;
				switch (conditions.Count) {
					case 3:
						SetupSpinUI_AddCondition(conditions[1]);
						SetupSpinUI_AddCondition(conditions[0]);
						SetupSpinUI_AddCondition(conditions[2]);
						break;
					case 4:
						SetupSpinUI_AddCondition(conditions[1]);
						SetupSpinUI_AddCondition(conditions[0]);
						SetupSpinUI_AddCondition(conditions[3]);
						SetupSpinUI_AddCondition(conditions[2]);
						break;
					case 5:
						SetupSpinUI_AddCondition(conditions[1]);
						SetupSpinUI_AddCondition(conditions[0]);
						SetupSpinUI_AddCondition(conditions[3]);
						SetupSpinUI_AddCondition(conditions[2]);
						SetupSpinUI_AddCondition(conditions[4]);
						break;
					default:
						thisCodeIsStupid = false;
						break;
				}
			}
			if (!thisCodeIsStupid) {
				foreach (var condition in conditions) {
					SetupSpinUI_AddCondition(condition);
				}
			}
		}

		private void SetupBingoUI() {
			ContentGrid.Children.Clear();
			if (card == null) return;
			var size = (int)Math.Floor(Math.Sqrt(card.Tiles.Count));
            ContentGrid.Columns = size;
			ContentGrid.Rows = size;
            foreach (var tile in card.Tiles) {
                var control = new ContentPresenter {
                    Content = tile,
                    DataContext = tile,
                    ContentTemplate = (DataTemplate)Resources["BingoTileDataTemplate"],
                };
                ContentGrid.Children.Add(control);
            }
        }

		private void SetupSpinUI_AddCondition(SpinCondition condition) {
			var control = new ContentPresenter {
				Content = condition,
				DataContext = condition,
				ContentTemplate = (DataTemplate)Resources["SpinConditionDataTemplate"],
			};
			ContentGrid.Children.Add(control);
		}

		private void RefitWindow(bool keepSize = false) {
			if (Mode == GameMode.Bingo) {
                var h = 53 + (ShowStatusBar ? StatusGrid.ActualHeight : 0);
                RefitWindow_Bingo(keepSize);
                SizeToContent = SizeToContent.Manual;
				MinHeight = Width * .75 + h;
				MaxHeight = Width * .75 + h;
				Height = Width * .75 + h;
				
                double v = (Width / 50) * 1.3;
                SpinFontSize = Math.Max(11.5, v);
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
                return;
			}

			if (StaticSize) {
				var h = SpinGridHeight * GetNumRows() + 53 + (ShowStatusBar ? StatusGrid.ActualHeight : 0);
				SizeToContent = SizeToContent.Manual;
				MinHeight = h;
				MaxHeight = h;
				double v = (Width / 50) * 1.1;
				SpinFontSize = Math.Max(11.5, v);
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
				return;
			}

			var numColumns = StaticSize ? 2 : GetNumColumns();

			var width = Math.Max(numColumns switch {
				1 => Config.Default.Width1Column,
				2 => Config.Default.Width2Column,
				_ => Width,
			}, 450);

			if (!keepSize) {
				this.Width = width;
			}

			// Scale content (poorly)
			var scale = GetContentScale();
			//double rowScaling = Math.Ceiling((double)numItems / numColumns);
			double contentHeight = width / scale;
			contentHeight += HeaderGrid.ActualHeight + StatusGrid.ActualHeight + 2;
			MinHeight = contentHeight;
			MaxHeight = contentHeight;
			SizeToContent = SizeToContent.Manual;

			ScaleFonts(width);
		}

		private void RefitWindow_Bingo(bool keepSize = false) {
			
		}


        private double GetContentScale() {
			if (VerticalDisplay) {
				return conditions.Count switch {
					1 => 2.75,
					2 => 1.5,
					4 => 0.7,
					5 => 0.6,
					_ => 1,
				};
			}
			return conditions.Count switch {
				1 or 3 or 4 => 3.0,
				2 => 1.5,
				5 => 2.0,
				_ => 1,
			};
		}

		private void ScaleFonts(double width) {
			var numColumns = GetNumColumns();
			// Scale fonts (poorly)
			double w = width;
			var scale = numColumns > 1 ? 0.75 : 1.3;
			double v = (w / 35) * scale;
			SpinFontSize = Math.Max(10, v);
		}

		private void Window_Closing(object? sender, CancelEventArgs e) {
			timer.Stop();
			StatisticsWindowInst?.Close();
			Config.Save(true);
		}

		private void CopySpinCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			var text = new Spin(conditions).ToString();

			// Have a few attempts at accessing the clipboard. In Windows this is usually a data race vs. other processes.
			for (var i = 0; i < 10; ++i) {
				try {
					Clipboard.SetText(text);
					return;
				} catch {
				}
				System.Threading.Thread.Sleep(10);
			}

			// Give up for now and inform the use to retry.
			MessageBox.Show(
				"Clipboard access failed. Another process may have attempted to access the clipboard at the same time. Please try again.",
				"Copy Spin Failed"
			);
		}

		private void CopySpinLinkCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			var text = new Spin(conditions).ToString().Replace("'", "");
			var url = "https://croupier.nbeatz.net/?spin=" + Strings.URLCharacterRegex.Replace(text, "-").Replace("--", "-");

			// Have a few attempts at accessing the clipboard. In Windows this is usually a data race vs. other processes.
			for (var i = 0; i < 10; ++i) {
				try {
					Clipboard.SetText(url);
					return;
				} catch {
				}
				System.Threading.Thread.Sleep(10);
			}

			// Give up for now and inform the use to retry.
			MessageBox.Show(
				"Clipboard access failed. Another process may have attempted to access the clipboard at the same time. Please try again.",
				"Copy Spin Link Failed",
				MessageBoxButton.OK,
				MessageBoxImage.Warning
			);
		}

		private void PasteSpinCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			var text = Clipboard.GetText();
			if (SpinParser.TryParse(text, out var spin)) {
				if (this.spin == null || this.spin.ToString() == spin!.ToString())
					return;
				SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
				ResetCurrentSpinProgress();
				ResetTimer();
				TrackNewSpin();
			}
		}

		private void EditMapPoolCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (EditMapPoolWindowInst != null) {
				EditMapPoolWindowInst.Activate();
				return;
			}
			EditMapPoolWindowInst = new() {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditMapPoolWindowInst.AddMissionToPool += EditMapPoolWindow_AddMissionToPool;
			EditMapPoolWindowInst.RemoveMissionFromPool += EditMapPoolWindow_RemoveMissionFromPool;
			EditMapPoolWindowInst.Closed += (object? sender, EventArgs e) => {
				EditMapPoolWindowInst = null;
			};
			EditMapPoolWindowInst.UpdateMissionPool(missionPool);
			EditMapPoolWindowInst.Show();
		}

		private void EditRulesetsCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			LoadRouletteConfiguration();

			if (EditRulesetWindowInst != null) {
				EditRulesetWindowInst.Activate();
				return;
			}
			EditRulesetWindowInst = new(rulesets) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditRulesetWindowInst.Closed += (object? sender, EventArgs e) => {
				EditRulesetWindowInst = null;
			};
			EditRulesetWindowInst.Show();
		}

		private void EditHotkeysCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (EditHotkeysWindowInst != null) {
				EditHotkeysWindowInst.Activate();
				return;
			}

			EditHotkeysWindowInst = new() {
				Owner = this,
				WindowStartupLocation= WindowStartupLocation.CenterOwner,
			};
			EditHotkeysWindowInst.Closed += (object? sender, EventArgs e) => {
				EditHotkeysWindowInst = null;
			};
			EditHotkeysWindowInst.Show();
		}

		private void ResetStreakCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			ResetStreak();
		}

		private void ShowStatisticsWindowCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (StatisticsWindowInst != null) {
				StatisticsWindowInst.Activate();
				return;
			}
			StatisticsWindowInst = new() {
				Owner = null,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			StatisticsWindowInst.Closed += (object? sender, EventArgs e) => {
				StatisticsWindowInst = null;
			};
			StatisticsWindowInst.Show();
		}

		private void StreakSettingsCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			TimerSettingsCommand_Executed(sender, e);
		}

		private void TimerSettingsCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (TimerSettingsWindowInst != null) {
				TimerSettingsWindowInst.Activate();
				return;
			}
			TimerSettingsWindowInst = new() {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			TimerSettingsWindowInst.Closed += (object? sender, EventArgs e) => {
				TimerSettingsWindowInst = null;
			};
			TimerSettingsWindowInst.ResetStreak += (object? sender, int _) => ResetStreak();
			TimerSettingsWindowInst.ResetStreakPB += (object? sender, int _) => UpdateStreakStatus();
			TimerSettingsWindowInst.Show();
		}

		private void CheckDailySpinsCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			CheckDailySpinsAsync();
		}

		private void DailySpin1Command_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (dailySpin1 == null) return;
			if (SpinParser.TryParse(dailySpin1.spin, out var spin)) {
				SetSpin(spin!);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
				ResetCurrentSpinProgress();
				ResetTimer();
				TrackNewSpin();
			}
		}
		
		private void DailySpin2Command_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (dailySpin2 == null) return;
			if (SpinParser.TryParse(dailySpin2.spin, out var spin)) {
				SetSpin(spin!);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
				ResetCurrentSpinProgress();
				ResetTimer();
				TrackNewSpin();
			}
		}

		private void DailySpin3Command_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (dailySpin3 == null) return;
			if (SpinParser.TryParse(dailySpin3.spin, out var spin)) {
				SetSpin(spin!);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
				ResetCurrentSpinProgress();
				ResetTimer();
				TrackNewSpin();
			}
		}

		private void DailySpin1Command_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = dailySpin1 != null && SpinParser.TryParse(dailySpin1.spin, out _);
		}

		private void DailySpin2Command_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = dailySpin2 != null && SpinParser.TryParse(dailySpin2.spin, out _);
		}

		private void DailySpin3Command_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = dailySpin3 != null && SpinParser.TryParse(dailySpin3.spin, out _);
		}

		private void DebugWindowCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
#if DEBUG
			e.CanExecute = true;
#endif
		}

		private void DebugWindowCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (DebugWindowInst != null) {
				DebugWindowInst.Activate();
				return;
			}
			DebugWindowInst = new(this) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			DebugWindowInst.Closed += (object? sender, EventArgs e) => {
				DebugWindowInst = null;
			};
			DebugWindowInst.Show();
		}

		private void ShowHitmapsWindowCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (HitmapsWindowInst != null) {
				HitmapsWindowInst.Activate();
				return;
			}
			HitmapsWindowInst = new() {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			HitmapsWindowInst.Closed += (object? sender, EventArgs e) => {
				HitmapsWindowInst = null;
			};
			HitmapsWindowInst.Show();
		}

		private void ShowLiveSplitWindowCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (LiveSplitWindowInst != null) {
				LiveSplitWindowInst.Activate();
				return;
			}
			LiveSplitWindowInst = new() {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			LiveSplitWindowInst.Closed += (object? sender, EventArgs e) => {
				LiveSplitWindowInst = null;
			};
			LiveSplitWindowInst.Show();
		}

		private void EditSpinCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (EditSpinWindowInst != null) {
				EditSpinWindowInst.Activate();
				return;
			}
			EditSpinWindowInst = new(conditions) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditSpinWindowInst.SetCondition += EditSpinWindow_SetCondition;
			EditSpinWindowInst.Closed += (object? sender, EventArgs e) => {
				EditSpinWindowInst = null;
			};
			EditSpinWindowInst.Show();
		}

		private void PrevSpinCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			PreviousSpin();
		}

		private void PrevSpinCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = spinHistory.Count > 1 && spinHistoryIndex < spinHistory.Count;
		}

		private void NextSpinCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = spinHistoryIndex > 1;
		}

		private void NextSpinCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			NextSpin();
		}

		private void PrevMapCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void NextMapCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void NextMapCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (spin == null) return;
			var nextMission = MissionID.NONE;
			var nextMissionGroupOrderDiff = -1;
			var current = spin.Mission.GetGroupOrder();
			for (var i = 0; i < missionPool.Count; ++i) {
				var diff = missionPool[i].GetGroupOrder() - current;
				if (diff <= 0) continue;
				if (nextMission != MissionID.NONE && nextMissionGroupOrderDiff < diff)
					continue;
				nextMission = missionPool[i];
				nextMissionGroupOrderDiff = diff;
			}

			if (nextMission != MissionID.NONE)
				Spin(nextMission);
		}

		private void PrevMapCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (spin == null)
				return;
			var nextMission = MissionID.NONE;
			var nextMissionGroupOrderDiff = -1;
			var current = spin.Mission.GetGroupOrder();
			for (var i = 0; i < missionPool.Count; ++i) {
				var diff = missionPool[i].GetGroupOrder() - current;
				if (diff >= 0)
					continue;
				if (nextMission != MissionID.NONE && nextMissionGroupOrderDiff > diff)
					continue;
				nextMission = missionPool[i];
				nextMissionGroupOrderDiff = diff;
			}

			if (nextMission != MissionID.NONE)
				Spin(nextMission);
		}

		private void RespinCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			Spin();
		}

		private void ShuffleCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			Shuffle();
		}

		private void ResetTimerCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			timerManuallyStopped = false;
			StopTimer();
			ResetTimer();
		}

		private void StartTimerCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			timerManuallyStopped = false;
			StartTimer();
		}

		private void StartTimerCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = timerStopped;
		}

		private void StopTimerCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			timerManuallyStopped = true;
			StopTimer();
		}

		private void StopTimerCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = !timerManuallyStopped;
		}

		private void ShuffleCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = ShuffleButtonEnabled;
		}

		private void PasteSpinCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = SpinParser.TryParse(Clipboard.GetText(), out _);
		}

		private void CheckUpdateCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			DoUpdateCheck(true);
		}

		private void Command_AlwaysCanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private async void CheckDailySpinsAsync() {
			try {
				var result = await DailySpinChecker.CheckForDailySpinsAsync();

				for (var i = 0; i < result.Length; ++i) {
					var spin = result[i];
					if (i == 0) dailySpin1 = spin;
					else if (i == 1) dailySpin2 = spin;
					else if (i == 2) dailySpin3 = spin;
				}

				OnPropertyChanged(nameof(DailySpin1Completed));
				OnPropertyChanged(nameof(DailySpin2Completed));
				OnPropertyChanged(nameof(DailySpin3Completed));

				OnPropertyChanged(nameof(DailySpin1Label));
				OnPropertyChanged(nameof(DailySpin2Label));
				OnPropertyChanged(nameof(DailySpin3Label));

				OnPropertyChanged(nameof(DailySpin1Tooltip));
				OnPropertyChanged(nameof(DailySpin2Tooltip));
				OnPropertyChanged(nameof(DailySpin3Tooltip));
			} catch (Exception e) {
				MessageBox.Show(
					$"Error checking for daily spins.\n{e.Message}",
					"Daily Spin Check",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}
		}

		private static async void DoUpdateCheck(bool informOnFail = false) {
			try {
				var result = await UpdateChecker.CheckForUpdateAsync();
				if (result == null) {
					if (informOnFail) {
						MessageBox.Show(
							$"Croupier is up-to-date!",
							"Update Check - Croupier",
							MessageBoxButton.OK
						);
					}
					return;
				}

				var msgRes = MessageBox.Show(
					$"Click Yes to download {result.name}. Click No to update later.",
					"Update Available - Croupier",
					MessageBoxButton.YesNo
				);
				if (msgRes == MessageBoxResult.Yes)
					UpdateChecker.OpenUrl(result.url);
			}
			catch (Exception ex) {
				if (informOnFail) MessageBox.Show(
					$"Update check error: {ex.Message}",
					"Update Check - Croupier",
					MessageBoxButton.OK
				);
			}
		}

		private void BingoTile_MouseDown(object sender, RoutedEventArgs e) {
			var tile = (BingoTile)((Button)sender).DataContext;
			tile.Complete = !tile.Complete;
			//SetupBingoUI();
		}
	}
}
