using Aurora.EffectsEngine.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Control_AnimationFrameItem.xaml
    /// </summary>
    public partial class Control_AnimationFrameItem : UserControl
    {
        public delegate void DragAdjust(object sender, double delta);

        public event DragAdjust LeftSplitterDrag;

        public event DragAdjust RightSplitterDrag;

        public event DragAdjust ContentSplitterDrag;

        public event DragAdjust CompletedDrag;

        public delegate void AnimationFrameItemArgs(object sender, AnimationFrame track);

        public event AnimationFrameItemArgs AnimationFrameItemUpdated;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ContextFrameProperty = DependencyProperty.Register("ContextFrame", typeof(AnimationFrame), typeof(Control_AnimationFrameItem));

        public AnimationFrame ContextFrame
        {
            get
            {
                return (AnimationFrame)GetValue(ContextFrameProperty);
            }
            set
            {
                SetValue(ContextFrameProperty, value);

                if(value != null)
                {
                    rectDisplay.Fill = new LinearGradientBrush(Utils.ColorUtils.DrawingColorToMediaColor(value.Color), Color.FromArgb(0, 0, 0, 0), new Point(0.5, 0), new Point(0.5, 1));
                    grdSplitterLeft.Background = new SolidColorBrush(Utils.ColorUtils.DrawingColorToMediaColor(value.Color));
                    grdSplitterRight.Background = new SolidColorBrush(Utils.ColorUtils.DrawingColorToMediaColor(value.Color));
                }

                AnimationFrameItemUpdated?.Invoke(this, value);
            }
        }

        public Control_AnimationFrameItem()
        {
            InitializeComponent();
        }

        private void grdSplitterLeft_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            LeftSplitterDrag?.Invoke(this, e.HorizontalChange);
        }

        private void grdSplitterRight_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            RightSplitterDrag?.Invoke(this, e.HorizontalChange);
        }

        private void grdSplitterContent_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ContentSplitterDrag?.Invoke(this, e.HorizontalChange);
        }

        private void grdSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            CompletedDrag?.Invoke(this, 0.0);
        }
    }
}
