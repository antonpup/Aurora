using Aurora.Devices;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Control_DeviceItem.xaml
    /// </summary>
    public partial class Control_DeviceItem : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register("Device", typeof(DeviceContainer), typeof(Control_DeviceItem));

        public DeviceContainer Device
        {
            get
            {
                return (DeviceContainer)GetValue(DeviceProperty);
            }
            set
            {
                SetValue(DeviceProperty, value);

                UpdateControls();
            }
        }

        public Control_DeviceItem()
        {
            InitializeComponent();

            Timer update_controls_timer = new Timer(1000); //Update every second
            WeakEventManager<Timer, ElapsedEventArgs>.AddHandler(
                update_controls_timer,
                nameof(update_controls_timer.Elapsed),
                Update_controls_timer_Elapsed);
            update_controls_timer.Start();
        }

        private void Update_controls_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => { if (IsVisible) UpdateControls(); });
            }
            catch (Exception ex)
            {
                Global.logger.Warn(ex.ToString());
            }
        }

        private void btnToggleOnOff_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                btnStart.Content = "Working...";
                btnStart.IsEnabled = false;
                var device = Device;
                Task.Run(async () =>
                {
                    await device.ActionLock.WaitAsync();

                    if (device.Device.IsInitialized)
                    {
                        await Global.dev_manager.DisableDevice(device.Device);
                    } else { 
                        await Global.dev_manager.EnableDevice(device.Device);
                    }

                    device.ActionLock.Release();
                });
            }
        }

        private void btnToggleEnableDisable_Click(object sender, RoutedEventArgs e)
        {
            if (Global.Configuration.DevicesDisabled.Contains(Device.Device.GetType()))
                Global.Configuration.DevicesDisabled.Remove(Device.Device.GetType());
            else
            {
                Global.Configuration.DevicesDisabled.Add(Device.Device.GetType());
                var device = Device;
                Task.Run(async () =>
                {
                    await device.ActionLock.WaitAsync();
                    if (device.Device.IsInitialized)
                    {
                        await device.Device.Shutdown();
                    }

                    device.ActionLock.Release();
                });
            }

            UpdateControls();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (Device.Device.IsInitializing)
            {
                btnStart.Content = "Working...";
                btnStart.IsEnabled = false;
            }
            else if (Device.Device.IsInitialized)
            {
                btnStart.Content = "Stop";
                btnStart.IsEnabled = true;
            }
            else
            {
                btnStart.Content = "Start";
                btnStart.IsEnabled = true;
            }

            deviceName.Text = Device.Device.DeviceName;
            deviceDetails.Text = Device.Device.DeviceDetails;
            devicePerformance.Text = Device.Device.DeviceUpdatePerformance;

            if (Device is Devices.ScriptedDevice.ScriptedDevice)
                btnEnable.IsEnabled = false;
            else
            {
                if (Global.Configuration.DevicesDisabled.Contains(Device.Device.GetType()))
                {
                    btnEnable.Content = "Enable";
                    btnStart.IsEnabled = false;
                }
                else
                {
                    btnEnable.Content = "Disable";
                    btnStart.IsEnabled = true;
                }
            }

            if (Device.Device.RegisteredVariables.GetRegisteredVariableKeys().Count() == 0)
                btnOptions.IsEnabled = false;
        }

        private void btnViewOptions_Click(object sender, RoutedEventArgs e)
        {
            Window_VariableRegistryEditor options_window = new Window_VariableRegistryEditor();
            options_window.Title = $"{Device.Device.DeviceName} - Options";
            options_window.SizeToContent = SizeToContent.WidthAndHeight;
            options_window.VarRegistryEditor.RegisteredVariables = Device.Device.RegisteredVariables;
            options_window.Closing += (_sender, _eventArgs) =>
            {
                ConfigManager.Save(Global.Configuration);
            };

            options_window.ShowDialog();
        }
    }
}
