using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Croupier {
	public static class BrowserService {
		public static event EventHandler<string>? InstallStatusUpdate;
		private static bool installInProgress = false;
		private static int installProgressPercentage = 0;
		private static long installTotalBytes = 0;

		public static bool IsBrowserInstalling {
			get => installInProgress;
		}

		public static int BrowserInstallProgressPercent {
			get => installProgressPercentage;
		}

		public static long BrowserInstallTotalMBs {
			get => installTotalBytes != 0 ? (installTotalBytes / 1024 / 1024) : 0;
		}

		public static BrowserFetcher GetBrowserFetcher()
		{
			return new BrowserFetcher();
		}

		public static bool IsBrowserInstalled()
		{
			var browsers = GetBrowserFetcher().GetInstalledBrowsers();
			return browsers.Any();
		}

		public static async Task<bool> InstallBrowser()
		{
			if (IsBrowserInstalled() || installInProgress)
				return true;
			
			installInProgress = true;
			
			var browserFetcher = GetBrowserFetcher();

			browserFetcher.DownloadProgressChanged += (object sender, System.Net.DownloadProgressChangedEventArgs e) => {
				installProgressPercentage = e.ProgressPercentage;
				installTotalBytes = e.TotalBytesToReceive;
				Status($"Downloading... {BrowserInstallProgressPercent}% of {BrowserInstallTotalMBs} MB");
			};

			var res = await browserFetcher.DownloadAsync();
			installInProgress = false;
			Status("");
			return res != null;
		}

		public static void UninstallBrowser()
		{
			if (!IsBrowserInstalled() || IsBrowserInstalling)
				return;
			var browserFetcher = GetBrowserFetcher();
			var browsers = browserFetcher.GetInstalledBrowsers();
			installInProgress = true;

			foreach (var browser in browsers) {
				Status($"Uninstalling browser ${browser}");
				try {
					browserFetcher.Uninstall(browser.BuildId);
					Status("");
				}
				catch (Exception e) {
					Status(e.ToString());
				}
				break;
			}

			installInProgress = false;
		}

		public static void Status(string status)
		{
			InstallStatusUpdate?.Invoke(null, status);
		}
	}
	
	public class HitmapsSpinLink {
		public static event EventHandler<string>? ReceiveNewSpinData;
		public event EventHandler<string>? OnStatusChange;
		public event EventHandler<string>? OnBrowserStatusChange;
		protected string url = "";
		protected Thread? thread;
		protected IBrowser? browser;
		protected volatile bool awaitingPreviousThread = true;

		public void SetLink(string url)
		{
			Stop();
			awaitingPreviousThread = false;
			this.url = url;

			var browserFetcher = BrowserService.GetBrowserFetcher();
			var res = browserFetcher.GetInstalledBrowsers();

			thread = new Thread(async () => {
				var launchOptions = new LaunchOptions {
					Headless = true,
				};
				var currentUrl = url;
				Status("Launching browser");
				try {
					using (browser = await Puppeteer.LaunchAsync(launchOptions))
					using (var page = await browser.NewPageAsync()) {
						Status($"Navigating to URL {url}");
						var res = await page.GoToAsync(url);
						if (!res.Ok) {
							Status($"Failed to load URL {url}");
							return;
						}

						var elem = await page.WaitForSelectorAsync(".targets,.spin-container,.spin", new() { Timeout = 15000 });
						try {
							var overlayElem = await page.WaitForSelectorAsync(".overlay-targets,.spin,.viewer-spin", new() { Timeout = 500 });
							if (overlayElem != null) elem = overlayElem;
						} catch (Exception) {}

						if (elem == null) {
							Status(".targets element not found.");
							return;
						}

						Status("Success. Watching for updates...");

						try {
							while (!awaitingPreviousThread) {
								var text = await page.EvaluateFunctionAsync<string>("elem => elem.innerText", elem);
								App.Current.Dispatcher.Invoke(new Action(() => ReceiveNewSpinData?.Invoke(this, text)));

								while (!awaitingPreviousThread && text == await page.EvaluateFunctionAsync<string>("elem => elem.innerText", elem))
									await page.WaitForTimeoutAsync(1000);
							}
						} catch (ThreadInterruptedException) {}

						await browser.CloseAsync();
					};
				} catch (Exception ex) {
					Status(ex.ToString());
				}
			});
			thread.Start();
			thread.IsBackground = true;
		}

		public void Stop()
		{
			url = "";
			if (thread == null) return;
			awaitingPreviousThread = true;
			thread.Interrupt();
			thread.Join(2000);
			thread = null;
			Status("Stopped watching spin link.");
		}

		public void ForceStop()
		{
			browser?.CloseAsync();
		}

		protected void Status(string text)
		{
			App.Current.Dispatcher.Invoke(new Action(() => OnStatusChange?.Invoke(this, text)));
		}

		protected void BrowserStatus(string text)
		{
			App.Current.Dispatcher.Invoke(new Action(() => OnBrowserStatusChange?.Invoke(this, text)));
		}
	}
}
