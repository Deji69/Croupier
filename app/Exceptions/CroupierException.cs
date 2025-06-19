using System;

namespace Croupier.Exceptions {
	[Serializable]
	public class CroupierException : Exception {
		public CroupierException() : base() { }
		public CroupierException(string message) : base(message) { }
		public CroupierException(string message, Exception inner) : base(message, inner) { }
	}
}
