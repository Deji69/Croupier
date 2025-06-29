using System;

namespace Croupier.Exceptions {
	[Serializable]
	public class RouletteSpinException : Exception {
		public RouletteSpinException() : base() { }
		public RouletteSpinException(string message) : base(message) { }
		public RouletteSpinException(string message, Exception inner) : base(message, inner) { }
	}
}
