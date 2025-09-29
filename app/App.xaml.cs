using RestoreWindowPlace;
using System;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Threading;

namespace Croupier
{
	public partial class App : Application
	{
		public readonly HitmapsSpinLink HitmapsSpinLink = new();
		public readonly LiveSplitClient LiveSplitClient = new();

		[SupportedOSPlatform("windows7.0")]
		public WindowPlace WindowPlace { get; } = new WindowPlace("app.config");

		public App() : base() {
			Logging.Clear();
			Config.Load();
			CroupierSocketServer.Start();
			//CroupierPipeServer.Start();
			if (Config.Default.LiveSplitEnabled)
				_ = LiveSplitClient.StartAsync();
		} 

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show("An unhandled exception just occurred: " + (e.Exception.InnerException ?? e.Exception).Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			LiveSplitClient.Stop();
			HitmapsSpinLink.ForceStop();
			base.OnExit(e);
			Config.Save(true);
			if (OperatingSystem.IsWindowsVersionAtLeast(7))
				WindowPlace.Save();
		}
	}
}
