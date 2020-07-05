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
    /// Interaction logic for Control_Dota2AbilityLayer.xaml
    /// </summary>
    public partial class Control_Dota2AbilityLayer : UserControl
    {
        private bool settingsset = false;

        public Control_Dota2AbilityLayer()
        {
            InitializeComponent();
        }

        public Control_Dota2AbilityLayer(Dota2AbilityLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Dota2AbilityLayerHandler && !settingsset)
            {
                this.ColorPicker_CanCastAbility.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2AbilityLayerHandler).Properties._CanCastAbilityColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_CanNotCastAbility.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Dota2AbilityLayerHandler).Properties._CanNotCastAbilityColor ?? System.Drawing.Color.Empty);
                UIUtils.SetSingleKey(this.ability_key1_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 0);
                UIUtils.SetSingleKey(this.ability_key2_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 1);
                UIUtils.SetSingleKey(this.ability_key3_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 2);
                UIUtils.SetSingleKey(this.ability_key4_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 3);
                UIUtils.SetSingleKey(this.ability_key5_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 4);
                UIUtils.SetSingleKey(this.ability_key6_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 5);

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

        private void abilities_canuse_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2AbilityLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2AbilityLayerHandler).Properties._CanCastAbilityColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void abilities_cannotuse_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Dota2AbilityLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Dota2AbilityLayerHandler).Properties._CanNotCastAbilityColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ability_key1_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 1 Key", sender as TextBlock, ability1_keys_callback);
        }

        private void ability1_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability1_keys_callback;

            Dispatcher.Invoke(() =>
                {
                    ability_key1_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                    if (resulting_keys.Length > 0)
                    {
                        if (IsLoaded)
                            (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys[0] = resulting_keys[0];

                        UIUtils.SetSingleKey(this.ability_key1_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 0);
                    }
                });

            Global.key_recorder.Reset();
        }

        private void ability_key2_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 2 Key", sender as TextBlock, ability2_keys_callback);
        }

        private void ability2_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability2_keys_callback;

            Dispatcher.Invoke(() =>
                {
                    ability_key2_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                    if (resulting_keys.Length > 0)
                    {
                        if (IsLoaded)
                            (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys[1] = resulting_keys[0];

                        UIUtils.SetSingleKey(this.ability_key2_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 1);
                    }
                });

            Global.key_recorder.Reset();
        }

        private void ability_key3_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 3 Key", sender as TextBlock, ability3_keys_callback);
        }

        private void ability3_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability3_keys_callback;

            Dispatcher.Invoke(() =>
                {
                    ability_key3_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                    if (resulting_keys.Length > 0)
                    {
                        if (IsLoaded)
                            (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys[2] = resulting_keys[0];

                        UIUtils.SetSingleKey(this.ability_key3_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 2);
                    }
                });

            Global.key_recorder.Reset();
        }

        private void ability_key4_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 4 Key", sender as TextBlock, ability4_keys_callback);
        }

        private void ability4_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability4_keys_callback;

            Dispatcher.Invoke(() =>
                {
                    ability_key4_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                    if (resulting_keys.Length > 0)
                    {
                        if (IsLoaded)
                            (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys[3] = resulting_keys[0];

                        UIUtils.SetSingleKey(this.ability_key4_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 3);
                    }
                });

            Global.key_recorder.Reset();
        }

        private void ability_key5_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ability 5 Key", sender as TextBlock, ability5_keys_callback);
        }

        private void ability5_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability5_keys_callback;

            Dispatcher.Invoke(() =>
                {
                    ability_key5_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                    if (resulting_keys.Length > 0)
                    {
                        if (IsLoaded)
                            (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys[4] = resulting_keys[0];

                        UIUtils.SetSingleKey(this.ability_key5_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 4);
                    }
                });

            Global.key_recorder.Reset();
        }

        private void ability_key6_textblock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSingleKey("Dota 2 - Ultimate Ability Key", sender as TextBlock, ability6_keys_callback);
        }

        private void ability6_keys_callback(DeviceKeys[] resulting_keys)
        {
            Global.key_recorder.FinishedRecording -= ability6_keys_callback;

            Dispatcher.Invoke(() =>
                {
                    ability_key6_textblock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                    if (resulting_keys.Length > 0)
                    {
                        if (IsLoaded)
                            (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys[5] = resulting_keys[0];

                        UIUtils.SetSingleKey(this.ability_key6_textblock, (this.DataContext as Dota2AbilityLayerHandler).Properties._AbilityKeys, 5);
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
    }
}
