using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Aurora.EffectsEngine;
using SourceChord.FluentWPF;
using static Aurora.Utils.Win32Transparency;

namespace Aurora.Utils;

public class TransparencyComponent
{
    public static bool UseMica { get; } = Environment.OSVersion.Version.Build >= 22000;

    private readonly AcrylicWindow _window;
    private readonly Panel _bg;
    private readonly RegistryWatcher _lightThemeRegistryWatcher;

    private HwndSource? _hwHandle;

    public TransparencyComponent(AcrylicWindow window, Panel bg)
    {
        _window = window;
        _bg = bg;
        _lightThemeRegistryWatcher = new RegistryWatcher(RegistryHiveOpt.CurrentUser,
            @"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
            "AppsUseLightTheme");
    }

    public void UpdateStyleAttributes()
    {
        _lightThemeRegistryWatcher.RegistryChanged += LightThemeRegistryWatcherOnRegistryChanged;
        _lightThemeRegistryWatcher.StartWatching();
    }

    private void LightThemeRegistryWatcherOnRegistryChanged(object? sender, RegistryChangedEventArgs e)
    {
        
        if (e.Data is not int lightThemeEnabled)
        {
            return;
        }

        _window.Dispatcher.Invoke(() => { SetTransparencyEffect(lightThemeEnabled); });
    }

    private void SetTransparencyEffect(int lightThemeEnabled)
    {
        _hwHandle ??= HwndSource.FromHwnd(new WindowInteropHelper(_window).Handle)!;
        var darkThemeEnabled = lightThemeEnabled == 0;

        if (UseMica && Global.Configuration.AllowTransparency)
        {
            var trueValue = 0x01;
            var falseValue = 0x00;

            AcrylicWindow.SetAcrylicAccentState(_window, AcrylicAccentState.Disabled);

            // Set dark mode before applying the material, otherwise you'll get an ugly flash when displaying the window.
            if (darkThemeEnabled)
                DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE,
                    ref trueValue, Marshal.SizeOf(typeof(int)));
            else
                DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE,
                    ref falseValue, Marshal.SizeOf(typeof(int)));

            DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue,
                Marshal.SizeOf(typeof(int)));
            _window.TintColor = Color.FromArgb(1, 0, 0, 0);
            _window.FallbackColor = Color.FromArgb(64, 0, 0, 0);
        }
        else
        {
            _window.TintColor = Color.FromArgb(240, 128, 128, 128);
            _window.FallbackColor = Color.FromArgb(64, 0, 0, 0);
        }
    }

    public void SetBackgroundColor(EffectColor a)
    {
        if (Global.Configuration.AllowTransparency && UseMica)
        {
            var brush = new SolidColorBrush(Color.FromArgb((byte)(a.Alpha * 64 / 255), a.Red, a.Green, a.Blue));
            brush.Freeze();
            _bg.Background = brush;
        }
        else
        {
            _window.FallbackColor = Colors.Black;
            _window.TintColor = Colors.Transparent;
            var brush = new SolidColorBrush(Color.FromArgb(255, a.Red, a.Green, a.Blue));
            brush.Freeze();
            _bg.Background = brush;
        }
    }
}