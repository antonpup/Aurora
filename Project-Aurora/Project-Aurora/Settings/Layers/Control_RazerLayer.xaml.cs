using Aurora.Devices;
using Microsoft.Win32;
using RazerSdkWrapper.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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
                BrightnessSlider.Value = Context.Properties.BrightnessChange;
                SaturationSlider.Value = Context.Properties.SaturationChange;
                HueSlider.Value = Context.Properties.HueShift;
                CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
                settingsset = true;
            }
        }

        private void OnUserControlLoaded(object sender, RoutedEventArgs e)
        {
            SetSettings();
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
            if (cloneMap.ContainsKey(destKey))
                return;

            cloneMap.Add(destKey, sourceKey);

            KeyCloneDestinationButtonComboBox.SelectedItem = null;
            CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
        }

        private void OnDeleteKeyCloneButtonClick(object sender, RoutedEventArgs e)
        {
            if (KeyCloneListBox.SelectedItems == null)
                return;

            var cloneMap = Context.Properties.KeyCloneMap;
            foreach (var o in KeyCloneListBox.SelectedItems)
            {
                if (o is KeyValuePair<DeviceKeys, DeviceKeys> item)
                {
                    if (!cloneMap.ContainsKey(item.Key) || cloneMap[item.Key] != item.Value)
                        continue;

                    cloneMap.Remove(item.Key);
                }
            }

            CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
        }

        private void OnSdkDumpToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            if (Context.StartDumpingData())
            {
                SdkDumpToggleButton.Content = "Dumping...";
                SdkDumpToggleButton.IsEnabled = false;

                Task.Delay(5000).ContinueWith((t) =>
                    Application.Current.Dispatcher.Invoke(() => SdkDumpToggleButton.IsChecked = false)
                );
            }
        }

        private void OnSdkDumpToggleButtonUnchecked(object sender, RoutedEventArgs e)
        {
            SdkDumpToggleButton.Content = "Start";
            SdkDumpToggleButton.IsEnabled = true;
            Context.StopDumpingData();
        }
    }
}
