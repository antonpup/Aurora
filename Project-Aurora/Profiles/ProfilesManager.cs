using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Profiles.Desktop;
using Aurora.Profiles.Generic_Application;
using Aurora.Profiles.Overlays.SkypeOverlay;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Profiles
{
    public class ProfilesManager : IInit
    {
        //private static ProfilesManager instance;

        //public static ProfilesManager Instance { get { return instance ?? (instance = new ProfilesManager()); } }

        public class LayerHandlerEntry
        {
            public Type Type { get; set; }

            public string Title { get; set; }

            public string Key { get; set; }

            public LayerHandlerEntry(string key, string title, Type type)
            {
                this.Type = type;
                this.Title = title;
                this.Key = key;
            }

            public override string ToString()
            {
                return Title;
            }
        }

        /*public class ProfilesManagerSettings
        {
            public List<string> ProfileOrder { get; protected set; } = new List<string>();
        }*/

        public Dictionary<string, ILightEvent> Events { get; private set; } = new Dictionary<string, ILightEvent> { { "desktop", new Desktop.DesktopProfileManager() } };

        public Desktop.DesktopProfileManager DesktopProfile { get { return (Desktop.DesktopProfileManager)Events["desktop"]; } }

        private List<string> Underlays = new List<string>();
        private List<string> Normal = new List<string>();
        private List<string> Overlays = new List<string>();

        private Dictionary<string, string> EventProcesses { get; set; } = new Dictionary<string, string>();

        private Dictionary<string, string> EventAppIDs { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, LayerHandlerEntry> LayerHandlers { get; private set; } = new Dictionary<string, LayerHandlerEntry>();

        public List<string> DefaultLayerHandlers { get; private set; } = new List<string>();

        public string AdditionalProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "AdditionalProfiles");

        //public string ProfilesManagerSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "ProfilesSettings.json");

        //public static ProfilesManagerSettings Settings { get; private set; }

        private ActiveProcessMonitor processMonitor;

        public bool Initialized { get; private set; }

        public bool Initialize()
        {
            if (Initialized)
                return true;

            processMonitor = new ActiveProcessMonitor();

            #region Initiate Defaults
            RegisterEvents(new List<ILightEvent> {
                new Dota_2.Dota2ProfileManager(),
                new CSGO.CSGOProfileManager(),
                new GTA5.GTA5ProfileManager(),
                new RocketLeague.RocketLeagueProfileManager(),
                new Overwatch.OverwatchProfileManager(),
                new Payday_2.PD2ProfileManager(),
                new TheDivision.TheDivisionProfileManager(),
                new LeagueOfLegends.LoLProfileManager(),
                new HotlineMiami.HMProfileManager(),
                new TheTalosPrinciple.TalosPrincipleProfileManager(),
                new BF3.BF3ProfileManager(),
                new Blacklight.BLightProfileManager(),
                new Magic_Duels_2012.MagicDuels2012ProfileManager(),
                new ShadowOfMordor.ShadowOfMordorProfileManager(),
                new Serious_Sam_3.SSam3ProfileManager(),
                new DiscoDodgeball.DiscoDodgeballProfileManager(),
                new XCOM.XCOMProfileManager(),
                new Evolve.EvolveProfileManager(),
                new Metro_Last_Light.MetroLLProfileManager(),
                new Guild_Wars_2.GW2ProfileManager(),
                new WormsWMD.WormsWMDProfileManager(),
                new Blade_and_Soul.BnSProfileManager(),
                new Event_SkypeOverlay()
            });

            RegisterLayerHandlers(new List<LayerHandlerEntry> {
                new LayerHandlerEntry("Default", "Default Layer", typeof(DefaultLayerHandler)),
                new LayerHandlerEntry("Solid", "Solid Color Layer", typeof(SolidColorLayerHandler)),
                new LayerHandlerEntry("SolidFilled", "Solid Fill Color Layer", typeof(SolidFillLayerHandler)),
                new LayerHandlerEntry("Gradient", "Gradient Layer", typeof(GradientLayerHandler)),
                new LayerHandlerEntry("GradientFill", "Gradient Fill Layer", typeof(GradientFillLayerHandler)),
                new LayerHandlerEntry("Breathing", "Breathing Layer", typeof(BreathingLayerHandler)),
                new LayerHandlerEntry("Blinking", "Blinking Layer", typeof(BlinkingLayerHandler)),
                new LayerHandlerEntry("Image", "Image Layer", typeof(ImageLayerHandler)),
                new LayerHandlerEntry("Script", "Script Layer", typeof(ScriptLayerHandler)),
                new LayerHandlerEntry("Percent", "Percent Effect Layer", typeof(PercentLayerHandler)),
                new LayerHandlerEntry("PercentGradient", "Percent (Gradient) Effect Layer", typeof(PercentGradientLayerHandler)),
                new LayerHandlerEntry("Interactive", "Interactive Layer", typeof(InteractiveLayerHandler) ),
                new LayerHandlerEntry("ShortcutAssistant", "Shortcut Assistant Layer", typeof(ShortcutAssistantLayerHandler) ),
                new LayerHandlerEntry("Equalizer", "Equalizer Layer", typeof(EqualizerLayerHandler) ),
                new LayerHandlerEntry("Ambilight", "Ambilight Layer", typeof(AmbilightLayerHandler) ),
                new LayerHandlerEntry("LockColor", "Lock Color Layer", typeof(LockColourLayerHandler) ),
            }, true);

            #endregion

            if (Directory.Exists(AdditionalProfilesPath))
            {
                List<string> additionals = new List<string>(Directory.EnumerateDirectories(AdditionalProfilesPath));
                foreach (var dir in additionals)
                {
                    if (File.Exists(Path.Combine(dir, "default.json")))
                    {
                        string proccess_name = Path.GetFileName(dir);
                        RegisterEvent(new GenericApplicationProfileManager(proccess_name));
                    }
                }
            }

            foreach (var profile in Events)
            {
                profile.Value.Initialize();
            }

            LoadSettings();

            this.InitUpdate();

            Initialized = true;
            return Initialized;
        }

        private void LoadSettings()
        {
            /*if (File.Exists(ProfilesManagerSettingsPath))
            {
                try
                {
                    Settings = JsonConvert.DeserializeObject<ProfilesManagerSettings>(File.ReadAllText(ProfilesManagerSettingsPath), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine("Exception occured while loading Profiles Settings.\nException:" + exc, Logging_Level.Error);
                    SaveSettings();
                }
            }
            else
                SaveSettings();*/

            foreach (var kvp in Events)
            {
                if (!Global.Configuration.ProfileOrder.Contains(kvp.Key) && kvp.Value is ProfileManager)
                    Global.Configuration.ProfileOrder.Add(kvp.Key);
            }

            foreach(string key in Global.Configuration.ProfileOrder.ToList())
            {
                if (!Events.ContainsKey(key) || !(Events[key] is ProfileManager))
                    Global.Configuration.ProfileOrder.Remove(key);
            }

            //Settings.ProfileOrder.Remove("desktop");
            //Settings.ProfileOrder.Insert(0, "desktop");
        }

        /*public static void SaveSettings()
        {
            if (Settings == null)
                Settings = new ProfilesManagerSettings();

            File.WriteAllText(ProfilesManagerSettingsPath, JsonConvert.SerializeObject(Settings, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
        }*/

        public void SaveAll()
        {
            //SaveSettings();

            foreach(var profile in Events)
            {
                if (profile.Value is ProfileManager)
                    ((ProfileManager)profile.Value).SaveProfiles();
            }
        }

        public bool RegisterProfile(LightEventConfig config)
        {
            return RegisterEvent(new ProfileManager(config));
        }

        private List<string> GetEventTable(LightEventType type)
        {
            List<string> events;
            switch (type)
            {
                case LightEventType.Normal:
                    events = Normal;
                    break;
                case LightEventType.Overlay:
                    events = Overlays;
                    break;
                case LightEventType.Underlay:
                    events = Underlays;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return events;
        }

        private bool InsertLightEvent(ILightEvent lightEvent, LightEventType? old = null)
        {
            if (old == null)
            {
                lightEvent.Config.PropertyChanged += LightEvent_PropertyChanged;
            }
            else
            {
                var oldEvents = GetEventTable((LightEventType)old);
                oldEvents.Remove(lightEvent.Config.ID);
            }

            var events = GetEventTable(lightEvent.Config.Type);

            events.Add(lightEvent.Config.ID);

            return true;   
        }

        private void LightEvent_PropertyChanged(object sender, PropertyChangedExEventArgs e)
        {
            ILightEvent lightEvent = (ILightEvent)sender;
            if (e.PropertyName.Equals(nameof(LightEventConfig.Type)))
            {
                LightEventType old = (LightEventType)e.OldValue;
                LightEventType newVal = (LightEventType)e.NewValue;

                if (!old.Equals(newVal))
                {
                    InsertLightEvent(lightEvent, old);
                }
            }
        }

        public bool RegisterEvent(ILightEvent @event)
        {
            string key = @event.Config.ID;
            if (string.IsNullOrWhiteSpace(key) || Events.ContainsKey(key))
                return false;

            Events.Add(key, @event);

            if (@event.Config.ProcessNames != null)
            {
                foreach (string exe in @event.Config.ProcessNames)
                {
                    if (!exe.Equals(key))
                        EventProcesses.Add(exe, key);
                }
            }

            if (!String.IsNullOrWhiteSpace(@event.Config.AppID))
                EventAppIDs.Add(@event.Config.AppID, key);

            if (@event is ProfileManager)
            {
                if (!Global.Configuration.ProfileOrder.Contains(key))
                    Global.Configuration.ProfileOrder.Add(key);
            }

            this.InsertLightEvent(@event);

            if (Initialized)
                @event.Initialize();

            return true;
        }

        public void RegisterProfiles(List<LightEventConfig> profiles)
        {
            foreach (var profile in profiles)
            {
                RegisterProfile(profile);
            }
        }

        public void RegisterEvents(List<ILightEvent> profiles)
        {
            foreach (var profile in profiles)
            {
                RegisterEvent(profile);
            }
        }

        public void RemoveGenericProfile(string key)
        {
            if (Events.ContainsKey(key))
            {
                if (!(Events[key] is GenericApplicationProfileManager))
                    return;
                GenericApplicationProfileManager profile = (GenericApplicationProfileManager)Events[key];
                string path = profile.GetProfileFolderPath();
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                Events.Remove(key);
                Global.Configuration.ProfileOrder.Remove(key);

                //SaveSettings();
            }
        }

        public ILightEvent GetProfileFromProcess(string process)
        {
            if (EventProcesses.ContainsKey(process))
                return Events[EventProcesses[process]];
            else if (Events.ContainsKey(process))
                return Events[process];
            else if (Events.ContainsKey(process))
                return Events[process];


            return null;
        }

        public ILightEvent GetProfileFromAppID(string appid)
        {
            if (EventAppIDs.ContainsKey(appid))
                return Events[EventAppIDs[appid]];
            else if (Events.ContainsKey(appid))
                return Events[appid];
            else if (Events.ContainsKey(appid))
                return Events[appid];

            return null;
        }

        public void RegisterLayerHandlers(List<LayerHandlerEntry> layers, bool @default = true)
        {
            foreach(var layer in layers)
            {
                RegisterLayerHandler(layer, @default);
            }
        }

        public bool RegisterLayerHandler(LayerHandlerEntry entry, bool @default = true)
        {
            if (LayerHandlers.ContainsKey(entry.Key) || DefaultLayerHandlers.Contains(entry.Key))
                return false;

            LayerHandlers.Add(entry.Key, entry);

            if (@default)
                DefaultLayerHandlers.Add(entry.Key);

            return true;
        }

        public bool RegisterLayerHandler(string key, string title, Type type, bool @default = true)
        {
            return RegisterLayerHandler(new LayerHandlerEntry(key, title, type));
        }

        public Type GetLayerHandlerType(string key)
        {
            return LayerHandlers.ContainsKey(key) ? LayerHandlers[key].Type : null;
        }

        public ILayerHandler GetLayerHandlerInstance(LayerHandlerEntry entry)
        {
            return (ILayerHandler)Activator.CreateInstance(entry.Type);
        }

        public ILayerHandler GetLayerHandlerInstance(string key)
        {
            if (LayerHandlers.ContainsKey(key))
            {
                return GetLayerHandlerInstance(LayerHandlers[key]);
            }


            return null;
        }

        private Timer updateTimer;

        private const int defaultTimerInterval = 33;
        private int timerInterval = defaultTimerInterval;

        private long nextProcessNameUpdate;
        private long currentTick;
        private string previewModeProfileKey = "";

        private List<TimedListObject> overlays = new List<TimedListObject>();
        private Event_Idle idle_e = new Event_Idle();

        public string PreviewProfileKey { get { return previewModeProfileKey; } set { previewModeProfileKey = value ?? string.Empty; } }

        private void InitUpdate()
        {
            updateTimer = new System.Threading.Timer(g => {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                try
                {
                    Update();
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine("ProfilesManager.Update() Exception, " + exc, Logging_Level.Error);
                }

                watch.Stop();
                currentTick += timerInterval + watch.ElapsedMilliseconds;
                updateTimer?.Change(Math.Max(timerInterval, 0), Timeout.Infinite);
            }, null, 0, System.Threading.Timeout.Infinite);
            GC.KeepAlive(updateTimer);
        }

        private void UpdateProcess()
        {
            if (Global.Configuration.detection_mode == Settings.ApplicationDetectionMode.ForegroroundApp && (currentTick >= nextProcessNameUpdate))
            {
                processMonitor.GetActiveWindowsProcessname();
                nextProcessNameUpdate = currentTick + 1000L;
            }
        }

        private void UpdateIdleEffects(EffectsEngine.EffectFrame newFrame)
        {
            tagLASTINPUTINFO LastInput = new tagLASTINPUTINFO();
            Int32 IdleTime;
            LastInput.cbSize = (uint)Marshal.SizeOf(LastInput);
            LastInput.dwTime = 0;

            if (ActiveProcessMonitor.GetLastInputInfo(ref LastInput))
            {
                IdleTime = System.Environment.TickCount - LastInput.dwTime;

                if (IdleTime >= Global.Configuration.idle_delay * 60 * 1000)
                {
                    if (!(Global.Configuration.time_based_dimming_enabled &&
                    Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute))
                    )
                    {
                        idle_e.UpdateLights(newFrame);
                    }
                }
            }
        }

        private void Update()
        {
            //Blackout. TODO: Cleanup this a bit. Maybe push blank effect frame to keyboard incase it has existing stuff displayed
            if ((Global.Configuration.time_based_dimming_enabled &&
               Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute)))
                return;

            UpdateProcess();

            string process_name = System.IO.Path.GetFileName(processMonitor.ProcessPath).ToLowerInvariant();

            EffectsEngine.EffectFrame newFrame = new EffectsEngine.EffectFrame();

            foreach(var underlay in Underlays)
            {
                ILightEvent @event = Events[underlay];
                if (@event.IsEnabled && (@event.Config.ProcessNames == null || ProcessUtils.AnyProcessExists(@event.Config.ProcessNames)))
                    @event.UpdateLights(newFrame);
            }

            //TODO: Move these IdleEffects to an event
            //this.UpdateIdleEffects(newFrame);

            ILightEvent profile = null;
            ILightEvent tempProfile = null;
            //Global.logger.LogLine(process_name);
            if (!Global.Configuration.excluded_programs.Contains(process_name)) { 
                //TODO: GetProfile that checks based on event type
                if ((((tempProfile = GetProfileFromProcess(process_name)) != null) && tempProfile.Config.Type == LightEventType.Normal && tempProfile.IsEnabled)
                    || (((tempProfile = GetProfileFromProcess(previewModeProfileKey)) != null) && tempProfile.Config.Type == LightEventType.Normal && tempProfile.IsEnabled)
                    || (Global.Configuration.allow_wrappers_in_background && Global.net_listener != null && Global.net_listener.IsWrapperConnected && ((tempProfile = GetProfileFromProcess(Global.net_listener.WrappedProcess)) != null) && tempProfile.Config.Type == LightEventType.Normal && tempProfile.IsEnabled)
                )
                    profile = tempProfile;
            }

            profile = profile ?? DesktopProfile;

            timerInterval = (profile as ProfileManager)?.Config?.UpdateInterval ?? defaultTimerInterval;

            Global.dev_manager.InitializeOnce();
            profile.UpdateLights(newFrame);

            foreach (var overlay in Overlays)
            {
                ILightEvent @event = Events[overlay];
                if (@event.IsEnabled && (@event.Config.ProcessNames == null || ProcessUtils.AnyProcessExists(@event.Config.ProcessNames)))
                    @event.UpdateLights(newFrame);
            }

            //Add overlays
            TimedListObject[] overlay_events = overlays.ToArray();
            foreach (TimedListObject evnt in overlay_events)
            {
                if ((evnt.item as LightEvent).IsEnabled)
                    (evnt.item as LightEvent).UpdateLights(newFrame);
            }

            Global.effengine.PushFrame(newFrame);
        }

        public void GameStateUpdate(IGameState gs)
        {
            //Debug.WriteLine("Received gs!");

            //Global.logger.LogLine(gs.ToString(), Logging_Level.None, false);

            //UpdateProcess();

            string process_name = System.IO.Path.GetFileName(processMonitor.ProcessPath).ToLowerInvariant();

            //EffectsEngine.EffectFrame newFrame = new EffectsEngine.EffectFrame();

            try
            {
                ILightEvent profile = this.GetProfileFromProcess(process_name);

                JObject provider = Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider"));
                string appid = provider.GetValue("appid").ToString();
                string name = provider.GetValue("name").ToString().ToLowerInvariant();

                if (profile != null || (profile = GetProfileFromAppID(appid)) != null || (profile = GetProfileFromProcess(name)) != null)
                    profile.SetGameState((IGameState)Activator.CreateInstance(profile.Config.GameStateType, gs));
                else if (gs is GameState_Wrapper && Global.Configuration.allow_all_logitech_bitmaps)
                {
                    string gs_process_name = Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider")).GetValue("name").ToString().ToLowerInvariant();

                    profile = profile ?? GetProfileFromProcess(gs_process_name);

                    if (profile == null)
                    {
                        Events.Add(gs_process_name, new GameEvent_Aurora_Wrapper());
                        profile = Events[gs_process_name];
                    }

                    profile.SetGameState(gs);
                }

                //UpdateIdleEffects(newFrame);
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Exception during GameStateUpdate(), error: " + e, Logging_Level.Warning);
            }
        }

        public void ResetGameState(string process)
        {
            ILightEvent profile;
            if (((profile = GetProfileFromProcess(process)) != null))
                profile.ResetGameState();
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

        public void Dispose()
        {
        }
    }
}
