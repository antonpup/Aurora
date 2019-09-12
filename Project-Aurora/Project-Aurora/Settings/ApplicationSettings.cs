using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class ApplicationSettings : Settings
    {
        #region Private Properties
        private bool isEnabled = true;
        private bool isOverlayEnabled = true;

        private bool hidden = false;

        private string selectedProfile = "default";
        #endregion

        #region Public Properties
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; InvokePropertyChanged(); } }
        public bool IsOverlayEnabled { get { return isOverlayEnabled; } set { isOverlayEnabled = value; InvokePropertyChanged(); } }

        public bool Hidden { get { return hidden; } set { hidden = value; InvokePropertyChanged(); } }

        public string SelectedProfile { get { return selectedProfile; } set { selectedProfile = value; InvokePropertyChanged(); } }
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
        public bool IsFirstTimeInstalled { get { return isFirstTimeInstalled; } set { isFirstTimeInstalled = value; InvokePropertyChanged(); } }
        #endregion

        public FirstTimeApplicationSettings()
        {

        }
    }
}
