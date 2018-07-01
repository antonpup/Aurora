using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurora.Plugins
{
    public class PluginManager : IInitialize
    {
        public List<PluginBase> Plugins { get; set; } = new List<PluginBase>();

        public bool Initialized { get; private set; } = false;

        public bool Initialize()
        {
            if (Initialized)
                return true;

            LoadPlugins();

            return (Initialized = true);
        }

        public void LoadPlugins()
        {
            if (Plugins.Count > 0)
                return;

            //Iterate and load
        }

        //TODO: Rename this to something better
        public void CallVisit(IPluginConsumer consumer)
        {
            foreach (PluginBase plugin in Plugins)
                consumer.Visit(plugin);
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
        // ~PluginManager() {
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
