using Croupier.Properties;
using RestoreWindowPlace;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Croupier
{
	public partial class App : Application
	{
		public readonly HitmapsSpinLink HitmapsSpinLink = new();
		public readonly LiveSplitClient LiveSplitClient = new();

		public WindowPlace WindowPlace { get; } = new WindowPlace("app.config");

		public App() : base() {
			Config.Load();
			CroupierSocketServer.Start();
			if (Config.Default.LiveSplitEnabled)
				_ = LiveSplitClient.StartAsync();
		} 

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}

		protected override void OnExit(ExitEventArgs e)
		{
			LiveSplitClient.Stop();
			HitmapsSpinLink.ForceStop();
			base.OnExit(e);
			Config.Save(true);
			WindowPlace.Save();
		}
	}
}
