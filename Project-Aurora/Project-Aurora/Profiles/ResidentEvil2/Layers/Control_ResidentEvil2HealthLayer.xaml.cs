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
    /// Interaction logic for Control_ResidentEvil2HealthLayer.xaml
    /// </summary>
    public partial class Control_ResidentEvil2HealthLayer : UserControl
    {
        private bool settingsset = false;

        public Control_ResidentEvil2HealthLayer()
        {
            InitializeComponent();
        }

        public Control_ResidentEvil2HealthLayer(ResidentEvil2HealthLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is ResidentEvil2HealthLayerHandler && !settingsset)
            {
                if (!this.status_style.HasItems)
                {
                    this.status_style.Items.Add(ResidentEvil2HealthLayerHandlerProperties.HealthDisplayType.Static);
                    this.status_style.Items.Add(ResidentEvil2HealthLayerHandlerProperties.HealthDisplayType.Scanning);
                }

                this.status_style.SelectedItem = (this.DataContext as ResidentEvil2HealthLayerHandler).Properties._DisplayType ?? ResidentEvil2HealthLayerHandlerProperties.HealthDisplayType.Static;

                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void status_style_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is ResidentEvil2HealthLayerHandler)
            {
                (this.DataContext as ResidentEvil2HealthLayerHandler).Properties._DisplayType = (ResidentEvil2HealthLayerHandlerProperties.HealthDisplayType)this.status_style.SelectedItem;
            }
        }
    }
}
