using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Aurora.Utils;
using Common.Devices;
using Newtonsoft.Json;

namespace Aurora.Settings;

public static class ConfigManager
{
    private static readonly Dictionary<string, long> LastSaveTimes = new();
    private const long SaveInterval = 300L;

    public static Configuration Load()
    {
        var config = TryLoad();

        config.OnPostLoad();
        config.PropertyChanged += (_, _) => { Save(config, Configuration.ConfigFile); };

        return config;
    }

    private static Configuration TryLoad()
    {
        try
        {
            return Parse();
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception during ConfigManager.Load(). Error: ");
            var result = MessageBox.Show(
                $"Exception loading configuration. Error: {exc.Message}\r\n\r\n" +
                $" Do you want to reset settings? (this won't reset profiles).",
                "Aurora - Error",
                MessageBoxButton.YesNo
            );

            if (result == MessageBoxResult.Yes)
            {
                return CreateDefaultConfigurationFile();
            }

            App.ForceShutdownApp(-1);
            throw new Exception();
        }
    }

    private static Configuration Parse()
    {
        if (!File.Exists(Configuration.ConfigFile))
            return CreateDefaultConfigurationFile();
        
        var content = File.ReadAllText(Configuration.ConfigFile, Encoding.UTF8);
        return string.IsNullOrWhiteSpace(content)
            ? CreateDefaultConfigurationFile()
            : JsonConvert.DeserializeObject<Configuration>(content,
                new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                })!;
    }

    public static DeviceConfig LoadDeviceConfig()
    {
        DeviceConfig config;
        try
        {
            config = TryLoadDevice();
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception loading DeviceConfig. Error: ");
            config = new DeviceConfig();
        }

        config.OnPostLoad();
        config.PropertyChanged += (_, _) => { Save(config, Configuration.ConfigFile); };

        return config;
    }

    private static DeviceConfig TryLoadDevice()
    {
        if (!File.Exists(DeviceConfig.ConfigFile))
            return MigrateDeviceConfig();
        
        var content = File.ReadAllText(DeviceConfig.ConfigFile, Encoding.UTF8);
        return JsonConvert.DeserializeObject<DeviceConfig>(content,
            new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            }) ?? MigrateDeviceConfig();
    }

    public static void Save(object configuration, string path)
    {
        var currentTime = Time.GetMillisecondsSinceEpoch();

        if (LastSaveTimes.TryGetValue(path, out var lastSaveTime))
        {
            if (lastSaveTime + SaveInterval > currentTime)
                return;
        }

        LastSaveTimes[path] = currentTime;

        var content = JsonConvert.SerializeObject(configuration, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto, SerializationBinder = new AuroraSerializationBinder()
        });

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content, Encoding.UTF8);
    }

    private static Configuration CreateDefaultConfigurationFile()
    {
        Global.logger.Information("Creating default configuration");
        var config = new Configuration();
        Save(config, Configuration.ConfigFile);
        return config;
    }

    private static DeviceConfig MigrateDeviceConfig()
    {
        Global.logger.Information("Migrating default device configuration");
        var content = File.ReadAllText(Configuration.ConfigFile, Encoding.UTF8);
        var config = JsonConvert.DeserializeObject<DeviceConfig>(content,
            new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                SerializationBinder = new AuroraSerializationBinder(),
            }) ?? new DeviceConfig();
        Save(config, DeviceConfig.ConfigFile);
        return config;
    }
}