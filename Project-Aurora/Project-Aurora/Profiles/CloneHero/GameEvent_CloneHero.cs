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
using Aurora.Profiles.CloneHero.GSI;
using Aurora.Profiles.CloneHero.GSI.Nodes;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

/*
 * Clone Hero support added by @Joey4305
 */

namespace Aurora.Profiles.CloneHero
{
    public class GameEvent_CloneHero : LightEvent
    {
        private bool isInitialized = false;

        //Pointers
        private CloneHeroPointers pointers;

        public GameEvent_CloneHero() : base()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers");
            watcher.Changed += CHPointers_Changed;
            watcher.EnableRaisingEvents = true;

            ReloadPointers();
        }

        private void CHPointers_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Equals("CloneHero.json") && e.ChangeType == WatcherChangeTypes.Changed)
                ReloadPointers();
        }

        private void ReloadPointers()
        {
            string path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers", "CloneHero.json");
            
            if (File.Exists(path))
            {
                try
                {
                    // deserialize JSON directly from a file
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, System.Text.Encoding.Default))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        pointers = (CloneHeroPointers)serializer.Deserialize(sr, typeof(CloneHeroPointers));
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error(exc.Message);
                    isInitialized = false;
                }

                isInitialized = true;
            }
            else
            {
                isInitialized = false;
            }
        }

        public override void ResetGameState()
        {
            _game_state = new GameState_CloneHero();
        }

        public new bool IsEnabled
        {
            get { return this.Application.Settings.IsEnabled && isInitialized; }
        }

        public override void UpdateLights(EffectFrame frame)
        {

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            CloneHeroProfile settings = (CloneHeroProfile)this.Application.Profile;

            Process[] process_search = Process.GetProcessesByName("Clone Hero");
            if (process_search.Length != 0)
            {

                try
                {
                    using (MemoryReader memread = new MemoryReader(process_search[0], "mono-2.0-bdwgc.dll", true))
                    {
                        (_game_state as GameState_CloneHero).Player.NoteStreak = memread.ReadInt(pointers.NoteStreak.baseAddress, pointers.NoteStreak.pointers);

                        #region NoteStreak Extras

                        // Breaks up the note streak into the 1x, 2x, 3x, 4x zones for easy lighting options
                        int streak = (_game_state as GameState_CloneHero).Player.NoteStreak;
                        if (streak >= 0 && streak <= 10)
                        {
                            (_game_state as GameState_CloneHero).Player.NoteStreak1x = streak;
                            (_game_state as GameState_CloneHero).Player.NoteStreak2x = 0;
                            (_game_state as GameState_CloneHero).Player.NoteStreak3x = 0;
                            (_game_state as GameState_CloneHero).Player.NoteStreak4x = 0;

                            // This accounts for how CH changes the color once the bar fills up
                            if (streak == 10)
                            {
                                (_game_state as GameState_CloneHero).Player.NoteStreak2x = 10;
                            }
                        }
                        else if (streak > 10 && streak <= 20)
                        {
                            (_game_state as GameState_CloneHero).Player.NoteStreak1x = 0;
                            (_game_state as GameState_CloneHero).Player.NoteStreak2x = streak - 10;
                            (_game_state as GameState_CloneHero).Player.NoteStreak3x = 0;
                            (_game_state as GameState_CloneHero).Player.NoteStreak4x = 0;

                            // This accounts for how CH changes the color once the bar fills up
                            if (streak == 20)
                            {
                                (_game_state as GameState_CloneHero).Player.NoteStreak3x = 10;
                            }
                        }
                        else if (streak > 20 && streak <= 30)
                        {
                            (_game_state as GameState_CloneHero).Player.NoteStreak1x = 0;
                            (_game_state as GameState_CloneHero).Player.NoteStreak2x = 0;
                            (_game_state as GameState_CloneHero).Player.NoteStreak3x = streak - 20;
                            (_game_state as GameState_CloneHero).Player.NoteStreak4x = 0;

                            // This accounts for how CH changes the color once the bar fills up
                            if (streak == 30)
                            {
                                (_game_state as GameState_CloneHero).Player.NoteStreak4x = 10;
                            }
                        }
                        else if (streak > 30 && streak <= 40)
                        {
                            (_game_state as GameState_CloneHero).Player.NoteStreak1x = 0;
                            (_game_state as GameState_CloneHero).Player.NoteStreak2x = 0;
                            (_game_state as GameState_CloneHero).Player.NoteStreak3x = 0;
                            (_game_state as GameState_CloneHero).Player.NoteStreak4x = streak - 30;
                        }
                        #endregion

                        (_game_state as GameState_CloneHero).Player.IsStarPowerActive = memread.ReadInt(pointers.IsStarPowerActive.baseAddress, pointers.IsStarPowerActive.pointers) == 1 ? true : false;

                        (_game_state as GameState_CloneHero).Player.StarPowerPercent = memread.ReadFloat(pointers.StarPowerPercent.baseAddress, pointers.StarPowerPercent.pointers) * 100;

                        (_game_state as GameState_CloneHero).Player.IsAtMenu = memread.ReadInt(pointers.IsAtMenu.baseAddress, pointers.IsAtMenu.pointers) == 1 ? true : false;

                        (_game_state as GameState_CloneHero).Player.NotesTotal = memread.ReadInt(pointers.NotesTotal.baseAddress, pointers.NotesTotal.pointers);

                        #region FC Indicator

                        // Resets at the beginning of a new song
                        if ((_game_state as GameState_CloneHero).Player.NoteStreak == 0 && (_game_state as GameState_CloneHero).Player.NotesTotal == 0)
                        {
                            (_game_state as GameState_CloneHero).Player.IsFC = true;
                        }

                        // Doing this check is necessary to prevent a weird bug
                        if ((_game_state as GameState_CloneHero).Player.IsFC)
                        {
                            if ((_game_state as GameState_CloneHero).Player.NoteStreak < (_game_state as GameState_CloneHero).Player.NotesTotal)
                            {
                                (_game_state as GameState_CloneHero).Player.IsFC = false;
                            }
                            else
                            {
                                (_game_state as GameState_CloneHero).Player.IsFC = true;
                            }
                        }

                        #endregion

                        (_game_state as GameState_CloneHero).Player.SoloPercent = memread.ReadInt(pointers.SoloPercent.baseAddress, pointers.SoloPercent.pointers);

                        (_game_state as GameState_CloneHero).Player.Score = memread.ReadInt(pointers.Score.baseAddress, pointers.Score.pointers);

                        (_game_state as GameState_CloneHero).Player.IsGreenPressed = memread.ReadInt(pointers.IsGreenPressed.baseAddress, pointers.IsGreenPressed.pointers) > 0 ? true : false;
                        (_game_state as GameState_CloneHero).Player.IsRedPressed = memread.ReadInt(pointers.IsRedPressed.baseAddress, pointers.IsRedPressed.pointers) > 0 ? true : false;
                        (_game_state as GameState_CloneHero).Player.IsYellowPressed = memread.ReadInt(pointers.IsYellowPressed.baseAddress, pointers.IsYellowPressed.pointers) > 0 ? true : false;
                        (_game_state as GameState_CloneHero).Player.IsBluePressed = memread.ReadInt(pointers.IsBluePressed.baseAddress, pointers.IsBluePressed.pointers) > 0 ? true : false;
                        (_game_state as GameState_CloneHero).Player.IsOrangePressed = memread.ReadInt(pointers.IsOrangePressed.baseAddress, pointers.IsOrangePressed.pointers) > 0 ? true : false;
                        Console.WriteLine(memread.ReadInt(pointers.IsGreenPressed.baseAddress, pointers.IsGreenPressed.pointers));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            foreach (var layer in this.Application.Profile.Layers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            //Scripts
            this.Application.UpdateEffectScripts(layers);

            frame.AddLayers(layers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state)
        {
            //UpdateLights(frame);
        }
    }
}
