using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Aurora.Settings;

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

        public GlobalDeviceLayoutSettings()
        {
            
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            //TODO: Check if the Dictionary index (Device type ID) matches device type id of matching DeviceLayouts
        }
    }
}
