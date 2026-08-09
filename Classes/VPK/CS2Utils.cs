using Microsoft.Win32;
using SteamDatabase.ValvePak;
using ValveResourceFormat.IO;

namespace Titled_Gui.Classes.VPK
{
    public static class CS2Utils // stuff for finding cs2 path and vpk paths
    {
        public static Package? Package { get; private set; }
        public static GameFileLoader? Loader { get; private set; }
        private static bool _initialized = false;

        public static bool Initialize()
        {
            if (_initialized)
                return true;

            var dir = FindVPKPath();
            if (string.IsNullOrEmpty(dir) || !File.Exists(dir))
                return false;

            Package package = new();
            package.Read(dir);

            Package = package;
            Loader = new GameFileLoader(package, "csgo");
            _initialized = true;
            return true;
        }

        private static string? FindSteamPath()
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            string? steamPath = (string?)key?.GetValue("SteamPath");
            if (!string.IsNullOrWhiteSpace(steamPath))
            {
                foreach (var sub in new[] { "csgo", "core" })
                {
                    var path = Path.Combine(steamPath, "steamapps", "common", "Counter-Strike Global Offensive", "game", sub, "pak01_dir.vpk");
                    if (File.Exists(path))
                        return steamPath;
                }
            }

            using var key1 = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\Valve\Steam");
            string? install = (string?)key1?.GetValue("InstallPath");
            if (!string.IsNullOrWhiteSpace(install))
            {
                if (!string.IsNullOrWhiteSpace(steamPath))
                {
                    foreach (var sub in new[] { "csgo", "core" })
                    {
                        var path = Path.Combine(steamPath, "steamapps", "common", "Counter-Strike Global Offensive", "game", sub, "pak01_dir.vpk");
                        if (File.Exists(path))
                            return steamPath;
                    }
                }
            }

            return null;
        }

        private static string? FindVPKPath()
        {
            string? steamPath = FindSteamPath();

            if (string.IsNullOrWhiteSpace(steamPath))
                return null;

            foreach (var sub in new[] { "csgo", "core" })
            {
                var path = Path.Combine(steamPath, "steamapps", "common", "Counter-Strike Global Offensive", "game", sub, "pak01_dir.vpk");
                if (File.Exists(path))
                    return path;
            }

            return null;
        }
    }
}