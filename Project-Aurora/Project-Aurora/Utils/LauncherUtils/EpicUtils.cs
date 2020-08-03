using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aurora.Utils
{
    /// <summary>
    /// A class for handling Epic Games games
    /// </summary>
    public static class EpicUtils
    {
        private static List<GameManifest> CachedManifestList;
        private static long CachedAtTime;


        public class GameManifest
        {
            /// <summary>
            /// Isn't combined with InstallLocation. Could be "\bin\Game.exe"
            /// </summary>
            public string LaunchExecutable { get; set; }
            [JsonIgnore]
            public string ExeFileName { get => Path.GetFileName(LaunchExecutable); }
            /// <summary>
            /// Epic Games codename
            /// </summary>
            public string AppName { get; set; }
            /// <summary>
            /// Game root folder
            /// </summary>
            public string InstallLocation { get; set; }
        }

        /// <summary>
        /// Retrieves info about a game by it's Epic AppName (codename)
        /// </summary>
        /// <param name="appName">Epic Games AppName/codename</param>
        /// <param name="manifestList">Optional GameManifestList to search for the appName</param>
        /// <returns>game info</returns>
        public static GameManifest GetGameManifestByAppName(string appName, List<GameManifest> manifestList = null)
        {
            manifestList ??= GetManifestList();
            return manifestList.Find(GameManifest => GameManifest.AppName == appName);
        }

        /// <summary>
        /// Retrieves info about a game installed with the Epic Games Launcher by it's Executable filename
        /// </summary>
        /// <param name="exeFileName">The game's .exe file name</param>
        /// <param name="manifestList">Optional GameManifestList to search for the file name</param>
        /// <returns>game info</returns>
        public static GameManifest GetGameManifestByExe(string exeFileName, List<GameManifest> manifestList = null)
        {
            manifestList ??= GetManifestList();
            return manifestList.Find(GameManifest => GameManifest.ExeFileName == exeFileName);
        }

        /// <summary>
        /// Deserializes all found Manifest files into a list. (Will use cached list if last request was less than 5 secounds ago.)
        /// </summary>
        /// <returns>List of all GameManifests</returns>
        public static List<GameManifest> GetManifestList()
        {
            long SecondsSinceEpoche = Utils.Time.GetSecondsSinceEpoch();
            if (CachedManifestList == null || SecondsSinceEpoche >= CachedAtTime + 5)
            {
                CachedManifestList = DeserializeManifestFiles();
                CachedAtTime = SecondsSinceEpoche;
            }

            return CachedManifestList;
        }

        /// <summary>
        /// Deserializes all found Manifest files.
        /// </summary>
        /// <returns>List of all GameManifests</returns>
        private static List<GameManifest> DeserializeManifestFiles()
        {
            List<GameManifest> GameManifestList = new List<GameManifest>();

            string manifestPath = "";

            try
            {
                //alternative data source "C:\ProgramData\Epic\UnrealEngineLauncher\LauncherInstalled.dat"
                //default is C:\ProgramData\Epic\EpicGamesLauncher\Data\(Manifests)
                manifestPath = Path.Combine((string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Epic Games\EpicGamesLauncher", "AppDataPath", null) 
                    ?? (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Epic Games\EpicGamesLauncher", "AppDataPath", null), "Manifests");
            }
            catch (Exception exc)
            {
                Global.logger.Info("EpicUtils: could't find Epic Games Launcher AppDataPath: " + exc);
                return GameManifestList;
            }

            try
            {
                string[] manifestFiles = Directory.GetFiles(manifestPath, "*.item", SearchOption.TopDirectoryOnly);
                GameManifest manifest;

                for (int i = 0; i < manifestFiles.Length; i++)
                {
                    using (StreamReader file = File.OpenText(manifestFiles[i]))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        manifest = (GameManifest)serializer.Deserialize(file, typeof(GameManifest));
                        GameManifestList.Add(manifest);
                    }
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("EpicUtils: GetManifestList exception: " + exc);
            }
            return GameManifestList;
        }
    }
}
