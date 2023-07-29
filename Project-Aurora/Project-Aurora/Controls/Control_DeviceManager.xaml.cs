using System.Threading.Tasks;
using System.Windows;
using Aurora.Devices;

namespace Aurora.Controls;

/// <summary>
/// Interaction logic for Control_DeviceManager.xaml
/// </summary>
public partial class Control_DeviceManager
{
    private readonly Task<DeviceManager> _deviceManager;

    public Control_DeviceManager(Task<DeviceManager> deviceManager)
    {
        _deviceManager = deviceManager;
        
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        UpdateControls();
    }

    private void UpdateControls()
    {
        lstDevices.ItemsSource = _deviceManager.Result.DeviceContainers;
        lstDevices.Items.Refresh();
    }

        private void btnRestartAll_Click(object? sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                var devManager = await _deviceManager;
                await devManager.ShutdownDevices();
                await devManager.InitializeDevices();
                Dispatcher.Invoke(UpdateControls);
            });
        }

    private void btnCalibrate_Click(object? sender, RoutedEventArgs e)
    {
        new Control_DeviceCalibration(_deviceManager).Show();
    }
}