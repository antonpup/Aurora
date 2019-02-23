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

namespace Aurora.Profiles.ResidentEvil2.Layers
{
    /// <summary>
    /// Interaction logic for Control_ResidentEvil2RankLayer.xaml
    /// </summary>
    public partial class Control_ResidentEvil2RankLayer : UserControl
    {
        private bool settingsset = false;
        private bool profileset = false;

        public Control_ResidentEvil2RankLayer()
        {
            InitializeComponent();
        }

        public Control_ResidentEvil2RankLayer(ResidentEvil2RankLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is ResidentEvil2RankLayerHandler && !settingsset)
            {
                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
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
