using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Windows.Controls;
using FastMember;
using System.ComponentModel;
using System.Windows;
using Application = Aurora.Profiles.Application;

namespace Aurora.Settings.Layers
{
    public abstract class LayerHandlerProperties<TProperty> : IValueOverridable, INotifyPropertyChanged, IDisposable where TProperty : LayerHandlerProperties<TProperty>
    {
        private static readonly Lazy<TypeAccessor> Accessor = new(() => TypeAccessor.Create(typeof(TProperty)));

        public event PropertyChangedEventHandler PropertyChanged;

        [GameStateIgnore, JsonIgnore]
        public TProperty Logic { get; set; }

        [JsonIgnore]
        private Color? _primaryColor;

        [LogicOverridable("Primary Color")]
        public Color? _PrimaryColor
        {
            get => _primaryColor;
            set
            {
                _primaryColor = value;
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(""));
            }
        }

        [JsonIgnore]
        public Color PrimaryColor => Logic?._PrimaryColor ?? _PrimaryColor ?? Color.Empty;

        [JsonIgnore]
        private KeySequence _sequence;

        [LogicOverridable("Affected Keys")]
        public virtual KeySequence _Sequence
        {
            get => _sequence;
            set
            {
                _sequence = value;
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(""));
            }
        }

        [JsonIgnore]
        public KeySequence Sequence => Logic._Sequence ?? _Sequence;


        #region Override Special Properties
        // These properties are special in that they are designed only for use with the overrides system and
        // allows the overrides to access properties not actually present on the Layer.Handler.Properties.
        // Note that this is NOT the variable that is changed when the user changes one of these settings in the
        // UI (for example not changed when an item in the layer list is enabled/disabled with the checkbox).
        [LogicOverridable("Enabled")]
        public bool? _Enabled { get; set; }
        [JsonIgnore]
        public bool Enabled => Logic._Enabled ?? _Enabled ?? true;

        // Renamed to "Layer Opacity" so that if the layer properties needs an opacity for whatever reason, it's
        // less likely to have a name collision.
        [LogicOverridable("Opacity")]
        public float? _LayerOpacity { get; set; }
        [JsonIgnore]
        public float LayerOpacity => Logic._LayerOpacity ?? _LayerOpacity ?? 1f;

        [LogicOverridable("Excluded Keys")]
        public KeySequence _Exclusion { get; set; }
        [JsonIgnore]
        public KeySequence Exclusion => Logic._Exclusion ?? _Exclusion ?? new KeySequence();
        #endregion

        public LayerHandlerProperties()
        {
            Default();
        }

        public LayerHandlerProperties(bool empty = false)
        {
            if (!empty)
                Default();
        }

        public virtual void Default()
        {
            Logic = (TProperty)Activator.CreateInstance(typeof(TProperty), new object[] { true });
            Logic.PropertyChanged += OnPropertiesChanged;
            _PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
            _Sequence = new KeySequence();
            _Sequence.freeform.ValuesChanged += OnPropertiesChanged;
        }

        public object GetOverride(string propertyName) {
            try {
                return Accessor.Value[Logic, propertyName];
            } catch (ArgumentOutOfRangeException) {
                return null;
            }
        }

        public void SetOverride(string propertyName, object value) {
            if (Accessor.Value[Logic, propertyName] == value)
            {
                return;
            }
            try {
                if (value == null || !value.Equals(Accessor.Value[Logic, propertyName]))
                {
                    Accessor.Value[Logic, propertyName] = value;
                }
            } catch (ArgumentOutOfRangeException) { }
        }

        public void OnPropertiesChanged(object sender, object args = null)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(""));
        }

        public void OnPropertiesChanged(object sender)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(""));
        }

        public void Dispose()
        {
            _Sequence.freeform.ValuesChanged -= OnPropertiesChanged;
        }
    }

    public class LayerHandlerProperties2Color<TProperty> : LayerHandlerProperties<TProperty> where TProperty : LayerHandlerProperties2Color<TProperty>
    {
        private Color? _secondaryColor;

        [LogicOverridable("Secondary Color")]
        public Color? _SecondaryColor
        {
            get => _secondaryColor;
            set
            {
                _secondaryColor = value;
                OnPropertiesChanged(null);
            }
        }

        [JsonIgnore]
        public Color SecondaryColor => Logic._SecondaryColor ?? _SecondaryColor ?? Color.Empty;

        public LayerHandlerProperties2Color(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _SecondaryColor = Utils.ColorUtils.GenerateRandomColor();
        }
    }

    public class LayerHandlerProperties : LayerHandlerProperties<LayerHandlerProperties>
    {
        public LayerHandlerProperties()
        { }

        public LayerHandlerProperties(bool assign_default = false) : base(assign_default) { }
    }

    public interface ILayerHandler: IDisposable
    {
        UserControl Control { get; }

        object Properties { get; set; }

        bool EnableSmoothing { get; set; }

        bool EnableExclusionMask { get; }
        bool? _EnableExclusionMask { get; set; }

        KeySequence ExclusionMask { get; }
        KeySequence _ExclusionMask { get; set; }

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
        public UserControl Control => _Control ??= CreateControl();

        private TProperty _properties = Activator.CreateInstance<TProperty>();
        public TProperty Properties
        {
            get => _properties;
            set
            {
                _properties.PropertyChanged -= PropertiesChanged;
                _properties = value;
                value.PropertyChanged += PropertiesChanged;
                value.OnPropertiesChanged(this);
            }
        }

        object ILayerHandler.Properties {
            get => Properties;
            set => Properties = value as TProperty;
        }
        
        public bool EnableSmoothing { get; set; }

        // Always return true if the user is overriding the exclusion zone (so that we don't have to present the user with another
        // option in the overrides asking if they want to enabled/disable it), otherwise if there isn't an overriden value for
        // exclusion, simply return the value of the settings checkbox (as normal)
        [JsonIgnore]
        public bool EnableExclusionMask => Properties.Logic._Exclusion != null || (_EnableExclusionMask ?? false);
        public bool? _EnableExclusionMask { get; set; }

        [JsonIgnore]
        public KeySequence ExclusionMask => Properties.Exclusion;
        public KeySequence _ExclusionMask {
            get => Properties._Exclusion;
            set => Properties._Exclusion = value;
        }

        public float Opacity => Properties.LayerOpacity;
        public float? _Opacity {
            get => Properties._LayerOpacity;
            set => Properties._LayerOpacity = value;
        }

        [JsonIgnore]
        private EffectLayer _previousRender = EffectLayer.EmptyLayer; //Previous layer

        [JsonIgnore]
        private EffectLayer _previousSecondRender = EffectLayer.EmptyLayer; //Layer before previous

        public LayerHandler()
        {
            //ScriptProperties = new LayerHandlerProperties();
            _ExclusionMask = new KeySequence();
            Properties.PropertyChanged += PropertiesChanged;
            WeakEventManager<Effects, CanvasChangedArgs>.AddHandler(null, "CanvasChanged", PropertiesChanged);
        }

        public virtual EffectLayer Render(IGameState gamestate)
        {
            return EffectLayer.EmptyLayer;
        }

        public virtual void SetGameState(IGameState gamestate)
        {

        }

        public EffectLayer PostRenderFX(EffectLayer rendered_layer)
        {
            if (EnableSmoothing)
            {
                EffectLayer returnLayer = new EffectLayer(rendered_layer);
                EffectLayer previousLayer = new EffectLayer(_previousRender);
                EffectLayer previousSecondLayer = new EffectLayer(_previousSecondRender);

                returnLayer = returnLayer + previousLayer * 0.50 + previousSecondLayer * 0.25;

                //Update previous layers
                _previousSecondRender = _previousRender;
                _previousRender = rendered_layer;
                
                //Last PostFX is exclusion
                if (EnableExclusionMask)
                    returnLayer.Exclude(ExclusionMask);
                return returnLayer;
            }

            //Last PostFX is exclusion
            if (EnableExclusionMask)
                rendered_layer.Exclude(ExclusionMask);

            rendered_layer *= Properties.LayerOpacity;

            return rendered_layer;
        }

        public virtual void SetApplication(Application profile)
        {
            Application = profile;
        }

        protected virtual UserControl CreateControl()
        {
            return new Control_DefaultLayer();
        }
        
        protected virtual void PropertiesChanged(object sender, PropertyChangedEventArgs args)
        {
            
        }

        private void PropertiesChanged(object sender, CanvasChangedArgs e)
        {
            PropertiesChanged(sender, new PropertyChangedEventArgs(""));
        }

        public virtual void Dispose()
        {
            Properties.PropertyChanged -= PropertiesChanged;
            WeakEventManager<Effects, CanvasChangedArgs>.RemoveHandler(null, "CanvasChanged", PropertiesChanged);
        }
    }

    [LayerHandlerMeta(Exclude = true)]
    public class LayerHandler : LayerHandler<LayerHandlerProperties>
    {

    }


    public interface IValueOverridable {
        /// <summary>
        /// Gets the overriden value of the speicifed property.
        /// </summary>
        object GetOverride(string propertyName);

        /// <summary>
        /// Sets the overriden value of the speicifed property to the given value.
        /// </summary>
        void SetOverride(string propertyName, object value);
    }
}
