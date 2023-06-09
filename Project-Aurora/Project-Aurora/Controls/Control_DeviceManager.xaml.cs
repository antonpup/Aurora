using System.Threading.Tasks;
using System.Windows;

namespace Aurora.Controls;

/// <summary>
/// Interaction logic for Control_DeviceManager.xaml
/// </summary>
public partial class Control_DeviceManager
{
    public Control_DeviceManager()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateControls();
    }

    private void UpdateControls()
    {
        lstDevices.ItemsSource = Global.dev_manager.DeviceContainers;
        lstDevices.Items.Refresh();
    }

        private void btnRestartAll_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                await Global.dev_manager.ShutdownDevices();
                await Global.dev_manager.InitializeDevices();
                Dispatcher.Invoke(UpdateControls);
            });
        }

    private void btnCalibrate_Click(object sender, RoutedEventArgs e)
    {
        new Control_DeviceCalibration().Show();
    }
}