using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Croupier
{
	public enum MissionPoolPresetID {
		Custom,
		MainMissions,
		BonusMissions,
		AdditionalMissions,
		AllMissions,
	}

	public static class MissionPoolPresetIDMethods {
		private static readonly MissionID[] mainMissions = [
			MissionID.PARIS_SHOWSTOPPER, MissionID.SAPIENZA_WORLDOFTOMORROW, MissionID.MARRAKESH_GILDEDCAGE,
			MissionID.BANGKOK_CLUB27, MissionID.COLORADO_FREEDOMFIGHTERS, MissionID.HOKKAIDO_SITUSINVERSUS,
			MissionID.MIAMI_FINISHLINE, MissionID.SANTAFORTUNA_THREEHEADEDSERPENT, MissionID.MUMBAI_CHASINGAGHOST,
			MissionID.WHITTLETON_ANOTHERLIFE, MissionID.ISLEOFSGAIL_THEARKSOCIETY, MissionID.NEWYORK_GOLDENHANDSHAKE,
			MissionID.HAVEN_THELASTRESORT, MissionID.DUBAI_ONTOPOFTHEWORLD, MissionID.DARTMOOR_DEATHINTHEFAMILY,
			MissionID.CHONGQING_ENDOFANERA, MissionID.BERLIN_APEXPREDATOR, MissionID.MENDOZA_THEFAREWELL,
			MissionID.AMBROSE_SHADOWSINTHEWATER,
		];
		private static readonly MissionID[] additionalMissions = [
			MissionID.ICAFACILITY_FREEFORM,
			MissionID.ICAFACILITY_FINALTEST,
			MissionID.HAWKESBAY_NIGHTCALL,
			MissionID.CARPATHIAN_UNTOUCHABLE,
		];
		private static readonly MissionID[] bonusMissions = [
			MissionID.BANGKOK_THESOURCE, MissionID.SAPIENZA_THEAUTHOR, MissionID.HOKKAIDO_PATIENTZERO,
			MissionID.PARIS_HOLIDAYHOARDERS, MissionID.SAPIENZA_THEICON, MissionID.SAPIENZA_LANDSLIDE,
			MissionID.MARRAKESH_HOUSEBUILTONSAND, MissionID.HOKKAIDO_SNOWFESTIVAL, MissionID.MIAMI_ASILVERTONGUE,
			MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT, MissionID.MUMBAI_ILLUSIONSOFGRANDEUR, MissionID.WHITTLETON_ABITTERPILL,
		];

		public static List<MissionID> GetMissions(this MissionPoolPresetID presetID)
		{
			var list = new List<MissionID>();
			switch (presetID)
			{
				case MissionPoolPresetID.AllMissions:
					list.AddRange(mainMissions);
					list.AddRange(additionalMissions);
					list.AddRange(bonusMissions);
					break;
				case MissionPoolPresetID.MainMissions:
					list.AddRange(mainMissions);
					break;
				case MissionPoolPresetID.BonusMissions:
					list.AddRange(bonusMissions);
					break;
				case MissionPoolPresetID.AdditionalMissions:
					list.AddRange(additionalMissions);
					break;
				case MissionPoolPresetID.Custom:
					if (Config.Default.CustomMissionPool == null) break;
					
					var toRemove = new List<string>();
					
					foreach (var key in Config.Default.CustomMissionPool) {
						var id = MissionIDMethods.FromKey(key);
						if (id != MissionID.NONE) {
							toRemove.Add(key);
							continue;
						}

						if (!list.Contains(id)) list.Add(id);
					}

					if (toRemove.Count > 0) {
						toRemove.ForEach(key => Config.Default.CustomMissionPool.Remove(key));
						Config.Save();
					}
					break;
			}
			return list;
		}
	}

	public class MissionPoolEntry(MissionID missionID, string name) : INotifyPropertyChanged
	{
		private bool _IsInPool = false;

		public MissionID MissionID { get; set; } = missionID;
		public string Name { get; set; } = name;
		
		public bool IsInPool {
			get => _IsInPool;
			set {
				_IsInPool = value;
				OnPropertyChanged(nameof(IsInPool));
			}
		}

		public int Column { get; set; } = 0;
		public int Row { get; set; } = 0;

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class MissionPoolPresetEntry : INotifyPropertyChanged
	{
		public MissionPoolPresetID Preset { get; set; }

		private string? _Name;

		public string Name {
			get => _Name ?? "";
			set {
				_Name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public List<MissionID> GetMissions() {
			return Preset.GetMissions();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class MissionPoolGroup : INotifyPropertyChanged
	{
		public MissionPoolGroup(string name, ObservableCollection<MissionPoolEntry> entries) {
			Name = name;
			Entries = entries;
			foreach (var entry in Entries)
				entry.PropertyChanged += EntryPopertyChanged;
		}

		private void EntryPopertyChanged(object? sender, PropertyChangedEventArgs e) {
			OnPropertyChanged(nameof(PoolState));
		}

		public string Name { get; set; }
		public bool? PoolState {
			get {
				var count = Entries.Count(entry => entry.IsInPool);
				if (count == 0) return false;
				if (count == Entries.Count) return true;
				return null;
			}
			set {
				if (value == null) value = false;
				foreach (var entry in Entries) {
					entry.IsInPool = value.Value;
				}
				OnPropertyChanged(nameof(Entries));
			}
		}

		private ObservableCollection<MissionPoolEntry> _Entries = [];
		public ObservableCollection<MissionPoolEntry> Entries {
			get => _Entries;
			set {
				_Entries = value;
				OnPropertyChanged(nameof(Entries));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public partial class EditMapPoolWindow : Window, INotifyPropertyChanged
	{
		public event EventHandler<MissionID> AddMissionToPool;
		public event EventHandler<MissionID> RemoveMissionFromPool;

		private readonly ObservableCollection<MissionPoolGroup> _MissionPoolList = [
			new("Prologue", [
				new(MissionID.ICAFACILITY_FREEFORM, "Freeform Training"),
				new(MissionID.ICAFACILITY_FINALTEST, "The Final Test") { Column = 1 },
			]),
			new("Season 1", [
				new(MissionID.PARIS_SHOWSTOPPER, "Paris: The Showstopper"),
				new(MissionID.SAPIENZA_WORLDOFTOMORROW, "Sapienza: World of Tomorrow") { Column = 1 },
				new(MissionID.MARRAKESH_GILDEDCAGE, "Marrakesh: A Gilded Cage") { Column = 2 },
				new(MissionID.BANGKOK_CLUB27, "Bangkok: Club 27") { Row = 1 },
				new(MissionID.COLORADO_FREEDOMFIGHTERS, "Colorado: Freedom Fighters") { Row = 1, Column = 1 },
				new(MissionID.HOKKAIDO_SITUSINVERSUS, "Hokkaido: Situs Inversus") { Row = 1, Column = 2 },
			]),
			new("Season 1 Bonus", [
				new(MissionID.BANGKOK_THESOURCE, "Bangkok: The Source"),
				new(MissionID.SAPIENZA_THEAUTHOR, "Sapienza: The Author") { Column = 1 },
				new(MissionID.HOKKAIDO_PATIENTZERO, "Hokkaido: Patient Zero") { Column = 2 },
				new(MissionID.PARIS_HOLIDAYHOARDERS, "Paris: Holiday Hoarders") { Row = 1 },
				new(MissionID.SAPIENZA_THEICON, "Sapienza: The Icon") { Row = 1, Column = 1 },
				new(MissionID.SAPIENZA_LANDSLIDE, "Sapienza: Landslide") { Row = 1, Column = 2 },
				new(MissionID.MARRAKESH_HOUSEBUILTONSAND, "Marrakesh: A House Built On Sand") { Row = 2 },
				new(MissionID.HOKKAIDO_SNOWFESTIVAL, "Hokkaido: Snow Festival") { Row = 2, Column = 1 },
			]),
			new("Season 2", [
				new(MissionID.HAWKESBAY_NIGHTCALL, "Hawke's Bay: Nightcall"),
				new(MissionID.MIAMI_FINISHLINE, "Miami: The Finish Line") { Column = 1 },
				new(MissionID.SANTAFORTUNA_THREEHEADEDSERPENT, "Santa Fortuna: Three-Headed Serpent") { Column = 2 },
				new(MissionID.MUMBAI_CHASINGAGHOST, "Mumbai: Chasing a Ghost") { Row = 1 },
				new(MissionID.WHITTLETON_ANOTHERLIFE, "Whittleton Creek: Another Life") { Row = 1, Column = 1 },
				new(MissionID.ISLEOFSGAIL_THEARKSOCIETY, "Isle of Sgàil: The Ark Society") { Row = 1, Column = 2 },
				new(MissionID.NEWYORK_GOLDENHANDSHAKE, "New York: Golden Handshake") { Row = 2 },
				new(MissionID.HAVEN_THELASTRESORT, "Haven: The Last Resort") { Row = 2, Column = 1 },
			]),
			new("Special Assignments", [
				new(MissionID.MIAMI_ASILVERTONGUE, "Miami: A Silver Tongue"),
				new(MissionID.SANTAFORTUNA_EMBRACEOFTHESERPENT, "Santa Fortuna: Embrace of the Serpent") { Column = 1 },
				new(MissionID.MUMBAI_ILLUSIONSOFGRANDEUR, "Mumbai: Illusions of Grandeur") { Column = 2 },
				new(MissionID.WHITTLETON_ABITTERPILL, "Whittleton Creek: A Bitter Pill") { Row = 1 },
			]),
			new("Season 3", [
				new(MissionID.DUBAI_ONTOPOFTHEWORLD, "Dubai: On Top of the World"),
				new(MissionID.DARTMOOR_DEATHINTHEFAMILY, "Dartmoor: Death in the Family") { Column = 1 },
				new(MissionID.BERLIN_APEXPREDATOR, "Berlin: Apex Predator") { Column = 2 },
				new(MissionID.CHONGQING_ENDOFANERA, "Chongqing: End of an Era") { Row = 1 },
				new(MissionID.MENDOZA_THEFAREWELL, "Mendoza: The Farewell") { Row = 1, Column = 1 },
				new(MissionID.CARPATHIAN_UNTOUCHABLE, "Carpathian Mountains: Untouchable") { Row = 1, Column = 2 },
				new(MissionID.AMBROSE_SHADOWSINTHEWATER, "Ambrose Island: Shadows in the Water") { Row = 2 },
			]),
		];

		public ObservableCollection<MissionPoolGroup> MissionPoolList {
			get {
				return _MissionPoolList;
			}
		}

		private static readonly MissionPoolPresetEntry customMissionPoolPresetEntry = new() { Name = $"Custom ({Config.Default.CustomMissionPool?.Count ?? 0})", Preset = MissionPoolPresetID.Custom };
		public ObservableCollection<MissionPoolPresetEntry> MissionPoolPresetEntries = [
			customMissionPoolPresetEntry,
			new(){Name = "Main Missions (19)", Preset = MissionPoolPresetID.MainMissions},
			new(){Name = "Additional Missions (4)", Preset = MissionPoolPresetID.AdditionalMissions},
			new(){Name = "Bonus Missions (12)", Preset = MissionPoolPresetID.BonusMissions},
			new(){Name = "All Missions (35)", Preset = MissionPoolPresetID.AllMissions},
		];

		public MissionPoolPresetID MissionPoolPreset { get; set; }

		private bool _IsCustomPoolSelected = false;
		public bool IsCustomPoolSelected {
			get { return _IsCustomPoolSelected; }
			set { _IsCustomPoolSelected = value; OnPropertyChanged(nameof(IsCustomPoolSelected)); }
		}

		public EditMapPoolWindow()
		{
			InitializeComponent();
			MapPool.ItemsSource = MissionPoolList;
			MissionPoolPresetComboBox.ItemsSource = MissionPoolPresetEntries;

			if (!Enum.IsDefined(typeof(MissionPoolPresetID), Config.Default.MissionPool))
				Config.Default.MissionPool = MissionPoolPresetID.MainMissions;

			MissionPoolPresetComboBox.SelectedValue = Config.Default.MissionPool;

			if (Config.Default.CustomMissionPool == null)
				Config.Default.CustomMissionPool = [];
			else {
				var unique = Config.Default.CustomMissionPool.OfType<string>().Distinct().Where(v => MissionIDMethods.FromKey(v) != MissionID.NONE).ToArray();
				Config.Default.CustomMissionPool.Clear();
				Config.Default.CustomMissionPool.AddRange(unique);
			}

			AddMissionToPool += (object? sender, MissionID e) => {
				if (!IsCustomPoolSelected) return;
				var key = e.GetKey();
				if (!Config.Default.CustomMissionPool.Contains(key))
					Config.Default.CustomMissionPool.Add(key);
				Config.Save();

				customMissionPoolPresetEntry.Name = "Custom (" + Config.Default.CustomMissionPool.Count.ToString() + ")";
			};
			RemoveMissionFromPool += (object? sender, MissionID e) => {
				if (!IsCustomPoolSelected) return;
				Config.Default.CustomMissionPool.Remove(e.GetKey());
				Config.Save();
				customMissionPoolPresetEntry.Name = "Custom (" + Config.Default.CustomMissionPool.Count.ToString() + ")";
			};
		}

		public void UpdateMissionPool(List<MissionID> pool) {
			foreach (var item in MissionPoolList) {
				foreach (var entry in item.Entries)
					entry.IsInPool = pool.Contains(entry.MissionID);
			}
		}

		private void GroupCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			if (checkbox.DataContext is not MissionPoolGroup) return;
			var group = (MissionPoolGroup)checkbox.DataContext;
			foreach (var entry in group.Entries)
				AddMissionToPool?.Invoke(this, entry.MissionID);
		}

		private void GroupCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			if (checkbox.DataContext is not MissionPoolGroup) return;
			var group = (MissionPoolGroup)checkbox.DataContext;
			foreach (var entry in group.Entries)
				RemoveMissionFromPool?.Invoke(this, entry.MissionID);
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			if (checkbox.DataContext is not MissionPoolEntry) return;
			var entry = (MissionPoolEntry)checkbox.DataContext;
			entry.IsInPool = true;
			AddMissionToPool?.Invoke(this, entry.MissionID);
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			if (checkbox.DataContext is not MissionPoolEntry) return;
			var entry = (MissionPoolEntry)checkbox.DataContext;
			entry.IsInPool = false;
			RemoveMissionFromPool?.Invoke(this, entry.MissionID);
		}

		private void MissionPoolPresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var comboBox = (ComboBox)sender;
			if (comboBox.SelectedValue == null) {
				comboBox.SelectedValue = Config.Default.MissionPool;
				return;
			}

			var id = (MissionPoolPresetID)comboBox.SelectedValue;
			var presetEntry = MissionPoolPresetEntries.ToList().Find(entry => entry.Preset == id);
			if (presetEntry == null) return;
			UpdateMissionPool(presetEntry.GetMissions());
			IsCustomPoolSelected = (MissionPoolPresetID)comboBox.SelectedValue == MissionPoolPresetID.Custom;
			Config.Default.MissionPool = id;
			Config.Save();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
