using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Controls;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Modules.GameStateListen;
using Aurora.Profiles;
using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Profiles.Generic_Application;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Hardcodet.Wpf.TaskbarNotification;
using PropertyChanged;
using RazerSdkWrapper;
using SourceChord.FluentWPF;
using static Aurora.Utils.Win32Transparency;
using Application = Aurora.Profiles.Application;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Timer = System.Timers.Timer;

namespace Aurora;

[DoNotNotify]
partial class ConfigUI : INotifyPropertyChanged
{
    private readonly  Control_Settings _settingsControl;
    private readonly Control_LayerControlPresenter _layerPresenter = new();
    private readonly Control_ProfileControlPresenter _profilePresenter = new();

    private readonly EffectColor _desktopColorScheme = new(0, 0, 0, 0);

    private EffectColor _previousColor = new(0, 0, 0, 0);
    private EffectColor _currentColor = new(0, 0, 0, 0);

    private double _transitionAmount;

    private FrameworkElement? _selectedManager;

    private bool _shownHiddenMessage;

    private string _savedPreviewKey = "";

    private readonly Timer _virtualKeyboardTimer = new(8);
    private readonly Action _keyboardTimerCallback;
    private Grid _virtualKb = new();

    public static readonly DependencyProperty FocusedApplicationProperty = DependencyProperty.Register(
        nameof(FocusedApplication), typeof(Application), typeof(ConfigUI),
        new PropertyMetadata(null, FocusedProfileChanged));

    private readonly Task<KeyboardLayoutManager> _layoutManager;
    private readonly Task<AuroraHttpListener?> _httpListener;
    private readonly Task<IpcListener?> _ipcListener;
    private readonly Task<LightingStateManager> _lightingStateManager;

    private HwndSource _hwHandle;
    private readonly bool _useMica;

    public Application? FocusedApplication
    {
        get => (Application)GetValue(FocusedApplicationProperty);
        set
        {
            SetValue(FocusedApplicationProperty, value);
            _lightingStateManager.Result.PreviewProfileKey = value != null ? value.Config.ID : string.Empty;
        }
    }

    private bool _showHidden;

    public bool ShowHidden
    {
        get => _showHidden;
        set
        {
            _showHidden = value;
            ShowHiddenChanged(value);
        }
    }

    #region Mica

    private void Window_ContentRendered(object? sender, EventArgs e)
    {
        UpdateStyleAttributes();
    }

    public void UpdateStyleAttributes()
    {
        _lightThemeRegistryWatcher = new RegistryWatcher(RegistryHiveOpt.CurrentUser,
            @"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
            "AppsUseLightTheme");
        _lightThemeRegistryWatcher.RegistryChanged += LightThemeRegistryWatcherOnRegistryChanged;
        _lightThemeRegistryWatcher.StartWatching();
    }

    private void LightThemeRegistryWatcherOnRegistryChanged(object? sender, RegistryChangedEventArgs e)
    {
        
        if (e.Data is not int lightThemeEnabled)
        {
            return;
        }

        Dispatcher.Invoke(() =>
        {
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
            _hwHandle = HwndSource.FromHwnd(mainWindowPtr);
            var darkThemeEnabled = lightThemeEnabled == 0;

            NoiseOpacity = 0.005;

            if (_useMica)
            {
                var trueValue = 0x01;
                var falseValue = 0x00;

                SetAcrylicAccentState(this, AcrylicAccentState.Disabled);

                // Set dark mode before applying the material, otherwise you'll get an ugly flash when displaying the window.
                if (darkThemeEnabled)
                    DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE,
                        ref trueValue, Marshal.SizeOf(typeof(int)));
                else
                    DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE,
                        ref falseValue, Marshal.SizeOf(typeof(int)));

                DwmSetWindowAttribute(_hwHandle.Handle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue,
                    Marshal.SizeOf(typeof(int)));
                TintColor = Color.FromArgb(0, 0, 0, 0);
                FallbackColor = Color.FromArgb(1, 0, 0, 0);
            }
            else
            {
                TintColor = Color.FromArgb(240, 128, 128, 128);
                Activate();
            }
        });
    }

    private void Window_Loaded_Mica(object? sender, RoutedEventArgs e)
    {
        // Get PresentationSource
        PresentationSource presentationSource = PresentationSource.FromVisual((Visual)sender);

        // Subscribe to PresentationSource's ContentRendered event
        presentationSource.ContentRendered += Window_ContentRendered;
    }

    #endregion

    public ConfigUI(Task<RzSdkManager?> rzSdkManager, Task<PluginManager> pluginManager,
        Task<KeyboardLayoutManager> layoutManager, Task<AuroraHttpListener?> httpListener,
        Task<IpcListener?> ipcListener, Task<DeviceManager> deviceManager, Task<LightingStateManager> lightingStateManager)
    {
        _httpListener = httpListener;
        _layoutManager = layoutManager;
        _ipcListener = ipcListener;
        _lightingStateManager = lightingStateManager;
        _settingsControl = new(rzSdkManager, pluginManager, layoutManager, httpListener, deviceManager);

        _useMica = Environment.OSVersion.Version.Build >= 22000;
        InitializeComponent();

        ContentRendered += Window_ContentRendered;
        ctrlProfileManager.ProfileSelected += CtrlProfileManager_ProfileSelected;

        _settingsControl.DataContext = this;

        _keyboardTimerCallback = () =>
        {
            if (_transitionAmount <= 1.0f)
            {
                _transitionAmount += _keyboardTimer.Elapsed.TotalSeconds;
                var smooth = 1 - Math.Pow(1 - Math.Min(_transitionAmount, 1d), 3);
                var a = EffectColor.BlendColors(_previousColor, _currentColor, smooth);

                if (!Global.Configuration.AllowTransparency)
                {
                    FallbackColor = Colors.Black;
                    TintColor = Colors.Transparent;
                    bg_grid.Background =
                        new SolidColorBrush(Color.FromArgb(255, a.Red, a.Green, a.Blue));
                }
                else if (_useMica)
                {
                    bg_grid.Background =
                        new SolidColorBrush(Color.FromArgb((byte)(a.Alpha * 64 / 255), a.Red, a.Green, a.Blue));
                    FallbackColor = Colors.Transparent;
                    TintColor = Colors.Transparent;
                }
                else
                {
                    var color = (Color)a;
                    TintColor = color;
                    FallbackColor = color with {A = (byte)(color.A * 0.8)};
                }
            }

            var keyLights = Global.effengine.GetKeyboardLights();
            _layoutManager.Result.SetKeyboardColors(keyLights);

            KeyboardRecordMessage.Visibility = Global.key_recorder.IsRecording() ? Visibility.Visible : Visibility.Hidden;
        };
        _virtualKeyboardTimer.Elapsed += virtual_keyboard_timer_Tick;
    }

    public async Task Initialize()
    {
        await GenerateProfileStack();

        await _settingsControl.Initialize();
        
        (await _layoutManager).KeyboardLayoutUpdated += KbLayout_KeyboardLayoutUpdated;

        var ipcListener = await _ipcListener;
        if (ipcListener != null)
        {
            ipcListener.AuroraCommandReceived += OnAuroraCommandReceived;
        }
    }

    internal void DisplayIfNotSilent()
    {
        if (!App.IsSilent)
        {
            Display();
        }
    }

    private void OnAuroraCommandReceived(object? sender, string e)
    {
        switch (e)
        {
            case "restore":
                Dispatcher.Invoke(Display);
                break;
        }
    }

    private void Display()
    {
        ShowInTaskbar = true;
        if (Top <= 0)
        {
            Top = 0;
        }

        if (Left <= 0)
        {
            Left = 0;
        }
        if (WindowStyle != WindowStyle.None)
        {
            WindowStyle = WindowStyle.None;
        }
        Show();
        Activate();
        _virtualKeyboardTimer.Start();
    }

    private void Restart()
    {
        var auroraPath = Path.Combine(Global.ExecutingDirectory, "Aurora.exe");

        var currentProcess = Environment.ProcessId;
        var minimizedArg = Visibility == Visibility.Visible ? "" : " -minimized";
        Process.Start(new ProcessStartInfo
        {
            FileName = auroraPath,
            Arguments = "-restart " + currentProcess + minimizedArg
        });

        ExitApp();
    }

    private void CtrlProfileManager_ProfileSelected(ApplicationProfile profile)
    {
        _profilePresenter.Profile = profile;

        if (_selectedManager.Equals(ctrlProfileManager))
            SelectedControl = _profilePresenter;   
    }

    private async void KbLayout_KeyboardLayoutUpdated(object? sender)
    {
        _virtualKb = (await _layoutManager).VirtualKeyboard;

        keyboard_grid.Children.Clear();
        keyboard_grid.Children.Add(_virtualKb);
        keyboard_grid.Children.Add(new LayerEditor());

        keyboard_grid.Width = _virtualKb.Width;

        keyboard_grid.Height = _virtualKb.Height;

        keyboard_grid.UpdateLayout();

        keyboard_viewbox.MaxWidth = _virtualKb.Width + 50;
        keyboard_viewbox.MaxHeight = _virtualKb.Height + 50;
        keyboard_viewbox.UpdateLayout();

        UpdateLayout();
    }

    private async void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        KeyboardRecordMessage.Visibility = Visibility.Hidden;

        _currentColor = _desktopColorScheme;
        bg_grid.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));

        _virtualKb = (await _layoutManager).VirtualKeyboard;

        keyboard_grid.Children.Clear();
        keyboard_grid.Children.Add(_virtualKb);
        keyboard_grid.Children.Add(new LayerEditor());

        keyboard_grid.Width = _virtualKb.Width;

        keyboard_grid.Height = _virtualKb.Height;

        keyboard_grid.UpdateLayout();

        keyboard_viewbox.MaxWidth = _virtualKb.Width + 50;
        keyboard_viewbox.MaxHeight = _virtualKb.Height + 50;
        keyboard_viewbox.UpdateLayout();

        UpdateManagerStackFocus(ctrlLayerManager);

        UpdateLayout();

        foreach (Image child in profiles_stack.Children)
        {
            if (child.Visibility != Visibility.Visible) continue;
            ProfileImage_MouseDown(child, null);
            break;
        }
        
        Window_Loaded_Mica(sender, e);

    }

    private readonly Stopwatch _keyboardTimer = Stopwatch.StartNew();
    private void virtual_keyboard_timer_Tick(object? sender, EventArgs e)
    {
        if (Visibility != Visibility.Visible) return;
        Dispatcher.Invoke(_keyboardTimerCallback);
        _keyboardTimer.Restart();
    }

    ////Misc

    private void trayicon_menu_quit_Click(object? sender, RoutedEventArgs e)
    {
        //TODO make async
        ExitApp();
    }

    private void trayicon_menu_settings_Click(object? sender, RoutedEventArgs e)
    {
        Display();
    }

    private void trayicon_menu_restart_Click(object? sender, RoutedEventArgs e)
    {
        Restart();
    }

    private void Window_Initialized(object? sender, EventArgs e)
    {
        //unused
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        switch (Global.Configuration.CloseMode)
        {
            case AppExitMode.Ask:
            {
                MessageBoxResult result = MessageBox.Show("Would you like to Exit Aurora?",
                    "Aurora", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    MinimizeApp();
                    e.Cancel = true;
                }
                else
                {
                    ExitApp();
                }

                break;
            }
            case AppExitMode.Minimize:
                MinimizeApp();
                e.Cancel = true;
                break;
            default:
                ExitApp();
                break;
        }
    }

    private void ExitApp()
    {
        trayicon.Visibility = Visibility.Hidden;
        _virtualKeyboardTimer.Stop();
        System.Windows.Application.Current.Shutdown();
    }

    private async void MinimizeApp()
    {
        FocusedApplication?.SaveAll();

        if (!_shownHiddenMessage)
        {
            trayicon.ShowBalloonTip("Aurora", "This program is now hidden in the tray.", BalloonIcon.None);
            _shownHiddenMessage = true;
        }

        var lightingStateManager = await _lightingStateManager;
        lightingStateManager.PreviewProfileKey = string.Empty;

        Visibility = Visibility.Hidden;
        Hide();
    }

    private async void Window_Activated(object? sender, EventArgs e)
    {
        var lightingStateManager = await _lightingStateManager;
        lightingStateManager.PreviewProfileKey = _savedPreviewKey;
    }

    private async void Window_Deactivated(object? sender, EventArgs e)
    {
        var lightingStateManager = await _lightingStateManager;
        _savedPreviewKey = lightingStateManager.PreviewProfileKey;
        lightingStateManager.PreviewProfileKey = string.Empty;
    }

    private readonly Image _profileAdd = new()
    {
        Source = new BitmapImage(new Uri(@"Resources/addprofile_icon.png", UriKind.Relative)),
        ToolTip = "Add a new Lighting Profile",
        Margin = new Thickness(0, 5, 0, 0)
    };

    private Image _profileHidden;

    private readonly BitmapImage _visible = new(new Uri(@"Resources/Visible.png", UriKind.Relative));
    private readonly BitmapImage _notVisible = new(new Uri(@"Resources/Not Visible.png", UriKind.Relative));
        
    private async Task GenerateProfileStack(string focusedKey = null)
    {
        profiles_stack.Children.Clear();

        var lightingStateManager = await _lightingStateManager;
        foreach (var application in Global.Configuration.ProfileOrder
                     .Where(profileName =>
                     {
                         return lightingStateManager.Events.ContainsKey(profileName);
                     })
                     .Select(profileName => (Application)lightingStateManager.Events[profileName])
                     .OrderBy(item => item.Settings.Hidden))
        {
            ImageSource icon = application.Icon;
            Image profileImage;
            if (application is GenericApplication)
            {
                GenericApplicationSettings settings = (GenericApplicationSettings)application.Settings;
                profileImage = new Image
                {
                    Tag = application,
                    Source = icon,
                    ToolTip = settings.ApplicationName + " Settings",
                    Margin = new Thickness(0, 5, 0, 0)
                };
                profileImage.MouseDown += ProfileImage_MouseDown;

                Image profileRemove = new Image
                {
                    Source = new BitmapImage(new Uri(@"Resources/removeprofile_icon.png", UriKind.Relative)),
                    ToolTip = $"Remove {settings.ApplicationName} Profile",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Height = 16,
                    Width = 16,
                    Visibility = Visibility.Hidden,
                    Tag = application.Config.ID
                };
                profileRemove.MouseDown += RemoveProfile_MouseDown;

                Grid profileGrid = new Grid
                {
                    Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                    Margin = new Thickness(0, 5, 0, 0),
                    Tag = profileRemove
                };

                profileGrid.MouseEnter += Profile_grid_MouseEnter;
                profileGrid.MouseLeave += Profile_grid_MouseLeave;

                profileGrid.Children.Add(profileImage);
                profileGrid.Children.Add(profileRemove);

                profiles_stack.Children.Add(profileGrid);
            }
            else
            {
                profileImage = new Image
                {
                    Tag = application,
                    Source = icon,
                    ToolTip = application.Config.Name + " Settings",
                    Margin = new Thickness(0, 5, 0, 0),
                    Visibility = application.Settings.Hidden ? Visibility.Collapsed : Visibility.Visible
                };
                profileImage.MouseDown += ProfileImage_MouseDown;
                profiles_stack.Children.Add(profileImage);
            }

            if (!application.Config.ID.Equals(focusedKey)) continue;
            FocusedApplication = application;
            TransitionToProfile(profileImage);
        }

        //Add new profiles button
        _profileAdd.MouseDown -= AddProfile_MouseDown;
        _profileAdd.MouseDown += AddProfile_MouseDown;
        profiles_stack.Children.Add(_profileAdd);

        //Show hidden profiles button
        _profileHidden = new Image
        {
            Source = _notVisible,
            ToolTip = "Toggle Hidden profiles' visibility",
            Margin = new Thickness(0, 5, 0, 0)
        };
        _profileHidden.MouseDown += HiddenProfile_MouseDown;
        profiles_stack.Children.Add(_profileHidden);
    }

    private void HiddenProfile_MouseDown(object? sender, EventArgs e)
    {
        ShowHidden = !ShowHidden;
    }

    private void ShowHiddenChanged(bool value)
    {
        _profileHidden.Source = value ? _visible : _notVisible;

        foreach (FrameworkElement ctrl in profiles_stack.Children)
        {
            Image img = ctrl as Image ?? (ctrl is Grid ? ((Grid)ctrl).Children[0] as Image : null);
            if (img == null) continue;
            Application profile = img.Tag as Application;
            if (profile == null) continue;
            img.Visibility = profile.Settings.Hidden && !value ? Visibility.Collapsed : Visibility.Visible;
            img.Opacity = profile.Settings.Hidden ? 0.5 : 1;
        }
    }

    private void mbtnHidden_Checked(object? sender, RoutedEventArgs e)
    {
        MenuItem btn = (MenuItem)sender;

        if (cmenuProfiles.PlacementTarget is not Image img) return;
        img.Opacity = btn.IsChecked ? 0.5 : 1;

        if (!ShowHidden && btn.IsChecked)
            img.Visibility = Visibility.Collapsed;

        (img.Tag as Application)?.SaveProfiles();
    }

    private void cmenuProfiles_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
    {
        if (((ContextMenu)e.Source).PlacementTarget is not Image)
            e.Handled = true;
    }

    private void ContextMenu_Opened(object? sender, RoutedEventArgs e)
    {
        ContextMenu context = (ContextMenu)e.OriginalSource;

        if (!(context.PlacementTarget is Image img))
            return;

        Application profile = img.Tag as Application;
        context.DataContext = profile;
    }

    private void Profile_grid_MouseLeave(object? sender, MouseEventArgs e)
    {
        if ((sender as Grid)?.Tag is Image)
            ((Image)((Grid)sender).Tag).Visibility = Visibility.Hidden;
    }

    private void Profile_grid_MouseEnter(object? sender, MouseEventArgs e)
    {
        if ((sender as Grid)?.Tag is Image)
            ((Image)((Grid)sender).Tag).Visibility = Visibility.Visible;
    }

    private void TransitionToProfile(Image source)
    {
        FocusedApplication = source.Tag as Application;
        var bitmap = (BitmapSource)source.Source;
        var color = ColorUtils.GetAverageColor(bitmap);

        _previousColor = _currentColor;
        _currentColor = new EffectColor(color);
        _currentColor *= 0.85f;

        _transitionAmount = 0.0;
    }

    private void ProfileImage_MouseDown(object? sender, MouseButtonEventArgs? e)
    {
        if (sender is not Image { Tag: Application } image) return;
        if (e == null || e.LeftButton == MouseButtonState.Pressed)
            TransitionToProfile(image);
        else if (e.RightButton == MouseButtonState.Pressed)
        {
            cmenuProfiles.PlacementTarget = image;
            cmenuProfiles.IsOpen = true;
        }
    }

    private static void FocusedProfileChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    {
        ConfigUI th = (ConfigUI)source;
        Application value = e.NewValue as Application;

        th.gridManagers.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;

        if (value == null)
            return;

        th.SelectedControl = value.Control;
    }

    private async void RemoveProfile_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not Image { Tag: string } image)
        {
            return;
        }

        string name = (string)image.Tag;

        var lightingStateManager = await _lightingStateManager;
        if (!lightingStateManager.Events.ContainsKey(name)) return;
        if (MessageBox.Show(
                "Are you sure you want to delete profile for " +
                (((Application)lightingStateManager.Events[name]).Settings as
                    GenericApplicationSettings).ApplicationName + "?", "Remove Profile", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        var eventList = Global.Configuration.ProfileOrder
            .ToDictionary(x => x, x => lightingStateManager.Events[x])
            .Where(x => ShowHidden || !(x.Value as Application).Settings.Hidden)
            .ToList();
        var idx = Math.Max(eventList.FindIndex(x => x.Key == name), 0);
        lightingStateManager.RemoveGenericProfile(name);
        await GenerateProfileStack(eventList[idx].Key);
    }

    private async void AddProfile_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        Window_ProcessSelection dialog = new Window_ProcessSelection { CheckCustomPathExists = true, ButtonLabel = "Add Profile", Title ="Add Profile" };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ChosenExecutablePath))
            return; // do not need to check if dialog is already in excluded_programs since it is a Set and only contains unique items by definition

        string filename = Path.GetFileName(dialog.ChosenExecutablePath.ToLowerInvariant());

        var lightingStateManager = await _lightingStateManager;
        if (lightingStateManager.Events.ContainsKey(filename))
        {
            if (lightingStateManager.Events[filename] is GameEvent_Aurora_Wrapper)
                lightingStateManager.Events.Remove(filename);
            else
            {
                MessageBox.Show("Profile for this application already exists.");
                return;
            }
        }

        GenericApplication genAppPm = new GenericApplication(filename);
        genAppPm.Initialize();
        ((GenericApplicationSettings)genAppPm.Settings).ApplicationName = Path.GetFileNameWithoutExtension(filename);

        Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(dialog.ChosenExecutablePath.ToLowerInvariant());

        if (!Directory.Exists(genAppPm.GetProfileFolderPath()))
            Directory.CreateDirectory(genAppPm.GetProfileFolderPath());

        using (var iconAsbitmap = ico.ToBitmap())
        {
            iconAsbitmap.Save(Path.Combine(genAppPm.GetProfileFolderPath(), "icon.png"), ImageFormat.Png);
        }
        ico.Dispose();

        lightingStateManager.RegisterEvent(genAppPm);
        ConfigManager.Save(Global.Configuration);
        GenerateProfileStack(filename);
    }

    private void DesktopControl_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        FocusedApplication = null;
        SelectedControl = _settingsControl;

        _previousColor = _currentColor;
        _currentColor = _useMica ? _desktopColorScheme : EffectColor.FromRGBA(0, 0, 0, 184);
        _transitionAmount = 0.0;
    }
    private void cmbtnOpenBitmapWindow_Clicked(object? sender, RoutedEventArgs e) => Window_BitmapView.Open();
    private void cmbtnOpenHttpDebugWindow_Clicked(object? sender, RoutedEventArgs e) =>Window_GSIHttpDebug.Open(_httpListener);

    private void trayicon_TrayMouseDoubleClick(object? sender, RoutedEventArgs e)
    {
        Display();
    }

    private void UpdateManagerStackFocus(object focusedElement, bool forced = false)
    {
        if (focusedElement is not FrameworkElement element || (element.Equals(_selectedManager) && !forced)) return;
        _selectedManager = element;
        if(gridManagers.ActualHeight != 0)
            stackPanelManagers.Height = gridManagers.ActualHeight;
        double totalHeight = stackPanelManagers.Height;

        foreach (FrameworkElement child in stackPanelManagers.Children)
        {
            if(child.Equals(element))
                child.Height = totalHeight - 28.0 * (stackPanelManagers.Children.Count - 1);
            else
                child.Height = 25.0;
        }
        _selectedManager.RaiseEvent(new RoutedEventArgs(GotFocusEvent));
    }

    private void ctrlLayerManager_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (!sender.Equals(_selectedManager))
            SelectedControl = FocusedApplication.Profile.Layers.Count > 0 ? _layerPresenter : FocusedApplication.Control;
        UpdateManagerStackFocus(sender);
    }

    private void ctrlOverlayLayerManager_PreviewMouseDown(object? sender, MouseButtonEventArgs e) {
        if (!sender.Equals(_selectedManager))
            SelectedControl = FocusedApplication.Profile.OverlayLayers.Count > 0 ? _layerPresenter : FocusedApplication.Control;
        UpdateManagerStackFocus(sender);
    }

    private void ctrlProfileManager_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (!sender.Equals(_selectedManager))
            SelectedControl = _profilePresenter;
        UpdateManagerStackFocus(sender);
    }

    private void brdOverview_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
    {
        _selectedManager = SelectedControl = FocusedApplication.Control;

    }

    private void Window_SizeChanged(object? sender, SizeChangedEventArgs e) {
        UpdateManagerStackFocus(_selectedManager, true);
    }

    // This new code for the layer selection has been separated from the existing code so that one day we can sort all
    // the above out and make it more WPF with bindings and other dark magic like that.
    #region PropertyChangedEvent and Helpers
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Call the PropertyChangedEvent for a single property.
    /// </summary>
    private void NotifyChanged(string prop) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    /// <summary>
    /// Sets a field and calls <see cref="NotifyChanged(string)"/> with the calling member name and any additional properties.
    /// Designed for setting a field from a property.
    /// </summary>
    private void SetField<T>(ref T var, T value, string[]? additional = null, [CallerMemberName] string name = null) {
        var = value;
        NotifyChanged(name);
        if (additional == null) return;
        foreach (var prop in additional)
            NotifyChanged(prop);
    }
    #endregion

    #region Properties
    /// <summary>A reference to the currently selected layer in either the regular or overlay layer list. When set, will update the <see cref="SelectedControl"/> property.</summary>
    public Layer? SelectedLayer {
        get => _selectedLayer;
        set {
            SetField(ref _selectedLayer, value);
            if (value == null)
                SelectedControl = FocusedApplication?.Control;
            else {
                _layerPresenter.Layer = value;
                SelectedControl = _layerPresenter;
            }
        }
    }
    private Layer? _selectedLayer;

    /// <summary>The control that is currently displayed underneath they device preview panel. This could be an overview control or a layer presenter etc.</summary>
    public Control? SelectedControl { get => _selectedControl; set => SetField(ref _selectedControl, value); }
    private Control? _selectedControl;
    private RegistryWatcher _lightThemeRegistryWatcher;

    #endregion
}
