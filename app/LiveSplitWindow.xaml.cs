using System;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace Croupier {
	public class LiveSplitWindowViewModel : ViewModel {
		private static readonly Brush connectedBrush = new SolidColorBrush(Color.FromRgb(0, 200, 0));
		private static readonly Brush statusBrush = new SolidColorBrush(Color.FromRgb(225, 225, 225));
		private bool enabled = false;
		private string portInput = "16834";
		private string ipInput = "127.0.0.1";
		private string statusText = "";

		public bool Enabled {
			get => enabled;
			set => SetProperty(ref enabled, value);
		}

		public string Port {
			get => portInput;
			set => SetProperty(ref portInput, value);
		}

		public string IP {
			get => ipInput;
			set => SetProperty(ref ipInput, value);
		}

		public string StatusText {
			get => statusText;
			set {
				SetProperty(ref statusText, value);
				UpdateProperty(nameof(StatusColour));
			}
		}

		public Brush StatusColour {
			get => statusText == "Connected." ? connectedBrush : statusBrush;
		}
	}

	public partial class LiveSplitWindow : Window {
		private readonly LiveSplitWindowViewModel viewModel = new();

		public LiveSplitWindow() {
			var client = ((App)App.Current).LiveSplitClient;

			client.OnStatusChange += (object? sender, string status) =>viewModel.StatusText = status;

			viewModel.StatusText = client.CurrentStatus;
			viewModel.Enabled = Config.Default.LiveSplitEnabled;
			viewModel.IP = Config.Default.LiveSplitIP;
			viewModel.Port = Config.Default.LiveSplitPort.ToString();

			DataContext = viewModel;
			InitializeComponent();
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs e) {
			viewModel.IP = viewModel.IP.Trim();
			viewModel.Port = viewModel.Port.Trim();

			if (viewModel.IP.Length == 0)
				viewModel.IP = "127.0.0.1";
			else if (viewModel.IP.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
				viewModel.IP = "localhost";
			else if (IPAddress.TryParse(viewModel.IP, out IPAddress? ipAddress))
				viewModel.IP = ipAddress.ToString();
			else {
				MessageBox.Show("Invalid IP address.");
				return;
			}

			if (!int.TryParse(viewModel.Port, out var port) || port < 0 || port > 65535) {
				MessageBox.Show("Invalid port number.");
				return;
			}

			Config.Default.LiveSplitEnabled = viewModel.Enabled;
			Config.Default.LiveSplitIP = viewModel.IP;
			Config.Default.LiveSplitPort = port;
			Config.Save();

			if (Config.Default.LiveSplitEnabled)
				_ = ((App)App.Current).LiveSplitClient.StartAsync();
			else
				((App)App.Current).LiveSplitClient.Stop();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e) {
			viewModel.Enabled = Config.Default.LiveSplitEnabled;
			viewModel.IP = Config.Default.LiveSplitIP;
			viewModel.Port = Config.Default.LiveSplitPort.ToString();
		}
	}
}
