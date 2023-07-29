using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Aurora.Controls;
using Aurora.Devices;
using Aurora.Devices.RGBNet.Config;
using Aurora.Modules.GameStateListen;
using Aurora.Modules.HardwareMonitor;
using Aurora.Utils;
using RazerSdkWrapper;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace Aurora.Settings;

/// <summary>
/// Interaction logic for Control_Settings.xaml
/// </summary>
public partial class Control_Settings
{
    private readonly Task<PluginManager> _pluginManager;
    private readonly Task<AuroraHttpListener?> _httpListener;
    private readonly Control_SettingsDevicesAndWrappers _devicesAndWrappers;

    public Control_Settings(Task<RzSdkManager?> rzSdkManager, Task<PluginManager> pluginManager,
        Task<KeyboardLayoutManager> layoutManager, Task<AuroraHttpListener?> httpListener, Task<DeviceManager> deviceManager)
    {
        _pluginManager = pluginManager;
        _httpListener = httpListener;
        InitializeComponent();

        var v = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        var o = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).CompanyName;
        var r = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName;

        lblVersion.Content = (v[0].ToString().Length > 0 ? "" : "beta ") + $"{v} {o}/{r}";
        LnkIssues.NavigateUri = new Uri($"https://github.com/{o}/{r}/issues/");
        LnkRepository.NavigateUri = new Uri($"https://github.com/{o}/{r}");
        LnkContributors.NavigateUri = new Uri($"https://github.com/{o}/{r}#contributors-");

        _devicesAndWrappers = new Control_SettingsDevicesAndWrappers(rzSdkManager, layoutManager, deviceManager);
        var controlDeviceManager = new Control_DeviceManager(deviceManager);
        var deviceMapping = new DeviceMapping(deviceManager);
        
        DevicesAndWrappersTab.Content = _devicesAndWrappers;
        DeviceManagerTab.Content = controlDeviceManager;
        RemapDevicesTab.Content = deviceMapping;
        
        _devicesAndWrappers.BeginInit();
        controlDeviceManager.BeginInit();
        deviceMapping.BeginInit();
    }

    public async Task Initialize()
    {
        await _devicesAndWrappers.Initialize();
    }

    private async void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        ctrlPluginManager.Host = await _pluginManager;
    }

    private void Hyperlink_RequestNavigate(object? sender, RequestNavigateEventArgs e)
    {
        Process.Start("explorer", e.Uri.AbsoluteUri);
        e.Handled = true;
    }

    private async void updates_check_Click(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        await DesktopUtils.CheckUpdate();
    }

    private void btnShowLogsFolder_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button)
            Process.Start("explorer", Path.Combine(Global.LogsDirectory));
    }

    private void btnShowBitmapWindow_Click(object? sender, RoutedEventArgs e) => Window_BitmapView.Open();

    private void btnShowGSILog_Click(object? sender, RoutedEventArgs e) => Window_GSIHttpDebug.Open(_httpListener);

    private void btnDumpSensors_Click(object? sender, RoutedEventArgs e)
    {
        MessageBox.Show(HardwareMonitor.TryDump()
            ? "Successfully wrote sensor info to logs folder"
            : "Error dumping file. Consult log for details.");
    }
}