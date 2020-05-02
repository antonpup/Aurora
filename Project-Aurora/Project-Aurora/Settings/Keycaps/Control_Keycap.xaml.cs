using Aurora.Devices;
using Aurora.Utils;
using System;
using System.Collections.Generic;
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

namespace Aurora.Settings.Keycaps
{
    /// <summary>
    /// Interaction logic for Control_Keycap.xaml
    /// </summary>
    public partial class Control_Keycap : UserControl, IKeycap
    {
        private Color currentColor = Color.FromArgb(0, 0, 0, 0);
        private DeviceKey AssociatedKey = DeviceKeys.NONE;
        private bool IsImage = false;
        public bool IsKeyMoveEnabled = false;
        private System.Windows.Point _positionInBlock;
        private DeviceKeyConfiguration Config;
        public System.Windows.Point Offset = new System.Windows.Point();
        public Control_Keycap()
        {
            InitializeComponent();
        }

        public Control_Keycap(DeviceKeyConfiguration key, string image_path)
        {
            InitializeComponent();

            Config = key;
            AssociatedKey = key.Key;

            this.Width = key.Region.Width;
            this.Height = key.Region.Height;

            //Keycap adjustments
            if (string.IsNullOrWhiteSpace(key.Image))
                keyBorder.BorderThickness = new Thickness(1.5);
            else
                keyBorder.BorderThickness = new Thickness(0.0);
            keyBorder.IsEnabled = key.Enabled.Value;

            if (!key.Enabled.Value)
            {
                ToolTipService.SetShowOnDisabled(keyBorder, true);
                keyBorder.ToolTip = new ToolTip { Content = "Changes to this key are not supported" };
            }

            if (string.IsNullOrWhiteSpace(key.Image))
            {
                keyCap.Text = AssociatedKey.VisualName;
                //keyCap.Tag = associatedKey.Tag;
                if (key.FontSize != null)
                    keyCap.FontSize = key.FontSize.Value;
            }
            else
            {
                keyCap.Visibility = System.Windows.Visibility.Hidden;

                if (System.IO.File.Exists(image_path))
                {
                    var memStream = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(image_path));
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = memStream;
                    image.EndInit();

                    grid_backglow.Visibility = Visibility.Hidden;
                    if (key.Tag == -1)
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


        }

        public DeviceKey GetKey()
        {
            return AssociatedKey;
        }

        public void SetColor(Color key_color)
        {
            //key_color = Color.FromArgb(255, 255, 255, 255); //No colors allowed!
            if (keyBorder.IsEnabled)
            {
                if (!IsImage)
                {
                    switch (Global.Configuration.virtualkeyboard_keycap_type)
                    {
                        case KeycapType.Default_backglow:
                            keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 30, 30, 30));
                            keyCap.Foreground = new SolidColorBrush(key_color);
                            grid_backglow.Background = new SolidColorBrush(key_color);
                            break;
                        case KeycapType.Default_backglow_only:
                            keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 30, 30, 30));
                            grid_backglow.Background = new SolidColorBrush(key_color);
                            break;
                        case KeycapType.Colorized:
                        case KeycapType.Colorized_blank:
                            keyBorder.Background = new SolidColorBrush(Utils.ColorUtils.MultiplyColorByScalar(key_color, 0.6));
                            keyBorder.BorderBrush = new SolidColorBrush(key_color);
                            break;
                        default:
                            keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 30, 30, 30));
                            if (string.IsNullOrWhiteSpace(keyCap.Text))
                                keyBorder.BorderBrush = new SolidColorBrush(key_color);
                            else
                                keyCap.Foreground = new SolidColorBrush(key_color);
                            break;
                    }

                }
                else
                {
                    if (AssociatedKey.Tag != (int)DeviceKeys.NONE)
                        keyBorder.Background = new SolidColorBrush(key_color);
                }
            }
            else
            {
                keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 100, 100));
                keyBorder.BorderThickness = new Thickness(0);
            }
            

            if (Global.key_recorder.HasRecorded(AssociatedKey))
                keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)255, (byte)0, (byte)(Math.Min(Math.Pow(Math.Cos((double)(Utils.Time.GetMilliSeconds() / 1000.0) * Math.PI) + 0.05, 2.0), 1.0) * 255), (byte)0));
            if (Global.Configuration.virtualkeyboard_keycap_type == KeycapType.Colorized_blank)
            {
                keyCap.Text = "";
            }
            else
            {
                UpdateText();
            }
            /*else
            {
                if (keyBorder.IsEnabled)
                {
                    if (isImage)
                        keyBorder.Background = new SolidColorBrush(key_color);
                    else
                        keyBorder.Background = new SolidColorBrush(Utils.ColorUtils.MultiplyColorByScalar(key_color, 0.6));
                }
                else
                {
                    keyBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 100, 100));
                    keyBorder.BorderThickness = new Thickness(0);
                }
            }*/

        }

        private void keyBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            virtualkeyboard_key_selected(AssociatedKey);
        }

        private void keyBorder_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void virtualkeyboard_key_selected(DeviceKey key)
        {
            if (key != DeviceKeys.NONE)
            {
                if (Global.key_recorder.HasRecorded(key))
                    Global.key_recorder.RemoveKey(key);
                else
                    Global.key_recorder.AddKey(key);
            }
        }

        private void keyBorder_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void keyBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                virtualkeyboard_key_selected(AssociatedKey);
        }

        public void UpdateText()
        {
            //if (Global.kbLayout.Loaded_Localization.IsAutomaticGeneration())
            {

                //if (keyCap.Text.Length > 1)
                //    return;

                StringBuilder sb = new StringBuilder(2);
                var scan_code = KeyUtils.GetScanCode((DeviceKeys)AssociatedKey.Tag);
                if (scan_code == -1)
                    return;
                /*var key = KeyUtils.GetFormsKey((KeyboardKeys)associatedKey.LedID);
                var scan_code = KeyUtils.MapVirtualKeyEx((uint)key, KeyUtils.MapVirtualKeyMapTypes.MapvkVkToVsc, (IntPtr)0x8090809);*/

                int ret = KeyUtils.GetKeyNameTextW((uint)scan_code << 16, sb, 2);
                keyCap.Text = sb.ToString().ToUpper();
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsKeyMoveEnabled)
            {
                // when the mouse is down, get the position within the current control. (so the control top/left doesn't move to the mouse position)
                _positionInBlock = Mouse.GetPosition(this);

                // capture the mouse (so the mouse move events are still triggered (even when the mouse is not above the control)
                this.CaptureMouse();
            }

        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            // if the mouse is captured. you are moving it. (there is your 'real' boolean)
            if (this.IsMouseCaptured)
            {
                // get the parent container
                var container = VisualTreeHelper.GetParent(this) as UIElement;

                // get the position within the container
                var mousePosition = e.GetPosition(container);

                // move the usercontrol.
                this.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, mousePosition.Y - _positionInBlock.Y);

            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // release this control.
            this.ReleaseMouseCapture();

            //DeviceLayoutUpdated?.Invoke(this);

            /*if (this.RenderTransform is TranslateTransform)
            {
                DeviceConfig.Offset.X = (this.RenderTransform as TranslateTransform).X;
                DeviceConfig.Offset.Y = (this.RenderTransform as TranslateTransform).Y;
                DeviceConfig.Save();
            }*/
            //SaveLayoutPosition(TranslatePoint(new Point(0, 0), VisualTreeHelper.GetParent(this) as UIElement));
        }

        internal DeviceKeyConfiguration GetConfiguration()
        {
            var offset = this.TranslatePoint(new Point(0, 0), VisualTreeHelper.GetParent(this) as UIElement) - Offset;
            const int epsilon = 3;
            if (Config.Region.X < offset.X - epsilon || Config.Region.X > offset.X + epsilon || Config.Region.Y < offset.Y - epsilon || Config.Region.Y > offset.Y + epsilon)
            {
                Config.Region.X = (int)offset.X;// - (int)Offset.X;
                Config.Region.Y = (int)offset.Y;// - (int)Offset.Y;
            }

            return Config;
        }
    }
}
