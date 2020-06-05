using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Management;

namespace Aurora.Devices.Omen
{
    class OmenFourZoneLighting : IOmenDevice
    {
        private static byte[] sign = new byte[4] { (byte)'S', (byte)'E', (byte)'C', (byte)'U' };

        internal static int Execute(int command, int commandType, int inputDataSize, byte[] inputData, out byte[] returnData)
        {
            returnData = new byte[0];
            try
            {
                ManagementObject classInstance = new ManagementObject("root\\wmi", "hpqBIntM.InstanceName='ACPI\\PNP0C14\\0_0'", null);
                ManagementObject DataIn = new ManagementClass("root\\wmi:hpqBDataIn");
                ManagementBaseObject inParams = classInstance.GetMethodParameters("hpqBIOSInt128");
                ManagementBaseObject DataOut = new ManagementClass("root\\wmi:hpqBDataOut128");

                DataIn["Sign"] = sign;
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
                //Console.WriteLine("WmiCommand.Execute occurs exception: " + err);
                Global.logger.Error("OMEN Four zone lighting - WmiCommand.Execute occurs exception: " + err);
                return -1;
            }
        }

        public void SetLights(Dictionary<DeviceKeys, Color> keyColors)
        {
            const int colorOffset = 25;
            Task.Run(() => {
                if (Monitor.TryEnter(this))
                {
                    try
                    {
                        byte[] inData = new byte[128];
                        inData[0] = 0x3;
                        inData[colorOffset + 0] = keyColors[DeviceKeys.ENTER].R;
                        inData[colorOffset + 1] = keyColors[DeviceKeys.ENTER].G;
                        inData[colorOffset + 2] = keyColors[DeviceKeys.ENTER].B;
                        inData[colorOffset + 1 * 3 + 0] = keyColors[DeviceKeys.K].R;
                        inData[colorOffset + 1 * 3 + 1] = keyColors[DeviceKeys.K].G;
                        inData[colorOffset + 1 * 3 + 2] = keyColors[DeviceKeys.K].B;
                        inData[colorOffset + 2 * 3 + 0] = keyColors[DeviceKeys.D].R;
                        inData[colorOffset + 2 * 3 + 1] = keyColors[DeviceKeys.D].G;
                        inData[colorOffset + 2 * 3 + 2] = keyColors[DeviceKeys.D].B;
                        inData[colorOffset + 3 * 3 + 0] = (byte)(255 - keyColors[DeviceKeys.D].R);
                        inData[colorOffset + 3 * 3 + 1] = (byte)(255 - keyColors[DeviceKeys.D].G);
                        inData[colorOffset + 3 * 3 + 2] = (byte)(255 - keyColors[DeviceKeys.D].B);

                        byte[] outData;
                        var res = Execute(0x20009, 0x03, 128, inData, out outData);
                        if(res != 0)
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
