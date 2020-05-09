using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using FastMember;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    /// <summary>
    /// A class representing a default settings layer
    /// </summary>
    public class Layer : INotifyPropertyChanged, ICloneable, IDisposable {

        public string Name { get; set; } = "New Layer";
        public bool Enabled { get; set; } = true;
        [JsonIgnore] public Application AssociatedApplication { get; private set; }
        [JsonIgnore] public UserControl Control => Handler.Control;

        #region LayerHandler
        [OnChangedMethod(nameof(OnHandlerChanged))]
        public ILayerHandler Handler { get; set; } = new DefaultLayerHandler();
        private void OnHandlerChanged() => Handler.SetApplication(AssociatedApplication);
        #endregion

        #region OverrideLogic
        [OnChangedMethod(nameof(OnOverrideLogicChanged))]
        public Dictionary<string, IOverrideLogic> OverrideLogic { get; set; }
        private void OnOverrideLogicChanged(object before, object after) {
            // TODO: Make override logic trigger a property changed event when an override changes to trigger a save
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public Layer() { }

        public Layer(string name, ILayerHandler handler = null) {
            Name = name;
            Handler = handler ?? new DefaultLayerHandler();
        }

        public Layer(string name, ILayerHandler handler, Dictionary<string, IOverrideLogic> overrideLogic) : this(name, handler) {
            OverrideLogic = overrideLogic;
        }

        public Layer(string name, ILayerHandler handler, OverrideLogicBuilder builder) : this(name, handler, builder.Create()) { }

        public EffectLayer Render(IGameState gs) {
            if (OverrideLogic != null) {

                // Create an accessor for this override instance
                var target = ((IValueOverridable)Handler.Properties).Overrides;
                var accessor = TypeAccessor.Create(target.GetType());

                // For every property which has an override logic assigned
                foreach (var kvp in OverrideLogic) {
                    // Set the value of the logic evaluation as the override for this property
                    accessor[target, kvp.Key] = kvp.Value.Evaluate(gs);
                }
            }
            
            return ((dynamic)Handler.Properties).Enabled ? Handler.PostRenderFX(Handler.Render(gs)) : new EffectLayer();
        }

        public void SetProfile(Application profile) {
            AssociatedApplication = profile;
            Handler?.SetApplication(AssociatedApplication);
        }

        public void SetGameState(IGameState new_game_state) => Handler.SetGameState(new_game_state);

        public object Clone() {
            string str = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Binder = Aurora.Utils.JSONUtils.SerializationBinder });
            return JsonConvert.DeserializeObject(
                str,
                this.GetType(),
                new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All, Binder = Aurora.Utils.JSONUtils.SerializationBinder }
            );
        }

        public void Dispose() => Handler.Dispose();
    }

    /// <summary>
    /// Interface for layers that fire an event when the layer is rendered.<para/>
    /// To use, the layer handler should call <code>LayerRender?.Invoke(this, layer.GetBitmap());</code> at the end of their <see cref="Layer.Render(IGameState)"/> method.
    /// </summary>
    public interface INotifyRender {
        event EventHandler<Bitmap> LayerRender;
    }
}
