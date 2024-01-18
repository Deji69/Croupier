﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Croupier.UI
{
	public class Disguise(string name, string image, bool suit = false) {
		public string Name { get; set; } = name;
		public string Image { get; set; } = image;
		public bool Suit { get; set; } = suit;

		public Uri ImageUri {
			get {
				return new Uri(Path.Combine(Environment.CurrentDirectory, "outfits", Image));
			}
		}
	}
}
