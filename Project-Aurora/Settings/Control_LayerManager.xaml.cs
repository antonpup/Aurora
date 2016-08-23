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

        public event NewLayerHandler NewLayer;

        public static readonly DependencyProperty FocusedProfileProperty = DependencyProperty.Register("FocusedProfile", typeof(ProfileManager), typeof(Control_LayerManager), new PropertyMetadata(null, new PropertyChangedCallback(FocusedProfileChanged)));

        public ProfileManager FocusedProfile
        {
            get { return (ProfileManager)GetValue(FocusedProfileProperty); }
            set
            {
                SetValue(FocusedProfileProperty, value);
            }
        }

        public Control_LayerManager()
        {
            InitializeComponent();

            lstLayers.SelectionMode = SelectionMode.Single;
            lstLayers.SelectionChanged += Layers_listbox_SelectionChanged;
        }

        public static void FocusedProfileChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Control_LayerManager self = source as Control_LayerManager;

            if (e.OldValue != null)
                ((ProfileManager)e.OldValue).ProfileChanged -= self.UpdateLayers;

            self.UpdateLayers();
            ((ProfileManager)e.NewValue).ProfileChanged += self.UpdateLayers;
        }

        public void UpdateLayers()
        {
            this.UpdateLayers(null, null);
        }

        public void UpdateLayers(object sender, EventArgs e)
        {
            this.lstLayers.ItemsSource = this.FocusedProfile.Settings.Layers;
        }

        private void Layers_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var hander = NewLayer;

                if (lstLayers.SelectedItem != null)
                    hander?.Invoke(lstLayers.SelectedItem as DefaultLayer);
            }
        }

        private void add_layer_button_Click(object sender, RoutedEventArgs e)
        {
            DefaultLayer lyr = new DefaultLayer("New layer " + Utils.Time.GetMilliSeconds());
            lyr.AnythingChanged += this.FocusedProfile.SaveProfilesEvent;
            /*CheckBox new_layer = new CheckBox();
            new_layer.Tag = new DefaultLayer();
            new_layer.Content = "New layer " + Utils.Time.GetMilliSeconds();
            new_layer.ClickMode = ClickMode.Release;*/


            this.FocusedProfile?.Settings?.Layers.Add(lyr);
            //this.lstLayers.
        }

        private void btnRemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            this.FocusedProfile?.Settings?.Layers.RemoveAt(this.lstLayers.SelectedIndex);
        }
    }
}
