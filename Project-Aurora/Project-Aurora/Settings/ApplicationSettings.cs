using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class ApplicationSettings : SettingsBase
    {
        #region Private Properties
        private bool isEnabled = true;

        private bool hidden = false;

        private string selectedProfile = "default";
        #endregion

        #region Public Properties
        public bool IsEnabled { get { return isEnabled; } set { UpdateVar(ref isEnabled, value); } }

        public bool Hidden { get { return hidden; } set { UpdateVar(ref hidden, value); } }

        public string SelectedProfile { get { return selectedProfile; } set { UpdateVar(ref selectedProfile, value); } }
        #endregion

        public ApplicationSettings()
        {

        }
    }

    public class FirstTimeApplicationSettings : ApplicationSettings
    {
        #region Private Properties
        private bool isFirstTimeInstalled = false;
        #endregion

        #region Public Properties
        public bool IsFirstTimeInstalled { get { return isFirstTimeInstalled; } set { UpdateVar(ref isFirstTimeInstalled, value); } }
        #endregion

        public FirstTimeApplicationSettings()
        {

        }
    }
}
