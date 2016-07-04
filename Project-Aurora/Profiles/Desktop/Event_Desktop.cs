using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.EffectsEngine.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        public enum input_type
        {
            AnimationMix,
            Spectrum
        };

        public Devices.DeviceKeys key;
        public float progress;
        public AnimationMix animation;
        public ColorSpectrum spectrum;
        public readonly input_type type;

        public input_item(Devices.DeviceKeys key, float progress, AnimationMix animation)
        {
            this.key = key;
            this.progress = progress;
            this.animation = animation;

            type = input_type.AnimationMix;
        }

        public input_item(Devices.DeviceKeys key, float progress, ColorSpectrum spectrum)
        {
            this.key = key;
            this.progress = progress;
            this.spectrum = spectrum;

            type = input_type.Spectrum;
        }

    }

    public class Event_Desktop : GameEvent
    {
        private List<input_item> input_list = new List<input_item>();
        private Keys previous_key = Keys.None;

        private Random randomizer = new Random();

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

            Global.input_subscriptions.MouseClick += GlobalHookMouseClick;
            Global.input_subscriptions.KeyDown += GlobalHookKeyDown;
            Global.input_subscriptions.KeyUp += GlobalHookKeyUp;
        }

        private input_item CreateInputItem(Devices.DeviceKeys key, EffectPoint origin)
        {
            Color primary_c = (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_primary_color;
            Color secondary_c = (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_secondary_color;

            if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effects_random_primary_color)
                primary_c = Utils.ColorUtils.GenerateRandomColor(primary_c);

            if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effects_random_secondary_color)
                secondary_c = Utils.ColorUtils.GenerateRandomColor(secondary_c);

            AnimationMix anim_mix = new AnimationMix();

            if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_type == InteractiveEffects.Wave)
            {
                AnimationTrack wave = new AnimationTrack("Wave effect", 1.0f);
                wave.SetFrame(0.0f,
                    new AnimationCircle(origin.ToPointF(), 0, primary_c, (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                    );
                wave.SetFrame(0.80f,
                    new AnimationCircle(origin.ToPointF(), Effects.canvas_width * 0.80f, secondary_c, (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                    );
                wave.SetFrame(1.00f,
                    new AnimationCircle(origin.ToPointF(), Effects.canvas_width, Color.FromArgb(0, secondary_c), (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                    );
                anim_mix.AddTrack(wave);
            }
            else if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_type == InteractiveEffects.Wave_Filled)
            {
                AnimationTrack wave = new AnimationTrack("Filled Wave effect", 1.0f);
                wave.SetFrame(0.0f,
                    new AnimationFilledCircle(origin.ToPointF(), 0, primary_c, (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                    );
                wave.SetFrame(0.80f,
                    new AnimationFilledCircle(origin.ToPointF(), Effects.canvas_width * 0.80f, secondary_c, (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                    );
                wave.SetFrame(1.00f,
                    new AnimationFilledCircle(origin.ToPointF(), Effects.canvas_width, Color.FromArgb(0, secondary_c), (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                    );
                anim_mix.AddTrack(wave);
            }
            else if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_type == InteractiveEffects.KeyPress)
            {
                ColorSpectrum spec = new ColorSpectrum(primary_c, secondary_c);
                if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_type == InteractiveEffects.KeyPress)
                {
                    spec = new ColorSpectrum(primary_c, Color.FromArgb(0, secondary_c));
                    spec.SetColorAt(0.80f, secondary_c);
                }

                return new input_item(key, 0.0f, spec);
            }
            else if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_type == InteractiveEffects.ArrowFlow)
            {
                PointF starting_pt = origin.ToPointF();

                AnimationTrack arrow = new AnimationTrack("Arrow Flow effect", 1.0f);
                arrow.SetFrame(0.0f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(starting_pt, starting_pt, primary_c, (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width),
                            new AnimationLine(starting_pt, starting_pt, primary_c, (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                        }
                        )
                    );
                arrow.SetFrame(0.33f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(starting_pt, new PointF(starting_pt.X + Effects.canvas_width * 0.33f, starting_pt.Y), Utils.ColorUtils.BlendColors(primary_c, secondary_c, 0.33D), (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width),
                            new AnimationLine(starting_pt, new PointF(starting_pt.X - Effects.canvas_width * 0.33f, starting_pt.Y), Utils.ColorUtils.BlendColors(primary_c, secondary_c, 0.33D), (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                        }
                        )
                    );
                arrow.SetFrame(0.66f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(new PointF(starting_pt.X + Effects.canvas_width * 0.33f, starting_pt.Y), new PointF(starting_pt.X + Effects.canvas_width * 0.66f, starting_pt.Y), secondary_c, (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width),
                            new AnimationLine(new PointF(starting_pt.X - Effects.canvas_width * 0.33f, starting_pt.Y), new PointF(starting_pt.X - Effects.canvas_width * 0.66f, starting_pt.Y), secondary_c, (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                        }
                        )
                    );
                arrow.SetFrame(1.0f,
                    new AnimationLines(
                        new AnimationLine[] {
                            new AnimationLine(new PointF(starting_pt.X + Effects.canvas_width * 0.66f, starting_pt.Y), new PointF(starting_pt.X + Effects.canvas_width, starting_pt.Y), Color.FromArgb(0, secondary_c), (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width),
                            new AnimationLine(new PointF(starting_pt.X - Effects.canvas_width * 0.66f, starting_pt.Y), new PointF(starting_pt.X - Effects.canvas_width, starting_pt.Y), Color.FromArgb(0, secondary_c), (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_width)
                        }
                        )
                    );
                anim_mix.AddTrack(arrow);
            }

            return new input_item(key, 0.0f, anim_mix);
        }

        private void GlobalHookMouseClick(object sender, MouseEventArgs e)
        {
            if (!(Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effects_mouse_clicking)
                return;

            Devices.DeviceKeys device_key = Devices.DeviceKeys.Peripheral;

            if (device_key != Devices.DeviceKeys.NONE && (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effects_enabled)
            {
                EffectPoint pt = Effects.GetBitmappingFromDeviceKey(device_key).GetCenter();
                if (pt != new EffectPoint(0, 0))
                {
                    input_list.Add(CreateInputItem(device_key, pt));
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
            if (Utils.Time.GetMillisecondsSinceEpoch() - previoustime > 1000L)
                return; //This event wasn't used for at least 1 second

            if (previous_key == e.KeyCode)
                previous_key = Keys.None;
        }

        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            if (Utils.Time.GetMillisecondsSinceEpoch() - previoustime > 1000L)
                return; //This event wasn't used for at least 1 second

            if (previous_key == e.KeyCode)
                return;

            Devices.DeviceKeys device_key = Utils.KeyUtils.GetDeviceKey(e.KeyCode);

            if (device_key != Devices.DeviceKeys.NONE && (Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effects_enabled)
            {
                EffectPoint pt = Effects.GetBitmappingFromDeviceKey(device_key).GetCenter();
                if (pt != new EffectPoint(0, 0))
                {
                    input_list.Add(CreateInputItem(device_key, pt));
                    previous_key = e.KeyCode;
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
            cz_layer.DrawColorZones((Global.Configuration.desktop_profile.Settings as DesktopSettings).lighting_areas.ToArray());
            layers.Enqueue(cz_layer);

            if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).cpu_usage_enabled)
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

                Color cpu_used = (Global.Configuration.desktop_profile.Settings as DesktopSettings).cpu_used_color;
                Color cpu_free = (Global.Configuration.desktop_profile.Settings as DesktopSettings).cpu_free_color;

                if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).cpu_free_color_transparent)
                {
                    cpu_free = Color.FromArgb(0, cpu_used);
                }

                cpu.PercentEffect(cpu_used, cpu_free, (Global.Configuration.desktop_profile.Settings as DesktopSettings).cpu_sequence, transitionalCPUValue, 100.0f, (Global.Configuration.desktop_profile.Settings as DesktopSettings).cpu_usage_effect_type);

                layers.Enqueue(cpu);
            }

            if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).ram_usage_enabled)
            {
                EffectLayer memory = new EffectLayer("Memory");

                double percentFree = ((double)memory_Available / (double)memory_Total);
                double percentOccupied = 1.0D - percentFree;

                Color ram_used = (Global.Configuration.desktop_profile.Settings as DesktopSettings).ram_used_color;
                Color ram_free = (Global.Configuration.desktop_profile.Settings as DesktopSettings).ram_free_color;

                if ((Global.Configuration.desktop_profile.Settings as DesktopSettings).ram_free_color_transparent)
                {
                    ram_free = Color.FromArgb(0, ram_used);
                }

                memory.PercentEffect(ram_used, ram_free, (Global.Configuration.desktop_profile.Settings as DesktopSettings).ram_sequence, percentOccupied, 1.0D, (Global.Configuration.desktop_profile.Settings as DesktopSettings).ram_usage_effect_type);

                layers.Enqueue(memory);
            }

            EffectLayer interactive_layer = new EffectLayer("Interactive Effects");

            foreach (var input in input_list.ToArray())
            {
                if(input.type == input_item.input_type.Spectrum)
                {
                    float transition_value = input.progress / Effects.canvas_width;

                    if (transition_value > 1.0f)
                        continue;

                    Color color = input.spectrum.GetColorAt(transition_value);

                    interactive_layer.Set(input.key, color);
                }
                else if(input.type == input_item.input_type.AnimationMix)
                {
                    float time_value = input.progress / Effects.canvas_width;

                    if (time_value > 1.0f)
                        continue;

                    input.animation.Draw(interactive_layer.GetGraphics(), time_value);
                }
            }

            for (int x = input_list.Count - 1; x >= 0; x--)
            {
                if (input_list[x].progress > Effects.canvas_width)
                    input_list.RemoveAt(x);
                else
                {
                    float trans_added = ((Global.Configuration.desktop_profile.Settings as DesktopSettings).interactive_effect_speed * (getDeltaTime() * 5.0f));
                    input_list[x].progress += trans_added;
                }
            }

            layers.Enqueue(interactive_layer);

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
                    if (Global.held_modified == Keys.LControlKey)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_CONTROL, (Global.Configuration.desktop_profile.Settings as DesktopSettings).ctrl_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_CONTROL, (Global.Configuration.desktop_profile.Settings as DesktopSettings).ctrl_key_color);
                    sc_assistant_layer.Set((Global.Configuration.desktop_profile.Settings as DesktopSettings).ctrl_key_sequence, (Global.Configuration.desktop_profile.Settings as DesktopSettings).ctrl_key_color);
                }
                else if (Global.held_modified == Keys.LMenu || Global.held_modified == Keys.RMenu)
                {
                    if (Global.held_modified == Keys.LMenu)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_ALT, (Global.Configuration.desktop_profile.Settings as DesktopSettings).alt_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_ALT, (Global.Configuration.desktop_profile.Settings as DesktopSettings).alt_key_color);
                    sc_assistant_layer.Set((Global.Configuration.desktop_profile.Settings as DesktopSettings).alt_key_sequence, (Global.Configuration.desktop_profile.Settings as DesktopSettings).alt_key_color);
                }
                else if (Global.held_modified == Keys.LWin || Global.held_modified == Keys.RWin)
                {
                    if (Global.held_modified == Keys.LWin)
                        sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_WINDOWS, (Global.Configuration.desktop_profile.Settings as DesktopSettings).win_key_color);
                    else
                        sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_WINDOWS, (Global.Configuration.desktop_profile.Settings as DesktopSettings).win_key_color);
                    sc_assistant_layer.Set((Global.Configuration.desktop_profile.Settings as DesktopSettings).win_key_sequence, (Global.Configuration.desktop_profile.Settings as DesktopSettings).win_key_color);
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
