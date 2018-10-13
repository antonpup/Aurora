using Aurora.Plugins;
using Aurora.Settings;
using Aurora.Utils;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aurora.Applications.Application;

namespace Aurora.Applications
{
    public class ApplicationManagerSettings : SettingsProfile
    {
        public override void Default()
        {
            
            
        }
    }

    public class ApplicationManager : ObjectSettings<ApplicationManagerSettings>, IInitialize, IPluginConsumer
    {
        public bool Initialized { get; private set; } = false;
        
        //TODO: Try and find a better way to handle these applications
        public Dictionary<string, ApplicationBase> Applications { get; private set; } = new Dictionary<string, ApplicationBase>();// { { "desktop", new Desktop.Desktop() } };

        //public Desktop.Desktop DesktopProfile { get { return (Desktop.Desktop)Events["desktop"]; } }

        private Dictionary<string, string> EventProcesses { get; set; } = new Dictionary<string, string>();

        private Dictionary<string, string> EventTitles { get; set; } = new Dictionary<string, string>();

        private Dictionary<string, string> EventAppIDs { get; set; } = new Dictionary<string, string>();

        public bool Initialize()
        {
            if (this.Initialized)
                return true;
            


            return (this.Initialized = true);
        }
        
        public ApplicationBase GetProfileFromProcessName(string process)
        {
            if (this.Initialized)
                return null;

            if (EventProcesses.ContainsKey(process))
            {
                if (!Applications.ContainsKey(EventProcesses[process]))
                    Logger.Log.Warn($"GetProfileFromProcess: The process '{process}' exists in EventProcesses but subsequently '{EventProcesses[process]}' does not exist in Events!");

                return Applications[EventProcesses[process]];
            }
            else if (Applications.ContainsKey(process))
                return Applications[process];

            return null;
        }

        public ApplicationBase GetProfileFromProcessTitle(string title) {
            foreach (var entry in EventTitles) {
                if (Regex.IsMatch(title, entry.Key, RegexOptions.IgnoreCase)) {
                    if (!Applications.ContainsKey(entry.Value))
                        Logger.Log.Warn($"GetProfileFromProcess: The process with title '{title}' matchs an item in EventTitles but subsequently '{entry.Value}' does not exist in Events!");
                    else
                        return Applications[entry.Value]; // added in an else so we keep searching for more valid regexes.
                }
            }
            return null;
        }

        public ApplicationBase GetProfileFromAppID(string appid)
        {
            if (EventAppIDs.ContainsKey(appid))
            {
                if (!Applications.ContainsKey(EventAppIDs[appid]))
                    Logger.Log.Warn($"GetProfileFromAppID: The appid '{appid}' exists in EventAppIDs but subsequently '{EventAppIDs[appid]}' does not exist in Events!");
                return Applications[EventAppIDs[appid]];
            }
            else if (Applications.ContainsKey(appid))
                return Applications[appid];

            return null;
        }

        
        public void Visit(PluginBase plugin)
        {
            plugin.Process(this);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ApplicationManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}