using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_ProfileManager.xaml
    /// </summary>
    public partial class Control_ScriptManager : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ProfileManagerProperty = DependencyProperty.Register("ProfileManager", typeof(ProfileManager), typeof(Control_ScriptManager));

        public ProfileManager ProfileManager
        {
            get
            {
                return (ProfileManager)GetValue(ProfileManagerProperty);
            }
            set
            {
                SetValue(ProfileManagerProperty, value);

                value.ProfileChanged += (sender, e) => {
                    this.Scripts = (sender as ProfileManager)?.Settings.ScriptSettings;
                };
                this.Scripts = value.Settings.ScriptSettings;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ScriptsProperty = DependencyProperty.Register("Scripts", typeof(Dictionary<string, ScriptSettings>), typeof(Control_ScriptManager));

        public Dictionary<string, ScriptSettings> Scripts
        {
            get
            {
                return (Dictionary<string, ScriptSettings>)GetValue(ScriptsProperty);
            }
            set
            {
                SetValue(ScriptsProperty, value);
            }
        }

        public Control_ScriptManager()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ProfileManager.SaveProfiles();
        }
    }
}
