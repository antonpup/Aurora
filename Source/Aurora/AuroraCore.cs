using Aurora.Applications;
using Aurora.Plugins;
using Aurora.Settings;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Aurora.Applications.Layers;

namespace Aurora
{
    public static class GlobalConstants
    {
        public const string DataStorageDirectory = "";
    }
    
    public class AuroraCore : IInitialize, IPluginConsumer
    {
        public bool Initialized { get; private set; } = false;

        public PluginManager PluginManager { get; } = new PluginManager();

        public ApplicationManager ApplicationManager { get; } = new ApplicationManager();

        public bool Initialize()
        {
            //Return true if this has already been initialized
            if (this.Initialized)
                return true;
            
            
            Debug.Assert(Logger.Initialize(), "Logger failed to initialize!");

            PluginManager.LoadPlugins();
            PluginManager.CallVisit(this);

            return (Initialized = true);
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
        // ~AuroraCore() {
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

        public void Visit(PluginBase plugin)
        {
            //Call visit on child IPluginConsumers
            this.ApplicationManager.Visit(plugin);
            LayerFactory.Instance.Visit(plugin);

        }
    }
}
