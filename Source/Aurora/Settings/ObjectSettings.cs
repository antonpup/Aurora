using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Aurora.Settings
{
    public abstract class ObjectSettings<T> where T : SettingsProfile
    {
        private string _settingsSavePath = "";
        protected string SettingsSavePath
        {
            get => _settingsSavePath;
            set => _settingsSavePath = Path.Combine(GlobalConstants.DataStorageDirectory, value);
        }
        public T Settings { get; protected set; }

        public virtual void SaveSettings()
        {
            this.SaveSettings(typeof(T));
        }

        protected virtual void SaveSettings(Type settingsType)
        {
            if (Settings == null)
                Settings = (T)Activator.CreateInstance(settingsType);

            string dir = Path.GetDirectoryName(SettingsSavePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(SettingsSavePath, JsonConvert.SerializeObject(Settings, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented }));
        }

        protected void LoadSettings()
        {
            this.LoadSettings(typeof(T));
        }

        protected virtual void LoadSettings(Type settingsType)
        {
            if (File.Exists(SettingsSavePath))
            {
                try
                {
                    Settings = (T)JsonConvert.DeserializeObject(File.ReadAllText(SettingsSavePath), settingsType, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                }
                catch (Exception exc)
                {
                    Logger.Log.Error($"Exception occured while loading \"{this.GetType().Name}\" Settings.\nException:" + exc);
                    SaveSettings(settingsType);
                }
            }
            else
                SaveSettings(settingsType);
        }
    }
}
