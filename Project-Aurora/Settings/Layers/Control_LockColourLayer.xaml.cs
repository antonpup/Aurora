#pragma warning disable CS0103


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
    /// Interaction logic for Control_LockColourLayer.xaml
    /// </summary>
    public partial class Control_LockColourLayer : UserControl
    {
        public Control_LockColourLayer()
        {
            InitializeComponent();
            cmbKey.Items.Add(System.Windows.Forms.Keys.CapsLock);
            cmbKey.Items.Add(System.Windows.Forms.Keys.NumLock);
            cmbKey.Items.Add(System.Windows.Forms.Keys.Scroll);
            this.Loaded += (obj, e) => { this.SetSettings(); };
        }

        public Control_LockColourLayer(LockColourLayerHandler datacontext) : this()
        {
            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            this.keysLock.Sequence = ((LockColourLayerHandler)this.DataContext).Properties._Sequence;
        }

        private void keysLock_SequenceUpdated(object sender, EventArgs e)
        {
            ((LockColourLayerHandler)this.DataContext).Properties._Sequence = ((Controls.KeySequence)sender).Sequence;
        }
    }
}
