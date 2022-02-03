using Aurora.Devices;
using Aurora.Settings.DeviceLayoutViewer.Keycaps;
using Aurora.Settings.Keycaps;
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

    /// <summary>
    /// Interaction logic for Control_Keycap.xaml
    /// </summary>
    public partial class Control_Keycap : UserControl, IKeycap
    {
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
            else if (e.PropertyName == "VisualName")
            {
                Keycap = GetKeycapViewer(sender as DeviceKeyConfiguration);
            }
        }

        public DeviceKeyConfiguration Config => Keycap.Config;
        private KeycapViewer _keycap;
        public KeycapViewer Keycap
        {
            get { return _keycap; }
            set
            {
                _keycap = value;
                this.Content = _keycap;
                RenderTransform = new TranslateTransform(_keycap.Config.X, _keycap.Config.Y);
                _keycap.Config.PropertyChanged += PositionChanged;
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

            switch (Global.Configuration.VirtualkeyboardKeycapType)
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

        public Control_Keycap(DeviceKeyConfiguration key)
        {

            InitializeComponent();
            Keycap = GetKeycapViewer(key);

            DataContext = Keycap;

        }

        public DeviceKeys GetKey()
        {
            return (DeviceKeys)Keycap.Config.Key.Tag;
        }

        public void SetColor(Color key_color)
        {
            Keycap.SelectKey(Global.key_recorder.HasRecorded(GetKey()));
            Keycap.SetColor(key_color);
        }

    }
}
