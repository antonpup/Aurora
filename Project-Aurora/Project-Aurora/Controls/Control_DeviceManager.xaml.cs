using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for Control_DeviceManager.xaml
    /// </summary>
    public partial class Control_DeviceManager : UserControl
    {
        public Devices.DeviceManager deviceManager = App.Core.dev_manager;
        public Control_DeviceManager()
        {
            InitializeComponent();

            deviceManager.NewDevicesInitialized += Dev_manager_NewDevicesInitialized;
        }

        private void Dev_manager_NewDevicesInitialized(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                    {
                        int attempts = deviceManager.RetryAttempts;

                        if (attempts <= 0)
                            this.txtBlk_retries.Visibility = Visibility.Collapsed;
                        else
                            this.txtBlk_retries.Text = $"Retries Remaining: {deviceManager.RetryAttempts}";

                        UpdateControls();
                    });
            }
            catch (Exception ex)
            {
                App.logger.Warn(ex.ToString());
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            this.lstDevices.ItemsSource = deviceManager.Devices;
            this.lstDevices.Items.Refresh();
        }

        private void btnRestartAll_Click(object sender, RoutedEventArgs e)
        {
            deviceManager.Shutdown();
            deviceManager.Initialize();
            UpdateControls();
        }
    }
}
