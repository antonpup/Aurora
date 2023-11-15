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
using Aurora.Modules;
using Aurora.Modules.GameStateListen;
using Aurora.Modules.HardwareMonitor;
using RazerSdkReader;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace Aurora.Settings.Controls;

/// <summary>
/// Interaction logic for Control_Settings.xaml
/// </summary>
public partial class Control_Settings
{
    private readonly Task<PluginManager> _pluginManager;
    private readonly Task<AuroraHttpListener?> _httpListener;
    private readonly Control_SettingsDevicesAndWrappers _devicesAndWrappers;
    private readonly Control_DeviceManager _controlDeviceManager;
    private readonly DeviceMapping _deviceMapping;

    public Control_Settings(Task<ChromaReader?> rzSdkManager, Task<PluginManager> pluginManager,
        Task<KeyboardLayoutManager> layoutManager, Task<AuroraHttpListener?> httpListener, Task<DeviceManager> deviceManager, Task<IpcListener?> ipcListener)
    {
        _pluginManager = pluginManager;
        _httpListener = httpListener;
        InitializeComponent();

        var v = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "?";
        var o = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).CompanyName ?? "Aurora-RGB";
        var r = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName ?? "Aurora";

        lblVersion.Content = (v[0].ToString().Length > 0 ? "" : "beta ") + $"{v} {o}/{r}";
        LnkIssues.NavigateUri = new Uri($"https://github.com/{o}/{r}/issues/");
        LnkRepository.NavigateUri = new Uri($"https://github.com/{o}/{r}");
        LnkContributors.NavigateUri = new Uri($"https://github.com/{o}/{r}#contributors-");

        _devicesAndWrappers = new Control_SettingsDevicesAndWrappers(rzSdkManager, layoutManager, deviceManager);
        _controlDeviceManager = new Control_DeviceManager(deviceManager);
        _deviceMapping = new DeviceMapping(deviceManager, ipcListener);
        
        DevicesAndWrappersTab.Content = _devicesAndWrappers;
        DeviceManagerTab.Content = _controlDeviceManager;
        RemapDevicesTab.Content = _deviceMapping;
        
        _devicesAndWrappers.BeginInit();
        _controlDeviceManager.BeginInit();
        _deviceMapping.BeginInit();
    }

    public async Task Initialize()
    {
        await _devicesAndWrappers.Initialize();
        await _controlDeviceManager.Initialize();
        await _deviceMapping.Initialize();
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
        await UpdateModule.CheckUpdate();
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