using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aurora.Utils;

using Corale.Colore.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Settings.Bindables
{
    public class Bindable<T> : IBindable<T>, ISerializableBindable
    {
        [UsedImplicitly]
        private Bindable() : this(default)
        {

        }

        public Bindable(T value = default)
        {
            this.value = value;
        }

        void IBindable.BindTo(IBindable them)
        {
            if (!(them is Bindable<T> tThem))
                throw new InvalidCastException($"Can't bind to a bindable of type {them.GetType()} from a bindable of type {GetType()}.");

            BindTo(tThem);
        }

        void IBindable<T>.BindTo(IBindable<T> them)
        {
            if (!(them is Bindable<T> tThem))
                throw new InvalidCastException($"Can't bind to a bindable of type {them.GetType()} from a bindable of type {GetType()}.");

            BindTo(tThem);
        }
        public Bindable<T> BindTarget
        {
            set => BindTo(value);
        }

        public Bindable<T> GetBoundCopy()
        {
            var copy = (Bindable<T>)Activator.CreateInstance(GetType(), Value);
            copy.BindTo(this);
            return copy;
        }

        IBindable IBindable.GetBoundCopy() => GetBoundCopy();

        IBindable<T> IBindable<T>.GetBoundCopy() => GetBoundCopy();

        private WeakReference<Bindable<T>> weakReferenceValue;

        private WeakReference<Bindable<T>> weakReference => weakReferenceValue ?? (weakReferenceValue = new WeakReference<Bindable<T>>(this));

        protected LockedWeakList<Bindable<T>> Bindings { get; private set; }

        public event Action<ValueChangedEvent<T>> ValueChanged;

        private T value;

        public virtual T Value
        {
            get => value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value)) return;
                SetValue(this.value, value);
            }
        }
        public T Default { get; set; }

        public virtual bool IsDefault => Equals(value, Default);

        public void SetDefault() => Value = Default;

        public virtual void BindTo(Bindable<T> them)
        {
            Value = them.Value;
            Default = them.Default;

            addWeakReference(them.weakReference);
            them.addWeakReference(weakReference);
        }

        public void BindValueChanged(Action<ValueChangedEvent<T>> onChange, bool runOnceImmediatly = false)
        {
            ValueChanged = onChange;
            if(runOnceImmediatly)
                onChange(new ValueChangedEvent<T>(Value, Value));
        }

        private void addWeakReference(WeakReference<Bindable<T>> weakReference)
        {
            if (Bindings == null)
                Bindings = new LockedWeakList<Bindable<T>>();

            Bindings.Add(weakReference);
        }

        private void removeWeakReference(WeakReference<Bindable<T>> weakReference) => Bindings?.Remove(weakReference);

        public virtual void Parse(object input)
        {
            switch (input)
            {
                case T t:
                    Value = t;
                    break;

                case string s:
                    var underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                    if (underlyingType.IsEnum)
                        Value = (T)Enum.Parse(underlyingType, s);
                    else
                        Value = (T)Convert.ChangeType(s, underlyingType, CultureInfo.InvariantCulture);
                    break;

                case JValue v:
                    Value = (T)v.ToObject(typeof(T));
                    break;
                default:
                    throw new ArgumentException($@"Could not parse provided {input.GetType()} ({input}) to {typeof(T)}.");
            }
        }

        internal void SetValue(T previousValue, T value, Bindable<T> source = null)
        {
            this.value = value;
            TriggerValueChanged(previousValue, source ?? this);
        }

        public virtual void TriggerChange()
        {
            TriggerValueChanged(value, this, false);
        }

        public Bindable<T> GetUnboundCopy()
        {
            var clone = GetBoundCopy();
            clone.UnbindAll();
            return clone;
        }

        public virtual void UnbindAll()
        {
            UnbindEvents();
            UnbindBindings();
        }

        protected void TriggerValueChanged(T previousValue, Bindable<T> source, bool propagateToBindings = true)
        {
            T beforePropagation = value;

            if (propagateToBindings && Bindings != null)
            {
                foreach (var b in Bindings)
                {
                    if (b == source) continue;
                    b.SetValue(previousValue, value, this);
                }
            }

            if(EqualityComparer<T>.Default.Equals(beforePropagation, value))
                ValueChanged?.Invoke(new ValueChangedEvent<T>(previousValue, value));
        }

        public void UnbindEvents()
        {
            ValueChanged = null;
        }

        public void UnbindBindings()
        {
            if (Bindings == null) return;

            foreach (var b in Bindings) b.Unbind(this);
            
            Bindings.Clear();
        }

        protected void Unbind(Bindable<T> bindings) => Bindings.Remove(bindings.weakReference);

        void ISerializableBindable.SerializeTo(JsonWriter writer, JsonSerializer serializer)
        {
            serializer.Serialize(writer, Value);
        }

        void ISerializableBindable.DeserializeFrom(JsonReader reader, JsonSerializer serializer)
        {
            Value = serializer.Deserialize<T>(reader);
        }
    }

    [JsonConverter(typeof(BindableJsonConverter))]
    internal interface ISerializableBindable
    {
        void SerializeTo(JsonWriter writer, JsonSerializer serializer);
        void DeserializeFrom(JsonReader reader, JsonSerializer serializer);
    }

    internal class BindableJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(ISerializableBindable).IsAssignableFrom(objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bindable = (ISerializableBindable)value;
            bindable.SerializeTo(writer, serializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!(existingValue is ISerializableBindable bindable))
                bindable = (ISerializableBindable)Activator.CreateInstance(objectType, true);

            bindable.DeserializeFrom(reader, serializer);

            return bindable;
        }
    }
}
