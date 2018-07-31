using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;

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

        // Reset KBLED Colors (It uses Clevo's Hotkeys Registry to determine the current selected color) 
        public bool ResetKBLEDColors() {
            
            RegistryKey hotkeyReg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\hotkey\LEDKB");

            if (hotkeyReg?.GetValue("LEDKB_Status", "0")?.Equals("0") ?? true)
            {
                // KB LED OFF
                this.SetKBLEDMode(KBLEDMODE.KBLEDOFF);
            }
            else
            {
                // KB LED ON
                // KB LED Mode
                switch ((string) hotkeyReg.GetValue("KbMode", "1")) {
                    case "0":  // 0: Random Mode
                        this.SetKBLEDMode(KBLEDMODE.FXRandom);
                        break;

                    case "2": // 2: Breathe
                        this.SetKBLEDMode(KBLEDMODE.FXBreath);
                        break;

                    case "3": // 3: Cyclic
                        this.SetKBLEDMode(KBLEDMODE.FXSweep); // TODO: Find the equivalent one
                        break;

                    case "4": // 4: Wave
                        this.SetKBLEDMode(KBLEDMODE.FXSweep); // TODO: Find the equivalent one
                        break;

                    case "5": // 5: Dance
                        this.SetKBLEDMode(KBLEDMODE.FXDance);
                        break;

                    case "6": // 6: Tempo
                        this.SetKBLEDMode(KBLEDMODE.FXSweep);
                        break;

                    case "7": // 7: Flash
                        this.SetKBLEDMode(KBLEDMODE.FXSweep); // TODO: Find the equivalent one
                        break;

                    case "1":  // 1: Static Light
                        double alpha = 0; // Color Alpha
                        try {
                            alpha = (Int32.Parse((string) hotkeyReg.GetValue("LEDKB_Backlight", "3"))/3);
                        }
                        catch (FormatException) { } // Ignore

                        // Left Side Colors
                        if (hotkeyReg.GetValue("KbLeftStatus", "1").Equals("1")) 
                        {
                            // KBLEFT is ON
                            Color KBLeftColor = parseClevoRGBString((string) hotkeyReg.GetValue("KbLeft", "0_0_255"));
                            this.SetKBLED(KBLEDAREA.ColorKBLeft, KBLeftColor.B, KBLeftColor.R, KBLeftColor.G, alpha); // Why is it BRG instead of RGB?
                        }
                        else
                        {
                            // KBLEFT is OFF
                            this.SetKBLED(KBLEDAREA.ColorKBLeft, 0, 0, 0, 1);
                        }

                        // Middle Colors
                        if (hotkeyReg.GetValue("KbMidStatus", "1").Equals("1"))
                        {
                            // KBCENTER is ON
                            Color KBCenterColor = parseClevoRGBString((string)hotkeyReg.GetValue("KbMid", "0_0_255"));
                            this.SetKBLED(KBLEDAREA.ColorKBCenter, KBCenterColor.B, KBCenterColor.R, KBCenterColor.G, alpha); // Why is it BRG instead of RGB?
                        }
                        else
                        {
                            // KBCENTER is OFF
                            this.SetKBLED(KBLEDAREA.ColorKBCenter, 0, 0, 0, 1);
                        }

                        // Right Side Colors
                        if (hotkeyReg.GetValue("KbRightStatus", "1").Equals("1"))
                        {
                            // KBCENTER is ON
                            Color KBRightColor = parseClevoRGBString((string)hotkeyReg.GetValue("KbRight", "0_0_255"));
                            this.SetKBLED(KBLEDAREA.ColorKBRight, KBRightColor.B, KBRightColor.R, KBRightColor.G, alpha); // Why is it BRG instead of RGB?
                        }
                        else
                        {
                            // KBCENTER is OFF
                            this.SetKBLED(KBLEDAREA.ColorKBRight, 0, 0, 0, 1);
                        }

                        // Touchpad Colors
                        // KbTp (Old Reg Key), KbLogo (New Reg Key)
                        switch ((string)hotkeyReg.GetValue("KbLogoStatus", "UNKNOWN")) {
                            case "UNKNOWN":
                                // Maybe using old KbTp key?
                                if (hotkeyReg.GetValue("KbTpStatus", "1").Equals("1"))
                                {
                                    // KBCENTER is ON
                                    Color KBTouchpadColorLegacy = parseClevoRGBString((string)hotkeyReg.GetValue("KbTp", "0_0_255"));
                                    this.SetKBLED(KBLEDAREA.ColorTouchpad, KBTouchpadColorLegacy.B, KBTouchpadColorLegacy.R, KBTouchpadColorLegacy.G, alpha); // Why is it BRG instead of RGB?
                                }
                                else
                                {
                                    // KBCENTER is OFF
                                    this.SetKBLED(KBLEDAREA.ColorTouchpad, 0, 0, 0, 1);
                                }
                                break;

                            case "1":
                                // Touchpad is ON
                                Color KBTouchpadColor = parseClevoRGBString((string)hotkeyReg.GetValue("KbLogo", "0_0_255"));
                                this.SetKBLED(KBLEDAREA.ColorTouchpad, KBTouchpadColor.B, KBTouchpadColor.R, KBTouchpadColor.G, alpha); // Why is it BRG instead of RGB?
                                break;

                            case "0":
                                // Touchpad is OFF
                                this.SetKBLED(KBLEDAREA.ColorTouchpad, 0, 0, 0, 1);
                                break;
                        }
                        
                        break;

                    default:
                        this.SetKBLEDMode(KBLEDMODE.KBLEDOFF);
                        break;

                }
            }

            // Cleanup
            hotkeyReg?.Close();
            hotkeyReg?.Dispose();

            return false;
        }

        // Quick Tools
        private Color parseClevoRGBString(string clevo_rgb)
        {
            string[] rgb = clevo_rgb.Split('_'); // Split string by "_"
            if (rgb.Length == 3) {
                try
                {
                    return Color.FromArgb(0, byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
                }
                catch (Exception) {
                    return Color.Black;
                }
            }
            return Color.Black;
        }
    }
}
