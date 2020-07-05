using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Profiles.Desktop;
using Aurora.Profiles.Generic_Application;
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;

namespace Aurora.Profiles
{
    public class ProfilesManagerSettings
    {
        public ProfilesManagerSettings()
        {

        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {

        }
    }

    public class LightingStateManager : ObjectSettings<ProfilesManagerSettings>, IInit
    {
        public Dictionary<string, ILightEvent> Events { get; private set; } = new Dictionary<string, ILightEvent> { { "desktop", new Desktop.Desktop() } };

        public Desktop.Desktop DesktopProfile { get { return (Desktop.Desktop)Events["desktop"]; } }

        private List<ILightEvent> StartedEvents = new List<ILightEvent>();
        private List<ILightEvent> UpdatedEvents = new List<ILightEvent>();

        private Dictionary<string, string> EventProcesses { get; set; } = new Dictionary<string, string>();

        private Dictionary<string, string> EventTitles { get; set; } = new Dictionary<string, string>();

        private Dictionary<string, string> EventAppIDs { get; set; } = new Dictionary<string, string>();

        public Dictionary<Type, LayerHandlerMeta> LayerHandlers { get; private set; } = new Dictionary<Type, LayerHandlerMeta>();

        public string AdditionalProfilesPath = Path.Combine(Global.AppDataDirectory, "AdditionalProfiles");

        public event EventHandler PreUpdate;
        public event EventHandler PostUpdate;

        private ActiveProcessMonitor processMonitor;
        private RunningProcessMonitor runningProcessMonitor;
        public RunningProcessMonitor RunningProcessMonitor => runningProcessMonitor;

        public LightingStateManager()
        {
            SettingsSavePath = Path.Combine(Global.AppDataDirectory, "ProfilesSettings.json");
        }

        public bool Initialized { get; private set; }

        public bool Initialize()
        {
            if (Initialized)
                return true;

            processMonitor = new ActiveProcessMonitor();
            runningProcessMonitor = new RunningProcessMonitor();

            // Register all Application types in the assembly
            var profileTypes = from type in Assembly.GetExecutingAssembly().GetTypes()
                               where type.BaseType == typeof(Application) && type != typeof(GenericApplication)
                               let inst = (Application)Activator.CreateInstance(type)
                               orderby inst.Config.Name
                               select inst;
            foreach (var inst in profileTypes)
                RegisterEvent(inst);

            // Register all layer types that are in the Aurora.Settings.Layers namespace.
            // Do not register all that are inside the assembly since some are application-specific (e.g. minecraft health layer)
            var layerTypes = from type in Assembly.GetExecutingAssembly().GetTypes()
                             where type.GetInterfaces().Contains(typeof(ILayerHandler))
                             let name = type.Name.CamelCaseToSpaceCase()
                             let meta = type.GetCustomAttribute<LayerHandlerMetaAttribute>()
                             where !type.IsGenericType
                             where meta == null || !meta.Exclude
                             select (type, meta);
            foreach (var (type, meta) in layerTypes)
                LayerHandlers.Add(type, new LayerHandlerMeta(type, meta));

            LoadSettings();

            LoadPlugins();

            if (Directory.Exists(AdditionalProfilesPath))
            {
                List<string> additionals = new List<string>(Directory.EnumerateDirectories(AdditionalProfilesPath));
                foreach (var dir in additionals)
                {
                    if (File.Exists(Path.Combine(dir, "settings.json")))
                    {
                        string proccess_name = Path.GetFileName(dir);
                        RegisterEvent(new GenericApplication(proccess_name));
                    }
                }
            }

            foreach (var profile in Events)
            {
                profile.Value.Initialize();
            }

            this.InitUpdate();

            // Listen for profile keybind triggers
            Global.InputEvents.KeyDown += CheckProfileKeybinds;

            Initialized = true;
            return Initialized;
        }

        private void LoadPlugins()
        {
            Global.PluginManager.ProcessManager(this);
        }

        protected override void LoadSettings(Type settingsType)
        {
            base.LoadSettings(settingsType);

            foreach (var kvp in Events)
            {
                if (!Global.Configuration.ProfileOrder.Contains(kvp.Key) && kvp.Value is Application)
                    Global.Configuration.ProfileOrder.Add(kvp.Key);
            }

            foreach(string key in Global.Configuration.ProfileOrder.ToList())
            {
                if (!Events.ContainsKey(key) || !(Events[key] is Application))
                    Global.Configuration.ProfileOrder.Remove(key);
            }

            Global.Configuration.ProfileOrder.Remove("desktop");
            Global.Configuration.ProfileOrder.Insert(0, "desktop");
        }

        public void SaveAll()
        {
            SaveSettings();

            foreach(var profile in Events)
            {
                if (profile.Value is Application)
                    ((Application)profile.Value).SaveAll();
            }
        }

        public bool RegisterProfile(LightEventConfig config)
        {
            return RegisterEvent(new Application(config));
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
                        EventProcesses.Add(exe.ToLower(), key);
                }
            }

            if (@event.Config.ProcessTitles != null)
                foreach (string titleRx in @event.Config.ProcessTitles)
                    EventTitles.Add(titleRx, key);

            if (!String.IsNullOrWhiteSpace(@event.Config.AppID))
                EventAppIDs.Add(@event.Config.AppID, key);

            if (@event is Application)
            {
                if (!Global.Configuration.ProfileOrder.Contains(key))
                    Global.Configuration.ProfileOrder.Add(key);
            }

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
                if (!(Events[key] is GenericApplication))
                    return;
                GenericApplication profile = (GenericApplication)Events[key];
                Events.Remove(key);
                Global.Configuration.ProfileOrder.Remove(key);

                profile.Dispose();

                string path = profile.GetProfileFolderPath();
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                //SaveSettings();
            }
        }

        // Used to match a process's name and optional window title to a profile
        public ILightEvent GetProfileFromProcessData(string processName, string processTitle = null)
        {
            var processNameProfile = GetProfileFromProcessName(processName);

            if (processNameProfile == null)
                return null;

            // Is title matching required?
            if (processNameProfile.Config.ProcessTitles != null)
            {
                var processTitleProfile = GetProfileFromProcessTitle(processTitle);

                if (processTitleProfile != null && processTitleProfile.Equals(processNameProfile))
                {
                    return processTitleProfile;
                }
            } else
            {
                return processNameProfile;
            }

            return null;
        }

        public ILightEvent GetProfileFromProcessName(string process)
        {
            if (EventProcesses.ContainsKey(process))
            {
                if (!Events.ContainsKey(EventProcesses[process]))
                    Global.logger.Warn($"GetProfileFromProcess: The process '{process}' exists in EventProcesses but subsequently '{EventProcesses[process]}' does not in Events!");

                return Events[EventProcesses[process]];
            }
            else if (Events.ContainsKey(process))
                return Events[process];

            return null;
        }

        public ILightEvent GetProfileFromProcessTitle(string title) {
            foreach (var entry in EventTitles) {
                if (Regex.IsMatch(title, entry.Key, RegexOptions.IgnoreCase)) {
                    if (!Events.ContainsKey(entry.Value))
                        Global.logger.Warn($"GetProfileFromProcess: The process with title '{title}' matchs an item in EventTitles but subsequently '{entry.Value}' does not in Events!");
                    else
                        return Events[entry.Value]; // added in an else so we keep searching for more valid regexes.
                }
            }
            return null;
        }

        public ILightEvent GetProfileFromAppID(string appid)
        {
            if (EventAppIDs.ContainsKey(appid))
            {
                if (!Events.ContainsKey(EventAppIDs[appid]))
                    Global.logger.Warn($"GetProfileFromAppID: The appid '{appid}' exists in EventAppIDs but subsequently '{EventAppIDs[appid]}' does not in Events!");
                return Events[EventAppIDs[appid]];
            }
            else if (Events.ContainsKey(appid))
                return Events[appid];

            return null;
        }

        /// <summary>
        /// Manually registers a layer. Only needed externally.
        /// </summary>
        public bool RegisterLayer<T>() where T : ILayerHandler
        {
            var t = typeof(T);
            if (LayerHandlers.ContainsKey(t)) return false;
            var meta = t.GetCustomAttribute<LayerHandlerMetaAttribute>() as LayerHandlerMetaAttribute;
            LayerHandlers.Add(t, new LayerHandlerMeta(t, meta));
            return true;
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
                if (Global.isDebug)
                    Update();
                else
                {
                    try
                    {
                        Update();
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("ProfilesManager.Update() Exception, " + exc);
                    }
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
                        UpdateEvent(idle_e, newFrame);
                    }
                }
            }
        }
        
        private void UpdateEvent(ILightEvent @event, EffectsEngine.EffectFrame frame)
        {
            StartEvent(@event);
            @event.UpdateLights(frame);
        }

        private bool StartEvent(ILightEvent @event)
        {
            UpdatedEvents.Add(@event);
            
            // Skip if event was already started
            if (StartedEvents.Contains(@event)) return false;

            StartedEvents.Add(@event);
            @event.OnStart();

            return true;
        }

        private bool StopUnUpdatedEvents()
        {
            // Skip if there are no started events or started events are the same since last update
            if (!StartedEvents.Any() || StartedEvents.SequenceEqual(UpdatedEvents)) return false;
            
            List<ILightEvent> eventsToStop = StartedEvents.Except(UpdatedEvents).ToList();
            foreach (var eventToStop in eventsToStop)
                eventToStop.OnStop();
            
            StartedEvents.Clear();
            StartedEvents.AddRange(UpdatedEvents);
            
            return true;
        }

        private void Update()
        {
            PreUpdate?.Invoke(this, null);
            UpdatedEvents.Clear();

            //Blackout. TODO: Cleanup this a bit. Maybe push blank effect frame to keyboard incase it has existing stuff displayed
            if ((Global.Configuration.time_based_dimming_enabled &&
               Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_start_minute, Global.Configuration.time_based_dimming_end_hour, Global.Configuration.time_based_dimming_end_minute)))
            {
                StopUnUpdatedEvents();
                return;
            }

            string raw_process_name = Path.GetFileName(processMonitor.ProcessPath);

            UpdateProcess();
            EffectsEngine.EffectFrame newFrame = new EffectsEngine.EffectFrame();



            //TODO: Move these IdleEffects to an event
            //this.UpdateIdleEffects(newFrame);
            
            ILightEvent profile = GetCurrentProfile(out bool preview);

            timerInterval = profile?.Config?.UpdateInterval ?? defaultTimerInterval;

            // If the current foreground process is excluded from Aurora, disable the lighting manager
            if ((profile is Desktop.Desktop && !profile.IsEnabled) || Global.Configuration.excluded_programs.Contains(raw_process_name))
            {
                StopUnUpdatedEvents();
                Global.dev_manager.Shutdown();
                Global.effengine.PushFrame(newFrame);
                return;
            }
            else
                Global.dev_manager.InitializeOnce();

            //Need to do another check in case Desktop is disabled or the selected preview is disabled
            if (profile.IsEnabled)
                UpdateEvent(profile, newFrame);

            // Overlay layers
            if (!preview || Global.Configuration.OverlaysInPreview) {
                foreach (var @event in GetOverlayActiveProfiles())
                    @event.UpdateOverlayLights(newFrame);

                //Add the Light event that we're previewing to be rendered as an overlay (assuming it's not already active)
                if (preview && Global.Configuration.OverlaysInPreview && !GetOverlayActiveProfiles().Contains(profile))
                    profile.UpdateOverlayLights(newFrame);

                UpdateIdleEffects(newFrame);
            }


            Global.effengine.PushFrame(newFrame);

            StopUnUpdatedEvents();
            PostUpdate?.Invoke(this, null);
        }

        /// <summary>Gets the current application.</summary>
        /// <param name="preview">Boolean indicating whether the application is selected because it is previewing (true) or because the process is open (false).</param>
        public ILightEvent GetCurrentProfile(out bool preview) {
            string process_name = Path.GetFileName(processMonitor.ProcessPath).ToLower();
            string process_title = processMonitor.GetActiveWindowsProcessTitle();
            ILightEvent profile = null;
            ILightEvent tempProfile = null;
            preview = false;

            //TODO: GetProfile that checks based on event type
            if ((tempProfile = GetProfileFromProcessData(process_name, process_title)) != null && tempProfile.IsEnabled)
                profile = tempProfile;
            else if ((tempProfile = GetProfileFromProcessName(previewModeProfileKey)) != null) //Don't check for it being Enabled as a preview should always end-up with the previewed profile regardless of it being disabled
            {
                profile = tempProfile;
                preview = true;
            } else if (Global.Configuration.allow_wrappers_in_background && Global.net_listener != null && Global.net_listener.IsWrapperConnected && ((tempProfile = GetProfileFromProcessName(Global.net_listener.WrappedProcess)) != null) && tempProfile.IsEnabled)
                profile = tempProfile;

            profile = profile ?? DesktopProfile;

            return profile;
        }
        /// <summary>Gets the current application.</summary>
        public ILightEvent GetCurrentProfile() => GetCurrentProfile(out bool _);

        /// <summary>
        /// Returns a list of all profiles that should have their overlays active. This will include processes that running but not in the foreground.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ILightEvent> GetOverlayActiveProfiles() => Events.Values
            .Where(evt => evt.IsOverlayEnabled)
            .Where(evt => evt.Config.ProcessNames == null || evt.Config.ProcessNames.Any(name => runningProcessMonitor.IsProcessRunning(name)));
            //.Where(evt => evt.Config.ProcessTitles == null || ProcessUtils.AnyProcessWithTitleExists(evt.Config.ProcessTitles));

        /// <summary>KeyDown handler that checks the current application's profiles for keybinds.
        /// In the case of multiple profiles matching the keybind, it will pick the next one as specified in the Application.Profile order.</summary>
        public void CheckProfileKeybinds(object sender, SharpDX.RawInput.KeyboardInputEventArgs e) {
            ILightEvent profile = GetCurrentProfile();

            // Check profile is valid and do not switch profiles if the user is trying to enter a keybind
            if (profile is Application && Controls.Control_Keybind._ActiveKeybind == null) {

                // Find all profiles that have their keybinds pressed
                List<ApplicationProfile> possibleProfiles = new List<ApplicationProfile>();
                foreach (var prof in (profile as Application).Profiles)
                    if (prof.TriggerKeybind.IsPressed())
                        possibleProfiles.Add(prof);

                // If atleast one profile has it's key pressed
                if (possibleProfiles.Count > 0) {
                    // The target profile is the NEXT valid profile after the currently selected one (or the first valid one if the currently selected one doesn't share this keybind)
                    int trg = (possibleProfiles.IndexOf((profile as Application).Profile) + 1) % possibleProfiles.Count;
                    (profile as Application).SwitchToProfile(possibleProfiles[trg]);
                }
            }
        }


        public void GameStateUpdate(IGameState gs)
        {
            //Debug.WriteLine("Received gs!");

//Global.logger.LogLine(gs.ToString(), Logging_Level.None, false);

//UpdateProcess();

//string process_name = System.IO.Path.GetFileName(processMonitor.ProcessPath).ToLowerInvariant();

//EffectsEngine.EffectFrame newFrame = new EffectsEngine.EffectFrame();
#if DEBUG
#else
            try
            {
#endif
                ILightEvent profile;// = this.GetProfileFromProcess(process_name);
                

                JObject provider = Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider"));
                string appid = provider.GetValue("appid").ToString();
                string name = provider.GetValue("name").ToString().ToLowerInvariant();

                if ((profile = GetProfileFromAppID(appid)) != null || (profile = GetProfileFromProcessName(name)) != null)
                {
                    IGameState gameState = gs;
                    if (profile.Config.GameStateType != null)
                        gameState = (IGameState)Activator.CreateInstance(profile.Config.GameStateType, gs.Json);
                    profile.SetGameState(gameState);
                }
                else if (gs is GameState_Wrapper && Global.Configuration.allow_all_logitech_bitmaps)
                {
                    string gs_process_name = Newtonsoft.Json.Linq.JObject.Parse(gs.GetNode("provider")).GetValue("name").ToString().ToLowerInvariant();
                    lock (Events)
                    {
                        profile = profile ?? GetProfileFromProcessName(gs_process_name);

                        if (profile == null)
                        {
                            Events.Add(gs_process_name, new GameEvent_Aurora_Wrapper(new LightEventConfig { GameStateType = typeof(GameState_Wrapper), ProcessNames = new[] { gs_process_name } }));
                            profile = Events[gs_process_name];
                        }

                        profile.SetGameState(gs);
                    }
                }
#if DEBUG
#else
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Exception during GameStateUpdate(), error: " + e, Logging_Level.Warning);
            }
#endif
        }

        public void ResetGameState(string process)
        {
            ILightEvent profile;
            if (((profile = GetProfileFromProcessName(process)) != null))
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
            updateTimer = null;

            foreach (var app in this.Events)
                app.Value.Dispose();
        }
    }


    /// <summary>
    /// POCO that stores data about a type of layer.
    /// </summary>
    public class LayerHandlerMeta {

        /// <summary>Creates a new LayerHandlerMeta object from the given meta attribute and type.</summary>
        public LayerHandlerMeta(Type type, LayerHandlerMetaAttribute attribute) {
            Name = attribute?.Name ?? type.Name.CamelCaseToSpaceCase().TrimEndStr(" Layer Handler");
            Type = type;
            IsDefault = attribute?.IsDefault ?? type.Namespace == "Aurora.Settings.Layers"; // if the layer is in the Aurora.Settings.Layers namespace, make the IsDefault true unless otherwise specified. If it is in another namespace, it's probably a custom application layer and so make IsDefault false unless otherwise specified
            Order = attribute?.Order ?? 0;
        }

        public string Name { get; }
        public Type Type { get; }
        public bool IsDefault { get; }
        public int Order { get; }
    }


    /// <summary>
    /// Attribute to provide additional meta data about layers for them to be registered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class LayerHandlerMetaAttribute : Attribute {
        /// <summary>A different name for the layer. If not specified, will automatically take it from the layer's class name.</summary>
        public string Name { get; set; }

        /// <summary>If true, this layer will be excluded from automatic registration. Default false.</summary>
        public bool Exclude { get; set; } = false;

        /// <summary>If true, this layer will be registered as a 'default' layer for all applications. Default true.</summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>A number used when ordering the layer entry in the list. Only to be used for layers that need to appear at the top/bottom of the list.</summary>
        public int Order { get; set; } = 0;
    }
}
