using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public delegate void NewLayerRendered(Canvas c);

    public class GlobalDeviceLayout : ObjectSettings<GlobalDeviceLayoutSettings>, IInit
    {
        public Dictionary<(byte type, byte id), DeviceLayout> DeviceLookup = null;

        public List<DeviceLayout> AllLayouts => DeviceLookup.Values.ToList();

        private bool isIntialized = false;
        public bool Initialized => isIntialized;

        public delegate void LayoutUpdatedEventHandler(object sender);
        public event LayoutUpdatedEventHandler LayoutChanged;

        public static GlobalDeviceLayout Instance { get; } = new GlobalDeviceLayout();

        public int CanvasWidth => LayoutUtils.PixelToByte(this.Width);
        public int CanvasHeight => LayoutUtils.PixelToByte(this.Height);
        public float Width { get; protected set; }
        public float Height { get; protected set; }

        public int CanvasBiggest => CanvasWidth > CanvasHeight ? CanvasWidth : CanvasHeight;

        public List<DeviceLED> AllLeds { get; protected set; }
        public int CanvasWidthCenter => CanvasWidth / 2;
        public int CanvasHeightCenter => CanvasHeight / 2;

        public event NewLayerRendered NewLayerRender = delegate { };


        private GlobalDeviceLayout()
        {
            SettingsSavePath = Path.Combine(Global.SavePath, "GlobalDeviceLayout.json");
        }

        public bool Initialize()
        {
            if (isIntialized)
                return true;

            LoadSettings();

            this.initialiseDevices();
            this.deviceLayoutsChanged();

            return (isIntialized = true);
        }

        private void initialiseDevices()
        {
            DeviceLookup = new Dictionary<(byte type, byte id), DeviceLayout>();

            foreach (KeyValuePair<byte, ObservableCollection<DeviceLayout>> deviceType in this.Settings.Devices)
            {
                deviceType.Value.CollectionChanged += deviceLayoutsOrderChanged;

                for (byte i = 0; i < deviceType.Value.Count; i++)
                {
                    DeviceLayout deviceLayout = deviceType.Value[i];
                    DeviceLookup.Add((type: deviceLayout.GetDeviceTypeID, id: i), deviceLayout);
                    deviceLayout.DeviceID = i;

                    initialiseDevice(deviceLayout);
                }
            }
        }

        private void initialiseDevice(DeviceLayout deviceLayout)
        {
            deviceLayout.Initialize();
            deviceLayout.PropertyChanged += this.DeviceLayout_PropertyChanged;
            deviceLayout.LayoutUpdated += this.DeviceLayout_LayoutUpdated;
        }

        private void DeviceLayout_PropertyChanged(object sender, PropertyChangedExEventArgs e)
        {
            if (sender is DeviceLayout deviceLayout)
                deviceLayout.GenerateLayout();
        }

        private void DeviceLayout_LayoutUpdated(object sender)
        {
            this.deviceLayoutsChanged();
        }

        private void deviceLayoutsChanged()
        {
            //DeviceLookup = new Dictionary<(byte type, byte id), DeviceLayout>();
            AllLeds = new List<DeviceLED>();
            float maxWidth = 0;
            float maxHeight = 0;

            foreach (KeyValuePair<byte, ObservableCollection<DeviceLayout>> deviceType in this.Settings.Devices)
            {
                for (byte i = 0; i < deviceType.Value.Count; i++)
                {
                    DeviceLayout deviceLayout = deviceType.Value[i];

                    //Ensure LyoutUpdated event handler is being used
                    //deviceLayout.LayoutUpdated -= this.DeviceLayout_LayoutUpdated;

                    //Update global width and height
                    float w = deviceLayout.Location.X + deviceLayout.VirtualGroup.Region.Width;
                    float h = deviceLayout.Location.Y + deviceLayout.VirtualGroup.Region.Height;
                    if (w > maxWidth) maxWidth = w;
                    if (h > maxHeight) maxHeight = h;

                    //Update AllLeds
                    AllLeds.AddRange(deviceLayout.GetAllDeviceLEDs());
                }
            }

            this.Width = maxWidth;
            this.Height = maxHeight;
            LayoutChanged?.Invoke(this);
        }

        private void deviceLayoutsOrderChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DeviceLookup = new Dictionary<(byte type, byte id), DeviceLayout>();

            foreach (KeyValuePair<byte, ObservableCollection<DeviceLayout>> deviceType in this.Settings.Devices)
            {
                for (byte i = 0; i < deviceType.Value.Count; i++)
                {
                    DeviceLayout deviceLayout = deviceType.Value[i];
                    DeviceLookup.Add((type: deviceLayout.GetDeviceTypeID, id: i), deviceLayout);
                    deviceLayout.DeviceID = i;
                }
            }
        }

        public void AddDeviceLayout(DeviceLayout deviceLayout)
        {
            initialiseDevice(deviceLayout);

            if (this.Settings.Devices.ContainsKey(deviceLayout.GetDeviceTypeID))
                this.Settings.Devices[deviceLayout.GetDeviceTypeID].Add(deviceLayout);
            else
            {
                ObservableCollection<DeviceLayout> deviceLayouts = new ObservableCollection<DeviceLayout>();
                this.Settings.Devices.Add(deviceLayout.GetDeviceTypeID, deviceLayouts);
                //Add events for updating lookup on order changed and trigger it. Must be after it's been added to the main Devices Dict as otherwise it won't be processed in deviceLayoutsOrderChanged
                deviceLayouts.CollectionChanged += deviceLayoutsOrderChanged;
                deviceLayouts.Add(deviceLayout);
            }
        }

        public void PushFrame(Canvas canvas, bool applyBrightnessModifier = true)
        {
            //Apply global brightness
            if (applyBrightnessModifier)
                canvas *= this.Settings.GlobalBrightness;

            //Give bitmap to DeviceLayouts
            foreach (KeyValuePair<(byte type, byte id), DeviceLayout> device in this.DeviceLookup)
                device.Value.UpdateColors(canvas.GetDeviceBitmap(device.Key));

            //Push to DeviceManager
            Global.dev_manager.UpdateDevices(canvas.GlobalColour, this.DeviceLookup.Values.ToList());

            //Call NewLayerRender
            NewLayerRender?.Invoke(canvas);
        }

        public void UpdateDeviceControlColors()
        {
            foreach (KeyValuePair<(byte type, byte id), DeviceLayout> device in this.DeviceLookup)
                device.Value.VirtualGroup.UpdateColors(device.Value.DeviceColours);
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

        public string GetDeviceLEDName(DeviceLED deviceLED)
        {
            (DeviceLayout layout, LEDINT led) = GetDeviceFromDeviceLED(deviceLED);
            return layout.GetLEDName(led);
        }

        public DeviceLED SanitizeDeviceLED(DeviceLED deviceLED)
        {
            (DeviceLayout layout, LEDINT led) = GetDeviceFromDeviceLED(deviceLED);
            return layout.GetDeviceLED(layout.Sanitize(led));
        }

        public BitmapRectangle GetDeviceLEDBitmapRegion(DeviceLED led, bool local = false)
        {
            DeviceLayout layout;
            if ((layout = GetDeviceFromDeviceLED(led).layout).VirtualGroup.BitmapMap.TryGetValue(led.LedID, out BitmapRectangle rect))
            {
                if (!local)
                    rect = rect.AddOffset(layout.Location.ToPixel());

                return rect;
            }

            return null;
        }

        public Canvas GetCanvas()
        {
            return new Canvas(this);
        }

        public Grid GetControl(bool abstractView = false)
        {
            if (abstractView)
                throw new NotImplementedException();

            Grid grid = new Grid();
            
            foreach (DeviceLayout deviceLayout in AllLayouts)
            {
                Grid deviceGrid = deviceLayout.VirtualGroup.VirtualLayout;
                deviceGrid.Margin = deviceLayout.Location.GetMargin();

                grid.Children.Add(deviceGrid);
            }
            grid.Width = Width;
            grid.Height = Height;
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
