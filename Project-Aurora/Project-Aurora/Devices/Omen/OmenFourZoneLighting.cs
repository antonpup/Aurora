using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Management;

using Aurora.Settings;

namespace Aurora.Devices.Omen
{
    class OmenFourZoneLighting : IOmenDevice
    {
        private const int ColorSize = 3;
        private const int ColorOffset = 25;
        private static byte[] Sign = new byte[4] { (byte)'S', (byte)'E', (byte)'C', (byte)'U' };

        public static OmenFourZoneLighting GetFourZoneLighting()
        {
            switch (Global.Configuration.keyboard_brand)
            {
                case PreferredKeyboard.OMEN_Four_Zone:
                    return new OmenFourZoneLighting();
            }

            return null;
        }

        internal static int Execute(int command, int commandType, int inputDataSize, byte[] inputData, out byte[] returnData)
        {
            returnData = new byte[0];
            try
            {
                ManagementObject classInstance = new ManagementObject("root\\wmi", "hpqBIntM.InstanceName='ACPI\\PNP0C14\\0_0'", null);
                ManagementObject DataIn = new ManagementClass("root\\wmi:hpqBDataIn");
                ManagementBaseObject inParams = classInstance.GetMethodParameters("hpqBIOSInt128");
                ManagementBaseObject DataOut = new ManagementClass("root\\wmi:hpqBDataOut128");

                DataIn["Sign"] = Sign;
                DataIn["Command"] = command;
                DataIn["CommandType"] = commandType;
                DataIn["Size"] = inputDataSize;
                DataIn["hpqBData"] = inputData;

                inParams["InData"] = DataIn as ManagementBaseObject;
                InvokeMethodOptions methodOptions = new InvokeMethodOptions { Timeout = System.TimeSpan.MaxValue, };

                ManagementBaseObject outParams = classInstance.InvokeMethod("hpqBIOSInt128", inParams, methodOptions);
                DataOut = outParams["OutData"] as ManagementBaseObject;

                returnData = (DataOut["Data"] as byte[]);

                return Convert.ToInt32(DataOut["rwReturnCode"]);
            }
            catch (Exception err)
            {
                Global.logger.Error("OMEN Four zone lighting - WmiCommand.Execute occurs exception: " + err);
                return -1;
            }
        }

        public void SetLights(Dictionary<DeviceKeys, Color> keyColors)
        {
            Task.Run(() => {
                if (Monitor.TryEnter(this))
                {
                    try
                    {
                        // Check Fn + F4 status.
                        byte[] outData = null;
                        int res = Execute(0x20009, 0x04, 0, null, out outData);
                        if (!Convert.ToBoolean(outData[0] & 0x80)) return;

                        // Get colors of four zonesss.
                        outData = null;
                        res = Execute(0x20009, 0x02, 0, null, out outData);

                        byte[] inData = outData;
                        inData[0] = 0x3;
                        if(keyColors.ContainsKey(DeviceKeys.ENTER))
                        {
                            inData[ColorOffset + 0] = keyColors[DeviceKeys.ENTER].R;
                            inData[ColorOffset + 1] = keyColors[DeviceKeys.ENTER].G;
                            inData[ColorOffset + 2] = keyColors[DeviceKeys.ENTER].B;
                        }

                        if (keyColors.ContainsKey(DeviceKeys.J))
                        {
                            inData[ColorOffset + 1 * ColorSize + 0] = keyColors[DeviceKeys.J].R;
                            inData[ColorOffset + 1 * ColorSize + 1] = keyColors[DeviceKeys.J].G;
                            inData[ColorOffset + 1 * ColorSize + 2] = keyColors[DeviceKeys.J].B;
                        }

                        if (keyColors.ContainsKey(DeviceKeys.E))
                        {
                            inData[ColorOffset + 2 * ColorSize + 0] = keyColors[DeviceKeys.E].R;
                            inData[ColorOffset + 2 * ColorSize + 1] = keyColors[DeviceKeys.E].G;
                            inData[ColorOffset + 2 * ColorSize + 2] = keyColors[DeviceKeys.E].B;
                        }

                        if (keyColors.ContainsKey(DeviceKeys.A))
                        {
                            inData[ColorOffset + 3 * ColorSize + 0] = keyColors[DeviceKeys.A].R;
                            inData[ColorOffset + 3 * ColorSize + 1] = keyColors[DeviceKeys.A].G;
                            inData[ColorOffset + 3 * ColorSize + 2] = keyColors[DeviceKeys.A].B;
                        }

                        // Set colors of four zonesss.
                        outData = null;
                        res = Execute(0x20009, 0x03, 128, inData, out outData);
                        if (res != 0)
                        {
                            Global.logger.Error("OMEN Four zone lighting fail: " + res);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }
            });
        }

        public string GetDeviceName()
        {
            return string.Empty;
        }
        
        public void Shutdown()
        {

        }
    }
}
