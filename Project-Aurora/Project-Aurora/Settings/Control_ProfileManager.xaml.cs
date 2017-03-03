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
    /// Interaction logic for Control_SubProfileManager.xaml
    /// </summary>
    public partial class Control_ProfileManager : UserControl
    {
        public delegate void ProfileSelectedHandler(ProfileSettings profile);

        public event ProfileSelectedHandler ProfileSelected;

        public static readonly DependencyProperty FocusedProfileProperty = DependencyProperty.Register("FocusedProfile", typeof(ProfileManager), typeof(Control_ProfileManager), new PropertyMetadata(null, new PropertyChangedCallback(FocusedProfileChanged)));

        public Dictionary<ProfileManager, ProfileSettings> LastSelectedProfile = new Dictionary<ProfileManager, ProfileSettings>();

        public ProfileManager FocusedProfile
        {
            get { return (ProfileManager)GetValue(FocusedProfileProperty); }
            set
            { 
                SetValue(FocusedProfileProperty, value);
            }
        }

        public Control_ProfileManager()
        {
            InitializeComponent();

            lstProfiles.SelectionMode = SelectionMode.Single;
            lstProfiles.SelectionChanged += lstProfiles_SelectionChanged;
        }

        public static void FocusedProfileChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            Control_ProfileManager self = source as Control_ProfileManager;
            if (e.OldValue != null)
            {
                ProfileManager prof = ((ProfileManager)e.OldValue);
                prof.ProfileChanged -= self.UpdateProfiles;
                prof.SaveProfiles();

                if (self.LastSelectedProfile.ContainsKey(prof))
                    self.LastSelectedProfile.Remove(prof);

                self.LastSelectedProfile.Add(prof, self.lstProfiles.SelectedItem as ProfileSettings);

            }
            self.UpdateProfiles();
            if (e.NewValue != null)
            {
                ProfileManager profile = ((ProfileManager)e.NewValue);

                profile.ProfileChanged += self.UpdateProfiles;

                if (self.LastSelectedProfile.ContainsKey(profile))
                    self.lstProfiles.SelectedItem = self.LastSelectedProfile[profile];
            }
        }

        public void UpdateProfiles()
        {
            this.UpdateProfiles(null, null);
        }

        public void UpdateProfiles(object sender, EventArgs e)
        {
            this.lstProfiles.ItemsSource = this.FocusedProfile?.Profiles;
        }

        private void lstProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                if (lstProfiles.SelectedItem != null)
                {
                    if (!(lstProfiles.SelectedItem is ProfileSettings))
                        throw new ArgumentException($"Items contained in the ListView must be of type 'ProfileSettings', not '{lstProfiles.SelectedItem.GetType()}'");

                    this.FocusedProfile?.SwitchToProfile(lstProfiles.SelectedItem as ProfileSettings);

                    ProfileSelected?.Invoke(lstProfiles.SelectedItem as ProfileSettings);
                    this.btnDeleteProfile.IsEnabled = true;
                }
                else
                    this.btnDeleteProfile.IsEnabled = false;
            }
            else if (e.RemovedItems.Count > 0)
                this.FocusedProfile?.SaveProfiles();
        }

        private void buttonSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            this.FocusedProfile?.SaveDefaultProfile();

            this.lstProfiles.SelectedIndex = this.lstProfiles.Items.Count - 1;
        }

        private void buttonDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (this.lstProfiles.SelectedIndex > -1)
            {
                if (MessageBox.Show($"Are you sure you want to delete Profile '{((ProfileSettings)lstProfiles.SelectedItem).ProfileName}'", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    int index = this.lstProfiles.SelectedIndex;
                    ProfileSettings profile = (ProfileSettings)this.lstProfiles.SelectedItem;

                    this.FocusedProfile.DeleteProfile(profile);

                    this.lstProfiles.SelectedIndex = Math.Max(0, index - 1);
                }
            }
        }

        Point? DragStartPosition;
        FrameworkElement DraggingItem;

        private void stckProfile_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (DragStartPosition == null || !this.lstProfiles.IsMouseOver)
                return;

            Point curr = e.GetPosition(null);
            Point start = (Point)DragStartPosition;

            if (Math.Abs(curr.X - start.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(curr.Y - start.Y) >= SystemParameters.MinimumVerticalDragDistance)
            {
                DragDrop.DoDragDrop(DraggingItem, DraggingItem.DataContext, DragDropEffects.Move);

            }
        }

        private void stckProfile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement stckProfile;
            if ((stckProfile = sender as FrameworkElement) != null)
            {
                //this.lstLayers.SelectedValue = stckLayer.DataContext;
                DragStartPosition = e.GetPosition(null);
                DraggingItem = stckProfile;
                //stckLayer.IsSelected = true;
            }
        }

        private void lstProfiles_PreviewMouseUp(object sender, EventArgs e)
        {
            DraggingItem = null;
            DragStartPosition = null;
        }

        //Based on: http://stackoverflow.com/questions/3350187/wpf-c-rearrange-items-in-listbox-via-drag-and-drop
        private void stckProfile_Drop(object sender, DragEventArgs e)
        {
            ProfileSettings droppedData = e.Data.GetData(typeof(ProfileSettings)) as ProfileSettings;
            ProfileSettings target = ((FrameworkElement)(sender)).DataContext as ProfileSettings;

            int removedIdx = lstProfiles.Items.IndexOf(droppedData);
            int targetIdx = lstProfiles.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                this.FocusedProfile?.Profiles.Insert(targetIdx + 1, droppedData);
                this.FocusedProfile?.Profiles.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;

                if (this.FocusedProfile?.Profiles.Count + 1 > remIdx)
                {
                    this.FocusedProfile?.Profiles.Insert(targetIdx, droppedData);
                    this.FocusedProfile?.Profiles.RemoveAt(remIdx);
                }
            }
        }

        private void btnProfilePath_Click(object sender, RoutedEventArgs e)
        {
            if (FocusedProfile != null)
            {
                System.Diagnostics.Process.Start(FocusedProfile.GetProfileFolderPath());
            }
        }

        private void lstProfiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                    Global.Clipboard = (this.lstProfiles.SelectedItem as ProfileSettings)?.Clone();
                else if (e.Key == Key.V && Global.Clipboard is ProfileSettings)
                {
                    ProfileSettings prof = (ProfileSettings)((ProfileSettings)Global.Clipboard)?.Clone();
                    prof.ProfileName += " - Copy";

                    FocusedProfile.Profiles.Insert(0, prof);

                    FocusedProfile.SaveProfiles();
                }
            }
            else if (e.Key == Key.Delete)
            {
                this.buttonDeleteProfile_Click(null, null);
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height < 80)
            {
                this.textblockDownload.Visibility = Visibility.Collapsed;
                this.borderBottom.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.textblockDownload.Visibility = Visibility.Visible;
                this.borderBottom.Visibility = Visibility.Visible;
            }
        }
    }
}
