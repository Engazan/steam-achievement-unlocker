using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;

namespace SteamAchievementUnlocker.Updates
{
    internal static class ApplicationUpdateService
    {
        private const string ManifestUrl =
            "https://github.com/Engazan/steam-achievement-unlocker/releases/latest/download/latest.json";

        private static readonly HttpClient HttpClient = CreateHttpClient();

        public static bool TryOfferUpdate()
        {
            try
            {
                Version currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
                string json = HttpClient.GetStringAsync(ManifestUrl)
                    .GetAwaiter()
                    .GetResult();
                UpdateManifest manifest = JsonSerializer.Deserialize<UpdateManifest>(json);

                if (manifest == null ||
                    Version.TryParse(manifest.Version, out Version availableVersion) == false ||
                    currentVersion == null ||
                    availableVersion <= currentVersion ||
                    Uri.TryCreate(manifest.DownloadUrl, UriKind.Absolute, out Uri downloadUri) == false)
                {
                    return false;
                }

                bool shouldUpdate = StyledDialog.Show(
                    null,
                    $"A new version ({availableVersion}) is available. Do you want to update now?",
                    "Update available",
                    StyledDialogButtons.YesNo,
                    StyledDialogIcon.Information);
                if (shouldUpdate == false)
                {
                    return false;
                }

                string applicationPath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(applicationPath))
                {
                    return false;
                }

                string updateDirectory = Path.GetDirectoryName(applicationPath);
                string downloadPath = Path.Combine(updateDirectory, $".update-{availableVersion}.exe");
                byte[] executable = HttpClient.GetByteArrayAsync(downloadUri)
                    .GetAwaiter()
                    .GetResult();
                if (string.IsNullOrWhiteSpace(manifest.Sha256) ||
                    string.Equals(
                        Convert.ToHexString(SHA256.HashData(executable)),
                        manifest.Sha256,
                        StringComparison.OrdinalIgnoreCase) == false)
                {
                    return false;
                }
                File.WriteAllBytes(downloadPath, executable);

                string updaterPath = Path.Combine(
                    updateDirectory,
                    $".updater-{Process.GetCurrentProcess().Id}.exe");
                File.Copy(applicationPath, updaterPath, true);
                ProcessStartInfo startInfo = new()
                {
                    FileName = updaterPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(applicationPath),
                };
                startInfo.ArgumentList.Add("--apply-update");
                startInfo.ArgumentList.Add(downloadPath);
                startInfo.ArgumentList.Add(applicationPath);
                startInfo.ArgumentList.Add(Process.GetCurrentProcess().Id.ToString());
                Process.Start(startInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient client = new()
            {
                Timeout = TimeSpan.FromSeconds(5),
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SteamAchievementUnlocker-Updater/1.0");
            return client;
        }
    }
}
