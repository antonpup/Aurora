using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Settings
{
    /// <summary>
    /// A window that shows the Bitmap 
    /// </summary>
    public class Window_BitmapView : Window
    {

        private static Window_BitmapView winBitmapView = null;
        private Image imgBitmap = new Image();

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

            this.SourceInitialized += WinBitmapView_SourceInitialized;
            this.Closing += WinBitmapView_Closing;
            this.Closed += WinBitmapView_Closed;
            this.ResizeMode = ResizeMode.CanResize;

            this.SetBinding(Window.TopmostProperty, new Binding("BitmapDebugTopMost") { Source = Global.Configuration });

            this.Title = "Bitmap Debug Window";
            this.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            Global.effengine.NewLayerRender += Effengine_NewLayerRender;

            imgBitmap.SnapsToDevicePixels = true;
            imgBitmap.HorizontalAlignment = HorizontalAlignment.Stretch;
            imgBitmap.VerticalAlignment = VerticalAlignment.Stretch;
            /*imgBitmap.MinWidth = 0;
            imgBitmap.MinHeight = 0;*/
            imgBitmap.MinWidth = Effects.canvas_width;
            imgBitmap.MinHeight = Effects.canvas_height;

            this.Content = imgBitmap;
        }

        private void Effengine_NewLayerRender(System.Drawing.Bitmap bitmap)
        {
            try
            {
                Dispatcher.Invoke(
                    () =>
                    {
                        lock (bitmap)
                        {
                            using (MemoryStream memory = new MemoryStream())
                            {
                                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                                memory.Position = 0;
                                BitmapImage bitmapimage = new BitmapImage();
                                bitmapimage.BeginInit();
                                bitmapimage.StreamSource = memory;
                                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapimage.EndInit();

                                imgBitmap.Source = bitmapimage;
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                Global.logger.Warn(ex.ToString());
            }
        }
        private void WinBitmapView_SourceInitialized(object sender, EventArgs e)
        {
            Utils.WindowPlacement.SetPlacement(this, Global.Configuration.BitmapPlacement);
        }

        private void WinBitmapView_Closing(object sender, EventArgs e)
        {
            Global.Configuration.BitmapPlacement = Aurora.Utils.WindowPlacement.GetPlacement(this);
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