using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace Aurora.Devices.Arduino
{
    public class ArduinoDevice : Device
    {
        private string devicename = "Arduino";

        private SerialPort port;
        private bool isConnected;

        private Color device_color;


        private VariableRegistry default_registry = null;

        public string GetDeviceDetails()
        {
            return devicename + (isConnected ? ": Connected" : ": Not connected");
        }

        public string GetDeviceName()
        {
            return devicename;
        }

        public bool Initialize()
        {
            if (!isConnected)
            {
                try
                {
                    Reset();
                }
                catch (Exception exc)
                {
                    Global.logger.Error($"Device {devicename} encountered an error during Connecting. Exception: {exc}");
                    isConnected = false;
                    return false;
                }
            }

            return isConnected;
        }

        public bool IsConnected()
        {
            try { return port.IsOpen; }
            catch { return false; }
        }

        public bool IsInitialized()
        {
            try { return port.IsOpen; }
            catch { return false; }
        }

        public bool IsKeyboardConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsPeripheralConnected()
        {
            throw new NotImplementedException();
        }

        public bool Reconnect()
        {
            Reset();
            return isConnected;
        }

        public void Reset()
        {if (port != null)
            {
                if (port.IsOpen)
                {
                    port.Close();
                    isConnected = false;
                }
            }

            String arduino = AutodetectArduinoPort();
            if (arduino != null)
            {
                port = new SerialPort(arduino, 115200, Parity.None, 8, StopBits.One);
                port.Open();
                isConnected = port.IsOpen;
            }
            device_color = new Color();
            device_color = Color.FromArgb(0, 0, 0);
            device_color = Color.FromArgb(0, 0, 0);

        }

        public void Shutdown()
        {
            if (port.IsOpen)
            {
                port.Close();
                isConnected = false;
            }
        }


        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);
            return update_result;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                //Gather colors
                Color newColor = new Color();
                DeviceKeys deviceKey = default_registry.GetVariable <DeviceKeys> ($"{devicename}_devicekey");

                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    //Iterate over each key and color and prepare to send them to the device
                    if (key.Key == deviceKey)
                    {
                        newColor = key.Value;
                    }
                }

                System.Threading.Tasks.Task.Run(() => SendColorToDevice(newColor));

                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public string GetDeviceUpdatePerformance()
        {
            return ("");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            if (default_registry == null)
            {
                default_registry = new VariableRegistry();
                default_registry.Register($"{devicename}_devicekey", DeviceKeys.ESC, "Key to Use", DeviceKeys.MOUSEPADLIGHT15, DeviceKeys.Peripheral);
            }

            return default_registry;
        }

        private void SendColorToDevice(Color color)
        {
            if (device_color != color)
            {
                string data = color.R + "," + color.G + "," + color.B + "\n";
                port.Write(data);
                device_color = color;
            }
        }

        private string AutodetectArduinoPort()
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

            try
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();

                    if (desc.Contains("Arduino"))
                    {
                        return deviceId;
                    }
                }
            }
            catch (ManagementException e)
            {
                /* Do Nothing */
            }

            return null;
        }
    }
}
