using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class DeviceKey
    {
        [JsonProperty("tag")]
        public int Tag { get; set; }
        [JsonProperty("visual_name")]
        public string VisualName { get; set; }


        [JsonProperty("device_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? DeviceId { get; set; }

        /*public bool Equals(DeviceKey otherKey)
        {
            return Tag == otherKey.Tag && DeviceId == otherKey.DeviceId;
        }

        public override bool Equals(object obj)
        {
            // Again just optimization
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            // Actually check the type, should not throw exception from Equals override
            if (obj.GetType() != this.GetType()) return false;

            // Call the implementation from IEquatable
            return Equals((DeviceKey)obj);
        }*/

        public bool Equals(DeviceKey key1, DeviceKey key2)
        {
            return key1.Tag == key2.Tag && key1.DeviceId == key2.DeviceId;
        }


        public int GetHashCode(DeviceKey obj)
        {
            return obj.Tag;
        }
        public class EqualityComparer : IEqualityComparer<DeviceKey>
        {
            public bool Equals(DeviceKey key1, DeviceKey key2)
            {
                return key1.Tag == key2.Tag && (key2.DeviceId == null || key1.DeviceId == key2.DeviceId);
            }


            public int GetHashCode(DeviceKey obj)
            {
                return obj.Tag;
            }

        }
        public static bool operator ==(DeviceKey key1, DeviceKey key2)
        {
            return key1.Tag == key2.Tag && (key2.DeviceId == null || key1.DeviceId == key2.DeviceId);
        }
        public static bool operator !=(DeviceKey key1, DeviceKey key2)
        {
            return !(key1.Tag == key2.Tag && (key2.DeviceId == null || key1.DeviceId == key2.DeviceId));
        }
        public DeviceKey()
        {
            Tag = -1;
            DeviceId = null;
        }
        public DeviceKey(int key)
        {
            Tag = key;
            DeviceId = null;
            VisualName = ((Devices.DeviceKeys)key).ToString();
        }
        public DeviceKey(Devices.DeviceKeys key, int? deviceId = null, string visualName = null)
        {
            Tag = (int)key;
            DeviceId = deviceId;
            if (visualName != null)
                VisualName = visualName;
            else
                VisualName = key.ToString();
        }
        public static implicit operator DeviceKey(Devices.DeviceKeys k) => new DeviceKey(k);

        public static implicit operator DeviceKey(Int64 k) => new DeviceKey((int)k);

    }
    public class DeviceKeyModifier
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string VisualName;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? X;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Y;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Width;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Height;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Image;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? FontSize;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled;

        public DeviceKeyModifier() { }

        public DeviceKeyModifier(DeviceKeyConfiguration conf)
        {
            VisualName = conf.Key.VisualName;
            X = conf.X;
            Y = conf.Y;
            Height = conf.Height;
            Width = conf.Width;
            FontSize = conf.FontSize;
            Enabled = conf.Enabled;
            Image = conf.Image;
        }
        public DeviceKeyModifier(DeviceKeyConfiguration baseConf, DeviceKeyConfiguration updateConf)
        {
            if (baseConf.Key == updateConf.Key)
            {
                if (updateConf.Key.VisualName != baseConf.Key.VisualName) VisualName = updateConf.Key.VisualName;
                if (updateConf.X != baseConf.X) X = updateConf.X;
                if (updateConf.Y != baseConf.Y) Y = updateConf.Y;
                if (updateConf.Height != baseConf.Height) Height = updateConf.Height - baseConf.Height;
                if (updateConf.Width != baseConf.Width) Width = updateConf.Width - baseConf.Width;
                if (updateConf.FontSize != baseConf.FontSize) FontSize = updateConf.FontSize;
                if (updateConf.Enabled != baseConf.Enabled) Enabled = updateConf.Enabled;
                if (updateConf.Image != baseConf.Image) Image = updateConf.Image;
            }
        }
    }
    public class DeviceKeyConfiguration : INotifyPropertyChanged
    {
        public DeviceKey Key = Devices.DeviceKeys.NONE;
        private int _x;
        public int X
        {
            get { return _x; }
            set
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }
        private int _y;
        public int Y
        {
            get { return _y; }
            set
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
        private int _height;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
        private string _image = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
                OnPropertyChanged(nameof(IsImage));
            }
        }
        [JsonIgnore]
        public bool IsImage => !String.IsNullOrWhiteSpace(Image);
        public double? FontSize;
        public bool? Enabled = true;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [JsonIgnore]
        public int Tag
        {
            get { return Key.Tag; }
            set
            {
                Key.Tag = value;
                OnPropertyChanged(nameof(Tag));
            }
        }

        [JsonIgnore]
        public bool VisualNameUpdateEnabled = true;
        [JsonIgnore]
        public string VisualName
        {
            get { return Key.VisualName; }
            set
            {
                Key.VisualName = value;
                OnPropertyChanged(nameof(VisualName));
            }
        }
        public DeviceKeyConfiguration()
        {
        }
        public DeviceKeyConfiguration(KeyboardKey key, int? deviceId)
        {
            Key = new DeviceKey(key.tag, deviceId, key.visualName);
            if (key.width != null) Width = (int)key.width;
            if (key.height != null) Height = (int)key.height;
            if (key.font_size != null) FontSize = key.font_size;
            if (key.margin_left != null) X = (int)key.margin_left;
            if (key.margin_top != null) Y = (int)key.margin_top;
            if (key.enabled != null) Enabled = key.enabled;
            if (!String.IsNullOrWhiteSpace(key.image)) Image = key.image;
        }
        public void UpdateFromOtherKey(KeyboardKey key)
        {
            if (key != null)
            {

                if (key.visualName != null) Key.VisualName = key.visualName;
                if ((int)key.tag != -1)
                    Key.Tag = (int)key.tag;
                if (key.width != null) Width = (int)key.width;
                if (key.height != null) Height = (int)key.height;
                if (key.font_size != null) FontSize = key.font_size;
                if (key.margin_left != null) X = (int)key.margin_left;
                if (key.margin_top != null) Y = (int)key.margin_top;
                if (key.enabled != null) Enabled = key.enabled;
                if (key.image != null) Image = key.image;
            }
        }
        public void ApplyModifier(DeviceKeyModifier modifier)
        {
            if (modifier.VisualName != null)
            {
                VisualName = modifier.VisualName;
                VisualNameUpdateEnabled = false;
            }
            if (modifier.X != null) X = modifier.X.Value;
            if (modifier.Y != null) Y = modifier.Y.Value;
            if (modifier.Height != null) Height += modifier.Height.Value;
            if (modifier.Width != null) Width += modifier.Width.Value;
            if (modifier.FontSize != null) FontSize = modifier.FontSize;
            if (modifier.Enabled != null) Enabled = modifier.Enabled;
            if (modifier.Image != null) Image = modifier.Image;
        }
        public static bool operator ==(DeviceKeyConfiguration key1, DeviceKeyConfiguration key2)
        {
            return key1.Tag == key2.Tag && key1.VisualName == key2.VisualName && key1.Image == key2.Image && key1.Enabled == key2.Enabled && key1.FontSize == key2.FontSize &&
                key1.Width == key2.Width && key1.Height == key2.Height && key1.X == key2.X && key1.Y == key2.Y;
        }
        public static bool operator !=(DeviceKeyConfiguration key1, DeviceKeyConfiguration key2)
        {
            return !(key1 == key2);
        }
    }
}
