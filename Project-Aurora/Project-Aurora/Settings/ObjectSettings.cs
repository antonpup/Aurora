using Newtonsoft.Json;
using System;
using System.IO;

namespace Aurora.Settings;

public class ObjectSettings<T>
{
    protected string SettingsSavePath { get; set; }
    public T Settings { get; protected set; }

    public void SaveSettings()
    {
        SaveSettings(typeof(T));
    }

    protected void SaveSettings(Type settingsType)
    {
        if (Settings == null) {
            Settings = (T)Activator.CreateInstance(settingsType);
            SettingsCreateHook();
        }

        var dir = Path.GetDirectoryName(SettingsSavePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(SettingsSavePath, JsonConvert.SerializeObject(Settings, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented }));
    }

    /// <summary>A method that is called immediately after the settings being created. Can be overriden to provide specalized handling.</summary>
    protected virtual void SettingsCreateHook() { }

    protected void LoadSettings()
    {
        LoadSettings(typeof(T));
    }

    protected virtual void LoadSettings(Type settingsType)
    {
        if (File.Exists(SettingsSavePath))
        {
            try
            {
                Settings = (T)JsonConvert.DeserializeObject(File.ReadAllText(SettingsSavePath), settingsType, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                if (Settings == null)
                {
                    SaveSettings(settingsType);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc, "Exception occured while loading \\\"{Name}\\\" Settings", GetType().Name);
                SaveSettings(settingsType);
            }
        }
        else
            SaveSettings(settingsType);
    }
}