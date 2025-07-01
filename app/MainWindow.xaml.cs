using Croupier.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Croupier
{
	public partial class MenuEntryBase(string name) : INotifyPropertyChanged {
		public string Name { get; set; } = name;

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class GameModeEntry(GameMode mode, string name) : MenuEntryBase(name) {
		public GameMode Mode { get; set; } = mode;
		public int Index => (int)Mode;
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
	}

	public class BingoTileTypeEntry(BingoTileType type, string name) : MenuEntryBase(name) {
		public BingoTileType Type { get; set; } = type;
		public int Index => (int)Type;
		public bool IsSelected {
			get => Config.Default.BingoTileType == Type;
			set {
				Config.Default.BingoTileType = Type;
				OnPropertyChanged(nameof(IsSelected));
			}
		}

		public void Refresh() {
			OnPropertyChanged(nameof(IsSelected));
			OnPropertyChanged(nameof(Name));
		}
	}

	public class TargetNameFormatEntry(TargetNameFormat id, string name) : MenuEntryBase(name) {
		public TargetNameFormat ID { get; set; } = id;
		public int Index => (int)ID;
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
	}

	public class ContextSubmenuEntry(string name, int index) : MenuEntryBase(name) {
		private bool isSelected = false;
		private int _index = index;

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

		public bool IsBingoMode => GameController.Main.Mode == GameMode.Bingo || GameController.Main.Mode == GameMode.RouletteBingo;
		public bool IsRouletteMode => GameController.Main.Mode == GameMode.Roulette || GameController.Main.Mode == GameMode.RouletteBingo;
		public bool IsHybridMode => GameController.Main.Mode == GameMode.RouletteBingo;

		public bool BingoSize3x3 => GameController.Main.Bingo.CardSize == 3 * 3;
		public bool BingoSize4x4 => GameController.Main.Bingo.CardSize == 4 * 4;
		public bool BingoSize5x5 => GameController.Main.Bingo.CardSize == 5 * 5;
		public bool BingoSize6x6 => GameController.Main.Bingo.CardSize == 6 * 6;
		public bool BingoSize7x7 => GameController.Main.Bingo.CardSize == 7 * 7;
		public bool BingoSize8x8 => GameController.Main.Bingo.CardSize == 8 * 8;

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

		public double SpinSmallFontSize => _spinFontSize * 0.8;

		public double SpinNTKOHeight => UseNoKOBanner ? _spinFontSize * 1.1 : 0;

		public static double BingoFontSizeScale => GameController.Main.Bingo.Card?.Size.Rows switch {
			3 => 1.2,
			4 => 1.1,
			5 => 1,
			6 => .8,
			7 => .7,
			8 => .6,
			_ => 1,
		};

		public double SpinFontSize {
			get => IsBingoMode ? _spinFontSize * BingoFontSizeScale : _spinFontSize;
			private set {
				if (_spinFontSize == value) return;
				_spinFontSize = value;
				OnPropertyChanged(nameof(SpinFontSize));
				OnPropertyChanged(nameof(SpinSmallFontSize));
				OnPropertyChanged(nameof(SpinNTKOHeight));
				OnPropertyChanged(nameof(BingoGroupFontSize));
			}
		}

		public double BingoFontSize => IsHybridMode ? SpinFontSize * .51 : SpinFontSize * .7;
		public double BingoGroupFontSize => BingoFontSize * .8;

		public static bool ShuffleButtonEnabled => GameController.Main.MissionPool.Count > 0;

		public bool IsBingoTileTypeObjective => GameController.Main.Bingo.TileType == BingoTileType.Objective;
		public bool IsBingoTileTypeComplication => GameController.Main.Bingo.TileType == BingoTileType.Complication;
		public bool IsBingoTileTypeMixed => GameController.Main.Bingo.TileType == BingoTileType.Mixed;

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

		public bool EnableGroupTileColors {
			get => Config.Default.EnableGroupTileColors;
			set {
				GameController.Main.Bingo.EnableGroupTileColours = value;
				OnPropertyChanged(nameof(EnableGroupTileColors));
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

				if (GameController.Main.Roulette.Spin is Spin spin) {
					foreach (var cond in spin.Conditions)
						cond.ForceUpdate();
				}
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
				UpdateStreakStatus();
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
			=> IsBingoMode
				? FlowDirection.LeftToRight
				: (!StaticSize ? FlowDirection.LeftToRight : (StaticSizeLHS ? FlowDirection.LeftToRight : FlowDirection.RightToLeft));
		public HorizontalAlignment SpinAlignHorz
			=> !StaticSize ? HorizontalAlignment.Stretch : (StaticSizeLHS ? HorizontalAlignment.Left : HorizontalAlignment.Right);
		public bool StatusAlignLeft => (StaticSize ? StaticSizeLHS : !RightToLeft);
		public static double BingoContentRows => GameController.Main.Bingo.Card?.Size.Rows ?? 5;
		public static double BingoContentCols => GameController.Main.Bingo.Card?.Size.Columns ?? 5;
		public double ContentHeight => Height - HeaderGrid.Height - (ShowStatusBar ? StatusGrid.Height : 0);
		public static double BingoBorderMarginFix => BingoContentRows switch {
			4 => 3.5,
			5 => 2.8,
			6 => 2.2,
			_ => 2.2,
		};
		public double SpinGridWidth => IsRouletteMode
			? (!StaticSize ? double.NaN : (VerticalDisplay ? Width : Width / 2) - 6)
			: Width / BingoContentCols;
		public double SpinGridHeight => IsRouletteMode
			? !StaticSize ? double.NaN : SpinGridWidth * 0.33
			: (ContentHeight / BingoContentRows) - BingoBorderMarginFix;
		public double GridTileWidth => IsHybridMode ? (Width / BingoContentCols) * .47
			: Width / BingoContentCols;
		public double GridTileHeight => ((ContentHeight / BingoContentRows) - BingoBorderMarginFix);

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

		public ObservableCollection<GameModeEntry> GameModeEntries = [
			new(GameMode.Roulette, "Roulette"),
			new(GameMode.Bingo, "Bingo"),
			new(GameMode.RouletteBingo, "Hybrid"),
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

		private readonly List<MissionID> missionPool = [];
		private Entrance? usedEntrance = null;
		private string[] loadout = [];
		private bool disableClientUpdate = false;
		private bool timerStopped = true;
		private bool timerManuallyStopped = false;
		private DateTime timerStart = DateTime.Now;
		private TimeSpan? timeElapsed = null;
		private List<Target> trackedValidKills = [];
		private DateTime? autoSpinSchedule = null;
		private MissionID autoSpinMission = MissionID.NONE;
		private readonly LiveSplitClient liveSplit;
		private readonly DispatcherTimer timer;
		private readonly ObservableCollection<MissionComboBoxItem> missionListItems = [];

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

		private void SetupHotkeys() {
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
		}

		private void SetupVersionTooltip() {
			var ver = Assembly.GetExecutingAssembly().GetName().Version;
			if (ver != null) {
				var logoVer = Version.Parse(ver.ToString());
				var logoVerStr = logoVer.Major + "." + logoVer.Minor;
				if (logoVer.Build != 0)
					logoVerStr += "." + logoVer.Build;
				Logo.ToolTip = "Croupier v" + logoVerStr;
			}
		}

		private DispatcherTimer SetupTimer() {
			var timer = new DispatcherTimer {
				Interval = TimeSpan.FromMilliseconds(1)
			};
			timer.Tick += (sender, e) => {
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
			return timer;
		}

		private void SetupGameControllerEvents() {
			GameController.Main.MissionPoolUpdated += (sender, mission) => {
				// Send mission pool to client, save in config and update properties
				SendMissionsToClient();
				SaveCustomMissionPool();
				OnPropertyChanged(nameof(ShuffleButtonEnabled));
			};
			GameController.Main.Roulette.SpinEdited += (sender, spin) => {
				CancelScheduledAutoSpin();
				UpdateSpinHistory();
				PostConditionUpdate();
			};
			GameController.Main.Bingo.CardUpdated += (sender, card) => {
				PostConditionUpdate();
			};
			GameController.Main.MissionCompleted += (sender, mc) => {
				HandleTimingOnSpinComplete(mc);
				TrackGameMissionCompletion(mc);
			};
			GameController.Main.MissionChanged += (sender, mission) => {
				// Sync the mission select combo box when the mission is updated.
				SyncMissionSelect(mission);
			};
			GameController.Main.RoundStarted += (sender, mission) => {
				if (GameController.Main.IsPlayingRoulette) {
					spinHistoryIndex = 1;
					PushCurrentSpinToHistory();
				}

				HandleTimingOnNewRound(mission);
				TrackNewSpin();
			};
			GameController.Main.GameModeChanged += (sender, mode) => {
				foreach (var entry in GameModeEntries)
					entry.Refresh();
				SetupGameMode();
				LoadBingoConfiguration();
				GameController.Main.AssureRoundIsStarted();
				PostConditionUpdate();
			};
			GameController.Main.StreakUpdated += (sender, mode) => {
				UpdateStreakStatus();
			};
			GameController.Main.Roulette.KillValidationUpdated += (sender, spin) => {
				TrackKillValidation(spin);
			};
		}

		private void SetupSocketServerEvents() {
			CroupierSocketServer.Connected += (sender, _) => {
				SendMissionsToClient();
				SendSpinToClient();
				SendSpinLockToClient();
				SendStreakToClient();
				SendTimerToClient();
			};
			CroupierSocketServer.Respin += (sender, id) => Spin(id);
			CroupierSocketServer.AutoSpin += (sender, id) => {
				if (SpinLock) return;
				if (Config.Default.AutoSpinCountdown > 0)
					ScheduleAutoSpin(TimeSpan.FromSeconds(Config.Default.AutoSpinCountdown), id);
				else
					AutoSpin(id);
			};
			CroupierSocketServer.Random += (sender, _) => GameController.Main.Shuffle();
			CroupierSocketServer.ToggleSpinLock += (sender, _) => {
				disableClientUpdate = true;
				SpinLock = !SpinLock;
				disableClientUpdate = false;
			};
			CroupierSocketServer.ToggleTimer += (sender, enable) => {
				ShowTimer = enable;
			};
			CroupierSocketServer.PauseTimer += (sender, pause) => {
				if (pause) StopTimer();
				else ResumeTimer();
				isLoadRemoving = false;
				timerManuallyStopped = pause;
			};
			CroupierSocketServer.ResetTimer += (sender, _) => {
				StopTimer();
				ResetTimer();
			};
			CroupierSocketServer.SplitTimer += (sender, _) => {
				SplitTimer();
			};
			CroupierSocketServer.ResetStreak += (sender, _) => GameController.Main.Streak = 0;
			CroupierSocketServer.MissionStart += (sender, start) => {
				TrackGameMissionAttempt(start);
			};
			CroupierSocketServer.MissionOutroBegin += (sender, _) => {
				if (Config.Default.TimerPauseDuringOutro)
					StopTimer();
			};
			CroupierSocketServer.Missions += (sender, missions) => {
				missionPool.Clear();
				missionPool.AddRange(missions);
				EditMapPoolWindowInst?.UpdateMissionPool(missionPool);
			};
			CroupierSocketServer.Prev += (sender, _) => {
				disableClientUpdate = true;
				Previous();
				disableClientUpdate = false;
				SendSpinToClient();
			};
			CroupierSocketServer.Next += (sender, _) => {
				disableClientUpdate = true;
				Next();
				disableClientUpdate = false;
				SendSpinToClient();
			};
			CroupierSocketServer.SpinData += (sender, data) => {
				if (SpinParser.TryParse(data, out var spin)) {
					disableClientUpdate = true;
					GameController.Main.Roulette.SetSpin(spin);
					StartTimer();
					disableClientUpdate = false;
				}
			};
			CroupierSocketServer.LoadStarted += (sender, _) => {
				HandleTimingOnLoadStart();
			};
			CroupierSocketServer.LoadFinished += (sender, _) => {
				HandleTimingOnLoadFinish();
			};
			HitmapsSpinLink.ReceiveNewSpinData += (sender, data) => {
				if (SpinParser.TryParse(data, out var spin)) {
					GameController.Main.Roulette.SetSpin(spin!);
					StartTimer(true);
					TrackNewSpin();
				}
			};
		}

		public MainWindow() {
			InitializeComponent();
			liveSplit = ((App)Application.Current).LiveSplitClient;
			DataContext = this;
			MainContextMenu.DataContext = this;
			SizeToContent = SizeToContent.Height;

			PropertyChanged += MainWindow_PropertyChanged;

			RegisterWindowPlace();
			Focus();
			SetupVersionTooltip();
			timer = SetupTimer();

			SetupGameControllerEvents();
			LoadSettings();
			SetupGameMode();
			LoadSpinHistory();
			SetupHotkeys();
			SetupSocketServerEvents();

			if (IsBingoMode)
				Spin(MissionID.PARIS_SHOWSTOPPER);

			SendMissionsToClient();
			SendSpinToClient();
		}

		private void SyncMissionSelect(MissionID mission) {
			var idx = MissionListItems.ToList().FindIndex(item => item.ID == mission);
			MissionSelect.SelectedIndex = idx;
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
			if (IsRouletteMode) LoadRouletteConfiguration();
			else LoadBingoConfiguration();

			UpdateStreakStatus();

			MissionSelect.ItemsSource = MissionListItems;
			ContextMenuGameMode.ItemsSource = GameModeEntries;
			ContextMenuGameMode.DataContext = this;
			ContextMenuBingoMode.DataContext = this;
			ContextMenuTargetNameFormat.ItemsSource = TargetNameFormatEntries;
			ContextMenuTargetNameFormat.DataContext = this;
			ContextMenuHistory.ItemsSource = HistoryEntries;
			ContextMenuHistory.DataContext = this;
			ContextMenuBookmarks.ItemsSource = BookmarkEntries;
			ContextMenuBookmarks.DataContext = this;

			ContextMenuHistory.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuCopySpin.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuPasteSpin.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuBookmarks.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuTargetNameFormat.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuRouletteKillConfirmations.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuTopSeparator.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuSpinSeparator.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			ContextMenuBingoMode.Visibility = IsBingoMode ? Visibility.Visible : Visibility.Collapsed;

			DisplayOptionUseNoKOBanner.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			DisplayOptionRTL.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			DisplayOptionVertical.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			DisplayOptionStatic.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			DisplayOptionStaticAlign.Visibility = IsRouletteMode ? Visibility.Visible : Visibility.Collapsed;
			DisplayOptionGroupTileColors.Visibility = IsBingoMode ? Visibility.Visible : Visibility.Collapsed;

			SyncMissionSelect(GameController.Main.MissionID);

			OnPropertyChanged(nameof(IsBingoMode));
			OnPropertyChanged(nameof(IsRouletteMode));
			OnPropertyChanged(nameof(SpinGridWidth));
			OnPropertyChanged(nameof(SpinGridHeight));
			OnPropertyChanged(nameof(ContentGridFlowDir));
			OnPropertyChanged(nameof(ShuffleButtonEnabled));
		}

		private void LoadMainConfiguration(bool reload = false) {
			try {
				Roulette.Main.Load(reload);
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

		private void LoadRouletteConfiguration(bool reload = false) {
			LoadMainConfiguration(reload);
			Ruleset.LoadConfiguration(reload);
		}

		private void LoadBingoConfiguration(bool reload = false) {
			LoadMainConfiguration(reload);
			Bingo.Main.LoadConfiguration(reload);
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

			if (spinHistory.Count > 0)
				SetSpinHistory(1);
			else
				Spin(MissionID.PARIS_SHOWSTOPPER);
		}

		private void LoadSettings() {
			if (!Enum.IsDefined(typeof(MissionPoolPresetID), Config.Default.MissionPool))
				Config.Default.MissionPool = MissionPoolPresetID.MainMissions;

			VerticalDisplay = Config.Default.VerticalDisplay;
			TopmostEnabled = Config.Default.AlwaysOnTop;
			UseNoKOBanner = Config.Default.UseNoKOBanner;
			RightToLeft = Config.Default.RightToLeft;
			StaticSize = Config.Default.StaticSize;
			StaticSizeLHS = Config.Default.StaticSizeLHS;
			ShowTimer = Config.Default.Timer;
			ShowStreak = Config.Default.Streak;
			ShowStreakPB = Config.Default.ShowStreakPB;
			TimerMultiSpin = Config.Default.TimerMultiSpin;
			TimerFractions = Config.Default.TimerFractions;
			KillValidations = Config.Default.KillValidations;
			TargetNameFormat = TargetNameFormatMethods.FromString(Config.Default.TargetNameFormat);
			GameController.Main.LoadConfig(Config.Default);

			foreach (var bookmark in Config.Default.Bookmarks) {
				BookmarkEntries.Add(new(bookmark, BookmarkEntries.Count));
			}
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

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			ThemeManager.SetCurrentTheme(this, new Uri("/Croupier;component/Resources/DarkTheme.xaml", UriKind.Relative));

			if (Config.Default.CheckUpdate) {
				DoUpdateCheck();
				CheckDailySpinsAsync();
			}
		}

		public static void AutoSpin(MissionID id = MissionID.NONE) {
			if (id != MissionID.NONE && GameController.Main.MissionID == id && !GameController.Main.IsFinished)
				return;

			GameController.Main.MissionID = id;
			GameController.Main.StartNewRound();
		}

		public void ScheduleAutoSpin(TimeSpan time, MissionID id = MissionID.NONE) {
			autoSpinSchedule = DateTime.Now + time;
			autoSpinMission = id;
		}

		public void CancelScheduledAutoSpin() {
			autoSpinSchedule = null;
		}

		public static void Spin(MissionID id = MissionID.NONE) {
			if (id != MissionID.NONE)
				GameController.Main.MissionID = id;
			try {
				GameController.Main.StartNewRound();
			}
			catch (CroupierException e) {
				MessageBox.Show($"Error: {e.Message}", "Error - Croupier", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			catch (RouletteSpinException e) {
				MessageBox.Show(e.Message);
				return;
			}
		}

		public bool PushSpinToHistory(Spin spin, bool skipSync = false) {
			if (spin == null || spin.Conditions.Count == 0) return false;
			if (spin.Conditions == GameController.Main.Roulette.Spin?.Conditions) return false;
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

		public void UpdateSpinHistory()
		{
			if (spinHistory.Count == 0) return;
			if (GameController.Main.Roulette.Spin != null) {
				//var spin = spinHistory[^1];
				//spin.Conditions.Clear();
				//spin.Conditions.AddRange(GameController.Main.Roulette.Spin.Conditions);
				SyncHistoryEntries();
			}
			return;
		}

		public bool PushCurrentSpinToHistory() {
			if (GameController.Main.Roulette.Spin == null) return false;
			return PushSpinToHistory(new Spin([..GameController.Main.Roulette.Spin.Conditions]));
		}

		public void Previous() {
			if (spinHistoryIndex < 1) spinHistoryIndex = 1;
			if (spinHistoryIndex >= spinHistory.Count) {
				spinHistoryIndex = spinHistory.Count;
				return;
			}

			SetSpinHistory(spinHistoryIndex + 1);
			GameController.Main.ResetProgress();
			ResetTimer();
		}

		public void Next() {
			if (spinHistoryIndex > spinHistory.Count)
				spinHistoryIndex = spinHistory.Count;
			if (spinHistoryIndex <= 1) {
				spinHistoryIndex = 1;
				return;
			}

			SetSpinHistory(spinHistoryIndex - 1);
			GameController.Main.ResetProgress();
			ResetTimer();
		}

		public void SetSpinHistory(int idx) {
			spinHistoryIndex = idx;
			GameController.Main.Roulette.SetSpin(spinHistory[^spinHistoryIndex]);
			SyncHistoryEntries();
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

		private void PostConditionUpdate() {
			SendSpinToClient();
			SetupGameUI();
			RefitWindow();
		}

		private void UpdateStreakStatus() {
			SendStreakToClient();

			if (Config.Default.ShowStreakPB)
				Streak.Text = "Streak: " + GameController.Main.Streak + " (PB: " + Config.Default.StreakPB + ")";
			else
				Streak.Text = "Streak: " + GameController.Main.Streak;
		}

		private void TrackNewSpin() {
			var spin = GameController.Main.Roulette.Spin;
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

		private void TrackKillValidation(Spin spin) {
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
			var spin = GameController.Main.Roulette.Spin;
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

			var spin = GameController.Main.Roulette.Spin;
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
				RTA = GameController.Main.RoundTimeElapsed.TotalSeconds,
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
						break;
				}
			}

			timerStopped = false;
			timerStart = timeElapsed.HasValue ? DateTime.Now - timeElapsed.Value : DateTime.Now;
			GameController.Main.ResumeRoundTimer();

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
				GameController.Main.PauseRoundTimer();
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
			timeElapsed = null;
			timerStopped = false;
			GameController.Main.ResumeRoundTimer();
		}

		private void ResetTimer() {
			liveSplit.Reset();
			if (!timerStopped)
				liveSplit.StartOrSplit();

			timeElapsed = null;
			timerStart = DateTime.Now;
			GameController.Main.ResetRoundTimer();
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
					if (!GameController.Main.IsFinished)
						ResumeTimer();
					break;
			}
			
			SendTimerToClient();
		}

		private void HandleTimingOnNewRound(MissionID mission) {
			if (!TimerMultiSpin || Config.Default.TimerResetMission == mission) {
				ResetTimer();
				StopTimer();
			}

			StartTimer();

			SendTimerToClient();
		}

		private void HandleTimingOnSpinComplete(MissionCompletion completion) {
			if (!Config.Default.SplitRequiresSA || completion.SA)
				SplitTimer();

			switch (Config.Default.TimingMode) {
				case TimingMode.IGT:
					timeElapsed = (timeElapsed ?? TimeSpan.Zero) + TimeSpan.FromSeconds(completion.IGT);
					SendTimerToClient();
					break;
				case TimingMode.Spin:
					StopTimer();
					break;
			}

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
			GameController.Main.Roulette.Spin?.Conditions.ForEach(condition => {
				if (spinData.Length > 0) spinData += ", ";
				spinData += $"{condition.ToString(TargetNameFormat.Initials)}";
			});
			CroupierSocketServer.Send("SpinData:" + spinData);
		}

		public void SendMissionsToClient() {
			if (disableClientUpdate) return;
			var missions = "";
			GameController.Main.MissionPool.ForEach(mission => {
				var miss = Mission.TryGet(mission);
				if (miss == null) return;
				missions += missions.Length > 0 ? $",{miss.Codename}" : miss.Codename;
			});
			CroupierSocketServer.Send("Missions:" + missions);
		}

		public void SendStreakToClient() {
			if (disableClientUpdate) return;
			CroupierSocketServer.Send($"Streak:{GameController.Main.Streak}");
		}

		private void OnMouseDown(object? sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void MissionSelect_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count == 0) return;
			var item = (MissionComboBoxItem)e.AddedItems[0]!;
			GameController.Main.SelectNewMission(item.ID);
		}

		private ICommand? _gameModeSelectCommand;
		private ICommand? _bingoModeSelectCommand;
		private ICommand? _setBoardSizeCommand;
		private ICommand? _historyEntrySelectCommand;
		private ICommand? _bookmarkEntrySelectCommand;
		private ICommand? _targetNameFormatSelectCommand;

		public ICommand HistoryEntrySelectCommand => _historyEntrySelectCommand ??= new RelayCommand(param => HistoryEntrySelected(param));

		public ICommand BookmarkEntrySelectCommand  => _bookmarkEntrySelectCommand ??= new RelayCommand(param => BookmarkEntrySelected(param));

		public ICommand TargetNameFormatSelectCommand => _targetNameFormatSelectCommand ??= new RelayCommand(param => TargetNameFormatSelected(param));

		public ICommand GameModeSelectCommand => _gameModeSelectCommand ??= new RelayCommand(param => GameModeSelected(param));

		public ICommand BingoModeSelectCommand => _bingoModeSelectCommand ??= new RelayCommand(param => BingoModeSelected(param));

		public ICommand SetBoardSizeCommand => _setBoardSizeCommand ??= new RelayCommand(param => BingoSizeSelected(param));

		private void HistoryEntrySelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= spinHistory.Count) return;
			SetSpinHistory(Math.Abs(index.Value - spinHistory.Count));
		}

		private void BookmarkEntrySelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= BookmarkEntries.Count) return;
			if (index == 0) {
				if (GameController.Main.Roulette.Spin == null) return;
				BookmarkEntries.Add(new(
					new Spin(GameController.Main.Roulette.Spin.Conditions).ToString(),
					BookmarkEntries.Count
				));
				SyncBookmarks();
			}
			else if (index == 1) {
				if (GameController.Main.Roulette.Spin == null) return;
				var currentSpinStr = GameController.Main.Roulette.Spin.ToString();
				for (int i = 2; i < BookmarkEntries.Count; ++i) {
					if (currentSpinStr != BookmarkEntries[i].Name) continue;
					BookmarkEntries.RemoveAt(i);
					for (; i < BookmarkEntries.Count; ++i)
						--BookmarkEntries[i].Index;
					break;
				}
				SyncBookmarks();
			}
			else if (SpinParser.TryParse(BookmarkEntries[index.Value].Name, out var spin) && spin is not null) {
				GameController.Main.Roulette.SetSpin(spin);
				SyncHistoryEntries();
			}
		}

		private void GameModeSelected(object param) {
			var index = param as int?;
			if (index == null || index < 0 || index >= GameModeEntries.Count)
				return;
			GameController.Main.Mode = GameModeEntries[index.Value].Mode;
			OnPropertyChanged(nameof(BingoFontSize));
			OnPropertyChanged(nameof(BingoGroupFontSize));
			OnPropertyChanged(nameof(BingoFontSizeScale));
		}

		private void BingoModeSelected(object param) {
			if (param is not BingoTileType type)
				return;
			GameController.Main.Bingo.TileType = type;
			OnPropertyChanged(nameof(IsBingoTileTypeComplication));
			OnPropertyChanged(nameof(IsBingoTileTypeObjective));
			OnPropertyChanged(nameof(IsBingoTileTypeMixed));
		}

		private void BingoSizeSelected(object param) {
			GameController.Main.Bingo.CardSize = param switch {
				"3x3" => 3 * 3,
				"4x4" => 4 * 4,
				"5x5" => 5 * 5,
				"6x6" => 6 * 6,
				"7x7" => 7 * 7,
				//"8x8" => 8 * 8,
				_ => 5 * 5,
			};

			OnPropertyChanged(nameof(BingoSize3x3));
			OnPropertyChanged(nameof(BingoSize4x4));
			OnPropertyChanged(nameof(BingoSize5x5));
			OnPropertyChanged(nameof(BingoSize6x6));
			OnPropertyChanged(nameof(BingoSize7x7));
			OnPropertyChanged(nameof(BingoSize8x8));
			OnPropertyChanged(nameof(SpinGridHeight));
			OnPropertyChanged(nameof(SpinGridWidth));
			DoHackyWindowSizeFix();
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
				var numColumns = GetNumColumnsForRoulette();
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

		private int GetNumColumnsForRoulette() {
			if (!IsRouletteMode) return 0;
			if (VerticalDisplay) return 1;
			if (GameController.Main.Roulette.Spin == null) return 0;
			if (HorizontalSpinDisplay) return GameController.Main.Roulette.Spin.Conditions.Count;
			return GameController.Main.Roulette.Spin.Conditions.Count switch {
				3 or 4 or 5 => 2,
				_ => 1,
			};
		}

		private int GetNumRowsForRoulette() {
			if (HorizontalSpinDisplay) return 1;
			if (GameController.Main.Roulette.Spin == null) return 0;
			if (VerticalDisplay) return GameController.Main.Roulette.Spin.Conditions.Count;
			return GameController.Main.Roulette.Spin.Conditions.Count switch {
				2 or 3 or 4 => 2,
				5 => 3,
				_ => 1,
			};
		}

		private void SetupGameUI() {
			ContentGrid.Children.Clear();

			var bingo = IsBingoMode ? CreateBingoUI(GameController.Main.Bingo) : null;
			var roulette = IsRouletteMode ? SetupSpinUI() : null;
			var numRouletteCols = GetNumColumnsForRoulette();
			
			if (bingo != null) ContentGrid.Children.Add(bingo);
			if (roulette != null) {
				if (bingo == null || numRouletteCols != 2) ContentGrid.Children.Add(roulette);
			}

			ContentGrid.Columns = 0;
			ContentGrid.Rows = ContentGrid.Children.Count;
			
			if (IsHybridMode) {
				bingo!.Margin = new Thickness(1, 0, 1, 0);

				switch (numRouletteCols) {
					case 1:
						ContentGrid.Columns = ContentGrid.Children.Count;
						ContentGrid.Rows = 0;
						break;
					case 2:
						ContentGrid.Columns = 0;
						ContentGrid.Rows = 1;
						break;
				}
			}

			DoHackyWindowSizeFix(true, 40);
		}

		private UniformGrid SetupSpinUI() {
			var grid = new UniformGrid();
			var numColumns = GetNumColumnsForRoulette();
			grid.Columns = numColumns;
			grid.Rows = 0;
			var thisCodeIsStupid = false;

			if (StaticSize && !StaticSizeLHS && !VerticalDisplay) {
				thisCodeIsStupid = true;
				switch (GameController.Main.Roulette.Spin?.Conditions.Count ?? 0) {
					case 3:
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[1]));
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[0]));
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[2]));
						break;
					case 4:
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[1]));
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[0]));
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[3]));
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[2]));
						break;
					case 5:
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[1]));
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[0]));
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[3]));
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[2]));
						grid.Children.Add(CreateSpinConditionUI(GameController.Main.Roulette.Spin!.Conditions[4]));
						break;
					default:
						thisCodeIsStupid = false;
						break;
				}
			}
			if (!thisCodeIsStupid && GameController.Main.Roulette.Spin != null) {
				foreach (var condition in GameController.Main.Roulette.Spin.Conditions) {
					grid.Children.Add(CreateSpinConditionUI(condition));
				}
			}
			return grid;
		}

		private ContentPresenter CreateSpinConditionUI(SpinCondition condition) {
			return new ContentPresenter {
				Content = condition,
				DataContext = condition,
				ContentTemplate = (DataTemplate)Resources["SpinConditionDataTemplate"],
			};
		}

		private static UniformGrid CreateBingoGridUI(BingoCardSize size) {
			return new UniformGrid {
				Columns = size.Columns,
				Rows = size.Rows
			};
		}

		private UniformGrid? CreateBingoUI(BingoGame bingo) {
			if (bingo.Card == null) return null;
			var grid = CreateBingoGridUI(bingo.Card.Size);

			foreach (var tile in bingo.Card.Tiles) {
				var control = new ContentPresenter {
					Content = tile,
					DataContext = tile,
					ContentTemplate = (DataTemplate)Resources["BingoTileDataTemplate"],
				};
				grid.Children.Add(control);
			}

			return grid;
		}

		private double GetBingoWindowHeight() {
			return ContentGrid.ActualWidth * .85;
		}

		private double GetBingoFontScale() {
			return Math.Max(11, (Width / 50) * (IsHybridMode ? 1.2 : 1.7));
		}

		private void RefitWindow(bool keepSize = false) {
			SpinFontSize = GetBingoFontScale();

			if (IsBingoMode && !IsRouletteMode) {
				var h = GetBingoWindowHeight();
				SizeToContent = SizeToContent.Manual;
				MinHeight = h;
				MaxHeight = h;
				OnPropertyChanged(nameof(GridTileWidth));
				OnPropertyChanged(nameof(GridTileHeight));
				OnPropertyChanged(nameof(SpinFontSize));
				OnPropertyChanged(nameof(BingoFontSize));
				return;
			}

			OnPropertyChanged(nameof(SpinFontSize));
			OnPropertyChanged(nameof(BingoFontSize));

			if (StaticSize) {
				var h = SpinGridHeight * GetNumRowsForRoulette() + 53 + (ShowStatusBar ? StatusGrid.ActualHeight : 0);
				SizeToContent = SizeToContent.Manual;
				MinHeight = h;
				MaxHeight = h;
				double v = (Width / 50) * 1.1;
				SpinFontSize = Math.Max(11.5, v);
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
				OnPropertyChanged(nameof(GridTileWidth));
				OnPropertyChanged(nameof(GridTileHeight));
				return;
			}

			var numColumns = StaticSize ? 2 : GetNumColumnsForRoulette();

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

			SpinFontSize = GetFontScale(width);
		}


		private double GetContentScale() {
			var numConds = GameController.Main.Roulette.Spin?.Conditions.Count ?? 0;
			if (VerticalDisplay) {
				return numConds switch {
					1 => 2.75,
					2 => 1.5,
					4 => 0.7,
					5 => 0.6,
					_ => 1,
				};
			}
			return numConds switch {
				1 or 3 or 4 => 3.0,
				2 => 1.5,
				5 => 2.0,
				_ => 1,
			};
		}

		private double GetFontScale(double width) {
			var numColumns = GetNumColumnsForRoulette();
			// Scale fonts (poorly)
			double w = width;
			var scale = numColumns > 1 ? 0.75 : 1.3;
			double v = (w / 35) * scale;
			return Math.Max(10, v);
		}

		private void Window_Closing(object? sender, CancelEventArgs e) {
			timer.Stop();
			StatisticsWindowInst?.Close();
			Config.Save(true);
		}

		private void CopySpinCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (GameController.Main.Roulette.Spin == null) return;

			var text = GameController.Main.Roulette.Spin.ToString();

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
			if (GameController.Main.Roulette.Spin == null) return;
			var text = GameController.Main.Roulette.Spin.ToString().Replace("'", "");
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
			if (SpinParser.TryParse(text, out var spin) && spin is not null) {
				var roulette = GameController.Main.Roulette;
				var curSpin = roulette.Spin;
				if (curSpin == null || curSpin.ToString() == spin.ToString())
					return;

				roulette.SetSpin(spin);
				PushCurrentSpinToHistory();

				spinHistoryIndex = 1;
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
			EditMapPoolWindowInst.Closed += (sender, e) => {
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
			EditRulesetWindowInst = new(Ruleset.Rulesets) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditRulesetWindowInst.Closed += (sender, e) => {
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
			EditHotkeysWindowInst.Closed += (sender, e) => {
				EditHotkeysWindowInst = null;
			};
			EditHotkeysWindowInst.Show();
		}

		private void ResetStreakCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			GameController.Main.Streak = 0;
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
			StatisticsWindowInst.Closed += (sender, e) => {
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
			TimerSettingsWindowInst.Closed += (sender, e) => {
				TimerSettingsWindowInst = null;
			};
			TimerSettingsWindowInst.ResetStreak += (sender, _) => GameController.Main.Streak = 0;
			TimerSettingsWindowInst.ResetStreakPB += (sender, _) => UpdateStreakStatus();
			TimerSettingsWindowInst.Show();
		}

		private void CheckDailySpinsCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			CheckDailySpinsAsync();
		}

		private void DailySpin1Command_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (dailySpin1 == null) return;
			if (SpinParser.TryParse(dailySpin1.spin, out var spin)) {
				GameController.Main.Roulette.SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				ResetTimer();
				TrackNewSpin();
			}
		}
		
		private void DailySpin2Command_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (dailySpin2 == null) return;
			if (SpinParser.TryParse(dailySpin2.spin, out var spin)) {
				GameController.Main.Roulette.SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				ResetTimer();
				TrackNewSpin();
			}
		}

		private void DailySpin3Command_Executed(object? sender, ExecutedRoutedEventArgs e) {
			if (dailySpin3 == null) return;
			if (SpinParser.TryParse(dailySpin3.spin, out var spin)) {
				GameController.Main.Roulette.SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
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
			EditSpinWindowInst = new(GameController.Main.Roulette) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditSpinWindowInst.Closed += (object? sender, EventArgs e) => {
				EditSpinWindowInst = null;
			};
			EditSpinWindowInst.Show();
		}

		private void PrevCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			Previous();
		}

		private void PrevCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = GameController.Main.IsPlayingRoulette
				&& spinHistory.Count > 1
				&& spinHistoryIndex < spinHistory.Count;
		}

		private void NextSpinCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = GameController.Main.IsPlayingRoulette && spinHistoryIndex > 1;
		}

		private void NextSpinCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			Next();
		}

		private void PrevMapCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void NextMapCommand_CanExecute(object? sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void NextMapCommand_Executed(object? sender, ExecutedRoutedEventArgs e) {
			var spin = GameController.Main.Roulette.Spin;
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
			var spin = GameController.Main.Roulette.Spin;
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
			GameController.Main.Shuffle();
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
		}

		private void EditSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = IsRouletteMode;
		}
	}
}
