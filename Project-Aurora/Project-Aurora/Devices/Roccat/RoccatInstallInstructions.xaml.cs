using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Devices.Roccat
{
    /// <summary>
    /// Interaction logic for RoccatInstallInstructions.xaml
    /// </summary>
    public partial class RoccatInstallInstructions : Window
    {
        public RoccatInstallInstructions()
        {
            InitializeComponent();
        }

        private void GoToTalkFXPage_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://www.computerbase.de/downloads/systemtools/roccat-talk-fx/");
        }
    }
}
