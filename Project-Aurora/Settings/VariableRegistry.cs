using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class VariableRegistryItem
    {
        public object variable = null;
        public object variable_default = null;
        public object variable_max = null;
        public object variable_min = null;
        public string variable_title = "";
        public string variable_remark = "";

        public VariableRegistryItem()
        {
        }

        public VariableRegistryItem(object variable, object variable_max = null, object variable_min = null, string variable_title = "", string variable_remark = "")
        {
            this.variable = variable;
            this.variable_default = variable;

            if (this.variable != null && variable_max != null && this.variable.GetType() == variable_max.GetType())
                this.variable_max = variable_max;

            if (this.variable != null && variable_min != null && this.variable.GetType() == variable_min.GetType())
                this.variable_min = variable_min;

            this.variable_title = variable_title;
            this.variable_remark = variable_remark;
        }

        public void SetVariable(object newvalue)
        {
            if (this.variable != null && newvalue != null && this.variable.GetType() == newvalue.GetType())
            {
                this.variable = newvalue;
            }
        }
    }

    public class VariableRegistry
    {
        [JsonProperty("Variables")]
        private Dictionary<string, VariableRegistryItem> _variables;

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

        public void Register(string name, object default_variable, string title = "", object max_variable = null, object min_variable = null, string remark = "")
        {
            if (!_variables.ContainsKey(name))
                _variables.Add(name, new VariableRegistryItem(default_variable, max_variable, min_variable, title, remark));
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
                _variables[name].variable = _variables[name].variable_default;
        }

        public T GetVariable<T>(string name)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].variable != null && _variables[name].variable.GetType() == typeof(T))
                return (T)_variables[name].variable;

            return Activator.CreateInstance<T>();
        }

        public T GetVariableDefault<T>(string name)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].variable_default != null && _variables[name].variable_default.GetType() == typeof(T))
                return (T)_variables[name].variable_default;

            return Activator.CreateInstance<T>();
        }

        public bool GetVariableMax<T>(string name, out T value)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].variable_max != null && _variables[name].variable_max.GetType() == typeof(T))
            {
                value = (T)_variables[name].variable_max;
                return true;
            }

            value = Activator.CreateInstance<T>();
            return false;
        }

        public bool GetVariableMin<T>(string name, out T value)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].variable_min != null && _variables[name].variable_min.GetType() == typeof(T))
            {
                value = (T)_variables[name].variable_min;
                return true;
            }

            value = Activator.CreateInstance<T>();
            return false;
        }

        public Type GetVariableType(string name)
        {
            if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].variable != null)
                return _variables[name].variable.GetType();

            return typeof(object);
        }

        public string GetTitle(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name].variable_title;

            return "";
        }

        public string GetRemark(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name].variable_remark;

            return "";
        }
    }
}
