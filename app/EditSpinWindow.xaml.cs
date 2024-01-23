using Croupier.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Croupier
{
	public class MethodComboBoxItem(string name = "", bool isSeparator = false) {
		public KillType? KillType { get; set; } = null;
		public StandardKillMethod? Standard { get; set; } = null;
		public SpecificKillMethod? Specific { get; set; } = null;
		public FirearmKillMethod? Weapon { get; set; } = null;
		public Disguise Disguise { get; set; } = null;
		public string Name { get; set; } = name;
		public Uri Image { get; set; }
		public bool IsSeparator { get; set; } = isSeparator;
	}

	public class EditSpinCondition : SpinCondition, INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private readonly List<MethodComboBoxItem> firearmKillTypes = [
			new("Any") { KillType = KillType.Any },
			new("Silenced") { KillType = KillType.Silenced },
			new("Loud") { KillType = KillType.Loud },
		];
		private readonly List<MethodComboBoxItem> explosiveKillTypes = [
			new("Any") { KillType = KillType.Any },
			new("Loud") { KillType = KillType.Loud },
		];
		private readonly List<MethodComboBoxItem> meleeKillTypes = [
			new("Any") { KillType = KillType.Any },
			new("Melee") { KillType = KillType.Melee },
			new("Thrown") { KillType = KillType.Thrown },
		];

		public bool IsLiveKillChecked {
			get {
				return Method.Complication == KillComplication.Live;
			}
			set {
				Method.Complication = value ? KillComplication.Live : null;
				OnPropertyChanged(nameof(IsLiveKillChecked));
			}
		}

		public MethodComboBoxItem SelectedType {
			get {
				foreach (var item in KillTypes) {
					if (item.KillType == Method.KillType)
						return item;
				}
				return null;
			}
			set {
				if (value.KillType == null) return;
				Method.KillType = value.KillType.Value;
				OnPropertyChanged(nameof(SelectedType));
			}
		}

		public int SelectedTypeIndex {
			get {
				var anyIdx = 0;
				for (var i = 0; i < KillTypes.Count; i++) {
					if (anyIdx == -1 && KillTypes[i].KillType == KillType.Any)
						anyIdx = i;
					if (KillTypes[i].KillType == Method.KillType)
						return i;
				}
				return anyIdx;
			}
			set {
				if (value < 0 || value > KillTypes.Count) return;
				SelectedType = KillTypes[value];
			}
		}

		public MethodComboBoxItem SelectedDisguise {
			get {
				return Disguises.Find(v => v.Disguise.Name == Disguise.Name);
			}
			set {
				if (value.Disguise == null) return;
				Disguise = value.Disguise;
				OnPropertyChanged(nameof(SelectedDisguise));
			}
		}

		public int SelectedDisguiseIndex {
			get {
				return Disguises.FindIndex(v => v.Disguise.Name == Disguise.Name);
			}
			set {
				if (value < 0 || value > Disguises.Count) return;
				SelectedDisguise = Disguises[value];
			}
		}

		public MethodComboBoxItem SelectedMethod {
			get {
				return ValidMethods.Find(v => Method.Type switch {
					KillMethodType.Standard => v.Standard != null && v.Standard.Value == Method.Standard,
					KillMethodType.Firearm => v.Weapon != null && v.Weapon.Value == Method.Firearm,
					KillMethodType.Specific => v.Specific != null && v.Specific.Value == Method.Specific,
					_ => false,
				});
			}
			set {
				bool methodTypeChange;
				if (value.Standard != null) {
					methodTypeChange = Method.Type != KillMethodType.Standard;
					Method.Type = KillMethodType.Standard;
					Method.Standard = value.Standard;
				}
				else if (value.Specific != null) {
					methodTypeChange = Method.Type != KillMethodType.Specific;
					Method.Type = KillMethodType.Specific;
					Method.Specific = value.Specific;
				}
				else if (value.Weapon != null) {
					methodTypeChange = Method.Type != KillMethodType.Firearm;
					Method.Type = KillMethodType.Firearm;
					Method.Firearm = value.Weapon;
				}
				else return;
				if (methodTypeChange) {
					Method.KillType = KillType.Any;
					OnPropertyChanged(nameof(KillTypes));
					OnPropertyChanged(nameof(SelectedTypeIndex));
				}
				OnPropertyChanged(nameof(SelectedMethod));
			}
		}

		public int SelectedMethodIndex {
			get {
				var index = ValidMethods.FindIndex(v => Method.Type switch {
					KillMethodType.Standard => v.Standard != null && v.Standard.Value == Method.Standard,
					KillMethodType.Firearm => v.Weapon != null && v.Weapon.Value == Method.Firearm,
					KillMethodType.Specific => v.Specific != null && v.Specific.Value == Method.Specific,
					_ => false,
				});
				return index;
			}
			set {
				if (value < 0 || value > ValidMethods.Count) return;
				SelectedMethod = ValidMethods[value];
			}
		}

		public List<MethodComboBoxItem> KillTypes {
			get {
				if (Method.Type == KillMethodType.Firearm) {
					if (Method.Firearm == FirearmKillMethod.Explosive)
						return explosiveKillTypes;
					return firearmKillTypes;
				}
				else if (Method.Type == KillMethodType.Specific) {
					return meleeKillTypes;
				}
				return [];
			}
		}

		public List<MethodComboBoxItem> Disguises {
			get {
				var items = new List<MethodComboBoxItem>();
				var mission = new Mission(Target.Mission);
				mission.Disguises.ForEach(v => {
					items.Add(new(v.Name) {
						Image = v.ImageUri,
						Disguise = v,
					});
				});
				return items;
			}
		}

		public List<MethodComboBoxItem> ValidMethods {
			get {
				var items = new List<MethodComboBoxItem>();
				var mission = new Mission(Target.Mission);
				if (Target.Type == TargetType.Soders) {
					items.Add(new("Firearms", true));
					KillMethod.FirearmList.ForEach(v => {
						var km = new KillMethod(KillMethodType.Firearm) { Firearm = v };
						items.Add(new(km.Name) {
							Weapon = v,
							Image = km.ImageUri,
						});
					});
					items.Add(new("Specific", true));
					KillMethod.SodersKillsList.ForEach(v => {
						var km = new KillMethod(KillMethodType.Specific) { Specific = v };
						items.Add(new(km.Name) {
							Specific = v,
							Image = km.ImageUri,
						});
					});
					return items;
				}

				items.Add(new("Standard", true));
				KillMethod.StandardList.ForEach(v => {
					var km = new KillMethod(KillMethodType.Standard) { Standard = v };
					items.Add(new(km.Name) {
						Standard = v,
						Image = km.ImageUri,
					});
				});

				items.Add(new("Weapons", true));
				KillMethod.WeaponList.ForEach(v => {
					var km = new KillMethod(KillMethodType.Firearm) { Firearm = v };
					items.Add(new(km.Name) {
						Weapon = v,
						Image = km.ImageUri,
					});
				});

				if (mission.Methods.Count > 0) {
					items.Add(new("Lethal Melee", true));
					mission.Methods.ForEach(v => {
						var km = new KillMethod(KillMethodType.Specific) { Specific = v };
						items.Add(new(km.Name) {
							Specific = v,
							Image = km.ImageUri,
						});
					});
				}

				return items;
			}
		}

		public virtual void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	// Let's try actually using a view model?
	public class EditSpinViewModel : ViewModel {
		public ObservableCollection<EditSpinCondition> Conditions { get; set; } = [];
	}

	public partial class EditSpinWindow : Window
	{
		private readonly EditSpinViewModel viewModel = new();
		public event EventHandler<SpinCondition> SetCondition;


		public EditSpinWindow(List<SpinCondition> conds)
		{
			UpdateConditions(conds);
			DataContext = viewModel;
			//ConditionsEdit.ItemsSource = viewModel.Conditions;
			InitializeComponent();
		}

		public void UpdateConditions(List<SpinCondition> conds)
		{
			viewModel.Conditions.Clear();
			foreach (var cond in conds) {
				var condition = new EditSpinCondition() {
					Target = cond.Target,
					Disguise = cond.Disguise,
					Method = cond.Method,
				};
				viewModel.Conditions.Add(condition);
				condition.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
					SetCondition?.Invoke(sender, condition);
				};
			}

			RefitWindow();
		}

		private void OnSizeChange(object sender, SizeChangedEventArgs e)
		{
			if (e.WidthChanged) {
				RefitWindow();
			}
		}

		private void RefitWindow()
		{
			if (viewModel.Conditions.Count >= 4) MinHeight = 4 * 104.3 + 40;
			else if (viewModel.Conditions.Count >= 3) MinHeight = 3 * 104.3 + 40;
			else if (viewModel.Conditions.Count >= 2) MinHeight = 2 * 104.3 + 40;
			else if (viewModel.Conditions.Count >= 1) MinHeight = 1 * 104.3 + 40;
			MaxHeight = viewModel.Conditions.Count * 104.3 + 40;
		}
	}
}
