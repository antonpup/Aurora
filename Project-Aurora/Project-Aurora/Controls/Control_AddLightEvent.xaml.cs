using Aurora.Profiles;
using Aurora.Profiles.Generic_Application;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using Aurora.Settings;

namespace Aurora.Controls
{
    public class EventSelectedEventArgs : EventArgs
    {
        public string SelectedKey { get; set; }

        public EventSelectedEventArgs(string selectedKey = null)
        {
            this.SelectedKey = selectedKey;
        }
    }

    /// <summary>
    /// Interaction logic for Control_AddLightEvent.xaml
    /// </summary>
    public partial class Control_AddLightEvent : UserControl
    {
        public event EventHandler<EventSelectedEventArgs> EventSelected;

        public DependencyProperty FocusedLayerProperty = DependencyProperty.Register("FocusedLayer", typeof(LightEventType?), typeof(Control_AddLightEvent), new PropertyMetadata(null, new PropertyChangedCallback(FocusedLayerChanged)));

        public LightEventType? FocusedLayer
        {
            get { return (LightEventType?)GetValue(FocusedLayerProperty); }
            set { SetValue(FocusedLayerProperty, value); }
        }

        public Control_AddLightEvent()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private static void FocusedLayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Control_AddLightEvent self = (Control_AddLightEvent)d;
            self.SetItems();
        }

        private void SetItems()
        {
            this.lstLightEvents.ItemsSource = Global.ProfilesManager.AvailableEvents.Where(s => {
                return s.Value.Config.Type == null || s.Value.Config.Type == FocusedLayer;
            });
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EventSelected?.Invoke(this, new EventSelectedEventArgs());
        }

        private void imgAdd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EventSelected?.Invoke(this, new EventSelectedEventArgs(((KeyValuePair<string, ILightEvent>)((FrameworkElement)sender).DataContext).Key));
        }

        private void imgRemove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            KeyValuePair<string, ILightEvent> levent = (KeyValuePair<string, ILightEvent>)((FrameworkElement)sender).DataContext;
            if (MessageBox.Show($"Are you sure you want to delete the Profile for \"{levent.Value.Config.Name}\"?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            Global.ProfilesManager.DeleteEvent(levent.Key);
            this.SetItems();
        }

        private void CreateNewEvent(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog exe_filedlg = new Microsoft.Win32.OpenFileDialog();

            exe_filedlg.DefaultExt = ".exe";
            exe_filedlg.Filter = "Executable Files (*.exe)|*.exe;";

            bool? result = exe_filedlg.ShowDialog();

            if (result.HasValue && result == true)
            {
                string filename = System.IO.Path.GetFileName(exe_filedlg.FileName.ToLowerInvariant());

                if (Global.ProfilesManager.GetEvent(filename) != null)
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

                    Global.ProfilesManager.RegisterEvent(gen_app_pm);
                    ConfigManager.Save(Global.Configuration);
                    this.SetItems();
                }
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.SetItems();
        }
    }
}
