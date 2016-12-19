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
        public delegate void NewLayerHandler(Layer layer);

        public event NewLayerHandler NewLayer;

        public delegate void ProfileOverviewHandler(UserControl profile_control);

        public event ProfileOverviewHandler ProfileOverviewRequest;

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
            {
                ProfileManager prof = ((ProfileManager)e.OldValue);
                prof.ProfileChanged -= self.UpdateLayers;
                prof.SaveProfiles();
            }
            self.UpdateLayers();
            if (e.NewValue != null)
                ((ProfileManager)e.NewValue).ProfileChanged += self.UpdateLayers;
        }

        public void UpdateLayers()
        {
            this.UpdateLayers(null, null);
        }

        public void UpdateLayers(object sender, EventArgs e)
        {
            this.lstLayers.ItemsSource = this.FocusedProfile?.Settings?.Layers;
        }

        private void Layers_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var hander = NewLayer;
                if (lstLayers.SelectedItem != null)
                {
                    if (!(lstLayers.SelectedItem is Layer))
                        throw new ArgumentException($"Items contained in the ListView must be of type 'Layer', not '{lstLayers.SelectedItem.GetType()}'");

                    (lstLayers.SelectedItem as Layer).SetProfile(FocusedProfile);

                    hander?.Invoke(lstLayers.SelectedItem as Layer);

                }
            }
            this.FocusedProfile?.SaveProfiles();
        }

        private void add_layer_button_Click(object sender, RoutedEventArgs e)
        {
            Layer lyr = new Layer("New layer " + Utils.Time.GetMilliSeconds());
            lyr.AnythingChanged += this.FocusedProfile.SaveProfilesEvent;
            /*CheckBox new_layer = new CheckBox();
            new_layer.Tag = new DefaultLayer();
            new_layer.Content = "New layer " + Utils.Time.GetMilliSeconds();
            new_layer.ClickMode = ClickMode.Release;*/

            lyr.SetProfile(FocusedProfile);
            this.FocusedProfile?.Settings?.Layers.Add(lyr);
            this.lstLayers.SelectedItem = lyr;
            //this.lstLayers.
        }

        private void btnRemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            if (this.lstLayers.SelectedIndex > -1)
                this.FocusedProfile?.Settings?.Layers.RemoveAt(this.lstLayers.SelectedIndex);
        }

        Point? DragStartPosition;
        FrameworkElement DraggingItem;

        private void stckLayer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (DragStartPosition == null || !this.lstLayers.IsMouseOver)
                return;

            Point curr = e.GetPosition(null);
            Point start = (Point)DragStartPosition;

            if (Math.Abs(curr.X - start.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(curr.Y - start.Y) >= SystemParameters.MinimumVerticalDragDistance)
            {
                DragDrop.DoDragDrop(DraggingItem, DraggingItem.DataContext, DragDropEffects.Move);

            }
        }

        private void stckLayer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement stckLayer;
            if ((stckLayer = sender as FrameworkElement) != null)
            {
                //this.lstLayers.SelectedValue = stckLayer.DataContext;
                DragStartPosition = e.GetPosition(null);
                DraggingItem = stckLayer;
                //stckLayer.IsSelected = true;
            }
        }

        private void lstLayers_PreviewMouseUp(object sender, EventArgs e)
        {
            DraggingItem = null;
            DragStartPosition = null;
        }

        //Based on: http://stackoverflow.com/questions/3350187/wpf-c-rearrange-items-in-listbox-via-drag-and-drop
        private void stckLayer_Drop(object sender, DragEventArgs e)
        {
            Layer droppedData = e.Data.GetData(typeof(Layer)) as Layer;
            Layer target = ((FrameworkElement)(sender)).DataContext as Layer;

            int removedIdx = lstLayers.Items.IndexOf(droppedData);
            int targetIdx = lstLayers.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                this.FocusedProfile?.Settings?.Layers.Insert(targetIdx + 1, droppedData);
                this.FocusedProfile?.Settings?.Layers.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (this.FocusedProfile?.Settings?.Layers.Count + 1 > remIdx)
                {
                    this.FocusedProfile?.Settings?.Layers.Insert(targetIdx, droppedData);
                    this.FocusedProfile?.Settings?.Layers.RemoveAt(remIdx);
                }
            }
        }

        private void lstLayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView)
                this.btnRemoveLayer.IsEnabled = (sender as ListView).HasItems && (sender as ListView).SelectedIndex > -1;
        }

        private void btnProfileOverview_Click(object sender, RoutedEventArgs e)
        {
            if (FocusedProfile != null)
            {
                ProfileOverviewRequest?.Invoke(FocusedProfile.Control);
                lstLayers.SelectedIndex = -1;
            }
        }
    }
}
