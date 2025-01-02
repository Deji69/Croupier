using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Croupier {
	public class EditHotkeysEntry(Hotkey hotkey) : ViewModel {
		private readonly Hotkey hotkey = hotkey;
		private bool editKeybind = false;

		public string Action { get; set; } = hotkey.Name;
		public Keybind Keybind {
			get => hotkey.Keybind;
			set {
				hotkey.Keybind = value;
				UpdateProperty(nameof(Keybind));
				UpdateProperty(nameof(ButtonText));
				hotkey.RebindAction?.Invoke(hotkey.Keybind);
			}
		}
		public bool EditKeybind {
			get => editKeybind;
			set {
				editKeybind = value;
				UpdateProperty(nameof(ButtonText));
			}
		}
		public string ButtonText {
			get => EditKeybind ? "..." : Keybind.ToString();
		}
	}

	public class EditHotkeysViewModel : ViewModel {
		public List<EditHotkeysEntry> Hotkeys { get; set; } = [];
	}

	public partial class EditHotkeys : Window {
		private EditHotkeysViewModel viewModel;
		private EditHotkeysEntry? editHotkey = null;

		public EditHotkeys() {
			var hotkeys = Hotkeys.GetHotkeys();
			List<EditHotkeysEntry> entries = [];
			foreach (var hotkey in hotkeys) {
				var entry = new EditHotkeysEntry(hotkey);
				entry.PropertyChanged += Entry_PropertyChanged;
				entries.Add(entry);
			}
			viewModel = new() { Hotkeys = entries };

			DataContext = viewModel;

			InitializeComponent();
		}

		private void Entry_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
			var entry = (EditHotkeysEntry?)sender;
			if (entry == null)
				return;
			if (e.PropertyName != nameof(entry.Keybind))
				return;
			
			Hotkeys.RegisterAll();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			var button = (Button)e.Source;
			if (button == null)
				return;
			editHotkey = (EditHotkeysEntry)button.DataContext;
			editHotkey.EditKeybind = true;
		}

		private void Hotkey_PreviewKeyDown(object sender, KeyEventArgs e) {
			if (editHotkey == null)
				return;

			e.Handled = true;


			var modifiers = Keyboard.Modifiers;
			var key = e.Key;

			if (key == Key.System)
				key = e.SystemKey;

			if (modifiers == ModifierKeys.None && (key == Key.Delete || key == Key.Back || key == Key.Escape)) {
				editHotkey.Keybind = new Keybind();
				editHotkey.EditKeybind = false;
				editHotkey = null;
				return;
			}

			if (key == Key.LeftCtrl ||
				key == Key.RightCtrl ||
				key == Key.LeftAlt ||
				key == Key.RightAlt ||
				key == Key.LeftShift ||
				key == Key.RightShift ||
				key == Key.LWin ||
				key == Key.RWin ||
				key == Key.Clear ||
				key == Key.OemClear ||
				key == Key.Apps)
			{
				return;
			}

			editHotkey.Keybind = new Keybind(key, modifiers);
			editHotkey.EditKeybind = false;
			editHotkey = null;
		}

		private void Hotkey_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			if (editHotkey == null) return;
			editHotkey.EditKeybind = false;
			editHotkey = null;
		}
	}
}
