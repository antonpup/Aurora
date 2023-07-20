using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Aurora.Devices.RGBNet.Config;
using Aurora.Modules.Razer;
using Aurora.Utils;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Aurora.Settings;

public partial class Control_SettingsDevicesAndWrappers
{
    private readonly Task<KeyboardLayoutManager> _layoutManager;
    
    public Control_SettingsDevicesAndWrappers(Task<RzSdkManager?> rzSdkManager, Task<KeyboardLayoutManager> layoutManager)
    {
        _layoutManager = layoutManager;

        InitializeComponent();

        var rzVersion = RzHelper.GetSdkVersion();

        ChromaInstalledVersionLabel.Content = rzVersion.ToString();
        ChromaInstalledVersionLabel.Foreground = new SolidColorBrush(
            RzHelper.IsSdkVersionSupported(rzVersion) ? Colors.LightGreen : Colors.PaleVioletRed);
        ChromaSupportedVersionsLabel.Content = $"[{RzHelper.SupportedFromVersion}-{RzHelper.SupportedToVersion}]";

        if (rzVersion == new RzSdkVersion())
            ChromaUninstallButton.Visibility = Visibility.Hidden;

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

    private void LoadBrandDefault(object sender, SelectionChangedEventArgs e)
    {
        _layoutManager.Result.LoadBrandDefault();
    }

    private void ResetDevices(object sender, RoutedEventArgs e) => Task.Run(async () => await Global.dev_manager.ResetDevices());

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
}