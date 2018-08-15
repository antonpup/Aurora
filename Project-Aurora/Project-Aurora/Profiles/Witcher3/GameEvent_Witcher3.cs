using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.EffectsEngine;
using System.Diagnostics;
using Aurora.Utils;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Aurora.Profiles.Witcher3.GSI;
using System.IO;
using System.Text.RegularExpressions;
using Aurora.Profiles.Witcher3.GSI.Nodes;

namespace Aurora.Profiles.Witcher3
{
    public class GameEvent_Witcher3 : LightEvent
    {
        private bool isInitialized = false;
        private readonly Regex _configRegex;
        private string configContent;
        string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\The Witcher 3";
        string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\The Witcher 3", "user.settings");
        //Most of this code, and the mod was taken from https://github.com/SpoinkyNL/Artemis/, with Spoinky's permission
        public GameEvent_Witcher3() : base()
        {
            _configRegex = new Regex("\\[Artemis\\](.+?)\\[", RegexOptions.Singleline);

            if (Directory.Exists(dataFolder))
            {
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = dataFolder;
                watcher.Changed += dataFile_Changed;
                watcher.EnableRaisingEvents = true;

                ReloadData();
            }
        }

        private void dataFile_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Equals("user.settings") && e.ChangeType == WatcherChangeTypes.Changed)
                ReloadData();
        }

        private void ReloadData()
        {
            if (File.Exists(dataPath))
            {
                var reader = new StreamReader(File.Open(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                configContent = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                isInitialized = true;
            }
            else
            {
                isInitialized = false;
            }
        }

        public override void ResetGameState()
        {
            _game_state = new GameState_Witcher3();
        }

        public new bool IsEnabled
        {
            get { return this.Application.Settings.IsEnabled && isInitialized; }
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            if (File.Exists(dataPath))
            {
                if(configContent != null)
                {
                    var signRes = _configRegex.Match(configContent);
                    var parts = signRes.Value.Split('\n').Skip(1).Select(v => v.Replace("\r", "")).ToList();
                    if (parts.Count != 0)
                    {
                        parts.RemoveAt(parts.Count - 1);

                        // Update sign
                        var sign = parts.FirstOrDefault(p => p.Contains("ActiveSign="));
                        if (sign != null)
                        {
                            var signString = sign.Split('=')[1];
                            switch (signString)
                            {
                                case "ST_Aard":
                                    (_game_state as GameState_Witcher3).Player.ActiveSign = WitcherSign.Aard;
                                    break;
                                case "ST_Yrden":
                                    (_game_state as GameState_Witcher3).Player.ActiveSign = WitcherSign.Yrden;
                                    break;
                                case "ST_Igni":
                                    (_game_state as GameState_Witcher3).Player.ActiveSign = WitcherSign.Igni;
                                    break;
                                case "ST_Quen":
                                    (_game_state as GameState_Witcher3).Player.ActiveSign = WitcherSign.Quen;
                                    break;
                                case "ST_Axii":
                                    (_game_state as GameState_Witcher3).Player.ActiveSign = WitcherSign.Axii;
                                    break;
                            }
                        }

                        // Update max health
                        var maxHealth = parts.FirstOrDefault(p => p.Contains("MaxHealth="));
                        if (maxHealth != null)
                        {
                            var maxHealthInt = int.Parse(maxHealth.Split('=')[1].Split('.')[0]);
                            (_game_state as GameState_Witcher3).Player.MaximumHealth = maxHealthInt;
                        }
                        // Update health
                        var health = parts.FirstOrDefault(p => p.Contains("CurrHealth="));
                        if (health != null)
                        {
                            var healthInt = int.Parse(health.Split('=')[1].Split('.')[0]);
                            (_game_state as GameState_Witcher3).Player.CurrentHealth = healthInt;
                        }
                        // Update stamina
                        var stamina = parts.FirstOrDefault(p => p.Contains("Stamina="));
                        if (stamina != null)
                        {
                            var staminaInt = int.Parse(stamina.Split('=')[1].Split('.')[0]);
                            (_game_state as GameState_Witcher3).Player.Stamina = staminaInt;
                        }
                        // Update Toxicity
                        var toxicity = parts.FirstOrDefault(p => p.Contains("Toxicity="));
                        if (toxicity != null)
                        {
                            var toxicityInt = int.Parse(toxicity.Split('=')[1].Split('.')[0]);
                            (_game_state as GameState_Witcher3).Player.Toxicity = toxicityInt;
                        }
                    }                   
                }
            }
            //Artemis code


            foreach (var layer in this.Application.Profile.Layers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            //Scripts
            this.Application.UpdateEffectScripts(layers);

            frame.AddLayers(layers.ToArray());
        }
    }
}
