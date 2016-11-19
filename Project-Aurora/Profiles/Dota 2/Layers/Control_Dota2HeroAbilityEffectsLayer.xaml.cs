using Aurora.Devices;
using Aurora.Settings;
using Aurora.Utils;
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

namespace Aurora.Profiles.Dota_2.Layers
{
    /// <summary>
    /// Interaction logic for Control_Dota2HeroAbilityEffectsLayer.xaml
    /// </summary>
    public partial class Control_Dota2HeroAbilityEffectsLayer : UserControl
    {
        private bool settingsset = false;
        private bool profileset = false;

        public Control_Dota2HeroAbilityEffectsLayer()
        {
            InitializeComponent();
        }

        public Control_Dota2HeroAbilityEffectsLayer(Dota2HeroAbiltiyEffectsLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Dota2HeroAbiltiyEffectsLayerHandler && !settingsset)
            {
                //Settings are set here...

                settingsset = true;
            }
        }

        internal void SetProfile(ProfileManager profile)
        {
            if (profile != null && !profileset)
            {
                var var_types_numerical = profile.ParameterLookup?.Where(kvp => Utils.TypeUtils.IsNumericType(kvp.Value.Item1));

                profileset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }
    }
}
