using System.Text;
using AuroraDeviceManager.Devices.AtmoOrb;
using AuroraDeviceManager.Devices.RGBNet;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraDeviceManager;

public static class ConfigManager
{
    private static readonly string ConfigFile = Path.Combine(Global.AppDataDirectory, DeviceConfig.FileName);

    private static FileSystemWatcher? _configFileWatcher;

    private static readonly Dictionary<string, Type> Migrations = new()
    {
        { "Aurora.Devices.SteelSeriesHID.SteelSeriesHIDDevice", typeof(Devices.UnifiedHID.UnifiedHIDDevice) },
        { "Aurora.Devices.Asus.AsusDevice", typeof(AsusDevice) },
        { "Aurora.Devices.CoolerMaster.CoolerMasterDevice", typeof(CoolerMasterRgbNetDevice) },
        { "Aurora.Devices.AtmoOrbDevice.AtmoOrbDevice", typeof(AtmoOrbDevice) },
    };

    public static async Task Load()
    {
        _configFileWatcher = new FileSystemWatcher(Global.AppDataDirectory)
        {
            Filter = "Config.json",
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        await TryLoad();
        
        _configFileWatcher.Changed += ConfigFileWatcherOnChanged;
    }

    private static void ConfigFileWatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        Thread.Sleep(200);
        try
        {
            TryLoad().Wait();
        }
        catch (Exception exc)
        {
            Global.Logger.Error(exc, "Failed to load configuration");
        }
    }

    private static async Task TryLoad()
    {
        DeviceConfig config;

        if (!File.Exists(ConfigFile))
            config = CreateDefaultConfigurationFile();
        else
        {
            var content = await File.ReadAllTextAsync(ConfigFile, Encoding.UTF8);
            config = string.IsNullOrWhiteSpace(content)
                ? CreateDefaultConfigurationFile()
                : JsonConvert.DeserializeObject<DeviceConfig>(content,
                    new JsonSerializerSettings
                    {
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        Error = DeserializeErrorHandler
                    })!;
        }
        config.OnPostLoad();

        Global.DeviceConfig = config;
        Global.DeviceManager.RegisterVariables();
    }

    private static void DeserializeErrorHandler(object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
    {
        if (e.ErrorContext.Path == "$type")
        {
            e.ErrorContext.Handled = true;
            return;
        }
        
        if (e.CurrentObject is not ICollection<Type> dd)
        {
            return;
        }

        foreach (var (key, value) in Migrations)
        {
            if (!e.ErrorContext.Error.Message.Contains(key)) continue;
            dd.Add(value);
            e.ErrorContext.Handled = true;
            return;
        }
    }

    private static DeviceConfig CreateDefaultConfigurationFile()
    {
        return new DeviceConfig();
    }
}