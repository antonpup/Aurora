using System;
using System.Collections.Generic;

namespace Aurora.EffectsEngine;

/// <summary>
/// A class representative of one frame. Contains a list of EffectLayers as layers and overlay layers.
/// </summary>
public sealed class EffectFrame : IDisposable
{
    private readonly Queue<EffectLayer> _overLayers = new();
    private readonly Queue<EffectLayer> _layers = new();

    /// <summary>
    /// Adds layers into the frame
    /// </summary>
    /// <param name="effectLayers">Array of layers to be added</param>
    public void AddLayers(IEnumerable<EffectLayer> effectLayers)
    {
        foreach (var layer in effectLayers)
            AddLayer(layer);
    }

    public void AddLayer(EffectLayer layer)
    {
        _layers.Enqueue(layer);
    }

    /// <summary>
    /// Add overlay layers into the frame
    /// </summary>
    /// <param name="effectLayers">Array of layers to be added</param>
    public void AddOverlayLayers(IEnumerable<EffectLayer> effectLayers)
    {
        foreach(var layer in effectLayers)
            AddOverlayLayer(layer);
    }

    /// <summary>
    /// Add overlay layer into the frame
    /// </summary>
    /// <param name="effectLayer">Array of layers to be added</param>
    public void AddOverlayLayer(EffectLayer effectLayer)
    {
        _overLayers.Enqueue(effectLayer);
    }

    /// <summary>
    /// Gets the queue of layers
    /// </summary>
    /// <returns>Queue of layers</returns>
    public Queue<EffectLayer> GetLayers()
    {
        return _layers;
    }

    /// <summary>
    /// Gets the queue of overlay layers
    /// </summary>
    /// <returns>Queue of overlay layers</returns>
    public Queue<EffectLayer> GetOverlayLayers()
    {
        return _overLayers;
    }

    public void Dispose()
    {
        _overLayers.Clear();
        _layers.Clear();
    }
}