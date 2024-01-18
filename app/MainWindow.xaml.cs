﻿using Croupier.UI.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using RestoreWindowPlace;
using System.Reflection;
using System.Data.Common;
using static System.Formats.Asn1.AsnWriter;

namespace Croupier.UI
{
	public class MissionComboBoxItem
	{
		public MissionID ID { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public Uri Image { get; set; }
		public bool IsSeparator { get; set; }
	}

	public class SpinHistoryEntry(string name, int index) : INotifyPropertyChanged
	{
		private string _name = name;
		private bool isSelected = false;
		private int _index = index;

		public string Name {
			get { return _name; }
			set {
				_name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public int Index {
			get { return _index; }
			set {
				_index = value;
				OnPropertyChanged(nameof(Index));
			}
		}
		public bool IsSelected {
			get { return isSelected; }
			set {
				isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged {

		private const int MAX_HISTORY_ENTRIES = 30;
		public event PropertyChangedEventHandler PropertyChanged;

		public static readonly RoutedUICommand CopySpinCommand = new("Copy Spin", "CopySpin", typeof(MainWindow), [
			new KeyGesture(Key.C, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand PasteSpinCommand = new("Paste Spin", "PasteSpin", typeof(MainWindow), [
			new KeyGesture(Key.V, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand EditMapPoolCommand = new("Edit Map Pool", "EditMapPool", typeof(MainWindow), [
			new KeyGesture(Key.M, ModifierKeys.Alt),
		]);
		public static readonly RoutedUICommand EditRulesetsCommand = new("Edit Rulesets", "EditRulesets", typeof(MainWindow), [
			new KeyGesture(Key.R, ModifierKeys.Alt),
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
		public static readonly RoutedUICommand ShuffleCommand = new("Shuffle", "Shuffle", typeof(MainWindow), [
			new KeyGesture(Key.Space, ModifierKeys.Control),
		]);
		public static readonly RoutedUICommand RespinCommand = new("Respin", "Respin", typeof(MainWindow), [
			new KeyGesture(Key.R, ModifierKeys.Control),
		]);

		private static readonly Random random = new();
		private static readonly List<Spin> spinHistory = [];
		private static int spinHistoryIndex = 1;

		private EditMapPoolWindow EditMapPoolWindowInst;
		private EditRulesetWindow EditRulesetWindowInst;
		private EditSpinWindow EditSpinWindowInst;
		private double _spinFontSize = 16;
		private bool _rightToLeft = false;
		private bool _topmostEnabled = false;
		private bool _verticalDisplay = false;
		private bool _spinLock = false;
		private bool _editMode = false;
		private bool _staticSize = false;
		private bool _staticSizeLHS = false;

		public double SpinFontSize {
			get { return _spinFontSize; }
			private set {
				if (_spinFontSize != value) {
					_spinFontSize = value;
					OnPropertyChanged(nameof(SpinFontSize));
				}
			}
		}

		public bool ShuffleButtonEnabled {
			get {
				return missionPool.Count > 0;
			}
		}

		public bool TopmostEnabled {
			get {
				return _topmostEnabled;
			}
			set {
				_topmostEnabled = value;
				Topmost = value;
				Settings.Default.AlwaysOnTop = value;
				Settings.Default.Save();
				OnPropertyChanged(nameof(TopmostEnabled));
			}
		}
		public bool VerticalDisplay {
			get { return _verticalDisplay; }
			set {
				_verticalDisplay = value;
				Settings.Default.VerticalDisplay = value;
				Settings.Default.Save();
				OnPropertyChanged(nameof(VerticalDisplay));
			}
		}
		public bool SpinLock {
			get { return _spinLock; }
			set {
				if (value == _spinLock) return;
				_spinLock = value;
				OnPropertyChanged(nameof(SpinLock));
				SendSpinLockToClient();
			}
		}
		public bool HorizontalSpinDisplay { get; set; } = false;
		public bool RightToLeft {
			get { return _rightToLeft; }
			set {
				if (value == _rightToLeft) return;
				_rightToLeft = value;
				if (value != Settings.Default.RightToLeft) {
					Settings.Default.RightToLeft = value;
					Settings.Default.Save();
				}
				OnPropertyChanged(nameof(RightToLeft));
				OnPropertyChanged(nameof(RightToLeftFlowDir));
			}
		}
		public bool StaticSize {
			get { return _staticSize; }
			set {
				if (value == _staticSize) return;
				_staticSize = value;
				if (value != Settings.Default.StaticSize) {
					Settings.Default.StaticSize = value;
					Settings.Default.Save();
				}
				OnPropertyChanged(nameof(ContentGridAlign));
				OnPropertyChanged(nameof(StaticSize));
				OnPropertyChanged(nameof(SpinAlignHorz));
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
			}
		}
		public bool StaticSizeLHS {
			get { return _staticSizeLHS; }
			set {
				if (value == _staticSizeLHS) return;
				_staticSizeLHS = value;
				if (value != Settings.Default.StaticSizeLHS) {
					Settings.Default.StaticSizeLHS = value;
					Settings.Default.Save();
				}
				OnPropertyChanged(nameof(ContentGridAlign));
				OnPropertyChanged(nameof(StaticSize));
				OnPropertyChanged(nameof(SpinAlignHorz));
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
			}
		}
		public bool ShowSpinLabels {
			get { return !_editMode; }
		}
		public bool EditMode {
			get { return _editMode; }
			set {
				if (value == _editMode) return;
				_editMode = value;
				OnPropertyChanged(nameof(EditMode));
				OnPropertyChanged(nameof(ShowSpinLabels));
			}
		}
		public FlowDirection RightToLeftFlowDir {
			get {
				return RightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
			}
		}
		public HorizontalAlignment ContentGridAlign {
			get {
				if (!StaticSize)
					return HorizontalAlignment.Stretch;
				return StaticSizeLHS ? HorizontalAlignment.Left : HorizontalAlignment.Right;
			}
		}
		public HorizontalAlignment SpinAlignHorz {
			get {
				if (!StaticSize) return HorizontalAlignment.Stretch;
				return StaticSizeLHS ? HorizontalAlignment.Left : HorizontalAlignment.Right;
			}
		}
		public double SpinGridWidth {
			get {
				if (!StaticSize) return double.NaN;
				var w = (VerticalDisplay ? 300.0 : 150.0) * (Width / 300);
				return w;
			}
		}
		public double SpinGridHeight {
			get {
				if (!StaticSize)
					return double.NaN;
				return SpinGridWidth * 0.36;
			}
		}

		public ObservableCollection<SpinHistoryEntry> HistoryEntries = [];

		private readonly List<SpinCondition> conditions = [];
		private readonly List<Mission> missions = [
			new Mission(MissionID.ICAFACILITY_FREEFORM),
			new Mission(MissionID.ICAFACILITY_FINALTEST),
			new Mission(MissionID.PARIS_SHOWSTOPPER),
			new Mission(MissionID.SAPIENZA_WORLDOFTOMORROW),
			new Mission(MissionID.MARRAKESH_GILDEDCAGE),
			new Mission(MissionID.BANGKOK_CLUB27),
			new Mission(MissionID.COLORADO_FREEDOMFIGHTERS),
			new Mission(MissionID.HOKKAIDO_SITUSINVERSUS),
			new Mission(MissionID.BANGKOK_THESOURCE),
			new Mission(MissionID.SAPIENZA_THEAUTHOR),
			new Mission(MissionID.HOKKAIDO_PATIENTZERO),
			new Mission(MissionID.PARIS_HOLIDAYHOARDERS),
			new Mission(MissionID.SAPIENZA_THEICON),
			new Mission(MissionID.SAPIENZA_LANDSLIDE),
			new Mission(MissionID.MARRAKESH_HOUSEBUILTONSAND),
			new Mission(MissionID.HOKKAIDO_SNOWFESTIVAL),
			new Mission(MissionID.HAWKESBAY_NIGHTCALL),
			new Mission(MissionID.MIAMI_FINISHLINE),
			new Mission(MissionID.SANTAFORTUNA_THREEHEADEDSERPENT),
			new Mission(MissionID.MUMBAI_CHASINGAGHOST),
			new Mission(MissionID.WHITTLETON_ANOTHERLIFE),
			new Mission(MissionID.ISLEOFSGAIL_THEARKSOCIETY),
			new Mission(MissionID.NEWYORK_GOLDENHANDSHAKE),
			new Mission(MissionID.HAVEN_THELASTRESORT),
			new Mission(MissionID.MIAMI_ASILVERTONGUE),
			new Mission(MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT),
			new Mission(MissionID.MUMBAI_ILLUSIONSOFGRANDEUR),
			new Mission(MissionID.WHITTLETON_ABITTERPILL),
			new Mission(MissionID.DUBAI_ONTOPOFTHEWORLD),
			new Mission(MissionID.DARTMOOR_DEATHINTHEFAMILY),
			new Mission(MissionID.BERLIN_APEXPREDATOR),
			new Mission(MissionID.CHONGQING_ENDOFANERA),
			new Mission(MissionID.MENDOZA_THEFAREWELL),
			new Mission(MissionID.CARPATHIAN_UNTOUCHABLE),
			new Mission(MissionID.AMBROSE_SHADOWSINTHEWATER),
		];
		private Ruleset rules = new("current", RulesetPreset.RRWC2023);
		private readonly ObservableCollection<Ruleset> rulesets = [
			new("RRWC 2023", RulesetPreset.RRWC2023),
			new("RR12", RulesetPreset.RR12),
			new("RR11", RulesetPreset.RR11),
			new("Croupier", RulesetPreset.Croupier),
			new("Custom", RulesetPreset.Custom),
		];

		private readonly List<MissionID> missionPool = [];
		private Mission currentMission = null;
		private bool disableClientUpdate = false;
		
		private ObservableCollection<MissionComboBoxItem> MissionListItems {
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
						Name = mission.Name,
						Image = mission.ImagePath,
						Location = mission.Location,
						IsSeparator = false
					});
				});
				return items;
			}
		}

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
			MainContextMenu.DataContext = this;
			SizeToContent = SizeToContent.Height;
			((App)Application.Current).WindowPlace.Register(this);
			Focus();

			LoadSettings();

			PropertyChanged += MainWindow_PropertyChanged;
			CroupierSocketServer.Connected += (object sender, int _) => {
				SendMissionsToClient();
				SendSpinToClient();
				SendSpinLockToClient();
			};
			CroupierSocketServer.Respin += (object sender, MissionID id) => Spin(id);
			CroupierSocketServer.AutoSpin += (object sender, MissionID id) => { if (!SpinLock) Spin(id); };
			CroupierSocketServer.Random += (object sender, int _) => Shuffle();
			CroupierSocketServer.ToggleSpinLock += (object sender, int _) => {
				SpinLock = !SpinLock;
			};
			CroupierSocketServer.Missions += (object sender, List<MissionID> missions) => {
				missionPool.Clear();
				missionPool.AddRange(missions);
				EditMapPoolWindowInst?.UpdateMissionPool(missionPool);
			};
			CroupierSocketServer.Prev += (object sender, int _) => {
				disableClientUpdate = true;
				PreviousSpin();
				disableClientUpdate = false;
			};
			CroupierSocketServer.Next += (object sender, int _) => {
				disableClientUpdate = true;
				NextSpin();
				disableClientUpdate = false;
			};
			CroupierSocketServer.SpinData += (object sender, string data) => {
				if (UI.Spin.Parse(data, out var spin)) {
					disableClientUpdate = true;
					SetSpin(spin);
					UpdateSpinHistory();
					PostConditionUpdate();
					disableClientUpdate = false;
				}
			};

			SendMissionsToClient();
			SendSpinToClient();
		}

		private void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(VerticalDisplay)
				|| e.PropertyName == nameof(StaticSize)) {
				RefitWindow();
				return;
			}
		}

		private void LoadSettings()
		{
			if (!Enum.IsDefined(typeof(MissionPoolPresetID), Settings.Default.MissionPool))
				Settings.Default.MissionPool = (int)MissionPoolPresetID.MainMissions;
			
			VerticalDisplay = Settings.Default.VerticalDisplay;
			TopmostEnabled = Settings.Default.AlwaysOnTop;
			RightToLeft = Settings.Default.RightToLeft;
			StaticSize = Settings.Default.StaticSize;
			StaticSizeLHS = Settings.Default.StaticSizeLHS;
			LoadMissionPool();
		}

		private void EditRulesetWindow_Closed(object sender, EventArgs e)
		{
			EditRulesetWindowInst = null;
		}

		private void EditRulesetWindow_ApplyRuleset(object sender, Ruleset ruleset)
		{
			rules = ruleset;
		}

		private void EditMapPoolWindow_Closed(object sender, EventArgs e)
		{
			EditMapPoolWindowInst = null;
		}

		private void EditSpinWindow_Closed(object sender, EventArgs e)
		{
			EditSpinWindowInst = null;
		}

		private void EditSpinWindow_SetCondition(object _sender, SpinCondition condition)
		{
			if (currentMission.ID != condition.Target.Mission) return;
			for (var i = 0; i < conditions.Count; i++) {
				if (conditions[i].Target != condition.Target) continue;
				conditions[i] = condition;
				break;
			}

			UpdateSpinHistory();
			PostConditionUpdate();
		}

		private void EditMapPoolWindow_AddMissionToPool(object sender, MissionID e)
		{
			if (missionPool.Contains(e)) return;
			missionPool.Add(e);
			SendMissionsToClient();
			OnPropertyChanged(nameof(ShuffleButtonEnabled));
			SaveCustomMissionPool();
		}

		private void EditMapPoolWindow_RemoveMissionFromPool(object sender, MissionID e)
		{
			missionPool.Remove(e);
			SendMissionsToClient();
			OnPropertyChanged(nameof(ShuffleButtonEnabled));
			SaveCustomMissionPool();
		}

		private void SaveCustomMissionPool()
		{
			if ((MissionPoolPresetID)Settings.Default.MissionPool != MissionPoolPresetID.Custom)
				return;
			Settings.Default.CustomMissionPool ??= [];
			Settings.Default.CustomMissionPool.Clear();
			foreach (var mission in missionPool) {
				if (!Mission.GetMissionCodename(mission, out var name))
					continue;
				Settings.Default.CustomMissionPool.Add(name);
			}
		}

		private void LoadMissionPool()
		{
			missionPool.Clear();
			missionPool.AddRange(((MissionPoolPresetID)Settings.Default.MissionPool).GetMissions());
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			ThemeManager.SetCurrentTheme(this, new Uri("/Croupier.UI;component/Resources/DarkTheme.xaml", UriKind.Relative));

			MissionSelect.ItemsSource = MissionListItems;
			ContextMenuHistory.ItemsSource = HistoryEntries;
			ContextMenuHistory.DataContext = this;

			if (Settings.Default.SpinHistory != null && Settings.Default.SpinHistory.Count > 0) {
				string[] history = new string[Settings.Default.SpinHistory.Count];
				Settings.Default.SpinHistory.CopyTo(history, 0);

				foreach (var item in history.Reverse()) {
					if (UI.Spin.Parse(item, out var spin)) PushSpinToHistory(spin);
				}
			}

			if (spinHistory.Count > 0)
				SetSpinHistory(1);
			else {
				SetMission(missions[0].ID);
				Spin(currentMission.ID);
			}

			var idx = MissionListItems.ToList().FindIndex(item => item.ID == currentMission.ID);
			MissionSelect.SelectedIndex = idx;
		}

		public void Spin(MissionID id = MissionID.NONE) {
			if (id == MissionID.NONE) id = currentMission.ID;
			if (!SetMission(id)) return;

			Generator gen = new(rules, currentMission);
			var spin = gen.GenerateSpin();
			SetSpin(spin);
			spinHistoryIndex = 1;
			PushCurrentSpinToHistory();
			PostConditionUpdate();
		}

		public bool SetMission(MissionID id) {
			currentMission = missions.Find(m => m.ID == id);
			if (currentMission == null) return false;
			var idx = MissionListItems.ToList().FindIndex(item => item.ID == id);
			MissionSelect.SelectedIndex = idx;
			return idx != -1;
		}

		public bool PushSpinToHistory(Spin spin) {
			if (spin == null || spin.Conditions.Count == 0) return false;
			if (spin.Conditions == conditions) return false;
			if (spinHistory.Count > 0 && spinHistory[^1].ToString() == spin.ToString())
				return true;

			spinHistory.Add(spin);
			if (HistoryEntries.Count < MAX_HISTORY_ENTRIES)
				HistoryEntries.Insert(0, new(spin.ToString(), spinHistory.Count - 1));
			while (spinHistory.Count > MAX_HISTORY_ENTRIES)
				spinHistory.RemoveAt(0);
			SyncHistoryEntries();
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
		}

		public void NextSpin() {
			if (spinHistoryIndex > spinHistory.Count)
				spinHistoryIndex = spinHistory.Count;
			if (spinHistoryIndex <= 1) {
				spinHistoryIndex = 1;
				return;
			}

			SetSpinHistory(spinHistoryIndex - 1);
		}

		public void SetSpinHistory(int idx) {
			spinHistoryIndex = idx;
			SetSpin(spinHistory[^spinHistoryIndex]);
			SyncHistoryEntries();
			SendSpinToClient();
		}

		public void SyncHistoryEntries() {
			var fwdIndex = spinHistory.Count - spinHistoryIndex;

			if (Settings.Default.SpinHistory == null)
				Settings.Default.SpinHistory = [];

			Settings.Default.SpinHistory.Clear();

			for (var i = 0; i < spinHistory.Count && i < HistoryEntries.Count; ++i) {
				var absIdx = spinHistory.Count - i - 1;
				var entry = HistoryEntries[i];
				entry.Name = spinHistory[absIdx].ToString();
				entry.Index = absIdx;
				entry.IsSelected = absIdx == fwdIndex;
				Settings.Default.SpinHistory.Add(entry.Name);
			}

			Settings.Default.Save();
			OnPropertyChanged(nameof(HistoryEntries));
		}

		public void SetSpin(Spin spin) {
			if (spin == null) return;
			conditions.Clear();
			conditions.AddRange(spin.Conditions);
			SetMission(spin.Mission);
			EditSpinWindowInst?.UpdateConditions(conditions);
			PostConditionUpdate();
		}

		private void PostConditionUpdate()
		{
			SendSpinToClient();
			SetupSpinUI();
			RefitWindow();
		}

		public void SendSpinLockToClient()
		{
			if (disableClientUpdate) return;
			CroupierSocketServer.Send("SpinLock:" + (SpinLock ? "1" : "0"));
		} 

		public void SendSpinToClient() {
			if (disableClientUpdate) return;
			var spinData = "";
			conditions.ForEach(condition => {
				if (spinData.Length > 0) spinData += ", ";
				spinData += $"{condition}";
			});
			CroupierSocketServer.Send("SpinData:" + spinData);
		}

		public void SendMissionsToClient()
		{
			if (disableClientUpdate) return;
			var missions = "";
			missionPool.ForEach(mission => {
				if (!Mission.GetMissionCodename(mission, out var name))
					return;
				missions += missions.Length > 0 ? $",{name}" : name;
			});
			CroupierSocketServer.Send("Missions:" + missions);
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void MissionSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0) return;
			var item = (MissionComboBoxItem)e.AddedItems[0];
			if (currentMission == null || currentMission.ID != item.ID)
				Spin(item.ID);
		}

		private void Shuffle()
		{
			if (missionPool.Count == 0) return;
			Spin(missionPool[random.Next(missionPool.Count)]);
		}

		private ICommand _historyEntrySelectCommand;

		public ICommand HistoryEntrySelectCommand {
			get {
				return _historyEntrySelectCommand ??= new RelayCommand(param => this.HistoryEntrySelected(param));
			}
		}

		private void HistoryEntrySelected(object param)
		{
			var index = param as int?;
			if (index == null || index < 0 || index >= spinHistory.Count) return;
			SetSpinHistory(Math.Abs(index.Value - spinHistory.Count));
		}

		private void ContextMenu_Exit(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			Window window = (Window)sender;
			window.Topmost = TopmostEnabled;
		}

		private void OnSizeChange(object sender, SizeChangedEventArgs e)
		{
			if (e.WidthChanged) {
				var numColumns = GetNumColumns();
				if (!StaticSize) {
					if (numColumns == 2) {
						Settings.Default.Width2Column = e.NewSize.Width;
						Settings.Default.Save();
					}
					else if (numColumns == 1) {
						Settings.Default.Width1Column = e.NewSize.Width;
						Settings.Default.Save();
					}
				}
				
				RefitWindow(true);
			}
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
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

		private void SetupSpinUI() {
			var numColumns = GetNumColumns();
			ContentGrid.Columns = numColumns;
			ContentGrid.Children.Clear();

			foreach (var condition in conditions) {
				var control = new ContentPresenter {
					Content = condition,
					DataContext = condition,
					ContentTemplate = (DataTemplate)Resources["SpinConditionDataTemplate"],
				};
				ContentGrid.Children.Add(control);
			}
		}

		private void RefitWindow(bool keepSize = false) {
			if (StaticSize) {
				SizeToContent = SizeToContent.Manual;
				MinHeight = SpinGridHeight * GetNumRows() + 40;
				MaxHeight = SpinGridHeight * GetNumRows() + 40;
				double v = (Width / 50) * 1.1;
				SpinFontSize = Math.Max(11.5, v);
				OnPropertyChanged(nameof(SpinGridWidth));
				OnPropertyChanged(nameof(SpinGridHeight));
				return;
			}

			var numColumns = GetNumColumns();

			var width = Math.Max(numColumns switch {
				1 => Settings.Default.Width1Column,
				2 => Settings.Default.Width2Column,
				_ => Width,
			}, 450);

			if (!keepSize) {
				this.Width = width;
			}

			// Scale content (poorly)
			var scale = GetContentScale();
			//double rowScaling = Math.Ceiling((double)numItems / numColumns);
			double contentHeight = width / scale;
			contentHeight += HeaderGrid.ActualHeight;
			MinHeight = contentHeight;
			MaxHeight = contentHeight;
			SizeToContent = SizeToContent.Manual;

			ScaleFonts(width);
		}

		private double GetContentScale()
		{
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

		private void ScaleFonts(double width)
		{
			var numColumns = GetNumColumns();
			// Scale fonts (poorly)
			double w = width;
			var scale = numColumns > 1 ? 0.75 : 1.3;
			double v = (w / 35) * scale;
			SpinFontSize = Math.Max(10, v);
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			Settings.Default.Save();
		}

		private void CopySpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Clipboard.SetText(new Spin(conditions).ToString());
		}

		private void PasteSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var text = Clipboard.GetText();
			if (UI.Spin.Parse(text, out var spin)) {
				SetSpin(spin);
				spinHistoryIndex = 1;
				PushCurrentSpinToHistory();
				PostConditionUpdate();
			}
		}

		private void EditMapPoolCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
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
			EditMapPoolWindowInst.Closed += EditMapPoolWindow_Closed;
			EditMapPoolWindowInst.UpdateMissionPool(missionPool);
			EditMapPoolWindowInst.Show();
		}

		private void EditRulesetsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (EditRulesetWindowInst != null) {
				EditRulesetWindowInst.Activate();
				return;
			}
			EditRulesetWindowInst = new(rulesets) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditRulesetWindowInst.ApplyRuleset += EditRulesetWindow_ApplyRuleset;
			EditRulesetWindowInst.Closed += EditRulesetWindow_Closed;
			EditRulesetWindowInst.Show();
		}

		private void EditSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (EditSpinWindowInst != null) {
				EditSpinWindowInst.Activate();
				return;
			}
			EditSpinWindowInst = new(conditions) {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			EditSpinWindowInst.SetCondition += EditSpinWindow_SetCondition;
			EditSpinWindowInst.Closed += EditSpinWindow_Closed;
			EditSpinWindowInst.Show();
		}

		private void PrevSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			PreviousSpin();
		}

		private void PrevSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = spinHistory.Count > 1 && spinHistoryIndex < spinHistory.Count;
		}

		private void NextSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = spinHistoryIndex > 1;
		}

		private void NextSpinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			NextSpin();
		}

		private void RespinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Spin();
		}

		private void ShuffleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Shuffle();
		}

		private void ShuffleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ShuffleButtonEnabled;
		}

		private void PasteSpinCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = UI.Spin.Parse(Clipboard.GetText(), out _);
		}

		private void Command_AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
	}
}