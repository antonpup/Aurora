using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public interface IValueOverridable
    {
        IStringProperty Overrides { get; set; }
    }

    public abstract class LayerHandlerProperties<TProperty> : StringProperty<TProperty>, IValueOverridable where TProperty : LayerHandlerProperties<TProperty>
    {
        [GameStateIgnoreAttribute]
        [JsonIgnore]
        public TProperty Logic { get; set; }
        IStringProperty IValueOverridable.Overrides {
            get => (IStringProperty)Logic;
            set => Logic = value as TProperty;
        }

        [LogicOverridable("Primary Color")]
        public Color? _PrimaryColor { get; set; }
        [JsonIgnore]
        public Color PrimaryColor { get { return Logic._PrimaryColor ?? _PrimaryColor ?? Color.Empty; } }

        [LogicOverridable("Affected Keys")]
        public KeySequence _Sequence { get; set; }
        [JsonIgnore]
        public KeySequence Sequence { get { return Logic._Sequence ?? _Sequence; } }

        // Special opacity design for use with the overrides system.
        [LogicOverridable("Opacity")]
        public float? _Opacity { get; set; }
        [JsonIgnore]
        public float Opacity => Logic._Opacity ?? _Opacity ?? 1f;
        
        // This is a special enabled that is designed only for use with the overrides system
        // and allows the overrides to disable layers on certain conditions.
        // Note that this is NOT the variable that is changed when the user presses the checkbox
        // on an item in the layer list.
        [LogicOverridable("Enabled")]
        public bool? _Enabled { get; set; }
        [JsonIgnore]
        public bool Enabled => Logic._Enabled ?? _Enabled ?? true;

        public LayerHandlerProperties()
        {
            this.Default();
        }

        public LayerHandlerProperties(bool empty = false)
        {
            if (!empty)
                this.Default();
        }

        public virtual void Default()
        {
            Logic = (TProperty)Activator.CreateInstance(typeof(TProperty), new object[] { true });
            _PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
            _Sequence = new KeySequence();
        }
    }

    public class LayerHandlerProperties2Color<TProperty> : LayerHandlerProperties<TProperty> where TProperty : LayerHandlerProperties2Color<TProperty>
    {
        [LogicOverridable("Secondary Color")]
        public Color? _SecondaryColor { get; set; }
        [JsonIgnore]
        public Color SecondaryColor { get { return Logic._SecondaryColor ?? _SecondaryColor ?? Color.Empty; } }

        public LayerHandlerProperties2Color(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _SecondaryColor = Utils.ColorUtils.GenerateRandomColor();
        }
    }

    public class LayerHandlerProperties : LayerHandlerProperties<LayerHandlerProperties>
    {
        public LayerHandlerProperties() : base() { }

        public LayerHandlerProperties(bool assign_default = false) : base(assign_default) { }
    }

    public interface ILayerHandler : IDisposable
    {
        UserControl Control { get; }

        string ID { get; }

        IStringProperty Properties { get; set; }

        bool EnableSmoothing { get; set; }

        bool EnableExclusionMask { get; set; }

        KeySequence ExclusionMask { get; set; }

        float Opacity { get; }
        float? _Opacity { get; set; }

        EffectLayer Render(IGameState gamestate);

        EffectLayer PostRenderFX(EffectLayer layer_render);

        void SetApplication(Application profile);
        void SetGameState(IGameState gamestate);
        void Dispose();
    }

    public abstract class LayerHandler<TProperty> : ILayerHandler where TProperty : LayerHandlerProperties<TProperty>
    {
        [JsonIgnore]
        public Application Application { get; protected set; }

        [JsonIgnore]
        protected UserControl _Control;

        [JsonIgnore]
        public UserControl Control => _Control ?? (_Control = this.CreateControl());

        [JsonIgnore]
        protected string _ID;

        [JsonIgnore]
        public string ID { get { return _ID; } }

        public TProperty Properties { get; set; } = Activator.CreateInstance<TProperty>();

        IStringProperty ILayerHandler.Properties {
            get => Properties;
            set => Properties = value as TProperty;
        }

        public bool EnableSmoothing { get; set; }

        public bool EnableExclusionMask { get; set; }

        public KeySequence ExclusionMask { get; set; }

        public float Opacity => Properties.Opacity;
        public float? _Opacity {
            get => Properties._Opacity;
            set => Properties._Opacity = value;
        }

        //public Color PrimaryColor { get; set; }

        [JsonIgnore]
        private EffectLayer _PreviousRender = new EffectLayer(); //Previous layer

        [JsonIgnore]
        private EffectLayer _PreviousSecondRender = new EffectLayer(); //Layer before previous

        public LayerHandler()
        {
            //Properties = new LayerHandlerProperties();
            //ScriptProperties = new LayerHandlerProperties();
            ExclusionMask = new KeySequence();
        }

        public LayerHandler(LayerHandler other) : base()
        {
            Properties._Sequence = other.Properties._Sequence;
        }

        public virtual EffectLayer Render(IGameState gamestate)
        {
            return new EffectLayer();
        }

        public virtual void SetGameState(IGameState gamestate)
        {

        }

        public EffectLayer PostRenderFX(EffectLayer rendered_layer)
        {
            EffectLayer returnLayer = new EffectLayer(rendered_layer);

            if (EnableSmoothing)
            {
                EffectLayer previousLayer = new EffectLayer(_PreviousRender);
                EffectLayer previousSecondLayer = new EffectLayer(_PreviousSecondRender);

                returnLayer = returnLayer + (previousLayer * 0.50) + (previousSecondLayer * 0.25);

                //Update previous layers
                _PreviousSecondRender = _PreviousRender;
                _PreviousRender = rendered_layer;
            }


            //Last PostFX is exclusion
            if (EnableExclusionMask)
                returnLayer.Exclude(ExclusionMask);

            returnLayer *= Properties.Opacity;

            return returnLayer;
        }

        public virtual void SetApplication(Application profile)
        {
            Application = profile;
        }

        protected virtual UserControl CreateControl()
        {
            return new Control_DefaultLayer();
        }

        public virtual void Dispose()
        {

        }
    }

    public class LayerHandler : LayerHandler<LayerHandlerProperties>
    {

    }
}
