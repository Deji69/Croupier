using PuppeteerSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
		protected string url = "";
		protected Task? task;
		protected CancellationTokenSource cancel = new();
		protected IBrowser? browser;
		protected volatile bool awaitingPreviousThread = true;

		public void SetLink(string url)
		{
			Stop();
			this.url = url;

			var browserFetcher = BrowserService.GetBrowserFetcher();
			var res = browserFetcher.GetInstalledBrowsers();

			cancel = new();
			var cancelToken = cancel.Token;
			task = Task.Run(async () => {
				var launchOptions = new LaunchOptions {
					Headless = true,
				};
				var currentUrl = url;
				Status("Launching browser");
				try {
					using (browser ??= await Puppeteer.LaunchAsync(launchOptions))
					using (var page = await browser.NewPageAsync()) {
						Status($"Navigating to URL {url}");
						var res = await page.GoToAsync(url);
						if (!res.Ok) {
							Status($"Failed to load URL {url}");
							return;
						}

						Status($"Parsing web page.");
						await page.WaitForNetworkIdleAsync(new WaitForNetworkIdleOptions() { Timeout = 15000 });

						var elem = await page.WaitForSelectorAsync(".targets,.spin-container,.spin,#container", new() { Timeout = 500 });
						try {
							var overlayElem = await page.WaitForSelectorAsync(".overlay-targets,.spin,.viewer-spin", new() { Timeout = 500 });
							if (overlayElem != null) elem = overlayElem;
						} catch (Exception) {}

						if (elem == null) {
							Status(".targets element not found.");
							return;
						}

						Status("Success. Watching for updates...");

						while (!cancelToken.IsCancellationRequested) {
							var text = await page.EvaluateFunctionAsync<string>("elem => elem.innerText", elem);
							App.Current.Dispatcher.Invoke(new Action(() => ReceiveNewSpinData?.Invoke(this, text)));

							while (!cancelToken.IsCancellationRequested && text == await page.EvaluateFunctionAsync<string>("elem => elem.innerText", elem))
								await page.WaitForTimeoutAsync(1000);
						}

						page.Dispose();
					};
					if (browser != null) {
						await browser.CloseAsync();
						browser = null;
					}
				} catch (Exception ex) {
					Status(ex.ToString());
				}
			}, cancelToken);
		}

		public async void Stop()
		{
			url = "";
			if (task == null) return;
			await cancel.CancelAsync();
			task = null;
			Status("Stopped watching spin link.");
		}

		public void ForceStop()
		{
			Stop();
			browser?.CloseAsync();
			browser = null;
		}

		protected void Status(string text)
		{
			App.Current.Dispatcher.Invoke(new Action(() => OnStatusChange?.Invoke(this, text)));
		}
	}
}
