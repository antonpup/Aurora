using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides.Logic;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;
using Aurora.Utils;

namespace Aurora.Settings.Layers;

/// <summary>
/// A class representing a default settings layer
/// </summary>
public class Layer : INotifyPropertyChanged, ICloneable, IDisposable
{
    [DoNotNotify, JsonIgnore]
    public Application? AssociatedApplication { get; private set; }

    public string Name { get; set; } = "New Layer";

    [OnChangedMethod(nameof(OnHandlerChanged))]
    public ILayerHandler Handler { get; set; } = new DefaultLayerHandler();

    [JsonIgnore]
    public UserControl Control => Handler.Control;

    public bool Enabled { get; set; } = true;

    public Dictionary<string, IOverrideLogic>? OverrideLogic { get; set; }
    // private void OnOverrideLogicChanged() => // Make the logic collection changed event trigger a property change to ensure it gets saved?

    #region Constructors
    public Layer() { }

    public Layer(string name, ILayerHandler? handler = null) : this() {
        Name = name;
        Handler = handler ?? Handler;
    }

    private Layer(string name, ILayerHandler handler, Dictionary<string, IOverrideLogic> overrideLogic) : this(name, handler) {
        OverrideLogic = overrideLogic;
    }

    public Layer(string name, ILayerHandler handler, OverrideLogicBuilder builder) : this(name, handler, builder.Create()) { }
    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnHandlerChanged() {
        if (AssociatedApplication != null)
            Handler.SetApplication(AssociatedApplication);
    }

    public EffectLayer Render(IGameState gs) {
        if (OverrideLogic != null)
        {
            // For every property which has an override logic assigned
            foreach (var kvp in OverrideLogic)
                // Set the value of the logic evaluation as the override for this property
            {
                var value = kvp.Value.Evaluate(gs);
                if (kvp.Value.VarType is { IsEnum: true })
                {
                    ((IValueOverridable)Handler.Properties).SetOverride(kvp.Key,
                        value == null ? null : Enum.ToObject(kvp.Value.VarType, value));
                }
                else
                {
                    ((IValueOverridable)Handler.Properties).SetOverride(kvp.Key, value);
                }
            }
        }

        var effectLayer = ((dynamic)Handler.Properties).Enabled ? Handler.PostRenderFX(Handler.Render(gs)) : EffectLayer.EmptyLayer;
        return effectLayer;
    }

    public void SetProfile(Application profile) {
        AssociatedApplication = profile;
        Handler.SetApplication(AssociatedApplication);
    }

    public object Clone() {
        var str = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        return JsonConvert.DeserializeObject(
            str,
            GetType(),
            new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new AuroraSerializationBinder()
            }
        )!;
    }

    public void SetGameState(IGameState newGameState) => Handler.SetGameState(newGameState);
    public void Dispose() => Handler.Dispose();
}

/// <summary>
/// Interface for layers that fire an event when the layer is rendered.<para/>
/// To use, the layer handler should call <code>LayerRender?.Invoke(this, layer.GetBitmap());</code> at the end of their <see cref="Layer.Render(IGameState)"/> method.
/// </summary>
public interface INotifyRender {
    event EventHandler<Bitmap> LayerRender;
}