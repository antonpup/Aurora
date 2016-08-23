using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public enum LayerType
    {
        Solid,
        Percent
    }

    /// <summary>
    /// A class representing a default settings layer
    /// </summary>
    public class DefaultLayer
    {
        public event EventHandler AnythingChanged;

        protected string _Name = "New Layer";

        public string Name {
            get { return _Name; }
            set
            {
                _Name = value;
                AnythingChanged?.Invoke(this, null);
            }
        }
        
        [JsonIgnore]
        public UserControl Control { get; set; }

        protected bool _Enabled = true;

        public bool Enabled { get { return _Enabled; }
            set {
                _Enabled = value;
                AnythingChanged?.Invoke(this, null);
            }
        }

        protected LayerType _Type;

        public LayerType Type { get { return _Type; }
            set
            {
                _Type = value;
                AnythingChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DefaultLayer()
        {
            Control = new Control_DefaultLayer();
        }

        public DefaultLayer(string name, UserControl control = null)
        {
            Name = name;
            Control = control ?? new Control_DefaultLayer();
        }
    }
}
