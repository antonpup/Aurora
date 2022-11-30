using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Aurora.Devices;

namespace Aurora.Settings.Layers.Controls
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
            if (KeyCloneSourceButtonComboBox.SelectedValue == null || KeyCloneDestinationButtonComboBox.SelectedValue == null)
                return;

            var sourceKey = (DeviceKeys)KeyCloneSourceButtonComboBox.SelectedValue;
            var destKey = (DeviceKeys)KeyCloneDestinationButtonComboBox.SelectedValue;

            if (sourceKey == destKey)
                return;

            var cloneMap = Context.Properties.KeyCloneMap;
            if (cloneMap.ContainsKey(destKey))
                return;

            cloneMap.Add(destKey, sourceKey);

            KeyCloneDestinationButtonComboBox.SelectedValue = null;
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
    }
}
