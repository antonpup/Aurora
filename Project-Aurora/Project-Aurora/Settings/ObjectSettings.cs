using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class ObjectSettings<T>
    {
        protected string SettingsSavePath { get; set; }
        public T Settings { get; protected set; }

        public void SaveSettings()
        {
            this.SaveSettings(typeof(T));
        }

        protected void SaveSettings(Type settingsType)
        {
            if (Settings == null) {
                Settings = (T)Activator.CreateInstance(settingsType);
                SettingsCreateHook();
            }

            string dir = Path.GetDirectoryName(SettingsSavePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(SettingsSavePath, JsonConvert.SerializeObject(Settings, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented }));
        }

        /// <summary>A method that is called immediately after the settings being created. Can be overriden to provide specalized handling.</summary>
        protected virtual void SettingsCreateHook() { }

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
                    Global.logger.Error($"Exception occured while loading \"{this.GetType().Name}\" Settings.\nException:" + exc);
                    SaveSettings(settingsType);
                }
            }
            else
                SaveSettings(settingsType);
        }
    }
}
