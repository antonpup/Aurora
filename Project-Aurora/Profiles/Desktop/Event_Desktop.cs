using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Functions;
using Aurora.Settings;
using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }

        }

        public static Int64 GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                return -1;
            }

        }
    }

    public class input_item
    {
        public Devices.DeviceKeys key;
        public float progress;
        public ColorSpectrum spectrum;

        public input_item(Devices.DeviceKeys key, float progress, ColorSpectrum spectrum)
        {
            this.key = key;
            this.progress = progress;
            this.spectrum = spectrum;
        }
    }

    public class Event_Desktop : GameEvent
    {
        private List<input_item> input_list = new List<input_item>();
        private Keys previous_key = Keys.None;

        private long internalcounter;

        private float previousCPUValue;
        private float currentCPUValue;
        private float transitionalCPUValue;

        //RAM
        private long memory_Available = 0L;
        private long memory_Total = 0L;

        private System.Timers.Timer gatherInfo;

        private long previoustime = 0;
        private long currenttime = 0;

        private float getDeltaTime()
        {
            return (currenttime - previoustime) / 1000.0f;
        }

        private void getCPUCounter()
        {
            while (true)
            {
                try
                {
                    PerformanceCounter cpuCounter = new PerformanceCounter();
                    cpuCounter.CategoryName = "Processor";
                    cpuCounter.CounterName = "% Processor Time";
                    cpuCounter.InstanceName = "_Total";

                    // will always start at 0
                    float firstValue = cpuCounter.NextValue();
                    Thread.Sleep(1000);
                    // now matches task manager reading
                    float secondValue = cpuCounter.NextValue();

                    previousCPUValue = currentCPUValue;
                    currentCPUValue = secondValue;

                    //return secondValue;
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine("PerformanceCounter exception: " + exc, Logging_Level.Error);
                }
            }
        }

        public Event_Desktop()
        {
            Thread workerThread = new Thread(getCPUCounter);
            workerThread.Start();

            internalcounter = 0;
            previousCPUValue = 0.0f;
            currentCPUValue = 0.0f;
            transitionalCPUValue = 0.0f;

            gatherInfo = new System.Timers.Timer(500);
            gatherInfo.Elapsed += GatherInfo_Elapsed;
            gatherInfo.Start();

            Global.input_hook.MouseClick += GlobalHookMouseClick;
            Global.input_hook.KeyDown += GlobalHookKeyDown;
            Global.input_hook.KeyUp += GlobalHookKeyUp;
        }

        private void GlobalHookMouseClick(object sender, MouseEventArgs e)
        {
            if (Global.isLoaded)
            {
                if (!(Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effects_mouse_clicking)
                    return;

                Devices.DeviceKeys device_key = Devices.DeviceKeys.Peripheral;

                if (device_key != Devices.DeviceKeys.NONE && (Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effects_enabled)
                {
                    EffectPoint pt = Effects.GetBitmappingFromDeviceKey(device_key).GetCenter();

                    if (pt != new EffectPoint(0, 0))
                    {
                        //Debug.WriteLine("Created circle at {0}", pt);

                        Color primary_c = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_primary_color;
                        Color secondary_c = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_secondary_color;

                        if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effects_random_primary_color)
                            primary_c = Utils.ColorUtils.GenerateRandomColor(primary_c);

                        if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effects_random_secondary_color)
                            secondary_c = Utils.ColorUtils.GenerateRandomColor(secondary_c);

                        ColorSpectrum spec = new ColorSpectrum(primary_c, secondary_c);
                        if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_type == InteractiveEffects.KeyPress)
                        {
                            spec = new ColorSpectrum(primary_c, Color.FromArgb(0, secondary_c));
                            spec.SetColorAt(0.80f, secondary_c);
                        }

                        input_list.Add(new input_item(device_key, 0.0f, spec));
                    }
                }
            }
        }

        private void GatherInfo_Elapsed(object sender, ElapsedEventArgs e)
        {
            memory_Available = PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
            memory_Total = PerformanceInfo.GetTotalMemoryInMiB();
        }

        private void GlobalHookKeyUp(object sender, KeyEventArgs e)
        {
            if (Global.isLoaded)
            {
                if (Utils.Time.GetMillisecondsSinceEpoch() - previoustime > 1000L)
                    return; //This event wasn't used for at least 1 second

                if (previous_key == e.KeyCode)
                    previous_key = Keys.None;
            }
        }

        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            if (Global.isLoaded)
            {
                if (Utils.Time.GetMillisecondsSinceEpoch() - previoustime > 1000L)
                    return; //This event wasn't used for at least 1 second

                if (previous_key == e.KeyCode)
                    return;

                Devices.DeviceKeys device_key = Utils.KeyUtils.GetDeviceKey(e.KeyCode);

                if (device_key != Devices.DeviceKeys.NONE && (Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effects_enabled)
                {
                    EffectPoint pt = Effects.GetBitmappingFromDeviceKey(device_key).GetCenter();

                    if (pt != new EffectPoint(0, 0))
                    {
                        Color primary_c = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_primary_color;
                        Color secondary_c = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_secondary_color;

                        if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effects_random_primary_color)
                            primary_c = Utils.ColorUtils.GenerateRandomColor(primary_c);

                        if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effects_random_secondary_color)
                            secondary_c = Utils.ColorUtils.GenerateRandomColor(secondary_c);

                        ColorSpectrum spec = new ColorSpectrum(primary_c, secondary_c);
                        if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_type == InteractiveEffects.KeyPress)
                        {
                            spec = new ColorSpectrum(primary_c, Color.FromArgb(0, secondary_c));
                            spec.SetColorAt(0.80f, secondary_c);
                        }

                        input_list.Add(new input_item(device_key, 0.0f, spec));
                        previous_key = e.KeyCode;
                    }
                }
            }
        }

        public void UpdateLights(EffectFrame frame)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            float time = (float)Math.Pow(Math.Sin(1.0D * (internalcounter++ / 10.0f)), 2.0d);

            EffectLayer cz_layer = new EffectLayer("Color Zones");
            cz_layer.DrawColorZones((Global.Configuration.dekstop_profile.Settings as DesktopSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).cpu_usage_enabled)
            {
                EffectLayer cpu = new EffectLayer("CPU");

                if (currentCPUValue - previousCPUValue > 0.0f && transitionalCPUValue < currentCPUValue)
                {
                    transitionalCPUValue += (currentCPUValue - previousCPUValue) / 100.0f;
                }
                else if (currentCPUValue - previousCPUValue < 0.0f && transitionalCPUValue > currentCPUValue)
                {
                    transitionalCPUValue -= (previousCPUValue - currentCPUValue) / 100.0f;
                }
                else if (currentCPUValue - previousCPUValue == 0.0f && transitionalCPUValue != currentCPUValue)
                {
                    transitionalCPUValue = currentCPUValue;
                }

                Color cpu_used = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).cpu_used_color;
                Color cpu_free = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).cpu_free_color;

                if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).cpu_free_color_transparent)
                {
                    cpu_free = Color.FromArgb(0, cpu_used);
                }

                cpu.PercentEffect(cpu_used, cpu_free, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).cpu_sequence, transitionalCPUValue, 100.0f, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).cpu_usage_effect_type);

                layers.Enqueue(cpu);
            }

            if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).ram_usage_enabled)
            {
                EffectLayer memory = new EffectLayer("Memory");

                double percentFree = ((double)memory_Available / (double)memory_Total);
                double percentOccupied = 1.0D - percentFree;

                Color ram_used = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).ram_used_color;
                Color ram_free = (Global.Configuration.dekstop_profile.Settings as DesktopSettings).ram_free_color;

                if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).ram_free_color_transparent)
                {
                    ram_free = Color.FromArgb(0, ram_used);
                }

                memory.PercentEffect(ram_used, ram_free, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).ram_sequence, percentOccupied, 1.0D, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).ram_usage_effect_type);

                layers.Enqueue(memory);
            }

            EffectLayer interactive_layer = new EffectLayer("Interactive Effects");

            if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_type == InteractiveEffects.Wave)
            {
                using (Graphics g = interactive_layer.GetGraphics())
                {
                    foreach (var input in input_list.ToArray())
                    {
                        EffectPoint pt = Effects.GetBitmappingFromDeviceKey(input.key).GetCenter();

                        float transition_value = input.progress / Effects.canvas_width;

                        g.DrawEllipse(new Pen(input.spectrum.GetColorAt(transition_value), (Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_width),
                            pt.X - input.progress,
                            pt.Y - input.progress,
                            2 * input.progress,
                            2 * input.progress);
                    }
                }
            }
            else if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_type == InteractiveEffects.KeyPress)
            {
                foreach (var input in input_list.ToArray())
                {
                    float transition_value = input.progress / Effects.canvas_width;

                    if (transition_value > 1.0f)
                        continue;

                    Color color = input.spectrum.GetColorAt(transition_value);

                    interactive_layer.Set(input.key, color);
                }
            }

            for (int x = input_list.Count - 1; x >= 0; x--)
            {
                if (input_list[x].progress > Effects.canvas_width)
                    input_list.RemoveAt(x);
                else
                {
                    float trans_added = ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).interactive_effect_speed * (getDeltaTime() * 5.0f));
                    input_list[x].progress += trans_added;
                }
            }

            layers.Enqueue(interactive_layer);

            if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_enabled)
            {
                if (
                    Utils.Time.IsCurrentTimeBetween((Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_start_hour, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).time_based_dimming_end_hour)
                    )
                {
                    layers.Clear();

                    EffectLayer time_based_dim_layer = new EffectLayer("Time Based Dim");
                    time_based_dim_layer.Fill(Color.Black);

                    layers.Enqueue(time_based_dim_layer);
                }
            }

            EffectLayer sc_assistant_layer = new EffectLayer("Shortcut Assistant");
            if ((Global.Configuration.dekstop_profile.Settings as DesktopSettings).shortcuts_assistant_enabled)
            {
                if (Global.held_modified == Keys.LControlKey || Global.held_modified == Keys.RControlKey)
                {
                    if (Global.held_modified == Keys.LControlKey)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_CONTROL, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).ctrl_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_CONTROL, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).ctrl_key_color);
                    sc_assistant_layer.Set((Global.Configuration.dekstop_profile.Settings as DesktopSettings).ctrl_key_sequence, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).ctrl_key_color);
                }
                else if (Global.held_modified == Keys.LMenu || Global.held_modified == Keys.RMenu)
                {
                    if (Global.held_modified == Keys.LMenu)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_ALT, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).alt_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_ALT, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).alt_key_color);
                    sc_assistant_layer.Set((Global.Configuration.dekstop_profile.Settings as DesktopSettings).alt_key_sequence, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).alt_key_color);
                }
                else if (Global.held_modified == Keys.LWin || Global.held_modified == Keys.RWin)
                {
                    if (Global.held_modified == Keys.LWin)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_WINDOWS, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).win_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_WINDOWS, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).win_key_color);
                    sc_assistant_layer.Set((Global.Configuration.dekstop_profile.Settings as DesktopSettings).win_key_sequence, (Global.Configuration.dekstop_profile.Settings as DesktopSettings).win_key_color);
                }

            }
            layers.Enqueue(sc_assistant_layer);

            frame.SetLayers(layers.ToArray());
        }

        public void UpdateLights(EffectFrame frame, GameState new_game_state)
        {
            //No need to do anything... This doesn't have any gamestates.
            UpdateLights(frame);
        }

        public bool IsEnabled()
        {
            return true;
        }
    }
}
