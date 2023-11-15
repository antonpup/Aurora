using Aurora.Settings;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Aurora.Devices;
using Aurora.Settings.Controls;
using Common.Devices;

namespace Aurora.Controls;

/// <summary>
/// Interaction logic for Control_DeviceItem.xaml
/// </summary>
public partial class Control_DeviceItem
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register(nameof(Device), typeof(DeviceContainer), typeof(Control_DeviceItem));

    private readonly Timer _updateControlsTimer;

    public DeviceContainer Device
    {
        get => (DeviceContainer)GetValue(DeviceProperty);
        set => SetValue(DeviceProperty, value);
    }

    public Control_DeviceItem()
    {
        InitializeComponent();

        _updateControlsTimer = new Timer(1000);
        _updateControlsTimer.Elapsed += Update_controls_timer_Elapsed;
    }

    private void Update_controls_timer_Elapsed(object? sender, EventArgs e)
    {
        if (IsVisible)
        {
            Dispatcher.Invoke(() =>
            {
                var memorySharedDevice = Device.Device;
                Task.Run(() =>
                {
                    memorySharedDevice.RequestUpdate();
                });
            });
        }
    }

    private void btnToggleOnOff_Click(object? sender, EventArgs e)
    {
        btnStart.Content = "Working...";
        btnStart.IsEnabled = false;
        var device = Device;
        Task.Run(async () =>
        {
            if (device.Device.IsInitialized)
            {
                await device.DisableDevice();
            }
            else
            {
                await device.EnableDevice();
            }
        });
    }

    private void btnToggleEnableDisable_Click(object? sender, EventArgs e)
    {
        if (!Global.DeviceConfigration.EnabledDevices.Contains(Device.Device.DeviceName))
        {
            Global.DeviceConfigration.EnabledDevices.Add(Device.Device.DeviceName);
        }
        else
        {
            Global.DeviceConfigration.EnabledDevices.Remove(Device.Device.DeviceName);
        }
        btnStart.Content = "Working...";
        btnStart.IsEnabled = false;
        btnEnable.IsEnabled = false;

        var device = Device;
        Task.Run(async () =>
        {
            if (device.Device.IsInitialized)
            {
                await device.DisableDevice();
            }
            else
            {
                await device.EnableDevice();
            }
            ConfigManager.Save(Global.DeviceConfigration, DeviceConfig.ConfigFile);
        });
    }

    private void UserControl_Loaded(object? sender, EventArgs e)
    {
        Device.Device.Updated += OnDeviceOnUpdated;
        Dispatcher.Invoke(() =>
        {
            try
            {
                if (IsVisible)
                {
                    UpdateStatic();
                    UpdateDynamic();
                }
            }
            catch (Exception ex)
            {
                Global.logger.Warning(ex, "DeviceItem update error:");
            }
        });
    }

    private void Control_DeviceItem_OnUnloaded(object? sender, EventArgs e)
    {
        Device.Device.Updated -= OnDeviceOnUpdated;
        _updateControlsTimer.Stop();
    }

    private void OnDeviceOnUpdated(object? o, EventArgs eventArgs)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                if (IsVisible) UpdateDynamic();
            }
            catch (Exception ex)
            {
                Global.logger.Warning(ex, "DeviceItem update error:");
            }
        });
    }

    private void UpdateStatic()
    {
        Beta.Visibility = Device.Device.Tooltips.Beta ? Visibility.Visible : Visibility.Hidden;
            
        var infoTooltip = Device.Device.Tooltips.Info;
        if (infoTooltip != null)
        {
            InfoTooltip.HintTooltip = infoTooltip;
            InfoTooltip.Visibility = Visibility.Visible;
        }
        else
        {
            InfoTooltip.Visibility = Visibility.Hidden;
        }

        var sdkLink = Device.Device.Tooltips.SdkLink;
        if (sdkLink != null)
        {
            SdkLink.MouseDoubleClick -= SdkLink_Clicked;
            SdkLink.MouseDoubleClick += SdkLink_Clicked;
            SdkLink.Visibility = Visibility.Visible;
        }
        else
        {
            SdkLink.MouseDoubleClick -= SdkLink_Clicked;
            SdkLink.Visibility = Visibility.Hidden;
        }

        Recommended.Visibility = Device.Device.Tooltips.Recommended ? Visibility.Visible : Visibility.Hidden;

        btnOptions.IsEnabled = Device.Device.RegisteredVariables.Count != 0;
    }

    private void UpdateDynamic()
    {
        if (Device.Device.isDoingWork)
        {
            btnStart.Content = "Working...";
            btnStart.IsEnabled = false;
            btnEnable.IsEnabled = false;
        }
        else if (Device.Device.IsInitialized)
        {
            btnStart.Content = "Stop";
            btnStart.IsEnabled = true;
            btnEnable.IsEnabled = true;
            _updateControlsTimer.Start();
        }
        else
        {
            btnStart.Content = "Start";
            btnStart.IsEnabled = true;
            btnEnable.IsEnabled = true;
        }

        deviceName.Text = Device.Device.DeviceName;
        deviceDetails.Text = Device.Device.DeviceDetails;
        devicePerformance.Text = Device.Device.DeviceUpdatePerformance;

        if (!Global.DeviceConfigration.EnabledDevices.Contains(Device.Device.DeviceName))
        {
            btnEnable.Content = "Enable";
            btnStart.IsEnabled = false;
        }
        else if (!Device.Device.isDoingWork)
        {
            btnEnable.Content = "Disable";
            btnStart.IsEnabled = true;
        }
    }

    private void SdkLink_Clicked(object? sender, MouseButtonEventArgs e)
    {
        var sdkLink = Device.Device.Tooltips.SdkLink;
        if (sdkLink != null)
        {
            System.Diagnostics.Process.Start("explorer", sdkLink);
        }
    }

    private void btnViewOptions_Click(object? sender, RoutedEventArgs e)
    {
        var optionsWindow = new Window_VariableRegistryEditor
        {
            Title = $"{Device.Device.DeviceName} - Options",
            SizeToContent = SizeToContent.WidthAndHeight,
            VarRegistryEditor =
            {
                RegisteredVariables = Device.Device.RegisteredVariables
            }
        };
        optionsWindow.Closing += (_, _) =>
        {
            ConfigManager.Save(Global.DeviceConfigration, DeviceConfig.ConfigFile);
        };

        optionsWindow.ShowDialog();
    }
}