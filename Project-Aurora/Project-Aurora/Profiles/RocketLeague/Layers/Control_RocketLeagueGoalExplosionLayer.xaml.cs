using Aurora.Settings;
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

namespace Aurora.Profiles.RocketLeague.Layers
{
    /// <summary>
    /// Interaction logic for Control_RocketLeagueBackgroundLayer.xaml
    /// </summary>
    public partial class Control_RocketLeagueGoalExplosionLayer : UserControl
    {
        public Control_RocketLeagueGoalExplosionLayer()
        {
            InitializeComponent();
        }

        public Control_RocketLeagueGoalExplosionLayer(RocketLeagueGoalExplosionLayerHandler datacontext)
        {
            this.DataContext = datacontext;
            InitializeComponent();         
        }

        internal void SetProfile(Application profile)
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
        }
    }
}
