using Aurora.Settings;
using Aurora.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LEDINT = System.Int16;

namespace Aurora.Devices.Layout
{

    /// <summary>
    /// Struct representing color settings being sent to devices
    /// </summary>
    public class DeviceColorComposition
    {
        public readonly object bitmapLock = new object();
        public Dictionary<LEDINT, System.Drawing.Color> deviceColours;
        public Bitmap keyBitmap;
    }

    public struct DynamicDeviceLED
    {
        public LEDINT LedID { get; private set; }
        public DeviceLayout Layout { get; private set; }

        public bool IsNone => LedID == -1;

        public DynamicDeviceLED(LEDINT ledID, DeviceLayout layout)
        {
            LedID = ledID;
            Layout = layout;
        }

        public DeviceLED GetDeviceLED()
        {
            return new DeviceLED(Layout.GetDeviceTypeID, Layout.DeviceID, LedID);
        }
    }



    public abstract class DeviceLayout : Settings.SettingsBase, IDisposable
    {
        #region settings
        private System.Drawing.Point location = new System.Drawing.Point(0,0);
        //[JsonIgnore]
        public System.Drawing.Point Location { get { return location; } set { UpdateVar(ref location, value); } }

        private short selectedSDK = -1;
        //[JsonIgnore]
        public short SelectedSDK { get { return selectedSDK; } set { UpdateVar(ref selectedSDK, value); } }

        private bool enabled = true;
        //[JsonIgnore]
        public bool Enabled { get { return enabled; } set { UpdateVar(ref enabled, value); } }
        #endregion

        //Needs to be changed in child classes by redefining with new keyword
        [JsonIgnore]
        public static readonly byte DeviceTypeID = 255;

        [JsonIgnore]
        public virtual byte GetDeviceTypeID { get { return DeviceTypeID; } }

        [JsonIgnore]
        public byte DeviceID { get; set; }

        [JsonIgnore]
        protected VirtualGroup virtualGroup;

        [JsonIgnore]
        public VirtualGroup VirtualGroup
        {
            get
            {
                return virtualGroup;
            }
        }

        [JsonIgnore]
        public DeviceColorComposition DeviceColours { get; set; }

        public delegate void LayoutUpdatedEventHandler(object sender);
        public event LayoutUpdatedEventHandler LayoutUpdated;

        public DeviceLayout()
        {

        }

        public static bool LayoutsLoaded = false;
        public void LoadLayouts(bool force = false)
        {
            if (LayoutsLoaded && !force)
                return;
            LayoutsLoaded = true;
            loadLayouts();
        }

        protected abstract void loadLayouts();


        //Create VirtualGroup and VirtualLayout
        public void Initialize()
        {
            LoadLayouts();
            GenerateLayout();
        }

        public abstract void GenerateLayout();

        public abstract string GetLEDName(LEDINT ledID);

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            //Do we want to do this here?
            Initialize();
        }

        protected void LayoutChanged()
        {
            LayoutUpdated?.Invoke(this);
        }

        //Shit broke
        /*public static DeviceLED GetGenericDeviceLED(LEDINT ledID)
        {
            //Returns it for the first device of this type
            return new DeviceLED(DeviceTypeID, 0, ledID);
        }*/

        public DeviceLED GetDeviceLED(LEDINT ledID)
        {
            //Returns it for the first device of this type
            return new DeviceLED(DeviceTypeID, this.DeviceID, ledID);
        }

        public virtual LEDINT Sanitize(LEDINT ledID)
        {
            return ledID;
        }

        public List<DeviceLED> GetAllDeviceLEDs()
        {
            return this.virtualGroup.grouped_keys.ConvertAll(s => GetDeviceLED(s.tag));
        }

        internal void UpdateColors(Bitmap colormap)
        {
            Dictionary<LEDINT, System.Drawing.Color> colors = new Dictionary<LEDINT, System.Drawing.Color>();

            foreach (KeyValuePair<LEDINT, BitmapRectangle> keyRegion in this.VirtualGroup.BitmapMap)
            {
                colors.Add(keyRegion.Key, BitmapUtils.GetRegionColor(colormap, keyRegion.Value));
            }

            this.DeviceColours = new DeviceColorComposition()
            {
                deviceColours = colors,
                keyBitmap = colormap
            };
        }

        public virtual void Dispose()
        {

        }

        internal void Moved(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                this.DeviceID = (byte)((ObservableCollection<DeviceLayout>)sender).IndexOf(this);
        }
    }
}
