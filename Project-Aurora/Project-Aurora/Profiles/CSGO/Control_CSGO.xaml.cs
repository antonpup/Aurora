using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Utils;
using Timer = System.Timers.Timer;

namespace Aurora.Profiles.CSGO;

/// <summary>
/// Interaction logic for Control_CSGO.xaml
/// </summary>
public partial class Control_CSGO
{
    private Application _profileManager;

    private readonly Timer _previewBombTimer;
    private readonly Timer _previewBombRemoveEffectTimer;

    private int _previewKills;
    private int _previewKillshs;

    public Control_CSGO(Application profile)
    {
        InitializeComponent();

        _profileManager = profile;

        SetSettings();

        _previewBombTimer = new Timer(45000);
        _previewBombTimer.Elapsed += preview_bomb_timer_Tick;

        _previewBombRemoveEffectTimer = new Timer(5000);
        _previewBombRemoveEffectTimer.Elapsed += preview_bomb_remove_effect_timer_Tick;

        _profileManager.ProfileChanged += Profile_manager_ProfileChanged;

        //Copy cfg file if needed
        if (!(_profileManager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled)
        {
            InstallGSI();
            (_profileManager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled = true;
        }
    }

    private void Profile_manager_ProfileChanged(object? sender, EventArgs e)
    {
        SetSettings();
    }

    private void SetSettings()
    {
        game_enabled.IsChecked = _profileManager.Settings.IsEnabled;
        preview_team.Items.Clear();
        preview_team.Items.Add(PlayerTeam.Undefined);
        preview_team.Items.Add(PlayerTeam.CT);
        preview_team.Items.Add(PlayerTeam.T);
        preview_team.SelectedItem = PlayerTeam.Undefined;
    }

    private void preview_bomb_timer_Tick(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(
            () =>
            {
                preview_bomb_defused.IsEnabled = false;
                preview_bomb_start.IsEnabled = true;

                (_profileManager.Config.Event._game_state as GameState_CSGO).Round.Bomb = BombState.Exploded;
                _previewBombTimer.Stop();

                _previewBombRemoveEffectTimer.Start();
            });
    }

    private void preview_bomb_remove_effect_timer_Tick(object? sender, EventArgs e)
    {
        (_profileManager.Config.Event._game_state as GameState_CSGO).Round.Bomb = BombState.Undefined;
        _previewBombRemoveEffectTimer.Stop();
    }

    //Overview

    private void patch_button_Click(object? sender, RoutedEventArgs e)
    {
        if (InstallGSI())
            MessageBox.Show("Aurora GSI Config file installed successfully.");
        else
            MessageBox.Show("Aurora GSI Config file could not be installed.\r\nGame is not installed.");
    }

    private void unpatch_button_Click(object? sender, RoutedEventArgs e)
    {
        if (UninstallGSI())
            MessageBox.Show("Aurora GSI Config file uninstalled successfully.");
        else
            MessageBox.Show("Aurora GSI Config file could not be uninstalled.\r\nGame is not installed.");
    }

    private void game_enabled_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded)
        {
            _profileManager.Settings.IsEnabled = game_enabled.IsChecked.HasValue ? game_enabled.IsChecked.Value : false;
            _profileManager.SaveProfiles();
        }
    }

    ////Preview

    private void preview_team_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (preview_team.Items.Count == 0)
            return;
        (_profileManager.Config.Event._game_state as GameState_CSGO).Player.Team = (PlayerTeam)preview_team.SelectedItem;
    }

    private void preview_health_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var hp_val = (int)preview_health_slider.Value;
        if (preview_health_amount is Label)
        {
            preview_health_amount.Content = hp_val + "%";
            (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.Health = hp_val;
        }
    }

    private void preview_ammo_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var ammo_val = (int)preview_ammo_slider.Value;
        if (preview_ammo_amount is Label)
        {
            preview_ammo_amount.Content = ammo_val + "%";
            (_profileManager.Config.Event._game_state as GameState_CSGO).Player.Weapons.ActiveWeapon.AmmoClip = ammo_val;
            (_profileManager.Config.Event._game_state as GameState_CSGO).Player.Weapons.ActiveWeapon.AmmoClipMax = 100;
        }
    }

    private void preview_flash_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var flash_val = (int)preview_flash_slider.Value;
        if (preview_flash_amount is Label)
        {
            preview_flash_amount.Content = flash_val + "%";
            (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.Flashed = (int)(flash_val / 100.0 * 255.0);
        }
    }

    private void preview_burning_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var burning_val = (int)preview_burning_slider.Value;
        if (preview_burning_amount is Label)
        {
            preview_burning_amount.Content = burning_val + "%";
            (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.Burning = (int)(burning_val / 100.0 * 255.0);
        }
    }

    private void preview_bomb_start_Click(object? sender, RoutedEventArgs e)
    {
        preview_bomb_defused.IsEnabled = true;
        preview_bomb_start.IsEnabled = false;

        (_profileManager.Config.Event._game_state as GameState_CSGO).Round.Bomb = BombState.Planted;
        _previewBombTimer.Start();
        _previewBombRemoveEffectTimer.Stop();
    }

    private void preview_bomb_defused_Click(object? sender, RoutedEventArgs e)
    {
        preview_bomb_defused.IsEnabled = false;
        preview_bomb_start.IsEnabled = true;

        (_profileManager.Config.Event._game_state as GameState_CSGO).Round.Bomb = BombState.Defused;
        _previewBombTimer.Stop();
        _previewBombRemoveEffectTimer.Start();
    }

    private void preview_typing_enabled_Checked(object? sender, RoutedEventArgs e)
    {
        (_profileManager.Config.Event._game_state as GameState_CSGO).Player.Activity = preview_typing_enabled.IsChecked.HasValue &&
            preview_typing_enabled.IsChecked.Value ? PlayerActivity.TextInput : PlayerActivity.Undefined;
    }

    private void preview_respawn_Click(object? sender, RoutedEventArgs e)
    {
        (_profileManager.Config.Event._game_state as GameState_CSGO).Provider.SteamID = (_profileManager.Config.Event._game_state as GameState_CSGO).Player.SteamID;

        (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.Health = 100;
        (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.Health = 99;

        var curr_hp_val = (int)preview_health_slider.Value;

        System.Threading.Timer reset_conditions_timer = null;
        reset_conditions_timer = new System.Threading.Timer(obj =>
            {
                (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.Health = curr_hp_val;
                (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.Health = 100;

                reset_conditions_timer.Dispose();
            },
            null, 500, Timeout.Infinite);
    }

    private void preview_addkill_hs_Click(object? sender, RoutedEventArgs e)
    {
        (_profileManager.Config.Event._game_state as GameState_CSGO).Provider.SteamID = (_profileManager.Config.Event._game_state as GameState_CSGO).Player.SteamID;

        (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = _previewKills;
        (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = ++_previewKills;
        (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKillHS = _previewKillshs;
        (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKillHS = ++_previewKillshs;

        System.Threading.Timer reset_conditions_timer = null;
        reset_conditions_timer = new System.Threading.Timer(obj =>
            {
                (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = _previewKills;
                (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = _previewKills;
                (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKillHS = _previewKillshs;
                (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKillHS = _previewKillshs;

                reset_conditions_timer.Dispose();
            },
            null, 500, Timeout.Infinite);

        preview_kills_label.Text = string.Format("Kills: {0} Headshots: {1}", _previewKills, _previewKillshs);
    }

    private void preview_addkill_Click(object? sender, RoutedEventArgs e)
    {
        (_profileManager.Config.Event._game_state as GameState_CSGO).Provider.SteamID = (_profileManager.Config.Event._game_state as GameState_CSGO).Player.SteamID;

        (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = _previewKills;
        (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = ++_previewKills;

        System.Threading.Timer reset_conditions_timer = null;
        reset_conditions_timer = new System.Threading.Timer(obj =>
            {
                (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = _previewKills;
                (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = _previewKills;

                reset_conditions_timer.Dispose();
            },
            null, 500, Timeout.Infinite);

        preview_kills_label.Text = string.Format("Kills: {0} Headshots: {1}", _previewKills, _previewKillshs);
    }
    private void preview_kills_reset_Click(object? sender, RoutedEventArgs e)
    {
        (_profileManager.Config.Event._game_state as GameState_CSGO).Provider.SteamID = (_profileManager.Config.Event._game_state as GameState_CSGO).Player.SteamID;

        (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = _previewKills;
        (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = 0;
        (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKillHS = _previewKillshs;
        (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKillHS = 0;

        System.Threading.Timer reset_conditions_timer = null;
        reset_conditions_timer = new System.Threading.Timer(obj =>
            {
                (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKills = 0;
                (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKills = 0;
                (_profileManager.Config.Event._game_state as GameState_CSGO).Previously.Player.State.RoundKillHS = 0;
                (_profileManager.Config.Event._game_state as GameState_CSGO).Player.State.RoundKillHS = 0;

                reset_conditions_timer.Dispose();
            },
            null, 500, Timeout.Infinite);

        _previewKills = 0;
        _previewKillshs = 0;

        preview_kills_label.Text = $"Kills: {_previewKills} Headshots: {_previewKillshs}";
    }

    private bool InstallGSI()
    {
        var installPath = SteamUtils.GetGamePath(730);
        if (string.IsNullOrWhiteSpace(installPath)) return false;
        var path = Path.Combine(installPath, "csgo", "cfg", "gamestate_integration_aurora.cfg");

        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        using var cfgStream = File.Create(path);
        cfgStream.Write(Properties.Resources.gamestate_integration_aurora_csgo, 0, Properties.Resources.gamestate_integration_aurora_csgo.Length);

        return true;
    }

    private bool UninstallGSI()
    {
        var installPath = SteamUtils.GetGamePath(730);
        if (string.IsNullOrWhiteSpace(installPath)) return false;
        var path = Path.Combine(installPath, "csgo", "cfg", "gamestate_integration_aurora.cfg");

        if (File.Exists(path))
            File.Delete(path);

        return true;

    }
}