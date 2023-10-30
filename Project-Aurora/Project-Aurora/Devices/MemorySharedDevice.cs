using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Modules.OnlineConfigs.Model;
using Aurora.Settings;
using Common;
using Common.Data;
using Common.Devices;

namespace Aurora.Devices;

public class MemorySharedDevice : IDevice
{
    public event EventHandler? Updated;

    private DeviceInformation DeviceInformation { get; set; }

    public string DeviceName => DeviceInformation.DeviceName;
    public string DeviceDetails => DeviceInformation.DeviceDetails;
    public string DeviceUpdatePerformance => DeviceInformation.DeviceUpdatePerformance;
    public bool isDoingWork => DeviceInformation.IsDoingWork;
    public bool IsInitialized => DeviceInformation.IsInitialized;
    public VariableRegistry RegisteredVariables { get; } = new();
    public bool IsRemappable => DeviceInformation.IsRemappable;

    private readonly MemorySharedStruct<DeviceInformation> _sharedDeviceInfo;
    private readonly MemorySharedArray<DeviceVariable> _deviceVariables;

    public MemorySharedDevice(string memoryName, VariableRegistry variableRegistry)
    {
        _sharedDeviceInfo = new MemorySharedStruct<DeviceInformation>(memoryName);
        _sharedDeviceInfo.Updated += OnSharedDeviceInfoOnUpdated;
        UpdateInformation();

        _deviceVariables = new MemorySharedArray<DeviceVariable>(memoryName + "-vars");
        _deviceVariables.Updated += DeviceVariablesOnUpdated;
        UpdateVariables();
    }

    private void OnSharedDeviceInfoOnUpdated(object? o, EventArgs eventArgs)
    {
        UpdateInformation();
    }

    private void DeviceVariablesOnUpdated(object? sender, EventArgs e)
    {
        UpdateVariables();
    }

    private void UpdateInformation()
    {
        var deviceInformation = _sharedDeviceInfo.ReadElement();
        DeviceInformation = deviceInformation;
        Updated?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateVariables()
    {
        foreach (var deviceVariable in _deviceVariables)
        {
            Func<byte[]?, object?> convert = deviceVariable.ValueType switch
            {
                VariableType.None => _ => new object(),
                VariableType.Boolean => data => BitConverter.ToBoolean(data),
                VariableType.Int => data => BitConverter.ToInt32(data),
                VariableType.Long => data => BitConverter.ToInt64(data),
                VariableType.Float => data => BitConverter.ToSingle(data),
                VariableType.Double => data => BitConverter.ToDouble(data),
                VariableType.String => _ => deviceVariable.StringValue,
                VariableType.Color => data => Color.FromArgb(BitConverter.ToInt32(data)),
                VariableType.DeviceKeys => data => (DeviceKeys)BitConverter.ToInt32(data),
            };

            RegisteredVariables.Register(deviceVariable.Name, new VariableRegistryItem(
                deviceVariable.ValueType == VariableType.String ? deviceVariable.StringValue : convert(deviceVariable.Value),
                convert(deviceVariable.Default),
                convert(deviceVariable.Max),
                convert(deviceVariable.Min),
                deviceVariable.Title,
                deviceVariable.Remark,
                (VariableFlags)deviceVariable.Flags
            ));
        }
    }

    public void RequestUpdate()
    {
        _sharedDeviceInfo.RequestUpdate();
    }

    public IEnumerable<string> GetDevices()
    {
        return DeviceInformation.Devices?.Split(Constants.StringSplit) ?? Enumerable.Empty<string>();
    }

    public DeviceTooltips Tooltips { get; set; } = new();
}