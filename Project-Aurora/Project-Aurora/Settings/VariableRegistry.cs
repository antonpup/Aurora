using System;
using System.Collections.Generic;
using System.Linq;

using Aurora.Settings.Bindables;
using Aurora.Utils;

using Newtonsoft.Json;

namespace Aurora.Settings
{
    public class VariableRegistryItem
    {
        public object Value;
        private object @default;
        public object Default { get => @default?.TryClone(); set => @default = value?.TryClone(); }
        public object Max;
        public object Min;
        public string Title = "";
        public string Remark = "";
        public VariableFlags Flags = VariableFlags.None;
        [JsonIgnore]
        public IBindable Bindable { get; set; }

        public VariableRegistryItem() { }

        public VariableRegistryItem(IBindable bindable, string title = "", string remark = "", VariableFlags flags = VariableFlags.None)
        {
            Bindable = bindable;
            switch (Bindable)
            {
                case Bindable<bool> b:
                    Value = b.Value;
                    Default = b.Default;
                    break;
                case BindableNumber<double> b:
                    Value = b.Value;
                    Default = b.Default;
                    Max = b.MaxValue;
                    Min = b.MinValue;
                    break;
                case BindableNumber<float> b:
                    Value = b.Value;
                    Default = b.Default;
                    Max = b.MaxValue;
                    Min = b.MinValue;
                    break;
                case BindableNumber<int> b:
                    Value = b.Value;
                    Default = b.Default;
                    Max = b.MaxValue;
                    Min = b.MinValue;
                    break;
                case BindableNumber<long> b:
                    Value = b.Value;
                    Default = b.Default;
                    Max = b.MaxValue;
                    Min = b.MinValue;
                    break;
                case BindableColor b:
                    Value = b.Value;
                    Default = b.Default;
                    break;
            }
            Title = title;
            Remark = remark;
            Flags = flags;
        }

        public VariableRegistryItem(object defaultValue, object max = null, object min = null, string title = "", string remark = "", VariableFlags flags = VariableFlags.None)
        {
            Value = defaultValue;
            Default = defaultValue;

            if (Value != null && max != null && Value.GetType() == max.GetType())
                Max = max;

            if (Value != null && min != null && Value.GetType() == min.GetType())
                Min = min;

            Title = title;
            Remark = remark;
            Flags = flags;
        }

        public void SetVariable(object newvalue)
        {
            if (Value != null && newvalue != null && Value.GetType() == newvalue.GetType())
            {
                if (Bindable != null)
                    switch (Bindable)
                    {
                        case Bindable<bool> b:
                            b.Value = (bool) newvalue;
                            break;
                        case BindableNumber<double> b:
                            b.Value = (double) newvalue;
                            break;
                        case BindableNumber<float> b:
                            b.Value = (float) newvalue;
                            break;
                        case BindableNumber<int> b:
                            b.Value = (int) newvalue;
                            break;
                        case BindableNumber<long> b:
                            b.Value = (long) newvalue;
                            break;
                        case BindableColor b:
                            b.Value = (RealColor) newvalue;
                            break;
                    }
                Value = newvalue;
            }
        }

        internal void Merge(VariableRegistryItem variableRegistryItem)
        {
            try
            {
                Default = variableRegistryItem.Default;
                Title = variableRegistryItem.Title;
                Remark = variableRegistryItem.Remark;
                Min = variableRegistryItem.Min;
                Max = variableRegistryItem.Max;
                Bindable = variableRegistryItem.Bindable;
                var typ = Value.GetType();
                var defaultType = variableRegistryItem.Default.GetType();

                if (!defaultType.Equals(typ) && typ.Equals(typeof(long)) && defaultType.IsEnum)
                    Value = Enum.ToObject(defaultType, Value);
                else if (!defaultType.Equals(typ) && Value.GetType().Equals(typeof(long)) && TypeUtils.IsNumericType(defaultType))
                    Value = Convert.ChangeType(Value, defaultType);
                else if (Value == null && !defaultType.Equals(typ))
                    Value = variableRegistryItem.Default;
                Flags = variableRegistryItem.Flags;
                if (Bindable != null)
                    switch (Bindable)
                    {
                        case Bindable<bool> b:
                            Value = b.Value;
                            b.ValueChanged += _ => ValueChanged();
                            break;
                        case BindableNumber<double> b:
                            Value = b.Value;
                            b.ValueChanged += _ => ValueChanged();
                            break;
                        case BindableNumber<float> b:
                            Value = b.Value;
                            b.ValueChanged += _ => ValueChanged();
                            break;
                        case BindableNumber<int> b:
                            Value = b.Value;
                            b.ValueChanged += _ => ValueChanged();
                            break;
                        case BindableNumber<long> b:
                            Value = b.Value;
                            b.ValueChanged += _ => ValueChanged();
                            break;
                        case BindableColor b:
                            Value = b.Value;
                            b.ValueChanged += _ => ValueChanged();
                            break;
                    }
            }
            catch (Exception e)
            {
                Global.logger.Error($"Exception in merging of VariableRegisties: {e.ToString()}");
            }
        }

        internal void ValueChanged()
        {
            switch (Bindable)
            {
                case Bindable<bool> b:
                    Value = b.Value;
                    break;
                case BindableNumber<double> b:
                    Value = b.Value;
                    break;
                case BindableNumber<float> b:
                    Value = b.Value;
                    break;
                case BindableNumber<int> b:
                    Value = b.Value;
                    break;
                case BindableNumber<long> b:
                    Value = b.Value;
                    break;
                case BindableColor b:
                    Value = b.Value;
                    break;
            }
        }
    }

    public enum VariableFlags
    {
        None = 0,
        UseHEX = 1
    }

    public class VariableRegistry : ICloneable //Might want to implement something like IEnumerable here
    {
        [JsonProperty("Variables")] private Dictionary<string, VariableRegistryItem> _variables;

        [JsonIgnore]
        public int Count => _variables.Count;

        public VariableRegistry()
        {
            _variables = new Dictionary<string, VariableRegistryItem>();
        }

        public void Combine(VariableRegistry otherRegistry, bool removeMissing = false)
        {
            //Below doesn't work for added variables
            var vars = new Dictionary<string, VariableRegistryItem>();

            foreach (var variable in otherRegistry._variables)
                if (removeMissing)
                {
                    var local = _variables.ContainsKey(variable.Key) ? _variables[variable.Key] : null;
                    if (local != null)
                        local.Merge(variable.Value);
                    else
                        local = variable.Value;

                    vars.Add(variable.Key, local);
                }
                else
                    Register(variable.Key, variable.Value);

            if (removeMissing)
                _variables = vars;
        }

        public string[] GetRegisteredVariableKeys()
        {
            return _variables.Keys.ToArray();
        }

        public void Register(string name, object defaultValue, string title = "", object max = null, object min = null, string remark = "", VariableFlags flags = VariableFlags.None)
        {
            if (!_variables.ContainsKey(name))
                _variables.Add(name, new VariableRegistryItem(defaultValue, max, min, title, remark, flags));
        }

        public void Register(string name, IBindable bindable, string title = "", string remark = "", VariableFlags flags = VariableFlags.None)
        {
            if (!_variables.ContainsKey(name))
                _variables.Add(name, new VariableRegistryItem(bindable, title, remark, flags));
            else
                _variables[name].Bindable = bindable;
        }

        public void Register(string name, VariableRegistryItem varItem)
        {
            if (!_variables.ContainsKey(name))
                _variables.Add(name, varItem);
            else
                _variables[name].Merge(varItem);
        }

        public bool SetVariable(string name, object variable)
        {
            if (_variables.ContainsKey(name))
            {
                _variables[name].SetVariable(variable);
                return true;
            }

            return false;
        }

        public void ResetVariable(string name)
        {
            if (_variables.ContainsKey(name))
            {
                _variables[name].Value = _variables[name].Default;
                _variables[name].Bindable.SetDefault();
            }
        }

        public T GetVariable<T>(string name)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Value != null && typeof(T).IsAssignableFrom(_variables[name].Value.GetType()))
                return (T) _variables[name].Value;

            return default;
        }

        public T GetVariableDefault<T>(string name)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Default != null && typeof(T).IsAssignableFrom(_variables[name].Value.GetType()))
                return (T) _variables[name].Default;

            return Activator.CreateInstance<T>();
        }

        public bool GetVariableMax<T>(string name, out T value)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Max != null && typeof(T).IsAssignableFrom(_variables[name].Value.GetType()))
            {
                value = (T) _variables[name].Max;
                return true;
            }

            value = Activator.CreateInstance<T>();
            return false;
        }

        public bool GetVariableMin<T>(string name, out T value)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Min != null && typeof(T).IsAssignableFrom(_variables[name].Value.GetType()))
            {
                value = (T) _variables[name].Min;
                return true;
            }

            value = Activator.CreateInstance<T>();
            return false;
        }

        public Type GetVariableType(string name)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Value != null)
                return _variables[name].Value.GetType();

            return typeof(object);
        }

        public string GetTitle(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name].Title;

            return "";
        }

        public string GetRemark(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name].Remark;

            return "";
        }

        public VariableFlags GetFlags(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name].Flags;

            return VariableFlags.None;
        }

        public void RemoveVariable(string name)
        {
            if (_variables.ContainsKey(name))
                _variables.Remove(name);
        }

        public object Clone()
        {
            var str = JsonConvert.SerializeObject(this, Formatting.None, Global.SerializerSettings);

            return JsonConvert.DeserializeObject(str,GetType(),Global.SerializerSettings);
        }
    }
}