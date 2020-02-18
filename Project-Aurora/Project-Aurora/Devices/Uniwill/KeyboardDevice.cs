using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Devices.RGBNet;
using Aurora.Settings;
using keyboard;
using Microsoft.Win32;
using RegistryUtils;

namespace Aurora.Devices.Uni
{
     
    public class KeyboardDevice : Device
    {
        
            // Generic Variables
            private string devicename = "keyboard";
            private bool isInitialized = false;

            private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            private long lastUpdateTime = 0;

         

        List<AuroraInterface> DeviceList = new List<AuroraInterface>();
        //  Controll Class
        private AuroraInterface keyboard = null;
        /// 
        //private AuroraInterface keyboard  = null;

        //private AuroraInterface lightbar = KeyboardFactory.CreateHIDDevice("hidlightbar");.

        System.Timers.Timer _CheckControlCenter = new System.Timers.Timer();

        int GamingCenterType = 0;
        public KeyboardDevice()
        {
            devicename = KeyboardFactory.GetOEMName();
            ChoiceGamingCenter();

            //_CheckControlCenter.Start();
            //_CheckControlCenter.Interval = 1000;
            //_CheckControlCenter.Elapsed += _CheckControlCenter_Elapsed;
           
 
        }

        private void _CheckControlCenter_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
           
        }

        private void ChoiceGamingCenter()
        {
            GamingCenterType = CheckGC();
             
            if (GamingCenterType == 1)
            {
                RegistryMonitor monitor = new RegistryMonitor(@"HKEY_LOCAL_MACHINE\SOFTWARE\OEM\Aurora");
                monitor.RegChangeNotifyFilter = RegChangeNotifyFilter.Value;
                monitor.RegChanged += new EventHandler(OnRegChanged);
                monitor.Start();
            }
           
      

        }

        private int CheckGC()
        {
            try
            {
                //CCI
                string m_sRegPathI = @"\OEM\Aurora";
                int Control = (int)RegistrySoftwareKeyRead(RegistryHive.LocalMachine, m_sRegPathI, "AuroraSwitch", 0);

                GamingCenterType = 1;
            }
            catch (Exception ex)
            {
                GamingCenterType = 0;
            }
            return GamingCenterType;
        }

        

        public static object RegistrySoftwareKeyRead(RegistryHive hKey, string SubKey, string Name, object defaultvalue)
        {
            if (Environment.Is64BitOperatingSystem)
                return RegistryKey.OpenBaseKey(hKey, RegistryView.Registry64).OpenSubKey("SOFTWARE" + SubKey).GetValue(Name, defaultvalue);
            else
                return RegistryKey.OpenBaseKey(hKey, RegistryView.Registry32).OpenSubKey("SOFTWARE\\WOW6432Node" + SubKey).GetValue(Name, defaultvalue);
        }
      

        public bool CheckGCPower()
        {
           if(GamingCenterType==1)
           {
                string m_sRegPathI = @"\OEM\Aurora";
                int Control = (int)RegistrySoftwareKeyRead(RegistryHive.LocalMachine, m_sRegPathI, "AuroraSwitch", 0);
                if (Control == 0)
                    return false;
                else
                    return true;
            }
            
            else 
              return true;
        }
   

        private void OnRegChanged(object sender, EventArgs e)
        {
            
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

        public string GetDeviceName()
            {
                return devicename;
            }

            public string GetDeviceDetails()
            {
                if (isInitialized)
                {
                    return devicename + ": Initialized";
                }
                else
                {
                    return devicename + ": Not initialized";
                }
            }

            public bool Initialize()
            {
           

              if (!isInitialized && CheckGCPower())
                {
                    try
                    {
                     
                        keyboard =   KeyboardFactory.CreateHIDDevice("hidkeyboard");
                   
                    if (keyboard!=null)
                        {
                            isInitialized = true;
                            return true;
                        }
                           
                    
                    isInitialized = false;
                    return false;
                    // Mark Initialized = TRUE
                   
                    }
                    catch (Exception ex)
                    {
                        Global.logger.Error("keyboard device, Exception! Message:" + ex);
                    }
                    //try
                    //{
                    //    // Initialize  
                       


                    //    // Mark Initialized = TRUE
                    //    isInitialized = true;
                    //    return true;
                    //}
                    //catch (Exception ex)
                    //{
                    //    Global.logger.Error("keyboard device, Exception! Message:" + ex);
                    //}



                   // Mark Initialized = FALSE
                   isInitialized = false;
                    return false;
                }
 

            return isInitialized;

            }

            // Handle Logon Event
            //void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
            //{
            //    if (this.IsInitialized() && e.Reason.Equals(SessionSwitchReason.SessionUnlock))
            //    { // Only Update when Logged In
            //        this.SendColorsToKeyboard(true);
            //    }
            //}

            public void Shutdown()
            {
                if (this.IsInitialized())
                {
                    if(CheckGCPower())
                     keyboard?.release();

                    bRefreshOnce = true;
                    isInitialized = false;
                }
            }

            public void Reset()
            {
                if (this.IsInitialized())
                {
                 if(CheckGCPower())
                    keyboard?.release();

                 bRefreshOnce = true;
                 isInitialized = false;
               }
                   
            }

            public bool Reconnect()
            {
                throw new NotImplementedException();
            }

            public bool IsInitialized()
            {
        
               return isInitialized;
            }

            public bool IsConnected()
            {
                 return isInitialized;
           }
            bool bRefreshOnce = true; // This is used to refresh effect between Row-Type and Fw-Type change or layout light level change
            public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false) // Is this necessary?
            {
                if (e.Cancel) return false;

                bool update_result = false;
              
                watch.Restart();
                keyboard?.SetEffect(0x64, 0x00, bRefreshOnce, keyColors, e);

                bRefreshOnce = false;
                watch.Stop();

                lastUpdateTime = watch.ElapsedMilliseconds;

                return update_result;
           }
         
        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;
            
            bool update_result = false;
             
            watch.Restart();
            Dictionary<DeviceKeys, Color> keyColors = colorComposition.keyColors;
            keyboard?.SetEffect(0x64, 0x00, bRefreshOnce, keyColors, e);

            bRefreshOnce = false;
            Thread.Sleep(1);

 
            watch.Stop();
 
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
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

            public string GetDeviceUpdatePerformance()
            {
                return (isInitialized ? lastUpdateTime + " ms" : "");
            }

            public VariableRegistry GetRegisteredVariables()
            {
                return new VariableRegistry();
            }
        }
   
}
