using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace Aurora.Profiles.Desktop
{
    static class PerformanceInfo
    {
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        public struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        public static Int64 GetPhysicalAvailableMemoryInMiB()
        {
            ulong availableMemory = new ComputerInfo().AvailablePhysicalMemory;
            return Convert.ToInt64(availableMemory / 1048576);
        }

        public static Int64 GetTotalMemoryInMiB()
        {
            ulong availableMemory = new ComputerInfo().TotalPhysicalMemory;
            return Convert.ToInt64(availableMemory / 1048576);

        }
    }

    public class Event_Desktop : LightEvent
    {
        private long internalcounter;

        public Event_Desktop()
        {
            internalcounter = 0;

            _game_state = new GameState();
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            float time = (float)Math.Pow(Math.Sin(1.0D * (internalcounter++ / 10.0f)), 2.0d);

            foreach(var layer in (Global.Configuration.desktop_profile.Settings as DesktopSettings).Layers.Reverse().ToArray())
            {
                if(layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Handler.Render(_game_state));
            }

            EffectLayer cz_layer = new EffectLayer("Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.desktop_profile.Settings as DesktopSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            //Scripts before interactive and shortcut assistant layers
            Global.Configuration.desktop_profile.UpdateEffectScripts(layers);

            if (Global.Configuration.time_based_dimming_enabled)
            {
                if (
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_end_hour)
                    )
                {
                    layers.Clear();

                    EffectLayer time_based_dim_layer = new EffectLayer("Time Based Dim");
                    time_based_dim_layer.Fill(Color.Black);

                    layers.Enqueue(time_based_dim_layer);
                }
            }

            EffectLayer sc_assistant_layer = new EffectLayer("Shortcut Assistant");
            if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).shortcuts_assistant_enabled)
            {
                if (Global.held_modified == Keys.LControlKey || Global.held_modified == Keys.RControlKey)
                {
                    if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).shortcuts_assistant_bim_bg)
                        sc_assistant_layer.Fill(Color.FromArgb(169, 0, 0, 0));

                    if (Global.held_modified == Keys.LControlKey)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_CONTROL, (Global.Configuration.desktop_profile.Settings as DesktopSettings).ctrl_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_CONTROL, (Global.Configuration.desktop_profile.Settings as DesktopSettings).ctrl_key_color);
                    sc_assistant_layer.Set((Global.Configuration.desktop_profile.Settings as DesktopSettings).ctrl_key_sequence, (Global.Configuration.desktop_profile.Settings as DesktopSettings).ctrl_key_color);
                }
                else if (Global.held_modified == Keys.LMenu || Global.held_modified == Keys.RMenu)
                {
                    if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).shortcuts_assistant_bim_bg)
                        sc_assistant_layer.Fill(Color.FromArgb(169, 0, 0, 0));

                    if (Global.held_modified == Keys.LMenu)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_ALT, (Global.Configuration.desktop_profile.Settings as DesktopSettings).alt_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_ALT, (Global.Configuration.desktop_profile.Settings as DesktopSettings).alt_key_color);
                    sc_assistant_layer.Set((Global.Configuration.desktop_profile.Settings as DesktopSettings).alt_key_sequence, (Global.Configuration.desktop_profile.Settings as DesktopSettings).alt_key_color);
                }
                else if (Global.held_modified == Keys.LWin || Global.held_modified == Keys.RWin)
                {
                    if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).shortcuts_assistant_bim_bg)
                        sc_assistant_layer.Fill(Color.FromArgb(169, 0, 0, 0));

                    if (Global.held_modified == Keys.LWin)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_WINDOWS, (Global.Configuration.desktop_profile.Settings as DesktopSettings).win_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_WINDOWS, (Global.Configuration.desktop_profile.Settings as DesktopSettings).win_key_color);
                    sc_assistant_layer.Set((Global.Configuration.desktop_profile.Settings as DesktopSettings).win_key_sequence, (Global.Configuration.desktop_profile.Settings as DesktopSettings).win_key_color);
                }

            }
            layers.Enqueue(sc_assistant_layer);

            frame.AddLayers(layers.ToArray());
        }

        public override void UpdateLights(EffectFrame frame, IGameState new_game_state)
        {
            //No need to do anything... This doesn't have any gamestates.
            UpdateLights(frame);
        }

        public override bool IsEnabled()
        {
            return true;
        }
    }
}
