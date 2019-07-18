using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Aurora.Settings.Bindables;

namespace Aurora.Settings
{
    public abstract class ConfigManager<T> : IDisposable
    {
        protected virtual bool AddMissingEntries => true;

        private readonly IDictionary<T, object> defaultOverrides;

        protected readonly IDictionary<T, IBindable> ConfigStore = new Dictionary<T, IBindable>();

        protected ConfigManager(IDictionary<T, object> defaultOverrides)
        {
            this.defaultOverrides = defaultOverrides;
        }

        protected virtual void InitialiseDefaults()
        {
        }

        public BindableDouble Set(T lookup, double value, double? min = null, double? max = null, double? precision = null)
        {
            value = getDefault(lookup, value);

            if (!(GetOriginalBindable<double>(lookup) is BindableDouble bindable))
            {
                bindable = new BindableDouble(value);
                AddBindable(lookup, bindable);
            }
            else
            {
                bindable.Value = value;
            }

            bindable.Default = value;
            if (min.HasValue) bindable.MinValue = min.Value;
            if (max.HasValue) bindable.MaxValue = max.Value;
            if (precision.HasValue) bindable.Precision = precision.Value;

            return bindable;
        }

        public BindableFloat Set(T lookup, float value, float? min = null, float? max = null, float? precision = null)
        {
            value = getDefault(lookup, value);

            if (!(GetOriginalBindable<float>(lookup) is BindableFloat bindable))
            {
                bindable = new BindableFloat(value);
                AddBindable(lookup, bindable);
            }
            else
            {
                bindable.Value = value;
            }

            bindable.Default = value;
            if (min.HasValue) bindable.MinValue = min.Value;
            if (max.HasValue) bindable.MaxValue = max.Value;
            if (precision.HasValue) bindable.Precision = precision.Value;

            return bindable;
        }

        public BindableInt Set(T lookup, int value, int? min = null, int? max = null)
        {
            value = getDefault(lookup, value);

            if (!(GetOriginalBindable<int>(lookup) is BindableInt bindable))
            {
                bindable = new BindableInt(value);
                AddBindable(lookup, bindable);
            }
            else
            {
                bindable.Value = value;
            }

            bindable.Default = value;
            if (min.HasValue) bindable.MinValue = min.Value;
            if (max.HasValue) bindable.MaxValue = max.Value;

            return bindable;
        }

        public BindableBool Set(T lookup, bool value)
        {
            value = getDefault(lookup, value);

            if (!(GetOriginalBindable<bool>(lookup) is BindableBool bindable))
            {
                bindable = new BindableBool(value);
                AddBindable(lookup, bindable);
            }
            else
            {
                bindable.Value = value;
            }

            bindable.Default = value;

            return bindable;
        }

        public Bindable<U> Set<U>(T lookup, U value)
        {
            if (lookup == null) throw new ArgumentNullException($"Lookup of type {lookup.GetType()} can not be of null");
            value = getDefault(lookup, value);

            Bindable<U> bindable = GetOriginalBindable<U>(lookup);

            if (bindable == null)
                bindable = set(lookup, value);
            else
                bindable.Value = value;

            bindable.Default = value;

            return bindable;
        }

        protected virtual void AddBindable<TBindable>(T lookup, Bindable<TBindable> bindable)
        {
            if(lookup == null) throw new ArgumentNullException($"Lookup of type {lookup.GetType()} can not be of null");
            ConfigStore[lookup] = bindable;
            bindable.ValueChanged += _ => backgroundSave();
        }

        private TType getDefault<TType>(T lookup, TType fallback)
        {
            if (defaultOverrides != null && defaultOverrides.TryGetValue(lookup, out object found))
                return (TType)found;

            return fallback;
        }

        private Bindable<U> set<U>(T lookup, U value)
        {
            Bindable<U> bindable = new Bindable<U>(value);
            AddBindable(lookup, bindable);
            return bindable;
        }

        public U Get<U>(T lookup) => GetOriginalBindable<U>(lookup).Value;

        protected Bindable<U> GetOriginalBindable<U>(T lookup)
        {
            if (ConfigStore.TryGetValue(lookup, out IBindable obj))
            {
                if (!(obj is Bindable<U>))
                    throw new InvalidCastException($"Cannot convert bindable of type {obj.GetType()} retrieved from {nameof(ConfigManager<T>)} to {typeof(Bindable<U>)}.");

                return (Bindable<U>)obj;
            }

            return null;
        }

        public Bindable<U> GetBindable<U>(T lookup) => GetOriginalBindable<U>(lookup)?.GetBoundCopy();

        public void BindWith<U>(T lookup, Bindable<U> bindable) => bindable.BindTo(GetOriginalBindable<U>(lookup));

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                Save();
                isDisposed = true;
            }
        }

        ~ConfigManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool hasLoaded;

        public void Load()
        {
            PerformLoad();
            hasLoaded = true;
        }

        private int lastSave;

        /// <summary>
        /// Perform a save with debounce.
        /// </summary>
        private void backgroundSave()
        {
            var current = Interlocked.Increment(ref lastSave);
            Task.Delay(100).ContinueWith(task =>
            {
                if (current == lastSave) Save();
            });
        }

        private readonly object saveLock = new object();

        public bool Save()
        {
            if (!hasLoaded) return false;

            lock (saveLock)
            {
                Interlocked.Increment(ref lastSave);
                return PerformSave();
            }
        }

        protected abstract void PerformLoad();

        protected abstract bool PerformSave();
    }
}
