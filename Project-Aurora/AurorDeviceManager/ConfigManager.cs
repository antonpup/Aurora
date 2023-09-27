using System.Text;
using AurorDeviceManager.Devices.AtmoOrb;
using AurorDeviceManager.Devices.RGBNet;
using AurorDeviceManager.Settings;
using AurorDeviceManager.Utils;
using Newtonsoft.Json;

namespace AurorDeviceManager;

public static class ConfigManager
{
    private static readonly string ConfigPath = Path.Combine(Global.AppDataDirectory, "Config");
    private const string ConfigExtension = ".json";
    
    private static readonly string ConfigFile = ConfigPath + ConfigExtension;
    private static FileSystemWatcher? _configFileWatcher;

    private static readonly Dictionary<string, Type> Migrations = new()
    {
        { "Aurora.Devices.SteelSeriesHID.SteelSeriesHIDDevice", typeof(Devices.UnifiedHID.UnifiedHIDDevice) },
        { "Aurora.Devices.Asus.AsusDevice", typeof(AsusDevice) },
        { "Aurora.Devices.CoolerMaster.CoolerMasterDevice", typeof(CoolerMasterRgbNetDevice) },
        { "Aurora.Devices.AtmoOrbDevice.AtmoOrbDevice", typeof(AtmoOrbDevice) },
    };

    private static readonly List<string> Discard = new()
    {
        "Aurora.Devices.Creative.SoundBlasterXDevice",
        "Aurora.Devices.NZXT.NZXTDevice",
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
        Configuration config;

        if (!File.Exists(ConfigFile))
            config = CreateDefaultConfigurationFile();
        else
        {
            var content = await File.ReadAllTextAsync(ConfigFile, Encoding.UTF8);
            config = string.IsNullOrWhiteSpace(content)
                ? CreateDefaultConfigurationFile()
                : JsonConvert.DeserializeObject<Configuration>(content,
                    new JsonSerializerSettings
                    {
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        TypeNameHandling = TypeNameHandling.None,
                        Error = DeserializeErrorHandler
                    })!;
        }

        Global.Configuration = config;
        Global.DeviceManager.RegisterVariables();
        await Global.DeviceManager.InitializeDevices();
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

        if (Discard.Any(s => e.ErrorContext.Error.Message.Contains(s)))
        {
            e.ErrorContext.Handled = true;
        }

        if (e.ErrorContext.Error.Message.Contains("to type 'System.Type'. Path 'DevicesDisabled.$values"))
        {
            e.ErrorContext.Handled = true;
        }
    }

    private static Configuration CreateDefaultConfigurationFile()
    {
        return new Configuration();
    }
}