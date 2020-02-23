using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AuraServiceLib;
using System.IO;
using Newtonsoft.Json;

namespace Aurora.Devices.Asus.Config
{
    /// <summary>
    /// Interaction logic for AsusConfigWindow.xaml
    /// </summary>
    public partial class AsusConfigWindow : Window
    {
        private AsusHandler asusHandler;
        private readonly List<IAuraSyncDevice> devices = new List<IAuraSyncDevice>();
        private readonly List<AsusKeyToDeviceKeyControl> keys = new List<AsusKeyToDeviceKeyControl>();
        private int selectedDevice = 0;

        private AsusConfig config;
        
        public AsusConfigWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!LoadAuraSdk())
                return;

            LoadConfigFile();
            LoadDevices();
        }

        private void LoadConfigFile()
        {
            config = AsusConfig.LoadConfig();
        }
        
        private void SaveConfigFile()
        {
            AsusConfig.SaveConfig(config);
            AsusSyncConfiguredDevice.UpdateConfig(config);
        }

        private bool LoadAuraSdk()
        {
            asusHandler = new AsusHandler();

            if (asusHandler.HasSdk)
                return true;

            MessageBox.Show("Can't detect AuraSDK", "Error", MessageBoxButton.OK);
            Close();
            return false;
        }

        private void ReloadDevices()
        {
            // refresh the aura sdk, I think it is internally cached
            if (!LoadAuraSdk())
                return;

            LoadDevices();
        }

        private void LoadDevices()
        {
            // clear current devices
            AsusDeviceList.Children.Clear();
            devices.Clear();

            foreach (IAuraSyncDevice auraDevice in asusHandler.AuraSdk.Enumerate((uint)AsusHandler.AsusDeviceType.All))
            {
                // ignore keyboards and mice, we already handle that
                if (auraDevice.Type == (uint)AsusHandler.AsusDeviceType.Keyboard || auraDevice.Type == (uint)AsusHandler.AsusDeviceType.Mouse)
                    continue;

                devices.Add(auraDevice);
                
                // create a new button for the ui
                var button = new Button();
                button.Content = $"[{(AsusHandler.AsusDeviceType) auraDevice.Type}] {auraDevice.Name}";
                int index = devices.Count - 1;
                SaveDevice();
                button.Click += (_,__) => DeviceSelect(index);

                AsusDeviceList.Children.Add(button);
            }

            if (devices.Count > 0)
                DeviceSelect(0);
        }

        private void DeviceSelect(int index)
        {
            selectedDevice = index;
            keys.Clear();

            // Rebuild the key area
            AsusDeviceKeys.Children.Clear();
            var device = devices[index];

            AsusConfig.AsusConfigDevice? configDevice = null;
            if (GetAsusConfigDevice(out var cDevice) >= 0)
                configDevice = cDevice;
            
            
            for (int i = 0; i < device.Lights.Count; i++)
            {
                var keyControl = new AsusKeyToDeviceKeyControl();
                keyControl.KeyIdValue.Text = i.ToString();
                if (configDevice.HasValue && configDevice.Value.KeyMapper.TryGetValue(i, out var deviceKey))
                    keyControl.DeviceKey.SelectedValue = deviceKey;
                
                keys.Add(keyControl);

                AsusDeviceKeys.Children.Add(keyControl);
            }
        }

        private void SaveDevice()
        {
            var index = GetAsusConfigDevice(out var device);

            foreach (var key in keys)
            {
                if (!int.TryParse(key.KeyIdValue.Text, out int keyIndex))
                    continue;
                
                if (key.DeviceKey.SelectedValue == null)
                    continue;
                
                device.KeyMapper[keyIndex] = (DeviceKeys)key.DeviceKey.SelectedValue;
            }

            if (index < 0)
                config.Devices.Add(device);
            else
                config.Devices[index] = device;
        }
        
        private void SaveToFile()
        {
            SaveDevice();
            SaveConfigFile();
        }

        /// <summary>
        /// Returns the index and the device, -1 if not found
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private int GetAsusConfigDevice(out AsusConfig.AsusConfigDevice device)
        {
            var currentDevice = devices[selectedDevice];
            device = new AsusConfig.AsusConfigDevice(currentDevice);
            
            for (var i = 0; i < config.Devices.Count; i++)
            {
                var savedDevice = config.Devices[i];
                if (Equals(device, savedDevice))
                {
                    device = savedDevice;
                    return i;
                }
            }

            return -1;
        }

        #region UI


        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadDevices();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile();
        }
        #endregion
    }
}
