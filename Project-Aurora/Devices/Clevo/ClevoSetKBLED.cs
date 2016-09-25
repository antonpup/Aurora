using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;


/**
*   ClevoSetKBLED - Class to control Clevo KBLED Colors
**/
namespace Aurora.Devices.Clevo{
    class ClevoSetKBLED{

        // Enums
        public enum KBLEDAREA : byte
        {
            ColorKBLeft = 0xF0,
            ColorKBCenter = 0xF1,
            ColorKBRight = 0xF2,
            ColorTouchpad = 0xF3
        };

        public enum KBLEDMODE : UInt32
        {
            KBLEDON = 0x00001000,
            KBLEDOFF = 0x0000A000,
            FXDance = 0x80000000,
            FXBreath = 0x30000000,
            FXBlink = 0xA0000000,
            FXRandom = 0x90000000,
            FXSweep = 0xB0000000
        };

        // Variables
        private ManagementObject clevo = null;

        // Initialize 
        public bool Initialize() {
            try {
                // Security Options 
                ConnectionOptions options = new ConnectionOptions();
                options.Impersonation = System.Management.ImpersonationLevel.Impersonate;

                // Make connection
                ManagementScope scope = new ManagementScope("root\\WMI", options);
                scope.Connect();

                //Query system for Operating System information
                ObjectQuery query = new ObjectQuery("SELECT * FROM CLEVO_GET");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

                ManagementObjectCollection queryCollection = searcher.Get();
                foreach (ManagementObject m in queryCollection)
                {
                    // Return Clevo Object
                    clevo = m;
                    //searcher.Dispose();
                    //queryCollection.Dispose();
                    return true;
                }

            }catch (Exception e){
                Console.WriteLine("ERROR: Could not connect to Clevo WMI Service. Clevo HotKey software may not be installed.");
            }

            return false;
        }

        // Release WMI Connection
        public bool Release() {
            if (clevo != null){
                clevo.Dispose();
                return true;
            }else {
                return false;
            }
        }

        // Set KBLED Mode
        public bool SetKBLEDMode(KBLEDMODE mode) {
            return SetKBLED((UInt32) mode);
        }

        // Set KBLED Color
        public bool SetKBLED(UInt32 hex_value) {
            try{
                clevo.InvokeMethod("SetKBLED", new Object[] { hex_value });
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        // Set KBLED Color
        public bool SetKBLED(KBLEDAREA area, byte color_r, byte color_g, byte color_b) {
            return this.SetKBLED(((((((UInt32) area << 8) + color_r) << 8) + color_g) << 8) + color_b);
        }

        // Set KBLED Color with Alpha
        public bool SetKBLED(KBLEDAREA area, byte color_r, byte color_g, byte color_b, double alpha) {
            if (alpha >= 1) { // Ignore all values over 1
                return this.SetKBLED(area, color_r, color_g, color_b);
            }
            else {
                return this.SetKBLED(area, Convert.ToByte(color_r * alpha), Convert.ToByte(color_g * alpha), Convert.ToByte(color_b * alpha));
            }
        }

        // Reset KBLED Colors
        public bool ResetKBLEDColors() {
            // TODO: Implement this
            return false;
        }
    }
}
