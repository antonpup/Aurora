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
using Aurora.Profiles;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_ProfileControlPresenter.xaml
    /// </summary>
    public partial class Control_ProfileControlPresenter : UserControl
    {
        private bool isSettingNewLayer = false;

        protected ProfileSettings _ProfileSettings;

        public ProfileSettings ProfileSettings { get { return _ProfileSettings; } set { _ProfileSettings = value; SetProfile(value); } }

        public Control_ProfileControlPresenter()
        {
            InitializeComponent();
        }

        public Control_ProfileControlPresenter(ProfileSettings profile) : this()
        {
            ProfileSettings = profile;
            grd_LayerControl.IsHitTestVisible = true;
            grd_LayerControl.Effect = null;
        }

        private void SetProfile(ProfileSettings profile)
        {
            isSettingNewLayer = true;

            DataContext = profile;
            this.keybindEditor.ContextKeybind = profile.TriggerKeybind;

            isSettingNewLayer = false;
        }

        private void ResetProfile()
        {
            if (IsLoaded && !isSettingNewLayer && DataContext != null)
            {
                Type settingsType = DataContext.GetType();

                ProfileSettings = (ProfileSettings)Activator.CreateInstance(settingsType);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is Button)
                ResetProfile();
        }

        private void Control_Keybind_KeybindUpdated(object sender, Keybind newKeybind)
        {
            if (IsLoaded && !isSettingNewLayer && DataContext != null && DataContext is ProfileSettings)
            {
                (DataContext as ProfileSettings).TriggerKeybind = newKeybind;
            }
        }

        private void buttonResetKeybind_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && DataContext != null && DataContext is ProfileSettings)
            {
                Keybind newkb = new Keybind();

                (DataContext as ProfileSettings).TriggerKeybind = newkb;
                this.keybindEditor.ContextKeybind = newkb;
            }
        }
    }
}
