using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Croupier {
	public static partial class Strings {
		public static readonly Regex TokenCharacterRegex = GenerateTokenCharacterRegex();
		public static readonly Regex TokenCharacterWithSpacesRegex = GenerateTokenCharacterWithSpacesRegex();
		public static readonly Regex URLCharacterRegex = GenerateURLCharacterRegex();

		static Dictionary<string, string> foreign_characters = new(){
			{ "äæǽ", "ae" },
			{ "öœ", "oe" },
			{ "ü", "ue" },
			{ "Ä", "Ae" },
			{ "Ü", "Ue" },
			{ "Ö", "Oe" },
			{ "ÀÁÂÃÄÅǺĀĂĄǍΑΆẢẠẦẪẨẬẰẮẴẲẶА", "A" },
			{ "àáâãåǻāăąǎªαάảạầấẫẩậằắẵẳặа", "a" },
			{ "Б", "B" },
			{ "б", "b" },
			{ "ÇĆĈĊČ", "C" },
			{ "çćĉċč", "c" },
			{ "Д", "D" },
			{ "д", "d" },
			{ "ÐĎĐΔ", "Dj" },
			{ "ðďđδ", "dj" },
			{ "ÈÉÊËĒĔĖĘĚΕΈẼẺẸỀẾỄỂỆЕЭ", "E" },
			{ "èéêëēĕėęěέεẽẻẹềếễểệеэ", "e" },
			{ "Ф", "F" },
			{ "ф", "f" },
			{ "ĜĞĠĢΓГҐ", "G" },
			{ "ĝğġģγгґ", "g" },
			{ "ĤĦ", "H" },
			{ "ĥħ", "h" },
			{ "ÌÍÎÏĨĪĬǏĮİΗΉΊΙΪỈỊИЫ", "I" },
			{ "ìíîïĩīĭǐįıηήίιϊỉịиыї", "i" },
			{ "Ĵ", "J" },
			{ "ĵ", "j" },
			{ "ĶΚК", "K" },
			{ "ķκк", "k" },
			{ "ĹĻĽĿŁΛЛ", "L" },
			{ "ĺļľŀłλл", "l" },
			{ "М", "M" },
			{ "м", "m" },
			{ "ÑŃŅŇΝН", "N" },
			{ "ñńņňŉνн", "n" },
			{ "ÒÓÔÕŌŎǑŐƠØǾΟΌΩΏỎỌỒỐỖỔỘỜỚỠỞỢО", "O" },
			{ "òóôõōŏǒőơøǿºοόωώỏọồốỗổộờớỡởợо", "o" },
			{ "П", "P" },
			{ "п", "p" },
			{ "ŔŖŘΡР", "R" },
			{ "ŕŗřρр", "r" },
			{ "ŚŜŞȘŠΣС", "S" },
			{ "śŝşșšſσςс", "s" },
			{ "ȚŢŤŦτТ", "T" },
			{ "țţťŧт", "t" },
			{ "ÙÚÛŨŪŬŮŰŲƯǓǕǗǙǛŨỦỤỪỨỮỬỰУ", "U" },
			{ "ùúûũūŭůűųưǔǖǘǚǜυύϋủụừứữửựу", "u" },
			{ "ÝŸŶΥΎΫỲỸỶỴЙ", "Y" },
			{ "ýÿŷỳỹỷỵй", "y" },
			{ "В", "V" },
			{ "в", "v" },
			{ "Ŵ", "W" },
			{ "ŵ", "w" },
			{ "ŹŻŽΖЗ", "Z" },
			{ "źżžζз", "z" },
			{ "ÆǼ", "AE" },
			{ "ß", "ss" },
			{ "Ĳ", "IJ" },
			{ "ĳ", "ij" },
			{ "Œ", "OE" },
			{ "ƒ", "f" },
			{ "ξ", "ks" },
			{ "π", "p" },
			{ "β", "v" },
			{ "μ", "m" },
			{ "ψ", "ps" },
			{ "Ё", "Yo" },
			{ "ё", "yo" },
			{ "Є", "Ye" },
			{ "є", "ye" },
			{ "Ї", "Yi" },
			{ "Ж", "Zh" },
			{ "ж", "zh" },
			{ "Х", "Kh" },
			{ "х", "kh" },
			{ "Ц", "Ts" },
			{ "ц", "ts" },
			{ "Ч", "Ch" },
			{ "ч", "ch" },
			{ "Ш", "Sh" },
			{ "ш", "sh" },
			{ "Щ", "Shch" },
			{ "щ", "shch" },
			{ "ЪъЬь", "" },
			{ "Ю", "Yu" },
			{ "ю", "yu" },
			{ "Я", "Ya" },
			{ "я", "ya" },
		};

		public static char RemoveDiacritics(this char c)
		{
			foreach (KeyValuePair<string, string> entry in foreign_characters) {
				if (entry.Key.Contains(c)) {
					return entry.Value[0];
				}
			}
			return c;
		}

		public static string RemoveDiacritics(this string s)
		{
			string text = "";


			foreach (char c in s) {
				int len = text.Length;

				foreach (KeyValuePair<string, string> entry in foreign_characters) {
					if (entry.Key.Contains(c)) {
						text += entry.Value;
						break;
					}
				}

				if (len == text.Length) {
					text += c;
				}
			}
			return text;
		}

		[GeneratedRegex("[^a-zA-Z0-9]")]
		private static partial Regex GenerateTokenCharacterRegex();

		[GeneratedRegex("[^\\sa-zA-Z0-9]")]
		private static partial Regex GenerateTokenCharacterWithSpacesRegex();

		[GeneratedRegex("[^a-zA-Z0-9]+")]
		private static partial Regex GenerateURLCharacterRegex();
	}
}