using Aurora.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.Skype
{
    /// <summary>
    /// Interaction logic for Control_Overwatch.xaml
    /// </summary>
    public partial class Control_Skype : UserControl
    {
        private Application application;

        public Control_Skype(Application profile)
        {
            InitializeComponent();

            this.DataContext = application = profile;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName("Aurora-SkypeIntegration.exe"))
                    proc.Kill();

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = Path.Combine(Global.ExecutingDirectory, "Aurora-SkypeIntegration.exe");
                Process.Start(startInfo);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Could not start Skype Integration. Error: " + exc);
            }
        }
    }
}
