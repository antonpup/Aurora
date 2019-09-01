using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Devices.Layout
{
    public enum KeycapType
    {
        [Description("Default")]
        Default = 0,
        [Description("Default (with Backglow)")]
        Default_backglow = 1,
        [Description("Default (Backglow only)")]
        Default_backglow_only = 2,
        [Description("Colorized")]
        Colorized = 3,
        [Description("Colorized (blank)")]
        Colorized_blank = 4
    }

    public enum BitmapAccuracy
    {
        Best = 1,
        Great = 3,
        Good = 6,
        Okay = 9,
        Fine = 12
    }

    //This should really be using NotifyPropertyChangedEx
    public class GlobalDeviceLayoutSettings : SettingsBase
    {
        //DeviceType -> List of Devices of that type
        //Need to seperate them off so we can easily keep track of the device indexes
        private Dictionary<byte, ObservableCollection<DeviceLayout>> _devices = new Dictionary<byte, ObservableCollection<DeviceLayout>>();
        public Dictionary<byte, ObservableCollection<DeviceLayout>> Devices { get => _devices; set => UpdateVar(ref _devices, value); }

        private float _globalBrightness = 1.0f;
        public float GlobalBrightness { get => _globalBrightness; set => UpdateVar(ref _globalBrightness, value); }

        private KeycapType virtualKeyboardKeycapType = KeycapType.Default;
        public KeycapType VirtualKeyboardKeycapType { get => virtualKeyboardKeycapType; set => UpdateVar(ref virtualKeyboardKeycapType, value); }

        private BitmapAccuracy bitmapAccuracy = BitmapAccuracy.Okay;
        public BitmapAccuracy BitmapAccuracy { get { return bitmapAccuracy; } set { UpdateVar(ref bitmapAccuracy, value); } }

        private bool antiAliasing = false;
        public bool AntiAliasing { get => antiAliasing; set => UpdateVar(ref antiAliasing, value); }

        //Apparently, according to this: https://docs.microsoft.com/en-us/dotnet/api/system.drawing.drawing2d.smoothingmode?view=netframework-4.7.2 most of the modes are equivalent to eachother and really the
        //only difference is AntiAliasing or no AntiAliasing so no point in having the option to select those
        [JsonIgnore]
        public SmoothingMode SmoothingMode => antiAliasing ? SmoothingMode.AntiAlias : SmoothingMode.None;

        public GlobalDeviceLayoutSettings() : base() {
            _devices.Add(KeyboardDeviceLayout.DeviceTypeID, new ObservableCollection<DeviceLayout>() { new KeyboardDeviceLayout() { Style = PreferredKeyboard.Wooting_One, Language = PreferredKeyboardLocalization.uk } });
            _devices.Add(MouseDeviceLayout.DeviceTypeID, new ObservableCollection<DeviceLayout>() { new MouseDeviceLayout() { Style = MouseDeviceLayout.PreferredMouse.SteelSeries_Rival_300 } });
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            //TODO: Check if the Dictionary index (Device type ID) matches device type id of matching DeviceLayouts
        }
    }
}
