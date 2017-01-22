using Aurora.EffectsEngine.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Control_AnimationEditor.xaml
    /// </summary>
    public partial class Control_AnimationEditor : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty AnimationMixProperty = DependencyProperty.Register("AnimationMix", typeof(AnimationMix), typeof(Control_AnimationEditor));

        public AnimationMix AnimationMix
        {
            get
            {
                return (AnimationMix)GetValue(AnimationMixProperty);
            }
            set
            {
                SetValue(AnimationMixProperty, value);

                animMixer.ContextMix = value;
            }
        }

        public Control_AnimationEditor()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void animMixer_AnimationMixRendered(object sender)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    (sender as Control_AnimationMixPresenter).RenderedBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    keyboard_overlayPreview.Source = bitmapimage;
                }
            }
            catch(Exception exc)
            {
            }
        }
    }
}
