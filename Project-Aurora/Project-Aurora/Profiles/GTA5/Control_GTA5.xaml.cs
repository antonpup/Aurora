using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Aurora.Profiles.GTA5.GSI;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Aurora.Profiles.GTA5;

/// <summary>
/// Interaction logic for Control_GTA5.xaml
/// </summary>
public partial class Control_GTA5
{
    private readonly Application _profileManager;

    private readonly Timer _previewWantedLevelTimer;
    private int _frame;

    public Control_GTA5(Application profile)
    {
        InitializeComponent();

        _profileManager = profile;

        SetSettings();

        _previewWantedLevelTimer = new Timer(1000);
        _previewWantedLevelTimer.Elapsed += PreviewWantedLevelTimerElapsed;
    }

    private void SetSettings()
    {
        game_enabled.IsChecked = _profileManager.Settings.IsEnabled;
    }

    private void preview_state_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
        {
            (_profileManager.Config.Event._game_state as GameState_GTA5).CurrentState =
                (PlayerState)preview_team.SelectedValue;
        }
    }

    private void PreviewWantedLevelTimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (_frame % 2 == 0)
        {
            (_profileManager.Config.Event._game_state as GameState_GTA5).LeftSirenColor = Color.Red;
            (_profileManager.Config.Event._game_state as GameState_GTA5).RightSirenColor = Color.Blue;
        }
        else
        {
            (_profileManager.Config.Event._game_state as GameState_GTA5).LeftSirenColor = Color.Blue;
            (_profileManager.Config.Event._game_state as GameState_GTA5).RightSirenColor = Color.Red;
        }

        _frame++;
    }

    private void preview_wantedLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (!IsLoaded || sender is not IntegerUpDown || !(sender as IntegerUpDown).Value.HasValue) return;
        var value = (sender as IntegerUpDown).Value.Value;
        if (value == 0)
        {
            _previewWantedLevelTimer.Stop();
            (_profileManager.Config.Event._game_state as GameState_GTA5).HasCops = false;
        }
        else
        {
            _previewWantedLevelTimer.Start();
            _previewWantedLevelTimer.Interval = 600D - 50D * value;
            (_profileManager.Config.Event._game_state as GameState_GTA5).HasCops = true;
        }
    }
    
    private async void patch_button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await InstallLogitech();
        }
        catch (Exception exc)
        {
            Global.logger.Error("Could not start Aurora Logitech Patcher. Error: " + exc);
        }
    }

    private void game_enabled_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        _profileManager.Settings.IsEnabled = game_enabled.IsChecked.HasValue && game_enabled.IsChecked.Value;
        _profileManager.SaveProfiles();
    }

    private static async Task InstallLogitech()
    {
        using var httpClient = new HttpClient();

        //Patch 32-bit
        var logitechPath = (string)Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\WOW6432Node\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary",
            null, null); //null gets the default value
        await Patch86(logitechPath, httpClient);

        //Patch 64-bit
        var logitechPath64 = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary",
            null, null);
        await Patch64(logitechPath64, httpClient);

        Global.logger.Information("Logitech LED SDK patched successfully");
        MessageBox.Show("Logitech LED SDK patched successfully");
    }

    private static async Task Patch86(string? logitechPath, HttpClient httpClient)
    {
        if (logitechPath is null or @"C:\Program Files\LGHUB\sdk_legacy_led_x86.dll")
        {
            logitechPath = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x86\LogitechLed.dll";

            if (!Directory.Exists(Path.GetDirectoryName(logitechPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(logitechPath));

            var key = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);

            key.CreateSubKey("Classes");
            key = key.OpenSubKey("Classes", true);

            key.CreateSubKey("WOW6432Node");
            key = key.OpenSubKey("WOW6432Node", true);

            key.CreateSubKey("CLSID");
            key = key.OpenSubKey("CLSID", true);

            key.CreateSubKey("{a6519e67-7632-4375-afdf-caa889744403}");
            key = key.OpenSubKey("{a6519e67-7632-4375-afdf-caa889744403}", true);

            key.CreateSubKey("ServerBinary");
            key = key.OpenSubKey("ServerBinary", true);

            key.SetValue(null, logitechPath); //null to set the default value
            key.Close();
        }

        if (File.Exists(logitechPath) && !File.Exists(logitechPath + ".aurora_backup"))
            File.Move(logitechPath, logitechPath + ".aurora_backup");

        var x86Uri = new Uri("https://github.com/Aurora-RGB/Aurora/blob/v137/Project-Aurora/Project-Aurora/Resources/Win32/Aurora-LogiLEDWrapper.dll");
        var x86Response = await httpClient.GetAsync(x86Uri);
        await using var fs = new FileStream(logitechPath, FileMode.CreateNew);
        await x86Response.Content.CopyToAsync(fs);
    }

    private static async Task Patch64(string? logitechPath64, HttpClient httpClient)
    {
        if (logitechPath64 is null or @"C:\Program Files\LGHUB\sdk_legacy_led_x64.dll")
        {
            logitechPath64 = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll";

            if (!Directory.Exists(Path.GetDirectoryName(logitechPath64)))
                Directory.CreateDirectory(Path.GetDirectoryName(logitechPath64));

            var key = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);

            key.CreateSubKey("Classes");
            key = key.OpenSubKey("Classes", true);

            key.CreateSubKey("CLSID");
            key = key.OpenSubKey("CLSID", true);

            key.CreateSubKey("{a6519e67-7632-4375-afdf-caa889744403}");
            key = key.OpenSubKey("{a6519e67-7632-4375-afdf-caa889744403}", true);

            key.CreateSubKey("ServerBinary");
            key = key.OpenSubKey("ServerBinary", true);

            key.SetValue(null, logitechPath64);
        }

        if (File.Exists(logitechPath64) && !File.Exists(logitechPath64 + ".aurora_backup"))
            File.Move(logitechPath64, logitechPath64 + ".aurora_backup");

        var x64Uri = new Uri("https://github.com/Aurora-RGB/Aurora/blob/v137/Project-Aurora/Project-Aurora/Resources/Win64/Aurora-LogiLEDWrapper.dll");
        var x64Response = await httpClient.GetAsync(x64Uri);
        await using var fs = new FileStream(logitechPath64, FileMode.CreateNew);
        await x64Response.Content.CopyToAsync(fs);
    }
}