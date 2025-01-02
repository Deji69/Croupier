using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace Croupier {
	public class Keybind(Key key = 0, ModifierKeys modifiers = 0) {
		public ModifierKeys Modifiers { get; set; } = modifiers;
		public Key Key { get; set; } = key;

		public bool IsAssigned { get => Key != 0; }

		public override string ToString() {
			if (Key == 0)
				return "(None)";
			var str = "";
			if (Modifiers.HasFlag(ModifierKeys.Control))
				str += "Ctrl + ";
			if (Modifiers.HasFlag(ModifierKeys.Shift))
				str += "Shift + ";
			if (Modifiers.HasFlag(ModifierKeys.Alt))
				str += "Alt + ";
			if (Modifiers.HasFlag(ModifierKeys.Windows))
				str += "Win + ";
			return str + Key;
		}
	}

	public class Hotkey(string name) : IDisposable {
		private static Dictionary<int, Hotkey>? hotkeyDict;
		private Keybind keybind = new();

		public int ID { get; set; }
		public readonly string Name = name;
		public Action? Action { get; set; }
		public Action<Keybind>? RebindAction { get; set; }
		public Keybind Keybind {
			get => keybind;
			set => keybind = value ?? new();
		}
		public bool Registered => registered;
		private bool disposed = false;
		private bool registered = false;
		public const int WmHotKey = 0x0312;

		public bool Register() {
			if (registered == true)
				return false;
			if (!Keybind.IsAssigned)
				return false;

			int vkc = KeyInterop.VirtualKeyFromKey(Keybind.Key);
			ID = vkc + ((int)Keybind.Modifiers * 0x10000);
			bool result = Hotkeys.RegisterHotKey(IntPtr.Zero, ID, (UInt32)Keybind.Modifiers, (UInt32)vkc);

			if (result) {
				registered = true;

				if (hotkeyDict == null) {
					hotkeyDict = [];
					ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
				}

				hotkeyDict.Add(ID, this);
			}

			return result;
		}

		public void Unregister() {
			if (registered == false) return;
			if (hotkeyDict != null && hotkeyDict.TryGetValue(ID, out _)) {
				Hotkeys.UnregisterHotKey(IntPtr.Zero, ID);
				hotkeyDict.Remove(ID);
				registered = false;
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposed)
				return;
			if (disposing)
				Unregister();
			disposed = true;
		}

		private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled) {
			if (handled)
				return;
			if (msg.message != WmHotKey)
				return;
			if (hotkeyDict != null && hotkeyDict.TryGetValue((int)msg.wParam, out var hotkey)) {
				hotkey.Action?.Invoke();
				handled = true;
			}
		}
	}

	public class Hotkeys {
		private static readonly List<Hotkey> hotkeys = [];

		[DllImport("user32.dll")]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vk);

		[DllImport("user32.dll")]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		public static Hotkey Add(string name, Action action) {
			var hotkey = new Hotkey(name) { Action = action };
			hotkeys.Add(hotkey);
			return hotkey;
		}

		public static List<Hotkey> GetHotkeys() {
			return hotkeys;
		}

		public static void RegisterAll() {
			foreach (var hotkey in hotkeys) {
				if (hotkey.Registered)
					hotkey.Unregister();
				if (hotkey.Keybind == null || !hotkey.Keybind.IsAssigned)
					continue;
				hotkey.Register();
			}
		}
	}
}
