﻿using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Overwatch
{
    public class OverwatchProfileManager : ProfileManager
    {
        public OverwatchProfileManager()
            : base("Overwatch", "overwatch", "overwatch.exe", new OverwatchSettings(), new GameEvent_Overwatch())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_Overwatch();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/overwatch_icon.png", UriKind.Relative));

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
                        return JsonConvert.DeserializeObject<OverwatchSettings>(profile_content, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
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
