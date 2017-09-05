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
        public Control_DeviceManager()
        {
            InitializeComponent();

            Global.dev_manager.NewDevicesInitialized += Dev_manager_NewDevicesInitialized;
        }

        private void Dev_manager_NewDevicesInitialized(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                    {
                        int attempts = Global.dev_manager.RetryAttempts;

                        if (attempts <= 0)
                            this.txtBlk_retries.Visibility = Visibility.Collapsed;
                        else
                            this.txtBlk_retries.Text = $"Retries Remaining: {Global.dev_manager.RetryAttempts}";

                        UpdateControls();
                    });
            }
            catch (Exception ex)
            {
                Global.logger.Warn(ex.ToString());
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
            this.lstDevices.ItemsSource = Global.dev_manager.Devices;
            this.lstDevices.Items.Refresh();
        }

        private void btnRestartAll_Click(object sender, RoutedEventArgs e)
        {
            Global.dev_manager.Shutdown();
            Global.dev_manager.Initialize();
            UpdateControls();
        }
    }
}
