using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Croupier {
	public class TimeFormatter {
		public static string FormatSecondsTime(double time, bool allowFrac = true) {
			var ts = TimeSpan.FromSeconds(time);
			string str;

			if (ts.TotalHours >= 1)
				str = ts.ToString(@"h\h\ m\m\ ss\s");
			else if (ts.TotalMinutes >= 1)
				str = ts.ToString(@"m\m\ ss\s");
			else
				str = ts.ToString(@"ss\s");

			var enableFrac = allowFrac && ts.TotalMinutes < 10;
			var frac = ts.ToString("FFF").TrimEnd('0');

			return str + (enableFrac && frac.Length > 0 ? $" {frac}ms" : "");
		}
	}
}
