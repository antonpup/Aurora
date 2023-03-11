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
        lstDevices.ItemsSource = Global.dev_manager.DeviceContainers;
        lstDevices.Items.Refresh();
    }

    private void btnRestartAll_Click(object sender, RoutedEventArgs e)
    {
        Global.dev_manager.ShutdownDevices();
        Global.dev_manager.InitializeDevices();
        UpdateControls();
    }

    private void btnCalibrate_Click(object sender, RoutedEventArgs e)
    {
        new Control_DeviceCalibration().Show();
    }
}