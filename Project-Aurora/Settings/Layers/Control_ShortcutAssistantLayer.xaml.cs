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
        public Control_ShortcutAssistantLayer()
        {
            InitializeComponent();
            this.Loaded += (obj, e) => { this.SetSettings(); };
        }

        public Control_ShortcutAssistantLayer(ShortcutAssistantLayerHandler datacontext) : this()
        {
            this.DataContext = datacontext;
            cmbModifier.ItemsSource = datacontext.Properties.DefaultKeys.Keys;
        }

        public void SetSettings()
        {
            this.keysMain.Sequence = ((ShortcutAssistantLayerHandler)this.DataContext).Properties._Sequence;
        }

        private void keysMain_SequenceUpdated(object sender, EventArgs e)
        {
            ((ShortcutAssistantLayerHandler)this.DataContext).Properties._Sequence = ((Controls.KeySequence)sender).Sequence;
        }

        private void cmbModifier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0) {
                ((ShortcutAssistantLayerHandler)this.DataContext).Properties.SetDefaultKeys();
                this.SetSettings();
            }
        }
    }
}
