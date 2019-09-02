using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Aurora.Devices.Logitech
{
    public partial class LogitechInstallInstructions : Window
    {
        private Tuple<Bitmap, string>[] steps = {
            new Tuple<Bitmap, string>(new Bitmap(Properties.Resources.LogitechInstall_Step1), "Open your Logitech Gaming Software by either launching it, or clicking the LGS icon in your tray. Then press the settings button."),
            new Tuple<Bitmap, string>(new Bitmap(Properties.Resources.LogitechInstall_Step2), "Under General Settings tab, enable the \"Allow games to control illumination\" and \"Show game integration customization view\". Press \"OK\" to save settings and close the window."),
            new Tuple<Bitmap, string>(new Bitmap(Properties.Resources.LogitechInstall_Step3), "Open the Game Integration configuration menu, by selecting it on bottom of LGS and clicking the settings button."),
            new Tuple<Bitmap, string>(new Bitmap(Properties.Resources.LogitechInstall_Step4), "Select \"Counter Strike - GO\" from the Applets list, and disable both \"Arx Control\" and \"LED Backlighting\" as well as set \"Lauch the applet when\" to \"Never launch\"."),
            new Tuple<Bitmap, string>(new Bitmap(Properties.Resources.LogitechInstall_Step5), "Select \"Dota 2\" from the Applets list, and disable both \"Arx Control\" and \"LED Backlighting\" as well as set \"Lauch the applet when\" to \"Never launch\"."),
        };

        private int currentStep = 0;


        public LogitechInstallInstructions()
        {
            InitializeComponent();

            UpdateStepContent();
        }

        private void UpdateStepContent()
        {
            if(currentStep < steps.Length)
            {
                BitmapImage bitmapimage = new BitmapImage();
                using (MemoryStream memory = new MemoryStream())
                {
                    steps[currentStep].Item1.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();
                }

                content_image.Source = bitmapimage;
                content_textblock.Text = (string)steps[currentStep].Item2;
            }

            System.Windows.Media.Color enabled_color = System.Windows.Media.Color.FromArgb(255, 255, 255, 255);
            System.Windows.Media.Color disabled_color = System.Windows.Media.Color.FromArgb(255, 125, 125, 125);
            step1_text.Foreground = new SolidColorBrush(disabled_color);
            step2_text.Foreground = new SolidColorBrush(disabled_color);
            step3_text.Foreground = new SolidColorBrush(disabled_color);
            step4_text.Foreground = new SolidColorBrush(disabled_color);
            step5_text.Foreground = new SolidColorBrush(disabled_color);


            switch (currentStep)
            {
                case 4:
                    step5_text.Foreground = new SolidColorBrush(enabled_color);
                    break;
                case 3:
                    step4_text.Foreground = new SolidColorBrush(enabled_color);
                    break;
                case 2:
                    step3_text.Foreground = new SolidColorBrush(enabled_color);
                    break;
                case 1:
                    step2_text.Foreground = new SolidColorBrush(enabled_color);
                    break;
                case 0:
                    step1_text.Foreground = new SolidColorBrush(enabled_color);
                    break;
            }


            if(currentStep == steps.Length - 1)
                button_next.Content = "Finish";
            else
                button_next.Content = "Next";

            button_back.IsEnabled = currentStep != 0;
        }

        private void button_back_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep != 0)
            {
                currentStep--;
                UpdateStepContent();
            }
        }

        private void button_next_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep == steps.Length - 1)
            {
                this.Close();
            }
            else
            {
                currentStep++;
                UpdateStepContent();
            }
        }
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member