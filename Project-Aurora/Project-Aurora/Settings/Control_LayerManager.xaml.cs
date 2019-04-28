using Aurora.Profiles;
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
        public delegate void NewLayerHandler(Layers.Layer layer);

        public event NewLayerHandler NewLayer;

        public delegate void ProfileOverviewHandler(UserControl profile_control);

        public event ProfileOverviewHandler ProfileOverviewRequest;

        public static readonly DependencyProperty FocusedApplicationProperty = DependencyProperty.Register("FocusedApplication", typeof(Profiles.Application), typeof(Control_LayerManager), new PropertyMetadata(null, new PropertyChangedCallback(FocusedProfileChanged)));

        public Dictionary<Profiles.Application, Layers.Layer> LastSelectedLayer = new Dictionary<Profiles.Application, Layer>();

        public Profiles.Application FocusedApplication
        {
            get { return (Profiles.Application)GetValue(FocusedApplicationProperty); }
            set
            { 
                SetValue(FocusedApplicationProperty, value);
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
                Profiles.Application prof = ((Profiles.Application)e.OldValue);
                prof.ProfileChanged -= self.UpdateLayers;
                prof.SaveProfile();

                if (self.LastSelectedLayer.ContainsKey(prof))
                    self.LastSelectedLayer.Remove(prof);

                self.LastSelectedLayer.Add(prof, self.lstLayers.SelectedItem as Layers.Layer);
            }
            self.UpdateLayers();
            if (e.NewValue != null)
            {
                Profiles.Application profile = ((Profiles.Application)e.NewValue);

                profile.ProfileChanged += self.UpdateLayers;

                if (self.LastSelectedLayer.ContainsKey(profile))
                    self.lstLayers.SelectedItem = self.LastSelectedLayer[profile];
            }
        }

        public void UpdateLayers()
        {
            this.UpdateLayers(null, null);
        }

        public void UpdateLayers(object sender, EventArgs e)
        {
            this.lstLayers.ItemsSource = this.FocusedApplication?.Profile?.Layers;
            
            //Make sure that it is on the correct list of layers
            this.radiobtn_daytime.IsChecked = true;
            if (this.FocusedApplication is Profiles.Generic_Application.GenericApplication && Global.Configuration.nighttime_enabled)
            {
                this.grid_timeselection.Visibility = Visibility.Visible;
                this.radiobtn_nighttime.IsChecked = false;
            }
            else
                this.grid_timeselection.Visibility = Visibility.Collapsed;
        }

        private void Layers_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var hander = NewLayer;
                if (lstLayers.SelectedItem != null)
                {
                    if (!(lstLayers.SelectedItem is Layers.Layer))
                        throw new ArgumentException($"Items contained in the ListView must be of type 'Layer', not '{lstLayers.SelectedItem.GetType()}'");

                    Layers.Layer lyr = (Layers.Layer)lstLayers.SelectedItem;

                    lyr.SetProfile(FocusedApplication);

                    hander?.Invoke(lyr);
                }
            }
            else if (lstLayers.SelectedItem == null && lstLayers.Items.Count > 0)
                lstLayers.SelectedIndex = 0;
        }

        private void add_layer_button_Click(object sender, RoutedEventArgs e)
        {
            Layers.Layer lyr = new Layers.Layer("New layer " + Utils.Time.GetMilliSeconds());
            lyr.AnythingChanged += this.FocusedApplication.SaveProfilesEvent;

            lyr.SetProfile(FocusedApplication);

            if (this.FocusedApplication is Profiles.Generic_Application.GenericApplication && this.radiobtn_nighttime.IsChecked.Value)
            {
                ((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers_NightTime?.Insert(0, lyr);
            }
            else
                this.FocusedApplication?.Profile?.Layers.Insert(0, lyr);

            this.lstLayers.SelectedItem = lyr;
        }

        private void btnRemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            if (this.lstLayers.SelectedIndex > -1)
            {
                if (MessageBox.Show($"Are you sure you want to delete Layer '{((Layers.Layer)lstLayers.SelectedItem).Name}'", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    int index = this.lstLayers.SelectedIndex;
                    if (this.FocusedApplication is Profiles.Generic_Application.GenericApplication && this.radiobtn_nighttime.IsChecked.Value)
                        ((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers_NightTime?.RemoveAt(this.lstLayers.SelectedIndex);
                    else
                        this.FocusedApplication?.Profile?.Layers.RemoveAt(index);

                    this.lstLayers.SelectedIndex = Math.Max(0, index - 1);
                }
            }
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
            Layers.Layer droppedData = e.Data.GetData(typeof(Layers.Layer)) as Layers.Layer;
            Layers.Layer target = ((FrameworkElement)(sender)).DataContext as Layers.Layer;

            int removedIdx = lstLayers.Items.IndexOf(droppedData);
            int targetIdx = lstLayers.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                if (this.FocusedApplication is Profiles.Generic_Application.GenericApplication && this.radiobtn_nighttime.IsChecked.Value)
                {
                    ((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers_NightTime?.Insert(targetIdx + 1, droppedData);
                    ((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers_NightTime?.RemoveAt(removedIdx);
                }
                else
                {
                    this.FocusedApplication?.Profile?.Layers.Insert(targetIdx + 1, droppedData);
                    this.FocusedApplication?.Profile?.Layers.RemoveAt(removedIdx);
                }
            }
            else
            {
                int remIdx = removedIdx + 1;

                if (this.FocusedApplication is Profiles.Generic_Application.GenericApplication && this.radiobtn_nighttime.IsChecked.Value)
                {
                    if (((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers_NightTime?.Count + 1 > remIdx)
                    {
                        ((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers_NightTime?.Insert(targetIdx, droppedData);
                        ((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers_NightTime?.RemoveAt(remIdx);
                    }
                }
                else
                {
                    if (this.FocusedApplication?.Profile?.Layers.Count + 1 > remIdx)
                    {
                        this.FocusedApplication?.Profile?.Layers.Insert(targetIdx, droppedData);
                        this.FocusedApplication?.Profile?.Layers.RemoveAt(remIdx);
                    }
                }
            }
        }

        private void lstLayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox)
            {
                ListBox lst = (ListBox)sender;
                this.btnRemoveLayer.IsEnabled = lst.HasItems && lst.SelectedIndex > -1;
            }
        }

        private void btnProfileOverview_Click(object sender, RoutedEventArgs e)
        {
            if (FocusedApplication != null)
            {
                ProfileOverviewRequest?.Invoke(FocusedApplication.Control);
                lstLayers.SelectedIndex = -1;
            }
        }

        private void lstLayers_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                    btnCopyLayer_Click(null, null);
                else if (e.Key == Key.V)
                    btnPasteLayer_Click(null, null);
            }
            else if (e.Key == Key.Delete)
                btnRemoveLayer_Click(null, null);
        }

        private void radiobtn_daytime_Checked(object sender, RoutedEventArgs e)
        {
            radiobtn_nighttime.IsChecked = false;

            if(FocusedApplication is Profiles.Generic_Application.GenericApplication)
                this.lstLayers.ItemsSource = ((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers;
        }

        private void radiobtn_nighttime_Checked(object sender, RoutedEventArgs e)
        {
            radiobtn_daytime.IsChecked = false;

            if (FocusedApplication is Profiles.Generic_Application.GenericApplication)
                this.lstLayers.ItemsSource = ((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers_NightTime;

        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            this.lstLayers.UpdateReordering(e);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.lstLayers.StopReordering();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.lstLayers.StopReordering();
        }

        private void LayerManager_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.lstLayers.SelectedItem == null)
                this.lstLayers.SelectedIndex = 0;

        }

        private void btnCopyLayer_Click(object sender, RoutedEventArgs e)
        {
            Global.Clipboard = (lstLayers.SelectedItem as Layer)?.Clone();
        }

        private void btnPasteLayer_Click(object sender, RoutedEventArgs e)
        {
            if (!(Global.Clipboard is Layer)) return;

            Layer lyr = (Layer)((Layer)Global.Clipboard).Clone();

            if (Global.LightingStateManager.DefaultLayerHandlers.Contains(lyr.Handler.ID) || FocusedApplication.Config.ExtraAvailableLayers.Contains(lyr.Handler.ID))
            {
                lyr.Name += " - Copy";
                lyr.SetProfile(FocusedApplication);

                if (FocusedApplication is Profiles.Generic_Application.GenericApplication && radiobtn_nighttime.IsChecked.Value)
                    ((FocusedApplication as Profiles.Generic_Application.GenericApplication)?.Profile as Profiles.Generic_Application.GenericApplicationProfile)?.Layers_NightTime?.Insert(0, lyr);
                else
                    FocusedApplication.Profile.Layers.Insert(0, lyr);
            }
        }
    }
}
