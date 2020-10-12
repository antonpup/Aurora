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
using IronPython.Runtime;

namespace Aurora.Devices.RGBFusion
{
    public class RGBFusionDevice : IDevice
    {
        private string _devicename = "RGB Fusion";
        private bool _isConnected = false;
        private long _lastUpdateTime = 0;
        private Stopwatch _ellapsedTimeWatch = new Stopwatch();
        private VariableRegistry _variableRegistry = null;
        private DeviceKeys _commitKey;
        private string _RGBFusionDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\GIGABYTE\\RGBFusion\\";
        private List<DeviceMapState> _deviceMap;
        private Color _initialColor = Color.Black;
        private string _ignoreLedsParam = string.Empty;
        private string _customArgs = string.Empty;
        private byte[] _setColorCommandDataPacket = new byte[1024];
        private List<string> _RGBFusionBridgeFiles = new List<string>()
        {
            {"RGBFusionAuroraListener.exe"},
            {"RGBFusionBridge.dll"}
        };
        private const string _RGBFusionExeName = "RGBFusion.exe";
        private const string _RGBFusionBridgeExeName = "RGBFusionAuroraListener.exe";
        private const string _defaultProfileFileName = "pro1.xml";
        private const string _defaultExtProfileFileName = "ExtPro1.xml";
        private int _connectRetryCountLeft = _maxConnectRetryCountLeft;
        private bool _starting = false;
        private const int _maxConnectRetryCountLeft = 10;
        private const int _ConnectRetryTimeOut = 100;

        private HashSet<byte> _rgbFusionLedIndexes;

        public bool Initialize()
        {
            _starting = true;
            _connectRetryCountLeft = _maxConnectRetryCountLeft;
            try
            {
                if (!TestRGBFusionBridgeListener(1))
                    Shutdown();

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
                    catch (Exception ex)
                    {
                        Global.logger.Error("RGBFusion Bridge cannot be initialized. Error: " + ex.Message);
                        return false;
                    }
                    return false;
                }

                //Start RGBFusion Bridge
                Global.logger.Info("Starting RGBFusion Bridge.");
                if (!StartListenerForDevice())
                {
                    _isConnected = false;
                    _starting = false;
                    return false;
                }

                Global.logger.Info("RGBFusion bridge is listening");
                //If device is restarted, re-send last color command.
                if (_setColorCommandDataPacket[0] != 0)
                {
                    SendCommandToRGBFusion(_setColorCommandDataPacket);
                }

                UpdateDeviceMap();
                _isConnected = true;
                _starting = false;
                _connectRetryCountLeft = _maxConnectRetryCountLeft;
                return true;
            }
            catch (Exception ex)
            {
                Global.logger.Error("RGBFusion Bridge cannot be initialized. Error: " + ex.Message);
                _isConnected = false;
                _starting = false;
                return false;
            }
            finally
            {
                _starting = false;
            }
        }

        private bool StartListenerForDevice()
        {
            try
            {
                _starting = true;
                //GetRegisteredVariables();
                string pStart = _RGBFusionDirectory + _RGBFusionBridgeExeName;
                string pArgs = _customArgs + " " + (ValidateIgnoreLedParam() ? "--ignoreled:" + _ignoreLedsParam : "");
                Process.Start(pStart, pArgs);
                bool state;
                if (!TestRGBFusionBridgeListener(60))
                {
                    Global.logger.Error("RGBFusion bridge listener didn't start on " + _RGBFusionDirectory + _RGBFusionBridgeExeName);
                    _starting = false;
                    return false;
                }
                else
                {
                    _starting = false;
                    return true;
                }
            }
            catch
            {
                _starting = false;
                return false;
            }
            finally
            {
                _starting = false;
            }
        }

        public void KillProcessByName(string processName)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = Environment.SystemDirectory + @"\taskkill.exe";
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.Arguments = string.Format(@"/f /im {0}", processName);
            cmd.Start();
            cmd.WaitForExit();
            cmd.Dispose();
        }

        public bool SendCommandToRGBFusion(byte[] args)
        {
            try
            {
                using (var pipe = new NamedPipeClientStream(".", "RGBFusionAuroraListener", PipeDirection.Out))
                using (var stream = new BinaryWriter(pipe))
                {
                    pipe.Connect(100);
                    stream.Write(args);
                    return true;
                }
            }
            catch
            {
                Thread.Sleep(100);
                return false;
            }
        }

        public void Reset()
        {
            if (_starting)
                return;

            if (IsRGBFusionBridgeRunning())
            {
                KillProcessByName(_RGBFusionBridgeExeName);
            }
            StartListenerForDevice();
        }

        public void Shutdown()
        {
            if (IsRGBFusionBridgeRunning())
            {
                SendCommandToRGBFusion(new byte[] { 1, 5, 0, 0, 0, 0, 0 });
                Thread.Sleep(1000);
                KillProcessByName(_RGBFusionBridgeExeName);
            }
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

            _deviceMap = new List<DeviceMapState>();

            _deviceMap.Clear();
            foreach (byte ledIndex in _rgbFusionLedIndexes)
            {
                _deviceMap.Add(new DeviceMapState(ledIndex, _initialColor, Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{_devicename}_area_" + ledIndex.ToString()))); // Led 255 is equal to set all areas at the same time.
            }

            _commitKey = _deviceMap.Max(k => k.deviceKey);
        }

        bool _deviceChanged = true;

        public VariableRegistry RegisteredVariables
        {
            get
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
                    _variableRegistry.Register($"{_devicename}_custom_args", "", "Custom command line arguments", null, null, "Just for advanced users.");
                    foreach (byte ledIndex in _rgbFusionLedIndexes)
                    {
                        _variableRegistry.Register($"{_devicename}_area_" + ledIndex.ToString(), DeviceKeys.ESC, "Key to Use for area index " + ledIndex.ToString(), devKeysEnumAsEnumerable.Max(), devKeysEnumAsEnumerable.Min(), "Require Aurora restart.");
                    }
                }
                _ignoreLedsParam = Global.Configuration.VarRegistry.GetVariable<string>($"{_devicename}_ignore_leds");
                _customArgs = Global.Configuration.VarRegistry.GetVariable<string>($"{_devicename}_custom_args");
                return _variableRegistry;
            }
        }

        private bool ValidateIgnoreLedParam()
        {
            if (_ignoreLedsParam == null)
                return false;

            string[] ignoreLedsParam = _ignoreLedsParam.Split(',');

            foreach (string s in ignoreLedsParam)
            {
                if (!byte.TryParse(s, out _))
                {
                    Global.logger.Warn("RGBFusion Bridge --ignoreled bad param {0}. Running Bridge in default mode.", s);
                    return false;
                }
            }
            return true;
        }

        public string DeviceName => _devicename;

        public string DeviceDetails => _devicename + (_isConnected ? ": Connected" : ": Not connected");

        public string DeviceUpdatePerformance => (IsConnected() ? _lastUpdateTime + " ms" : "");

        public bool Reconnect()
        {
            Shutdown();
            return Initialize();
        }

        public bool IsInitialized => IsConnected();

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
            if (_starting)
            {
                Global.logger.Warn("RGBFusion Bridge starting. Ignoring command.");
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
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 1] = 1;
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 2] = 10;
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 3] = Convert.ToByte(key.Value.R * key.Value.A / 255.0f);
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 4] = Convert.ToByte(key.Value.G * key.Value.A / 255.0f);
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 5] = Convert.ToByte(key.Value.B * key.Value.A / 255.0f);
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 6] = Convert.ToByte(_deviceMap[d].led);

                            if (key.Value != _deviceMap[d].color)
                            {
                                // set deviceChanged flag if at least one led changed.
                                _deviceMap[d] = new DeviceMapState(_deviceMap[d].led, key.Value, _deviceMap[d].deviceKey);
                                _deviceChanged = true;
                            }
                        }
                    }

                    if (key.Key == _commitKey)
                    {
                        // Send changes to device only if device actually changed.
                        if (_deviceChanged)
                        {
                            commandIndex++;
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 1] = 2;
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 2] = 0;
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 3] = 0;
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 4] = 0;
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 5] = 0;
                            _setColorCommandDataPacket[(commandIndex - 1) * 6 + 6] = 0;
                            _setColorCommandDataPacket[0] = commandIndex;
                            SendCommandToRGBFusion(_setColorCommandDataPacket);
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
            catch (Exception ex)
            {
                _setColorCommandDataPacket[0] = 0; //Invalidate bad command
                Global.logger.Warn(string.Format("RGBFusion device error while updatind device. Error: {0}", ex.Message));
                return false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            if (_starting)
                return false;

            _ellapsedTimeWatch.Restart();
            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);
            _ellapsedTimeWatch.Stop();
            _lastUpdateTime = _ellapsedTimeWatch.ElapsedMilliseconds;

            if (_lastUpdateTime > _ConnectRetryTimeOut)
            {
                _connectRetryCountLeft--;
                Global.logger.Warn(string.Format("{0} device reseted automatically.", _devicename));
            }
            else if (_lastUpdateTime < _ConnectRetryTimeOut)
            {
                _connectRetryCountLeft = _maxConnectRetryCountLeft;
            }

            if (_connectRetryCountLeft <= 0 && _isConnected)
            {
                Reset();
                Global.logger.Warn(string.Format("{0} device reseted automatically.", _devicename));
            }
            return update_result;
        }

        #region RGBFusion Specific Methods

        private HashSet<byte> GetLedIndexes()
        {
            HashSet<byte> rgbFusionLedIndexes = new HashSet<byte>();

            string mainProfileFilePath = _RGBFusionDirectory + _defaultProfileFileName;
            if (!IsRGBFusinMainProfileCreated())
            {
                Global.logger.Error(string.Format("Main profile file not found at {0}. Launch RGBFusion at least one time.", mainProfileFilePath));
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
                Global.logger.Error(string.Format("Main external devices profile file not found at {0}. Launch RGBFusion at least one time.", mainProfileFilePath));
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

        private bool IsRGBFusionBridgeRunning()
        {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_RGBFusionBridgeExeName)).Length > 0;
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
            foreach (string file in _RGBFusionBridgeFiles)
            {
                if (!File.Exists(_RGBFusionDirectory + file))
                {
                    Global.logger.Warn(String.Format("File {0} not installed.", file));
                    error = true;
                }
                else if (CalculateMD5(_RGBFusionDirectory + file).ToLower() != CalculateMD5("RGBFusionBridge\\" + file).ToLower())
                {
                    Global.logger.Warn(String.Format("File {0} MD5 incorrect.", file));
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

        private bool TestRGBFusionBridgeListener(byte secondsTimeOut)
        {
            bool result = false;
            for (int i = 0; i < secondsTimeOut; i++)
            {
                if (SendCommandToRGBFusion(new byte[] { 1, 255, 0, 0, 0, 0, 0 }))
                    return true;
                if (!IsRGBFusionBridgeRunning())
                    return false;
                //Test listener every 1000ms until pipe is up or timeout
                Thread.Sleep(1000);
            }
            return result;
        }

        private bool InstallRGBFusionBridge()
        {
            Shutdown();
            foreach (string fileName in _RGBFusionBridgeFiles)
            {
                try
                {
                    File.Copy(AppDomain.CurrentDomain.BaseDirectory + "RGBFusionBridge\\" + fileName, _RGBFusionDirectory + fileName, true);
                    Global.logger.Info(String.Format("RGBFusion file {0} install  OK.", fileName));
                }
                catch (Exception ex)
                {
                    Global.logger.Error(String.Format("RGBFusion file {0} install error: {1}", fileName, ex.Message));
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}