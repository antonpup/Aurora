using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aurora.Utils
{
    /// <summary>
    /// A class for handling GOG games. You can use <see href="https://www.gogdb.org/products">gogdb.org</see> to find the GameID (Type should be "Game")
    /// </summary>
    public static class GOGUtils
    {
        /// <summary>
        /// Retrieves a path to a specified AppID. You can use <see href="https://www.gogdb.org/products">gogdb.org</see> to find the GameID (Type should be "Game")
        /// </summary>
        /// <param name="gameId">The game's AppID</param>
        /// <returns>Path to the location of AppID's install</returns>
        public static string GetGamePath(int gameId) => (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\Games\" + gameId, "path", null);
    }
}
