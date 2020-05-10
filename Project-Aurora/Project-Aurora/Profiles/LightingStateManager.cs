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
using System.Windows.Data;
using System.Globalization;
using System.Windows;
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

    public class LightingStateManager : ObjectSettings<ProfilesManagerSettings>, IInit, IProcessChanged
    {

        private ProcessManager ProcessManager;
        public LightingEngine LightingEngine;

        public Dictionary<string, ILightEvent> Events { get; private set; } = new Dictionary<string, ILightEvent> { { DesktopProfileName, new Desktop.Desktop() } };
        public Dictionary<Type, LayerHandlerMeta> LayerHandlers { get; private set; } = new Dictionary<Type, LayerHandlerMeta>();

        public Desktop.Desktop DesktopProfile { get { return (Desktop.Desktop)Events[DesktopProfileName]; } }

        private List<string> BackgroundProfile = new List<string>();
        private string CurrentProfile = DesktopProfileName;
        private bool PreviewMode = false;
        private const string DesktopProfileName = "desktop";

        private Dictionary<string, string> EventProcesses { get; set; } = new Dictionary<string, string>();
        private Dictionary<string, string> EventAppIDs { get; set; } = new Dictionary<string, string>();
        public string AdditionalProfilesPath = Path.Combine(Global.AppDataDirectory, "AdditionalProfiles");

        public LightingStateManager()
        {
            Global.logger.LogLine("ProfileManager::ProfileManager()");
            SettingsSavePath = Path.Combine(Global.AppDataDirectory, "ProfilesSettings.json");
            ProcessManager = new ProcessManager(this);
            LightingEngine = new LightingEngine(DesktopProfile);
        }
        public bool Initialized { get; private set; }

        public bool Initialize()
        {
            if (Initialized)
                return true;

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

            this.LoadPlugins();

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

            LightingEngine.Initialize();
            OpenBackgroundProcess(DesktopProfileName);

            //Global.logger.LogLine("ProcessManager::Start()");
            ProcessManager.Start();

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

            foreach (string key in Global.Configuration.ProfileOrder.ToList())
            {
                if (!Events.ContainsKey(key) || !(Events[key] is Application))
                    Global.Configuration.ProfileOrder.Remove(key);
            }

            Global.Configuration.ProfileOrder.Remove(DesktopProfileName);
            Global.Configuration.ProfileOrder.Insert(0, DesktopProfileName);
        }

        public void SaveAll()
        {
            SaveSettings();

            foreach (var profile in Events)
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

            ProcessManager.SubsribeForChange(@event.Config);

            if (@event.Config.ProcessNames != null)
            {
                foreach (string exe in @event.Config.ProcessNames)
                {
                    if (!exe.Equals(key))
                        EventProcesses.Add(exe.ToLower(), key);
                }
            }

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
                    gameState = (IGameState)Activator.CreateInstance(profile.Config.GameStateType, gs.json);
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

        public void Dispose()
        {
            ProcessManager.Finish();
            LightingEngine.Dispose();

            foreach (var app in this.Events)
                app.Value.Dispose();
        }

        public void FocusedApplicationChanged(string key)
        {
            //Global.logger.LogLine("Focused:FocusedApplicationChanged" + key);
            if (key == null)
            {
                LightingEngine.ActiveProfileChanged(Events[CurrentProfile]);
                PreviewMode = false;
            }
            else
            {
                LightingEngine.ActiveProfileChanged(Events[key]);
                PreviewMode = true;
            }

        }

        public void ActiveProcessChanged(string key)
        {
            key = key ?? DesktopProfileName;
            //Global.logger.LogLine("Focused:ActiveProcessChanged" + key);

            if (Global.Configuration.excluded_programs.Contains(key))
            {
                return;
            }
            CurrentProfile = Events.Keys.Contains(key) ? key : DesktopProfileName;
            
            if (Events[CurrentProfile].IsEnabled)
            {
                LightingEngine.ActiveProfileChanged(Events[CurrentProfile]);
            }
            else
            {
                LightingEngine.ActiveProfileChanged(Events[DesktopProfileName]);
            }

            //(Global.Configuration.allow_wrappers_in_background && Global.net_listener != null && Global.net_listener.IsWrapperConnected && ((tempProfile = GetProfileFromProcessName(Global.net_listener.WrappedProcess)) != null) && tempProfile.Config.Type == LightEventType.Normal && tempProfile.IsEnabled)
        }

        public void OpenBackgroundProcess(string key)
        {
            //Global.logger.LogLine("Focused:OpenBackgroundProcess" + key);
            if (Global.Configuration.excluded_programs.Contains(key))
            {
                return;
            }
            BackgroundProfile.Add(key);
            Events[key].OnStart();
            RefreshBackroundProfile();
        }

        public void CloseBackgroundProcess(string key)
        {
            //Global.logger.LogLine("Focused:CloseBackgroundProcess" + key);
            if(BackgroundProfile.Contains(key))
            {
                BackgroundProfile.Remove(key);
                Events[key].OnStop();
                RefreshBackroundProfile();
            }
            
        }

        private void RefreshBackroundProfile()
        {
            if (Global.Configuration.OverlaysInPreview || PreviewMode)
            {
                LightingEngine.RefreshOverLayerProfiles(Events.Values.Where(p => BackgroundProfile.Contains(p.Config.ID) && p.IsOverlayEnabled).ToList());
            }
            else
            {
                List<ILightEvent> defaultOverlayerProfile = DesktopProfile.IsOverlayEnabled ? new List<ILightEvent> { DesktopProfile } : new List<ILightEvent>();
                LightingEngine.RefreshOverLayerProfiles(defaultOverlayerProfile);
            }
        }
    }

    /// <summary>
    /// POCO that stores data about a type of layer.
    /// </summary>
    public class LayerHandlerMeta
    {

        /// <summary>Creates a new LayerHandlerMeta object from the given meta attribute and type.</summary>
        public LayerHandlerMeta(Type type, LayerHandlerMetaAttribute attribute)
        {
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
    public class LayerHandlerMetaAttribute : Attribute
    {
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

