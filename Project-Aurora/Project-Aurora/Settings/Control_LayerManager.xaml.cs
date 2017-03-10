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

        public event NewLayerHandler SelectedLayerChanged;

        public delegate void ProfileOverviewHandler(UserControl profile_control);

        public event ProfileOverviewHandler ProfileOverviewRequest;

        public static readonly DependencyProperty FocusedProfileProperty = DependencyProperty.Register("FocusedProfile", typeof(ProfileManager), typeof(Control_LayerManager), new PropertyMetadata(null, new PropertyChangedCallback(FocusedProfileChanged)));

        public Dictionary<ProfileManager, Layers.Layer> LastSelectedLayer = new Dictionary<ProfileManager, Layers.Layer>();

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

                if (self.LastSelectedLayer.ContainsKey(prof))
                    self.LastSelectedLayer.Remove(prof);

                self.LastSelectedLayer.Add(prof, self.lstLayers.SelectedItem as Layers.Layer);

            }
            self.UpdateLayers();
            if (e.NewValue != null)
            {
                ProfileManager profile = ((ProfileManager)e.NewValue);

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
            this.lstLayers.ItemsSource = this.FocusedProfile?.Settings?.Layers;

            if (this.FocusedProfile is Profiles.Generic_Application.GenericApplicationProfileManager)
            {
                this.grid_timeselection.Visibility = Visibility.Visible;
                this.radiobtn_daytime.IsChecked = true;
                this.radiobtn_nighttime.IsChecked = false;
            }
            else
                this.grid_timeselection.Visibility = Visibility.Collapsed;

            if (lstLayers.SelectedItem != null)
                SelectedLayerChanged?.Invoke((Layers.Layer)lstLayers.SelectedItem);
            else
                ProfileOverviewRequest?.Invoke(FocusedProfile?.Control);
        }

        private void Layers_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.lstLayers.InternalItemsPanel.ManualDragUpdating = true;

            if (e.AddedItems.Count == 1)
            {
                var hander = SelectedLayerChanged;
                if (lstLayers.SelectedItem != null)
                {
                    if (!(lstLayers.SelectedItem is Layers.Layer))
                        throw new ArgumentException($"Items contained in the ListView must be of type 'Layer', not '{lstLayers.SelectedItem.GetType()}'");

                    Layers.Layer lyr = (Layers.Layer)lstLayers.SelectedItem;

                    lyr.SetProfile(FocusedProfile);

                    hander?.Invoke(lyr);
                    return;
                }
            }
            else if (e.RemovedItems.Count > 0)
            {
                this.FocusedProfile?.SaveProfiles();
            }

            ProfileOverviewRequest?.Invoke(FocusedProfile?.Control);
        }

        private void add_layer_button_Click(object sender, RoutedEventArgs e)
        {
            Layers.Layer lyr = new Layers.Layer("New layer " + Utils.Time.GetMilliSeconds());
            lyr.AnythingChanged += this.FocusedProfile.SaveProfilesEvent;

            lyr.SetProfile(FocusedProfile);

            if (this.FocusedProfile is Profiles.Generic_Application.GenericApplicationProfileManager && this.radiobtn_nighttime.IsChecked.Value)
            {
                ((FocusedProfile as Profiles.Generic_Application.GenericApplicationProfileManager)?.Settings as Profiles.Generic_Application.GenericApplicationSettings)?.Layers_NightTime?.Insert(0, lyr);
            }
            else
                this.FocusedProfile?.Settings?.Layers.Insert(0, lyr);

            this.lstLayers.SelectedItem = lyr;
        }

        private void btnRemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            if (this.lstLayers.SelectedIndex > -1)
            {
                if (MessageBox.Show($"Are you sure you want to delete Layer '{((Layers.Layer)lstLayers.SelectedItem).Name}'", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    int index = this.lstLayers.SelectedIndex;
                    if (this.FocusedProfile is Profiles.Generic_Application.GenericApplicationProfileManager && this.radiobtn_nighttime.IsChecked.Value)
                        ((FocusedProfile as Profiles.Generic_Application.GenericApplicationProfileManager)?.Settings as Profiles.Generic_Application.GenericApplicationSettings)?.Layers_NightTime?.RemoveAt(this.lstLayers.SelectedIndex);
                    else
                        this.FocusedProfile?.Settings?.Layers.RemoveAt(index);

                    this.lstLayers.SelectedIndex = Math.Max(-1, index - 1);
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
            if (FocusedProfile != null)
            {
                ProfileOverviewRequest?.Invoke(FocusedProfile.Control);
                lstLayers.SelectedIndex = -1;
            }
        }

        private void lstLayers_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                    Global.Clipboard = (this.lstLayers.SelectedItem as Layers.Layer)?.Clone();
                else if (e.Key == Key.V && Global.Clipboard is Layers.Layer)
                {
                    Layers.Layer lyr = (Layers.Layer)((Layers.Layer)Global.Clipboard)?.Clone();

                    if (Global.ProfilesManager.DefaultLayerHandlers.Contains(lyr.Handler.ID) || FocusedProfile.Config.ExtraAvailableLayers.Contains(lyr.Handler.ID))
                    {
                        lyr.Name += " - Copy";
                        lyr.SetProfile(FocusedProfile);

                        if (this.FocusedProfile is Profiles.Generic_Application.GenericApplicationProfileManager && this.radiobtn_nighttime.IsChecked.Value)
                            ((FocusedProfile as Profiles.Generic_Application.GenericApplicationProfileManager)?.Settings as Profiles.Generic_Application.GenericApplicationSettings)?.Layers_NightTime?.Insert(0, lyr);
                        else
                            FocusedProfile.Settings.Layers.Insert(0, lyr);

                    }
                }
            }
            else if (e.Key == Key.Delete)
            {
                this.btnRemoveLayer_Click(null, null);
            }
        }

        private void radiobtn_daytime_Checked(object sender, RoutedEventArgs e)
        {
            radiobtn_nighttime.IsChecked = false;

            if(FocusedProfile is Profiles.Generic_Application.GenericApplicationProfileManager)
                this.lstLayers.ItemsSource = ((FocusedProfile as Profiles.Generic_Application.GenericApplicationProfileManager)?.Settings as Profiles.Generic_Application.GenericApplicationSettings)?.Layers;
        }

        private void radiobtn_nighttime_Checked(object sender, RoutedEventArgs e)
        {
            radiobtn_daytime.IsChecked = false;

            if (FocusedProfile is Profiles.Generic_Application.GenericApplicationProfileManager)
                this.lstLayers.ItemsSource = ((FocusedProfile as Profiles.Generic_Application.GenericApplicationProfileManager)?.Settings as Profiles.Generic_Application.GenericApplicationSettings)?.Layers_NightTime;

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

        public UserControl GetCurrentControl()
        {
            return (this.lstLayers.SelectedItem as Layer)?.Control ?? this.FocusedProfile?.Control;
        }
    }
}
