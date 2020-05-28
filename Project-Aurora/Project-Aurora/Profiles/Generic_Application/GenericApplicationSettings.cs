using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Generic_Application
{
    public class GenericApplicationSettings : ApplicationSettings
    {
        public string ApplicationName { get; set; } = "New Application Profile";

        public GenericApplicationSettings()
        {

        }

        public GenericApplicationSettings(string appname) : base()
        {
            ApplicationName = appname;
        }
    }
}
