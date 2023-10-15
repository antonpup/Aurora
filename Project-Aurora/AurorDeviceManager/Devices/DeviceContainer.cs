using System.ComponentModel;
using System.Drawing;
using Amib.Threading;
using AurorDeviceManager.Devices.RGBNet;
using Common;
using Common.Data;
using Common.Devices;
using Common.Devices.RGBNet;

namespace AurorDeviceManager.Devices;

public sealed class DeviceContainer : IDisposable
{
    public IDevice Device { get; }

    private readonly SmartThreadPool _worker = new(1000, 1)
    {
        Concurrency = 1,
        MaxQueueLength = 1
    };

    private DeviceColorComposition _currentComp = new(new Dictionary<DeviceKeys, Color>());

    private readonly SemaphoreSlim _actionLock = new(1);
    private readonly Action _updateAction;
    
    private readonly MemorySharedStruct<DeviceInformation> _deviceInformation;
    private readonly MemorySharedArray<DeviceVariable> _deviceVariables;

    public string SharedObjectName => Device.DeviceName;

    public DeviceContainer(IDevice device)
    {
        Device = device;
        _worker.Name = device.DeviceName + " Thread";
        var args = new DoWorkEventArgs(null);
        _updateAction = () =>
        {
            WorkerOnDoWork(args).Wait();
        };

        _deviceInformation = new MemorySharedStruct<DeviceInformation>(SharedObjectName, GetSharedDeviceInformation());
        _deviceInformation.UpdateRequested += (_, _) =>
        {
            UpdateSharedMemory();
        };

        var deviceVariables = CreateDeviceVariables();
        _deviceVariables = new MemorySharedArray<DeviceVariable>(SharedObjectName + "-vars", deviceVariables.Count);
        _deviceVariables.WriteCollection(deviceVariables);

        if (device is RgbNetDevice rgbNetDeviceProvider)
        {
            foreach (var rgbNetDevice in rgbNetDeviceProvider.Devices)
            {
                foreach (var led in rgbNetDevice)
                {
                    
                }
            }
        }
    }

    private List<DeviceVariable> CreateDeviceVariables()
    {
        var deviceVariables = new List<DeviceVariable>();
        foreach (var (varName, item) in Device.RegisteredVariables.Variables)
        {
            var vt = item.Default switch
            {
                bool => VariableType.Boolean,
                int => VariableType.Int,
                long => VariableType.Long,
                float => VariableType.Float,
                double => VariableType.Double,
                string => VariableType.String,
                Color => VariableType.Color, //TODO check color type
                DeviceKeys => VariableType.DeviceKeys,
                _ => VariableType.None,
            };
            Func<object?, byte[]?> convert = vt switch
            {
                VariableType.None => _ => null,
                VariableType.Boolean => data => data == null ? null : BitConverter.GetBytes((long)((bool)data ? 1 : 0)),
                VariableType.Float => data => data == null ? null : BitConverter.GetBytes((double)(float)data),
                VariableType.Double => data => data == null ? null : BitConverter.GetBytes((double)data),
                VariableType.Int => data => data == null ? null : BitConverter.GetBytes((long)(int)data),
                VariableType.Long => data => data == null ? null : BitConverter.GetBytes((long)data),
                VariableType.DeviceKeys => data => data == null ? null : BitConverter.GetBytes((long)(int)data),
                VariableType.String => _ => null,
                VariableType.Color => data => data == null ? null : BitConverter.GetBytes((long)((Color)data).ToArgb()),
            };

            var variable = new DeviceVariable(
                Device.DeviceName, varName,
                convert(item.Value),
                convert(item.Default),
                convert(item.Min),
                convert(item.Max),
                item.Title, item.Remark, (int)item.Flags, vt,
                vt == VariableType.String ? (string)item.Value : ""
            );
            deviceVariables.Add(variable);
        }

        return deviceVariables;
    }

    private Task WorkerOnDoWork(DoWorkEventArgs doWorkEventArgs)
    {
        using (_actionLock)
        {
            if (Device is { IsInitialized: false, isDoingWork: false })
            {
                Device.Initialize();
            }

            try
            {
                Device.UpdateDevice(_currentComp, doWorkEventArgs);
            }
            catch (Exception e)
            {
                Global.Logger.Error(e, "Device update thread error");
            }
        }
        return Task.CompletedTask;
    }

    public void UpdateDevice(Dictionary<DeviceKeys, Color> keyColors)
    {
        _currentComp.KeyColors = keyColors;
        if (_worker.WaitingCallbacks == 0 && !Device.isDoingWork)
        {
            _worker.QueueWorkItem(_updateAction);
        }else if (_worker.IsIdle || _worker.ActiveThreads == 0)
        {
            _worker.Start();
        }
    }

    public async Task EnableDevice()
    {
        using (_actionLock)
        {
            var initTask = Device.Initialize();
            UpdateSharedMemory();
            if (await initTask)
            {
                Global.Logger.Information("[Device][{DeviceName}] Initialized Successfully", Device.DeviceName);
            }
            else
            {
                Global.Logger.Information("[Device][{DeviceName}] Failed to initialize", Device.DeviceName);
            }
        }
        UpdateSharedMemory();
    }

    public async Task DisableDevice()
    {
        using (_actionLock)
        {
            await Device.ShutdownDevice();
            Global.Logger.Information("[Device][{DeviceName}] Shutdown", Device.DeviceName);
        }
        UpdateSharedMemory();
    }

    public void UpdateVariables()
    {
        Global.Configuration.VarRegistry.Combine(Device.RegisteredVariables);

        var deviceVariables = CreateDeviceVariables();
        _deviceVariables.WriteCollection(deviceVariables);
    }

    public void Dispose()
    {
        _worker.Shutdown(250);
        _worker.Dispose();
    }

    private void UpdateSharedMemory()
    {
        _deviceInformation.WriteObject(GetSharedDeviceInformation());
    }

    private DeviceInformation GetSharedDeviceInformation()
    {
        return new DeviceInformation(Device.DeviceName,
            Device.DeviceDetails,
            Device.DeviceUpdatePerformance,
            Device.isDoingWork,
            Device.IsInitialized,
            Device.GetDevices(),
            Device is RgbNetDevice
        );
    }
}