using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aurora.Utils;

public class ItemValues
{
    public Dictionary<string, object> Items { get; } = new();

    public ItemValues(Stream instream)
    {
        using var sr = new StreamReader(instream);
        while (!sr.EndOfStream)
        {
            //Attempt to read the item key
            var keyValue = ReadValue(sr);
            if (keyValue != null)
                Items.Add(((string)keyValue).ToLowerInvariant(), keyValue);

            //Skip over any whitespace characters to get to next value
            while (char.IsWhiteSpace((char)sr.Peek()))
                sr.Read();
        }
    }

    private object? ReadValue(StreamReader instream)
    {
        object? returnValue = null;

        //Skip over any whitespace characters to get to next value
        while (char.IsWhiteSpace((char)instream.Peek()))
            instream.Read();

        var peekchar = (char)instream.Peek();

        switch (peekchar)
        {
            case '{':
                returnValue = ReadSubValues(instream);
                break;
            case '/':
                //Comment, read until end of line
                instream.ReadLine();
                break;
            default:
                returnValue = ReadString(instream);
                break;
        }

        return returnValue;
    }

    private string ReadString(StreamReader instream)
    {
        var builder = new StringBuilder();

        var isQuote = ((char)instream.Peek()).Equals('"');

        if (isQuote)
            instream.Read();

        for (var chr = (char)instream.Read(); !instream.EndOfStream; chr = (char)instream.Read())
        {
            if (isQuote && chr.Equals('"') ||
                !isQuote && char.IsWhiteSpace(chr)) //Arrived at end of string
                break;

            if (chr.Equals('\\')) //Fix up escaped characters
            {
                var escape = (char)instream.Read();

                switch (escape)
                {
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case '\'':
                        builder.Append('\'');
                        break;
                    case '"':
                        builder.Append('"');
                        break;
                    case '\\':
                        builder.Append('\\');
                        break;
                    case 'b':
                        builder.Append('\b');
                        break;
                    case 'f':
                        builder.Append('\f');
                        break;
                    case 'v':
                        builder.Append('\v');
                        break;
                }
            }
            else
                builder.Append(chr);
        }

        return builder.ToString();
    }

    private Dictionary<string, object> ReadSubValues(StreamReader instream)
    {
        Dictionary<string, object> subValues = new();

        //Read first {
        instream.Read();

        //Seek to next data
        while (char.IsWhiteSpace((char)instream.Peek()))
            instream.Read();

        while (!((char)instream.Peek()).Equals('}'))
        {
            var keyValue = ReadValue(instream);
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
    public static string? GetGamePath(int gameId)
    {
        Global.logger.Debug("Trying to get game path for: {GameId}", gameId);

        try
        {
            string? steamPath;

            try
            {
                steamPath = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) as string;
            }
            catch (Exception)
            {
                steamPath = "";
            }

            if (string.IsNullOrWhiteSpace(steamPath))
            {
                try
                {
                    steamPath = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null) as string;
                }
                catch (Exception)
                {
                    steamPath = "";
                }
            }

            if (!string.IsNullOrWhiteSpace(steamPath))
            {
                var manifestFile = Path.Combine(steamPath, "SteamApps", $"appmanifest_{gameId}.acf");
                if (File.Exists(manifestFile))
                {
                    ItemValues manifestData;
                    using (Stream manifestStream = new FileStream(manifestFile, FileMode.Open))
                        manifestData = new ItemValues(manifestStream);

                    var installdir = (string)((Dictionary<string, object>)manifestData.Items["appstate"])["installdir"];
                    var appidpath = Path.Combine(steamPath, "SteamApps", "common", installdir);
                    if (Directory.Exists(appidpath))
                        return appidpath;
                }
                else
                {
                    var librariesFile = Path.Combine(steamPath, "SteamApps", "libraryfolders.vdf");
                    if (File.Exists(librariesFile))
                    {
                        ItemValues libData;
                        using (Stream libStream = new FileStream(librariesFile, FileMode.Open))
                            libData = new ItemValues(libStream);

                        var libraryFolders = (Dictionary<string, object>)libData.Items["libraryfolders"];

                        for (var libraryId = 1; libraryFolders.ContainsKey(libraryId.ToString()); libraryId++)
                        {
                            var library = (Dictionary<string, object>)libraryFolders[libraryId.ToString()];
                            var libraryPath = (string)library["path"];

                            manifestFile = Path.Combine(libraryPath, "steamapps", $"appmanifest_{gameId}.acf");
                            if (File.Exists(manifestFile))
                            {
                                ItemValues manifestData;
                                using (Stream s = File.OpenRead(manifestFile))
                                {
                                    manifestData = new ItemValues(s);
                                }

                                var installdir = (string)((Dictionary<string, object>)manifestData.Items["appstate"])["installdir"];
                                var appidpath = Path.Combine(libraryPath, "steamapps", "common", installdir);
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
            Global.logger.Error(exc, "SteamUtils: GetGamePath({GameId})", gameId);
        }

        return null;
    }
}