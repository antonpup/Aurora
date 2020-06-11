using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AuraServiceLib;

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
        private bool wasEnabled = false;
        private AsusDevice asusDevice;

        private static bool windowOpen = false;
        
        public AsusConfigWindow()
        {
            if (windowOpen)
            {
                Close();
                return;
            }
            
            windowOpen = true;
            InitializeComponent();
            Loaded += OnLoaded;
            Closed += OnClosed;

            asusDevice = Global.dev_manager.Devices.FirstOrDefault(device => device.Device is AsusDevice)?.Device as AsusDevice;

            if (asusDevice != null && asusDevice.IsInitialized())
            {
                wasEnabled = true;
                asusDevice.Shutdown();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!LoadAuraSdk())
                return;

            LoadConfigFile();
            LoadDevices();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            windowOpen = false;
            if (asusDevice != null && wasEnabled)
                asusDevice.Initialize();
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
            asusHandler = new AsusHandler(Global.Configuration.VarRegistry.GetVariable<bool>($"Asus_enable_unsupported_version"));
            asusHandler.AuraSdk?.SwitchMode();

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

            for (int i = 0; i < AsusDeviceList.Children.Count; i++)
            {
                if (AsusDeviceList.Children[i] is Button button) 
                    button.IsEnabled = i != index;
            }

            // Rebuild the key area
            AsusDeviceKeys.Children.Clear();
            var device = devices[index];

            AsusConfig.AsusConfigDevice? configDevice = null;
            if (GetAsusConfigDevice(out var cDevice) >= 0)
                configDevice = cDevice;

            DeviceEnabledCheckBox.IsChecked = configDevice?.Enabled ?? false;
            for (int i = 0; i < device.Lights.Count; i++)
            {
                var lightIndex = i;
                var keyControl = new AsusKeyToDeviceKeyControl();
                
                keyControl.BlinkCallback += () => BlinkKey(lightIndex);
                keyControl.KeyIdValue.Text = i.ToString();
                if (configDevice.HasValue && configDevice.Value.KeyMapper.TryGetValue(i, out var deviceKey))
                    keyControl.DeviceKey.SelectedValue = deviceKey;
                else
                    keyControl.DeviceKey.SelectedValue = DeviceKeys.NONE;
                
                keys.Add(keyControl);
                
                AsusDeviceKeys.Children.Add(keyControl);
            }
        }

        private void BlinkKey(int lightIndex)
        {
            BlinkLight(devices[selectedDevice], lightIndex);
        }
        
        private CancellationTokenSource tokenSource;
        private const int BlinkCount = 7;
        public async void BlinkLight(IAuraSyncDevice device, int lightId)
        {
            if (tokenSource != null && !tokenSource.Token.IsCancellationRequested)
                tokenSource.Cancel();
            
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            try
            {
                for (int i = 0; i < BlinkCount; i++)
                {
                    if (token.IsCancellationRequested || device == null)
                        return;

                    // set everything to black
                    foreach (IAuraRgbLight light in device.Lights)
                        light.Color = 0;

                    // set this one key to white
                    if (i % 2 == 1)
                    {
                        device.Lights[lightId].Red = 255;
                        device.Lights[lightId].Green = 255;
                        device.Lights[lightId].Blue = 255;
                    }

                    device.Apply();
                    await Task.Delay(200, token); // ms
                }
            }
            catch { }
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
            
            device.Enabled = DeviceEnabledCheckBox.IsChecked ?? false;
            
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
        
        private void SetAllNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var key in keys)
                key.DeviceKey.SelectedValue = DeviceKeys.NONE;
        }
        
        private void SetAllLogo_Click(object sender, RoutedEventArgs e)
        {
            foreach (var key in keys)
                key.DeviceKey.SelectedValue = DeviceKeys.Peripheral_Logo;
        }
        #endregion
    }
}
