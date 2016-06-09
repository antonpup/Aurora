using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class KeyboardKey
    {
        public String visualName;
        public Devices.DeviceKeys tag;
        public bool line_break;
        public double margin_left;
        public double margin_top;
        public double width;
        public double height;
        public double font_size;
        public int width_bits;
        public int height_bits;
        public int margin_left_bits;
        public int margin_top_bits;
        public bool enabled = true;

        public KeyboardKey(String text, Devices.DeviceKeys tag, bool enabled = true, bool linebreak = false, double fontsize = 12, double margin_left = 7, double margin_top = 0, double width = 30, double height = 30, int width_bits = 2, int height_bits = 2, int margin_left_bits = 0, int margin_top_bits = 0)
        {
            this.visualName = text;
            this.tag = tag;
            this.line_break = linebreak;
            this.width = width;
            this.height = height;
            this.font_size = fontsize;
            this.margin_left = margin_left;
            this.margin_top = margin_top;
            this.width_bits = width_bits;
            this.height_bits = height_bits;
            this.margin_left_bits = margin_left_bits;
            this.margin_top_bits = margin_top_bits;
            this.enabled = enabled;
        }
    }

    public enum KeyboardBrand
    {
        Logitech,
        Corsair,
        Razer
    };

    public class KeyboardLayoutManager
    {
        private List<KeyboardKey> keyboard = new List<KeyboardKey>();

        private double bitmap_one_pixel = 12.0; // 12 pixels = 1 byte

        private Dictionary<Devices.DeviceKeys, Bitmaping> bitmap_map = new Dictionary<Devices.DeviceKeys, Bitmaping>();

        private String cultures_folder = "kb_layouts";

        public KeyboardLayoutManager(KeyboardBrand brand = KeyboardBrand.Logitech)
        {
            try
            {
                //System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

                //Global.logger.LogLine("Loading brand: " + brand.ToString() + " for: " + System.Threading.Thread.CurrentThread.CurrentCulture.Name);

                if (Directory.Exists(Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), cultures_folder)))
                {
                    string culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

                    switch (Global.Configuration.keyboard_localization)
                    {
                        case PreferredKeyboardLocalization.None:
                            break;
                        case PreferredKeyboardLocalization.intl:
                            culture = "intl";
                            break;
                        case PreferredKeyboardLocalization.us:
                            culture = "en-US";
                            break;
                        case PreferredKeyboardLocalization.uk:
                            culture = "en-GB";
                            break;
                        case PreferredKeyboardLocalization.ru:
                            culture = "ru-RU";
                            break;
                        case PreferredKeyboardLocalization.fr:
                            culture = "fr-FR";
                            break;
                        case PreferredKeyboardLocalization.de:
                            culture = "de-DE";
                            break;
                        case PreferredKeyboardLocalization.jpn:
                            culture = "ja-JP";
                            break;
                            
                    }

                    switch (culture)
                    {
                        case ("ja-JP"):
                            LoadCulture(brand, "jpn");
                            break;
                        case ("de-DE"):
                        case ("hsb-DE"):
                        case ("dsb-DE"):
                            LoadCulture(brand, "de");
                            break;
                        case ("fr-FR"):
                        case ("br-FR"):
                        case ("oc-FR"):
                        case ("co-FR"):
                        case ("gsw-FR"):
                            LoadCulture(brand, "fr");
                            break;
                        case ("cy-GB"):
                        case ("gd-GB"):
                        case ("en-GB"):
                            LoadCulture(brand, "uk");
                            break;
                        case ("ru-RU"):
                        case ("tt-RU"):
                        case ("ba-RU"):
                        case ("sah-RU"):
                            LoadCulture(brand, "ru");
                            break;
                        case ("en-US"):
                            LoadCulture(brand, "us");
                            break;
                        default:
                            LoadCulture(brand, "intl");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LoadDefault();
            }
        }

        private int PixelToByte(int pixel)
        {
            return PixelToByte((double)pixel);
        }

        private int PixelToByte(double pixel)
        {
            return (int)Math.Round(pixel / (double)(this.bitmap_one_pixel));
        }

        private void CalculateBitmap()
        {
            int width_bit = 0;
            int height_bit = 0;
            int width_bit_max = 0;
            int height_bit_max = 0;

            foreach (KeyboardKey key in keyboard)
            {
                int key_tly = key.margin_top_bits + height_bit;
                int key_tlx = key.margin_left_bits + width_bit;

                int key_bry = key_tly + key.height_bits;
                int key_brx = key_tlx + key.width_bits;

                this.bitmap_map[key.tag] = new Bitmaping(key_tlx, key_tly, key_brx, key_bry);

                if (width_bit_max < key_brx) width_bit_max = key_brx;
                if (height_bit_max < key_bry) height_bit_max = key_bry;


                if (key.line_break)
                {
                    height_bit += PixelToByte(37);
                    width_bit = 0;
                }
                else
                {
                    width_bit = key_brx;
                    height_bit = key_tly;
                }
            }

            Global.effengine.SetCanvasSize(width_bit_max, height_bit_max);
            Global.effengine.SetBitmapping(this.bitmap_map);
        }

        public void LoadCulture(KeyboardBrand brand, String culture)
        {
            keyboard.Clear();

            var fileName = "layout." + culture + ".json";

            switch (brand)
            {
                case (KeyboardBrand.Corsair):
                    fileName = "Corsair\\" + fileName;
                    break;
                case (KeyboardBrand.Razer):
                    fileName = "Razer\\" + fileName;
                    break;
                default:
                    fileName = "Logitech\\" + fileName;
                    break;
            }

            var layoutPath = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), cultures_folder, fileName);

            if (!File.Exists(layoutPath))
                LoadDefault();

            string content = File.ReadAllText(layoutPath, Encoding.UTF8);
            keyboard = JsonConvert.DeserializeObject<List<KeyboardKey>>(content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

            if (keyboard.Count > 0)
                keyboard.Last().line_break = false;

            keyboard.Add(new KeyboardKey("Mouse", Devices.DeviceKeys.Peripheral, true, true, 12, 45, -60, 90, 90, 6, 6, 4, -3));

            if (keyboard.Count > 0)
                keyboard.Last().line_break = true;

            CalculateBitmap();
        }

        public void LoadDefault()
        {
            keyboard.Clear();

            keyboard.Add(new KeyboardKey("ESC", Devices.DeviceKeys.ESC));

            keyboard.Add(new KeyboardKey("F1", Devices.DeviceKeys.F1, true, false, 12, 32));
            keyboard.Add(new KeyboardKey("F2", Devices.DeviceKeys.F2));
            keyboard.Add(new KeyboardKey("F3", Devices.DeviceKeys.F3));
            keyboard.Add(new KeyboardKey("F4", Devices.DeviceKeys.F4));

            keyboard.Add(new KeyboardKey("F5", Devices.DeviceKeys.F5, true, false, 12, 34));
            keyboard.Add(new KeyboardKey("F6", Devices.DeviceKeys.F6));
            keyboard.Add(new KeyboardKey("F7", Devices.DeviceKeys.F7));
            keyboard.Add(new KeyboardKey("F8", Devices.DeviceKeys.F8));

            keyboard.Add(new KeyboardKey("F9", Devices.DeviceKeys.F9, true, false, 12, 29));
            keyboard.Add(new KeyboardKey("F10", Devices.DeviceKeys.F10));
            keyboard.Add(new KeyboardKey("F11", Devices.DeviceKeys.F11));
            keyboard.Add(new KeyboardKey("F12", Devices.DeviceKeys.F12));

            keyboard.Add(new KeyboardKey("PRINT", Devices.DeviceKeys.PRINT_SCREEN, true, false, 9, 14));
            keyboard.Add(new KeyboardKey("SCRL\r\nLOCK", Devices.DeviceKeys.SCROLL_LOCK, true, false, 9));
            keyboard.Add(new KeyboardKey("PAUSE", Devices.DeviceKeys.PAUSE_BREAK, true, true, 9));

            keyboard.Add(new KeyboardKey("~", Devices.DeviceKeys.TILDE));
            keyboard.Add(new KeyboardKey("1", Devices.DeviceKeys.ONE));
            keyboard.Add(new KeyboardKey("2", Devices.DeviceKeys.TWO));
            keyboard.Add(new KeyboardKey("3", Devices.DeviceKeys.THREE));
            keyboard.Add(new KeyboardKey("4", Devices.DeviceKeys.FOUR));
            keyboard.Add(new KeyboardKey("5", Devices.DeviceKeys.FIVE));
            keyboard.Add(new KeyboardKey("6", Devices.DeviceKeys.SIX));
            keyboard.Add(new KeyboardKey("7", Devices.DeviceKeys.SEVEN));
            keyboard.Add(new KeyboardKey("8", Devices.DeviceKeys.EIGHT));
            keyboard.Add(new KeyboardKey("9", Devices.DeviceKeys.NINE));
            keyboard.Add(new KeyboardKey("0", Devices.DeviceKeys.ZERO));
            keyboard.Add(new KeyboardKey("-", Devices.DeviceKeys.MINUS));
            keyboard.Add(new KeyboardKey("=", Devices.DeviceKeys.EQUALS));
            keyboard.Add(new KeyboardKey("BACKSPACE", Devices.DeviceKeys.BACKSPACE, true, false, 12, 7, 0, 67));

            keyboard.Add(new KeyboardKey("INSERT", Devices.DeviceKeys.INSERT, true, false, 9, 14));
            keyboard.Add(new KeyboardKey("HOME", Devices.DeviceKeys.HOME, true, false, 9));
            keyboard.Add(new KeyboardKey("PAGE\r\nUP", Devices.DeviceKeys.HOME, true, false, 9));

            keyboard.Add(new KeyboardKey("NUM\r\nLOCK", Devices.DeviceKeys.NUM_LOCK, true, false, 9, 14));
            keyboard.Add(new KeyboardKey("/", Devices.DeviceKeys.NUM_SLASH));
            keyboard.Add(new KeyboardKey("*", Devices.DeviceKeys.NUM_ASTERISK));
            keyboard.Add(new KeyboardKey("-", Devices.DeviceKeys.NUM_MINUS, true, true));

            keyboard.Add(new KeyboardKey("TAB", Devices.DeviceKeys.TAB, true, false, 12, 7, 0, 50));
            keyboard.Add(new KeyboardKey("Q", Devices.DeviceKeys.Q));
            keyboard.Add(new KeyboardKey("W", Devices.DeviceKeys.W));
            keyboard.Add(new KeyboardKey("E", Devices.DeviceKeys.E));
            keyboard.Add(new KeyboardKey("R", Devices.DeviceKeys.R));
            keyboard.Add(new KeyboardKey("T", Devices.DeviceKeys.T));
            keyboard.Add(new KeyboardKey("Y", Devices.DeviceKeys.Y));
            keyboard.Add(new KeyboardKey("U", Devices.DeviceKeys.U));
            keyboard.Add(new KeyboardKey("I", Devices.DeviceKeys.I));
            keyboard.Add(new KeyboardKey("O", Devices.DeviceKeys.O));
            keyboard.Add(new KeyboardKey("P", Devices.DeviceKeys.P));
            keyboard.Add(new KeyboardKey("{", Devices.DeviceKeys.OPEN_BRACKET));
            keyboard.Add(new KeyboardKey("}", Devices.DeviceKeys.CLOSE_BRACKET));
            keyboard.Add(new KeyboardKey("\\", Devices.DeviceKeys.BACKSLASH, true, false, 12, 7, 0, 49));

            keyboard.Add(new KeyboardKey("DEL", Devices.DeviceKeys.DELETE, true, false, 9, 12));
            keyboard.Add(new KeyboardKey("END", Devices.DeviceKeys.END, true, false, 9));
            keyboard.Add(new KeyboardKey("PAGE\r\nDOWN", Devices.DeviceKeys.PAGE_DOWN, true, false, 9));

            keyboard.Add(new KeyboardKey("7", Devices.DeviceKeys.NUM_SEVEN, true, false, 12, 14));
            keyboard.Add(new KeyboardKey("8", Devices.DeviceKeys.NUM_EIGHT));
            keyboard.Add(new KeyboardKey("9", Devices.DeviceKeys.NUM_NINE));
            keyboard.Add(new KeyboardKey("+", Devices.DeviceKeys.NUM_PLUS, true, true, 12, 7, 0, 30, 69));

            keyboard.Add(new KeyboardKey("CAPS\r\nLOCK", Devices.DeviceKeys.CAPS_LOCK, true, false, 9, 7, 0, 60));
            keyboard.Add(new KeyboardKey("A", Devices.DeviceKeys.A));
            keyboard.Add(new KeyboardKey("S", Devices.DeviceKeys.S));
            keyboard.Add(new KeyboardKey("D", Devices.DeviceKeys.D));
            keyboard.Add(new KeyboardKey("F", Devices.DeviceKeys.F));
            keyboard.Add(new KeyboardKey("G", Devices.DeviceKeys.G));
            keyboard.Add(new KeyboardKey("H", Devices.DeviceKeys.H));
            keyboard.Add(new KeyboardKey("J", Devices.DeviceKeys.J));
            keyboard.Add(new KeyboardKey("K", Devices.DeviceKeys.K));
            keyboard.Add(new KeyboardKey("L", Devices.DeviceKeys.L));
            keyboard.Add(new KeyboardKey(":", Devices.DeviceKeys.SEMICOLON));
            keyboard.Add(new KeyboardKey("\"", Devices.DeviceKeys.APOSTROPHE));
            keyboard.Add(new KeyboardKey("ENTER", Devices.DeviceKeys.ENTER, true, false, 12, 7, 0, 76));

            keyboard.Add(new KeyboardKey("4", Devices.DeviceKeys.NUM_FOUR, true, false, 12, 130));
            keyboard.Add(new KeyboardKey("5", Devices.DeviceKeys.NUM_FIVE));
            keyboard.Add(new KeyboardKey("6", Devices.DeviceKeys.NUM_SIX, true, true));
            //Space taken up by +

            keyboard.Add(new KeyboardKey("SHIFT", Devices.DeviceKeys.LEFT_SHIFT, true, false, 12, 7, 0, 78));
            keyboard.Add(new KeyboardKey("Z", Devices.DeviceKeys.Z));
            keyboard.Add(new KeyboardKey("X", Devices.DeviceKeys.X));
            keyboard.Add(new KeyboardKey("C", Devices.DeviceKeys.C));
            keyboard.Add(new KeyboardKey("V", Devices.DeviceKeys.V));
            keyboard.Add(new KeyboardKey("B", Devices.DeviceKeys.B));
            keyboard.Add(new KeyboardKey("N", Devices.DeviceKeys.N));
            keyboard.Add(new KeyboardKey("M", Devices.DeviceKeys.M));
            keyboard.Add(new KeyboardKey("<", Devices.DeviceKeys.COMMA));
            keyboard.Add(new KeyboardKey(">", Devices.DeviceKeys.PERIOD));
            keyboard.Add(new KeyboardKey("?", Devices.DeviceKeys.FORWARD_SLASH));
            keyboard.Add(new KeyboardKey("SHIFT", Devices.DeviceKeys.RIGHT_SHIFT, true, false, 12, 7, 0, 95));

            keyboard.Add(new KeyboardKey("UP", Devices.DeviceKeys.ARROW_UP, true, false, 9, 49));

            keyboard.Add(new KeyboardKey("1", Devices.DeviceKeys.NUM_ONE, true, false, 12, 51));
            keyboard.Add(new KeyboardKey("2", Devices.DeviceKeys.NUM_TWO));
            keyboard.Add(new KeyboardKey("3", Devices.DeviceKeys.NUM_THREE));
            keyboard.Add(new KeyboardKey("ENTER", Devices.DeviceKeys.NUM_ENTER, true, true, 9, 7, 0, 30, 67));

            keyboard.Add(new KeyboardKey("CTRL", Devices.DeviceKeys.RIGHT_CONTROL, true, false, 12, 7, 0, 51));
            keyboard.Add(new KeyboardKey("WIN", Devices.DeviceKeys.RIGHT_WINDOWS, true, false, 12, 5, 0, 39));
            keyboard.Add(new KeyboardKey("ALT", Devices.DeviceKeys.RIGHT_ALT, true, false, 12, 5, 0, 42));

            keyboard.Add(new KeyboardKey("SPACE", Devices.DeviceKeys.SPACE, true, false, 12, 7, 0, 208));
            keyboard.Add(new KeyboardKey("ALT", Devices.DeviceKeys.LEFT_ALT, true, false, 12, 5, 0, 41));
            keyboard.Add(new KeyboardKey("WIN", Devices.DeviceKeys.LEFT_WINDOWS, true, false, 12, 5, 0, 41));
            keyboard.Add(new KeyboardKey("APP", Devices.DeviceKeys.APPLICATION_SELECT, true, false, 12, 5, 0, 41));
            keyboard.Add(new KeyboardKey("CTRL", Devices.DeviceKeys.LEFT_CONTROL, true, false, 12, 5, 0, 50));

            keyboard.Add(new KeyboardKey("LEFT", Devices.DeviceKeys.ARROW_LEFT, true, false, 9, 12));
            keyboard.Add(new KeyboardKey("DOWN", Devices.DeviceKeys.ARROW_DOWN, true, false, 9));
            keyboard.Add(new KeyboardKey("RIGHT", Devices.DeviceKeys.ARROW_DOWN, true, false, 9));

            keyboard.Add(new KeyboardKey("0", Devices.DeviceKeys.NUM_ZERO, true, false, 12, 14, 0, 67));
            keyboard.Add(new KeyboardKey(".", Devices.DeviceKeys.NUM_PERIOD, true, true));

            CalculateBitmap();
        }

        public List<KeyboardKey> GetLayout()
        {
            return keyboard;
        }
    }
}
