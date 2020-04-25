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

namespace Aurora.Profiles
{
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
            public ProfilesManagerSettings()
            {

            }

            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {

            }
        }

    public class ProfileManager : ObjectSettings<ProfilesManagerSettings>, IInit, IProcessChanged
    {

        private ProcessManager ProcessManager;
        public LightingStateManager LightingStateManager;

        public Dictionary<string, ILightEvent> Events { get; private set; } = new Dictionary<string, ILightEvent> { { DesktopProfileName, new Desktop.Desktop() } };

        public Dictionary<string, LayerHandlerEntry> LayerHandlers { get; private set; } = new Dictionary<string, LayerHandlerEntry>();
        public List<string> DefaultLayerHandlers { get; private set; } = new List<string>();

        public Desktop.Desktop DesktopProfile { get { return (Desktop.Desktop)Events[DesktopProfileName]; } }

        private List<string> BackgroundProfile = new List<string>();
        private string CurrentProfile = DesktopProfileName;
        private bool PreviewMode = false;
        private const string DesktopProfileName = "desktop";

        private Dictionary<string, string> EventProcesses { get; set; } = new Dictionary<string, string>();
        private Dictionary<string, string> EventAppIDs { get; set; } = new Dictionary<string, string>();
        public string AdditionalProfilesPath = Path.Combine(Global.AppDataDirectory, "AdditionalProfiles");


        public ProfileManager()
        {
            Global.logger.LogLine("ProfileManager::ProfileManager()");
            SettingsSavePath = Path.Combine(Global.AppDataDirectory, "ProfilesSettings.json");
            ProcessManager = new ProcessManager(this);
            LightingStateManager = new LightingStateManager(DesktopProfile);
        }
        public bool Initialized { get; private set; }

        public bool Initialize()
        {
            if (Initialized)
                return true;

            #region Initiate Defaults
            RegisterEvents(new List<ILightEvent> {
                new Desktop.Desktop(),
                new Dota_2.Dota2(),
                new CSGO.CSGO(),
                new GTA5.GTA5(),
                new RocketLeague.RocketLeague(),
                new Borderlands2.Borderlands2(),
                new Overwatch.Overwatch(),
                new Payday_2.PD2(),
                new TheDivision.TheDivision(),
                new LeagueOfLegends.LoL(),
                new HotlineMiami.HotlineMiami(),
                new TheTalosPrinciple.TalosPrinciple(),
                new BF3.BF3(),
                new Blacklight.Blacklight(),
                new Magic_Duels_2012.MagicDuels2012(),
                new ShadowOfMordor.ShadowOfMordor(),
                new Serious_Sam_3.SSam3(),
                new DiscoDodgeball.DiscoDodgeballApplication(),
                new XCOM.XCOM(),
                new Evolve.Evolve(),
                new Metro_Last_Light.MetroLL(),
                new Guild_Wars_2.GW2(),
                new WormsWMD.WormsWMD(),
                new Blade_and_Soul.BnS(),
                new Skype.Skype(),
                new ROTTombRaider.ROTTombRaider(),
                new DyingLight.DyingLight(),
                new ETS2.ETS2(),
                new ATS.ATS(),
                new Move_or_Die.MoD(),
                new QuantumConumdrum.QuantumConumdrum(),
                new Battlefield1.Battlefield1(),
                new Dishonored.Dishonored(),
                new Witcher3.Witcher3(),
                new Minecraft.Minecraft(),
                new KillingFloor2.KillingFloor2(),
                new DOOM.DOOM(),
                new Factorio.Factorio(),
                new QuakeChampions.QuakeChampions(),
                new Diablo3.Diablo3(),
                new DeadCells.DeadCells(),
                new Subnautica.Subnautica(),
                new ResidentEvil2.ResidentEvil2(),
                new CloneHero.CloneHero(),
                new Osu.Osu(),
                new Slime_Rancher.Slime_Rancher(),
                new Terraria.Terraria(),
                new Discord.Discord(),
                new EliteDangerous.EliteDangerous()
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
                new LayerHandlerEntry("Razer", "Razer Chroma Layer", typeof(RazerLayerHandler)),
                new LayerHandlerEntry("Conditional", "Conditional Layer", typeof(ConditionalLayerHandler)),
                new LayerHandlerEntry("Comparison", "Comparison Layer", typeof(ComparisonLayerHandler)),
                new LayerHandlerEntry("Interactive", "Interactive Layer", typeof(InteractiveLayerHandler) ),
                new LayerHandlerEntry("ShortcutAssistant", "Shortcut Assistant Layer", typeof(ShortcutAssistantLayerHandler) ),
                new LayerHandlerEntry("Equalizer", "Audio Visualizer Layer", typeof(EqualizerLayerHandler) ),
                new LayerHandlerEntry("Ambilight", "Ambilight Layer", typeof(AmbilightLayerHandler) ),
                new LayerHandlerEntry("LockColor", "Lock Color Layer", typeof(LockColourLayerHandler) ),
                new LayerHandlerEntry("Glitch", "Glitch Effect Layer", typeof(GlitchLayerHandler) ),
                new LayerHandlerEntry("Animation", "Animation Layer", typeof(AnimationLayerHandler) ),
                new LayerHandlerEntry("ToggleKey", "Toggle Key Layer", typeof(ToggleKeyLayerHandler)),
                new LayerHandlerEntry("Timer", "Timer Layer", typeof(TimerLayerHandler)),
                new LayerHandlerEntry("Toolbar", "Toolbar Layer", typeof(ToolbarLayerHandler)),
                new LayerHandlerEntry("BinaryCounter", "Binary Counter Layer", typeof(BinaryCounterLayerHandler)),
                new LayerHandlerEntry("Particle", "Particle Layer", typeof(SimpleParticleLayerHandler)),
                new LayerHandlerEntry("InteractiveParticle", "Interactive Particle Layer", typeof(InteractiveParticleLayerHandler)),
                new LayerHandlerEntry("Radial", "Radial Layer", typeof(RadialLayerHandler))
            }, true);

            RegisterLayerHandler(new LayerHandlerEntry("WrapperLights", "Wrapper Lighting Layer", typeof(WrapperLightsLayerHandler)), false);
            
            #endregion

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

            LightingStateManager.Initialize();
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
        public void RegisterLayerHandlers(List<LayerHandlerEntry> layers, bool @default = true)
        {
            foreach (var layer in layers)
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
            LightingStateManager.Dispose();

            foreach (var app in this.Events)
                app.Value.Dispose();
        }

        public void FocusedApplicationChanged(string key)
        {
            //Global.logger.LogLine("Focused:FocusedApplicationChanged" + key);
            if (key == null)
            {
                LightingStateManager.ActiveProfileChanged(Events[CurrentProfile]);
                PreviewMode = false;
            }
            else
            {
                LightingStateManager.ActiveProfileChanged(Events[key]);
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
                LightingStateManager.ActiveProfileChanged(Events[CurrentProfile]);
            }
            else
            {
                LightingStateManager.ActiveProfileChanged(Events[DesktopProfileName]);
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
                LightingStateManager.RefreshOverLayerProfiles(Events.Values.Where(p => BackgroundProfile.Contains(p.Config.ID) && p.IsOverlayEnabled).ToList());
            }
            else
            {
                List<ILightEvent> defaultOverlayerProfile = DesktopProfile.IsOverlayEnabled ? new List<ILightEvent> { DesktopProfile } : new List<ILightEvent>();
                LightingStateManager.RefreshOverLayerProfiles(defaultOverlayerProfile);
            }
        }
    }
}

