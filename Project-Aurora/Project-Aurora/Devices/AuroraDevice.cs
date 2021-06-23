using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Devices
{
    public enum AuroraDeviceType
    {
        Keyboard    = 0,
        Mouse       = 1,
        Unkown      = 2,
        Headset     = 3,
        OpenRGBKeyboard = 4,
        OpenRGBMouse = 5,
        OpenRGBUnkown = 6,
        OpenRGBHeadset = 7,
    }
    public class UniqueDeviceId
    {
        public string ConnectorName = "";
        public string DeviceName { get; set; } = "Generic SDK device";
        public int Index = 0;
        [JsonIgnore]
        public int? ViewPort = null;
        public UniqueDeviceId()
        {
        }
        public UniqueDeviceId(AuroraDeviceConnector connector, AuroraDevice device)
        {
            ConnectorName = connector.GetConnectorName();
            DeviceName = device.GetDeviceName();
        }
        public static bool operator ==(UniqueDeviceId obj1, UniqueDeviceId obj2)
        {
            return (!(obj1 is null) && !(obj2 is null)
                        && string.Equals(obj1.ConnectorName, obj2.ConnectorName)
                        && string.Equals(obj1.DeviceName, obj2.DeviceName)
                        && obj1.Index == obj2.Index);
        }

        public static bool operator !=(UniqueDeviceId obj1, UniqueDeviceId obj2)
        {
            return !(obj1 == obj2);
        }
        public override bool Equals(object obj)
        {
            return this == obj as UniqueDeviceId;
        }

    }
    public abstract class AuroraDevice
    {
        private readonly Stopwatch Watch = new Stopwatch();
        private long LastUpdateTime = 0;
        private bool UpdateIsOngoing = false;
        private bool DeviceIsConnected = false;

        public UniqueDeviceId id = null;
        private VariableRegistry variableRegistry;

        public event EventHandler ConnectionHandler;
        public event EventHandler UpdateFinished;
        /// <summary>
        /// Is called every frame (30fps). Update the device here
        /// </summary>

        //[HandleProcessCorruptedStateExceptions, SecurityCritical]
        public async void UpdateDevice(DeviceColorComposition composition)
        {
            if (IsConnected())
            {
                if (Global.Configuration.DevicesDisabled.Contains(GetType()))
                {
                    //Initialized when it's supposed to be disabled? SMACK IT!
                    Disconnect();
                    return;
                }

                if (!UpdateIsOngoing)
                {
                    UpdateIsOngoing = true;
                    Watch.Restart();
                    try
                    {
                        if (!await Task.Run(() => UpdateDeviceImpl(composition)))
                        {
                            LogError(DeviceName + " device, error when updating device.");
                        }
                    }
                    catch (Exception exc)
                    {
                        LogError(DeviceName + " device, error when updating device. Exception: " + exc.Message);
                    }


                    Watch.Stop();
                    LastUpdateTime = Watch.ElapsedMilliseconds;


                    UpdateFinished.Invoke(this, new EventArgs());
                    UpdateIsOngoing = false;
                }

            }
        }
        protected abstract bool UpdateDeviceImpl(DeviceColorComposition composition);

        public string GetDeviceUpdatePerformance()
        {
            return IsConnected() ? LastUpdateTime + " ms" : "";
        }
        public async void Connect()
        {
            if (GetDeviceType() == AuroraDeviceType.Keyboard && Global.Configuration.DevicesDisableKeyboard ||
                GetDeviceType() == AuroraDeviceType.Mouse && Global.Configuration.DevicesDisableMouse ||
                GetDeviceType() == AuroraDeviceType.Headset && Global.Configuration.DevicesDisableHeadset ||
                GetDeviceType() == AuroraDeviceType.OpenRGBKeyboard && Global.Configuration.DevicesDisableOpenRGBKeyboard ||
                GetDeviceType() == AuroraDeviceType.OpenRGBMouse && Global.Configuration.DevicesDisableOpenRGBMouse ||
                GetDeviceType() == AuroraDeviceType.OpenRGBHeadset && Global.Configuration.DevicesDisableOpenRGBHeadset)
            {
                Disconnect();
            }
            else
            {
                try
                {
                    if (await Task.Run(() => ConnectImpl()))
                    {
                        ConnectionHandler.Invoke(this, new EventArgs());
                        DeviceIsConnected = true;
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Info("Device, " + GetDeviceName() + ", throwed exception:" + exc.ToString());
                }
            }
        }
        protected virtual bool ConnectImpl()
        {
            return true;
        }

        public void Disconnect()
        {
            if (IsConnected())
            {
                DisconnectImpl();
                DeviceIsConnected = false;
                ConnectionHandler.Invoke(this, new EventArgs());
            }
        }
        protected virtual void DisconnectImpl()
        {
        }

        public abstract List<DeviceKey> GetAllDeviceKey();

        protected abstract string DeviceName { get; }
        public string GetDeviceName() => DeviceName;

        public virtual string GetDeviceDetails() => DeviceName + ": " + (IsConnected() ? "Connected" : "Not connected");

        protected abstract AuroraDeviceType AuroraDeviceType { get; }
        public AuroraDeviceType GetDeviceType() => AuroraDeviceType;

        public VariableRegistry GetRegisteredVariables()
        {
            if (variableRegistry == null)
            {
                variableRegistry = new VariableRegistry();
                RegisterVariables(variableRegistry);
            }
            return variableRegistry;
        }
        /// <summary>
        /// Only called once when registering variables. Can be empty if not needed
        /// </summary>
        protected virtual void RegisterVariables(VariableRegistry local)
        {
            //purposefully empty, if varibles are needed, this should be overridden
        }

        public bool IsConnected() => DeviceIsConnected;

        protected void LogInfo(string s) => Global.logger.Info(s);

        protected void LogError(string s) => Global.logger.Error(s);

        protected Color CorrectAlpha(Color clr) => Utils.ColorUtils.CorrectWithAlpha(clr);

        protected VariableRegistry GlobalVarRegistry => Global.Configuration.VarRegistry;
    }

    public abstract class AuroraKeyboardDevice : AuroraDevice
    {
        protected override AuroraDeviceType AuroraDeviceType => AuroraDeviceType.Keyboard;

    }
}
