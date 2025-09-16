using System;

namespace Croupier.Exceptions {
	[Serializable]
	public class ParserException : Exception {
		public ParserException() : base() { }
		public ParserException(string message) : base(message) { }
		public ParserException(string message, Exception inner) : base(message, inner) { }
	}
}
