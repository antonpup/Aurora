﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aurora.Settings;
using Aurora.Utils;
using Microsoft.Win32;
using UniwillSDKDLL;

namespace Aurora.Devices.Uniwill
{
    enum GAMECENTERTYPE
    {
        NONE = 0,
        GAMINGTCENTER = 1,
        CONTROLCENTER = 2
    }

    public class UniwillDevice : IDevice
    {
        // Generic Variables
        private string devicename = "Uniwill";
        private bool isInitialized = false;

        private Stopwatch watch = new Stopwatch();
        private long lastUpdateTime = 0;

        System.Timers.Timer regTimer;
        const string Root = "HKEY_LOCAL_MACHINE";
        const string subkey = @"SOFTWARE\OEM\Aurora";
        const string keyName = Root + "\\" + subkey;
        int SwitchOn = 0;

        private AuroraInterface keyboard = null;

        GAMECENTERTYPE GamingCenterType = 0;

        float brightness = 1f;

        static UniwillDevice() {
            // Setup custom enum description resolver (for changing how some UNIWILL enums appear to the user)
            EnumUtils.RegisterCustomDescriptionResolver<PreferredKeyboard>(UniwillEnumDescriptionResolver);        }

        public UniwillDevice()
        {
            devicename = KeyboardFactory.GetOEMName();
            ChoiceGamingCenter();
        }

        private void ChoiceGamingCenter()
        {
            GamingCenterType = CheckGC();

            if (GamingCenterType == GAMECENTERTYPE.GAMINGTCENTER)
            {
                regTimer = new System.Timers.Timer();
                regTimer.Interval = 300;
                regTimer.Elapsed += OnRegChanged;
                regTimer.Stop();
                regTimer.Start();

            }
        }

        private GAMECENTERTYPE CheckGC()
        {
            int? Control = (int?)Registry.GetValue(keyName, "AuroraSwitch", null);
            
            if(Control.HasValue)
            {
                GamingCenterType = GAMECENTERTYPE.GAMINGTCENTER;
                SwitchOn = Control.Value;
            }
            else
            {
                GamingCenterType = GAMECENTERTYPE.NONE;
                SwitchOn = 0;
            }
            return GamingCenterType;
        }

        public bool CheckGCPower()
        {
            if (GamingCenterType == GAMECENTERTYPE.GAMINGTCENTER)
            {
                int Control = (int)Registry.GetValue(keyName, "AuroraSwitch", 0);
                return Control != 0;
            }
            else
            {
                return true;
            }
        }

        private void OnRegChanged(object sender, EventArgs e)
        {
            int newSwtich = (int)Registry.GetValue(keyName, "AuroraSwitch", 0);
            if (SwitchOn != newSwtich)
            {
                SwitchOn = newSwtich;
                if (CheckGCPower())
                {
                    Initialize();
                }
                else
                {
                    bRefreshOnce = true;
                    isInitialized = false;
                    Shutdown();
                }
            }
        }

        public string DeviceName => devicename;

        public string DeviceDetails => IsInitialized
            ? "Initialized"
            : "Not Initialized";

        public bool Initialize()
        {
            if (!isInitialized && CheckGCPower())
            {
                try
                {
                    keyboard = KeyboardFactory.CreateHIDDevice("hidkeyboard");
                    if (keyboard != null)
                    {
                        bRefreshOnce = true;
                        isInitialized = true;
                        //SetBrightness();
                        return true;
                    }

                    isInitialized = false;
                    return false;
                }
                catch
                {
                    Global.logger.Error("Uniwill device error!");
                }
                // Mark Initialized = FALSE
                isInitialized = false;
                return false;
            }

            return isInitialized;
        }

        public void Shutdown()
        {
            if (this.IsInitialized)
            {
                if (CheckGCPower())
                {
                    keyboard?.release();
                }

                bRefreshOnce = true;
                isInitialized = false;

            }
        }

        public void Reset()
        {
            if (this.IsInitialized)
            {
                if (CheckGCPower())
                {
                    keyboard?.release();
                }

                bRefreshOnce = true;
                isInitialized = false;
            }
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized => isInitialized;

        public bool IsConnected()
        {
            return isInitialized;
        }

        bool bRefreshOnce = true; // This is used to refresh effect between Row-Type and Fw-Type change or layout light level change

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;

            //Alpha necessary for Global Brightness modifier
            var adjustedColors = keyColors.Select(kc => AdjustBrightness(kc));

            bool ret = keyboard?.SetEffect(0x32, 0x00, bRefreshOnce, adjustedColors, e) ?? false;

            bRefreshOnce = false;

            return ret;
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        private KeyValuePair<DeviceKeys, Color> AdjustBrightness(KeyValuePair<DeviceKeys, Color> kc)
        {
            var newEntry = new KeyValuePair<DeviceKeys, Color>(kc.Key, Color.FromArgb(255, Utils.ColorUtils.MultiplyColorByScalar(kc.Value, (kc.Value.A / 255.0D) * brightness)));
            kc = newEntry;
            return kc;
        }

        // Device Status Methods
        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public string DeviceUpdatePerformance => (isInitialized ? lastUpdateTime + " ms" : "");

        public VariableRegistry RegisteredVariables => new VariableRegistry();

        private static string UniwillEnumDescriptionResolver(Enum @enum) {
            try {
                string descriptionString = @enum.GetCustomAttribute<DescriptionAttribute>()?.Description;
                string oemstring = UniwillSDKDLL.KeyboardFactory.GetOEMName();
                if (oemstring.Equals("XMG")) {
                    if (descriptionString.Contains("UNIWILL2P1"))
                        return descriptionString.Replace("UNIWILL2P1", oemstring + " FUSION");
                    else if (descriptionString.Contains("UNIWILL2ND"))
                        return descriptionString.Replace("UNIWILL2ND", oemstring + " NEO");
                    else if (descriptionString.Contains("UNIWILL2P2"))
                        return descriptionString.Replace("UNIWILL2P2", oemstring + " NEO 15");
                } else {
                    if (descriptionString.Contains("UNIWILL2P1"))
                        return descriptionString.Replace("UNIWILL2P1", oemstring + " 550");
                    else if (descriptionString.Contains("UNIWILL2ND"))
                        return descriptionString.Replace("UNIWILL2ND", oemstring + " 35X");
                    else if (descriptionString.Contains("UNIWILL2P2"))
                        return descriptionString.Replace("UNIWILL2P2", oemstring + " 650");
                }
            } catch {
                return null;
            }

            return null;
        }
    }
}
