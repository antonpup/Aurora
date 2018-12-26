using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Controls;
using Aurora.Profiles.Generic_Application;
using System.IO;
using Aurora.Settings.Keycaps;
using Aurora.Profiles;
using Aurora.Settings.Layers;
using Aurora.Profiles.Aurora_Wrapper;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Aurora
{
    partial class ConfigUI : Window
    {
        Settings.Control_Settings settings_control = new Settings.Control_Settings();
        //Profiles.Desktop.Control_Desktop desktop_control = new Profiles.Desktop.Control_Desktop();

        Control_LayerControlPresenter layercontrol_presenter = new Control_LayerControlPresenter();
        Control_ProfileControlPresenter profilecontrol_presenter = new Control_ProfileControlPresenter();

        EffectColor desktop_color_scheme = new EffectColor(0, 0, 0);

        EffectColor transition_color = new EffectColor();
        EffectColor current_color = new EffectColor();

        private float transitionamount = 0.0f;

        private FrameworkElement selected_item = null;
        private FrameworkElement _selectedManager = null;

        private bool settingsloaded = false;
        private bool shownHiddenMessage = false;

        private string saved_preview_key = "";

        private Timer virtual_keyboard_timer;
        private Stopwatch recording_stopwatch = new Stopwatch();
        private Grid virtial_kb = new Grid();

        private readonly double virtual_keyboard_width;
        private readonly double virtual_keyboard_height;

        private readonly double width;
        private readonly double height;

        public static readonly DependencyProperty FocusedApplicationProperty = DependencyProperty.Register("FocusedApplication", typeof(Profiles.Application), typeof(ConfigUI), new PropertyMetadata(null, new PropertyChangedCallback(FocusedProfileChanged)));

        public Profiles.Application FocusedApplication
        {
            get { return (Profiles.Application)GetValue(FocusedApplicationProperty); }
            set
            {
                SetValue(FocusedApplicationProperty, value);

                Global.LightingStateManager.PreviewProfileKey = value != null ? value.Config.ID : string.Empty;
            }
        }

        LayerEditor layer_editor = new LayerEditor();

        private bool _ShowHidden = false;

        public bool ShowHidden
        {
            get { return _ShowHidden; }
            set
            {
                _ShowHidden = value;
                this.ShowHiddenChanged(value);
            }
        }

        public ConfigUI()
        {
            InitializeComponent();

            virtual_keyboard_height = this.keyboard_grid.Height;
            virtual_keyboard_width = this.keyboard_grid.Width;

            width = Width;
            height = Height;

            Global.kbLayout.KeyboardLayoutUpdated += KbLayout_KeyboardLayoutUpdated;

            ctrlLayerManager.NewLayer += Layer_manager_NewLayer;
            ctrlLayerManager.ProfileOverviewRequest += CtrlLayerManager_ProfileOverviewRequest;

            ctrlProfileManager.ProfileSelected += CtrlProfileManager_ProfileSelected;

            GenerateProfileStack();
            settings_control.DataContext = this;

            
        }

        internal void Display()
        {
            if (App.isSilent || Global.Configuration.start_silently)
            {
                this.Visibility = Visibility.Hidden;
                this.WindowStyle = WindowStyle.None;
                this.ShowInTaskbar = false;
                Hide();
            }
            else
            {
                this.Show();
            }
        }

        private void CtrlProfileManager_ProfileSelected(ApplicationProfile profile)
        {
            profilecontrol_presenter.Profile = profile;

            if (_selectedManager.Equals(this.ctrlProfileManager))
                this.content_grid.Content = profilecontrol_presenter;   
        }

        private void CtrlLayerManager_ProfileOverviewRequest(UserControl profile_control)
        {
            if (this.content_grid.Content != profile_control)
                this.content_grid.Content = profile_control;
        }

        private void Layer_manager_NewLayer(Layer layer)
        {
            layercontrol_presenter.Layer = layer;

            if (_selectedManager.Equals(this.ctrlLayerManager))
                this.content_grid.Content = layercontrol_presenter;
        }

        private void KbLayout_KeyboardLayoutUpdated(object sender)
        {
            virtial_kb = Global.kbLayout.Virtual_keyboard;

            keyboard_grid.Children.Clear();
            keyboard_grid.Children.Add(virtial_kb);
            keyboard_grid.Children.Add(new LayerEditor());

            keyboard_grid.Width = virtial_kb.Width;
            this.Width = width + (virtial_kb.Width - virtual_keyboard_width);

            keyboard_grid.Height = virtial_kb.Height;
            this.Height = height + (virtial_kb.Height - virtual_keyboard_height);

            keyboard_grid.UpdateLayout();

            keyboard_viewbox.MaxWidth = virtial_kb.Width + 50;
            keyboard_viewbox.MaxHeight = virtial_kb.Height + 50;
            keyboard_viewbox.UpdateLayout();

            this.UpdateLayout();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

            if (!settingsloaded)
            {
                virtual_keyboard_timer = new Timer(100);
                virtual_keyboard_timer.Elapsed += new ElapsedEventHandler(virtual_keyboard_timer_Tick);
                virtual_keyboard_timer.Start();

                settingsloaded = true;
            }

            this.keyboard_record_message.Visibility = Visibility.Hidden;

            current_color = desktop_color_scheme;
            bg_grid.Background = new SolidColorBrush(Color.FromRgb(desktop_color_scheme.Red, desktop_color_scheme.Green, desktop_color_scheme.Blue));

            virtial_kb = Global.kbLayout.Virtual_keyboard;

            keyboard_grid.Children.Clear();
            keyboard_grid.Children.Add(virtial_kb);
            keyboard_grid.Children.Add(new LayerEditor());

            keyboard_grid.Width = virtial_kb.Width;
            this.Width = width + (virtial_kb.Width - virtual_keyboard_width);

            keyboard_grid.Height = virtial_kb.Height;
            this.Height = height + (virtial_kb.Height - virtual_keyboard_height);

            keyboard_grid.UpdateLayout();

            keyboard_viewbox.MaxWidth = virtial_kb.Width + 50;
            keyboard_viewbox.MaxHeight = virtial_kb.Height + 50;
            keyboard_viewbox.UpdateLayout();

            UpdateManagerStackFocus(ctrlLayerManager);

            this.UpdateLayout();

            foreach (Image child in this.profiles_stack.Children)
            {
                if (child.Visibility == Visibility.Visible)
                {
                    this.ProfileImage_MouseDown(child, null);
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

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        private void virtual_keyboard_timer_Tick(object sender, EventArgs e)
        {
            if (!ApplicationIsActivated())
                return;

            Dispatcher.Invoke(
                        () =>
                        {
                            if (transitionamount <= 1.0f)
                            {
                                transition_color.BlendColors(current_color, transitionamount += 0.07f);

                                bg_grid.Background = new SolidColorBrush(Color.FromRgb(transition_color.Red, transition_color.Green, transition_color.Blue));
                                bg_grid.UpdateLayout();
                            }


                            Dictionary<Devices.DeviceKeys, System.Drawing.Color> keylights = new Dictionary<Devices.DeviceKeys, System.Drawing.Color>();

                            if (IsActive)
                            {
                                keylights = Global.effengine.GetKeyboardLights();
                                Global.kbLayout.SetKeyboardColors(keylights);
                            }

                            if (Global.key_recorder.IsRecording())
                                this.keyboard_record_message.Visibility = Visibility.Visible;
                            else
                                this.keyboard_record_message.Visibility = Visibility.Hidden;

                        });
        }

        ////Misc
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void trayicon_menu_quit_Click(object sender, RoutedEventArgs e)
        {
            exitApp();
        }

        private void trayicon_menu_settings_Click(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.Show();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Global.Configuration.close_mode == AppExitMode.Ask)
            {
                MessageBoxResult result = MessageBox.Show("Would you like to Exit Aurora?", "Aurora", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    minimizeApp();
                    e.Cancel = true;
                }
                else
                {
                    exitApp();
                }
            }
            else if (Global.Configuration.close_mode == AppExitMode.Minimize)
            {
                minimizeApp();
                e.Cancel = true;
            }
            else
            {
                exitApp();
            }
        }

        private void exitApp()
        {
            trayicon.Visibility = Visibility.Hidden;
            virtual_keyboard_timer?.Stop();
            System.Windows.Application.Current.Shutdown();
        }

        private void minimizeApp()
        {
            this.FocusedApplication?.SaveAll();

            if (!shownHiddenMessage)
            {
                trayicon.ShowBalloonTip("Aurora", "This program is now hidden in the tray.", BalloonIcon.None);
                shownHiddenMessage = true;
            }

            Global.LightingStateManager.PreviewProfileKey = string.Empty;

            //Hide Window
            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (System.Windows.Threading.DispatcherOperationCallback)delegate (object o)
            {
                WindowStyle = WindowStyle.None;
                Hide();
                return null;
            }, null);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Global.LightingStateManager.PreviewProfileKey = saved_preview_key;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            saved_preview_key = Global.LightingStateManager.PreviewProfileKey;
            Global.LightingStateManager.PreviewProfileKey = string.Empty;
        }

        private Image profile_add;

        private Image profile_hidden;

        private BitmapImage _visible = new BitmapImage(new Uri(@"Resources/Visible.png", UriKind.Relative));
        private BitmapImage _not_visible = new BitmapImage(new Uri(@"Resources/Not Visible.png", UriKind.Relative));

        private void GenerateProfileStack(string focusedKey = null)
        {
            selected_item = null;
            this.profiles_stack.Children.Clear();

            /*Image profile_desktop = new Image
            {
                Tag = Global.Configuration.desktop_profile,
                Source = new BitmapImage(new Uri(@"Resources/desktop_icon.png", UriKind.Relative)),
                ToolTip = "Desktop Settings",
                Margin = new Thickness(0, 5, 0, 0)
            };
            profile_desktop.MouseDown += ProfileImage_MouseDown;
            this.profiles_stack.Children.Add(profile_desktop);*/

            //Included Game Profiles
            foreach (string profile_k in Global.Configuration.ProfileOrder)
            {
                if (!Global.LightingStateManager.Events.ContainsKey(profile_k))
                    continue;

                Profiles.Application application = (Profiles.Application)Global.LightingStateManager.Events[profile_k];
                ImageSource icon = application.Icon;
                UserControl control = application.Control;
                if (icon != null && control != null)
                {
                    Image profile_image;
                    if (application is GenericApplication)
                    {
                        GenericApplicationSettings settings = (application.Settings as GenericApplicationSettings);
                        profile_image = new Image
                        {
                            Tag = application,
                            Source = icon,
                            ToolTip = settings.ApplicationName + " Settings",
                            Margin = new Thickness(0, 5, 0, 0)
                        };
                        profile_image.MouseDown += ProfileImage_MouseDown;

                        Image profile_remove = new Image
                        {
                            Source = new BitmapImage(new Uri(@"Resources/removeprofile_icon.png", UriKind.Relative)),
                            ToolTip = $"Remove {settings.ApplicationName} Profile",
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Height = 16,
                            Width = 16,
                            Visibility = Visibility.Hidden,
                            Tag = profile_k
                        };
                        profile_remove.MouseDown += RemoveProfile_MouseDown;

                        Grid profile_grid = new Grid
                        {
                            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                            Margin = new Thickness(0, 5, 0, 0),
                            Tag = profile_remove
                        };

                        profile_grid.MouseEnter += Profile_grid_MouseEnter;
                        profile_grid.MouseLeave += Profile_grid_MouseLeave;

                        profile_grid.Children.Add(profile_image);
                        profile_grid.Children.Add(profile_remove);

                        this.profiles_stack.Children.Add(profile_grid);
                    }
                    else
                    {
                        profile_image = new Image
                        {
                            Tag = application,
                            Source = icon,
                            ToolTip = application.Config.Name + " Settings",
                            Margin = new Thickness(0, 5, 0, 0),
                            Visibility = application.Settings.Hidden ? Visibility.Collapsed : Visibility.Visible
                        };
                        profile_image.MouseDown += ProfileImage_MouseDown;
                        this.profiles_stack.Children.Add(profile_image);
                    }

                    if (application.Config.ID.Equals(focusedKey))
                    {
                        this.FocusedApplication = application;
                        this.TransitionToProfile(profile_image);
                    }
                }
            }

            //Add new profiles button
            profile_add = new Image
            {
                Source = new BitmapImage(new Uri(@"Resources/addprofile_icon.png", UriKind.Relative)),
                ToolTip = "Add a new Lighting Profile",
                Margin = new Thickness(0, 5, 0, 0)
            };
            profile_add.MouseDown += AddProfile_MouseDown;
            this.profiles_stack.Children.Add(profile_add);

            //Show hidden profiles button
            profile_hidden = new Image
            {
                Source = _not_visible,
                ToolTip = "Toggle Hidden profiles' visibility",
                Margin = new Thickness(0, 5, 0, 0)
            };
            profile_hidden.MouseDown += HiddenProfile_MouseDown;
            this.profiles_stack.Children.Add(profile_hidden);
        }

        private void HiddenProfile_MouseDown(object sender, EventArgs e)
        {
            this.ShowHidden = !this.ShowHidden;
        }

        protected void ShowHiddenChanged(bool value)
        {
            profile_hidden.Source = value ? _visible : _not_visible;

            foreach (FrameworkElement ctrl in profiles_stack.Children)
            {
                Image img = ctrl as Image ?? (ctrl is Grid ? ((Grid)ctrl).Children[0] as Image : null);
                if (img != null)
                {
                    Profiles.Application profile = img.Tag as Profiles.Application;
                    if (profile != null)
                    {
                        img.Visibility = profile.Settings.Hidden && !value ? Visibility.Collapsed : Visibility.Visible;
                        img.Opacity = profile.Settings.Hidden ? 0.5 : 1;
                    }
                }
            }

            //profile_add.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
        }

        private void mbtnHidden_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem btn = sender as MenuItem;
            Image img = this.cmenuProfiles.PlacementTarget as Image;

            if (img != null)
            {
                img.Opacity = btn.IsChecked ? 0.5 : 1;

                if (!this.ShowHidden && btn.IsChecked)
                    img.Visibility = Visibility.Collapsed;

                (img.Tag as Profiles.Application)?.SaveProfiles();
            }
        }

        private void cmenuProfiles_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!(((ContextMenu)e.Source).PlacementTarget is Image))
                e.Handled = true;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu context = (ContextMenu)e.OriginalSource;

            if (!(context.PlacementTarget is Image))
                return;

            Image img = (Image)context.PlacementTarget;
            Profiles.Application profile = img.Tag as Profiles.Application;
            context.DataContext = profile;
            /*
            this.mbtnEnabled.IsChecked = profile.Settings.isEnabled;
            this.mbtnHidden.IsChecked = profile.Settings.Hidden;*/

        }

        private void ProfileImage_Edit_MouseUp(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            Profiles.Application profile = img.Tag as Profiles.Application;
            profile.Settings.Hidden = !profile.Settings.Hidden;
            img.Opacity = profile.Settings.Hidden ? 0.5 : 1;
        }

        private void Profile_grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if ((sender as Grid)?.Tag is Image)
                ((sender as Grid).Tag as Image).Visibility = Visibility.Hidden;
        }

        private void Profile_grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if ((sender as Grid)?.Tag is Image)
                ((sender as Grid).Tag as Image).Visibility = Visibility.Visible;
        }

        private void TransitionToProfile(Image source)
        {
            this.FocusedApplication = source.Tag as Profiles.Application;
            var bitmap = (BitmapSource)source.Source;
            var color = Utils.ColorUtils.GetAverageColor(bitmap);

            current_color = new EffectColor(color);
            current_color *= 0.85f;

            transitionamount = 0.0f;

            UpdateProfileStackBackground(source);
        }

        private void ProfileImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            if (image != null && image.Tag != null && image.Tag is Profiles.Application)
            {
                if (e == null || e.LeftButton == MouseButtonState.Pressed)
                    this.TransitionToProfile(image);
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    this.cmenuProfiles.PlacementTarget = (Image)sender;
                    this.cmenuProfiles.IsOpen = true;
                }
            }
        }

        private static void FocusedProfileChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ConfigUI th = source as ConfigUI;
            Profiles.Application value = e.NewValue as Profiles.Application;

            //th.ctrlLayerManager.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;
            //th.ctrlProfileManager.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;
            th.gridManagers.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;

            if (value == null)
                return;

            /*th.content_grid.Children.Clear();
            UIElement element = value.GetUserControl();
            //th.content_grid.DataContext = element;
            th.content_grid.MinHeight = ((UserControl)element).MinHeight;
            th.content_grid.Children.Add(element);
            th.content_grid.UpdateLayout();*/
            th.content_grid.Content = value.Control;
            th.content_grid.UpdateLayout();

        }

        private void RemoveProfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && sender is Image && (sender as Image).Tag != null && (sender as Image).Tag is string)
            {
                string name = (sender as Image).Tag as string;

                if (Global.LightingStateManager.Events.ContainsKey(name))
                {
                    if (MessageBox.Show("Are you sure you want to delete profile for " + (((Profiles.Application)Global.LightingStateManager.Events[name]).Settings as GenericApplicationSettings).ApplicationName + "?", "Remove Profile", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        var eventList = Global.Configuration.ProfileOrder;
                        string prevProfile = eventList[eventList.FindIndex(s => s.Equals(name)) - 1];
                        Global.LightingStateManager.RemoveGenericProfile(name);
                        //ConfigManager.Save(Global.Configuration);
                        this.GenerateProfileStack(prevProfile);
                    }
                }
            }
        }

        private void AddProfile_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Window_ProcessSelection dialog = new Window_ProcessSelection { CheckCustomPathExists = true, ButtonLabel = "Add Profile", Title ="Add Profile" };
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ChosenExecutablePath)) { // do not need to check if dialog is already in excluded_programs since it is a Set and only contains unique items by definition

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

                GenericApplication gen_app_pm = new GenericApplication(filename);
                gen_app_pm.Initialize();
                ((GenericApplicationSettings)gen_app_pm.Settings).ApplicationName = Path.GetFileNameWithoutExtension(filename);

                System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(dialog.ChosenExecutablePath.ToLowerInvariant());

                if (!Directory.Exists(gen_app_pm.GetProfileFolderPath()))
                    Directory.CreateDirectory(gen_app_pm.GetProfileFolderPath());

                using (var icon_asbitmap = ico.ToBitmap())
                {
                    icon_asbitmap.Save(Path.Combine(gen_app_pm.GetProfileFolderPath(), "icon.png"), System.Drawing.Imaging.ImageFormat.Png);
                }
                ico.Dispose();

                Global.LightingStateManager.RegisterEvent(gen_app_pm);
                ConfigManager.Save(Global.Configuration);
                GenerateProfileStack(filename);
            }
        }

        private void DesktopControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.FocusedApplication = null;
            this.content_grid.Content = settings_control;

            current_color = desktop_color_scheme;
            transitionamount = 0.0f;

            UpdateProfileStackBackground(sender as FrameworkElement);
        }

        private void UpdateProfileStackBackground(FrameworkElement item)
        {
            selected_item = item;

            if (selected_item != null)
            {
                DrawingBrush mask = new DrawingBrush();
                GeometryDrawing visible_region =
                    new GeometryDrawing(
                        new SolidColorBrush(Color.FromArgb(64, 0, 0, 0)),
                        null,
                        new RectangleGeometry(new Rect(0, 0, profiles_background.ActualWidth, profiles_background.ActualHeight)));

                DrawingGroup drawingGroup = new DrawingGroup();
                drawingGroup.Children.Add(visible_region);

                Point relativePoint = selected_item.TransformToAncestor(profiles_background)
                              .Transform(new Point(0, 0));

                double x = 0.0D;
                double y = relativePoint.Y - 2.0D;
                double width = profiles_background.ActualWidth;
                double height = selected_item.ActualHeight + 4.0D;

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
                    height = selected_item.ActualHeight + 4.0D;

                    if (y + height > profiles_background.ActualHeight - 40)
                        height -= (y + height) - (profiles_background.ActualHeight - 40);
                }
                
                if (height > 0 && width > 0)
                {
                    GeometryDrawing transparent_region =
                        new GeometryDrawing(
                            new SolidColorBrush((Color)current_color),
                            null,
                            new RectangleGeometry(new Rect(x, y, width, height)));

                    drawingGroup.Children.Add(transparent_region);
                }

                mask.Drawing = drawingGroup;

                profiles_background.Background = mask;
            }
        }

        public void ShowWindow()
        {
            Global.logger.Info("Show Window called");
            this.Visibility = Visibility.Visible;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.ShowInTaskbar = true;
            //this.Topmost = true;
            this.Show();
            this.Activate();
        }

        private void trayicon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.ShowWindow();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateProfileStackBackground(selected_item);
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateProfileStackBackground(selected_item);
        }

        private void UpdateManagerStackFocus(object focusedElement, bool forced = false)
        {
            if(focusedElement != null && focusedElement is FrameworkElement && (!focusedElement.Equals(_selectedManager) || forced))
            {
                _selectedManager = focusedElement as FrameworkElement;
                if(gridManagers.ActualHeight != 0)
                    stackPanelManagers.Height = gridManagers.ActualHeight;
                double totalHeight = stackPanelManagers.Height;

                foreach (FrameworkElement child in stackPanelManagers.Children)
                {
                    if(child.Equals(focusedElement))
                        child.Height = totalHeight - (28.0 * (stackPanelManagers.Children.Count - 1));
                    else
                        child.Height = 25.0;
                }
                _selectedManager.RaiseEvent(new RoutedEventArgs(GotFocusEvent));
            }
        }

        private void ctrlLayerManager_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!sender.Equals(_selectedManager))
                this.content_grid.Content = this.FocusedApplication.Profile.Layers.Count > 0 ? layercontrol_presenter : this.FocusedApplication.Control;
            UpdateManagerStackFocus(sender);
        }

        private void ctrlProfileManager_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!sender.Equals(_selectedManager))
                this.content_grid.Content = profilecontrol_presenter;
            UpdateManagerStackFocus(sender);
        }

        private void brdOverview_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.content_grid.Content = this._selectedManager = this.FocusedApplication.Control;

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateManagerStackFocus(_selectedManager, true);
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member