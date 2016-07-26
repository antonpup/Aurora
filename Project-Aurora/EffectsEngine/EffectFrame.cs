﻿using System;
using System.Collections.Generic;

namespace Aurora.EffectsEngine
{
    public class EffectFrame : IDisposable
    {
        Queue<EffectLayer> over_layers = new Queue<EffectLayer>();
        Queue<EffectLayer> layers = new Queue<EffectLayer>();

        public EffectFrame()
        {

        }

        public void AddLayers(EffectLayer[] effectLayers)
        {
            foreach (EffectLayer layer in effectLayers)
                layers.Enqueue(layer);
        }

        public void AddOverlayLayers(EffectLayer[] effectLayers)
        {
            foreach(EffectLayer layer in effectLayers)
                over_layers.Enqueue(layer);
        }

        public Queue<EffectLayer> GetLayers()
        {
            return new Queue<EffectLayer>(layers);
        }

        public Queue<EffectLayer> GetOverlayLayers()
        {
            return new Queue<EffectLayer>(over_layers);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    over_layers.Clear();
                    layers.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EffectFrame() {
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
