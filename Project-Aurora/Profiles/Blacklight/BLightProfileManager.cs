using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Profiles.Blacklight
{
    public class BLightProfileManager : ProfileManager
    {
        public BLightProfileManager()
            : base("Blacklight: Retribution", "BLight", "FoxGame-win32-Shipping.exe", new BLightSettings(), new GameEvent_BLight())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_BLight();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/blacklight_64x64.png", UriKind.Relative));

            return Icon;
        }

        internal override ProfileSettings LoadProfile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string profile_content = File.ReadAllText(path, Encoding.UTF8);

                    if (!String.IsNullOrWhiteSpace(profile_content))
                        return JsonConvert.DeserializeObject<BLightSettings>(profile_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
                }
            }
            catch (Exception exc)
            {
                Global.logger.LogLine(string.Format("Exception Loading Profile: {0}, Exception: {1}", path, exc), Logging_Level.Error);
            }

            return null;
        }
    }
}
