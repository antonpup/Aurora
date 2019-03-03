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
using Xceed.Wpf.Toolkit;

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

        #region Overview handlers
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

        #endregion
        
        #region Preview Handlers
        private GameState_Subnautica State => profile.Config.Event._game_state as GameState_Subnautica;

        private void InGameCh_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                State.GameState.GameState = 2;
                State.GameState.InGame = true;
            }
            else
            {
                State.GameState.GameState = 0;
                State.GameState.InGame = false;
            }
        }

        private void HealthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            State.Player.Health = (int)e.NewValue;
        }

        private void HungerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            State.Player.Food = (int)e.NewValue;
        }

        private void WaterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            State.Player.Water = (int)e.NewValue;
        }

        private void Oxygen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            State.Player.OxygenAvailable = (int)e.NewValue;
        }

        private void preview_DepthLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
                State.Player.DepthLevel = (sender as IntegerUpDown).Value.Value;
        }
        #endregion
    }
}