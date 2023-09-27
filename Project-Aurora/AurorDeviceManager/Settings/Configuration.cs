using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using Newtonsoft.Json;

namespace AurorDeviceManager.Settings;

public class Configuration : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public Dictionary<string, Color> DeviceCalibrations { get; set; } = new();
    
    [JsonProperty("allow_peripheral_devices")] public bool AllowPeripheralDevices { get; set; } = true;
    [JsonProperty("devices_disable_keyboard")] public bool DevicesDisableKeyboard { get; set; }
    [JsonProperty("devices_disable_mouse")] public bool DevicesDisableMouse { get; set; }
    [JsonProperty("devices_disable_headset")] public bool DevicesDisableHeadset { get; set; }

    [JsonProperty("unified_hid_disabled")] public bool UnifiedHidDisabled { get; set; } = true;
    public ObservableCollection<string> EnabledDevices { get; set; }
    
    public VariableRegistry VarRegistry { get; set; } = new VariableRegistry();
}