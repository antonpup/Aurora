using Aurora.EffectsEngine;
using Aurora.Profiles;
using CSScriptLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using Aurora.Settings.Layers;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;

namespace Aurora.Settings
{
    public class LightEventConfig : NotifyPropertyChangedEx
    {
        //TODO: Add NotifyPropertyChanged to properties
        public string[] ProcessNames { get; set; }

        public string Name { get; set; }

        public string ID { get; set; }

        public string AppID { get; set; }

        public Type SettingsType { get; set; } = typeof(ProfileSettings);

        public Type OverviewControlType { get; set; }

        public Type GameStateType { get; set; }

        public LightEvent Event { get; set; }

        public int? UpdateInterval { get; set; } = null;

        public string IconURI { get; set; }

        public HashSet<string> ExtraAvailableLayers { get; set; } = new HashSet<string>();

        protected LightEventType type;
        public LightEventType Type
        {
            get { return type; }
            protected set
            {
                object old = type;
                object newVal = value;
                type = value;
                InvokePropertyChanged(old, newVal);
            }
        }
    }

    public class ProfileManager : IInit, ILightEvent
    {
        #region Public Properties
        public bool Initialized { get; protected set; } = false;
        public ProfileSettings Settings { get; set; }
        public Dictionary<string, ProfileSettings> Profiles { get; set; } //Profile name, Profile Settings
        public Dictionary<string, Tuple<Type, Type>> ParameterLookup { get; set; } //Key = variable path, Value = {Return type, Parameter type}
        public bool HasLayers { get; set; }
        public event EventHandler ProfileChanged;
        public bool ScriptsLoaded { get; protected set; }
        public LightEventConfig Config { get; protected set; }
        public string ID { get { return Config.ID; } }
        public Type GameStateType { get { return Config.GameStateType; } }
        public bool IsEnabled { get { return Settings.IsEnabled; } }
        public event PropertyChangedExEventHandler PropertyChanged;
        protected LightEventType type;
        public LightEventType Type
        {
            get { return type; }
            protected set
            {
                object old = type;
                object newVal = value;
                type = value;
                InvokePropertyChanged(old, newVal);
            }
        }
        #endregion

        #region Internal Properties
        internal ImageSource Icon { get; set; }
        internal UserControl Control { get; set; }
        internal Dictionary<string, IEffectScript> EffectScripts { get; set; }
        #endregion

        #region Private Fields/Properties
        #endregion

        public ProfileManager(LightEventConfig config)
        {
            Config = config;

            Settings = (ProfileSettings)Activator.CreateInstance(config.SettingsType);
            config.Event.Profile = this;
            Profiles = new Dictionary<string, ProfileSettings>();
            EffectScripts = new Dictionary<string, IEffectScript>();
            ParameterLookup = Utils.GameStateUtils.ReflectGameStateParameters(config.GameStateType);
        }

        public bool Initialize()
        {
            if (Initialized)
                return Initialized;

            LoadProfiles();
            Initialized = true;
            return Initialized;
        }

        protected void InvokePropertyChanged(object oldValue, object newValue, [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedExEventArgs(propertyName, oldValue, newValue));
        }

        public virtual UserControl GetUserControl()
        {
            return Control ?? (Control = (UserControl)Activator.CreateInstance(this.Config.OverviewControlType, this));
        }

        public virtual ImageSource GetIcon()
        {
            return Icon ?? (Icon = new BitmapImage(new Uri(Config.IconURI, UriKind.Relative)));
        }

        public void SwitchToProfile(string profile_name)
        {
            if (Profiles.ContainsKey(profile_name))
            {
                Type setting_type = Profiles[profile_name].GetType();

                Settings = CloneSettings(Profiles[profile_name]);

                if (ProfileChanged != null)
                    ProfileChanged(this, new EventArgs());
            }
        }

        public void SaveDefaultProfile(string profile_name)
        {
            profile_name = GetValidFilename(profile_name);

            if (Profiles.ContainsKey(profile_name))
            {
                MessageBoxResult result = MessageBox.Show("Profile already exists. Would you like to replace it?", "Aurora", MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                    return;


                Profiles[profile_name] = CloneSettings(Settings);
            }
            else
            {
                Profiles.Add(profile_name, Settings);
            }

            SaveProfiles();
        }

        protected ProfileSettings CloneSettings(ProfileSettings settings)
        {
            return (ProfileSettings)JsonConvert.DeserializeObject(
                    JsonConvert.SerializeObject(settings, Config.SettingsType, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
                    Config.SettingsType,
                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All }
                    ); //I know this is bad. You can laugh at me for this one. :(
        }

        private string GetValidFilename(string filename)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                filename = filename.Replace(c, '_');

            return filename;
        }

        public virtual string GetProfileFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Profiles", Config.ID);
        }

        public void ResetProfile()
        {
            try
            {
                Settings = (ProfileSettings)Activator.CreateInstance(Config.SettingsType);

                this.InitalizeScriptSettings(Settings, true);

                ProfileChanged?.Invoke(this, new EventArgs());
            }
            catch (Exception exc)
            {
                Global.logger.LogLine(string.Format("Exception Resetting Profile, Exception: {0}", exc), Logging_Level.Error);
            }
        }

        internal ProfileSettings LoadProfile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string profile_content = File.ReadAllText(path, Encoding.UTF8);

                    if (!String.IsNullOrWhiteSpace(profile_content))
                    {
                        ProfileSettings prof = (ProfileSettings)JsonConvert.DeserializeObject(profile_content, Config.SettingsType, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All });
                        foreach (Layer lyr in prof.Layers)
                        {
                            lyr.AnythingChanged += this.SaveProfilesEvent;
                            lyr.SetProfile(this);
                        }

                        prof.Layers.CollectionChanged += (s, e) =>
                        {
                            if (e.NewItems != null)
                            {
                                foreach (Layer lyr in e.NewItems)
                                {
                                    if (lyr == null)
                                        continue;
                                    lyr.AnythingChanged += this.SaveProfilesEvent;
                                }
                            }
                            this.SaveProfiles();
                        };

                        return prof;
                    }
                }
            }
            catch (Exception exc)
            {
                Global.logger.LogLine(string.Format("Exception Loading Profile: {0}, Exception: {1}", path, exc), Logging_Level.Error);
                if (Path.GetFileNameWithoutExtension(path).Equals("default"))
                {
                    string newPath = path + ".corrupted";
                    File.Move(path, newPath);
                    this.SaveProfile(path, Settings);
                    MessageBox.Show($"Default profile for {this.Config.Name} could not be loaded.\nMoved to {newPath}, reset to default settings.\nException={exc}", "Error loading default profile", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //if (Global.isDebug)
                    //throw exc;
            }

            return null;
        }

        public bool RegisterEffect(string key, IEffectScript obj)
        {
            if (this.EffectScripts.ContainsKey(key))
            {
                Global.logger.LogLine(string.Format("Effect script with key {0} already exists!", key), Logging_Level.External);
                return false;
            }

            this.EffectScripts.Add(key, obj);

            return true;
        }

        public virtual void UpdateLights(EffectFrame frame)
        {
            this.Config.Event.UpdateLights(frame);
        }

        public virtual void SetGameState(IGameState state)
        {
            this.Config.Event.SetGameState(state);
        }

        public virtual void ResetGameState()
        {
            Config.Event.ResetGameState();
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

        protected void LoadScripts(string profiles_path, bool force = false)
        {
            if (!force && ScriptsLoaded)
                return;

            this.EffectScripts.Clear();

            string scripts_path = Path.Combine(profiles_path, Global.ScriptDirectory);
            if (!Directory.Exists(scripts_path))
                Directory.CreateDirectory(scripts_path);

            foreach (string script in Directory.EnumerateFiles(scripts_path, "*.*"))
            {
                try
                {
                    string ext = Path.GetExtension(script);
                    switch (ext)
                    {
                        case ".py":
                            var scope = Global.PythonEngine.ExecuteFile(script);
                            foreach (var v in scope.GetItems())
                            {
                                if (v.Value is IronPython.Runtime.Types.PythonType)
                                {
                                    Type typ = ((IronPython.Runtime.Types.PythonType)v.Value).__clrtype__();
                                    if (!typ.IsInterface && typeof(IEffectScript).IsAssignableFrom(typ))
                                    {
                                        IEffectScript obj = Global.PythonEngine.Operations.CreateInstance(v.Value) as IEffectScript;
                                        if (obj != null)
                                        {
                                            if (!(obj.ID != null && this.RegisterEffect(obj.ID, obj)))
                                                Global.logger.LogLine($"Script \"{script}\" must have a unique string ID variable for the effect {v.Key}", Logging_Level.External);
                                        }
                                        else
                                            Global.logger.LogLine($"Could not create instance of Effect Script: {v.Key} in script: \"{script}\"");
                                    }
                                }
                            }


                            break;
                        case ".cs":
                            System.Reflection.Assembly script_assembly = CSScript.LoadCodeFrom(script);
                            Type effectType = typeof(IEffectScript);
                            foreach (Type typ in script_assembly.ExportedTypes)
                            {
                                if (effectType.IsAssignableFrom(typ))
                                {
                                    IEffectScript obj = (IEffectScript)Activator.CreateInstance(typ);
                                    if (!(obj.ID != null && this.RegisterEffect(obj.ID, obj)))
                                        Global.logger.LogLine(string.Format("Script \"{0}\" must have a unique string ID variable for the effect {1}", script, typ.FullName), Logging_Level.External);
                                }
                            }

                            break;
                        default:
                            Global.logger.LogLine(string.Format("Script with path {0} has an unsupported type/ext! ({1})", script, ext), Logging_Level.External);
                            break;
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.LogLine(string.Format("An error occured while trying to load script {0}. Exception: {1}", script, exc, Logging_Level.External));
                    //Maybe MessageBox info dialog could be included.
                }
            }
        }

        protected void InitalizeScriptSettings(ProfileSettings profile_settings, bool ignore_removal = false)
        {
            foreach (string id in this.EffectScripts.Keys)
            {
                if (!profile_settings.ScriptSettings.ContainsKey(id))
                    profile_settings.ScriptSettings.Add(id, new ScriptSettings(this.EffectScripts[id]));
            }


            if (!ignore_removal)
            {
                foreach (string key in profile_settings.ScriptSettings.Keys.Where(s => !this.EffectScripts.ContainsKey(s)).ToList())
                {
                    profile_settings.ScriptSettings.Remove(key);
                }
            }
        }

        public virtual void LoadProfiles()
        {
            string profiles_path = GetProfileFolderPath();

            if (Directory.Exists(profiles_path))
            {
                this.LoadScripts(profiles_path);

                foreach (string profile in Directory.EnumerateFiles(profiles_path, "*.json", SearchOption.TopDirectoryOnly))
                {
                    string profile_name = Path.GetFileNameWithoutExtension(profile);
                    ProfileSettings profile_settings = LoadProfile(profile);

                    if (profile_settings != null)
                    {
                        this.InitalizeScriptSettings(profile_settings);

                        if (profile_name.Equals("default"))
                            Settings = profile_settings;
                        else
                        {
                            if (!Profiles.ContainsKey(profile_name))
                                Profiles.Add(profile_name, profile_settings);
                        }
                    }
                }
            }
            else
            {
                Global.logger.LogLine(string.Format("Profiles directory for {0} does not exist.", Config.Name), Logging_Level.Info, false);
            }
        }

        internal virtual void SaveProfile(string path, ProfileSettings profile)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                string content = JsonConvert.SerializeObject(profile, Formatting.Indented, settings);

                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                File.WriteAllText(path, content, Encoding.UTF8);

            }
            catch (Exception exc)
            {
                Global.logger.LogLine(string.Format("Exception Saving Profile: {0}, Exception: {1}", path, exc), Logging_Level.Error);
            }
        }

        public void SaveProfilesEvent(object sender, EventArgs e)
        {
            this.SaveProfiles();
        }

        public virtual void SaveProfiles()
        {
            try
            {
                string profiles_path = GetProfileFolderPath();

                if (!Directory.Exists(profiles_path))
                    Directory.CreateDirectory(profiles_path);

                SaveProfile(Path.Combine(profiles_path, "default.json"), Settings);

                foreach (KeyValuePair<string, ProfileSettings> kvp in Profiles)
                {
                    SaveProfile(Path.Combine(profiles_path, kvp.Key + ".json"), kvp.Value);
                }
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Exception during SaveProfiles, " + exc, Logging_Level.Error);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
