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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Aurora
{
    partial class ConfigUI : Window
    {
        Settings.Control_Settings settings_control = new Settings.Control_Settings();
        Profiles.Desktop.Control_Desktop desktop_control = new Profiles.Desktop.Control_Desktop();

        EffectColor desktop_color_scheme = new EffectColor(0, 0, 0);

        EffectColor transition_color = new EffectColor();
        EffectColor current_color = new EffectColor();

        private float transitionamount = 0.0f;

        private FrameworkElement selected_item = null;

        private bool settingsloaded = false;
        private bool shownHiddenMessage = false;

        private PreviewType saved_preview = PreviewType.Desktop;
        private string saved_preview_key = "";

        private Timer virtual_keyboard_timer;
        private Stopwatch recording_stopwatch = new Stopwatch();
        private Grid virtial_kb = new Grid();

        private readonly double virtual_keyboard_width;
        private readonly double virtual_keyboard_height;

        private readonly double max_width;
        private readonly double max_height;

        LayerEditor layer_editor = new LayerEditor();

        public ConfigUI()
        {
            InitializeComponent();

            virtual_keyboard_height = this.keyboard_grid.Height;
            virtual_keyboard_width = this.keyboard_grid.Width;

            max_width = MaxWidth;
            max_height = MaxHeight;

            Global.kbLayout.KeyboardLayoutUpdated += KbLayout_KeyboardLayoutUpdated;

            GenerateProfileStack();
        }

        private void KbLayout_KeyboardLayoutUpdated(object sender)
        {
            virtial_kb = Global.kbLayout.Virtual_keyboard;

            keyboard_grid.Children.Clear();
            keyboard_grid.Children.Add(virtial_kb);
            keyboard_grid.Children.Add(new LayerEditor());

            keyboard_grid.Width = virtial_kb.Width;
            this.MaxWidth = max_width + (virtial_kb.Width - virtual_keyboard_width);
            this.Width = this.MaxWidth;

            keyboard_grid.Height = virtial_kb.Height;
            this.MaxHeight = max_height + (virtial_kb.Height - virtual_keyboard_height);
            this.Height = this.MaxHeight;

            keyboard_grid.UpdateLayout();
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
            this.content_grid.Children.Clear();
            this.content_grid.Children.Add(desktop_control);
            current_color = desktop_color_scheme;
            bg_grid.Background = new SolidColorBrush(Color.FromRgb(desktop_color_scheme.Red, desktop_color_scheme.Green, desktop_color_scheme.Blue));

            virtial_kb = Global.kbLayout.Virtual_keyboard;

            keyboard_grid.Children.Clear();
            keyboard_grid.Children.Add(virtial_kb);
            keyboard_grid.Children.Add(new LayerEditor());

            keyboard_grid.Width = virtial_kb.Width;
            this.MaxWidth = max_width + (virtial_kb.Width - virtual_keyboard_width);
            this.Width = this.MaxWidth;

            keyboard_grid.Height = virtial_kb.Height;
            this.MaxHeight = max_height + (virtial_kb.Height - virtual_keyboard_height);
            this.Height = this.MaxHeight;

            keyboard_grid.UpdateLayout();
            this.UpdateLayout();

            Global.input_subscriptions.Initialize();
        }

        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

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

                            if (Global.geh.GetPreview() != PreviewType.None)
                                keylights = Global.effengine.GetKeyboardLights();

                            Border[] keys = virtial_kb.Children.OfType<Border>().ToArray();


                            foreach (var child in keys)
                            {
                                if (child is Border &&
                                    (child as Border).Child is TextBlock &&
                                    ((child as Border).Child as TextBlock).Tag is Devices.DeviceKeys
                                    )
                                {
                                    if (keylights.ContainsKey((Devices.DeviceKeys)((child as Border).Child as TextBlock).Tag))
                                    {
                                        ((child as Border).Child as TextBlock).Foreground = new SolidColorBrush(Utils.ColorUtils.DrawingColorToMediaColor(keylights[(Devices.DeviceKeys)((child as Border).Child as TextBlock).Tag]));
                                    }

                                    if (Global.key_recorder.HasRecorded((Devices.DeviceKeys)((child as Border).Child as TextBlock).Tag))
                                        (child as Border).Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)0, (byte)(Math.Min(Math.Pow(Math.Cos((double)(Utils.Time.GetMilliSeconds() / 1000.0) * Math.PI) + 0.05, 2.0), 1.0) * 255), (byte)0));
                                    else
                                    {
                                        if ((child as Border).IsEnabled)
                                        {
                                            (child as Border).Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)30, (byte)30, (byte)30));
                                        }
                                        else
                                        {
                                            (child as Border).Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 100, 100));
                                            (child as Border).BorderThickness = new Thickness(0);
                                        }
                                    }
                                }

                            }//);

                            if (Global.key_recorder.IsRecording())
                                this.keyboard_record_message.Visibility = System.Windows.Visibility.Visible;
                            else
                                this.keyboard_record_message.Visibility = System.Windows.Visibility.Hidden;

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
            if (Program.isSilent || Global.Configuration.start_silently)
            {
                this.Visibility = System.Windows.Visibility.Hidden;
                this.WindowStyle = WindowStyle.None;
                this.ShowInTaskbar = false;
                Hide();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Global.Configuration.close_mode == AppExitMode.Ask)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Would you like to Exit Aurora?", "Aurora", MessageBoxButton.YesNo);

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
            trayicon.Visibility = System.Windows.Visibility.Hidden;
            virtual_keyboard_timer.Stop();
            Global.input_subscriptions.Dispose();
            Global.geh.Destroy();
            Global.net_listener.Stop();

            try
            {
                foreach (Process proc in Process.GetProcessesByName("Aurora-SkypeIntegration"))
                {
                    proc.Kill();
                }
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Exception closing \"Aurora-SkypeIntegration\", Exception: " + exc);
            }


            Application.Current.Shutdown();
        }

        private void minimizeApp()
        {
            if (!shownHiddenMessage)
            {
                trayicon.ShowBalloonTip("Aurora", "This program is now hidden in the tray.", BalloonIcon.None);
                shownHiddenMessage = true;
            }

            Global.geh.SetPreview(PreviewType.None);

            //Hide Window
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (System.Windows.Threading.DispatcherOperationCallback)delegate (object o)
            {
                WindowStyle = WindowStyle.None;
                Hide();
                return null;
            }, null);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Global.geh.SetPreview(saved_preview, saved_preview_key);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            saved_preview = Global.geh.GetPreview();
            saved_preview_key = Global.geh.GetPreviewProfileKey();
            Global.geh.SetPreview(PreviewType.None);
        }

        private void GenerateProfileStack()
        {
            selected_item = null;
            this.profiles_stack.Children.Clear();

            Image profile_desktop = new Image();
            profile_desktop.Tag = desktop_control;
            profile_desktop.Source = new BitmapImage(new Uri(@"Resources/desktop_icon.png", UriKind.Relative));
            profile_desktop.ToolTip = "Desktop Settings";
            profile_desktop.Margin = new Thickness(0, 5, 0, 0);
            profile_desktop.MouseDown += ProfileImage_MouseDown;
            this.profiles_stack.Children.Add(profile_desktop);

            //Included Game Profiles
            foreach (KeyValuePair<string, ProfileManager> kvp in Global.Configuration.ApplicationProfiles)
            {
                ProfileManager profile = kvp.Value;
                ImageSource icon = profile.GetIcon();
                UserControl control = profile.GetUserControl();

                if (icon != null && control != null)
                {
                    Image profile_image = new Image();
                    profile_image.Tag = control;
                    profile_image.Source = icon;
                    profile_image.ToolTip = profile.Name + " Settings";
                    profile_image.Margin = new Thickness(0, 5, 0, 0);
                    profile_image.MouseDown += ProfileImage_MouseDown;
                    this.profiles_stack.Children.Add(profile_image);
                }
            }

            //Populate with added profiles
            foreach (var kvp in Global.Configuration.additional_profiles)
            {
                ProfileManager profile = kvp.Value;
                ImageSource icon = profile.GetIcon();
                UserControl control = profile.GetUserControl();

                if (icon != null && control != null)
                {
                    Image profile_image = new Image();
                    profile_image.Tag = control;
                    profile_image.Source = icon;
                    profile_image.ToolTip = (profile.Settings as GenericApplicationSettings).ApplicationName + " Settings";
                    profile_image.Margin = new Thickness(0, 5, 0, 0);
                    profile_image.MouseDown += ProfileImage_MouseDown;

                    Image profile_remove = new Image();
                    profile_remove.Source = new BitmapImage(new Uri(@"Resources/removeprofile_icon.png", UriKind.Relative));
                    profile_remove.ToolTip = "Remove " + (profile.Settings as GenericApplicationSettings).ApplicationName + " Profile";
                    profile_remove.HorizontalAlignment = HorizontalAlignment.Right;
                    profile_remove.VerticalAlignment = VerticalAlignment.Bottom;
                    profile_remove.Height = 16;
                    profile_remove.Width = 16;
                    profile_remove.Visibility = Visibility.Hidden;
                    profile_remove.MouseDown += RemoveProfile_MouseDown;
                    profile_remove.Tag = kvp.Key;

                    Grid profile_grid = new Grid();
                    profile_grid.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                    profile_grid.Margin = new Thickness(0, 5, 0, 0);
                    profile_grid.Tag = profile_remove;
                    profile_grid.MouseEnter += Profile_grid_MouseEnter;
                    profile_grid.MouseLeave += Profile_grid_MouseLeave;
                    profile_grid.Children.Add(profile_image);
                    profile_grid.Children.Add(profile_remove);

                    this.profiles_stack.Children.Add(profile_grid);
                }
            }

            //Add new profiles button
            Image profile_add = new Image();
            profile_add.Source = new BitmapImage(new Uri(@"Resources/addprofile_icon.png", UriKind.Relative));
            profile_add.ToolTip = "Add a new Lighting Profile";
            profile_add.Margin = new Thickness(0, 5, 0, 0);
            profile_add.MouseDown += AddProfile_MouseDown;
            this.profiles_stack.Children.Add(profile_add);
        }

        private void Profile_grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender != null && sender is Grid && (sender as Grid).Tag != null && (sender as Grid).Tag is Image)
            {
                ((sender as Grid).Tag as Image).Visibility = Visibility.Hidden;
            }
        }

        private void Profile_grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender != null && sender is Grid && (sender as Grid).Tag != null && (sender as Grid).Tag is Image)
            {
                ((sender as Grid).Tag as Image).Visibility = Visibility.Visible;
            }
        }

        private void ProfileImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && sender is Image && (sender as Image).Tag != null && (sender as Image).Tag is UserControl)
            {
                UserControl tagged_control = (sender as Image).Tag as UserControl;

                this.content_grid.Children.Clear();
                this.content_grid.Children.Add(tagged_control);

                var bitmap = (BitmapSource)(sender as Image).Source;
                var color = GetAverageColor(bitmap);

                current_color = new EffectColor(color);
                current_color *= 0.85f;

                transitionamount = 0.0f;

                UpdateProfileStackBackground(sender as FrameworkElement);
            }
        }

        private void RemoveProfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && sender is Image && (sender as Image).Tag != null && (sender as Image).Tag is string)
            {
                string name = (sender as Image).Tag as string;

                if (Global.Configuration.additional_profiles.ContainsKey(name))
                {
                    if (System.Windows.MessageBox.Show("Are you sure you want to delete profile for " + (Global.Configuration.additional_profiles[name].Settings as GenericApplicationSettings).ApplicationName + "?", "Remove Profile", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        string path = Global.Configuration.additional_profiles[name].GetProfileFolderPath();
                        if (Directory.Exists(path))
                            Directory.Delete(path, true);

                        Global.Configuration.additional_profiles.Remove(name);
                        ConfigManager.Save(Global.Configuration);
                        GenerateProfileStack();
                    }
                }
            }
        }

        private void AddProfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog exe_filedlg = new Microsoft.Win32.OpenFileDialog();

            exe_filedlg.DefaultExt = ".exe";
            exe_filedlg.Filter = "Executable Files (*.exe)|*.exe;";

            Nullable<bool> result = exe_filedlg.ShowDialog();

            if (result.HasValue && result == true)
            {
                string filename = System.IO.Path.GetFileName(exe_filedlg.FileName.ToLowerInvariant());

                if (Global.Configuration.additional_profiles.ContainsKey(filename))
                {
                    System.Windows.MessageBox.Show("Profile for this application already exists.");
                }
                else
                {
                    GenericApplicationProfileManager gen_app_pm = new GenericApplicationProfileManager(filename);

                    System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(exe_filedlg.FileName.ToLowerInvariant());

                    if (!System.IO.Directory.Exists(gen_app_pm.GetProfileFolderPath()))
                        System.IO.Directory.CreateDirectory(gen_app_pm.GetProfileFolderPath());

                    using (var icon_asbitmap = ico.ToBitmap())
                    {
                        icon_asbitmap.Save(Path.Combine(gen_app_pm.GetProfileFolderPath(), "icon.png"), System.Drawing.Imaging.ImageFormat.Png);
                    }
                    ico.Dispose();

                    Global.Configuration.additional_profiles.Add(filename, gen_app_pm);
                    ConfigManager.Save(Global.Configuration);
                    GenerateProfileStack();
                }

                this.content_grid.Children.Clear();
                this.content_grid.Children.Add(new Profiles.Generic_Application.Control_GenericApplication(filename));

                current_color = desktop_color_scheme;
                transitionamount = 0.0f;
            }
        }

        private void DesktopControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.content_grid.Children.Clear();
            this.content_grid.Children.Add(settings_control);

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

        public Color GetAverageColor(BitmapSource bitmap)
        {
            var format = bitmap.Format;

            if (format != PixelFormats.Bgr24 &&
                format != PixelFormats.Bgr32 &&
                format != PixelFormats.Bgra32 &&
                format != PixelFormats.Pbgra32)
            {
                throw new InvalidOperationException("BitmapSource must have Bgr24, Bgr32, Bgra32 or Pbgra32 format");
            }

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var numPixels = width * height;
            var bytesPerPixel = format.BitsPerPixel / 8;
            var pixelBuffer = new byte[numPixels * bytesPerPixel];

            bitmap.CopyPixels(pixelBuffer, width * bytesPerPixel, 0);

            long blue = 0;
            long green = 0;
            long red = 0;

            for (int i = 0; i < pixelBuffer.Length; i += bytesPerPixel)
            {
                blue += pixelBuffer[i];
                green += pixelBuffer[i + 1];
                red += pixelBuffer[i + 2];
            }

            return Color.FromRgb((byte)(red / numPixels), (byte)(green / numPixels), (byte)(blue / numPixels));
        }

        private void trayicon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.Show();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateProfileStackBackground(selected_item);
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member