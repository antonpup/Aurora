using Aurora.Profiles.Slime_Rancher.GSI;
using Aurora.Profiles.Slime_Rancher.GSI.Nodes;
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
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.Slime_Rancher
{
    /// <summary>
    /// Interaction logic for Control_Slime_Rancher.xaml
    /// </summary>
    public partial class Control_Slime_Rancher : UserControl
    {

        private Application profile;

        public Control_Slime_Rancher(Application profile)
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

        #region Overview handlers
        private void GameEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                profile.Settings.IsEnabled = GameEnabled.IsChecked ?? false;
                profile.SaveProfiles();
            }
        }

        private void GoToSRMLPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://www.nexusmods.com/slimerancher/mods/2");
        }

        private void GoToModDownloadPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://www.nexusmods.com/slimerancher/mods/18");
        }

        #endregion
        
        #region Preview Handlers
        private GameState_Slime_Rancher State => profile.Config.Event._game_state as GameState_Slime_Rancher;

        private void InGameCh_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                State.GameState.State = GameStateEnum.InGame;
            }
            else
            {
                State.GameState.State = GameStateEnum.Menu;
            }
        }

        private void HealthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            State.Player.Health.Current = (int)e.NewValue;
            State.Player.Health.Max = 100;
        }

        private void EnergySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            State.Player.Energy.Current = (int)e.NewValue;
            State.Player.Energy.Max = 100;
        }

        private void RadSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            State.Player.Radiation.Current = (int)e.NewValue;
            State.Player.Radiation.Max = 100;
        }
        #endregion
    }
}