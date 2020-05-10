using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Aurora.Settings;
using RGB.NET.Core;
using RGB.NET.Devices.Corsair;
using RGB.NET.Groups;
using Color = System.Drawing.Color;

namespace Aurora.Devices.RGBNet
{
    public abstract class AbstractRGBNetDevice : Device
    {
        #region Properties & Fields

        private readonly object _lock = new object();

        private readonly RGBSurface _surface;
        protected abstract string DeviceName { get; }

        private readonly IRGBDeviceProvider _deviceProvider;
        private readonly AuroraRGBNetBrush _brush;
        private ListLedGroup _ledGroup;

        #endregion

        #region Constructors

        protected AbstractRGBNetDevice(IRGBDeviceProvider deviceProvider, AuroraRGBNetBrush brush = null)
        {
            this._deviceProvider = deviceProvider;

            _surface = RGBSurface.Instance;
            _brush = brush ?? new AuroraRGBNetBrush();
        }

        #endregion

        #region Methods

        public bool IsInitialized() => _deviceProvider?.IsInitialized ?? false;

        public string GetDeviceName() => DeviceName;

        public string GetDeviceDetails()
        {
            if (IsInitialized())
                return DeviceName + ": " + string.Join(" ", _deviceProvider.Devices.Select(d => d.DeviceInfo.DeviceName));

            return $"{DeviceName}: Not initialized";
        }

        public string GetDeviceUpdatePerformance() => "-"; //DarthAffe 03.02.2019: There's currently no way to get that information from RGB.NET

        public bool IsKeyboardConnected() => _deviceProvider.Devices.Any(d => d.DeviceInfo.DeviceType == RGBDeviceType.Keyboard);

        public bool IsPeripheralConnected() => _deviceProvider.Devices.Any(d => d.DeviceInfo.DeviceType == RGBDeviceType.Mouse); //TODO DarthAffe 03.02.2019: Which devices are "peripherals"?
        
        public bool Initialize()
        {
            lock (_lock)
            {
                if (IsInitialized()) return true; //DarthAffe 03.02.2019: I'm not sure if that's intended, but to me it seems aurora has a threading-issue in the initialization-part

                try
                {
                    _surface.LoadDevices(_deviceProvider, throwExceptions: true,exclusiveAccessIfPossible: false);
                    _ledGroup?.Detach(); //DarthAffe 03.02.2019: This should never run, but safety first
                    _ledGroup = new ListLedGroup(_deviceProvider.Devices.SelectMany(d => d)) { Brush = _brush };
                    //rgbDevice  = new CorsairDeviceProvider();
                   // _deviceProvider.Initialize(RGBDeviceType.Fan|RGBDeviceType.Cooler|RGBDeviceType.HeadsetStand);
                   
                    
                    //RGB.NET.Core.Color c = new RGB.NET.Core.Color(27, 184, 235);
                    //ledGroup.Brush = new SolidColorBrush(c);
                    //_surface.Update();
                    return IsInitialized();
                }
                catch (Exception ex)
                {
                    Global.logger.Error(ex, $"RGB.NET device ({GetDeviceName()}), Exception! Message: " + ex.Message);
                    return false;
                }
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
            => UpdateDevice(colorComposition.keyColors, e, forced);

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;

            _brush.KeyColors = keyColors;

            /*
            if (keyColors.ContainsKey(DeviceKeys.Peripheral_Logo))
            {
                if (_deviceProvider != null) {
                    var device = from d in _deviceProvider.Devices where d.DeviceInfo.DeviceType == RGBDeviceType.HeadsetStand select d;
                ILedGroup ledGroup = new ListLedGroup(device.First());
                ledGroup.Brush = new SolidColorBrush(new RGB.NET.Core.Color(234, 14, 214));
                }
            }
            */

            _surface.Update();
            return true;
        }

        public void Reset()
        {
            _deviceProvider.ResetDevices();
        }

        public void Shutdown()
        {
            _deviceProvider.Dispose();
        }

        public VariableRegistry GetRegisteredVariables() => new VariableRegistry();

        public bool Reconnect() => true;
        public bool IsConnected() => true;

        #endregion
    }
}
