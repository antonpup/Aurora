using Aurora.Controls;
using Aurora.Profiles.Payday_2.GSI;
using Aurora.Settings;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Profiles.Payday_2
{
    /// <summary>
    /// Interaction logic for Control_PD2.xaml
    /// </summary>
    public partial class Control_PD2 : UserControl
    {
        private Application profile_manager;

        public Control_PD2(Application profile)
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

        private void get_hook_button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(@"https://superblt.znix.xyz/#superblt"));
        }

        private void install_mod_button_Click(object sender, RoutedEventArgs e)
        {
            string pd2path = Utils.SteamUtils.GetGamePath(218620);

            if(!String.IsNullOrWhiteSpace(pd2path))
            {
                if(Directory.Exists(pd2path))
                {
                    if(Directory.Exists(System.IO.Path.Combine(pd2path, "mods")))
                    {
                        using (MemoryStream gsi_pd2_ms = new MemoryStream(Properties.Resources.PD2_GSI))
                        {
                            using (ZipArchive zip = new ZipArchive(gsi_pd2_ms))
                            {
                                foreach (ZipArchiveEntry entry in zip.Entries)
                                {
                                    entry.ExtractToFile(pd2path, true);
                                }
                            }

                        }

                        MessageBox.Show("GSI for Payday 2 installed.");
                    }
                    else
                    {
                        MessageBox.Show("BLT Hook was not found.\r\nCould not install the GSI mod.");
                    }
                }
                else
                {
                    MessageBox.Show("Payday 2 directory is not found.\r\nCould not install the GSI mod.");
                }
            }
            else
            {
                MessageBox.Show("Payday 2 is not installed through Steam.\r\nCould not install the GSI mod.");
            }
        }

        private void get_lib_button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(@"https://modworkshop.net/mod/14924"));
        }
    }
}
