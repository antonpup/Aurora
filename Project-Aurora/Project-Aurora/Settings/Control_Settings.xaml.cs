using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Aurora.Devices.RGBNet.Config;
using Aurora.Modules.GameStateListen;
using Aurora.Modules.HardwareMonitor;
using Aurora.Modules.Razer;
using Aurora.Utils;
using Microsoft.Win32.TaskScheduler;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using Xceed.Wpf.Toolkit;
using Action = System.Action;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;
using Task = System.Threading.Tasks.Task;

namespace Aurora.Settings;

/// <summary>
/// Interaction logic for Control_Settings.xaml
/// </summary>
public partial class Control_Settings
{

    private readonly Task<PluginManager> _pluginManager;
    private readonly Task<AuroraHttpListener?> _httpListener;

    public Control_Settings(Task<RzSdkManager?> rzSdkManager, Task<PluginManager> pluginManager,
        Task<KeyboardLayoutManager> layoutManager, Task<AuroraHttpListener?> httpListener)
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

        DevicesAndWrappersTab.Content = new Control_SettingsDevicesAndWrappers(rzSdkManager, layoutManager);
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ctrlPluginManager.Host = _pluginManager.Result;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start("explorer", e.Uri.AbsoluteUri);
        e.Handled = true;
    }

    private void updates_check_Click(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        DesktopUtils.CheckUpdate();
    }

    private void btnShowLogsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button)
            Process.Start("explorer", Path.Combine(Global.LogsDirectory));
    }

    private void btnShowBitmapWindow_Click(object sender, RoutedEventArgs e) => Window_BitmapView.Open();

    private void btnShowGSILog_Click(object sender, RoutedEventArgs e) => Window_GSIHttpDebug.Open(_httpListener);

    private void btnDumpSensors_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(HardwareMonitor.TryDump()
            ? "Successfully wrote sensor info to logs folder"
            : "Error dumping file. Consult log for details.");
    }
}