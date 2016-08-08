using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Aurora.Controls
{
    public partial class ColorZones : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ColorZonesListProperty = DependencyProperty.Register("ColorZonesList", typeof(List<ColorZone>), typeof(ColorZones));

        public List<ColorZone> ColorZonesList
        {
            get
            {
                return (List<ColorZone>)GetValue(ColorZonesListProperty);
            }
            set
            {
                SetValue(ColorZonesListProperty, value);

                if (allowListRefresh)
                {
                    cz_list.Items.Clear();
                    foreach (var key in value)
                        cz_list.Items.Add(key);

                    if(cz_list.Items.Count > 0)
                        cz_list.SelectedIndex = 0;
                }

                ColorZonesListUpdated?.Invoke(this, new EventArgs());

                verifyColorZones();
            }
        }
        private bool allowListRefresh = true;

        public event EventHandler ColorZonesListUpdated;

        public ColorZones()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void cz_list_add_Click(object sender, RoutedEventArgs e)
        {
            ColorZone newcz = new ColorZone();

            allowListRefresh = false;
            cz_list.Items.Add(newcz);
            allowListRefresh = true;

            cz_list.SelectedItem = newcz;
            cz_list.ScrollIntoView(newcz);

            update_cz_list();
        }

        private void cz_list_remove_Click(object sender, RoutedEventArgs e)
        {
            if (cz_list.SelectedIndex != -1)
            {
                int selected_index = cz_list.SelectedIndex;

                allowListRefresh = false;
                cz_list.Items.RemoveAt(selected_index);
                allowListRefresh = true;

                if (cz_list.Items.Count > selected_index)
                    cz_list.SelectedIndex = selected_index;
                else
                    cz_list.SelectedIndex = (cz_list.Items.Count - 1);

                if (cz_list.SelectedIndex > -1)
                    cz_list.ScrollIntoView(cz_list.Items[cz_list.SelectedIndex]);
                else
                {
                    ks.Sequence = new Settings.KeySequence();
                }
            }
            update_cz_list();
        }

        private void cz_name_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsInitialized)
            {
                if (this.cz_list.SelectedItem != null)
                {
                    ((ColorZone)this.cz_list.SelectedItem).name = cz_name_textbox.Text;
                    update_cz_list();
                }
            }
        }

        private void cz_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cz_list.SelectedItem != null)
            {
                ColorZone cz = (ColorZone)cz_list.SelectedItem;

                this.cz_name_textbox.Text = cz.name;
                this.cz_colorpicker.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(cz.color);
                this.cz_effect.SelectedItem = cz.effect;
                ks.Sequence = cz.keysequence;

                update_cz_list();
            }
        }

        private void cz_colorpicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (this.cz_colorpicker.SelectedColor.HasValue)
            {
                if (cz_list.SelectedItem != null)
                {
                    ((ColorZone)cz_list.SelectedItem).color = Utils.ColorUtils.MediaColorToDrawingColor(this.cz_colorpicker.SelectedColor.Value);
                    ConfigManager.Save(Global.Configuration);
                }
            }
        }

        private void update_cz_list()
        {
            cz_list.Items.Refresh();

            List<ColorZone> new_list = new List<ColorZone>();

            foreach (var item in cz_list.Items)
                new_list.Add((ColorZone)item);

            allowListRefresh = false;
            ColorZonesList = new_list;
            allowListRefresh = true;
        }

        private void ks_ListUpdated(object sender, EventArgs e)
        {
            if (cz_list.SelectedItem != null)
            {
                ((ColorZone)cz_list.SelectedItem).keysequence.keys = ks.List;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cz_effect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cz_list.SelectedItem != null)
            {
                LayerEffects selectedEffecttype = (LayerEffects)Enum.Parse(typeof(LayerEffects), this.cz_effect.SelectedIndex.ToString());

                ((ColorZone)cz_list.SelectedItem).effect = selectedEffecttype;
                ConfigManager.Save(Global.Configuration);

                effect_settings_button.IsEnabled = selectedEffecttype != LayerEffects.None;
            }
        }

        private void effect_settings_button_Click(object sender, RoutedEventArgs e)
        {
            if (cz_list.SelectedItem != null)
            {
                EffectSettingsWindow effect_settings = new EffectSettingsWindow(((ColorZone)cz_list.SelectedItem).effect_config);
                effect_settings.preview = Global.geh.GetPreview();
                effect_settings.preview_key = Global.geh.GetPreviewProfileKey();
                effect_settings.EffectConfigUpdated += Effect_settings_EffectConfigUpdated;

                effect_settings.ShowDialog();
            }
        }

        private void Effect_settings_EffectConfigUpdated(object sender, EventArgs e)
        {
            if (cz_list.SelectedItem != null)
            {
                ((ColorZone)cz_list.SelectedItem).effect_config = (sender as EffectSettingsWindow).EffectConfig;
                ConfigManager.Save(Global.Configuration);
            }
        }

        private void cz_list_up_Click(object sender, RoutedEventArgs e)
        {
            if (cz_list.SelectedIndex > 0)
            {
                int selected_index = cz_list.SelectedIndex;
                var saved = cz_list.Items[selected_index];

                allowListRefresh = false;
                cz_list.Items[selected_index] = cz_list.Items[selected_index - 1];
                cz_list.Items[selected_index - 1] = saved;
                allowListRefresh = true;

                cz_list.SelectedIndex = selected_index - 1;

                cz_list.ScrollIntoView(cz_list.Items[selected_index - 1]);
            }
            update_cz_list();
        }

        private void cz_list_down_Click(object sender, RoutedEventArgs e)
        {
            if (cz_list.SelectedIndex != -1 && cz_list.SelectedIndex < cz_list.Items.Count - 1)
            {
                int selected_index = cz_list.SelectedIndex;
                var saved = cz_list.Items[selected_index];

                allowListRefresh = false;
                cz_list.Items[selected_index] = cz_list.Items[selected_index + 1];
                cz_list.Items[selected_index + 1] = saved;
                allowListRefresh = true;

                cz_list.SelectedIndex = selected_index + 1;

                cz_list.ScrollIntoView(cz_list.Items[selected_index + 1]);
            }
            update_cz_list();
        }

        private void ks_SequenceUpdated(object sender, EventArgs e)
        {
            if (cz_list.SelectedItem != null)
            {
                ((ColorZone)cz_list.SelectedItem).keysequence = ks.Sequence;
                //ConfigManager.Save(Global.Configuration);
            }
        }

        private void verifyColorZones()
        {
            bool isEnabled = !(cz_list.Items.Count == 0);

            this.cz_name_textbox.IsEnabled = isEnabled;
            this.cz_colorpicker.IsEnabled = isEnabled;
            this.cz_effect.IsEnabled = isEnabled;
            this.effect_settings_button.IsEnabled = isEnabled;
            this.ks.IsEnabled = isEnabled;
            this.cz_list_remove.IsEnabled = isEnabled;
            this.cz_list_down.IsEnabled = (cz_list.Items.Count > 1);
            this.cz_list_up.IsEnabled = (cz_list.Items.Count > 1);
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member