using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace Aurora.Settings
{
    /// <summary>
    /// A window that shows the Bitmap 
    /// </summary>
    public class Window_BitmapView : Window
    {

        private static Window_BitmapView? winBitmapView;
        private Image imgBitmap = new();

        /// <summary>
        /// Opens the bitmap debug window if not already opened. If opened bring it to the foreground. 
        /// </summary>
        public static void Open()
        {
            if (winBitmapView == null)
            {
                winBitmapView = new Window_BitmapView();
                winBitmapView.UpdateLayout();
                winBitmapView.Show();
            }
            else
            {
                winBitmapView.Activate();
            }
        }

        private Window_BitmapView()
        {
            Closed += WinBitmapView_Closed;
            ResizeMode = ResizeMode.CanResize;

            SetBinding(TopmostProperty, new Binding("BitmapDebugTopMost") { Source = Global.Configuration });

            Title = "Bitmap Debug Window";
            Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            Global.effengine.NewLayerRender += Effengine_NewLayerRender;

            imgBitmap.SnapsToDevicePixels = true;
            imgBitmap.HorizontalAlignment = HorizontalAlignment.Stretch;
            imgBitmap.VerticalAlignment = VerticalAlignment.Stretch;
            imgBitmap.MinWidth = Effects.CanvasWidth;
            imgBitmap.MinHeight = Effects.CanvasHeight;

            Content = imgBitmap;
        }

        private void Effengine_NewLayerRender(Bitmap bitmap)
        {
            try
            {
                Dispatcher.Invoke(
                    () =>
                    {
                        lock (bitmap)
                        {
                            using MemoryStream memory = new MemoryStream();
                            bitmap.Save(memory, ImageFormat.Png);
                            memory.Position = 0;
                            BitmapImage bitmapimage = new BitmapImage();
                            bitmapimage.BeginInit();
                            bitmapimage.StreamSource = memory;
                            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapimage.EndInit();

                            imgBitmap.Source = bitmapimage;
                        }
                    });
            }
            catch (Exception ex)
            {
                Global.logger.Warning(ex.ToString());
            }
        }

        private void WinBitmapView_Closed(object sender, EventArgs e)
        {
            Global.effengine.NewLayerRender -= Effengine_NewLayerRender;

            //Set the winBitmapView instance to null if it got closed
            if (winBitmapView!= null && winBitmapView.Equals(this))
                winBitmapView = null;
        }
    }
}