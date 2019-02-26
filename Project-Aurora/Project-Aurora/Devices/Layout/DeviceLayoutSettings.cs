using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private bool antiAliasing = false;
        public bool AntiAliasing { get => antiAliasing; set => UpdateVar(ref antiAliasing, value); }

        //Apparently, according to this: https://docs.microsoft.com/en-us/dotnet/api/system.drawing.drawing2d.smoothingmode?view=netframework-4.7.2 most of the modes are equivalent to eachother and really the
        //only difference is AntiAliasing or no AntiAliasing so no point in having the option to select those
        [JsonIgnore]
        public SmoothingMode SmoothingMode => antiAliasing ? SmoothingMode.AntiAlias : SmoothingMode.None;

        public GlobalDeviceLayoutSettings() : base() { }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            //TODO: Check if the Dictionary index (Device type ID) matches device type id of matching DeviceLayouts
        }

        public override void Default()
        {
            _devices.Add(KeyboardDeviceLayout.DeviceTypeID, new ObservableCollection<DeviceLayout>() { new KeyboardDeviceLayout() { Style = KeyboardDeviceLayout.PreferredKeyboard.Wooting_One, Language = KeyboardDeviceLayout.PreferredKeyboardLocalization.uk } });
            _devices.Add(MouseDeviceLayout.DeviceTypeID, new ObservableCollection<DeviceLayout>() { new MouseDeviceLayout() { Style = MouseDeviceLayout.PreferredMouse.SteelSeries_Rival_300 } });
        }
    }
}
