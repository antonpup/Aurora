using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_ProfileManager.xaml
    /// </summary>
    public partial class Control_ScriptManager : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty ProfileManagerProperty = DependencyProperty.Register("ProfileManager", typeof(Settings.ProfileManager), typeof(Control_ScriptManager));

        public Settings.ProfileManager ProfileManager
        {
            get
            {
                return (Settings.ProfileManager)GetValue(ProfileManagerProperty);
            }
            set
            {
                SetValue(ProfileManagerProperty, value);

                this.lstScripts.Items.Clear();
                foreach (var kvp in value.EffectScripts)
                {
                    CheckBox chk = new CheckBox() { Name = kvp.Key, IsChecked = value.Settings.EnabledScripts.Contains(kvp.Key), Content = kvp.Value.Name ?? kvp.Key };
                    chk.Checked += ScriptCheckedChanged;
                    chk.Unchecked += ScriptCheckedChanged;
                    this.lstScripts.Items.Add(chk);
                }
                this.lstScripts.IsEnabled = this.lstScripts.Items.Count > 0;
            }
        }

       

        public Control_ScriptManager()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void ScriptCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                if (chk.IsChecked ?? false)
                    ProfileManager.Settings.EnabledScripts.Add(chk.Name);
                else
                    ProfileManager.Settings.EnabledScripts.Remove(chk.Name);

                ProfileManager.SaveProfiles();
            }
        }
    }
}
