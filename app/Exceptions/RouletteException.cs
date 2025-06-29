using System;

namespace Croupier.Exceptions {
	[Serializable]
	public class RouletteException : Exception {
		public RouletteException() : base() { }
		public RouletteException(string message) : base(message) { }
		public RouletteException(string message, Exception inner) : base(message, inner) { }
	}
}
