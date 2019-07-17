using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Corale.Colore.Annotations;

namespace Aurora.Settings.Bindables
{
    public class Bindable<T> : IBindable<T>
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
        
        internal void SetValue(T previousValue, T value, Bindable<T> source = null)
        {
            this.value = value;
            TriggerValueChanged(previousValue, source ?? this);
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


    }
}
