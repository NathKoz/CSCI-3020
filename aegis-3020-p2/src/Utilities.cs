using System.Runtime.InteropServices;

namespace aegis_3020_p2.src
{
    public class Utilities
    {
        public static string GetUserDownloadsFolder()
        {
            string userProfilePath = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile
            );

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string? xdgDownloadDir = Environment.GetEnvironmentVariable("XDG_DOWNLOAD_DIR");
                return !string.IsNullOrEmpty(xdgDownloadDir) && Directory.Exists(xdgDownloadDir)
                    ? xdgDownloadDir.Replace("$HOME", userProfilePath)
                    : Path.Combine(userProfilePath, "Downloads");
            }
            else if (
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            )
            {
                return Path.Combine(userProfilePath, "Downloads");
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
