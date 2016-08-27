using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        protected ObservableCollection<LogicItem> _Logics;

        public ObservableCollection<LogicItem> Logics
        {
            get { return _Logics; }
            set
            {
                _Logics = value;
                AnythingChanged?.Invoke(this, null);
                if (value != null)
                    _Logics.CollectionChanged += (sender, e) => AnythingChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DefaultLayer()
        {
            Control = new Control_DefaultLayer();
            Logics = new ObservableCollection<LogicItem>();
        }

        public DefaultLayer(string name, UserControl control = null) : this()
        {
            Name = name;
            if (control != null)
                Control = control;
        }
    }
}
