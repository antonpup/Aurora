using Aurora;
using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using CSScriptLibrary;
using Aurora.Settings;
using System.ComponentModel;
using Aurora.Utils;
using Mono.CSharp;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using System.Security.Cryptography;

namespace Aurora.Devices.RGBFusion
{
    public class RGBFusionDevice : Device
    {
        private string _devicename = "RGB Fusion";
        private bool _isConnected;
        private long _lastUpdateTime = 0;
        private Stopwatch _ellapsedTimeWatch = new Stopwatch();
        private VariableRegistry _variableRegistry = null;

        private string _RGBFusionDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\GIGABYTE\\RGBFusion\\";
        private string _RGBFusionExeName = "RGBFusion.exe";
        private string _RGBFusionBridgeExeName = "RGBFusionAuroraListener.exe";
        private List<DeviceMapState> _deviceMap;
        private Color _initialColor = Color.FromArgb(0, 0, 0);
        private string _defaultProfileFileName = "pro1.xml";
        private string _defaultExtProfileFileName = "ExtPro1.xml";
        private string _ignoreLedsParam = string.Empty;
        private byte[] _commandDataPacket = new byte[1024];
        private Dictionary<string, string> _RGBFusionBridgeFiles = new Dictionary<string, string>()
        {
            {"RGBFusionAuroraListener.exe","204503BFB993CE02ED495E190D16F757"},
            {"RGBFusionBridge.dll","DAC4AA9C104D8132D9284F15A0BDABBF"}
        };

        private HashSet<byte> _rgbFusionLedIndexes;

        public bool Initialize()
        {
            try
            {
                try
                {
                    Shutdown();
                }
                catch { }

                if (!IsRGBFusionInstalled())
                {
                    Global.logger.Error("RGBFusion is not installed.");
                    return false;
                }
                if (IsRGBFusionRunning())
                {
                    Global.logger.Error("RGBFusion should be closed before run RGBFusion Bridge.");
                    return false;
                }
                if (!IsRGBFusinMainProfileCreated())
                {
                    Global.logger.Error("RGBFusion main profile file is not created. Run RGBFusion for at least one time.");
                    return false;
                }
                if (!IsRGBFusionBridgeInstalled())
                {
                    Global.logger.Warn("RGBFusion Bridge is not installed. Installing. Installing.");
                    try
                    {
                        InstallRGBFusionBridge();
                    }
                    catch
                    {
                        Global.logger.Error("An error has occurred  while installing RGBFusion Bridge.");
                        return false;
                    }
                    return false;
                }

                //Start RGBFusion Bridge
                Global.logger.Info("Starting RGBFusion Bridge.");
                Process.Start(_RGBFusionDirectory + _RGBFusionBridgeExeName, ValidateIgnoreLedParam() ? "--ignoreled:" + _ignoreLedsParam : "");
                _isConnected = true;
                return true;
            }
            catch
            {
                Global.logger.Error("RGBFusion Bridge cannot be initialized.");
                _isConnected = false;
                return false;
            }
        }

        public void KillProcessByName(string processName)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = @"C:\Windows\System32\taskkill.exe";
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.Arguments = string.Format(@"/f /im {0}", processName);
            cmd.Start();
            cmd.Dispose();
        }

        public void SendCommandToRGBFusion(byte[] args)
        {
            using (var pipe = new NamedPipeClientStream(".", "RGBFusionAuroraListener", PipeDirection.Out))
            using (var stream = new BinaryWriter(pipe))
            {
                pipe.Connect(100);
                stream.Write(args);
            }
        }

        public void Reset()
        {
            Shutdown();
            Initialize();
        }

        public void Shutdown()
        {
            try
            {
                SendCommandToRGBFusion(new byte[] { 1, 5, 0, 0, 0, 0, 0 }); // Operatin code 5 set all leds to black and close the listener application.
            }
            catch
            {
                //Just in case Bridge is not responding or already closed
            }

            Thread.Sleep(1000); // Time to shutdown leds and close listener application.
            KillProcessByName("RGBFusionAuroraListener");
            _isConnected = false;
        }

        private struct DeviceMapState
        {
            public byte led;
            public Color color;
            public DeviceKeys deviceKey;
            public DeviceMapState(byte led, Color color, DeviceKeys deviceKeys)
            {
                this.led = led;
                this.color = color;
                this.deviceKey = deviceKeys;
            }
        }

        private void UpdateDeviceMap()
        {
            if (_deviceMap == null)
            {
                _deviceMap = new List<DeviceMapState>();

                _deviceMap.Clear();
                foreach (byte ledIndex in _rgbFusionLedIndexes)
                {
                    _deviceMap.Add(new DeviceMapState(ledIndex, _initialColor, Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{_devicename}_area_" + ledIndex.ToString()))); // Led 255 is equal to set all areas at the same time.
                }
            }
        }

        bool _deviceChanged = true;

        public VariableRegistry GetRegisteredVariables()
        {
            if (_rgbFusionLedIndexes == null)
            {
                _rgbFusionLedIndexes = GetLedIndexes();
            }

            if (_variableRegistry == null)
            {
                var devKeysEnumAsEnumerable = System.Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();
                _variableRegistry = new VariableRegistry();
                _variableRegistry.Register($"{_devicename}_ignore_leds", "", "Area index to be ignored by RGBFusion Bridge", null, null, "Comma separated. Require Aurora restart.");
                foreach (byte ledIndex in _rgbFusionLedIndexes)
                {
                    _variableRegistry.Register($"{_devicename}_area_" + ledIndex.ToString(), DeviceKeys.ESC, "Key to Use for area index " + ledIndex.ToString(), devKeysEnumAsEnumerable.Max(), devKeysEnumAsEnumerable.Min(), "Require Aurora restart.");
                }
            }
            _ignoreLedsParam = Global.Configuration.VarRegistry.GetVariable<string>($"{_devicename}_ignore_leds");
            return _variableRegistry;
        }

        private bool ValidateIgnoreLedParam()
        {
            string[] ignoreLedsParam = _ignoreLedsParam.Split(',');

            foreach (string s in ignoreLedsParam)
            {
                if (!byte.TryParse(s, out _))
                {
                    Global.logger.Error("RGBFusion Bridge --ignoreled bad param {0}. Running Bridge in default mode.", s);
                    return false;
                }
            }
            return true;
        }

        public string GetDeviceName()
        {
            return _devicename;
        }

        public string GetDeviceDetails()
        {
            return _devicename + (_isConnected ? ": Connected" : ": Not connected");
        }

        public string GetDeviceUpdatePerformance()
        {
            return (IsConnected() ? _lastUpdateTime + " ms" : "");
        }

        public bool Reconnect()
        {
            Shutdown();
            return Initialize();
        }

        public bool IsInitialized()
        {
            return IsConnected();
        }

        public bool IsConnected()
        {
            return _isConnected;
        }

        public bool IsKeyboardConnected()
        {
            return false;
        }

        public bool IsPeripheralConnected()
        {
            return true;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel)
            {
                return false;
            }
            byte commandIndex = 0;

            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
                {
                    for (byte d = 0; d < _deviceMap.Count; d++)
                    {
                        if ((_deviceMap[d].deviceKey == key.Key) && (key.Value != _deviceMap[d].color))
                        {
                            commandIndex++;
                            _commandDataPacket[(commandIndex - 1) * 6 + 1] = 1;
                            _commandDataPacket[(commandIndex - 1) * 6 + 2] = 10;
                            _commandDataPacket[(commandIndex - 1) * 6 + 3] = Convert.ToByte(key.Value.R * key.Value.A / 255);
                            _commandDataPacket[(commandIndex - 1) * 6 + 4] = Convert.ToByte(key.Value.G * key.Value.A / 255);
                            _commandDataPacket[(commandIndex - 1) * 6 + 5] = Convert.ToByte(key.Value.B * key.Value.A / 255);
                            _commandDataPacket[(commandIndex - 1) * 6 + 6] = Convert.ToByte(_deviceMap[d].led);

                            if (key.Value != _deviceMap[d].color)
                            {
                                //If at least one led change, set deviceChanged flag
                                _deviceMap[d] = new DeviceMapState(_deviceMap[d].led, key.Value, _deviceMap[d].deviceKey);
                                _deviceChanged = true;
                            }
                        }
                    }

                    if (key.Key == _deviceMap.Max(k => k.deviceKey))
                    {
                        // Send changes to device only if device actually changed.
                        if (_deviceChanged)
                        {
                            commandIndex++;
                            _commandDataPacket[(commandIndex - 1) * 6 + 1] = 2;
                            _commandDataPacket[(commandIndex - 1) * 6 + 2] = 0;
                            _commandDataPacket[(commandIndex - 1) * 6 + 3] = 0;
                            _commandDataPacket[(commandIndex - 1) * 6 + 4] = 0;
                            _commandDataPacket[(commandIndex - 1) * 6 + 5] = 0;
                            _commandDataPacket[(commandIndex - 1) * 6 + 6] = 0;
                            _commandDataPacket[0] = commandIndex;
                            SendCommandToRGBFusion(_commandDataPacket);
                        }
                        commandIndex = 0;
                        _deviceChanged = false;
                    }
                }
                if (e.Cancel)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private HashSet<byte> GetLedIndexes()
        {
            HashSet<byte> rgbFusionLedIndexes = new HashSet<byte>();

            string mainProfileFilePath = _RGBFusionDirectory + _defaultProfileFileName;
            if (!IsRGBFusinMainProfileCreated())
            {
                Global.logger.Warn(string.Format("Main profile file not found at {0}. Launch RGBFusion at least one time.", mainProfileFilePath));
            }
            else
            {

                XmlDocument mainProfileXml = new XmlDocument();
                mainProfileXml.Load(mainProfileFilePath);
                XmlNode ledInfoNode = mainProfileXml.DocumentElement.SelectSingleNode("/LED_info");
                foreach (XmlNode node in ledInfoNode.ChildNodes)
                {
                    rgbFusionLedIndexes.Add(Convert.ToByte(node.Attributes["Area_index"]?.InnerText));
                }
            }
            string extMainProfileFilePath = _RGBFusionDirectory + _defaultExtProfileFileName;
            if (!IsRGBFusinMainExtProfileCreated())
            {
                Global.logger.Warn(string.Format("Main external devices profile file not found at {0}. Launch RGBFusion at least one time.", mainProfileFilePath));
            }
            else
            {

                XmlDocument extMainProfileXml = new XmlDocument();
                extMainProfileXml.Load(extMainProfileFilePath);
                XmlNode extLedInfoNode = extMainProfileXml.DocumentElement.SelectSingleNode("/LED_info");
                foreach (XmlNode node in extLedInfoNode.ChildNodes)
                {
                    rgbFusionLedIndexes.Add(Convert.ToByte(node.Attributes["Area_index"]?.InnerText));
                }
            }
            return rgbFusionLedIndexes;
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            UpdateDeviceMap(); //Is ther any way to know when a config in VariableRegistry change to avoid do this task every time?
            _ellapsedTimeWatch.Restart();
            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);
            _ellapsedTimeWatch.Stop();
            _lastUpdateTime = _ellapsedTimeWatch.ElapsedMilliseconds;
            return update_result;
        }
        #region RGBFusion Specific Methods
        private bool IsRGBFusionInstalled()
        {
            string RGBFusionDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\GIGABYTE\\RGBFusion\\";
            bool result = (Directory.Exists(RGBFusionDirectory));
            return result;
        }

        private bool IsRGBFusionRunning()
        {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_RGBFusionExeName)).Length > 0;
        }

        private bool IsRGBFusinMainProfileCreated()
        {

            string defaulprofileFullpath = _RGBFusionDirectory + _defaultProfileFileName;
            bool result = (File.Exists(defaulprofileFullpath));
            return result;
        }

        private bool IsRGBFusinMainExtProfileCreated()
        {

            string defaulprofileFullpath = _RGBFusionDirectory + _defaultExtProfileFileName;
            bool result = (File.Exists(defaulprofileFullpath));
            return result;
        }

        private bool IsRGBFusionBridgeInstalled()
        {
            bool error = false;
            foreach (KeyValuePair<string, string> file in _RGBFusionBridgeFiles)
            {
                if (!File.Exists(_RGBFusionDirectory + file.Key))
                {
                    Global.logger.Error(String.Format("File {0} not installed.", file.Key));
                    error = true;
                }
                else if (CalculateMD5(_RGBFusionDirectory + file.Key).ToLower() != file.Value.ToLower())
                {
                    Global.logger.Error(String.Format("File {0} MD5 incorrect.", file.Key));
                    error = true;
                }
            }
            return !error;
        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                if (!File.Exists(filename))
                    return string.Empty;
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    var md5String = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    return md5String;
                }
            }
        }

        private bool InstallRGBFusionBridge()
        {
            foreach (string fileName in _RGBFusionBridgeFiles.Keys)
            {
                try
                {
                    File.Copy("RGBFusionBridge\\" + fileName, _RGBFusionDirectory + fileName, true);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}