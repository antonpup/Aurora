using System;
using System.IO;
using Microsoft.Win32;
using VdfParser;

namespace Aurora.Utils.Steam;

/// <summary>
/// A class for handling Steam games
/// </summary>
public static class SteamUtils
{
    private static readonly VdfDeserializer VdfDeserializer = new();

    /// <summary>
    /// Retrieves a path to a specified AppID
    /// </summary>
    /// <param name="gameId">The game's AppID</param>
    /// <returns>Path to the location of AppID's install</returns>
    public static string? GetGamePath(int gameId)
    {
        Global.logger.Debug("Trying to get game path for: {GameId}", gameId);

        try
        {
            var steamPath = GetSteamPath();

            if (string.IsNullOrWhiteSpace(steamPath))
            {
                return null;
            }

            var librariesFile = Path.Combine(steamPath, "SteamApps", "LibraryFolders.vdf");
            if (!File.Exists(librariesFile)) return null;

            var steamLibrary = VdfDeserializer.Deserialize<SteamLibrary>(File.ReadAllText(librariesFile));

            foreach (var (_, libraryFolder) in steamLibrary.Libraryfolders)
            {
                if (!libraryFolder.Apps.ContainsKey(gameId)) continue;

                var libraryPath = libraryFolder.Path;

                var manifestFile = Path.Combine(libraryPath, "SteamApps", $"AppManifest_{gameId}.acf");
                if (!File.Exists(manifestFile)) continue;

                var appManifest = VdfDeserializer.Deserialize<AppManifest>(File.ReadAllText(manifestFile));
                var installDir = appManifest.AppState.InstallDir;
                var appIdPath = Path.Combine(libraryFolder.Path, "SteamApps", "common", installDir);

                if (Directory.Exists(appIdPath))
                    return appIdPath;
            }
            return null;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "SteamUtils: GetGamePath({GameId})", gameId);
            return null;
        }
    }

    private static string? GetSteamPath()
    {
        string? steamPath;

        try
        {
            steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) as string;
        }
        catch (Exception)
        {
            steamPath = "";
        }

        if (!string.IsNullOrWhiteSpace(steamPath))
            return steamPath;
        try
        {
            return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null) as string;
        }
        catch (Exception)
        {
            return "";
        }
    }
}