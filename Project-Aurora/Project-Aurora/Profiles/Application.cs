using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.EffectsEngine;
using Aurora.Scripts.VoronScripts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Utils;
using CSScriptLib;
using IronPython.Runtime.Types;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EnumConverter = Aurora.Utils.EnumConverter;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Aurora.Profiles
{
    public class LightEventConfig : INotifyPropertyChanged
    {
        public string[]? ProcessNames
        {
            get => _processNames;
            set
            {
                _processNames = value.Select(s => s.ToLower()).ToArray();
                ProcessNamesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs>? ProcessNamesChanged; 

        /// <summary>One or more REGULAR EXPRESSIONS that can be used to match the title of an application</summary>
        public string[]? ProcessTitles { get; set; }

        public string Name { get; set; }

        public string ID { get; set; }

        public string AppID { get; set; }

        public Type SettingsType { get; set; } = typeof(ApplicationSettings);

        public Type ProfileType { get; set; } = typeof(ApplicationProfile);

        public Type OverviewControlType { get; set; }

        public Type? GameStateType { get; set; }

        private readonly Lazy<LightEvent> _lightEvent;
        private string[]? _processNames;
        public LightEvent Event => _lightEvent.Value;

        public int? UpdateInterval { get; set; } = null;

        public string IconURI { get; set; }

        public HashSet<Type> ExtraAvailableLayers { get; } = new();

        public bool EnableByDefault { get; set; } = true;
        public bool EnableOverlaysByDefault { get; set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public LightEventConfig() : this(new Lazy<LightEvent>(() => new GameEvent_Generic()))
        {
        }

        public LightEventConfig(Lazy<LightEvent> lightEvent)
        {
            _lightEvent = lightEvent;
        }

        public LightEventConfig WithLayer<T>() where T : ILayerHandler {
            ExtraAvailableLayers.Add(typeof(T));
            return this;
        }
    }

    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
    public class Application : ObjectSettings<ApplicationSettings>, ILightEvent, INotifyPropertyChanged
    {
        #region Public Properties
        public bool Initialized { get; private set; }
        public bool Disposed { get; private set; }
        public ApplicationProfile? Profile { get; private set; }
        public ObservableCollection<ApplicationProfile> Profiles { get; set; }
        public GameStateParameterLookup? ParameterLookup { get; }
        public event EventHandler? ProfileChanged;
        public LightEventConfig Config { get; }
        public bool IsEnabled => Settings.IsEnabled;
        public bool IsOverlayEnabled => Settings.IsOverlayEnabled;

        #endregion

        #region Internal Properties
        protected ImageSource? icon;
        public virtual ImageSource Icon => icon ??= new BitmapImage(new Uri(Config.IconURI, UriKind.Relative));

        private UserControl? _control;
        public UserControl Control => _control ??= (UserControl)Activator.CreateInstance(Config.OverviewControlType, this);

        internal Dictionary<string, IEffectScript> EffectScripts { get; } = new();
        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected Application(LightEventConfig config)
        {
            Config = config;
            SettingsSavePath = Path.Combine(GetProfileFolderPath(), "settings.json");
            config.Event.Application = this;
            config.Event.ResetGameState();
            Profiles = new ObservableCollection<ApplicationProfile>();
            Profiles.CollectionChanged += (_, e) =>
            {
                if (e.Action != NotifyCollectionChangedAction.Add) return;
                foreach (ApplicationProfile prof in e.NewItems)
                {
                    prof.SetApplication(this);
                }
            };
            if (config.GameStateType != null)
                ParameterLookup = new GameStateParameterLookup(config.GameStateType);
        }

        public virtual bool Initialize()
        {
            if (Initialized)
                return Initialized;

            LoadSettings(Config.SettingsType);
            LoadProfiles();
            Initialized = true;
            return Initialized;
        }

        protected override void SettingsCreateHook() {
            Settings.IsEnabled = Config.EnableByDefault;
            Settings.IsOverlayEnabled = Config.EnableOverlaysByDefault;
        }

        /// <summary>Enables the use of a non-default layer for this application.</summary>
        protected void AllowLayer<T>() where T : ILayerHandler => Config.WithLayer<T>();

        /// <summary>Determines if the given layer handler type can be used by this application.
        /// This is the case either if it is a default handler or has explicitly been allowed for this application.</summary>
        public bool IsAllowedLayer(Type type) => Global.LightingStateManager.LayerHandlers.TryGetValue(type, out var def) &&
                                                 (def.IsDefault || Config.ExtraAvailableLayers.Contains(type));

        /// <summary>Gets a list of layers that are allowed to be used by this application.</summary>
        public IEnumerable<LayerHandlerMeta> AllowedLayers
            => Global.LightingStateManager.LayerHandlers.Values.Where(val => val.IsDefault || Config.ExtraAvailableLayers.Contains(val.Type));

        public void SwitchToProfile(ApplicationProfile? newProfileSettings)
        {
            if (Disposed)
                return;

            if (newProfileSettings == null || Profile == newProfileSettings) return;
            if (Profile != null)
            {
                SaveProfile();
                Profile.PropertyChanged -= Profile_PropertyChanged;
            }

            Profile = newProfileSettings;
            Settings.SelectedProfile = Path.GetFileNameWithoutExtension(Profile.ProfileFilepath);
            Profile.PropertyChanged += Profile_PropertyChanged;

            App.Current.Dispatcher.Invoke(() => ProfileChanged?.Invoke(this, EventArgs.Empty));
        }

        protected virtual ApplicationProfile CreateNewProfile(string profileName)
        {
            ApplicationProfile profile = (ApplicationProfile)Activator.CreateInstance(Config.ProfileType);
            profile.ProfileName = profileName;
            profile.ProfileFilepath = Path.Combine(GetProfileFolderPath(), GetUnusedFilename(GetProfileFolderPath(), profile.ProfileName) + ".json");
            return profile;
        }

        protected ApplicationProfile CreateDefaultProfile()
        {
            return AddNewProfile("default");
        }

        public ApplicationProfile NewDefaultProfile()
        {
            return AddNewProfile($"Profile {Profiles.Count + 1}");
        }

        public ApplicationProfile AddNewProfile(String profileName)
        {
            if (Disposed)
                return null;

            ApplicationProfile newProfile = CreateNewProfile(profileName);

            Profiles.Add(newProfile);

            SaveProfiles();

            SwitchToProfile(newProfile);

            return newProfile;
        }

        public void DeleteProfile(ApplicationProfile? profile)
        {
            if (Disposed)
                return;

            if (Profiles.Count == 1)
                return;

            if (profile == null || String.IsNullOrWhiteSpace(profile.ProfileFilepath)) return;
            int profileIndex = Profiles.IndexOf(profile);

            if (Profiles.Contains(profile))
                Profiles.Remove(profile);

            if (profile.Equals(Profile))
                SwitchToProfile(Profiles[Math.Min(profileIndex, Profiles.Count - 1)]);

            if (File.Exists(profile.ProfileFilepath))
            {
                try
                {
                    File.Delete(profile.ProfileFilepath);
                }
                catch (Exception exc)
                {
                    Global.logger.Error($"Could not delete profile with path \"{profile.ProfileFilepath}\"");
                    Global.logger.Error($"Exception: {exc}");
                }
            }

            SaveProfiles();
        }

        private string GetValidFilename(string filename)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                filename = filename.Replace(c, '_');

            return filename;
        }

        protected string GetUnusedFilename(string dir, string filename) {
            var safeName = GetValidFilename(filename);
            if (!File.Exists(Path.Combine(dir, safeName + ".json"))) return safeName;
            var i = 0;
            while (File.Exists(Path.Combine(dir, safeName + "-" + ++i + ".json")));
            return safeName + "-" + i;
        }

        public virtual string GetProfileFolderPath()
        {
            return Path.Combine(Global.AppDataDirectory, "Profiles", Config.ID);
        }

        public void ResetProfile()
        {
            if (Disposed)
                return;

            try
            {
                Profile?.Reset();

                ProfileChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                Global.logger.Error(string.Format("Exception Resetting Profile, Exception: {0}", exc));
            }
        }

        //hacky fix to sort out MoD profile type change
        protected ISerializationBinder binder = JSONUtils.SerializationBinder;
        internal ApplicationProfile? LoadProfile(string path)
        {
            if (Disposed)
                return null;

            try
            {
                if (File.Exists(path))
                {
                    string profileContent = File.ReadAllText(path, Encoding.UTF8);

                    if (!String.IsNullOrWhiteSpace(profileContent))
                    {
                        var jsonSerializerSettings = new JsonSerializerSettings
                        {
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            TypeNameHandling = TypeNameHandling.Objects,
                            //MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                            FloatParseHandling = FloatParseHandling.Double,
                            SerializationBinder = binder,
                            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
                            Error = LoadProfilesError
                        };
                        jsonSerializerSettings.Converters.Add(new EnumConverter());
                        jsonSerializerSettings.Converters.Add(new SingleToDoubleConverter());
                        jsonSerializerSettings.Converters.Add(new OverrideTypeConverter());
                        jsonSerializerSettings.Converters.Add(new TypeAnnotatedObjectConverter());
                        jsonSerializerSettings.Converters.Add(new DictionaryJsonConverterAdapter());
                        jsonSerializerSettings.Converters.Add(new ConcurrentDictionaryJsonConverterAdapter());
                        ApplicationProfile prof = (ApplicationProfile)JsonConvert.DeserializeObject(
                            profileContent,
                            Config.ProfileType,
                            jsonSerializerSettings);
                        prof.ProfileFilepath = path;

                        if (String.IsNullOrWhiteSpace(prof.ProfileName))
                            prof.ProfileName = Path.GetFileNameWithoutExtension(path);

                        // Initalises a collection, setting the layers' profile/application property and adding events to them and the collections to save to disk.
                        void InitialiseLayerCollection(ObservableCollection<Layer> collection) { 
                            foreach (Layer lyr in collection.ToList()) {
                                //Remove any Layers that have non-functional handlers
                                if (lyr.Handler == null || !Global.LightingStateManager.LayerHandlers.ContainsKey(lyr.Handler.GetType())) {
                                    prof.Layers.Remove(lyr);
                                    continue;
                                }

                                WeakEventManager<Layer, PropertyChangedEventArgs>.AddHandler(lyr, nameof(lyr.PropertyChanged), SaveProfilesEvent);
                            }

                            collection.CollectionChanged += (_, e) => {
                                if (e.NewItems != null)
                                    foreach (Layer lyr in e.NewItems)
                                        if (lyr != null)
                                            WeakEventManager<Layer, PropertyChangedEventArgs>.AddHandler(lyr, nameof(lyr.PropertyChanged), SaveProfilesEvent);
                                SaveProfiles();
                            };
                        }

                        // Call the above setup method on the regular layers and the overlay layers.
                        InitialiseLayerCollection(prof.Layers);
                        InitialiseLayerCollection(prof.OverlayLayers);

                        prof.PropertyChanged += Profile_PropertyChanged;
                        return prof;
                    }
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error(string.Format("Exception Loading Profile: {0}, Exception: {1}", path, exc));
                if (Path.GetFileNameWithoutExtension(path).Equals("default"))
                {
                    string newPath = path + ".corrupted";

                    int copy = 1;
                    while (File.Exists(newPath))
                    {
                        newPath = path + $"({copy++}).corrupted";
                    }

                    File.Move(path, newPath);
                    MessageBox.Show($"Default profile for {Config.Name} could not be loaded.\nMoved to {newPath}, reset to default settings.\nException={exc.Message}", "Error loading default profile", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return null;
        }

        protected virtual void LoadProfilesError(object? sender, ErrorEventArgs e)
        {
            if (e.CurrentObject != null)
            {
                if (e.CurrentObject.GetType().Equals(typeof(ObservableCollection<Layer>)))
                    e.ErrorContext.Handled = true;

                if (e.CurrentObject.GetType() == typeof(Layer) && e.ErrorContext.Member.Equals("Handler"))
                {
                    ((Layer)e.ErrorContext.OriginalObject).Handler = null;
                    e.ErrorContext.Handled = true;
                }
            } else if (e.ErrorContext.Path.Equals("$type") && e.ErrorContext.Member == null)
            {
                MessageBox.Show($"The profile type for {Config.Name} has been changed, your profile will be reset and your old one moved to have the extension '.corrupted', ignore the following error", "Profile type changed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Profile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ApplicationProfile)
                SaveProfile(sender as ApplicationProfile);
        }

        public bool RegisterEffect(string key, IEffectScript obj)
        {
            if (Disposed)
                return false;

            if (EffectScripts.ContainsKey(key))
            {
                Global.logger.Warning(string.Format("Effect script with key {0} already exists!", key));
                return false;
            }

            EffectScripts.Add(key, obj);

            return true;
        }

        public virtual void UpdateLights(EffectFrame frame)
        {
            if (Disposed)
                return;

            Config.Event.UpdateLights(frame);
        }

        public virtual void UpdateOverlayLights(EffectFrame frame) {
            if (Disposed) return;
            Config.Event.UpdateOverlayLights(frame);
        }

        public virtual void SetGameState(IGameState state)
        {
            if (Disposed)
                return;

            Config.Event.SetGameState(state);
        }

        public virtual void ResetGameState()
        {
            if (Disposed)
                return;

            Config.Event.ResetGameState();
        }
        
        public virtual void OnStart()
        {
            if (Disposed)
                return;

            Config.Event.OnStart();
        }

        public virtual void OnStop()
        {
            if (Disposed)
                return;

            Config.Event.OnStop();
        }

        public virtual void UpdateEffectScripts(Queue<EffectLayer> layers, IGameState state = null)
        {
            /*var _scripts = new Dictionary<string, ScriptSettings>(this.Settings.ScriptSettings).Where(s => s.Value.Enabled);

            foreach (KeyValuePair<string, ScriptSettings> scr in _scripts)
            {
                try
                {
                    dynamic script = this.EffectScripts[scr.Key];
                    dynamic script_layers = script.UpdateLights(scr.Value, state);
                    if (layers != null)
                    {
                        if (script_layers is EffectLayer)
                            layers.Enqueue(script_layers as EffectLayer);
                        else if (script_layers is EffectLayer[])
                        {
                            foreach (var layer in (script_layers as EffectLayer[]))
                                layers.Enqueue(layer);
                        }
                    }

                }
                catch (Exception exc)
                {
                    Global.logger.LogLine(string.Format("Script disabled! Effect script with key {0} encountered an error. Exception: {1}", scr.Key, exc), Logging_Level.External);
                    scr.Value.Enabled = false;
                    scr.Value.ExceptionHit = true;
                    scr.Value.Exception = exc;
                }
            }*/
        }

        protected void LoadScripts(string profilesPath, bool force = false)
        {
            if (!force && EffectScripts.Count != 0)
                return;

            EffectScripts.Clear();
            var voronsScriptPerf = new PerformanceEffect();
            var voronsScriptPing = new PingEffect();
            RegisterEffect(voronsScriptPerf.ID, voronsScriptPerf);
            RegisterEffect(voronsScriptPing.ID, voronsScriptPing);

            string scriptsPath = Path.Combine(profilesPath, Global.ScriptDirectory);
            if (!Directory.Exists(scriptsPath))
                Directory.CreateDirectory(scriptsPath);

            foreach (string script in Directory.EnumerateFiles(scriptsPath, "*.*"))
            {
                try
                {
                    string ext = Path.GetExtension(script);
                    bool anyLoaded = false;
                    switch (ext)
                    {
                        case ".py":
                            var scope = Global.PythonEngine.ExecuteFile(script);
                            foreach (var v in scope.GetItems())
                            {
                                if (v.Value is not PythonType) continue;
                                Type typ = ((PythonType)v.Value).__clrtype__();
                                if (typ.IsInterface || !typeof(IEffectScript).IsAssignableFrom(typ)) continue;
                                if (Global.PythonEngine.Operations.CreateInstance(v.Value) is IEffectScript obj)
                                {
                                    if (!(obj.ID != null && RegisterEffect(obj.ID, obj)))
                                        Global.logger.Warning($"Script \"{script}\" must have a unique string ID variable for the effect {v.Key}");
                                    else
                                        anyLoaded = true;
                                }
                                else
                                    Global.logger.Error($"Could not create instance of Effect Script: {v.Key} in script: \"{script}\"");
                            }


                            break;
                        case ".cs":
                            Assembly scriptAssembly = CSScript.RoslynEvaluator.CompileCode(File.ReadAllText(script));
                            Type effectType = typeof(IEffectScript);
                            foreach (Type typ in scriptAssembly.ExportedTypes)
                            {
                                if (!effectType.IsAssignableFrom(typ)) continue;
                                IEffectScript obj = (IEffectScript)Activator.CreateInstance(typ);
                                if (!(obj.ID != null && RegisterEffect(obj.ID, obj)))
                                    Global.logger.Warning("Script {Script} must have a unique string ID variable for the effect {FullName}",
                                        script, typ.FullName);
                                else
                                    anyLoaded = true;
                            }

                            break;
                        default:
                            Global.logger.Warning("Script with path {Script} has an unsupported type/ext! ({Ext})", script, ext);
                            continue;
                    }

                    if (!anyLoaded)
                        Global.logger.Warning($"Script \"{script}\": No compatible effects found. Does this script need to be updated?");
                }
                catch (Exception exc)
                {
                    Global.logger.Error(exc, "An error occured while trying to load script {Script}", script);
                    //Maybe MessageBox info dialog could be included.
                }
            }
        }

        public void ForceScriptReload() {
            LoadScripts(GetProfileFolderPath(), true);
        }

        protected void InitalizeScriptSettings(ApplicationProfile profileSettings, bool ignoreRemoval = false)
        {
            foreach (string id in EffectScripts.Keys)
            {
                if (!profileSettings.ScriptSettings.ContainsKey(id))
                    profileSettings.ScriptSettings.Add(id, new ScriptSettings());
            }


            if (!ignoreRemoval)
            {
                foreach (string key in profileSettings.ScriptSettings.Keys.Where(s => !EffectScripts.ContainsKey(s)).ToList())
                {
                    profileSettings.ScriptSettings.Remove(key);
                }
            }
        }

        public virtual void LoadProfiles()
        {
            string profilesPath = GetProfileFolderPath();

            if (Directory.Exists(profilesPath))
            {
                LoadScripts(profilesPath);

                foreach (string profile in Directory.EnumerateFiles(profilesPath, "*.json", SearchOption.TopDirectoryOnly))
                {

                    string profileFilename = Path.GetFileNameWithoutExtension(profile);
                    if (profileFilename.Equals(Path.GetFileNameWithoutExtension(SettingsSavePath)))
                        continue;
                    var profileSettings = LoadProfile(profile);

                    if (profileSettings != null)
                    {
                        InitalizeScriptSettings(profileSettings);

                        if (profileFilename.Equals(Settings.SelectedProfile))
                            Profile = profileSettings;

                        Profiles.Add(profileSettings);
                    }
                }
            }
            else
            {
                Global.logger.Information(string.Format("Profiles directory for {0} does not exist.", Config.Name));
            }

            if (Profile == null)
                SwitchToProfile(Profiles.FirstOrDefault());
            else
                Settings.SelectedProfile = Path.GetFileNameWithoutExtension(Profile.ProfileFilepath);

            if (Profile == null)
                CreateDefaultProfile();

        }

        public virtual void SaveProfile()
        {
            SaveProfile(Profile);
        }

        public virtual void SaveProfile(ApplicationProfile profile, string? path = null)
        {
            if (Disposed)
                return;
            try
            {
                path ??= Path.Combine(GetProfileFolderPath(), profile.ProfileFilepath);
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Binder = JSONUtils.SerializationBinder };
                string content = JsonConvert.SerializeObject(profile, Formatting.Indented, settings);

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, content, Encoding.UTF8);

            }
            catch (Exception exc)
            {
                Global.logger.Error(string.Format("Exception Saving Profile: {0}, Exception: {1}", path, exc));
            }
        }

        public void SaveProfilesEvent(object? sender, EventArgs e)
        {
            SaveProfiles();
        }

        public virtual void SaveProfiles()
        {
            if (Disposed)
                return;

            try
            {
                string profilesPath = GetProfileFolderPath();

                if (!Directory.Exists(profilesPath))
                    Directory.CreateDirectory(profilesPath);

                foreach (var profile in Profiles)
                {
                    SaveProfile(profile, Path.Combine(profilesPath, profile.ProfileFilepath));
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception during SaveProfiles, " + exc);
            }
        }

        public virtual void SaveAll()
        {
            if (Disposed || Config == null)
                return;

            SaveSettings(Config.SettingsType);
            SaveProfiles();
        }

        protected override void LoadSettings(Type settingsType)
        {
            base.LoadSettings(settingsType);

            Settings.PropertyChanged += (_, e) => {
                SaveSettings(Config.SettingsType);
            };
        }

        public virtual void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            Profile = null;

            foreach (var profile in Profiles)
                profile.Dispose();
            Profiles = null;
            _control = null;
            EffectScripts.Clear();
        }
    }
}