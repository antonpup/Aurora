using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using Newtonsoft.Json;

namespace Common.Devices;

[JsonObject]
public class DeviceConfig : INotifyPropertyChanged
{
    public const string FileName = "DeviceConfig.json";

    public static readonly string ConfigFile =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", FileName);

    public event PropertyChangedEventHandler? PropertyChanged;

    public Dictionary<string, Color> DeviceCalibrations { get; set; } = new();

    [JsonProperty("allow_peripheral_devices")]
    public bool AllowPeripheralDevices { get; set; } = true;

    [JsonProperty("devices_disable_keyboard")]
    public bool DevicesDisableKeyboard { get; set; }

    [JsonProperty("devices_disable_mouse")]
    public bool DevicesDisableMouse { get; set; }

    [JsonProperty("devices_disable_headset")]
    public bool DevicesDisableHeadset { get; set; }

    [JsonProperty("unified_hid_disabled")]
    public bool UnifiedHidDisabled { get; set; } = true;

    [JsonIgnore]
    public ObservableCollection<string> EnabledDevices
    {
        get => _enabledDevices ??= new ObservableCollection<string>(_defaultEnabledDevices);
        set => _enabledDevices = value;
    }

    private static readonly Dictionary<string,string> Migrations = new()
    {
        {"Aurora.Devices.AtmoOrbDevice.AtmoOrbDevice", "AtmoOrb"},
        {"Aurora.Devices.Bloody.BloodyDevice" , "Bloody"},
        {"Aurora.Devices.Clevo.ClevoDevice" , "Clevo Keyboard"},
        {"Aurora.Devices.Creative.SoundBlasterXDevice", "SoundBlasterX"},
        {"Aurora.Devices.Drevo.BloodyDevice" , "DrevoDevice"},
        {"Aurora.Devices.Dualshock4.DualshockDevice" , "Sony DualShock 4(PS4)"},
        {"Aurora.Devices.Ducky.DuckyDevice", "Ducky"},
        {"Aurora.Devices.LightFX.LightFxDevice" , "LightFX"},
        {"Aurora.Devices.Omen.OmenDevices" , "OMEN"},
        {"Aurora.Devices.Razer.RazerDevice", "Razer"},
        {"Aurora.Devices.Roccat.RoccatDevice" , "Roccat"},
        {"Aurora.Devices.SteelSeries.SteelSeriesDevice" , "SteelSeries"},
        {"Aurora.Devices.UnifiedHID.UnifiedHIDDevice" , "UnifiedHID"},
        {"Aurora.Devices.Vulcan.VulcanDevice" , "Vulcan"},
        {"Aurora.Devices.Wooting.WootingDevice", "Wooting"},
        {"Aurora.Devices.YeeLight.YeeLightDevice" , "YeeLight"},
        {"Aurora.Devices.OpenRGB", "OpenRGB (RGB.NET)"},

        {"Aurora.Devices.RGBNet.AsusDevice", "Asus (RGB.NET)"},
        {"Aurora.Devices.RGBNet.BloodyRgbNetDevice", "Bloody (RGB.NET)"},
        {"Aurora.Devices.RGBNet.CoolerMasterRgbNetDevice", "CoolerMaster (RGB.NET)"},
        {"Aurora.Devices.RGBNet.CorsairRgbNetDevice", "Corsair (RGB.NET)"},
        {"Aurora.Devices.RGBNet.LogitechRgbNetDevice", "Logitech (RGB.NET)"},
        {"Aurora.Devices.RGBNet.OpenRgbNetDevice", "OpenRGB (RGB.NET)"},
        {"Aurora.Devices.RGBNet.RazerRgbNetDevice", "Razer (RGB.NET)"},
        {"Aurora.Devices.RGBNet.SteelSeriesRgbNetDevice", "SteelSeries (RGB.NET)"},
        {"Aurora.Devices.RGBNet.WootingRgbNetDevice", "Wooting (RGB.NET)"},
        {"Aurora.Devices.RGBNet.YeelightRgbNetDevice", "Yeelight (RGB.NET)"},
    };

    private readonly List<string> _defaultEnabledDevices = new()
    {
        "Corsair (RGB.NET)",
        "Logitech (RGB.NET)",
        "OpenRGB (RGB.NET)",
    };

    [JsonProperty(nameof(EnabledDevices))]
    private ObservableCollection<string>? _enabledDevices;

    public VariableRegistry VarRegistry { get; set; } = new();

    public void OnPostLoad()
    {
        _enabledDevices ??= new ObservableCollection<string>(_defaultEnabledDevices);

        MigrateDevices();

        PrioritizeDevice("SteelSeries (RGB.NET)", "SteelSeries");
        PrioritizeDevice("Logitech (RGB.NET)", "Logitech");

        EnabledDevices.CollectionChanged += (_, _) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnabledDevices)));
    }

    private void MigrateDevices()
    {
        foreach (var loadedDeviceString in EnabledDevices.ToArray())
        {
            if (!Migrations.TryGetValue(loadedDeviceString, out var migratedValue)) continue;
            EnabledDevices.Remove(loadedDeviceString);
            EnabledDevices.Add(migratedValue);
        }
    }

    private void PrioritizeDevice(string primary, string secondary)
    {
        if (EnabledDevices.Contains(primary))
        {
            EnabledDevices.Remove(secondary);
        }
    }
}