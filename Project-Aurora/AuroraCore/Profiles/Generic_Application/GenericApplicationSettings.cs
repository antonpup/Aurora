namespace Aurora.Profiles.Generic_Application
{
    public class GenericApplicationSettings : ApplicationSettings
    {
        #region Private Properties
        private string applicationName = "New Application Profile";
        #endregion

        #region Public Properties
        public string ApplicationName { get { return applicationName; } set { UpdateVar(ref applicationName, value); } }
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
