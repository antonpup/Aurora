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
        #region Private Properties
        private string applicationName = "New Application Profile";
        #endregion

        #region Public Properties
        public string ApplicationName { get { return applicationName; } set { applicationName = value; InvokePropertyChanged(); } }
        #endregion

        public GenericApplicationSettings()
        {

        }

        public GenericApplicationSettings(string appname) : base()
        {
            applicationName = appname;
        }

    }
}
