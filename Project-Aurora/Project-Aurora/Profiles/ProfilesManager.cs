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
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Profiles
{
    public class LightEventsLayer : IInitialize
    {
        [JsonIgnore]
        public Dictionary<string, ILightEvent> Events = new Dictionary<string, ILightEvent>();

        [JsonIgnore]
        private Dictionary<string, string> EventProcesses { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        private Dictionary<string, string> EventAppIDs { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public List<ILightEvent> SortedEvents { get { var lst = Events.Values.ToList(); lst.Sort((i, j) => EventOrder.IndexOf(i.Config.ID).CompareTo(EventOrder.IndexOf(j.Config.ID))); return lst; } }

        [JsonIgnore]
        public bool Initialized { get; private set; }

        public List<string> EventOrder = new List<string>();

        private void InsertEvent(ILightEvent @event)
        {
            string key = @event.Config.ID;
            Events.Add(key, @event);

            if (@event.Config.ProcessNames != null)
            {
                foreach (string exe in @event.Config.ProcessNames)
                {
                    if (!exe.Equals(key) && !EventProcesses.ContainsKey(exe))
                        EventProcesses.Add(exe, key);
                }
            }

            if (!String.IsNullOrWhiteSpace(@event.Config.AppID))
                EventAppIDs.Add(@event.Config.AppID, key);

            if(!EventOrder.Contains(key))
                EventOrder.Add(key);

        }

        public bool RegisterEvent(ILightEvent @event)
        {
            string key = @event.Config.ID;
            if (string.IsNullOrWhiteSpace(key) || Events.ContainsKey(key))
                return false;

            InsertEvent(@event);
            
            return true;
        }

        public ILightEvent GetEvent(string name)
        {
            if (Events.ContainsKey(name))
                return Events[name];
            else if (EventAppIDs.ContainsKey(name))
                return Events[EventAppIDs[name]];
            else if (EventProcesses.ContainsKey(name))
                return Events[EventProcesses[name]];

            return null;
        }

        public bool Initialize()
        {
            if (Initialized)
                return true;

            foreach(var lightEvent in Events)
            {
                lightEvent.Value.Initialize();
            }

            Initialized = true;
            return true;
        }

        public void SaveAll()
        {
            foreach(var lightEvent in Events)
            {
                if (lightEvent.Value is ProfileManager)
                {
                    ((ProfileManager)lightEvent.Value).SaveProfiles();
                }
            }
        }

        public void Dispose()
        {

        }

        public void Remove(string ID, ProfilesManager profiles)
        {
            if (!Events.ContainsKey(ID))
                return;

            ILightEvent levent = Events[ID];
            if (levent is ProfileManager)
                ((ProfileManager)levent).SaveProfiles();
            profiles.RegisterEvent(levent);
            Events.Remove(ID);

            if (levent.Config.ProcessNames != null)
            {
                foreach (string exe in levent.Config.ProcessNames)
                {
                    if (EventProcesses.ContainsKey(exe))
                        EventProcesses.Remove(exe);
                }
            }

            if (!String.IsNullOrWhiteSpace(levent.Config.AppID) && EventAppIDs.ContainsKey(levent.Config.AppID))
                EventAppIDs.Remove(levent.Config.AppID);

            EventOrder.Remove(ID);
        }

        internal void GetMissing(ProfilesManager profilesManager)
        {
            for (int i = 0; i < EventOrder.Count; i++)
            {
                string key = EventOrder[i];
                if (Events.ContainsKey(key))
                    continue;
                if (!profilesManager.AvailableEvents.ContainsKey(key))
                {
                    EventOrder.RemoveAt(i);
                    i = Math.Max(0, i - 1);
                }
                else
                {
                    InsertEvent(profilesManager.AvailableEvents[key]);
                    profilesManager.AvailableEvents.Remove(key);
                }
            }
        }
    }

    public class ProfilesManager : IInitialize
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

        public class ProfilesManagerSettings
        {
            public Dictionary<LightEventType, LightEventsLayer> EventLayers { get; private set; } = new Dictionary<LightEventType, LightEventsLayer>();

            public ProfilesManagerSettings()
            {
                AddMissing();
            }

            private void AddMissing()
            {
                foreach (var eventType in Enum.GetValues(typeof(LightEventType)))
                {
                    LightEventType type = (LightEventType)eventType;
                    if (!EventLayers.ContainsKey(type))
                        EventLayers.Add(type, new LightEventsLayer());
                }
            }

            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                AddMissing();
            }
        }

        public ProfilesManagerSettings Settings { get; protected set; }

        public Desktop.DesktopProfileManager DesktopProfile { get { return (Desktop.DesktopProfileManager)Settings.EventLayers[LightEventType.Normal].Events["desktop"]; } }

        public Dictionary<string, ILightEvent> AvailableEvents = new Dictionary<string, ILightEvent>();


        public Dictionary<string, LayerHandlerEntry> LayerHandlers { get; private set; } = new Dictionary<string, LayerHandlerEntry>();

        public List<string> DefaultLayerHandlers { get; private set; } = new List<string>();

        public string AdditionalProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "AdditionalProfiles");

        public string ProfilesManagerSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "ProfilesSettings.json");

        private ActiveProcessMonitor processMonitor;

        public bool Initialized { get; private set; }

        public ProfilesManager()
        {
        }

        public bool Initialize()
        {
            if (Initialized)
                return true;

            processMonitor = new ActiveProcessMonitor();

            LoadSettings();

            //TODO: Registration of outside events needs to be done after loading the settings

            #region Initiate Defaults
            RegisterEvents(new List<ILightEvent> {
                new Desktop.DesktopProfileManager(),
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

            foreach (var group in Settings.EventLayers)
            {
                group.Value.Initialize();
            }

            foreach(var layer in Settings.EventLayers)
            {
                layer.Value.GetMissing(this);
            }

            this.InitUpdate();

            Initialized = true;
            return Initialized;
        }

        private void LoadSettings()
        {
            if (File.Exists(ProfilesManagerSettingsPath))
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
                SaveSettings();
                
            //Settings.ProfileOrder.Remove("desktop");
            //Settings.ProfileOrder.Insert(0, "desktop");
        }

        public void SaveSettings()
        {
            if (Settings == null)
                Settings = new ProfilesManagerSettings();

            File.WriteAllText(ProfilesManagerSettingsPath, JsonConvert.SerializeObject(Settings, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
        }

        public void SaveAll()
        {
            SaveSettings();

            foreach (var group in Settings.EventLayers)
            {
                group.Value.SaveAll();
            }
        }

        public bool RegisterProfile(LightEventConfig config)
        {
            return RegisterEvent(new ProfileManager(config));
        }

        private bool InsertLightEvent(ILightEvent lightEvent, LightEventType? old = null)
        {
            if (old == null)
            {
                //lightEvent.Config.PropertyChanged += LightEvent_PropertyChanged;
            }
            else
            {
                //var oldEvents = Settings.EventLayers[(LightEventType)old];
                //oldEvents.Remove(lightEvent.Config.ID, this);
            }

            var events = Settings.EventLayers[(LightEventType)lightEvent.Config.Type];

            return events.RegisterEvent(lightEvent);
        }

        public bool RegisterEventToLayer(LightEventType layerType, string effectKey)
        {
            if (!AvailableEvents.ContainsKey(effectKey))
                return false;

            ILightEvent lightEvent = AvailableEvents[effectKey];
            AvailableEvents.Remove(effectKey);
            return Settings.EventLayers[layerType].RegisterEvent(lightEvent);
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
            LightEventType? type = @event.Config.Type;
            if (string.IsNullOrWhiteSpace(key))
                return false;


            if (!@event.Config.IsDefault)
            {
                if (!AvailableEvents.ContainsKey(key))
                    AvailableEvents.Add(key, @event);
            }
            else
                InsertLightEvent(@event);

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

        public void DeleteEvent(string key, LightEventType? type = null)
        {
            if (type != null)
                Settings.EventLayers[(LightEventType)type].Remove(key, this);

            ILightEvent levent = AvailableEvents[key];
            if (!levent.Config.IsDeletable)
                return;
            levent.Delete();
            AvailableEvents.Remove(key);
        }

        //TODO: Update this method
        /*public void RemoveGenericProfile(string key)
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
        }*/

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

        public void RemoveEventFromLayer(LightEventType layerType, string key)
        {
            Settings.EventLayers[layerType].Remove(key, this);
            this.SaveSettings();
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
            if (Global.Configuration.detection_mode == ApplicationDetectionMode.ForegroroundApp && (currentTick >= nextProcessNameUpdate))
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

        private void UpdateEventsLayer(LightEventType layer, EffectsEngine.EffectFrame newFrame)
        {
            var eventLayer = Settings.EventLayers[layer];
            foreach(var levent in eventLayer.EventOrder)
            {
                ILightEvent @event = eventLayer.Events[levent];
                if (@event.IsEnabled && (previewModeProfileKey.Equals(levent) || ((@event.Config.ProcessNames == null || ProcessUtils.AnyProcessExists(@event.Config.ProcessNames)) && string.IsNullOrWhiteSpace(previewModeProfileKey))))
                    @event.UpdateLights(newFrame);
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

            Global.dev_manager.InitializeOnce();

            this.UpdateEventsLayer(LightEventType.Underlay, newFrame);

            //TODO: Move these IdleEffects to an event
            //this.UpdateIdleEffects(newFrame);

            ILightEvent profile = null;
            ILightEvent tempProfile = null;
            if (!Global.Configuration.excluded_programs.Contains(process_name)) {
                var layer = Settings.EventLayers[LightEventType.Normal];
                if ((((tempProfile = layer.GetEvent(process_name)) != null) && tempProfile.IsEnabled)
                    || (((tempProfile = layer.GetEvent(previewModeProfileKey)) != null) && tempProfile.IsEnabled)
                    || (Global.Configuration.allow_wrappers_in_background && Global.net_listener != null && Global.net_listener.IsWrapperConnected && ((tempProfile = layer.GetEvent(Global.net_listener.WrappedProcess)) != null) && tempProfile.IsEnabled)
                )
                    profile = tempProfile;
            }

            if (profile == null && string.IsNullOrWhiteSpace(previewModeProfileKey))
                profile = DesktopProfile;

            timerInterval = (profile as ProfileManager)?.Config?.UpdateInterval ?? defaultTimerInterval;

            //Check if any keybinds have been triggered
            foreach(var prof in (profile as ProfileManager).Profiles)
            {
                if(prof.TriggerKeybind.IsPressed() && !(profile as ProfileManager).Settings.ProfileName.Equals(prof.ProfileName))
                {
                    (profile as ProfileManager).SwitchToProfile(prof);
                    break;
                }
            }

            profile?.UpdateLights(newFrame);

            this.UpdateEventsLayer(LightEventType.Overlay, newFrame);

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
            string process_name = System.IO.Path.GetFileName(processMonitor.ProcessPath).ToLowerInvariant();

            try
            {
                ILightEvent profile = this.GetEvent(process_name);

                JObject provider = Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider"));
                string appid = provider.GetValue("appid").ToString();
                string name = provider.GetValue("name").ToString().ToLowerInvariant();

                if (profile != null || (profile = GetEvent(appid)) != null || (profile = GetEvent(name)) != null)
                    profile.SetGameState((IGameState)Activator.CreateInstance(profile.Config.GameStateType, gs));
                else if (gs is GameState_Wrapper && Global.Configuration.allow_all_logitech_bitmaps)
                {
                    string gs_process_name = Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider")).GetValue("name").ToString().ToLowerInvariant();

                    profile = profile ?? GetEvent(gs_process_name);

                    if (profile == null)
                    {
                        var layer = Settings.EventLayers[LightEventType.Normal];
                        layer.RegisterEvent(new GameEvent_Aurora_Wrapper(gs_process_name));
                        profile = layer.Events[gs_process_name];
                    }

                    profile.SetGameState(gs);
                }
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Exception during GameStateUpdate(), error: " + e, Logging_Level.Warning);
            }
        }

        public ILightEvent GetEvent(string name)
        {
            foreach(var layer in Settings.EventLayers)
            {
                var levent = layer.Value.GetEvent(name);
                if (levent != null)
                    return levent;
            }

            return null;
        }

        public void ResetGameState(string process)
        {
            ILightEvent profile;
            if (((profile = GetEvent(process)) != null))
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
            updateTimer.Dispose();
            foreach (var layer in Settings.EventLayers)
            {
                foreach(var levent in layer.Value.Events)
                {
                    levent.Value.Dispose();
                }
            }

        }

    }
}
