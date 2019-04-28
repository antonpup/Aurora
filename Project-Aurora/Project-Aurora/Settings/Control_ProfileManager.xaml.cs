using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Aurora.Settings.Layers;
using Microsoft.Win32;
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
using System.Xml.Linq;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_SubProfileManager.xaml
    /// </summary>
    public partial class Control_ProfileManager : UserControl
    {
        public delegate void ProfileSelectedHandler(ApplicationProfile profile);

        public event ProfileSelectedHandler ProfileSelected;

        public static readonly DependencyProperty FocusedApplicationProperty = DependencyProperty.Register("FocusedApplication", typeof(Profiles.Application), typeof(Control_ProfileManager), new PropertyMetadata(null, new PropertyChangedCallback(FocusedProfileChanged)));

        public Dictionary<Profiles.Application, ApplicationProfile> LastSelectedProfile = new Dictionary<Profiles.Application, ApplicationProfile>();

        public Profiles.Application FocusedApplication
        {
            get { return (Profiles.Application)GetValue(FocusedApplicationProperty); }
            set
            {
                SetValue(FocusedApplicationProperty, value);
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
                Profiles.Application prof = ((Profiles.Application)e.OldValue);
                prof.ProfileChanged -= self.UpdateProfiles;
                //prof.SaveProfiles();

                if (self.LastSelectedProfile.ContainsKey(prof))
                    self.LastSelectedProfile.Remove(prof);

                self.LastSelectedProfile.Add(prof, self.lstProfiles.SelectedItem as ApplicationProfile);
            }
            self.UpdateProfiles();
            if (e.NewValue != null)
            {
                Profiles.Application profile = ((Profiles.Application)e.NewValue);

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
            this.lstProfiles.ItemsSource = this.FocusedApplication?.Profiles;
            lstProfiles.Items.SortDescriptions.Add(
            new System.ComponentModel.SortDescription("ProfileName",
            System.ComponentModel.ListSortDirection.Ascending));
            this.lstProfiles.SelectedItem = this.FocusedApplication?.Profiles.First((profile) => System.IO.Path.GetFileNameWithoutExtension(profile.ProfileFilepath).Equals(this.FocusedApplication?.Settings.SelectedProfile));
        }

        private void lstProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                if (lstProfiles.SelectedItem != null)
                {
                    if (!(lstProfiles.SelectedItem is ApplicationProfile))
                        throw new ArgumentException($"Items contained in the ListView must be of type 'ProfileSettings', not '{lstProfiles.SelectedItem.GetType()}'");

                    this.FocusedApplication?.SwitchToProfile(lstProfiles.SelectedItem as ApplicationProfile);

                    ProfileSelected?.Invoke(lstProfiles.SelectedItem as ApplicationProfile);
                    this.btnDeleteProfile.IsEnabled = true;
                }
                else
                    this.btnDeleteProfile.IsEnabled = false;
            }
        }

        private void btnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            this.FocusedApplication?.SaveDefaultProfile();

            this.lstProfiles.SelectedIndex = this.lstProfiles.Items.Count - 1;
        }

        private void buttonDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (this.lstProfiles.SelectedIndex > -1)
            {
                if (this.FocusedApplication.Profiles.Count == 1)
                {
                    MessageBox.Show("You cannot delete the last profile!");
                    return;
                }

                if (MessageBox.Show($"Are you sure you want to delete Profile '{((ApplicationProfile)lstProfiles.SelectedItem).ProfileName}'", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    int index = this.lstProfiles.SelectedIndex;
                    ApplicationProfile profile = (ApplicationProfile)this.lstProfiles.SelectedItem;

                    this.FocusedApplication.DeleteProfile(profile);

                    //this.lstProfiles.SelectedIndex = Math.Max(0, index - 1);
                }
            }
        }

        private void btnProfilePath_Click(object sender, RoutedEventArgs e)
        {
            if (FocusedApplication != null)
            {
                System.Diagnostics.Process.Start(FocusedApplication.GetProfileFolderPath());
            }
        }


        private void btnProfileReset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to reset the \"{this.FocusedApplication.Profile.ProfileName}\" profile?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                this.FocusedApplication?.ResetProfile();
        }

        private void lstProfiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                    btnCopyProfile_Click(null, null);
                else if (e.Key == Key.V)
                    btnPasteProfile_Click(null, null);
            }
            else if (e.Key == Key.Delete)
                buttonDeleteProfile_Click(null, null);
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

        private void btnImportProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                OpenFileDialog dlg = new OpenFileDialog
                {
                    Title = "Import profile",
                    // Set filter for file extension and default file extension 
                    Filter = "Importable files (*.json;*.cueprofile;*.cuefolder)|*.json;*.cueprofile;*.cuefolder|JSON files (*.json)|*.json|CUE Profile Files (*.cueprofile;*.cuefolder)|*.cueprofile;*.cuefolder"
                };

                // Display OpenFileDialog by calling ShowDialog method
                if (dlg.ShowDialog() == true)
                    FocusedApplication.ImportProfile(dlg.FileName);
            }
            catch (Exception exception)
            {
                Global.logger.Error("Exception Found: " + exception.ToString());
            }
        }

        private void btnExportProfile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export profile",
                Filter = "JSON file (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
                FocusedApplication.SaveProfile(FocusedApplication.Profile, dialog.FileName);
        }

        private void btnCopyProfile_Click(object sender, RoutedEventArgs e)
        {
            Global.Clipboard = (lstProfiles.SelectedItem as ApplicationProfile)?.Clone();
        }

        private void btnPasteProfile_Click(object sender, RoutedEventArgs e)
        {
            Global.isDebug = false;
            if (!(Global.Clipboard is ApplicationProfile)) return;

            ApplicationProfile src = (ApplicationProfile)Global.Clipboard;

            // Since we may be copying from one application to another, we need to re-create an application
            // profile since GTA profiles would not work with Desktop profiles for example.
            ApplicationProfile @new = FocusedApplication.AddNewProfile(src.ProfileName + " - Copy");
            @new.TriggerKeybind = src.TriggerKeybind.Clone();
            @new.Layers.Clear();

            // We then need to copy all layers from the layer on the clipboard to this new profile.
            // Check all the layers types to ensure that they can be added to this application (to prevent
            // crashes when copying a layer from an application that has a special layer unique to that app)
            for (int i = 0; i < src.Layers.Count; i++)
                if (Global.LightingStateManager.DefaultLayerHandlers.Contains(src.Layers[i].Handler.ID) || FocusedApplication.Config.ExtraAvailableLayers.Contains(src.Layers[i].Handler.ID))
                    @new.Layers.Add((Layer)src.Layers[i].Clone());
            
            FocusedApplication.SaveProfiles();
        }
    }
}