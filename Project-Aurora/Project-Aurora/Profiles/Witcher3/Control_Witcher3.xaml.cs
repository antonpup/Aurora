using Aurora.Controls;
using Aurora.Utils;
using System;
using System.IO;
using Ionic.Zip;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Aurora.Devices;
using Aurora.Settings;
using Xceed.Wpf.Toolkit;
using Aurora.Profiles.Witcher3.GSI;

namespace Aurora.Profiles.Witcher3
{
    /// <summary>
    /// Interaction logic for Control_Witcher3.xaml
    /// </summary>
    public partial class Control_Witcher3 : UserControl
    {
        private Application profile_manager;

        public Control_Witcher3(Application profile)
        {
            InitializeComponent();

            profile_manager = profile;

            SetSettings();

            if (!(profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled)
            {
                (profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled = true;
            }

            profile_manager.ProfileChanged += Control_Witcher3_ProfileChanged;

        }

        private void Control_Witcher3_ProfileChanged(object sender, EventArgs e)
        {
            SetSettings();
        }

        private void SetSettings()
        {
            this.game_enabled.IsChecked = profile_manager.Settings.IsEnabled;

        }

        //Overview
        
        private void install_mod_button_Click(object sender, RoutedEventArgs e)
        {
            String installpath = Utils.SteamUtils.GetGamePath(292030);
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                if (Directory.Exists(installpath))
                {
                    using (MemoryStream w3_mod = new MemoryStream(Properties.Resources.witcher3_mod))
                    {
                        using (ZipFile zip = ZipFile.Read(w3_mod))
                        {
                            foreach (ZipEntry entry in zip)
                            {
                                entry.Extract(installpath, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }

                    }

                    System.Windows.MessageBox.Show("Witcher 3 mod installed.");
                }
                else
                {
                    System.Windows.MessageBox.Show("Witcher 3 directory is not found.\r\nCould not install the mod.");
                }    
            }
            else
            {
                System.Windows.MessageBox.Show("Witcher 3 was not installed through steam, please pick the path manually");
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    using (MemoryStream w3_mod = new MemoryStream(Properties.Resources.witcher3_mod))
                    {
                        using (ZipFile zip = ZipFile.Read(w3_mod))
                        {
                            foreach (ZipEntry entry in zip)
                            {
                                entry.Extract(dialog.SelectedPath, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                    }
                }
            }
        }

        private void uninstall_mod_button_Click(object sender, RoutedEventArgs e)
        {
            String installpath = Utils.SteamUtils.GetGamePath(292030);
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                if (Directory.Exists(installpath))
                {
                    Directory.Delete(System.IO.Path.Combine(installpath, "mods", "modArtemis"), true);
                    File.Delete(System.IO.Path.Combine(installpath, "bin", "config", "r4game", "user_config_matrix", "pc", "artemis.xml"));
                    System.Windows.MessageBox.Show("Witcher 3 mod removed.");
                }
                else
                {
                    System.Windows.MessageBox.Show("Witcher 3 directory is not found.\r\nCould not uninstall the mod.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Witcher 3 was not installed through steam, please pick the path manually");
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Directory.Delete(System.IO.Path.Combine(installpath, "mods", "modArtemis"), true);
                    File.Delete(System.IO.Path.Combine(installpath, "bin", "config", "r4game", "user_config_matrix", "pc", "artemis.xml"));
                    System.Windows.MessageBox.Show("Witcher 3 mod removed.");
                }
            }
        }


        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile_manager.Settings.IsEnabled = (this.game_enabled.IsChecked.HasValue) ? this.game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
