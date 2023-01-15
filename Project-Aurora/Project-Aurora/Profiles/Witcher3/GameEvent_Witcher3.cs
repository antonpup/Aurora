using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Aurora.EffectsEngine;
using Aurora.Profiles.Witcher3.GSI;
using Aurora.Profiles.Witcher3.GSI.Nodes;
using Aurora.Utils;

namespace Aurora.Profiles.Witcher3;

public class GameEvent_Witcher3 : LightEvent
{
    private string _configContent;
    private bool _isGameStateDirty = false;//used to know if we need to update the gamestate or not
    private const string ArtemisString = "[Artemis]";
    private static readonly string ConfigFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\The Witcher 3";
    private static readonly string ConfigFile = Path.Combine(ConfigFolder, "user.settings");
    private static readonly string Dx12ConfigFile = Path.Combine(ConfigFolder, "dx12user.settings");

    private readonly FileSystemWatcher _watcher;
    private readonly FileSystemWatcher _dx12Watcher;

    //The mod this uses was taken from https://github.com/SpoinkyNL/Artemis/, with Spoinky's permission
    public GameEvent_Witcher3()
    {
        if (Directory.Exists(ConfigFolder))
        {
            _watcher = new FileSystemWatcher
            {
                Path = ConfigFolder,
                Filter = "user.settings",
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            _watcher.Changed += (o, e) => ReadConfigFile(ConfigFile);
        }
        if (Directory.Exists(ConfigFolder))
        {
            _dx12Watcher = new FileSystemWatcher
            {
                Path = ConfigFolder,
                Filter = "dx12user.settings",
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            _dx12Watcher.Changed += (o, e) => ReadConfigFile(Dx12ConfigFile);
        }

        ReadConfigFile(ConfigFile);
        ReadConfigFile(Dx12ConfigFile);
    }

    private void ReadConfigFile(string configFile)
    {
        if (!File.Exists(configFile)) return;
        try
        {
            using (var reader = new StreamReader(File.Open(configFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                _configContent = reader.ReadToEnd();
            }
            _isGameStateDirty = true;
        }
        catch (IOException) { }//ignore read exception
    }

    /// <summary>
    /// Returns each useful field parsed from the game file, or null if nothing found
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private string[] GetUsefulData(string file)
    {
        try
        {
            int start = file.IndexOf(ArtemisString);//finds the beginning of the artemis string
            int end = file.IndexOf("[", start + ArtemisString.Length);//finds the first [ after the artemis section header
            return file.Substring(start, end - start)//obtains just the useful part of the config
                .Replace("\n", "")//removes all \n
                .Split('\r')//splits into lines
                .Where(s => s != string.Empty && !s.Contains("Artemis"))//removes last empty line and header
                .ToArray();
        }
        catch//if we reach the catch block, we either didnt find anything, or what we found wasnt formatted correctly
        {//most likely the mod isnt installed properly
            return null;
        }
    }

    /// <summary>
    /// Returns an int referring to the field identified by the name parameter
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private int GetInt(string[] data, string name)
    {
        try
        {
            return int.Parse(data.FirstOrDefault(d => d.Contains(name)).Split('=')[1].Split('.')[0]);
        }
        catch
        {
            return -1;
        }
    }

    public override void ResetGameState()
    {
        _game_state = new GameState_Witcher3();
    }

    public override void UpdateTick()
    {
        if (!File.Exists(ConfigFile))
            return;

        if (_configContent == null || !_isGameStateDirty) return;
        try
        {
            var data = GetUsefulData(_configContent);
            if (data == null)//if this is null, no useful data was found
                return;

            var player = (_game_state as GameState_Witcher3).Player;

            player.Toxicity = GetInt(data, "Toxicity");
            player.Stamina = GetInt(data, "Stamina");
            player.MaximumHealth = GetInt(data, "MaxHealth");
            player.CurrentHealth = GetInt(data, "CurrHealth");
            var enumText = data.FirstOrDefault(d => d.Contains("ActiveSign"))?.Split('_')?.Last() ?? "";
            player.ActiveSign = EnumUtils.TryParseOr(enumText, true, WitcherSign.None);

            _isGameStateDirty = false;
        }
        catch
        { }
    }
}