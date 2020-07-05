using Aurora.Controls;
using Aurora.Settings;
using Aurora.Utils;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.LeagueOfLegends
{
    /// <summary>
    /// Interaction logic for Control_LoL.xaml
    /// </summary>
    public partial class Control_LoL : UserControl
    {
        private Application profile_manager;

        public Control_LoL(Application profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();

            profile_manager.ProfileChanged += Profile_manager_ProfileChanged;
        }

        private void Profile_manager_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.game_enabled.IsChecked = profile_manager.Settings.IsEnabled;
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile_manager.Settings.IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string lolpath;
            try
            {
                lolpath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Riot Games, Inc\League of Legends", "Location", null);
            }
            catch
            {
                lolpath = String.Empty;
            }

            if (string.IsNullOrWhiteSpace(lolpath))
            {
                MessageBox.Show("Could not find the league of legends path automatically. Please select the correct location(Usually in c:\\Riot Games\\League of Legends)");
                var fp = new System.Windows.Forms.FolderBrowserDialog();
                if(fp.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    MessageBox.Show("Could not remove wrapper patch");
                    return;
                }
                if(!fp.SelectedPath.EndsWith("League of Legends"))
                {
                    MessageBox.Show("Could not remove wrapper patch");
                    return;
                }
                lolpath = fp.SelectedPath;
            }

            if (FileUtils.TryDelete(Path.Combine(lolpath, "Game", "LightFx.dll")))
                MessageBox.Show("Deleted file successfully");
            else
                MessageBox.Show("Could not find the wrapper file.");
        }
    }
}
