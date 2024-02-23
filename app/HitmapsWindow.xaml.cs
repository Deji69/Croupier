using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Croupier {
	public class HitmapsWindowViewModel : ViewModel {
		private static readonly Brush notInstalledBrush = new SolidColorBrush(Color.FromRgb(200, 20, 20));
		private static readonly Brush installedBrush = new SolidColorBrush(Color.FromRgb(0, 200, 0));
		private static readonly Brush statusBrush = new SolidColorBrush(Color.FromRgb(225, 225, 225));
		private string hitmapsSpinLinkInput;
		private string spinLinkStatusText = "Enter a spin link URL and hit 'Start'.";
		private string browserStatusText = "";

		public string HitmapsSpinLinkInput {
			get => hitmapsSpinLinkInput;
			set => SetProperty(ref hitmapsSpinLinkInput, value);
		}

		public bool IsBrowserInstalled {
			get => BrowserService.IsBrowserInstalled();
			set => UpdateProperty();
		}

		public bool IsBrowserInstalling {
			get => BrowserService.IsBrowserInstalling;
			set => UpdateProperty();
		}

		public bool EnableInstallBrowserButton {
			get => !IsBrowserInstalled && !IsBrowserInstalling;
		}

		public bool EnableUninstallBrowserButton {
			get => IsBrowserInstalled && !IsBrowserInstalling;
			set => UpdateProperty();
		}

		public string BrowserInstallStatusText {
			get => browserStatusText != "" ? browserStatusText : (
					IsBrowserInstalled ? "Installed" : "Not Installed"
				);
			set {
				SetProperty(ref browserStatusText, value);
				UpdateProperty(nameof(BrowserInstallStatusColour));
			}
		}

		public Brush BrowserInstallStatusColour {
			get => browserStatusText != "" ? statusBrush : (IsBrowserInstalled ? installedBrush : notInstalledBrush);
		}

		public string SpinLinkStatusText {
			get => spinLinkStatusText;
			set => SetProperty(ref spinLinkStatusText, value);
		}
	}

	public partial class HitmapsWindow : Window {
		private readonly HitmapsWindowViewModel viewModel = new();

		public HitmapsWindow()
		{
			DataContext = viewModel;
			InitializeComponent();
			((App)App.Current).HitmapsSpinLink.OnStatusChange += (object sender, string status) => {
				viewModel.SpinLinkStatusText = status;
			};
			((App)App.Current).HitmapsSpinLink.OnBrowserStatusChange += (object sender, string status) => {
				viewModel.BrowserInstallStatusText = status;
			};
			BrowserService.InstallStatusUpdate += (object sender, string status) => {
				viewModel.BrowserInstallStatusText = status;
				if (status == "" || status == null) {
					viewModel.IsBrowserInstalling = false;
					viewModel.IsBrowserInstalled = false;
					viewModel.EnableUninstallBrowserButton = false;
				}
			};
		}

		private void SpinLinkStart_Click(object sender, RoutedEventArgs e)
		{
			if (viewModel.HitmapsSpinLinkInput == null || viewModel.HitmapsSpinLinkInput == "")
				return;
			((App)App.Current).HitmapsSpinLink.SetLink(viewModel.HitmapsSpinLinkInput);
		}

		private void SpinLinkStop_Click(object sender, RoutedEventArgs e)
		{
			((App)App.Current).HitmapsSpinLink.Stop();
		}

		private async void Install_Click(object sender, RoutedEventArgs e)
		{
			viewModel.IsBrowserInstalled = await BrowserService.InstallBrowser();
		}

		private void Uninstall_Click(object sender, RoutedEventArgs e)
		{
			BrowserService.UninstallBrowser();
			viewModel.IsBrowserInstalled = false;
		}
	}
}
