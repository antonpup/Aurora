using Aurora.Profiles.Subnautica.GSI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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

namespace Aurora.Profiles.Subnautica {
    /// <summary>
    /// Interaction logic for Control_Subnautica.xaml
    /// </summary>
    public partial class Control_Subnautica : UserControl
    {

        private Application profile;

        public Control_Subnautica(Application profile)
        {
            this.profile = profile;

            InitializeComponent();
            SetSettings();

            profile.ProfileChanged += (sender, e) => SetSettings();
        }

        private void SetSettings()
        {
            GameEnabled.IsChecked = profile.Settings.IsEnabled;
        }

        private void GameEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile.Settings.IsEnabled = GameEnabled.IsChecked ?? false;
                profile.SaveProfiles();
            }
        }

        private void GoToQModManagerPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://www.nexusmods.com/subnautica/mods/16/");
        }

        private void GoToModDownloadPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://www.nexusmods.com/subnautica/mods/171");
        }

    }
}