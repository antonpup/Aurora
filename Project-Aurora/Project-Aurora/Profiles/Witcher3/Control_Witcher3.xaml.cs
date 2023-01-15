using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;
using Aurora.Settings;
using Aurora.Utils;
using MessageBox = System.Windows.MessageBox;

namespace Aurora.Profiles.Witcher3
{
    /// <summary>
    /// Interaction logic for Control_Witcher3.xaml
    /// </summary>
    public partial class Control_Witcher3
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
            game_enabled.IsChecked = profile_manager.Settings.IsEnabled;

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
                MessageBox.Show("Witcher 3 was not installed through steam, please pick the path manually");
                var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
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
                var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UninstallMod(dialog.SelectedPath);
                }
            }
        }

        private void game_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile_manager.Settings.IsEnabled = (game_enabled.IsChecked.HasValue) ? game_enabled.IsChecked.Value : false;
                profile_manager.SaveProfiles();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("explorer", e.Uri.AbsoluteUri);
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
                using MemoryStream w3Mod = new MemoryStream(Properties.Resources.witcher3_mod);
                using ZipArchive zip = new ZipArchive(w3Mod);
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    var lowerByte = (byte)(entry.ExternalAttributes & 0x00FF);
                    var attributes = (FileAttributes)lowerByte;
                    if (attributes.HasFlag(FileAttributes.Directory))
                    {
                        Directory.CreateDirectory(Path.Combine(root, entry.FullName));
                    }
                    else
                    {
                        entry.ExtractToFile(Path.Combine(root, entry.FullName), true);
                    }
                }
                MessageBox.Show("Witcher 3 mod installed.");
            }
            catch (Exception e)
            {
                Global.logger.Error("Error installing the Witcher 3 mod: " + e.Message);
                MessageBox.Show("Witcher 3 directory is not found.\r\nCould not install the mod.");
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
            }
            catch (Exception e)
            {
                Global.logger.Error("Error uninstalling witcher 3 mod: " + e.Message);
                MessageBox.Show("Witcher 3 mod uninstall failed!");
            }
        }
    }
}