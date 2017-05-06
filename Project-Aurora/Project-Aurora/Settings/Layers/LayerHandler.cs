using Aurora.EffectsEngine;
using Aurora.Profiles;
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
    public interface ILogic
    {
        IStringProperty Logic { get; set; }
    }

    public abstract class LayerHandlerProperties<TProperty> : StringProperty<TProperty>, ILogic where TProperty : LayerHandlerProperties<TProperty>
    {
        [GameStateIgnoreAttribute]
        [JsonIgnore]
        public TProperty Logic { get; set; }

        IStringProperty ILogic.Logic
        {
            get
            {
                return (IStringProperty)this.Logic;
            }
            set
            {
                this.Logic = value as TProperty;
            }
        }

        public Color? _PrimaryColor { get; set; }

        [JsonIgnore]
        public Color PrimaryColor { get { return Logic._PrimaryColor ?? _PrimaryColor ?? Color.Empty; } }

        public KeySequence _Sequence { get; set; }

        [JsonIgnore]
        public KeySequence Sequence { get { return Logic._Sequence ?? _Sequence; } }

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

    public interface ILayerHandler
    {
        UserControl Control { get; }

        string ID { get; }

        IStringProperty Properties { get; set; }

        bool EnableSmoothing { get; set; }

        bool EnableExclusionMask { get; set; }

        KeySequence ExclusionMask { get; set; }

        float Opacity { get; set; }

        EffectLayer Render(IGameState gamestate);

        EffectLayer PostRenderFX(EffectLayer layer_render);

        void SetProfile(ProfileManager profile);
    }

    public abstract class LayerHandler<TProperty> : ILayerHandler where TProperty : LayerHandlerProperties<TProperty>
    {
        [JsonIgnore]
        public ProfileManager Profile { get; protected set; }

        [JsonIgnore]
        protected UserControl _Control;

        [JsonIgnore]
        public UserControl Control
        {
            get
            {
                return _Control ?? (_Control = this.CreateControl());
            }
        }

        [JsonIgnore]
        protected string _ID;

        [JsonIgnore]
        public string ID { get { return _ID; } }

        public TProperty Properties { get; set; } = Activator.CreateInstance<TProperty>();

        IStringProperty ILayerHandler.Properties
        {
            get
            {
                return Properties;
            }

            set
            {
                Properties = value as TProperty;
            }
        }

        public bool EnableSmoothing { get; set; }

        public bool EnableExclusionMask { get; set; }

        public KeySequence ExclusionMask { get; set; }

        public float Opacity { get; set; }

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
            Opacity = 1.0f;
        }

        public LayerHandler(LayerHandler other) : base()
        {
            Properties._Sequence = other.Properties._Sequence;
        }

        public virtual EffectLayer Render(IGameState gamestate)
        {
            return new EffectLayer();
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

            returnLayer *= Opacity;

            return returnLayer;
        }

        public virtual void SetProfile(ProfileManager profile)
        {
            Profile = profile;
        }

        protected virtual UserControl CreateControl()
        {
            return new Control_DefaultLayer();
        }
    }

    public class LayerHandler : LayerHandler<LayerHandlerProperties>
    {

    }
}
