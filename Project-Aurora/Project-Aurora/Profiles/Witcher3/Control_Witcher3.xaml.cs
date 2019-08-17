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
using MessageBox = System.Windows.MessageBox;

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
            String installpath = SteamUtils.GetGamePath(292030);
            if (!String.IsNullOrWhiteSpace(installpath))//if we find the path through steam
            {
                InstallMod(installpath);
            }
            else//user could have the GOG version of the game
            {
                System.Windows.MessageBox.Show("Witcher 3 was not installed through steam, please pick the path manually");
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    InstallMod(dialog.SelectedPath);
                }
            }
        }

        private void uninstall_mod_button_Click(object sender, RoutedEventArgs e)
        {
            String installpath = SteamUtils.GetGamePath(292030);
            if (!String.IsNullOrWhiteSpace(installpath))
            {
                UninstallMod(installpath);
            }
            else
            {
                MessageBox.Show("Witcher 3 was not installed through steam, please pick the path manually");
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    UninstallMod(dialog.SelectedPath);
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

        private void InstallMod(string root)
        {
            if (!Directory.Exists(root))
            {
                MessageBox.Show("Witcher 3 directory not found");
                return;
            }

            try
            {
                using (MemoryStream w3_mod = new MemoryStream(Properties.Resources.witcher3_mod))
                {
                    using (ZipFile zip = ZipFile.Read(w3_mod))
                    {
                        foreach (ZipEntry entry in zip)
                        {
                            entry.Extract(root, ExtractExistingFileAction.OverwriteSilently);//the zip's directory structure assumes
                                                                                             //it is extracted to the root folder of the game
                        }
                        MessageBox.Show("Witcher 3 mod installed.");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Global.logger.Error("Error installing the Witcher 3 mod: " + e.Message);
                MessageBox.Show("Witcher 3 directory is not found.\r\nCould not install the mod.");
                return;
            }
        }

        private void UninstallMod(string root)
        {
            if (!Directory.Exists(root))
            {
                MessageBox.Show("Witcher 3 directory not found");
                return;
            }

            var modfolder = Path.Combine(root, "mods", "modArtemis");
            var cfgfile = Path.Combine(root, "bin", "config", "r4game", "user_config_matrix", "pc", "artemis.xml");
            try
            {
                var previouslyInstalled = false;
                if (Directory.Exists(modfolder))
                {
                    previouslyInstalled = true;
                    Directory.Delete(modfolder, true);
                }
                if (File.Exists(cfgfile))
                {
                    previouslyInstalled = true;
                    File.Delete(cfgfile);
                }

                if(previouslyInstalled)
                    MessageBox.Show("Witcher 3 mod uninstalled successfully!");
                else
                    MessageBox.Show("Witcher 3 mod already uninstalled!");

                return;
            }
            catch (Exception e)
            {
                Global.logger.Error("Error uninstalling witcher 3 mod: " + e.Message);
                MessageBox.Show("Witcher 3 mod uninstall failed!");
                return;
            }
        }
    }
}