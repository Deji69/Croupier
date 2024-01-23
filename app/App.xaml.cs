using RestoreWindowPlace;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Croupier
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public WindowPlace WindowPlace { get; } = new WindowPlace("app.config");

		public App() : base() {
			CroupierSocketServer.Start();
		} 

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			WindowPlace.Save();
		}
	}
}
