using System;
using System.Collections.Generic;
using System.IO;
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

namespace Aurora.Settings.Keycaps
{
    /// <summary>
    /// Interaction logic for Control_DeviceConfig.xaml
    /// </summary>
    public partial class Window_DeviceConfig : Window
    {
        public DeviceConfig Config;
        public Window_DeviceConfig(DeviceConfig config)
        {
            InitializeComponent();
            Config = config;
            LoadDeviceType(config.Type);
            this.device_layout.SelectedItem = Config.SelectedLayout;
            if (config.SelectedKeyboardLayout != null) 
                this.keyboard_layout.SelectedItem = Config.SelectedKeyboardLayout;
        }
        private List<string> GetBrandsName(string dicName)
        {
            string layoutsPath = Path.Combine(Global.ExecutingDirectory, "DeviceLayouts", dicName);
            List<string> FilesName = new List<string>() { "None" };
            foreach (var name in Directory.GetFiles(layoutsPath))
            {
                FilesName.Add(name.Split('\\').Last().Split('.').First());
            }
            return FilesName;
        }
        private void LoadDeviceType(int type)
        {
            switch (type)
            {
                case 0:
                    this.device_layout.ItemsSource = GetBrandsName("Keyboard");
                    this.keyboard_layout.ItemsSource = GetBrandsName("Keyboard\\Plain Keyboard");
                    this.keyboard_layout.Visibility = Visibility.Visible;
                    this.keyboard_layout_tb.Visibility = Visibility.Visible;
                    break;
                default:
                    this.device_layout.ItemsSource = GetBrandsName("Mouse");
                    this.keyboard_layout.Visibility = Visibility.Collapsed;
                    this.keyboard_layout_tb.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void device_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Config.Type = this.device_layout.SelectedIndex;
            }
        }
        private void device_layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Config.SelectedLayout = this.device_layout.SelectedItem.ToString();
            }
        }
        private void keyboard_layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                Config.SelectedKeyboardLayout = this.keyboard_layout.SelectedItem.ToString();
            }
        }
        private void device_disable_lighting_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && sender is CheckBox)
            {
                Config.LightingEnabled = ((sender as CheckBox).IsChecked.HasValue) ? !(sender as CheckBox).IsChecked.Value : true;
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

    }
}
