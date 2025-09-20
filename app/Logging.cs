using System;
using System.IO;
using System.Reflection;

namespace Croupier {
	static public class Logging {
		private static string path = "";

		public static void Clear() {
			var path = GetLogPath();
			if (path.Length != 0) File.WriteAllText(path, "Logging started.\r\n");
		}

		public static void Info(string message) {
			System.Diagnostics.Debug.WriteLine(message);
			try {
				path = GetLogPath();
				if (path.Length == 0) return;
				using StreamWriter w = File.AppendText(path);
				Write(message, w);
				w.Flush();
			}
			catch (Exception) {
			}
		}

		private static void Write(string message, TextWriter writer) {
			writer.Write($"{DateTime.Now.ToShortTimeString()}: {message}\r\n");
		}

		private static string GetLogPath() {
			if (path.Length == 0) {
				path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
				if (path.Length != 0) path += "\\Croupier.log";
			}
			return path;
		}
	}
}
