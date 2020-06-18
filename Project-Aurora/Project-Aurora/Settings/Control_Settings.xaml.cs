using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Xceed.Wpf.Toolkit;
using Aurora.Profiles.Desktop;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;
using System.Windows.Data;
using RazerSdkWrapper.Utils;
using System.Net;
using RazerSdkWrapper.Data;
using System.Windows.Threading;
using Aurora.Devices.Asus.Config;
using Aurora.Utils;
using System.Globalization;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_Settings.xaml
    /// </summary>
    public partial class Control_Settings : UserControl
    {
        private RegistryKey runRegistryPath = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private const string StartupTaskID = "AuroraStartup";

        public Control_Settings()
        {
            InitializeComponent();

            this.tabMain.DataContext = Global.Configuration;

            if (runRegistryPath.GetValue("Aurora") != null)
                runRegistryPath.DeleteValue("Aurora");

            try
            {
                using (TaskService service = new TaskService())
                {
                    Microsoft.Win32.TaskScheduler.Task task = service.FindTask(StartupTaskID);
                    if (task != null)
                    {
                        TaskDefinition definition = task.Definition;
                        //Update path of startup task
                        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        definition.Actions.Clear();
                        definition.Actions.Add(new ExecAction(exePath, "-silent", Path.GetDirectoryName(exePath)));
                        service.RootFolder.RegisterTaskDefinition(StartupTaskID, definition);
                        RunAtWinStartup.IsChecked = task.Enabled;
                        startDelayAmount.Value = task.Definition.Triggers.FirstOrDefault(t => t.TriggerType == TaskTriggerType.Logon) is LogonTrigger trigger ? (int)trigger.Delay.TotalSeconds : 0;
                    }
                    else
                    {
                        TaskDefinition td = service.NewTask();
                        td.RegistrationInfo.Description = "Start Aurora on Startup";

                        td.Triggers.Add(new LogonTrigger { Enabled = true });

                        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                        td.Actions.Add(new ExecAction(exePath, "-silent", Path.GetDirectoryName(exePath)));

                        td.Principal.RunLevel = TaskRunLevel.Highest;
                        td.Settings.DisallowStartIfOnBatteries = false;
                        td.Settings.DisallowStartOnRemoteAppSession = false;
                        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;

                        service.RootFolder.RegisterTaskDefinition(StartupTaskID, td);
                        RunAtWinStartup.IsChecked = true;
                    }
                }
            }
            catch(Exception exc)
            {
                Global.logger.Error("Error caught when updating startup task. Error: " + exc.ToString());
            }

            string v = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;

            this.lblVersion.Content = ((int.Parse(v[0].ToString()) > 0) ? "" : "beta ") + $"v{v}";

            var rzVersion = RzHelper.GetSdkVersion();
            var rzSdkEnabled = RzHelper.IsSdkEnabled();

            this.razer_wrapper_installed_version_label.Content = rzVersion.ToString();
            this.razer_wrapper_installed_version_label.Foreground = new SolidColorBrush(RzHelper.IsSdkVersionSupported(rzVersion) ? Colors.LightGreen : Colors.PaleVioletRed);
            this.razer_wrapper_supported_versions_label.Content = $"[{RzHelper.SupportedFromVersion}-{RzHelper.SupportedToVersion}]";

            if (rzVersion == new RzSdkVersion())
                this.razer_wrapper_uninstall_button.Visibility = Visibility.Hidden;

            this.razer_wrapper_enabled_label.Content = rzSdkEnabled ? "Enabled" : "Disabled";
            this.razer_wrapper_enabled_label.Foreground = rzSdkEnabled ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.PaleVioletRed);

            if (Global.razerSdkManager != null)
            {
                this.razer_wrapper_connection_status_label.Content = "Success";
                this.razer_wrapper_connection_status_label.Foreground = new SolidColorBrush(Colors.LightGreen);

                {
                    var appList = Global.razerSdkManager.GetDataProvider<RzAppListDataProvider>();
                    appList.Update();
                    this.razer_wrapper_current_application_label.Content = $"{appList.CurrentAppExecutable ?? "None"} [{appList.CurrentAppPid}]";
                }

                Global.razerSdkManager.DataUpdated += (s, _) =>
                {
                    if (!(s is RzAppListDataProvider appList))
                        return;

                    appList.Update();
                    Global.logger.Debug("RazerManager current app: {0} [{1}]", appList.CurrentAppExecutable ?? "None", appList.CurrentAppPid);
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, (System.Action)(() => this.razer_wrapper_current_application_label.Content = $"{appList.CurrentAppExecutable} [{appList.CurrentAppPid}]"));
                };
            }
            else
            {
                this.razer_wrapper_connection_status_label.Content = "Failure";
                this.razer_wrapper_connection_status_label.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
            }
        }

        /// <summary>The excluded program the user has selected in the excluded list.</summary>
        public string SelectedExcludedProgram { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ctrlPluginManager.Host = Global.PluginManager;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ExcludedAdd_Click(object sender, RoutedEventArgs e) {
            Window_ProcessSelection dialog = new Window_ProcessSelection { ButtonLabel = "Exclude Process" };
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ChosenExecutableName) && !Global.Configuration.ExcludedPrograms.Contains(dialog.ChosenExecutableName))
                Global.Configuration.ExcludedPrograms.Add(dialog.ChosenExecutableName);
        }

        private void ExcludedRemove_Click(object sender, RoutedEventArgs e) {
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
                        var task = ts.FindTask(StartupTaskID);
                        task.Enabled = (sender as CheckBox).IsChecked.Value;
                    }
                }
                catch(Exception exc)
                {
                    Global.logger.Error("RunAtWinStartup_Checked Exception: " + exc);
                }
            }

        }

        private void devices_retry_Click(object sender, RoutedEventArgs e)
        {
            Global.dev_manager.Initialize();
        }

        private void devices_view_first_time_logitech_Click(object sender, RoutedEventArgs e)
        {
            Devices.Logitech.LogitechInstallInstructions instructions = new Devices.Logitech.LogitechInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_view_first_time_corsair_Click(object sender, RoutedEventArgs e)
        {
            Devices.Corsair.CorsairInstallInstructions instructions = new Devices.Corsair.CorsairInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_view_first_time_razer_Click(object sender, RoutedEventArgs e)
        {
            Devices.Razer.RazerInstallInstructions instructions = new Devices.Razer.RazerInstallInstructions();
            instructions.ShowDialog();
        }
        private void devices_view_first_time_steelseries_Click(object sender, RoutedEventArgs e)
        {
            Devices.SteelSeries.SteelSeriesInstallInstructions instructions = new Devices.SteelSeries.SteelSeriesInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_view_first_time_dualshock_Click(object sender, RoutedEventArgs e)
        {
            Devices.Dualshock.DualshockInstallInstructions instructions = new Devices.Dualshock.DualshockInstallInstructions();
            instructions.ShowDialog();
        }

        private void devices_view_first_time_roccat_Click(object sender, RoutedEventArgs e)
        {
            Devices.Roccat.RoccatInstallInstructions instructions = new Devices.Roccat.RoccatInstallInstructions();
            instructions.ShowDialog();
        }

        
        private void updates_check_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                string updater_path = System.IO.Path.Combine(Global.ExecutingDirectory, "Aurora-Updater.exe");

                if (File.Exists(updater_path))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = updater_path;
                    Process.Start(startInfo);
                }
                else
                {
                    System.Windows.MessageBox.Show("Updater is missing!");
                }
            }
        }

        private void LoadBrandDefault(object sender, SelectionChangedEventArgs e) => Global.kbLayout.LoadBrandDefault();
        private void ResetDevices(object sender, RoutedEventArgs e) => Global.dev_manager.ResetDevices();


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
                => Application.Current.Dispatcher.Invoke(() => razer_wrapper_install_button.Content = s);

            void ShowMessageBox(string message, string title, MessageBoxImage image = MessageBoxImage.Exclamation)
                => Application.Current.Dispatcher.Invoke(() => System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, image));

            razer_wrapper_install_button.IsEnabled = false;
            razer_wrapper_uninstall_button.IsEnabled = false;

            System.Threading.Tasks.Task.Run(async () =>
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
                    else if (t.Result == (int)RazerChromaInstallerExitCode.RestartRequired)
                    {
                        ShowMessageBox("The uninstaller requested system restart!\nPlease reboot your pc and re-run the installation.", "Restart required!");
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
                    if (t.Exception != null)
                    {
                        HandleExceptions(t.Exception);
                        return null;
                    }

                    return t.Result;
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
                => Application.Current.Dispatcher.Invoke(() => razer_wrapper_uninstall_button.Content = s);
            
            void ShowMessageBox(string message, string title, MessageBoxImage image = MessageBoxImage.Exclamation)
                => Application.Current.Dispatcher.Invoke(() => System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, image));

            razer_wrapper_install_button.IsEnabled = false;
            razer_wrapper_uninstall_button.IsEnabled = false;

            System.Threading.Tasks.Task.Run(async () =>
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

        private void wrapper_install_logitech_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.InstallLogitech();
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception during Logitech Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for Logitech could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_install_razer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    using (BinaryWriter razer_wrapper_86 = new BinaryWriter(new FileStream(System.IO.Path.Combine(dialog.SelectedPath, "RzChromaSDK.dll"), FileMode.Create)))
                    {
                        razer_wrapper_86.Write(Properties.Resources.Aurora_RazerLEDWrapper86);
                    }

                    using (BinaryWriter razer_wrapper_64 = new BinaryWriter(new FileStream(System.IO.Path.Combine(dialog.SelectedPath, "RzChromaSDK64.dll"), FileMode.Create)))
                    {
                        razer_wrapper_64.Write(Properties.Resources.Aurora_RazerLEDWrapper64);
                    }

                    System.Windows.MessageBox.Show("Aurora Wrapper Patch for Razer applied to\r\n" + dialog.SelectedPath);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception during Razer Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for Razer could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_install_lightfx_32_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    using (BinaryWriter lightfx_wrapper_86 = new BinaryWriter(new FileStream(System.IO.Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create)))
                    {
                        lightfx_wrapper_86.Write(Properties.Resources.Aurora_LightFXWrapper86);
                    }

                    System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) applied to\r\n" + dialog.SelectedPath);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception during LightFX (32 bit) Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_install_lightfx_64_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    using (BinaryWriter lightfx_wrapper_64 = new BinaryWriter(new FileStream(System.IO.Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create)))
                    {
                        lightfx_wrapper_64.Write(Properties.Resources.Aurora_LightFXWrapper64);
                    }

                    System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) applied to\r\n" + dialog.SelectedPath);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception during LightFX (64 bit) Wrapper install. Exception: " + exc);
                System.Windows.MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) could not be applied.\r\nException: " + exc.Message);
            }
        }

        private void wrapper_asus_configure_devices_Click(object sender, RoutedEventArgs e)
        {
            var window = new AsusConfigWindow();
            window.Show();
        }
        
        private void btnShowLogsFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
                System.Diagnostics.Process.Start(System.IO.Path.Combine(Global.LogsDirectory));
        }

        private void HighPriorityCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = Global.Configuration.HighPriority ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;
        }

        private void btnShowBitmapWindow_Click(object sender, RoutedEventArgs e) => Window_BitmapView.Open();

        private void btnShowGSILog_Click(object sender, RoutedEventArgs e) => Window_GSIHttpDebug.Open();

        private void StartDelayAmount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            using TaskService service = new TaskService();
            var task = service.FindTask(StartupTaskID);
            if (task != null && task.Definition.Triggers.FirstOrDefault(t => t.TriggerType == TaskTriggerType.Logon) is LogonTrigger trigger) {
                trigger.Delay = new TimeSpan(0, 0, ((IntegerUpDown)sender).Value ?? 0);
                task.RegisterChanges();
            }
        }

        private void btnDumpSensors_Click(object sender, RoutedEventArgs e)
        {
            if (HardwareMonitor.TryDump())
                System.Windows.MessageBox.Show("Successfully wrote sensor info to logs folder");
            else
                System.Windows.MessageBox.Show("Error dumping file. Consult log for details.");
        }
    }
}
