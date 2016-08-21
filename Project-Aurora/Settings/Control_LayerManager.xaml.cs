using Aurora.Settings.Layers;
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

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_LayerManager.xaml
    /// </summary>
    public partial class Control_LayerManager : UserControl
    {
        public delegate void NewLayerHandler(DefaultLayer layer);

        public event NewLayerHandler NewLayer = delegate { };

        public Control_LayerManager()
        {
            InitializeComponent();

            layers_listbox.SelectionMode = SelectionMode.Single;
            layers_listbox.SelectionChanged += Layers_listbox_SelectionChanged;
        }

        private void Layers_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (layers_listbox.SelectedItem != null && layers_listbox.SelectedItem is CheckBox)
            {
                var hander = NewLayer;

                if (hander != null && (layers_listbox.SelectedItem as CheckBox).Tag != null)
                    hander.Invoke((layers_listbox.SelectedItem as CheckBox).Tag as DefaultLayer);
            }

            layers_listbox.Items.Refresh();
        }

        private void add_layer_button_Click(object sender, RoutedEventArgs e)
        {
            CheckBox new_layer = new CheckBox();
            new_layer.Tag = new DefaultLayer();
            new_layer.Content = "New layer " + Utils.Time.GetMilliSeconds();
            new_layer.ClickMode = ClickMode.Release;

            layers_listbox.Items.Add(new_layer);
        }
    }
}
