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
using System.Windows.Shapes;

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for Control_AnimationLayer.xaml
    /// </summary>
    public partial class Control_AnimationLayer : UserControl
    {
        private Window windowAnimationEditor = null;
        private static bool windowAnimationEditorOpen;

        private bool settingsset = false;

        public Control_AnimationLayer()
        {
            InitializeComponent();
        }

        public Control_AnimationLayer(AnimationLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if(this.DataContext is AnimationLayerHandler && !settingsset)
            {
                this.chkboxForceKeySequence.IsChecked = (this.DataContext as AnimationLayerHandler).Properties._forceKeySequence;
                this.chkboxScaleToKeySequence.IsChecked = (this.DataContext as AnimationLayerHandler).Properties._scaleToKeySequenceBounds;
                this.KeySequence_keys.Sequence = (this.DataContext as AnimationLayerHandler).Properties._Sequence;
                this.updownAnimationDuration.Value = (double)(this.DataContext as AnimationLayerHandler).Properties._AnimationDuration;
                this.updownAnimationRepeat.Value = (this.DataContext as AnimationLayerHandler).Properties._AnimationRepeat;

                settingsset = true;
            }
        }

        private void btnEditAnimation_Click(object sender, RoutedEventArgs e)
        {
            if (windowAnimationEditor == null)
            {
                if (windowAnimationEditorOpen == true)
                {
                    MessageBox.Show("Animation Editor already open for another layer.\r\nPlease close it.");
                    return;
                }

                windowAnimationEditor = new Window();
                windowAnimationEditor.Closed += WindowAnimationEditor_Closed;

                windowAnimationEditor.Title = "Animation Editor";

                Controls.Control_AnimationEditor animEditor = new Controls.Control_AnimationEditor() { AnimationMix = (this.DataContext as AnimationLayerHandler).Properties._AnimationMix };
                animEditor.AnimationMixUpdated += AnimEditor_AnimationMixUpdated;

                windowAnimationEditor.Content = animEditor;
                windowAnimationEditor.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                windowAnimationEditor.Show();
            }
            else
            {
                windowAnimationEditor.BringIntoView();
            }

            windowAnimationEditorOpen = true;
        }

        private void WindowAnimationEditor_Closed(object sender, EventArgs e)
        {
            windowAnimationEditor = null;
            windowAnimationEditorOpen = false;
        }

        private void AnimEditor_AnimationMixUpdated(object sender, EffectsEngine.Animations.AnimationMix mix)
        {
            if (IsLoaded && settingsset && this.DataContext is AnimationLayerHandler && sender is Aurora.Controls.Control_AnimationEditor)
                (this.DataContext as AnimationLayerHandler).Properties._AnimationMix = mix;
        }

        private void chkboxForceKeySequence_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is AnimationLayerHandler && sender is CheckBox)
                (this.DataContext as AnimationLayerHandler).Properties._forceKeySequence = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
        }

        private void chkboxScaleToKeySequence_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is AnimationLayerHandler && sender is CheckBox)
                (this.DataContext as AnimationLayerHandler).Properties._scaleToKeySequenceBounds = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is AnimationLayerHandler && sender is Aurora.Controls.KeySequence)
                (this.DataContext as AnimationLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void updownAnimationDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && this.DataContext is AnimationLayerHandler && sender is Xceed.Wpf.Toolkit.DoubleUpDown)
                (this.DataContext as AnimationLayerHandler).Properties._AnimationDuration = (float)((sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value.HasValue ? (sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value.Value : 0.0f);
        }

        private void updownAnimationRepeat_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && this.DataContext is AnimationLayerHandler && sender is Xceed.Wpf.Toolkit.IntegerUpDown)
                (this.DataContext as AnimationLayerHandler).Properties._AnimationRepeat = ((sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.HasValue ? (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.Value : 0);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }
    }
}
