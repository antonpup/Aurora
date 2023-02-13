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
    private const string StartupTaskId = "AuroraStartup";
    private readonly bool _componentsInitialized;

    private readonly Task<PluginManager> _pluginManager;
    private readonly Task<KeyboardLayoutManager> _layoutManager;
    private readonly Task<AuroraHttpListener?> _httpListener;

    public Control_Settings(Task<RzSdkManager?> rzSdkManager, Task<PluginManager> pluginManager,
        Task<KeyboardLayoutManager> layoutManager, Task<AuroraHttpListener?> httpListener)
    {
        _pluginManager = pluginManager;
        _layoutManager = layoutManager;
        _httpListener = httpListener;
        InitializeComponent();
        _componentsInitialized = true;

        tabMain.DataContext = Global.Configuration;

        try
        {
            using var service = new TaskService();
            var task = service.FindTask(StartupTaskId);
            var exePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                
            TaskDefinition taskDefinition = task != null ? task.Definition : service.NewTask();
                
            //Update path of startup task
            taskDefinition.RegistrationInfo.Description = "Start Aurora on Startup";
            taskDefinition.Actions.Clear();
            taskDefinition.Actions.Add(new ExecAction(exePath, "-silent", Path.GetDirectoryName(exePath)));
            if (task != null)
            {
                startDelayAmount.Value = task.Definition.Triggers.FirstOrDefault(t =>
                    t.TriggerType == TaskTriggerType.Logon
                ) is LogonTrigger trigger
                    ? (int) trigger.Delay.TotalSeconds
                    : 0;
            }
            else
            {
                taskDefinition.Triggers.Add(new LogonTrigger { Enabled = true });

                taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;
                taskDefinition.Settings.DisallowStartIfOnBatteries = false;
                taskDefinition.Settings.DisallowStartOnRemoteAppSession = false;
                taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            }
            task = service.RootFolder.RegisterTaskDefinition(StartupTaskId, taskDefinition);
            RunAtWinStartup.IsChecked = task.Enabled;
        }
        catch (Exception exc)
        {
            Global.logger.Error("Error caught when updating startup task. Error: " + exc);
        }

        string v = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        string o = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).CompanyName;
        string r = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName;

        lblVersion.Content = (v[0].ToString().Length > 0 ? "" : "beta ") + $"{v} {o}/{r}";
        LnkIssues.NavigateUri = new Uri($"https://github.com/{o}/{r}/issues/");
        LnkRepository.NavigateUri = new Uri($"https://github.com/{o}/{r}");
        LnkContributors.NavigateUri = new Uri($"https://github.com/{o}/{r}#contributors-");

        var rzVersion = RzHelper.GetSdkVersion();
        var rzSdkEnabled = RzHelper.IsSdkEnabled();

        ChromaInstalledVersionLabel.Content = rzVersion.ToString();
        ChromaInstalledVersionLabel.Foreground = new SolidColorBrush(
            RzHelper.IsSdkVersionSupported(rzVersion) ? Colors.LightGreen : Colors.PaleVioletRed);
        ChromaSupportedVersionsLabel.Content = $"[{RzHelper.SupportedFromVersion}-{RzHelper.SupportedToVersion}]";

        if (rzVersion == new RzSdkVersion())
            ChromaUninstallButton.Visibility = Visibility.Hidden;

        ChromaEnabledLabel.Content = rzSdkEnabled ? "Enabled" : "Disabled";
        ChromaEnabledLabel.Foreground = rzSdkEnabled ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.DarkGray);

        var razerManager = rzSdkManager.Result;
        if (razerManager != null)
        {
            ChromaConnectionStatusLabel.Content = "Success";
            ChromaConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.LightGreen);

            var appList = Global.razerSdkManager.GetDataProvider<RzAppListDataProvider>();
            appList.Update();
            ChromaCurrentApplicationLabel.Content = $"{appList.CurrentAppExecutable ?? "None"} [{appList.CurrentAppPid}]";

            razerManager.DataUpdated += HandleChromaAppChange;
        }
        else
        {
            ChromaConnectionStatusLabel.Content = "Failure";
            ChromaConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
            ChromaDisableDeviceControlButton.IsEnabled = false;
        }
    }

    private void HandleChromaAppChange(object s, EventArgs _)
    {
        if (s is not RzAppListDataProvider appList) return;

        appList.Update();
        Global.logger.Debug("RazerManager current app: {Exe} [{Pid}]", appList.CurrentAppExecutable ?? "None", appList.CurrentAppPid);
        Dispatcher.BeginInvoke(DispatcherPriority.Background, 
            (Action) (() => ChromaCurrentApplicationLabel.Content = $"{appList.CurrentAppExecutable} [{appList.CurrentAppPid}]"));
    }

    /// <summary>The excluded program the user has selected in the excluded list.</summary>
    public string SelectedExcludedProgram { get; set; }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ctrlPluginManager.Host = _pluginManager.Result;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start("explorer", e.Uri.AbsoluteUri);
        e.Handled = true;
    }

    private void ExcludedAdd_Click(object sender, RoutedEventArgs e)
    {
        Window_ProcessSelection dialog = new Window_ProcessSelection { ButtonLabel = "Exclude Process" };
        if (dialog.ShowDialog() == true &&
            !string.IsNullOrWhiteSpace(dialog.ChosenExecutableName) &&
            !Global.Configuration.ExcludedPrograms.Contains(dialog.ChosenExecutableName)
           )
            Global.Configuration.ExcludedPrograms.Add(dialog.ChosenExecutableName);
    }

    private void ExcludedRemove_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedExcludedProgram))
            Global.Configuration.ExcludedPrograms.Remove(SelectedExcludedProgram);
    }

    private void RunAtWinStartup_Checked(object sender, RoutedEventArgs e)
    {
        if (IsLoaded && sender is CheckBox)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    //Find existing task
                    var task = ts.FindTask(StartupTaskId);
                    task.Enabled = (sender as CheckBox).IsChecked.Value;
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("RunAtWinStartup_Checked Exception: " + exc);
            }
        }

    }

    private void updates_check_Click(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        DesktopUtils.CheckUpdate();
    }

    private void LoadBrandDefault(object sender, SelectionChangedEventArgs e)
    {
        if (_componentsInitialized)
        {
            _layoutManager.Result.LoadBrandDefault();
        }
    }

    private async void ResetDevices(object sender, RoutedEventArgs e) => await Global.dev_manager.ResetDevices();

    private void razer_wrapper_install_button_Click(object sender, RoutedEventArgs e)
    {
        void HandleExceptions(AggregateException ae)
        {
            ShowMessageBox(ae.ToString(), "Exception!", MessageBoxImage.Error);
            ae.Handle(ex => {
                Global.logger.Error(ex.ToString());
                return true;
            });
        }

        void SetButtonContent(string s)
            => Application.Current.Dispatcher.Invoke(() => ChromaInstallButton.Content = s);

        void ShowMessageBox(string message, string title, MessageBoxImage image = MessageBoxImage.Exclamation)
            => Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message, title, MessageBoxButton.OK, image));

        ChromaInstallButton.IsEnabled = false;
        ChromaUninstallButton.IsEnabled = false;

        Task.Run(async () =>
        {
            SetButtonContent("Uninstalling");
            var uninstallSuccess = await RazerChromaUtils.UninstallAsync()
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        HandleExceptions(t.Exception);
                        return false;
                    }

                    if (t.Result == (int)RazerChromaInstallerExitCode.RestartRequired)
                    {
                        ShowMessageBox("The uninstaller requested system restart!\nPlease reboot your pc and re-run the installation.",
                            "Restart required!");
                        return false;
                    }

                    return true;
                })
                .ConfigureAwait(false);

            if (!uninstallSuccess)
                return;

            SetButtonContent("Downloading");
            var downloadPath = await RazerChromaUtils.DownloadAsync()
                .ContinueWith(t =>
                {
                    if (t.Exception == null) return t.Result;
                    HandleExceptions(t.Exception);
                    return null;
                })
                .ConfigureAwait(false);

            if (downloadPath == null)
                return;

            SetButtonContent("Installing");
            await RazerChromaUtils.InstallAsync(downloadPath)
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                        HandleExceptions(t.Exception);
                    else if (t.Result == (int)RazerChromaInstallerExitCode.RestartRequired)
                        ShowMessageBox("The installer requested system restart!\nPlease reboot your pc.", "Restart required!");
                    else
                    {
                        SetButtonContent("Done!");
                        ShowMessageBox("Installation successful!\nPlease restart aurora for changes to take effect.", "Restart required!");
                    }
                })
                .ConfigureAwait(false);
        });
    }

    private void razer_wrapper_uninstall_button_Click(object sender, RoutedEventArgs e)
    {
        void HandleExceptions(AggregateException ae)
        {
            ShowMessageBox(ae.ToString(), "Exception!", MessageBoxImage.Error);
            ae.Handle(ex => {
                Global.logger.Error(ex.ToString());
                return true;
            });
        }

        void SetButtonContent(string s)
            => Application.Current.Dispatcher.Invoke(() => ChromaUninstallButton.Content = s);

        void ShowMessageBox(string message, string title, MessageBoxImage image = MessageBoxImage.Exclamation)
            => Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message, title, MessageBoxButton.OK, image));

        ChromaInstallButton.IsEnabled = false;
        ChromaUninstallButton.IsEnabled = false;

        Task.Run(async () =>
        {
            SetButtonContent("Uninstalling");
            await RazerChromaUtils.UninstallAsync()
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                        HandleExceptions(t.Exception);
                    else if (t.Result == (int)RazerChromaInstallerExitCode.RestartRequired)
                        ShowMessageBox("The uninstaller requested system restart!\nPlease reboot your pc.", "Restart required!");
                    else if (t.Result == (int)RazerChromaInstallerExitCode.InvalidState)
                        ShowMessageBox("There is nothing to install!", "Invalid State!");
                    else
                    {
                        SetButtonContent("Done!");
                        ShowMessageBox("Uninstallation successful!\nPlease restart aurora for changes to take effect.", "Restart required!");
                    }
                })
                .ConfigureAwait(false);
        });
    }

    private async void razer_wrapper_disable_device_control_button_Click(object sender, RoutedEventArgs e)
    {
        await RazerChromaUtils.DisableDeviceControlAsync();
    }

    private void wrapper_install_logitech_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            App.InstallLogitech();
        }
        catch (Exception exc)
        {
            Global.logger.Error("Exception during Logitech Wrapper install. Exception: " + exc);
            MessageBox.Show("Aurora Wrapper Patch for Logitech could not be applied.\r\nException: " + exc.Message);
        }
    }

    private void wrapper_install_razer_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new FolderBrowserDialog();
        DialogResult result = dialog.ShowDialog();

        if (result != DialogResult.OK) return;
        try
        {
            TryInstallRazerWrapper(dialog);

            MessageBox.Show("Aurora Wrapper Patch for Razer applied to\r\n" + dialog.SelectedPath);
        }
        catch (Exception exc)
        {
            Global.logger.Error("Exception during Razer Wrapper install", exc);
            MessageBox.Show("Aurora Wrapper Patch for Razer could not be applied.\r\nException: " + exc.Message);
        }
    }

    private static void TryInstallRazerWrapper(FolderBrowserDialog dialog)
    {
        using (var razerWrapper86 =
               new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "RzChromaSDK.dll"), FileMode.Create)))
        {
            razerWrapper86.Write(Properties.Resources.Aurora_RazerLEDWrapper86);
        }

        using (var razerWrapper64 =
               new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "RzChromaSDK64.dll"), FileMode.Create)))
        {
            razerWrapper64.Write(Properties.Resources.Aurora_RazerLEDWrapper64);
        }
    }

    private void wrapper_install_lightfx_32_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result != DialogResult.OK) return;
            using (var lightfxWrapper86 = new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create)))
            {
                lightfxWrapper86.Write(Properties.Resources.Aurora_LightFXWrapper86);
            }

            MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) applied to\r\n" + dialog.SelectedPath);
        }
        catch (Exception exc)
        {
            Global.logger.Error("Exception during LightFX (32 bit) Wrapper install. Exception: " + exc);
            MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) could not be applied.\r\nException: " + exc.Message);
        }
    }

    private void wrapper_install_lightfx_64_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result != DialogResult.OK) return;
            using (var lightfxWrapper64 = new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create)))
            {
                lightfxWrapper64.Write(Properties.Resources.Aurora_LightFXWrapper64);
            }

            MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) applied to\r\n" + dialog.SelectedPath);
        }
        catch (Exception exc)
        {
            Global.logger.Error("Exception during LightFX (64 bit) Wrapper install. Exception: " + exc);
            MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) could not be applied.\r\nException: " + exc.Message);
        }
    }

    private void wrapper_asus_configure_devices_Click(object sender, RoutedEventArgs e)
    {
        new DeviceMappingConfigWindow().Show();
    }

    private void btnShowLogsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button)
            Process.Start("explorer", Path.Combine(Global.LogsDirectory));
    }

    private void HighPriorityCheckbox_Checked(object sender, RoutedEventArgs e)
    {
        Process.GetCurrentProcess().PriorityClass = Global.Configuration.HighPriority ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;
    }

    private void btnShowBitmapWindow_Click(object sender, RoutedEventArgs e) => Window_BitmapView.Open();

    private void btnShowGSILog_Click(object sender, RoutedEventArgs e) => Window_GSIHttpDebug.Open(_httpListener);

    private void StartDelayAmount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        using TaskService service = new TaskService();
        var task = service.FindTask(StartupTaskId);
        if (task?.Definition.Triggers.FirstOrDefault(t => t.TriggerType == TaskTriggerType.Logon) is not
            LogonTrigger trigger) return;
        trigger.Delay = new TimeSpan(0, 0, ((IntegerUpDown)sender).Value ?? 0);
        task.RegisterChanges();
    }

    private void btnDumpSensors_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(HardwareMonitor.TryDump()
            ? "Successfully wrote sensor info to logs folder"
            : "Error dumping file. Consult log for details.");
    }
}