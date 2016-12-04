using System;
using Aurora.EffectsEngine;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using Aurora.Utils;
using System.Drawing.Drawing2D;
using Aurora.Settings;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace Aurora.Profiles.RocketLeague
{
    public class GameEvent_RocketLeague : LightEvent
    {
        private bool isInitialized = false;

        //Pointers
        private RocketLeaguePointers pointers;

        private static float boost_amount = 0.0f;
        private static int team = 0;
        private static int team1_score = 0;
        private static int team2_score = 0;

        public GameEvent_RocketLeague()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = System.IO.Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "Pointers");
            watcher.Changed += RLPointers_Changed;
            watcher.EnableRaisingEvents = true;

            ReloadPointers();
        }

        private void RLPointers_Changed(object sender, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Debug.WriteLine("Name: " + e.Name + " File: " + e.FullPath + " " + e.ChangeType);

            if (e.Name.Equals("RocketLeague.json") && e.ChangeType == WatcherChangeTypes.Changed)
            {
                Debug.WriteLine("Rocket League file changed!");
                ReloadPointers();
            }
        }

        private void ReloadPointers()
        {
            string path = System.IO.Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "Pointers", "RocketLeague.json");

            if (File.Exists(path))
            {
                try
                {
                    // deserialize JSON directly from a file
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, System.Text.Encoding.Default))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        pointers = (RocketLeaguePointers)serializer.Deserialize(sr, typeof(RocketLeaguePointers));
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine(exc.Message, Logging_Level.Error);
                    isInitialized = false;
                }

                isInitialized = true;
            }
            else
            {
                isInitialized = false;
            }
        }

        public static void SetBoost(float boost_amt)
        {
            boost_amount = boost_amt;
        }

        public static void SetTeam(int team)
        {
            GameEvent_RocketLeague.team = team;
        }

        public static void SetTeam1Score(int score)
        {
            team1_score = score;
        }

        public static void SetTeam2Score(int score)
        {
            team2_score = score;
        }

        public override bool IsEnabled()
        {
            return (this.Profile.Settings as RocketLeagueSettings).isEnabled && isInitialized;
        }

        public override void UpdateLights(EffectFrame frame)
        {

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            RocketLeagueSettings settings = (RocketLeagueSettings)this.Profile.Settings;

            Process[] process_search = Process.GetProcessesByName("RocketLeague");

            if (process_search.Length != 0)
            {
                using (MemoryReader memread = new MemoryReader(process_search[0]))
                {
                    team = memread.ReadInt(pointers.Team.baseAddress, pointers.Team.pointers);

                    team1_score = memread.ReadInt(pointers.Orange_score.baseAddress, pointers.Orange_score.pointers); //Orange Team
                    team2_score = memread.ReadInt(pointers.Blue_score.baseAddress, pointers.Blue_score.pointers); //Blue Team

                    boost_amount = memread.ReadFloat(pointers.Boost_amount.baseAddress, pointers.Boost_amount.pointers);
                }
            }


            if (settings.bg_enabled)
            {
                EffectLayer bg_layer = new EffectLayer("Rocket League - Background");

                if (GameEvent_RocketLeague.team == 1 && settings.bg_use_team_color)
                {
                    bg_layer.Fill(settings.bg_team_1);
                }
                else if (GameEvent_RocketLeague.team == 0 && settings.bg_use_team_color)
                {
                    bg_layer.Fill(settings.bg_team_2);
                }
                else
                {
                    bg_layer.Fill(settings.bg_ambient_color);
                }

                if (settings.bg_show_team_score_split)
                {

                    if (team1_score != 0 || team2_score != 0)
                    {
                        int total_score = team1_score + team2_score;


                        LinearGradientBrush the__split_brush =
                    new LinearGradientBrush(
                        new Point(0, 0),
                        new Point(Effects.canvas_biggest, 0),
                        Color.Red, Color.Red);
                        Color[] colors = new Color[]
                        {
                                settings.bg_team_1, //Orange
                                settings.bg_team_1, //Orange "Line"
                                settings.bg_team_2, //Blue "Line"
                                settings.bg_team_2  //Blue
                        };
                        int num_colors = colors.Length;
                        float[] blend_positions = new float[num_colors];

                        if (team1_score > team2_score)
                        {
                            blend_positions[0] = 0.0f;
                            blend_positions[1] = ((float)team1_score / (float)total_score) - 0.01f;
                            blend_positions[2] = ((float)team1_score / (float)total_score) + 0.01f;
                            blend_positions[3] = 1.0f;
                        }
                        else if (team1_score < team2_score)
                        {
                            blend_positions[0] = 0.0f;
                            blend_positions[1] = (1.0f - ((float)team2_score / (float)total_score)) - 0.01f;
                            blend_positions[2] = (1.0f - ((float)team2_score / (float)total_score)) + 0.01f;
                            blend_positions[3] = 1.0f;
                        }
                        else
                        {
                            blend_positions[0] = 0.0f;
                            blend_positions[1] = 0.49f;
                            blend_positions[2] = 0.51f;
                            blend_positions[3] = 1.0f;
                        }

                        ColorBlend color_blend = new ColorBlend();
                        color_blend.Colors = colors;
                        color_blend.Positions = blend_positions;
                        the__split_brush.InterpolationColors = color_blend;

                        bg_layer.Fill(the__split_brush);

                    }
                }

                layers.Enqueue(bg_layer);
            }

            if (settings.boost_enabled)
            {
                EffectLayer boost_layer = new EffectLayer("Rocket League - Boost Indicator");

                double percentOccupied = boost_amount;

                ColorSpectrum boost_spec = new ColorSpectrum(settings.boost_low, settings.boost_high);
                boost_spec.SetColorAt(0.75f, settings.boost_mid);

                boost_layer.PercentEffect(boost_spec, settings.boost_sequence, percentOccupied, 1.0D, PercentEffectType.Progressive_Gradual);

                if (settings.boost_peripheral_use)
                    boost_layer.Set(Devices.DeviceKeys.Peripheral, boost_spec.GetColorAt((float)Math.Round(percentOccupied, 1)));

                layers.Enqueue(boost_layer);
            }

            //ColorZones
            EffectLayer cz_layer = new EffectLayer("Rocket League - Color Zones");
            cz_layer.DrawColorZones(settings.lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            //Scripts
            this.Profile.UpdateEffectScripts(layers);

            foreach (var layer in this.Profile.Settings.Layers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            frame.AddLayers(layers.ToArray());
        }

        public override void UpdateLights(EffectFrame frame, IGameState new_game_state)
        {
            UpdateLights(frame);
        }
    }
}
