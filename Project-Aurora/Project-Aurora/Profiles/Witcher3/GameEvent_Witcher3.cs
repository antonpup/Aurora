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
        private string configContent;
        private static readonly Regex _configRegex = new Regex("\\[Artemis\\](.+?)\\[", RegexOptions.Singleline);
        private static readonly string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\The Witcher 3";
        private static readonly string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\The Witcher 3", "user.settings");
        //Most of this code, and the mod was taken from https://github.com/SpoinkyNL/Artemis/, with Spoinky's permission
        public GameEvent_Witcher3() : base()
        {
            if (Directory.Exists(dataFolder))
            {
                FileSystemWatcher watcher = new FileSystemWatcher()
                {
                    Path = dataFolder,
                    EnableRaisingEvents = true
                };
                watcher.Changed += dataFile_Changed;
            }

            ReadConfigFile();
        }

        private void dataFile_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Equals("user.settings") && e.ChangeType == WatcherChangeTypes.Changed)
                ReadConfigFile();
        }

        private void ReadConfigFile()
        {
            if (File.Exists(dataPath))
            {
                try
                {
                    using (var reader = new StreamReader(File.Open(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        configContent = reader.ReadToEnd();
                    }
                }
                catch (IOException){ }//ignore read exception
            }
        }

        public override void ResetGameState()
        {
            _game_state = new GameState_Witcher3();
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            if (File.Exists(dataPath))
            {
                if (configContent != null)
                {
                    try
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
                                var signString = sign.Split('=')[1].Replace("ST_","");
                                if (Enum.TryParse(signString, out WitcherSign temp))
                                    (_game_state as GameState_Witcher3).Player.ActiveSign = temp;
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
                    catch (Exception e)
                    {
                        Global.logger.Warn("Error parsing Witcher 3 config data:" + e.Message);
                    }
                }
            }
            //Artemis code


            foreach (var layer in this.Application.Profile.Layers.Reverse().ToArray())
            {
                if (layer.Enabled)
                    layers.Enqueue(layer.Render(_game_state));
            }

            //Scripts
            this.Application.UpdateEffectScripts(layers);

            frame.AddLayers(layers.ToArray());
        }
    }
}
