using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Croupier
{
	public abstract class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
		{
			if (propertyName != null && !EqualityComparer<T>.Default.Equals(field, value)) {
				field = value;
				UpdateProperty(propertyName);
				return true;
			}
			return false;
		}

		protected void UpdateProperty([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
