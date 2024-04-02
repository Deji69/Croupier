using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Croupier {
	public class NewVersionInfo(string name, string url) {
		public string name = name;
		public string url = url;
	}

	public class UpdateChecker {
		public static async Task<NewVersionInfo> CheckForUpdateAsync() {
			var currentVer = Version.Parse(Assembly.GetExecutingAssembly().GetName().Version.ToString());
			var client = new GitHubClient(new ProductHeaderValue("Croupier"));
			var release = await client.Repository.Release.GetLatest("Deji69", "Croupier");
			if (!release.TagName.StartsWith('v') || release.TagName.Length < 2)
				return null;
			var releaseVer = Version.Parse(release.TagName[1..]);
			if (!release.Prerelease && !release.Draft && releaseVer > currentVer)
				return new NewVersionInfo(release.TagName, release.HtmlUrl);
			return null;
		}

		public static void OpenUrl(string url) {
			try {
				Process.Start(url);
			}
			catch {
				// hack because of this: https://github.com/dotnet/corefx/issues/10361
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
					Process.Start("xdg-open", url);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
					Process.Start("open", url);
				}
				else {
					throw;
				}
			}
		}
	}
}
