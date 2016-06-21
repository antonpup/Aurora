using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.CSGO
{
    public class CSGOProfileManager : ProfileManager
    {
        public CSGOProfileManager()
            : base("CS:GO", "csgo", "csgo.exe", new CSGOSettings(), new GameEvent_CSGO())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_CSGO();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/csgo_64x64.png", UriKind.Relative));

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
                        return JsonConvert.DeserializeObject<CSGOSettings>(profile_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
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
