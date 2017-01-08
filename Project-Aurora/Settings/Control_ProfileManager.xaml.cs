using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_ProfileManager.xaml
    /// </summary>
    public partial class Control_ProfileManager : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ProfileManagerProperty = DependencyProperty.Register("ProfileManager", typeof(ProfileManager), typeof(UserControl));

        public ProfileManager ProfileManager
        {
            get
            {
                return (ProfileManager)GetValue(ProfileManagerProperty);
            }
            set
            {
                SetValue(ProfileManagerProperty, value);

                this.profiles_combobox.Items.Clear();
                foreach(var kvp in value.Profiles)
                    this.profiles_combobox.Items.Add(kvp.Key);

                this.load_profile_button.IsEnabled = value.Profiles.Count > 0;
            }
        }

        public Control_ProfileManager()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void load_profile_button_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {
                if(this.profiles_combobox.SelectedItem != null && this.profiles_combobox.SelectedItem is string && !string.IsNullOrWhiteSpace(this.profiles_combobox.SelectedItem as string))
                {
                    ProfileManager.SwitchToProfile(this.profiles_combobox.SelectedItem as string);
                }
                else
                {
                    MessageBox.Show("Please either select an existing profile from the dropbox.");
                }

            }
        }

        private void save_profile_button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                if(this.profiles_combobox.Text != null && !string.IsNullOrWhiteSpace(this.profiles_combobox.Text))
                {
                    ProfileManager.SaveDefaultProfile(this.profiles_combobox.Text as string);

                    this.profiles_combobox.Items.Clear();
                    foreach (var kvp in ProfileManager.Profiles)
                        this.profiles_combobox.Items.Add(kvp.Key);

                    this.load_profile_button.IsEnabled = ProfileManager.Profiles.Count > 0;
                }
                else
                {
                    MessageBox.Show("Please either select an existing profile or\r\ntype a new profile name in the dropbox.");
                }
            }
        }

        private void reset_profile_button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                ProfileManager.ResetProfile();

                this.profiles_combobox.Items.Clear();
                foreach (var kvp in ProfileManager.Profiles)
                    this.profiles_combobox.Items.Add(kvp.Key);

                this.load_profile_button.IsEnabled = ProfileManager.Profiles.Count > 0;
            }
        }

        private void view_folder_button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                System.Diagnostics.Process.Start(ProfileManager.GetProfileFolderPath());
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
