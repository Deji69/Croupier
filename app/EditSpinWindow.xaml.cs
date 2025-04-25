using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Croupier
{
	public class EditSpinComboBoxItem (string name = "", bool isSeparator = false) {
		public string Name { get; set; } = name;
		public Uri? Image { get; set; }
		public bool IsSeparator { get; set; } = isSeparator;
	}

	public class MethodComboBoxItem : EditSpinComboBoxItem {
		public KillMethod Method { get; set; }

		public MethodComboBoxItem(KillMethod method, bool isSeparator = false) : base(method.Name, isSeparator) {
			Method = method;
			Image = method.ImageUri;
		}
	}

	public class VariantComboBoxItem(KillMethodVariant variant, bool isSeparator = false) : EditSpinComboBoxItem(variant.VariantName, isSeparator) {
		public KillMethodVariant Variant { get; set; } = variant;
	}

	public class DisguiseComboBoxItem : EditSpinComboBoxItem {
		public Disguise Disguise { get; set; }

		public DisguiseComboBoxItem(Disguise disguise, bool isSeparator = false) : base(disguise.Name, isSeparator) {
			Disguise = disguise;
			Image = disguise.ImageUri;
		}
	}

	public class EditSpinCondition(Target target, Disguise disguise, SpinKillMethod method) : SpinCondition(target, disguise, method), INotifyPropertyChanged {
		private bool _isLegal = false;

		public new bool IsEditLegal {
			get => _isLegal;
			set {
				_isLegal = value;
				OnPropertyChanged(nameof(IsEditLegal));
				OnPropertyChanged(nameof(LegalityText));
			}
		}

		public string LegalityText => IsEditLegal ? "" : "Illegal Condition";

		public bool IsLiveKillChecked {
			get => Kill.Complication == KillComplication.Live;
			set {
				Kill.Complication = value ? KillComplication.Live : KillComplication.None;
				OnPropertyChanged(nameof(IsLiveKillChecked));
			}
		}

		public VariantComboBoxItem? SelectedType {
			get => KillTypes.First(t => t.Variant == Kill.Method);
			set {
				if (value == null) return;
				Kill.Method = value.Variant;
				OnPropertyChanged(nameof(SelectedType));
			}
		}

		public int SelectedTypeIndex {
			get => KillTypes.FindIndex(t => t.Variant == Kill.Method);
			set {
				if (value < 0 || value > KillTypes.Count) return;
				SelectedType = KillTypes[value];
			}
		}

		public DisguiseComboBoxItem? SelectedDisguise {
			get => Disguises.Find(v => v.Disguise == Disguise);
			set {
				if (value?.Disguise == null) return;
				Disguise = value.Disguise;
				OnPropertyChanged(nameof(SelectedDisguise));
			}
		}

		public int SelectedDisguiseIndex {
			get => Disguises.FindIndex(v => v.Disguise == Disguise);
			set {
				if (value < 0 || value > Disguises.Count) return;
				SelectedDisguise = Disguises[value];
			}
		}

		public MethodComboBoxItem? SelectedMethod {
			get => (MethodComboBoxItem?)ValidMethods.Find(v => v is MethodComboBoxItem w && Kill.Method.IsSameMethod(w.Method));
			set {
				if (value == null) return;
				Kill.Method = value.Method;
				OnPropertyChanged(nameof(KillTypes));
				OnPropertyChanged(nameof(SelectedTypeIndex));
				OnPropertyChanged(nameof(SelectedMethod));
			}
		}

		public int SelectedMethodIndex {
			get => ValidMethods.FindIndex(v => v is MethodComboBoxItem w && Kill.Method.IsSameMethod(w.Method));
			set {
				if (value < 0 || value > ValidMethods.Count) return;
				if (ValidMethods[value] is MethodComboBoxItem km)
					SelectedMethod = km;
			}
		}

		public List<VariantComboBoxItem> KillTypes {
			get {
				var items = new List<VariantComboBoxItem>();
				Kill.Method.GetBasicMethod().Variants.ForEach(v => items.Add(new(v)));
				return items;
			}
		}

		public List<DisguiseComboBoxItem> Disguises {
			get {
				var items = new List<DisguiseComboBoxItem>();
				Target.Mission?.Disguises.ForEach(v => items.Add(new(v)));
				return items;
			}
		}

		public List<EditSpinComboBoxItem> ValidMethods {
			get {
				var items = new List<EditSpinComboBoxItem>();

				var uniqueMethods = Roulette.Main.GetUniqueMethods(Target).ToList();

				if (uniqueMethods.Count > 0) {
					items.Add(new("Unique", true));
					uniqueMethods.ForEach(km => items.Add(new MethodComboBoxItem(km)));
				}

				if (Target.Type == TargetType.Unique) {
					items.Add(new("Weapons", true));
					Roulette.Main.WeaponMethods.ForEach(km => items.Add(new MethodComboBoxItem(km)));
				}

				if (Target.Type == TargetType.Unique)
					return items;

				items.Add(new("Standard", true));
				Roulette.Main.StandardMethods.ForEach(km => items.Add(new MethodComboBoxItem(km)));

				items.Add(new("Weapons", true));
				Roulette.Main.WeaponMethods.ForEach(km => items.Add(new MethodComboBoxItem(km)));
				
				if (Target.Mission != null && Target.Mission.Methods.Count > 0) {
					items.Add(new("Lethal Melee", true));
					Target.Mission.Methods.ForEach(km => items.Add(new MethodComboBoxItem(km)));
				}

				return items;
			}
		}
	}

	public class EditSpinViewModel : ViewModel {
		public ObservableCollection<EditSpinCondition> Conditions { get; set; } = [];
	}

	public partial class EditSpinWindow : Window
	{
		private readonly EditSpinViewModel viewModel = new();
		public event EventHandler<SpinCondition>? SetCondition;

		public EditSpinWindow(List<SpinCondition> conds)
		{
			UpdateConditions(conds);
			DataContext = viewModel;
			InitializeComponent();
		}

		public void UpdateConditions(List<SpinCondition> conds)
		{
			viewModel.Conditions.Clear();
			foreach (var cond in conds) {
				var condition = new EditSpinCondition(cond.Target, cond.Disguise, cond.Kill) {
					IsEditLegal = cond.IsLegal()
				};
				viewModel.Conditions.Add(condition);
				condition.PropertyChanged += (object? sender, PropertyChangedEventArgs e) => {
					if (e.PropertyName != nameof(EditSpinCondition.IsEditLegal)
						&& e.PropertyName != nameof(EditSpinCondition.LegalityText))
						condition.IsEditLegal = condition.IsLegal();
					SetCondition?.Invoke(sender, condition);
				};
			}

			RefitWindow();
		}

		private void OnSizeChange(object sender, SizeChangedEventArgs e)
		{
			if (e.WidthChanged) RefitWindow();
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
