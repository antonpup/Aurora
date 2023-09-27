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

    public async Task Initialize()
    {
        var deviceManager = await _deviceManager;
        await UpdateControls();
        deviceManager.DevicesUpdated += async (_, _) =>
        {
            await UpdateControls();
        };
    }

    private async void Control_DeviceManager_Loaded(object? sender, RoutedEventArgs e)
    {
        await UpdateControls();
    }

    private async Task UpdateControls()
    {
        var deviceContainers = (await _deviceManager).DeviceContainers;
        Dispatcher.Invoke(() =>
        {
            LstDevices.ItemsSource = deviceContainers;
            LstDevices.Items.Refresh();
        });
    }

    private async void btnRestartAll_Click(object? sender, RoutedEventArgs e)
    {
        var devManager = await _deviceManager;
        await devManager.ShutdownDevices();
        await devManager.InitializeDevices();
        await UpdateControls();
    }

    private void btnCalibrate_Click(object? sender, RoutedEventArgs e)
    {
        new Control_DeviceCalibration(_deviceManager).Show();
    }
}