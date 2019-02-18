using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Aurora.Profiles.Osu {

    public partial class Control_Osu : UserControl {
        public Control_Osu(Application profile) {
            InitializeComponent();
            enableOsu.DataContext = profile.Settings;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
