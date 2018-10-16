using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Aurora.Service
{
    /// <summary>
    /// Interaction logic for ServiceSettingsUI.xaml
    /// </summary>
    public partial class ServiceSettingsUI : UserControl
    {
        public ServiceSettingsUI()
        {
            InitializeComponent();
            this.uiOutputClientID.Text = WebSocketClient.uriREST + Global.Configuration.ClientID;
            uiOutputClientID.IsReadOnly = true;
            uiOutputGenerated.IsReadOnly = true;
            uiOutputGenerated.TextWrapping = TextWrapping.Wrap;
            this.Loaded += ServiceSettingsUI_Loaded;
            
        }

        private void ServiceSettingsUI_Loaded(object sender, RoutedEventArgs e)
        {
            uiEndingProfile.Items.Clear();
            uiStartingProfile.Items.Clear();
            uiEndingProfile.Items.Add("- Return to default -");
            foreach (var item in Global.LightingStateManager.DesktopProfile.Profiles)
            {
                uiStartingProfile.Items.Add(item.ProfileName);
                uiEndingProfile.Items.Add(item.ProfileName);
            }
        }

        private void uiGenerateNewClientID_Click(object sender, RoutedEventArgs e)
        {
            uiOutputClientID.Text = WebSocketClient.uriREST +  ProfileSwitcher.GetNewClientID();
        }

        private void uiActionCopyClientID_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(uiOutputClientID.Text);
        }
        private void uiActionMakePermanent_Unchecked(object sender, RoutedEventArgs e)
        {
            uiLabel1.IsEnabled = true;
            uiLabel2.IsEnabled = true;
            uiInputDuration.IsEnabled = true;
            uiEndingProfile.IsEnabled = true;
        }
        private void uiActionMakePermanent_Checked(object sender, RoutedEventArgs e)
        {
            uiLabel1.IsEnabled = false;
            uiLabel2.IsEnabled = false;
            uiInputDuration.IsEnabled = false;
            uiEndingProfile.IsEnabled = false;
        }

        private void uiActionGenerate_Click(object sender, RoutedEventArgs e)
        {
            string profileEnd = "";
            int duration = -1;
            if (uiEndingProfile.SelectedIndex == 0 || uiEndingProfile.SelectedItem == null)
            {
                //use default
              profileEnd = "";
            }
            else
            {
                profileEnd = uiEndingProfile.SelectedItem.ToString();
            }
            if (uiActionMakePermanent.IsChecked.Value)
            {
                duration = -1;
            }
            else
            {
                if (!int.TryParse(uiInputDuration.Text, out duration))
                {
                    System.Windows.Forms.MessageBox.Show("Duration is a number");
                    return;
                }
            }
            if (uiStartingProfile.SelectedItem == null)
            {
                System.Windows.Forms.MessageBox.Show("Please select at least one profile");
                return;
            }
            var p = ProfileSwitcherCommand.Generate(uiStartingProfile.SelectedItem.ToString(), Global.Configuration.ClientID,profileEnd, duration);
            uiOutputGenerated.Text = p.ToString();
        }

        private void uiActionCopyCode_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(uiOutputGenerated.Text);
        }

      
    }
}