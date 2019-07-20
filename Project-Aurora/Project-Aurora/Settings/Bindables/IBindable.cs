using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Bindables
{

    public interface IBindable
    {
        void BindTo(IBindable them);

        IBindable GetBoundCopy();

        void Parse(object input);
    }

    public interface IBindable<T> : IBindable
    {
        event Action<ValueChangedEvent<T>> ValueChanged;

        T Value { get; }

        T Default { get; }

        void BindTo(IBindable<T> them);

        void BindValueChanged(Action<ValueChangedEvent<T>> onChange, bool runOnceImmediatly = false);

        new IBindable<T> GetBoundCopy();
    }

    public struct ValueChangedEvent<T>
    {
        public T NewValue;
        public T OldValue;

        public ValueChangedEvent(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
