using Aurora.Utils;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Aurora.Settings.DeviceLayoutViewer.Keycaps
{
    public abstract class KeycapViewer : UserControl
    {

        private DeviceKeyConfiguration _config = new DeviceKeyConfiguration();
        public DeviceKeyConfiguration Config
        {
            get { return _config; }
            set
            {
                _config = value;
                
                //OnPropertyChanged(new DependencyPropertyChangedEventArgs(oldValue));
                //_config.PropertyChanged += PositionChanged;

                //OnPropertyChanged(nameof(Config));
            }
        }
        public KeycapViewer()
        {
        }
        public KeycapViewer(DeviceKeyConfiguration conf)
        {
            Config = conf;
        }
        public abstract void SetColor(Color key_color);
        public virtual DeviceKey GetKey() { return Config.Key; }

        protected bool IsSelected = false;

        public void SelectKey(bool isSelected)
        {
            IsSelected = isSelected;
        }

        public void UpdateText(TextBlock keycapName)
        {
            if (Config.VisualNameUpdateEnabled)
            {

                //if (keyCap.Text.Length > 1)
                //    return;

                StringBuilder sb = new StringBuilder(2);
                var scan_code = KeyUtils.GetScanCode((Devices.DeviceKeys)Config.Key.Tag);
                if (scan_code == -1)
                    return;
                /*var key = KeyUtils.GetFormsKey((KeyboardKeys)associatedKey.LedID);
                var scan_code = KeyUtils.MapVirtualKeyEx((uint)key, KeyUtils.MapVirtualKeyMapTypes.MapvkVkToVsc, (IntPtr)0x8090809);*/

                int ret = KeyUtils.GetKeyNameTextW((uint)scan_code << 16, sb, 2);
                keycapName.Text = sb.ToString().ToUpper();
            }
        }
        protected void keyBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
                virtualkeyboard_key_selected(GetKey());
        }

        protected void keyBorder_MouseMove(object sender, MouseEventArgs e)
        {
        }

        protected void virtualkeyboard_key_selected(DeviceKey key)
        {
            if (key.Tag != (int)Devices.DeviceKeys.NONE)
            {
                if (Global.key_recorder.HasRecorded(key))
                    Global.key_recorder.RemoveKey(key);
                else
                    Global.key_recorder.AddKey(key);
            }
        }

        protected void keyBorder_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        protected void keyBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Border)
                virtualkeyboard_key_selected(GetKey());


        }
    }
}
