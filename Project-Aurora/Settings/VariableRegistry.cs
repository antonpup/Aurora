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

        public VariableRegistryItem()
        {
        }

        public VariableRegistryItem(object defaultValue, object max = null, object min = null, string title = "", string remark = "")
        {
            this.Value = defaultValue;
            this.Default = defaultValue;

            if (this.Value != null && max != null && this.Value.GetType() == max.GetType())
                this.Max = max;

            if (this.Value != null && min != null && this.Value.GetType() == min.GetType())
                this.Min = min;

            this.Title = title;
            this.Remark = remark;
        }

        public void SetVariable(object newvalue)
        {
            if (this.Value != null && newvalue != null && this.Value.GetType() == newvalue.GetType())
            {
                this.Value = newvalue;
            }
        }
    }

    public class VariableRegistry //Might want to implement something like IEnumerable here
    {
        [JsonProperty("Variables")]
        private Dictionary<string, VariableRegistryItem> _variables;

        public int Count { get { return _variables.Count; } }

        public VariableRegistry()
        {
            _variables = new Dictionary<string, VariableRegistryItem>();
        }

        public void Combine(VariableRegistry otherRegistry)
        {
            foreach(var variable in otherRegistry._variables)
                Register(variable.Key, variable.Value);
        }

        public string[] GetRegisteredVariableKeys()
        {
            return _variables.Keys.ToArray();
        }

        public void Register(string name, object defaultValue, string title = "", object max = null, object min = null, string remark = "")
        {
            if (!_variables.ContainsKey(name))
                _variables.Add(name, new VariableRegistryItem(defaultValue, max, min, title, remark));
        }

        public void Register(string name, VariableRegistryItem varItem)
        {
            if (!_variables.ContainsKey(name))
                _variables.Add(name, varItem);
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
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Value != null && _variables[name].Value.GetType() == typeof(T))
                return (T)_variables[name].Value;

            return Activator.CreateInstance<T>();
        }

        public T GetVariableDefault<T>(string name)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Default != null && _variables[name].Default.GetType() == typeof(T))
                return (T)_variables[name].Default;

            return Activator.CreateInstance<T>();
        }

        public bool GetVariableMax<T>(string name, out T value)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Max != null && _variables[name].Max.GetType() == typeof(T))
            {
                value = (T)_variables[name].Max;
                return true;
            }

            value = Activator.CreateInstance<T>();
            return false;
        }

        public bool GetVariableMin<T>(string name, out T value)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Min != null && _variables[name].Min.GetType() == typeof(T))
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
    }
}
