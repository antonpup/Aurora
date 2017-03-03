using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Timers;
using System.Threading;
using Aurora.Profiles;
using Aurora.Profiles.Desktop;
using System.Runtime.InteropServices;
using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Profiles.Generic_Application;
using Aurora.Utils;
using Aurora.Profiles.Overlays.SkypeOverlay;
using Aurora.Settings;

namespace Aurora
{
    

    public class GameEventHandler
    {
        
        private Event_Desktop desktop_e = new Event_Desktop();
        private Event_Idle idle_e = new Event_Idle();
        private Event_SkypeOverlay skype_overlay = new Event_SkypeOverlay();

        private List<TimedListObject> overlays = new List<TimedListObject>();

        private bool isForced = false;

        private Timer update_timer;
        private int timer_interval = 33;

        private string process_path = "";
        private long currentTick = 0L;
        private long nextProcessNameUpdate = 0L;

        private PreviewType preview_mode = PreviewType.Desktop;
        private string preview_mode_profile_key = "";


        public GameEventHandler()
        {

            overlays.Add(new TimedListObject(skype_overlay, 0, overlays));
        }

        ~GameEventHandler()
        {
            Destroy();
        }

        public bool Init()
        {
            

            try
            {
                update_timer = new System.Threading.Timer(g => {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    try
                    {
                        Update();
                    }
                    catch(Exception exc) {
                        Global.logger.LogLine("GameEventHandler.Update() Exception, " + exc, Logging_Level.Error);
                    }

                    watch.Stop();
                    update_timer?.Change(Math.Max(timer_interval, 0), Timeout.Infinite);
                }, null, 0, System.Threading.Timeout.Infinite);

                /*update_timer = new Timer(33);
                update_timer.Elapsed += new ElapsedEventHandler(update_timer_Tick);
                update_timer.Interval = 33; // in miliseconds
                update_timer.Start();*/
                GC.KeepAlive(update_timer);
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("GameEventHandler.Init() Exception, " + exc, Logging_Level.Error);
                return false;
            }

            return true;
        }

        public void Destroy()
        {
            update_timer?.Dispose();
            update_timer = null;
        }

        

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
        private const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;


        [DllImport("Oleacc.dll")]
        static extern IntPtr GetProcessHandleFromHwnd(IntPtr whandle);
        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
		
        private string GetActiveWindowsProcessname()
        {
            try
            {
                IntPtr windowHandle = IntPtr.Zero;
                IntPtr processhandle = IntPtr.Zero;
                IntPtr zeroHandle = IntPtr.Zero;
                windowHandle = GetForegroundWindow();
                processhandle = GetProcessHandleFromHwnd(windowHandle);

                StringBuilder sb = new StringBuilder(4048);
                GetModuleFileNameEx(processhandle, zeroHandle, sb, 4048);
                //Global.logger.LogLine("Current Foreground Window: " + sb.ToString(), Logging_Level.Info);

                System.IO.Path.GetFileName(sb.ToString());


               return sb.ToString();
            }
            catch (ArgumentException aex)
            {
                Global.logger.LogLine("Argument Exception: " + aex);
                return "";
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Exception in GetActiveWindowsProcessname" + exc);
                return "";
            }
        }

        private void Update()
        {
            if (Global.Configuration.detection_mode == Settings.ApplicationDetectionMode.ForegroroundApp && (currentTick >= nextProcessNameUpdate))
            {
                process_path = GetActiveWindowsProcessname();
                nextProcessNameUpdate = currentTick + 1000L;
            }

            string process_name = System.IO.Path.GetFileName(process_path).ToLowerInvariant();
            if (Global.Configuration.excluded_programs.Contains(process_name))
                return;

            EffectsEngine.EffectFrame newframe = new EffectsEngine.EffectFrame();

            tagLASTINPUTINFO LastInput = new tagLASTINPUTINFO();
            Int32 IdleTime;
            LastInput.cbSize = (uint)Marshal.SizeOf(LastInput);
            LastInput.dwTime = 0;

            if (GetLastInputInfo(ref LastInput))
            {
                IdleTime = System.Environment.TickCount - LastInput.dwTime;

                if (IdleTime >= Global.Configuration.idle_delay * 60 * 1000)
                {
                    if (!(Global.Configuration.time_based_dimming_enabled &&
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute))
                    )
                    {
                        idle_e.UpdateLights(newframe);
                    }
                }
            }

            ProfileManager profile;

            if (((profile = ProfilesManager.GetProfileFromProcess(process_name)) != null) && profile.Settings.isEnabled)
            {
                if (profile.UpdateInterval != null)
                    timer_interval = (int)profile.UpdateInterval; // in miliseconds
                else
                    timer_interval = 33;

                //Does this need to be here?
                /*if (!(Global.Configuration.time_based_dimming_enabled && Global.Configuration.time_based_dimming_affect_games &&
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute))
                    )
                {*/
                    Global.dev_manager.InitializeOnce();

                    profile.Event.UpdateLights(newframe);
                //}
            }
            else if (preview_mode == PreviewType.GenericApplication && ((profile = ProfilesManager.GetProfileFromProcess(preview_mode_profile_key)) != null) && profile.Settings.isEnabled)
            {
                Global.dev_manager.InitializeOnce();
                ProfilesManager.Profiles[preview_mode_profile_key].Event.UpdateLights(newframe);

                /*if (profiles.ContainsKey(preview_mode_profile_key))
                    profiles[preview_mode_profile_key].UpdateLights(newframe);
                else
                {
                    Event_GenericApplication app_event = new Event_GenericApplication();
                    app_event.UpdateLights(newframe);
                    profiles.Add(preview_mode_profile_key, app_event);
                }*/
            }
            else if (preview_mode == PreviewType.Predefined && ((profile = ProfilesManager.GetProfileFromProcess(preview_mode_profile_key)) != null) && profile.Event.IsEnabled())
            {
                Global.dev_manager.InitializeOnce();

                profile.Event.UpdateLights(newframe);
            }
            else if (Global.Configuration.allow_wrappers_in_background && Global.net_listener != null && Global.net_listener.IsWrapperConnected && ((profile = ProfilesManager.GetProfileFromProcess(Global.net_listener.WrappedProcess)) != null) && profile.Event.IsEnabled())
            {
                timer_interval = 33; // in miliseconds

                if (!(Global.Configuration.time_based_dimming_enabled && Global.Configuration.time_based_dimming_affect_games &&
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute))
                    )
                {
                    Global.dev_manager.InitializeOnce();

                    profile.Event.UpdateLights(newframe);
                }
            }
            else
            {
                timer_interval = (int)(1000.0D / 30); //50 in miliseconds
                if (!(Global.Configuration.time_based_dimming_enabled &&
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute))
                    )
                {
                    if (!ProfilesManager.DesktopProfile.Settings.isEnabled)
                        Global.dev_manager.Shutdown();
                    else
                    {
                        Global.dev_manager.InitializeOnce();
                        desktop_e.UpdateLights(newframe);
                    }
                }

            }

            //Add overlays
            TimedListObject[] overlay_events = overlays.ToArray();
            foreach (TimedListObject evnt in overlay_events)
            {
                if ((evnt.item as LightEvent).IsEnabled)
                    (evnt.item as LightEvent).UpdateLights(newframe);
            }

            Global.effengine.PushFrame(newframe);

            currentTick += (long)timer_interval;
        }

        

        private GameEvent_Aurora_Wrapper ExtraWrapperGameEvent = new GameEvent_Aurora_Wrapper();

        public void GameStateUpdate(IGameState gs)
        {
            //Debug.WriteLine("Received gs!");

            //Global.logger.LogLine(gs.ToString(), Logging_Level.None, false);

            if (Global.Configuration.detection_mode == Settings.ApplicationDetectionMode.ForegroroundApp && (currentTick >= nextProcessNameUpdate))
            {
                process_path = GetActiveWindowsProcessname();
                nextProcessNameUpdate = currentTick + 1000L;
            }

            string process_name = System.IO.Path.GetFileName(process_path).ToLowerInvariant();

            if (Global.Configuration.excluded_programs.Contains(process_name))
            {
                return;
            }

            EffectsEngine.EffectFrame newframe = new EffectsEngine.EffectFrame();

            try
            {
                bool resolved_state = false;

                ProfileManager profile = ProfilesManager.GetProfileFromProcess(process_name);

                if (profile != null)
                {
                    profile.Event.UpdateLights(newframe, (IGameState)Activator.CreateInstance(profile.GameStateType, gs));
                    resolved_state = true;
                }
                if (!resolved_state)
                {
                    switch (Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider")).GetValue("appid").ToString())
                    {
                        /*case "570":
                            if (process_name.EndsWith("dota2.exe") && profile != null && profile.Event.IsEnabled())
                            {
                                profile.Event.UpdateLights(newframe, new Profiles.Dota_2.GSI.GameState_Dota2(gs));
                                resolved_state = true;
                            }
                            break;
                        case "730":
                            if (process_name.EndsWith("csgo.exe") && profiles.ContainsKey(process_name) && profiles[process_name].IsEnabled())
                            {
                                profiles[process_name].UpdateLights(newframe, new Profiles.CSGO.GSI.GameState_CSGO(gs));
                                resolved_state = true;
                            }
                            break;
                        case "218620":
                            if (process_name.EndsWith("payday2_win32_release.exe") && profiles.ContainsKey(process_name) && profiles[process_name].IsEnabled())
                            {
                                profiles[process_name].UpdateLights(newframe, new Profiles.Payday_2.GSI.GameState_PD2(gs));
                                resolved_state = true;
                            }
                            break;*/
                        case "0":
                            /*if (process_name.EndsWith("gta5.exe") && Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider")).GetValue("name").ToString().ToLowerInvariant().Equals("gta5.exe") && profiles.ContainsKey(process_name) && profiles[process_name].IsEnabled())
                            {
                                profiles[process_name].UpdateLights(newframe, gs as Profiles.GTA5.GSI.GameState_GTA5);
                                resolved_state = true;
                            }
                            else*/
                            if (Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider")).GetValue("name").ToString().ToLowerInvariant().Equals("skype.exe") && skype_overlay.IsEnabled())
                            {
                                skype_overlay.UpdateLights(newframe, new Profiles.Overlays.SkypeOverlay.State_SkypeOverlay(gs));
                            }
                            else
                            {
                                if (gs is GameState_Wrapper && Global.Configuration.allow_all_logitech_bitmaps)
                                {
                                    string gs_process_name = Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider")).GetValue("name").ToString().ToLowerInvariant();

                                    ProfileManager gsProfile = ProfilesManager.GetProfileFromProcess(gs_process_name);
                                    profile = profile ?? gsProfile;

                                    if (profile == null)
                                        ExtraWrapperGameEvent.UpdateWrapperLights(gs as GameState_Wrapper);
                                    else if (profile?.Event is GameEvent_Aurora_Wrapper)
                                        (profile.Event as GameEvent_Aurora_Wrapper).UpdateWrapperLights(gs as GameState_Wrapper);
                                    else
                                    {
                                        profile.Event.UpdateLights(newframe, gs as GameState_Wrapper);
                                        resolved_state = true;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                tagLASTINPUTINFO LastInput = new tagLASTINPUTINFO();
                Int32 IdleTime;
                LastInput.cbSize = (uint)Marshal.SizeOf(LastInput);
                LastInput.dwTime = 0;

                if (GetLastInputInfo(ref LastInput) && resolved_state)
                {
                    IdleTime = System.Environment.TickCount - LastInput.dwTime;

                    if (IdleTime >= Global.Configuration.idle_delay * 60 * 1000)
                    {
                        if (!(Global.Configuration.time_based_dimming_enabled && Global.Configuration.time_based_dimming_affect_games &&
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute))
                    )
                        {
                            idle_e.UpdateLights(newframe);
                        }
                    }
                }

                if (!(Global.Configuration.time_based_dimming_enabled && Global.Configuration.time_based_dimming_affect_games &&
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute))
                    )
                {
                    if (resolved_state)
                    {
                        //Add overlays
                        TimedListObject[] overlay_events = overlays.ToArray();
                        foreach (TimedListObject evnt in overlay_events)
                        {
                            if ((evnt.item as LightEvent).IsEnabled())
                                (evnt.item as LightEvent).UpdateLights(newframe);
                        }

                        Global.effengine.PushFrame(newframe);
                    }

                }
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Exception during GameStateUpdate(), error: " + e, Logging_Level.Warning);
            }
        }

        public void SetForcedUpdate(bool forced)
        {
            this.isForced = forced;
        }


        public void AddOverlayForDuration(LightEvent overlay_event, int duration, bool isUnique = true)
        {
            if (isUnique)
            {
                TimedListObject[] overlays_array = overlays.ToArray();
                bool isFound = false;

                foreach (TimedListObject obj in overlays_array)
                {
                    if (obj.item.GetType() == overlay_event.GetType())
                    {
                        isFound = true;
                        obj.AdjustDuration(duration);
                        break;
                    }
                }

                if (!isFound)
                {
                    overlays.Add(new TimedListObject(overlay_event, duration, overlays));
                }
            }
            else
            {
                overlays.Add(new TimedListObject(overlay_event, duration, overlays));
            }
        }
    }
}
