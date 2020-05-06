using Aurora.Devices;
using Aurora.Settings.DeviceLayoutViewer.Keycaps;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Aurora.Settings.DeviceLayoutViewer
{
    public class Control_KeycapModel : ViewModelBase
    {

        private KeycapViewer _keycap;
        public KeycapViewer Keycap
        {
            get { return _keycap; }
            set
            {
                _keycap = value;
                this.OnPropertyChanged(nameof(Keycap));
                //keycap.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                //OnPropertyChanged(nameof(Config));
            }
        }
    }

    /// <summary>
    /// Interaction logic for Control_Keycap.xaml
    /// </summary>
    public partial class Control_Keycap : UserControl
    {
        private bool IsSelected = false;
        private void PositionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y")
            {
                RenderTransform = new TranslateTransform((sender as DeviceKeyConfiguration).X, (sender as DeviceKeyConfiguration).Y);
            }
            else if (e.PropertyName == "Image")
            {
                Keycap = GetKeycapViewer(sender as DeviceKeyConfiguration);
            }
        }
        private KeycapViewer _keycap;
        public KeycapViewer Keycap
        {
            get { return _keycap; }
            set
            {
                
                //this.RemoveLogicalChild(_keycap);
                _keycap = value;
                this.Content = _keycap;
                RenderTransform = new TranslateTransform(_keycap.Config.X, _keycap.Config.Y);
                _keycap.Config.PropertyChanged += PositionChanged;
                //this.AddLogicalChild(_keycap);
                //AddChild(_keycap);
                //keycap = _keycap;
                //keycap.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                //OnPropertyChanged(nameof(Config));
            }
        }

        public Control_Keycap()
        {
            InitializeComponent();
            DataContext = Keycap;
        }
        private KeycapViewer GetKeycapViewer(DeviceKeyConfiguration conf)
        {
            if (!string.IsNullOrWhiteSpace(conf.Image))
                return new Control_ImageKeycap(conf);
            //Ghost keycap is used for abstract representation of keys
            /*if (abstractKeycaps)
                return new Control_GhostKeycap(conf, image_path);
            else*/
            {
                switch (Global.Configuration.virtualkeyboard_keycap_type)
                {
                    case KeycapType.Default_backglow:
                        return new Control_DefaultKeycapBackglow(conf);
                    case KeycapType.Default_backglow_only:
                        return new Control_DefaultKeycapBackglowOnly(conf);
                    case KeycapType.Colorized:
                        return new Control_ColorizedKeycap(conf);
                    case KeycapType.Colorized_blank:
                        return new Control_ColorizedKeycapBlank(conf);
                    default:
                        return new Control_DefaultKeycap(conf);
                }
            }
        }

        public Control_Keycap(DeviceKeyConfiguration key)
        {

            InitializeComponent();
            Keycap = GetKeycapViewer(key);
            DataContext = Keycap;

        }
        private string layoutsPath = System.IO.Path.Combine(Global.ExecutingDirectory, "DeviceLayouts");

        /*private void LoadImage()
        {
            if (string.IsNullOrWhiteSpace(Config.Image))
            {
                keyCap.Visibility = Visibility.Visible;
                keyCap.Text = Config.Key.VisualName;
                //keyCap.Tag = associatedKey.Tag;
                if (Config.FontSize != null)
                    keyCap.FontSize = Config.FontSize.Value;
            }
            else
            {
                string image_path = System.IO.Path.Combine(layoutsPath, "Images", Config.Image);

                keyCap.Visibility = Visibility.Hidden;

                if (System.IO.File.Exists(image_path))
                {
                    var memStream = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(image_path));
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = memStream;
                    image.EndInit();

                    grid_backglow.Visibility = Visibility.Hidden;
                    if (Config.Tag == -1)
                    {
                        keyBorder.Background = new ImageBrush(image);
                    }
                    else
                    {
                        keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                        keyBorder.OpacityMask = new ImageBrush(image);
                    }

                    IsImage = true;
                }
            }
            
        }*/

        public DeviceKey GetKey()
        {
            return Keycap.Config.Key;
        }


        public void SetColor(Color key_color)
        {
            Keycap.SetColor(key_color, Global.key_recorder.HasRecorded(GetKey()));
        }
        public void SelectionChanged()
        {
            IsSelected = !IsSelected;
            Keycap.SetColor(IsSelected ? new Color() { A = 255, R = 10, G = 255, B = 10 } : new Color(),  false);
        }


        internal DeviceKeyConfiguration GetConfiguration()
        {
            /*var offset = this.TranslatePoint(new Point(0, 0), VisualTreeHelper.GetParent(this) as UIElement) - Offset;
            const int epsilon = 3;
            /*if (Config.X < offset.X - epsilon || Config.X > offset.X + epsilon || Config.Y < offset.Y - epsilon || Config.Y > offset.Y + epsilon)
            {
                Config.X = (int)offset.X;// - (int)Offset.X;
                Config.Y = (int)offset.Y;// - (int)Offset.Y;
            }*/

            return Keycap.Config;
        }

    }
}
