using Aurora.Devices;
using Aurora.Settings;
using Aurora.Utils;
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
using System.Windows.Shapes;

namespace Aurora.Profiles.Dota_2.Layers
{
    /// <summary>
    /// Interaction logic for Control_Dota2ItemLayer.xaml
    /// </summary>
    public partial class Control_Dota2ItemLayer : UserControl
    {
        private bool settingsset = false;
        private bool profileset = false;

        public Control_Dota2ItemLayer()
        {
            InitializeComponent();
        }

        public Control_Dota2ItemLayer(Dota2ItemLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Dota2ItemLayerHandler && !settingsset)
            {
                this.ColorPicker_Item_Empty.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2ItemLayerHandler).Properties._EmptyItemColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Item_Cooldown.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2ItemLayerHandler).Properties._ItemCooldownColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Item_NoCharges.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2ItemLayerHandler).Properties._ItemNoChargersColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Item_Color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2ItemLayerHandler).Properties._ItemsColor ?? System.Drawing.Color.Empty);
                this.CheckBox_Use_Item_Colors.IsChecked = (this.DataContext as Dota2ItemLayerHandler).Properties._UseItemColors;

                UIUtils.SetSingleKey(this.item_slot1_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 0);
                UIUtils.SetSingleKey(this.item_slot2_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 1);
                UIUtils.SetSingleKey(this.item_slot3_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 2);
                UIUtils.SetSingleKey(this.item_slot4_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 3);
                UIUtils.SetSingleKey(this.item_slot5_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 4);
                UIUtils.SetSingleKey(this.item_slot6_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 5);
                UIUtils.SetSingleKey(this.stash_slot1_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 6);
                UIUtils.SetSingleKey(this.stash_slot2_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 7);
                UIUtils.SetSingleKey(this.stash_slot3_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 8);
                UIUtils.SetSingleKey(this.stash_slot4_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 9);
                UIUtils.SetSingleKey(this.stash_slot5_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 10);
                UIUtils.SetSingleKey(this.stash_slot6_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 11);

                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void item_slot1_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 1 Key", sender as TextBlock, item1_keys_callback);
        }

        private void item1_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item1_keys_callback;

            Dispatcher.Invoke(() =>
                {
                    item_slot1_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                    if (resulting_keys.Length > 0)
                    {
                        if (IsLoaded)
                            (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[0] = resulting_keys[0];

                        UIUtils.SetSingleKey(this.item_slot1_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 0);
                    }
                });

            Global.key_recorder.Reset();
        }

        private void item_slot2_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 2 Key", sender as TextBlock, item2_keys_callback);
        }

        private void item2_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item2_keys_callback;

            Dispatcher.Invoke(() =>
            {
                item_slot2_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[1] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.item_slot2_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 1);
                }
            });

            Global.key_recorder.Reset();
        }

        private void item_slot3_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 3 Key", sender as TextBlock, item3_keys_callback);
        }

        private void item3_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item3_keys_callback;

            Dispatcher.Invoke(() =>
            {
                item_slot3_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[2] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.item_slot3_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 2);
                }
            });

            Global.key_recorder.Reset();
        }

        private void item_slot4_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 4 Key", sender as TextBlock, item4_keys_callback);
        }

        private void item4_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item4_keys_callback;

            Dispatcher.Invoke(() =>
            {
                item_slot4_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[3] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.item_slot4_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 3);
                }
            });

            Global.key_recorder.Reset();
        }

        private void item_slot5_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 5 Key", sender as TextBlock, item5_keys_callback);
        }

        private void item5_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item5_keys_callback;

            Dispatcher.Invoke(() =>
            {
                item_slot5_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[4] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.item_slot5_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 4);
                }
            });

            Global.key_recorder.Reset();
        }

        private void item_slot6_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Item Slot 6 Key", sender as TextBlock, item6_keys_callback);
        }

        private void item6_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= item6_keys_callback;

            Dispatcher.Invoke(() =>
            {
                item_slot6_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[5] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.item_slot6_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 5);
                }
            });

            Global.key_recorder.Reset();
        }

        private void stash_slot1_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 1 Key", sender as TextBlock, stash1_keys_callback);
        }

        private void stash1_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash1_keys_callback;

            Dispatcher.Invoke(() =>
            {
                stash_slot1_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[6] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.stash_slot1_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 6);
                }
            });

            Global.key_recorder.Reset();
        }

        private void stash_slot2_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 2 Key", sender as TextBlock, stash2_keys_callback);
        }

        private void stash2_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash2_keys_callback;

            Dispatcher.Invoke(() =>
            {
                stash_slot2_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[7] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.stash_slot2_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 7);
                }
            });

            Global.key_recorder.Reset();
        }

        private void stash_slot3_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 3 Key", sender as TextBlock, stash3_keys_callback);
        }

        private void stash3_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash3_keys_callback;

            Dispatcher.Invoke(() =>
            {
                stash_slot3_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[8] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.stash_slot3_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 8);
                }
            });

            Global.key_recorder.Reset();
        }

        private void stash_slot4_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 4 Key", sender as TextBlock, stash4_keys_callback);
        }

        private void stash4_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash4_keys_callback;

            Dispatcher.Invoke(() =>
            {
                stash_slot4_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[9] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.stash_slot4_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 9);
                }
            });

            Global.key_recorder.Reset();
        }

        private void stash_slot5_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 5 Key", sender as TextBlock, stash5_keys_callback);
        }

        private void stash5_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash5_keys_callback;

            Dispatcher.Invoke(() =>
            {
                stash_slot5_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[10] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.stash_slot5_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 10);
                }
            });

            Global.key_recorder.Reset();
        }

        private void stash_slot6_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Stash Slot 6 Key", sender as TextBlock, stash6_keys_callback);
        }

        private void stash6_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= stash6_keys_callback;

            Dispatcher.Invoke(() =>
            {
                stash_slot6_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (resulting_keys.Length > 0)
                {
                    if (IsLoaded)
                        (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys[11] = resulting_keys[0];

                    UIUtils.SetSingleKey(this.stash_slot6_textblock, (this.DataContext as Dota2ItemLayerHandler).Properties._ItemKeys, 11);
                }
            });

            Global.key_recorder.Reset();
        }

        private void RecordSingleKey(string whoisrecording, TextBlock textblock, KeyRecorder.RecordingFinishedHandler callback)
        {
            if (Global.key_recorder.IsRecording())
            {

                if (Global.key_recorder.GetRecordingType().Equals(whoisrecording))
                {
                    Global.key_recorder.StopRecording();

                    Global.key_recorder.Reset();
                }
                else
                {
                    MessageBox.Show("You are already recording a key sequence for " + Global.key_recorder.GetRecordingType());
                }
            }
            else
            {
                Global.key_recorder.FinishedRecording += callback;
                Global.key_recorder.StartRecording(whoisrecording, true);
                textblock.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            }
        }

        private void ColorPicker_Item_Empty_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2ItemLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2ItemLayerHandler).Properties._EmptyItemColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Item_Cooldown_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2ItemLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2ItemLayerHandler).Properties._ItemCooldownColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Item_NoCharges_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2ItemLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2ItemLayerHandler).Properties._ItemNoChargersColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Item_Color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2ItemLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2ItemLayerHandler).Properties._ItemsColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void CheckBox_Use_Item_Colors_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2ItemLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as Dota2ItemLayerHandler).Properties._UseItemColors = (sender as CheckBox).IsChecked.Value;
        }
    }
}

