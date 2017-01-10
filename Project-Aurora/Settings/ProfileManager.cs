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

namespace Aurora.Settings
{
    public class ProfileManager
    {
        #region Public Properties
        public string Name { get; set; }
        public string InternalName { get; set; }
        public string[] ProcessNames { get; set; }
        public string IconURI { get; set; }
        public ProfileSettings Settings { get; set; }
        public LightEvent Event { get; set; }
        public Dictionary<string, ProfileSettings> Profiles { get; set; } //Profile name, Profile Settings
        public Dictionary<string, Tuple<Type, Type>> ParameterLookup { get; set; } //Key = variable path, Value = {Return type, Parameter type}
        public bool HasLayers { get; set; }
        public HashSet<LayerType> AvailableLayers { get; set; }
        public event EventHandler ProfileChanged;
        #endregion

        #region Internal Properties
        internal ImageSource Icon { get; set; }
        internal UserControl Control { get; set; }
        internal Dictionary<string, dynamic> EffectScripts { get; set; }
        #endregion

        #region Private Fields/Properties
        protected Type SettingsType = typeof(ProfileSettings);
        protected Type ControlType = null;
        #endregion

        public ProfileManager(string name, string internal_name, string[] process_names, Type settings, Type control, LightEvent game_event)
        {
            Name = name;
            InternalName = internal_name;
            ProcessNames = process_names;
            Icon = null;
            Control = null;
            ControlType = control;
            SettingsType = settings;
            Settings = (ProfileSettings)Activator.CreateInstance(settings);
            game_event.Profile = this;
            Event = game_event;
            Profiles = new Dictionary<string, ProfileSettings>();
            EffectScripts = new Dictionary<string, dynamic>();
            if (game_event._game_state != null)
            {
                ParameterLookup = Utils.GameStateUtils.ReflectGameStateParameters(game_event._game_state.GetType());
            }
            else
                ParameterLookup = new Dictionary<string, Tuple<Type, Type>>();

            if (AvailableLayers == null)
                AvailableLayers = new HashSet<LayerType>();

            AvailableLayers.Add(LayerType.Default);
            AvailableLayers.Add(LayerType.Solid);
            AvailableLayers.Add(LayerType.SolidFilled);
            AvailableLayers.Add(LayerType.Gradient);
            AvailableLayers.Add(LayerType.GradientFill);
            AvailableLayers.Add(LayerType.Breathing);
            AvailableLayers.Add(LayerType.Blinking);
            AvailableLayers.Add(LayerType.Image);
            AvailableLayers.Add(LayerType.Percent);
            AvailableLayers.Add(LayerType.PercentGradient);
            AvailableLayers.Add(LayerType.Interactive);
            AvailableLayers.Add(LayerType.ShortcutAssistant);
            AvailableLayers.Add(LayerType.Equalizer);
            AvailableLayers.Add(LayerType.Ambilight);
            AvailableLayers.Add(LayerType.LockColor);

            LoadProfiles();
        }

        public ProfileManager(string name, string internal_name, string process_name, Type settings, Type control, LightEvent game_event) : this(name, internal_name, new string[] { process_name }, settings, control, game_event) { }

        public virtual UserControl GetUserControl()
        {
            return Control ?? (Control = (UserControl)Activator.CreateInstance(this.ControlType, this));
        }

        public virtual ImageSource GetIcon()
        {
            return Icon ?? (Icon = new BitmapImage(new Uri(IconURI, UriKind.Relative)));
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
                    JsonConvert.SerializeObject(settings, SettingsType, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
                    SettingsType,
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
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Profiles", InternalName);
        }

        public void ResetProfile()
        {
            try
            {
                Settings = (ProfileSettings)Activator.CreateInstance(SettingsType);

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
                        ProfileSettings prof = (ProfileSettings)JsonConvert.DeserializeObject(profile_content, SettingsType, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All });
                        foreach (Layer lyr in prof.Layers)
                        {
                            lyr.AnythingChanged += this.SaveProfilesEvent;
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
            }

            return null;
        }

        public void RegisterEffect(string key, dynamic obj)
        {
            if (this.EffectScripts.ContainsKey(key))
            {
                Global.logger.LogLine(string.Format("Effect script with key {0} already exists!", key), Logging_Level.External);
                return;
            }

            if (obj.GetType().GetMethod("UpdateLights") != null || obj.UpdateLights != null)
            {
                this.EffectScripts.Add(key, obj);
            }
            else
            {
                Global.logger.LogLine(string.Format("Effect script with key {0} is missing a method definition for 'update'", key), Logging_Level.External);
            }
        }

        public virtual void UpdateEffectScripts(Queue<EffectLayer> layers, IGameState state = null)
        {
            var _scripts = new Dictionary<string, ScriptSettings>(this.Settings.ScriptSettings).Where(s => s.Value.Enabled);

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
            }
        }

        protected void LoadScripts(string profiles_path)
        {
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
                            dynamic main_type;
                            if (scope.TryGetVariable("main", out main_type))
                            {
                                dynamic obj = Global.PythonEngine.Operations.CreateInstance(main_type);
                                if (obj.ID != null)
                                {
                                    this.RegisterEffect(obj.ID, obj);
                                }
                                else
                                    Global.logger.LogLine(string.Format("Script \"{0}\" does not have a public ID string variable", script), Logging_Level.External);
                            }
                            else
                                Global.logger.LogLine(string.Format("Script \"{0}\" does not contain a public 'main' class", script), Logging_Level.External);

                            break;
                        case ".cs":
                            System.Reflection.Assembly script_assembly = CSScript.LoadCodeFrom(script);
                            foreach (Type typ in script_assembly.ExportedTypes)
                            {
                                dynamic obj = Activator.CreateInstance(typ);
                                if (obj.ID != null)
                                {
                                    this.RegisterEffect(obj.ID, obj);
                                }
                                else
                                    Global.logger.LogLine(string.Format("Script \"{0}\" does not have a public ID string variable for the effect {1}", script, typ.FullName), Logging_Level.External);
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
                Global.logger.LogLine(string.Format("Profiles directory for {0} does not exist.", Name), Logging_Level.Info, false);
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
    }
}
