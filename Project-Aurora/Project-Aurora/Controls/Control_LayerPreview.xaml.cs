using Aurora.Settings.Layers;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Aurora.Controls {

    /// <summary>
    /// A control that is capable of rendering the preview of any layer that correctly implements <see cref="INotifyRender"/>.
    /// </summary>
    public partial class Control_LayerPreview : UserControl {

        private bool eventsAttached;

        public Control_LayerPreview() {
            InitializeComponent();
        }

        #region Target Layer Property
        public INotifyRender TargetLayer {
            get => (INotifyRender)GetValue(TargetLayerProperty);
            set => SetValue(TargetLayerProperty, value);
        }
        public static readonly DependencyProperty TargetLayerProperty =
            DependencyProperty.Register("TargetLayer", typeof(INotifyRender), typeof(Control_LayerPreview), new PropertyMetadata(null, TargetLayerChanged));

        private static void TargetLayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = (Control_LayerPreview)d;
            // If the events are currently attached (I.E. a preview is currently running, then remove the handler from the old target and add to the new one)
            if (control.eventsAttached) {
                if (e.OldValue is INotifyRender old)
                    old.LayerRender -= control.RenderLayerPreview;
                if (e.NewValue is INotifyRender @new)
                    @new.LayerRender += control.RenderLayerPreview;
            }
        }
        #endregion

        // Start listening for when the particle layer is rendered so we can update the preview
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            if (TargetLayer != null)
                TargetLayer.LayerRender += RenderLayerPreview;
            eventsAttached = true;
        }

        // Stop listenting for when the particle layer is rendered (since you can't see the image now anyways)
        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            if (TargetLayer != null)
                TargetLayer.LayerRender -= RenderLayerPreview;
            eventsAttached = false;
        }

        // Take the bitmap from the layer and transform it into a format that can be used by WPF
        private void RenderLayerPreview(object sender, System.Drawing.Bitmap bitmap) =>
            Dispatcher.Invoke(delegate {
                using (var ms = new MemoryStream()) {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    var bitmapImg = new BitmapImage();
                    bitmapImg.BeginInit();
                    bitmapImg.StreamSource = ms;
                    bitmapImg.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImg.EndInit();
                    imagePreview.Source = bitmapImg;
                }
            });
    }
}
