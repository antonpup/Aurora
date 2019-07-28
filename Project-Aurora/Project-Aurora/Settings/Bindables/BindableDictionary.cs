using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Corale.Colore.Annotations;

namespace Aurora.Settings.Bindables
{
    public class BindableDictionary<TKey, TValue> : IBindableDictionary<TKey, TValue>
    {
        public event Action<IEnumerable<KeyValuePair<TKey, TValue>>> ItemsAdded;

        public event Action<IEnumerable<KeyValuePair<TKey, TValue>>> ItemsRemoved;

        public readonly Dictionary<TKey, TValue> Collection;
        
        [UsedImplicitly]
        private BindableDictionary() : this(default)
        {

        }

        public BindableDictionary(Dictionary<TKey, TValue> dictionary = default)
        {
            Collection = dictionary ?? new Dictionary<TKey, TValue>();
        }

        private WeakReference<BindableDictionary<TKey, TValue>> weakReferenceValue;

        private WeakReference<BindableDictionary<TKey, TValue>> weakReference => weakReferenceValue ?? (weakReferenceValue = new WeakReference<BindableDictionary<TKey, TValue>>(this));

        private LockedWeakList<BindableDictionary<TKey, TValue>> bindings;

        public void Parse(object input)
        {
            switch (input)
            {
                case IDictionary<TKey, TValue> dictionary:
                    foreach (var keyValuePair in dictionary)
                    {
                        Add(keyValuePair);
                    }
                    break;
                default:
                    throw new ArgumentException($"Could not parse provided {input.GetType()} ({input}) to {typeof(KeyValuePair<TKey, TValue>)}.");
            }
        }

        public void SetDefault()
        {
            throw new NotImplementedException();
        }

        #region IDictionary
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGetValue(TKey key, out TValue value) => Collection.TryGetValue(key, out value);

        public TValue this[TKey key] { get => Collection[key]; set => Collection[key] = value; }

        public ICollection<TKey> Keys => Collection.Keys;

        public ICollection<TValue> Values => Collection.Values;

        public object this[object key] { get => this[(TKey) key]; set => this[(TKey) key] = (TValue) value; }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear() => Collection.Clear();

        bool ICollection<KeyValuePair<TKey,TValue>>.Contains(KeyValuePair<TKey,TValue> keyValuePair) => Collection.Contains(keyValuePair);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            foreach (var keyValuePair in Collection.Skip(index).Take(array.Length))
            {
                array[index++] = keyValuePair;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool entry = Collection.ContainsKey(item.Key);
            if (entry || !EqualityComparer<TValue>.Default.Equals(Collection[item.Key], item.Value))
                return false;
            Collection.Remove(item.Key);
            ItemsRemoved?.Invoke(new List<KeyValuePair<TKey, TValue>> {item});
            return true;
        }

        public int Count => Collection.Count;

        public bool IsReadOnly => false;

        public bool ContainsKey(TKey key) => Collection.ContainsKey(key);

        public void Add(TKey key, TValue value)
        {
            Collection.Add(key, value);
            ItemsAdded?.Invoke(new List<KeyValuePair<TKey, TValue>> {new KeyValuePair<TKey, TValue>(key, value)});
        }

        public bool Remove(TKey key)
        {
            var value = Collection[key];
            var ret = Collection.Remove(key);
            ItemsRemoved?.Invoke(new List<KeyValuePair<TKey, TValue>> {new KeyValuePair<TKey, TValue>(key, value)});
            return ret;
        }
        #endregion
        
        #region IBindable
        void IBindable.BindTo(IBindable them)
        {
            if (!(them is BindableDictionary<TKey, TValue> tThem))
                throw new InvalidCastException($"Can't bind to a bindable of type {them.GetType()} from a bindable of type {GetType()}.");

            BindTo(tThem);
        }

        void IBindableDictionary<TKey, TValue>.BindTo(IBindableDictionary<TKey, TValue> them)
        {
            if (!(them is BindableDictionary<TKey, TValue> tThem))
                throw new InvalidCastException($"Can't bind to a bindable of type {them.GetType()} from a bindable of type {GetType()}.");

            BindTo(tThem);
        }

        public void BindTo(BindableDictionary<TKey, TValue> them)
        {
            if (them == null)
                throw new ArgumentNullException(nameof(them));
            if (bindings?.Contains(weakReference) ?? false)
                throw new ArgumentException("An already bound collection can not be bound again.");
            if (them == this)
                throw new ArgumentException("A collection can not be bound to itself");

            // copy state and content over
            Parse(them);

            addWeakReference(them.weakReference);
            them.addWeakReference(weakReference);
        }

        private void addWeakReference(WeakReference<BindableDictionary<TKey, TValue>> weakReference)
        {
            if (bindings == null)
                bindings = new LockedWeakList<BindableDictionary<TKey, TValue>>();

            bindings.Add(weakReference);
        }

        private void removeWeakReference(WeakReference<BindableDictionary<TKey, TValue>> weakReference) => bindings?.Remove(weakReference);

        IBindable IBindable.GetBoundCopy()
        {
            return GetBoundCopy();
        }

        IBindableDictionary<TKey, TValue> IBindableDictionary<TKey, TValue>.GetBoundCopy()
        {
            return GetBoundCopy();
        }

        public BindableDictionary<TKey, TValue> GetBoundCopy()
        {
            var copy = (BindableDictionary<TKey, TValue>)Activator.CreateInstance(GetType(), Collection);
            copy.BindTo(this);
            return copy;
        }

        protected void InnerValueChanged()
        {
            ItemsAdded?.Invoke(new KeyValuePair<TKey, TValue>[0]);
        }
        #endregion

    }
}