using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Devices.SteelSeries
{
    public partial class SteelSeriesInstallInstructions : Window
    {
        private Tuple<Bitmap, string>[] steps = {
            new Tuple<Bitmap, string>(new Bitmap(Properties.Resources.SteelSeriesInstall_Step1), "Open your SteelSeries Engine 3 Client by either launching it, or clicking the SteelSeries icon in your tray. Then goto the Engine Apps tab."),
            new Tuple<Bitmap, string>(new Bitmap(Properties.Resources.SteelSeriesInstall_Step2), "Under Engine Apps tab, disable \"CS:GO\" and \"Dota 2\". You can close the SSE3 window now."),
        };

        private int currentStep = 0;


        public SteelSeriesInstallInstructions()
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


            switch (currentStep)
            {
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