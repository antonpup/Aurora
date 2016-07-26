﻿using Aurora.EffectsEngine;
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

namespace Aurora.Settings
{
    public class ProfileManager
    {
        public string Name { get; set; }
        public string InternalName { get; set; }
        public string[] ProcessNames { get; set; }
        internal ImageSource Icon { get; set; }
        internal UserControl Control { get; set; }
        public ProfileSettings Settings { get; set; }
        public LightEvent Event { get; set; }
        public Dictionary<string, ProfileSettings> Profiles { get; set; } //Profile name, Profile Settings
        internal Dictionary<string, dynamic> EffectScripts { get; set; }

        public event EventHandler ProfileChanged;

        public ProfileManager(string name, string internal_name, string process_name, ProfileSettings settings, LightEvent game_event)
        {
            Name = name;
            InternalName = internal_name;
            ProcessNames = new string[] { process_name };
            Icon = null;
            Control = null;
            Settings = settings;
            Event = game_event;
            Profiles = new Dictionary<string, ProfileSettings>();
            EffectScripts = new Dictionary<string, dynamic>();
            LoadProfiles();
        }

        public ProfileManager(string name, string internal_name, string[] process_names, ProfileSettings settings, LightEvent game_event)
        {
            Name = name;
            InternalName = internal_name;
            ProcessNames = process_names;
            Icon = null;
            Control = null;
            Settings = settings;
            Event = game_event;
            Profiles = new Dictionary<string, ProfileSettings>();
            EffectScripts = new Dictionary<string, dynamic>();
            LoadProfiles();
        }

        public virtual UserControl GetUserControl()
        {
            return null;
        }

        public virtual ImageSource GetIcon()
        {
            return null;
        }

        public void SwitchToProfile(string profile_name)
        {
            if(Profiles.ContainsKey(profile_name))
            {
                Type setting_type = Profiles[profile_name].GetType();

                Settings = (ProfileSettings)JsonConvert.DeserializeObject(
                    JsonConvert.SerializeObject(Profiles[profile_name], setting_type, new JsonSerializerSettings { }),
                    setting_type,
                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace }
                    ); //I know this is bad. You can laugh at me for this one. :(

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

                if(result != MessageBoxResult.Yes)
                    return;

                Type setting_type = Settings.GetType();

                Profiles[profile_name] = (ProfileSettings)JsonConvert.DeserializeObject(
                    JsonConvert.SerializeObject(Settings, setting_type, new JsonSerializerSettings { }),
                    setting_type,
                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace }
                    ); //I know this is bad. You can laugh at me for this one. :(
            }
            else
            {
                Profiles.Add(profile_name, Settings);
            }

            SaveProfiles();
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
                Type setting_type = Settings.GetType();
                Settings = (ProfileSettings)Activator.CreateInstance(setting_type);

                if (ProfileChanged != null)
                    ProfileChanged(this, new EventArgs());
            }
            catch (Exception exc)
            {
                Global.logger.LogLine(string.Format("Exception Resetting Profile, Exception: {0}", exc), Logging_Level.Error);
            }
        }

        internal virtual ProfileSettings LoadProfile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string profile_content = File.ReadAllText(path, Encoding.UTF8);

                    if (!String.IsNullOrWhiteSpace(profile_content))
                        return JsonConvert.DeserializeObject<ProfileSettings>(profile_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                }
            }
            catch(Exception exc)
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
            
            if (obj.GetType().GetMethod("update") != null || obj.update != null) 
            {
                this.EffectScripts.Add(key, obj);
            }
            else
            {
                Global.logger.LogLine(string.Format("Effect script with key {0} is missing a method definition for 'update'", key), Logging_Level.External);
            }
        }

        public virtual void UpdateEffectScripts(Queue<EffectLayer> layers, GameState state = null)
        {
            foreach(KeyValuePair<string, ScriptSettings> scr in this.Settings.ScriptSettings.Where(s => s.Value.Enabled))
            {
                try
                {
                    dynamic script = this.EffectScripts[scr.Key];
                    EffectLayer layer = script.update(scr.Value, state);
                    if (layer != null)
                        layers.Enqueue(layer);
                }
                catch(Exception exc)
                {
                    Global.logger.LogLine(string.Format("Effect script with key {0} encountered an error.\n{1}\n{2}", scr.Key, exc.Message, exc.StackTrace), Logging_Level.External);
                }
            }
        }

        public virtual void LoadProfiles()
        {
            string profiles_path = GetProfileFolderPath();

            if (Directory.Exists(profiles_path))
            {
                string scripts_path;
                if (Directory.Exists(scripts_path = Path.Combine(profiles_path, Global.ScriptDirectory)))
                {
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
                             Global.logger.LogLine(string.Format("An error occured while trying to load script {0}.\n{1}\n{2}", script, exc.Message, exc.StackTrace, Logging_Level.External));
                            //Maybe MessageBox info dialog could be included.
                        }
                    }
                }


                foreach (string profile in Directory.EnumerateFiles(profiles_path, "*.json", SearchOption.TopDirectoryOnly))
                {
                    string profile_name = Path.GetFileNameWithoutExtension(profile);
                    ProfileSettings profile_settings = LoadProfile(profile);                   

                    if(profile_settings != null)
                    {
                        HashSet<string> old_ids = new HashSet<string>();
                        foreach(string id in this.EffectScripts.Keys)
                        {
                            if (!profile_settings.ScriptSettings.ContainsKey(id))
                                profile_settings.ScriptSettings.Add(id, new ScriptSettings(this.EffectScripts[id]));
                        }

                        foreach(string key in profile_settings.ScriptSettings.Keys.Where(s => !this.EffectScripts.ContainsKey(s)))
                        {
                            profile_settings.ScriptSettings.Remove(key);
                        }

                        if (profile_name.Equals("default"))
                            Settings = profile_settings;
                        else
                        {
                            if(!Profiles.ContainsKey(profile_name))
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
                string content = JsonConvert.SerializeObject(profile, Formatting.Indented);

                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                File.WriteAllText(path, content, Encoding.UTF8);

            }
            catch (Exception exc)
            {
                Global.logger.LogLine(string.Format("Exception Saving Profile: {0}, Exception: {1}", path, exc), Logging_Level.Error);
            }
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
            catch(Exception exc)
            {
                Global.logger.LogLine("Exception during SaveProfiles, " + exc, Logging_Level.Error);
            }
        }
    }
}
