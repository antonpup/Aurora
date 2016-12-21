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

        public LayerHandlerProperties () {
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

        public LayerHandlerProperties2Color(bool assign_default = false) : base(assign_default) {}

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
        UserControl Control
        {
            get;
        }
        LayerType Type { get; }

        IStringProperty Properties { get; set; }

        EffectLayer Render(IGameState gamestate);

        void SetProfile(ProfileManager profile);
    }

    public abstract class LayerHandler<TProperty> : ILayerHandler where TProperty : LayerHandlerProperties<TProperty>
    {
        //public KeySequence AffectedSequence = new KeySequence();

        [JsonIgnore]
        internal UserControl _Control;

        [JsonIgnore]
        public UserControl Control
        {
            get
            {
                return _Control;
            }
        }

        [JsonIgnore]
        internal LayerType _Type;

        [JsonIgnore]
        public LayerType Type { get { return _Type; } }

        public TProperty Properties { get; set; } = (TProperty)Activator.CreateInstance(typeof(TProperty));

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

        //public Color PrimaryColor { get; set; }


        public LayerHandler()
        {
            //Properties = new LayerHandlerProperties();
            //ScriptProperties = new LayerHandlerProperties();

        }

        public LayerHandler(LayerHandler other) : base()
        {
            Properties._Sequence = other.Properties._Sequence;
        }

        public virtual EffectLayer Render(IGameState gamestate)
        {
            return new EffectLayer();
        }

        public virtual void SetProfile(ProfileManager profile)
        {

        }
    }

    public class LayerHandler : LayerHandler<LayerHandlerProperties>
    {

    }
}
