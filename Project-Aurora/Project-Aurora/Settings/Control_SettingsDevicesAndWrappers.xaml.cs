using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Aurora.Devices;
using Aurora.Modules;
using Aurora.Modules.Logitech;
using Aurora.Modules.Razer;
using Aurora.Utils;
using Microsoft.Win32;
using RazerSdkReader;
using RazerSdkReader.Structures;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Aurora.Settings;

public partial class Control_SettingsDevicesAndWrappers
{
    private readonly Task<KeyboardLayoutManager> _layoutManager;
    private readonly Task<ChromaReader?> _rzSdkManager;
    private readonly Task<DeviceManager> _deviceManager;
    
    public Control_SettingsDevicesAndWrappers(Task<ChromaReader?> rzSdkManager, Task<KeyboardLayoutManager> layoutManager,
        Task<DeviceManager> deviceManager)
    {
        _rzSdkManager = rzSdkManager;
        _layoutManager = layoutManager;
        _deviceManager = deviceManager;

        InitializeComponent();

        var rzVersion = RzHelper.GetSdkVersion();

        ChromaInstalledVersionLabel.Content = rzVersion.ToString();
        ChromaInstalledVersionLabel.Foreground = new SolidColorBrush(
            RzHelper.IsSdkVersionSupported(rzVersion) ? Colors.LightGreen : Colors.PaleVioletRed);
        ChromaSupportedVersionsLabel.Content = $"[{RzHelper.SupportedFromVersion}-{RzHelper.SupportedToVersion}]";

        if (rzVersion == new RzSdkVersion())
            ChromaUninstallButton.Visibility = Visibility.Hidden;
    }

    public async Task Initialize()
    {
        var razerManager = await _rzSdkManager;
        if (razerManager != null)
        {
            ChromaConnectionStatusLabel.Content = "Success";
            ChromaConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.LightGreen);

            var currentApp = RzHelper.CurrentAppExecutable;
            var currentAppId = RzHelper.CurrentAppId;
            ChromaCurrentApplicationLabel.Content = $"{currentApp ?? "None"} [{currentAppId}]";

            razerManager.AppDataUpdated += HandleChromaAppChange;
        }
        else
        {
            ChromaConnectionStatusLabel.Content = "Failure";
            ChromaConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
            ChromaDisableDeviceControlButton.IsEnabled = false;
        }

        var logitechSdkListener = LogitechSdkModule.LogitechSdkListener;
        switch (logitechSdkListener.State)
        {
            case LightsyncSdkState.Conflicted:
                LightsyncConnectionStatusLabel.Content = "Conflicted";
                LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Crimson);
                break;
            case LightsyncSdkState.NotInstalled:
                LightsyncConnectionStatusLabel.Content = "Not Installed";
                LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
                break;
            case LightsyncSdkState.Waiting:
                LightsyncConnectionStatusLabel.Content = "Waiting for game";
                LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Chocolate);
                break;
            case LightsyncSdkState.Connected:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        logitechSdkListener.ApplicationChanged += LogitechSdkListenerOnApplicationChanged;
    }

    private void HandleChromaAppChange(object? s, ChromaAppData appData)
    {
        uint currentAppId = 0;
        string? currentAppName = null;
        for (var i = 0; i < appData.AppCount; i++)
        {
            if (appData.CurrentAppId != appData.AppInfo[i].AppId) continue;

            currentAppId = appData.CurrentAppId;
            currentAppName = appData.AppInfo[i].AppName;
            break;
        }

        Dispatcher.BeginInvoke(DispatcherPriority.Background, 
            () => ChromaCurrentApplicationLabel.Content = $"{currentAppName} [{currentAppId}]");
    }

    private void LogitechSdkListenerOnApplicationChanged(object? sender, string? e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Background, 
            () =>
            {
                if (e == null)
                {
                    LightsyncConnectionStatusLabel.Content = "Disconnected";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Chocolate);
                    LightsyncCurrentApplicationLabel.Content = "-";
                }
                else
                {
                    LightsyncConnectionStatusLabel.Content = "Connected";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.LightGreen);
                    LightsyncCurrentApplicationLabel.Content = Path.GetFileName(e);
                }
            });
    }

    private async void LoadBrandDefault(object? sender, SelectionChangedEventArgs e)
    {
        await (await _layoutManager).LoadBrandDefault();
    }

    private void ResetDevices(object? sender, RoutedEventArgs e) => Task.Run(async () => await (await _deviceManager).ResetDevices());

    private async void razer_wrapper_install_button_Click(object? sender, RoutedEventArgs e)
    {
        ChromaInstallButton.IsEnabled = false;
        ChromaUninstallButton.IsEnabled = false;

        SetButtonContent("Uninstalling...");
        var uninstallSuccess = await RazerChromaUtils.UninstallAsync()
            .ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    HandleExceptions(t.Exception);
                    return false;
                }

                if (t.Result != (int)RazerChromaInstallerExitCode.RestartRequired) return true;
                ShowMessageBox(
                    "The uninstaller requested system restart!\nPlease reboot your pc and re-run the installation.",
                    "Restart required!");
                return false;
            })
            .ConfigureAwait(false);

        if (!uninstallSuccess)
            return;

        SetButtonContent("Downloading...");
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

        SetButtonContent("Installing...");
        await RazerChromaUtils.InstallAsync(downloadPath)
            .ContinueWith(t =>
            {
                if (t.Exception != null)
                    HandleExceptions(t.Exception);
                else if (t.Result == (int)RazerChromaInstallerExitCode.RestartRequired)
                    ShowMessageBox("The installer requested system restart!\nPlease reboot your pc.",
                        "Restart required!");
                else
                {
                    SetButtonContent("Disabling bloat...");
                    RazerChromaUtils.DisableChromaBloat();
                    SetButtonContent("Done!");
                    ShowMessageBox("Installation successful!\nPlease restart aurora for changes to take effect.",
                        "Restart required!");
                }
            })
            .ConfigureAwait(false);
        
        
        return;

        void HandleExceptions(AggregateException ae)
        {
            ShowMessageBox(ae.ToString(), "Exception!", MessageBoxImage.Error);
            ae.Handle(ex => {
                Global.logger.Error(ex, "Chroma install error");
                return true;
            });
        }

        void SetButtonContent(string s) => Application.Current.Dispatcher.Invoke(() => ChromaInstallButton.Content = s);

        void ShowMessageBox(string message, string title, MessageBoxImage image = MessageBoxImage.Exclamation)
            => Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message, title, MessageBoxButton.OK, image));
    }

    private void razer_wrapper_uninstall_button_Click(object? sender, RoutedEventArgs e)
    {
        void HandleExceptions(AggregateException ae)
        {
            ShowMessageBox(ae.ToString(), "Exception!", MessageBoxImage.Error);
            ae.Handle(ex => {
                Global.logger.Error(ex, "Razer wrapper uninstall error");
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

    private async void razer_wrapper_disable_device_control_button_Click(object? sender, RoutedEventArgs e)
    {
        await RazerChromaUtils.DisableDeviceControlAsync();
    }

    private void Lightsync_install_button_Click(object? sender, RoutedEventArgs e)
    {
        const string wrapperFolder = "C:\\ProgramData\\Aurora";
        const string dllName = "LogitechLed.dll";
        
        using var httpClient = new HttpClient();

        try
        {
            InstallLightsync64(wrapperFolder, dllName, httpClient);
            InstallLightsync86(wrapperFolder, dllName, httpClient);

            LightsyncConnectionStatusLabel.Content = "Waiting for game";
            MessageBox.Show("Logitech wrapper installed!");
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Error installing Lightsync wrapper!");
            MessageBox.Show(exc.Message, "Error installing Lightsync wrapper!");
        }
    }

    private const string ArtemisRepo =
        "https://raw.githubusercontent.com/Artemis-RGB/Artemis.Plugins.Wrappers/04d1f6acf3a93b0c883118f174b18ced8e264a84";
    private static void InstallLightsync64(string wrapperFolder, string dllName, HttpClient httpClient)
    {
        const string logiX64 = ArtemisRepo + "/src/Logitech/Artemis.Plugins.Wrappers.Logitech/x64/LogitechLed.dll";
        var x64DllFolder = Path.Combine(wrapperFolder, "x64");
        var x64DllPath = Path.Combine(x64DllFolder, dllName);

        var x64Response = httpClient.GetAsync(new Uri(logiX64));
        Directory.CreateDirectory(x64DllFolder);
        using var fs = new FileStream(x64DllPath, FileMode.Create);
        x64Response.Result.Content.CopyToAsync(fs).Wait();

        using var key =Registry.LocalMachine.CreateSubKey(
                @"SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", true);
        key.SetValue(null, x64DllPath);
    }

    private static void InstallLightsync86(string wrapperFolder, string dllName, HttpClient httpClient)
    {
        const string logiX86 = ArtemisRepo + "/src/Logitech/Artemis.Plugins.Wrappers.Logitech/x86/LogitechLed.dll";
        var x86DllFolder = Path.Combine(wrapperFolder, "x86");

        var x86Response = httpClient.GetAsync(new Uri(logiX86));
        Directory.CreateDirectory(x86DllFolder);
        var x86DllPath = Path.Combine(x86DllFolder, dllName);
        using var fs = new FileStream(x86DllPath, FileMode.Create);
        x86Response.Result.Content.CopyToAsync(fs).Wait();

        using var key = Registry.LocalMachine.CreateSubKey(
            @"SOFTWARE\Classes\WOW6432Node\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", true);
        key.SetValue(null, x86DllPath);
    }

    private void wrapper_install_lightfx_32_Click(object? sender, RoutedEventArgs e)
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
            Global.logger.Error(exc, "Exception during LightFX (32 bit) Wrapper install. Exception: ");
            MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) could not be applied.\r\nException: " + exc.Message);
        }
    }

    private void wrapper_install_lightfx_64_Click(object? sender, RoutedEventArgs e)
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
            Global.logger.Error(exc, "Exception during LightFX (64 bit) Wrapper install");
            MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) could not be applied.\r\nException: " + exc.Message);
        }
    }
}