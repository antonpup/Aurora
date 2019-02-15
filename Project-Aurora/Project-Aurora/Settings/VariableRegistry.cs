using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class VariableRegistryItem
    {
        public object Value = null;
        private object @default = null;
        public object Default { get { return @default?.TryClone(); } set { @default = value?.TryClone(); } }
        public object Max = null;
        public object Min = null;
        public string Title = "";
        public string Remark = "";
        public VariableFlags Flags = VariableFlags.None;

        public VariableRegistryItem()
        {
        }

        public VariableRegistryItem(object defaultValue, object max = null, object min = null, string title = "", string remark = "", VariableFlags flags = VariableFlags.None)
        {
            this.Value = defaultValue;
            this.Default = defaultValue;

            if (this.Value != null && max != null && this.Value.GetType() == max.GetType())
                this.Max = max;

            if (this.Value != null && min != null && this.Value.GetType() == min.GetType())
                this.Min = min;

            this.Title = title;
            this.Remark = remark;
            this.Flags = flags;
        }

        public void SetVariable(object newvalue)
        {
            if (this.Value != null && newvalue != null && this.Value.GetType() == newvalue.GetType())
            {
                this.Value = newvalue;
            }
        }

        internal void Merge(VariableRegistryItem variableRegistryItem)
        {
            this.Default = variableRegistryItem.Default;
            this.Title = variableRegistryItem.Title;
            this.Remark = variableRegistryItem.Remark;
            this.Min = variableRegistryItem.Min;
            this.Max = variableRegistryItem.Max;
            Type typ = this.Value.GetType();
            Type defaultType = variableRegistryItem.Default.GetType();

            if (!defaultType.Equals(typ) && typ.Equals(typeof(long)) && defaultType.IsEnum)
                this.Value = Enum.ToObject(defaultType, Value);
            else if (!defaultType.Equals(typ) && this.Value.GetType().Equals(typeof(long)) && TypeUtils.IsNumericType(defaultType))
                this.Value = Convert.ChangeType(this.Value, defaultType);
            else if (this.Value == null && !defaultType.Equals(typ))
                this.Value = variableRegistryItem.Default;
            this.Flags = variableRegistryItem.Flags;
        }
    }

    public enum VariableFlags
    {
        None = 0,
        UseHEX = 1
    }

    public class VariableRegistry : ICloneable //Might want to implement something like IEnumerable here
    {
        [JsonProperty("Variables")]
        private Dictionary<string, VariableRegistryItem> _variables;

        [JsonIgnore]
        public int Count { get { return _variables.Count; } }

        public VariableRegistry()
        {
            _variables = new Dictionary<string, VariableRegistryItem>();
        }

        public void Combine(VariableRegistry otherRegistry, bool removeMissing = false)
        {
            //Below doesn't work for added variables
            Dictionary<string, VariableRegistryItem> vars = new Dictionary<string, VariableRegistryItem>();

            foreach (var variable in otherRegistry._variables)
            {
                if (removeMissing)
                {
                    VariableRegistryItem local = _variables.ContainsKey(variable.Key) ? _variables[variable.Key] : null;
                    if (local != null)
                        local.Merge(variable.Value);
                    else
                        local = variable.Value;

                    vars.Add(variable.Key, local);
                }
                else
                    Register(variable.Key, variable.Value);
            }

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
            }
        }

        public T GetVariable<T>(string name)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Value != null && typeof(T).IsAssignableFrom(_variables[name].Value.GetType()))
                return (T)_variables[name].Value;

            return default(T);
        }

        public T GetVariableDefault<T>(string name)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Default != null && typeof(T).IsAssignableFrom(_variables[name].Value.GetType()))
                return (T)_variables[name].Default;

            return Activator.CreateInstance<T>();
        }

        public bool GetVariableMax<T>(string name, out T value)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Max != null && typeof(T).IsAssignableFrom(_variables[name].Value.GetType()))
            {
                value = (T)_variables[name].Max;
                return true;
            }

            value = Activator.CreateInstance<T>();
            return false;
        }

        public bool GetVariableMin<T>(string name, out T value)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Min != null && typeof(T).IsAssignableFrom(_variables[name].Value.GetType()))
            {
                value = (T)_variables[name].Min;
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
            string str = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Binder = Aurora.Utils.JSONUtils.SerializationBinder });

            return JsonConvert.DeserializeObject(
                    str,
                    this.GetType(),
                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All, Binder = Aurora.Utils.JSONUtils.SerializationBinder }
                    );
        }
    }
}
