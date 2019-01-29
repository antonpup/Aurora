using Aurora.Settings;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using LEDINT = System.Int16;

namespace Aurora.Devices.Layout
{
    public class GlobalDeviceLayout : ObjectSettings<GlobalDeviceLayoutSettings>, IInit
    {
        public Dictionary<(byte type, byte id), DeviceLayout> DeviceLookup = null;

        public List<DeviceLayout> AllLayouts => DeviceLookup.Values.ToList();

        private bool isIntialized = false;
        public bool Initialized => isIntialized;

        public event EventHandler LayoutChanged;

        public GlobalDeviceLayout()
        {
            SettingsSavePath = Path.Combine(Global.SavePath, "GlobalDeviceLayout.json");
        }

        public bool Initialize()
        {
            if (isIntialized)
                return true;

            LoadSettings();

            this.deviceLayoutsChanged();

            return (isIntialized = true);
        }

        private void deviceLayoutsChanged()
        {
            DeviceLookup = new Dictionary<(byte type, byte id), DeviceLayout>();
            foreach (KeyValuePair<byte, ObservableCollection<DeviceLayout>> deviceType in this.Settings.Devices)
            {
                for (byte i = 0; i < deviceType.Value.Count; i++)
                {
                    DeviceLayout deviceLayout = deviceType.Value[i];
                    DeviceLookup.Add((type: deviceLayout.DeviceID, id: i), deviceLayout);

                }
            }
        }



        public (DeviceLayout layout, LEDINT led) GetDeviceFromDeviceLED(DeviceLED led)
        {

            if (DeviceLookup.TryGetValue(led.GetLookupKey(), out DeviceLayout layout))
                return (layout: layout, led: led.LedID);

            throw new KeyNotFoundException();

            /*if (this.Settings.Devices.TryGetValue(led.DeviceTypeID, out ObservableCollection<DeviceLayout> layouts))
            {
                if (layouts.Count > led.DeviceID)
                {
                    return (layouts[led.DeviceID], led.LedID);
                }
                else
                {
                    //TODO: Not found
                }
            }
            else
            {
                //TODO: Not found
            }

            //TODO: Improve behavior
            throw new KeyNotFoundException();*/
        }

        public BitmapRectangle GetDeviceLEDBitmapRegion(DeviceLED led)
        {
            if (GetDeviceFromDeviceLED(led).layout.VirtualGroup.BitmapMap.TryGetValue(led.LedID, out BitmapRectangle rect))
                return rect;

            return null;
        }

        public Canvas GetCanvas()
        {
            return new Canvas(this);
        }

        public Grid GetControl()
        {
            Grid grid = new Grid();
            
            foreach (DeviceLayout deviceLayout in AllLayouts)
            {
                Grid deviceGrid = new Grid();
                deviceGrid.Margin = deviceLayout.Location.GetMargin();
                deviceGrid.Children.Add(deviceLayout.VirtualGroup.VirtualLayout);

                grid.Children.Add(deviceGrid);
            }

            return grid;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GlobalDeviceLayout() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}
