using Aurora.Devices;
using RazerSdkWrapper.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using System.Windows.Shapes;

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
                BrightnessBoostSlider.Value = Context.Properties.BrightnessBoost;
                SaturationBoostSlider.Value = Context.Properties.SaturationBoost;
                HueShiftSlider.Value = Context.Properties.HueShift;
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
                SdkInstalledVersionValueLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
            else
                SdkInstalledVersionValueLabel.Foreground = new SolidColorBrush(Colors.LightGreen);

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

        private void OnDownloadSdkButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start("http://cdn.razersynapse.com/156092369797u1UA8NRazerChromaBroadcasterSetup_v3.4.0630.061913.exe");
        }
    }
}
