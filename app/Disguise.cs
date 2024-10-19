using System;
using System.Collections.Specialized;
using System.IO;

namespace Croupier
{
	public class Disguise(Mission mission, string name, string image, bool suit = false, bool any = false, bool hostile = false, StringCollection? keywords = null) {
		public Mission Mission { get; private set; } = mission;
		public string Name { get; private set; } = name;
		public string Image { get; private set; } = image;
		public bool Suit { get; private set; } = suit;
		public bool Any { get; private set; } = any;
		public bool Hostile { get; private set; } = hostile;
		public StringCollection Keywords { get; private set; } = keywords ?? [];

		public Uri ImageUri {
			get => new(Path.Combine(Environment.CurrentDirectory, "outfits", Image));
		}

		public override string ToString() => Name;
	}
}
