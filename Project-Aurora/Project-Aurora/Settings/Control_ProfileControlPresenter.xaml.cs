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

        protected ApplicationProfile _Profile;

        public ApplicationProfile Profile { get { return _Profile; } set { _Profile = value; SetProfile(value); } }

        public Control_ProfileControlPresenter()
        {
            InitializeComponent();
        }

        public Control_ProfileControlPresenter(ApplicationProfile profile) : this()
        {
            Profile = profile;
            grd_LayerControl.IsHitTestVisible = true;
            grd_LayerControl.Effect = null;
        }

        private void SetProfile(ApplicationProfile profile)
        {
            isSettingNewLayer = true;

            DataContext = profile;
            this.keybindEditor.Stop();
            this.keybindEditor.ContextKeybind = profile.TriggerKeybind;

            isSettingNewLayer = false;
        }

        private void ResetProfile()
        {
            if (IsLoaded && !isSettingNewLayer && DataContext != null)
            {
                if (MessageBox.Show($"Are you sure you want to reset the \"{this.Profile.ProfileName}\" profile?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    this.Profile?.Reset();
                    //SetProfile(this.Profile);
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is Button)
                ResetProfile();
        }

        private void Control_Keybind_KeybindUpdated(object sender, Keybind newKeybind)
        {
            if (IsLoaded && !isSettingNewLayer && DataContext != null && DataContext is ApplicationProfile)
            {
                (DataContext as ApplicationProfile).TriggerKeybind = newKeybind;
            }
        }

        private void buttonResetKeybind_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && DataContext != null && DataContext is ApplicationProfile)
            {
                Keybind newkb = new Keybind();
                this.keybindEditor.Stop();
                (DataContext as ApplicationProfile).TriggerKeybind = newkb;
                this.keybindEditor.ContextKeybind = newkb;
            }
        }

        private void grd_LayerControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsLoaded)
                this.keybindEditor.Stop();
        }
    }
}
