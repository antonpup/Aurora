using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aurora.Utils
{
    public class ItemValues
    {
        public Dictionary<string, object> Items { get { return _items; } }

        private Dictionary<string, object> _items = new Dictionary<string, object>();
        public ItemValues(Stream instream)
        {
            using (StreamReader sr = new StreamReader(instream))
            {
                while (!sr.EndOfStream)
                {
                    //Attempt to read the item key
                    object keyValue = ReadValue(sr);
                    if (keyValue != null)
                        _items.Add(((string)keyValue).ToLowerInvariant(), ReadValue(sr));

                    //Skip over any whitespace characters to get to next value
                    while (char.IsWhiteSpace((char)sr.Peek()))
                        sr.Read();
                }
            }
        }

        private object ReadValue(StreamReader instream)
        {
            object returnValue = null;

            //Skip over any whitespace characters to get to next value
            while (char.IsWhiteSpace((char)instream.Peek()))
                instream.Read();

            char peekchar = (char)instream.Peek();

            if (peekchar.Equals('{'))
                returnValue = ReadSubValues(instream);
            else if (peekchar.Equals('/'))
            {
                //Comment, read until end of line
                instream.ReadLine();
            }
            else
                returnValue = ReadString(instream);

            return returnValue;
        }

        private string ReadString(StreamReader instream)
        {
            StringBuilder builder = new StringBuilder();

            bool isQuote = ((char)instream.Peek()).Equals('"');

            if (isQuote)
                instream.Read();

            for (char chr = (char)instream.Read(); !instream.EndOfStream; chr = (char)instream.Read())
            {

                if (isQuote && chr.Equals('"') ||
                    !isQuote && char.IsWhiteSpace(chr)) //Arrived at end of string
                    break;

                if (chr.Equals('\\')) //Fix up escaped characters
                {
                    char escape = (char)instream.Read();

                    if (escape.Equals('r'))
                        builder.Append('\r');
                    else if (escape.Equals('n'))
                        builder.Append('\n');
                    else if (escape.Equals('t'))
                        builder.Append('\t');
                    else if (escape.Equals('\''))
                        builder.Append('\'');
                    else if (escape.Equals('"'))
                        builder.Append('"');
                    else if (escape.Equals('\\'))
                        builder.Append('\\');
                    else if (escape.Equals('b'))
                        builder.Append('\b');
                    else if (escape.Equals('f'))
                        builder.Append('\f');
                    else if (escape.Equals('v'))
                        builder.Append('\v');

                }
                else
                    builder.Append(chr);

            }

            return builder.ToString();
        }

        private Dictionary<string, object> ReadSubValues(StreamReader instream)
        {
            Dictionary<string, object> subValues = new Dictionary<string, object>();

            //Read first {
            instream.Read();

            //Seek to next data
            while (char.IsWhiteSpace((char)instream.Peek()))
                instream.Read();

            while (!((char)instream.Peek()).Equals('}'))
            {
                object keyValue = ReadValue(instream);
                if (keyValue != null)
                    subValues.Add(((string)keyValue).ToLowerInvariant(), ReadValue(instream));

                //Seek to next data
                while (char.IsWhiteSpace((char)instream.Peek()))
                    instream.Read();
            }

            //Read last }
            instream.Read();

            return subValues;
        }
    }

    /// <summary>
    /// A class for handling Steam games
    /// </summary>
    public static class SteamUtils
    {
        /// <summary>
        /// Retrieves a path to a specified AppID
        /// </summary>
        /// <param name="gameId">The game's AppID</param>
        /// <returns>Path to the location of AppID's install</returns>
        public static string GetGamePath(int gameId)
        {
            Global.logger.Debug("Trying to get game path for: " + gameId);

            try
            {
                string steamPath = "";

                try
                {
                    steamPath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null);
                }
                catch (Exception exc)
                {
                    steamPath = "";
                }

                if (String.IsNullOrWhiteSpace(steamPath))
                {
                    try
                    {
                        steamPath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null);
                    }
                    catch (Exception exc)
                    {
                        steamPath = "";
                    }
                }


                if (!String.IsNullOrWhiteSpace(steamPath))
                {
                    string manifestFile = Path.Combine(steamPath, "SteamApps", String.Format("appmanifest_{0}.acf", gameId));
                    if (File.Exists(manifestFile))
                    {
                        ItemValues manifestData;
                        using (Stream manifestStream = new FileStream(manifestFile, FileMode.Open))
                            manifestData = new ItemValues(manifestStream);

                        string installdir = (string)((Dictionary<string, object>)manifestData.Items["appstate"])["installdir"];
                        string appidpath = Path.Combine(steamPath, "SteamApps", "common", installdir);
                        if (Directory.Exists(appidpath))
                            return appidpath;
                    }
                    else
                    {
                        string librariesFile = Path.Combine(steamPath, "SteamApps", string.Format("libraryfolders.vdf"));
                        if (File.Exists(librariesFile))
                        {
                            ItemValues libData;
                            using (Stream libStream = new FileStream(librariesFile, FileMode.Open))
                                libData = new ItemValues(libStream);

                            Dictionary<string, object> libraryFolders = (Dictionary<string, object>)libData.Items["libraryfolders"];

                            for (int libraryId = 1; libraryFolders.ContainsKey(libraryId.ToString()); libraryId++)
                            {
                                string libraryPath = (string)libraryFolders[libraryId.ToString()];

                                manifestFile = Path.Combine(libraryPath, "steamapps", string.Format("appmanifest_{0}.acf", gameId));
                                if (File.Exists(manifestFile))
                                {
                                    ItemValues manifestData;
                                    using (Stream s = File.OpenRead(manifestFile))
                                    {
                                        manifestData = new ItemValues(s);
                                    }

                                    string installdir = (string)((Dictionary<string, object>)manifestData.Items["appstate"])["installdir"];
                                    string appidpath = Path.Combine(libraryPath, "steamapps", "common", installdir);
                                    if (Directory.Exists(appidpath))
                                        return appidpath;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("SteamUtils: GetGamePath(" + gameId + ") exception: " + exc);
            }

            return null;
        }
    }
}
