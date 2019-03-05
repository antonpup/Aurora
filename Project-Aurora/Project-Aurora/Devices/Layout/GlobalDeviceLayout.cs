using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using LEDINT = System.Int16;

namespace Aurora.Devices.Layout
{

    public class TypeEntry
    {
        public Type Type { get; set; }

        public string Title { get; set; }

        public string Key { get; set; }

        public TypeEntry(string key, string title, Type type)
        {
            this.Type = type;
            this.Title = title;
            this.Key = key;
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public delegate void NewLayerRendered(Canvas c);

    public class GlobalDeviceLayout : ObjectSettings<GlobalDeviceLayoutSettings>, IInit
    {
        public Dictionary<(byte type, byte id), DeviceLayout> DeviceLookup = null;

        public List<DeviceLayout> AllLayouts => DeviceLookup.Values.ToList();

        public Dictionary<string, TypeEntry> DeviceLayoutTypes { get; private set; } = new Dictionary<string, TypeEntry>();

        private bool isIntialized = false;
        public bool Initialized => isIntialized;

        public delegate void LayoutUpdatedEventHandler(object sender);
        public event LayoutUpdatedEventHandler LayoutChanged;

        public static GlobalDeviceLayout Instance { get; } = new GlobalDeviceLayout();

        public int CanvasWidth => LayoutUtils.PixelToByte(this.Width);
        public int CanvasHeight => LayoutUtils.PixelToByte(this.Height);

        private float width = 0f;
        public float Width { get => width; protected set => UpdateVar(ref width, value); }
        private float height = 0f;
        public float Height { get => height; protected set => UpdateVar(ref height, value); }

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

            RegisterDeviceLayoutType(new List<TypeEntry>
            {
                new TypeEntry("keyboard", "Keyboard", typeof(KeyboardDeviceLayout)),
                new TypeEntry("mouse", "Mouse", typeof(MouseDeviceLayout))
            });

            LoadSettings();
            this.Settings.PropertyChanged += this.Settings_PropertyChanged;

            this.initialiseDevices();
            this.deviceLayoutsChanged();

            return (isIntialized = true);
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GlobalDeviceLayoutSettings.BitmapAccuracy) || e.PropertyName == nameof(GlobalDeviceLayoutSettings.VirtualKeyboardKeycapType))
            {
                RegenerateAll();
            }
        }

        bool ignoreLayoutUpdates = false;

        private void RegenerateAll()
        {
            ignoreLayoutUpdates = true;

            foreach (DeviceLayout layout in this.DeviceLookup.Values)
                layout.GenerateLayout();

            ignoreLayoutUpdates = false;
            this.deviceLayoutsChanged();
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

        private void DeviceLayout_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceLayout.Location))
            {
                deviceLocationChanged();
                return;
            }


            if (sender is DeviceLayout deviceLayout)
                deviceLayout.GenerateLayout();
        }

        private void DeviceLayout_LayoutUpdated(object sender)
        {
            if (ignoreLayoutUpdates)
                return;

            this.deviceLayoutsChanged();
        }

        private void deviceLocationChanged()
        {
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
                }
            }

            this.Width = maxWidth;
            this.Height = maxHeight;
        }

        private void deviceLayoutsChanged()
        {
            //DeviceLookup = new Dictionary<(byte type, byte id), DeviceLayout>();

            deviceLocationChanged();
            AllLeds = new List<DeviceLED>();
            foreach (KeyValuePair<byte, ObservableCollection<DeviceLayout>> deviceType in this.Settings.Devices)
            {
                for (byte i = 0; i < deviceType.Value.Count; i++)
                {
                    DeviceLayout deviceLayout = deviceType.Value[i];
                    //Update AllLeds
                    AllLeds.AddRange(deviceLayout.GetAllDeviceLEDs());
                }
            }
            control = null;
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

        #region DeviceLayoutType Handling
        //Basically just a modified version of what is done for LayerHandlers in LightStateManager. Could abstract it out into a base class, but doesn't seem super necessary atm


        public void RegisterDeviceLayoutType(List<TypeEntry> layers)
        {
            foreach (var layer in layers)
            {
                RegisterDeviceLayoutType(layer);
            }
        }

        public bool RegisterDeviceLayoutType(TypeEntry entry)
        {
            if (DeviceLayoutTypes.ContainsKey(entry.Key))
                return false;

            DeviceLayoutTypes.Add(entry.Key, entry);

            return true;
        }

        public bool RegisterDeviceLayoutType(string key, string title, Type type, bool @default = true)
        {
            return RegisterDeviceLayoutType(new TypeEntry(key, title, type));
        }

        public Type GetLayerHandlerType(string key)
        {
            return DeviceLayoutTypes.ContainsKey(key) ? DeviceLayoutTypes[key].Type : null;
        }

        public DeviceLayout GetDeviceLayoutInstance(TypeEntry entry)
        {
            return (DeviceLayout)Activator.CreateInstance(entry.Type);
        }

        public DeviceLayout GetLayerHandlerInstance(string key)
        {
            if (DeviceLayoutTypes.ContainsKey(key))
                return GetDeviceLayoutInstance(DeviceLayoutTypes[key]);

            return null;
        }

        #endregion

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

        private Grid control = null;
        
        public Grid GetControl(bool abstractView = false)
        {
            if (abstractView)
                throw new NotImplementedException();

            if (control != null)
                return control;

            Grid grid = new Grid();
            System.Windows.Controls.Canvas c = new System.Windows.Controls.Canvas() { ClipToBounds = true};
            grid.Children.Add(c);
            foreach (DeviceLayout deviceLayout in AllLayouts)
            {
                Grid deviceGrid = deviceLayout.VirtualGroup.VirtualLayout;
                c.Children.Add(deviceGrid);

                System.Windows.Controls.Canvas.SetTop(deviceGrid, deviceLayout.Location.Y);
                System.Windows.Controls.Canvas.SetLeft(deviceGrid, deviceLayout.Location.X);
                //grid.Children.Add(deviceGrid);
            }
            c.UpdateLayout();
            grid.DataContext = this;
            Binding bind = new Binding("Width");
            bind.Source = this;
            bind.Mode = BindingMode.OneWay;
            bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(grid, Grid.WidthProperty, bind); 


            bind = new Binding("Height");
            bind.Source = this;
            bind.Mode = BindingMode.OneWay;
            bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(grid, Grid.HeightProperty, bind);

            //grid.IsHitTestVisible = false;

            return (control = grid);
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
