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

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for Control_ShortcutAssistantLayer.xaml
    /// </summary>
    public partial class Control_ShortcutAssistantLayer : UserControl
    {
        private bool settingsset = false;

        public Control_ShortcutAssistantLayer()
        {
            InitializeComponent();
        }

        public Control_ShortcutAssistantLayer(ShortcutAssistantLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is ShortcutAssistantLayerHandler && !settingsset)
            {
                this.sc_assistant_dim_background.IsChecked = (this.DataContext as ShortcutAssistantLayerHandler).Properties._DimBackground;
                this.sc_assistant_ctrl_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as ShortcutAssistantLayerHandler).Properties._CtrlKeyColor ?? System.Drawing.Color.Empty);
                this.sc_assistant_ctrl_keys.Sequence = (this.DataContext as ShortcutAssistantLayerHandler).Properties._CtrlKeySequence;
                this.sc_assistant_win_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as ShortcutAssistantLayerHandler).Properties._WindowsKeyColor ?? System.Drawing.Color.Empty);
                this.sc_assistant_win_keys.Sequence = (this.DataContext as ShortcutAssistantLayerHandler).Properties._WindowsKeySequence;
                this.sc_assistant_alt_color.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as ShortcutAssistantLayerHandler).Properties._AltKeyColor ?? System.Drawing.Color.Empty);
                this.sc_assistant_alt_keys.Sequence = (this.DataContext as ShortcutAssistantLayerHandler).Properties._AltKeySequence;

                settingsset = true;
            }
        }

        private void sc_assistant_dim_background_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is ShortcutAssistantLayerHandler)
            {
                (this.DataContext as ShortcutAssistantLayerHandler).Properties._DimBackground = (this.sc_assistant_dim_background.IsChecked.HasValue) ? this.sc_assistant_dim_background.IsChecked.Value : false;
            }
        }

        private void sc_assistant_ctrl_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is ShortcutAssistantLayerHandler && this.sc_assistant_ctrl_color.SelectedColor.HasValue)
            {
                (this.DataContext as ShortcutAssistantLayerHandler).Properties._CtrlKeyColor = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_ctrl_color.SelectedColor.Value);
            }
        }

        private void sc_assistant_ctrl_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is ShortcutAssistantLayerHandler)
            {
                (this.DataContext as ShortcutAssistantLayerHandler).Properties._CtrlKeySequence = (sender as Controls.KeySequence).Sequence;
            }
        }

        private void sc_assistant_win_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is ShortcutAssistantLayerHandler && this.sc_assistant_win_color.SelectedColor.HasValue)
            {
                (this.DataContext as ShortcutAssistantLayerHandler).Properties._WindowsKeyColor = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_win_color.SelectedColor.Value);
            }
        }

        private void sc_assistant_win_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is ShortcutAssistantLayerHandler)
            {
                (this.DataContext as ShortcutAssistantLayerHandler).Properties._WindowsKeySequence = (sender as Controls.KeySequence).Sequence;
            }
        }

        private void sc_assistant_alt_color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is ShortcutAssistantLayerHandler && this.sc_assistant_alt_color.SelectedColor.HasValue)
            {
                (this.DataContext as ShortcutAssistantLayerHandler).Properties._AltKeyColor = Utils.ColorUtils.MediaColorToDrawingColor(this.sc_assistant_alt_color.SelectedColor.Value);
            }
        }

        private void sc_assistant_alt_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is ShortcutAssistantLayerHandler)
            {
                (this.DataContext as ShortcutAssistantLayerHandler).Properties._AltKeySequence = (sender as Controls.KeySequence).Sequence;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }
    }
}
