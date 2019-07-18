using Aurora.Devices;
using Microsoft.Win32;
using RazerSdkWrapper.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Control_RazerLayer : UserControl
    {
        private bool settingsset = false;
        protected RazerLayerHandler Context => DataContext as RazerLayerHandler;

        public Control_RazerLayer()
        {
            InitializeComponent();
        }

        public Control_RazerLayer(RazerLayerHandler datacontext)
        {
            InitializeComponent();

            DataContext = datacontext;
        }
        public void SetSettings()
        {
            if (Context != null && !settingsset)
            {
                ColorPostProcessCheckBox.IsChecked = Context.Properties.ColorPostProcessEnabled;
                BrightnessSlider.Value = Context.Properties.BrightnessChange;
                SaturationSlider.Value = Context.Properties.SaturationChange;
                HueSlider.Value = Context.Properties.HueShift;
                CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
                settingsset = true;
            }
        }

        private void OnUserControlLoaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            var version = RzHelper.GetSdkVersion();
            var enabled = RzHelper.IsSdkEnabled();

            SdkInstalledVersionValueLabel.Content = version.ToString();
            if (!RzHelper.IsSdkVersionSupported(version))
            {
                SdkInstalledVersionValueLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
                SdkInstallButton.Visibility = Visibility.Visible;
            }
            else
            {
                SdkInstalledVersionValueLabel.Foreground = new SolidColorBrush(Colors.LightGreen);
                SdkInstallButton.Visibility = Visibility.Hidden;
            }

            SdkEnabledValueLabel.Content = enabled ? "Enabled" : "Disabled";
            SdkEnabledValueLabel.Foreground = enabled ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.PaleVioletRed);

            SdkConnectionStatusLabel.Content = Context.Loaded ? "Success" : "Failure";
            SdkConnectionStatusLabel.Foreground = Context.Loaded ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.PaleVioletRed);

            Loaded -= OnUserControlLoaded;
        }

        private void OnAddKeyCloneButtonClick(object sender, RoutedEventArgs e)
        {
            if (KeyCloneSourceButtonComboBox.SelectedItem == null || KeyCloneDestinationButtonComboBox.SelectedItem == null)
                return;

            var sourceKey = (DeviceKeys)KeyCloneSourceButtonComboBox.SelectedItem;
            var destKey = (DeviceKeys)KeyCloneDestinationButtonComboBox.SelectedItem;

            if (sourceKey == destKey)
                return;

            var cloneMap = Context.Properties.KeyCloneMap;
            if (cloneMap.ContainsKey(destKey) && cloneMap[destKey] == sourceKey)
                return;

            cloneMap.Add(destKey, sourceKey);
            CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
        }

        private void OnDeleteKeyCloneButtonClick(object sender, RoutedEventArgs e)
        {
            if (KeyCloneListBox.SelectedItem == null)
                return;

            var cloneMap = Context.Properties.KeyCloneMap;
            var item = (KeyValuePair<DeviceKeys, DeviceKeys>)KeyCloneListBox.SelectedItem;
            if (!cloneMap.ContainsKey(item.Key) || cloneMap[item.Key] != item.Value)
                return;

            cloneMap.Remove(item.Key);
            CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
        }

        private void OnSdkDumpToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            if (Context.StartDumpingData())
            {
                SdkDumpToggleButton.Content = "Dumping...";
                SdkDumpToggleButton.IsEnabled = false;

                Task.Delay(5000).ContinueWith((t) =>
                    Application.Current.Dispatcher.Invoke(() => SdkDumpToggleButton.IsChecked = false)
                );
            }
        }

        private void OnSdkDumpToggleButtonUnchecked(object sender, RoutedEventArgs e)
        {
            SdkDumpToggleButton.Content = "Start";
            SdkDumpToggleButton.IsEnabled = true;
            Context.StopDumpingData();
        }

        private void OnSdkInstallButtonClick(object sender, RoutedEventArgs e)
        {
            SdkInstallButton.IsEnabled = false;

            bool HandleErrorLevel(int errorlevel)
            {
                switch (errorlevel)
                {
                    case 3010:
                        {
                            MessageBox.Show("Razer SDK requested system restart!\nPlease reboot your pc and re-run the installation.",
                                "Restart required!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return false;
                        }
                }

                return true;
            }

            void SetState(string name)
                => Application.Current.Dispatcher.Invoke(() => SdkInstallButton.Content = name);

            Task.Run(async () =>
            {
                try
                {
                    SetState("Uninstalling");
                    var errorlevel = await UninstallAsync();
                    if (!HandleErrorLevel(errorlevel))
                        return false;

                    SetState("Downloading");
                    var path = await DownloadAsync();

                    SetState("Installing");
                    errorlevel = await InstallAsync(path);
                    if (!HandleErrorLevel(errorlevel))
                        return false;

                    SetState("Done!");
                }
                catch (OperationCanceledException ex)
                {
                    SetState("Failure");
                    MessageBox.Show($"{ex.Message}:\n{ex.InnerException.ToString()}",
                        "Exception!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                return true;
            }).ContinueWith(t =>
            {
                if (t.Result)
                {
                    SetState("Success");
                    MessageBox.Show("Installation successful!\nPlease restart Aurora for changes to take effect.",
                        "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    SetState("Failure");
                }
            });
        }

        #region SDK Installer/Uninstaller helpers
        private Task<int> UninstallAsync()
        {
            return Task.Run(() =>
            {
                if (RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
                    return 0;

                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    var key = hklm.OpenSubKey(@"Software\Razer Chroma SDK");
                    var path = (string)key?.GetValue("UninstallPath", null);
                    var filename = (string)key?.GetValue("UninstallFilename", null);

                    if (path == null || filename == null)
                        return 0;

                    try
                    {
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = filename,
                            WorkingDirectory = path,
                            Arguments = $"/S _?={path}",
                            ErrorDialog = true
                        };

                        var process = Process.Start(processInfo);
                        process.WaitForExit(120000);
                        return process.ExitCode;
                    }
                    catch (Exception ex)
                    {
                        throw new OperationCanceledException("Razer SDK Uninstallation failed!", ex);
                    }
                }
            });
        }

        private Task<string> DownloadAsync()
        {
            return Task.Run(() =>
            {
                var url = "http://cdn.razersynapse.com/156092369797u1UA8NRazerChromaBroadcasterSetup_v3.4.0630.061913.exe";

                try
                {
                    using (var client = new WebClient())
                    {
                        var path = Path.ChangeExtension(Path.GetTempFileName(), ".exe");
                        client.DownloadFile(url, path);
                        return path;
                    }
                }
                catch (Exception ex)
                {
                    throw new OperationCanceledException("Razer SDK Downloading failed!", ex);
                }
            });
        }

        private Task<int> InstallAsync(string installerPath)
        {
            return Task.Run(() =>
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = Path.GetFileName(installerPath),
                        WorkingDirectory = Path.GetDirectoryName(installerPath),
                        Arguments = "/S",
                        ErrorDialog = true
                    };

                    var process = Process.Start(processInfo);
                    process.WaitForExit(120000);
                    return process.ExitCode;
                }
                catch (Exception ex)
                {
                    throw new OperationCanceledException("Razer SDK Installation failed!", ex);
                }
            });
        }
        #endregion
    }
}
