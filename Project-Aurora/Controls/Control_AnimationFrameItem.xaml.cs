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
    }
}
