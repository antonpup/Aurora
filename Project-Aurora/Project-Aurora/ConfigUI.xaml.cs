using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Controls;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Modules.GameStateListen;
using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Profiles.Generic_Application;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Utils;
using Hardcodet.Wpf.TaskbarNotification;
using JetBrains.Annotations;
using PropertyChanged;
using RazerSdkWrapper;
using Application = Aurora.Profiles.Application;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace Aurora;

[DoNotNotify]
partial class ConfigUI : INotifyPropertyChanged
{
    private readonly  Control_Settings _settingsControl;
    private readonly Control_LayerControlPresenter _layerPresenter = new();
    private readonly Control_ProfileControlPresenter _profilePresenter = new();

    private readonly EffectColor _desktopColorScheme = new(0, 0, 0);

    private readonly EffectColor _transitionColor = new();
    private EffectColor _currentColor = new();

    private float _transitionAmount;

    private FrameworkElement? _selectedItem;
    private FrameworkElement? _selectedManager;

    private bool _settingsLoaded;
    private bool _shownHiddenMessage;

    private string _savedPreviewKey = "";

    private Timer? _virtualKeyboardTimer;
    private Grid _virtualKb = new();

    public static readonly DependencyProperty FocusedApplicationProperty = DependencyProperty.Register(
        "FocusedApplication", typeof(Application), typeof(ConfigUI),
        new PropertyMetadata(null, FocusedProfileChanged));

    private readonly Task<KeyboardLayoutManager> _layoutManager;
    private readonly Task<AuroraHttpListener?> _httpListener;

    public Application? FocusedApplication
    {
        get => (Application)GetValue(FocusedApplicationProperty);
        set
        {
            SetValue(FocusedApplicationProperty, value);
            Global.LightingStateManager.PreviewProfileKey = value != null ? value.Config.ID : string.Empty;
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

    public ConfigUI(Task<RzSdkManager?> rzSdkManager, Task<PluginManager> pluginManager,
        Task<KeyboardLayoutManager> layoutManager, Task<AuroraHttpListener?> httpListener)
    {
        _httpListener = httpListener;
        _layoutManager = layoutManager;
        _settingsControl = new(rzSdkManager, pluginManager, layoutManager, httpListener);
        InitializeComponent();

        _layoutManager.Result.KeyboardLayoutUpdated += KbLayout_KeyboardLayoutUpdated;

        ctrlProfileManager.ProfileSelected += CtrlProfileManager_ProfileSelected;

        GenerateProfileStack();
        _settingsControl.DataContext = this;
    }

    internal void DisplayIfNotSilent()
    {
        if (App.IsSilent)
        {
            Visibility = Visibility.Hidden;
            WindowStyle = WindowStyle.None;
            ShowInTaskbar = false;
            Hide();
        }
        else
        {
            Display();
        }
    }

    private void Display()
    {
        ShowInTaskbar = true;
        WindowStyle = WindowStyle.SingleBorderWindow;
        if (Top <= 0)
        {
            Top = 0;
        }

        if (Left <= 0)
        {
            Left = 0;
        }
        Show();
        Focus();
    }

    private void CtrlProfileManager_ProfileSelected(ApplicationProfile profile)
    {
        _profilePresenter.Profile = profile;

        if (_selectedManager.Equals(ctrlProfileManager))
            SelectedControl = _profilePresenter;   
    }

    private void KbLayout_KeyboardLayoutUpdated(object sender)
    {
        _virtualKb = _layoutManager.Result.VirtualKeyboard;

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

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_settingsLoaded)
        {
            _virtualKeyboardTimer = new Timer(8);
            _virtualKeyboardTimer.Elapsed += virtual_keyboard_timer_Tick;
            _virtualKeyboardTimer.Start();

            _settingsLoaded = true;
        }

        keyboard_record_message.Visibility = Visibility.Hidden;

        _currentColor = _desktopColorScheme;
        bg_grid.Background = new SolidColorBrush(Color.FromRgb(_desktopColorScheme.Red, _desktopColorScheme.Green, _desktopColorScheme.Blue));

        _virtualKb = _layoutManager.Result.VirtualKeyboard;

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
            if (child.Visibility == Visibility.Visible)
            {
                ProfileImage_MouseDown(child, null);
                break;
            }
        }
    }

    public static bool ApplicationIsActivated()
    {
        var activatedHandle = GetForegroundWindow();
        if (activatedHandle == IntPtr.Zero)
            return false;       // No window is currently activated

        var procId = Process.GetCurrentProcess().Id;
        int activeProcId;
        GetWindowThreadProcessId(activatedHandle, out activeProcId);

        return activeProcId == procId;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

    private void virtual_keyboard_timer_Tick(object sender, EventArgs e)
    {
        if (!ApplicationIsActivated())
            return;

        Dispatcher.Invoke(
            () =>
            {
                if (_transitionAmount <= 1.0f)
                {
                    _transitionColor.BlendColors(_currentColor, _transitionAmount += 0.07f);

                    bg_grid.Background = new SolidColorBrush(Color.FromRgb(_transitionColor.Red,
                        _transitionColor.Green, _transitionColor.Blue));
                    bg_grid.UpdateLayout();
                }

                Dictionary<DeviceKeys, System.Drawing.Color> keylights = Global.effengine.GetKeyboardLights();
                _layoutManager.Result.SetKeyboardColors(keylights);

                keyboard_record_message.Visibility =
                    Global.key_recorder.IsRecording() ? Visibility.Visible : Visibility.Hidden;
            });
    }

    ////Misc

    private void trayicon_menu_quit_Click(object sender, RoutedEventArgs e)
    {
        ExitApp();
    }

    private void trayicon_menu_settings_Click(object sender, RoutedEventArgs e)
    {
        Display();
    }

    private void Window_Initialized(object sender, EventArgs e)
    {
        //unused
    }

    private void Window_Closing(object sender, CancelEventArgs e)
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
        _virtualKeyboardTimer?.Stop();
        System.Windows.Application.Current.Shutdown();
    }

    private void MinimizeApp()
    {
        FocusedApplication?.SaveAll();

        if (!_shownHiddenMessage)
        {
            trayicon.ShowBalloonTip("Aurora", "This program is now hidden in the tray.", BalloonIcon.None);
            _shownHiddenMessage = true;
        }

        Global.LightingStateManager.PreviewProfileKey = string.Empty;

        Visibility = Visibility.Hidden;
        Hide();
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        Global.LightingStateManager.PreviewProfileKey = _savedPreviewKey;
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        _savedPreviewKey = Global.LightingStateManager.PreviewProfileKey;
        Global.LightingStateManager.PreviewProfileKey = string.Empty;
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
        
    private void GenerateProfileStack(string focusedKey = null)
    {
        _selectedItem = null;
        profiles_stack.Children.Clear();

        foreach (var application in Global.Configuration.ProfileOrder
                     .Where(profileName => Global.LightingStateManager.Events.ContainsKey(profileName))
                     .Select(profileName => (Application)Global.LightingStateManager.Events[profileName])
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
                    Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
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

    private void HiddenProfile_MouseDown(object sender, EventArgs e)
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

    private void mbtnHidden_Checked(object sender, RoutedEventArgs e)
    {
        MenuItem btn = (MenuItem)sender;

        if (cmenuProfiles.PlacementTarget is not Image img) return;
        img.Opacity = btn.IsChecked ? 0.5 : 1;

        if (!ShowHidden && btn.IsChecked)
            img.Visibility = Visibility.Collapsed;

        (img.Tag as Application)?.SaveProfiles();
    }

    private void cmenuProfiles_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (((ContextMenu)e.Source).PlacementTarget is not Image)
            e.Handled = true;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        ContextMenu context = (ContextMenu)e.OriginalSource;

        if (!(context.PlacementTarget is Image img))
            return;

        Application profile = img.Tag as Application;
        context.DataContext = profile;
    }

    private void Profile_grid_MouseLeave(object sender, MouseEventArgs e)
    {
        if ((sender as Grid)?.Tag is Image)
            ((Image)((Grid)sender).Tag).Visibility = Visibility.Hidden;
    }

    private void Profile_grid_MouseEnter(object sender, MouseEventArgs e)
    {
        if ((sender as Grid)?.Tag is Image)
            ((Image)((Grid)sender).Tag).Visibility = Visibility.Visible;
    }

    private void TransitionToProfile(Image source)
    {
        FocusedApplication = source.Tag as Application;
        var bitmap = (BitmapSource)source.Source;
        var color = ColorUtils.GetAverageColor(bitmap);

        _currentColor = new EffectColor(color);
        _currentColor *= 0.85f;

        _transitionAmount = 0.0f;

        UpdateProfileStackBackground(source);
    }

    private void ProfileImage_MouseDown(object sender, MouseButtonEventArgs? e)
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

    private void RemoveProfile_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Image { Tag: string } image)
        {
            return;
        }

        string name = (string)image.Tag;

        if (!Global.LightingStateManager.Events.ContainsKey(name)) return;
        if (MessageBox.Show(
                "Are you sure you want to delete profile for " +
                (((Application)Global.LightingStateManager.Events[name]).Settings as
                    GenericApplicationSettings).ApplicationName + "?", "Remove Profile", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        var eventList = Global.Configuration.ProfileOrder
            .ToDictionary(x => x, x => Global.LightingStateManager.Events[x])
            .Where(x => ShowHidden || !(x.Value as Application).Settings.Hidden)
            .ToList();
        var idx = Math.Max(eventList.FindIndex(x => x.Key == name), 0);
        Global.LightingStateManager.RemoveGenericProfile(name);
        GenerateProfileStack(eventList[idx].Key);
    }

    private void AddProfile_MouseDown(object sender, MouseButtonEventArgs e)
    {

        Window_ProcessSelection dialog = new Window_ProcessSelection { CheckCustomPathExists = true, ButtonLabel = "Add Profile", Title ="Add Profile" };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ChosenExecutablePath))
            return; // do not need to check if dialog is already in excluded_programs since it is a Set and only contains unique items by definition

        string filename = Path.GetFileName(dialog.ChosenExecutablePath.ToLowerInvariant());

        if (Global.LightingStateManager.Events.ContainsKey(filename))
        {
            if (Global.LightingStateManager.Events[filename] is GameEvent_Aurora_Wrapper)
                Global.LightingStateManager.Events.Remove(filename);
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

        Global.LightingStateManager.RegisterEvent(genAppPm);
        ConfigManager.Save(Global.Configuration);
        GenerateProfileStack(filename);
    }

    private void DesktopControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        FocusedApplication = null;
        SelectedControl = _settingsControl;

        _currentColor = _desktopColorScheme;
        _transitionAmount = 0.0f;

        UpdateProfileStackBackground(sender as FrameworkElement);
    }
    private void cmbtnOpenBitmapWindow_Clicked(object sender, RoutedEventArgs e) => Window_BitmapView.Open();
    private void cmbtnOpenHttpDebugWindow_Clicked(object sender, RoutedEventArgs e) =>Window_GSIHttpDebug.Open(_httpListener);

    private void UpdateProfileStackBackground(FrameworkElement? item)
    {
        _selectedItem = item;

        if (_selectedItem == null) return;
        DrawingBrush mask = new DrawingBrush();
        GeometryDrawing visibleRegion =
            new GeometryDrawing(
                new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)),
                null,
                new RectangleGeometry(new Rect(0, 0, profiles_background.ActualWidth, profiles_background.ActualHeight)));

        DrawingGroup drawingGroup = new DrawingGroup();
        drawingGroup.Children.Add(visibleRegion);

        Point relativePoint = _selectedItem.TransformToAncestor(profiles_background)
            .Transform(new Point(0, 0));

        double x = 0.0D;
        double y = relativePoint.Y - 2.0D;
        double width = profiles_background.ActualWidth;
        double height = _selectedItem.ActualHeight + 4.0D;

        if (item.Parent != null && item.Parent.Equals(profiles_stack))
        {
            Point relativePointWithinStack = profiles_stack.TransformToAncestor(profiles_background)
                .Transform(new Point(0, 0));

            if (y < relativePointWithinStack.Y)
            {
                height -= relativePointWithinStack.Y - y;
                y = 0;
            }
            else if (y + height > profiles_background.ActualHeight - 40)
                height -= (y + height) - (profiles_background.ActualHeight - 40);

        }
        else
        {
            x = 0.0D;
            y = relativePoint.Y - 2.0D;
            width = profiles_background.ActualWidth;
            height = _selectedItem.ActualHeight + 4.0D;

            if (y + height > profiles_background.ActualHeight - 40)
                height -= (y + height) - (profiles_background.ActualHeight - 40);
        }
                
        if (height > 0 && width > 0)
        {
            GeometryDrawing transparentRegion =
                new GeometryDrawing(
                    new SolidColorBrush((Color)_currentColor),
                    null,
                    new RectangleGeometry(new Rect(x, y, width, height)));

            drawingGroup.Children.Add(transparentRegion);
        }

        mask.Drawing = drawingGroup;

        profiles_background.Background = mask;
    }

    private void ShowWindow()
    {
        Global.logger.Info("Show Window called");
        Display();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState != WindowState.Normal)
        {
            return;
        }

        base.OnStateChanged(e);
    }

    private void trayicon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        ShowWindow();
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        UpdateProfileStackBackground(_selectedItem);
    }

    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateProfileStackBackground(_selectedItem);
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
                child.Height = totalHeight - (28.0 * (stackPanelManagers.Children.Count - 1));
            else
                child.Height = 25.0;
        }
        _selectedManager.RaiseEvent(new RoutedEventArgs(GotFocusEvent));
    }

    private void ctrlLayerManager_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!sender.Equals(_selectedManager))
            SelectedControl = FocusedApplication.Profile.Layers.Count > 0 ? _layerPresenter : FocusedApplication.Control;
        UpdateManagerStackFocus(sender);
    }

    private void ctrlOverlayLayerManager_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
        if (!sender.Equals(_selectedManager))
            SelectedControl = FocusedApplication.Profile.OverlayLayers.Count > 0 ? _layerPresenter : FocusedApplication.Control;
        UpdateManagerStackFocus(sender);
    }

    private void ctrlProfileManager_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!sender.Equals(_selectedManager))
            SelectedControl = _profilePresenter;
        UpdateManagerStackFocus(sender);
    }

    private void brdOverview_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _selectedManager = SelectedControl = FocusedApplication.Control;

    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
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
    #endregion
}