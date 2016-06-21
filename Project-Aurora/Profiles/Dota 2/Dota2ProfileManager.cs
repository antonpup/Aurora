using Aurora.Settings;
using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Newtonsoft.Json;

namespace Aurora.Profiles.Dota_2
{
    public class Dota2ProfileManager : ProfileManager
    {
        public Dota2ProfileManager()
            : base("Dota 2", "dota2", "dota2.exe", new Dota2Settings(), new GameEvent_Dota2())
        {
        }

        public override UserControl GetUserControl()
        {
            if(Control == null)
                Control = new Control_Dota2();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/dota2_64x64.png", UriKind.Relative));

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
                        return JsonConvert.DeserializeObject<Dota2Settings>(profile_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
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
