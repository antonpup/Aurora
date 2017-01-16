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
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register("Device", typeof(Device), typeof(Control_DeviceItem));

        public Device Device
        {
            get
            {
                return (Device)GetValue(DeviceProperty);
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
            update_controls_timer.Elapsed += Update_controls_timer_Elapsed;
            update_controls_timer.Start();
        }

        private void Update_controls_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => { if(IsVisible) UpdateControls(); });
            }
            catch (Exception ex)
            {
                Global.logger.LogLine(ex.ToString(), Logging_Level.Warning);
            }
        }

        private void btnToggleOnOff_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                if(Device.IsInitialized())
                    Device.Shutdown();
                else
                    Device.Initialize();

                UpdateControls();
            }
        }

        private void btnToggleEnableDisable_Click(object sender, RoutedEventArgs e)
        {
            if (Global.Configuration.devices_disabled.Contains(Device.GetType()))
                Global.Configuration.devices_disabled.Remove(Device.GetType());
            else
                Global.Configuration.devices_disabled.Add(Device.GetType());

            UpdateControls();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (Device.IsInitialized())
                btnToggleOnOff.Content = "Stop";
            else
                btnToggleOnOff.Content = "Start";

            txtblk_DeviceStatus.Text = Device.GetDeviceDetails().TrimEnd(' ');
            txtblk_DevicePerformance.Text = Device.GetDeviceUpdatePerformance();

            if(Device is Devices.ScriptedDevice.ScriptedDevice)
                btnToggleEnableDisable.IsEnabled = false;
            else
            {
                if (Global.Configuration.devices_disabled.Contains(Device.GetType()))
                {
                    btnToggleEnableDisable.Content = "Enable";
                    btnToggleOnOff.IsEnabled = false;
                }
                else
                {
                    btnToggleEnableDisable.Content = "Disable";
                    btnToggleOnOff.IsEnabled = true;
                }
            }

            if(Device.GetRegisteredVariables().GetRegisteredVariableKeys().Count() == 0)
                btnViewOptions.IsEnabled = false;
        }

        private void btnViewOptions_Click(object sender, RoutedEventArgs e)
        {
            Window_VariableRegistryEditor options_window = new Window_VariableRegistryEditor();
            options_window.Title = $"{Device.GetDeviceName()} - Options";
            options_window.SizeToContent = SizeToContent.WidthAndHeight;
            options_window.RegisteredVariables = Device.GetRegisteredVariables();

            options_window.ShowDialog();
        }
    }
}
