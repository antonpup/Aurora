using System;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Applications.Layers
{
    public abstract class LogicProperties<TProperty> : StringProperty<TProperty> where TProperty : LogicProperties<TProperty>
    {
        [StringPropertyIgnore]
        [JsonIgnore]
        public TProperty Logic = (TProperty)Activator.CreateInstance(typeof(TProperty), new object[] { true });

        /*public Color? _PrimaryColor { get; set; }

        [JsonIgnore]
        public Color PrimaryColor { get { return Logic._PrimaryColor ?? _PrimaryColor ?? Color.Empty; } }*/

        public LogicProperties(bool empty = false)
        {
            if (!empty)
                this.Default();
        }

        public virtual void Default()
        {
            /*_PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
            _Sequence = new KeySequence();*/
        }
    }
}