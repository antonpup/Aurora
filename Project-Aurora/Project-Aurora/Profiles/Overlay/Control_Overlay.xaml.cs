using System.Windows.Controls;

namespace Aurora.Profiles.Overlay {
    /// <summary>
    /// Interaction logic for Control_Overlay.xaml
    /// </summary>
    public partial class Control_Overlay : UserControl {

        public Control_Overlay(Application profile) {
            DataContext = profile;
            InitializeComponent();

            showOverlaysInPreview.IsChecked = Global.Configuration.OverlaysInPreview;
        }

        private void ShowOverlaysInPreviewCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e) {
            // Unforunately this has to be implemented as a method instead of a WPF Binding because for some reason the Global.Configurations
            // are fields not properties (despite having the property naming convention)
            Global.Configuration.OverlaysInPreview = ((CheckBox)sender).IsChecked ?? false;
            Settings.ConfigManager.Save(Global.Configuration);
        }
    }
}
