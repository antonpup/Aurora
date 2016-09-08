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
        Default,
        Solid,
        Percent
    }

    /// <summary>
    /// A class representing a default settings layer
    /// </summary>
    public class Layer
    {
        private ProfileManager _profile;

        public event EventHandler AnythingChanged;

        protected string _Name = "New Layer";

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                AnythingChanged?.Invoke(this, null);
            }
        }

        private LayerHandler _Handler = new DefaultLayerHandler();

        public LayerHandler Handler
        {
            get { return _Handler; }
            set
            {
                _Handler = value;
                _Handler.SetProfile(_profile);
            }
        }

        [JsonIgnore]
        public UserControl Control
        {
            get
            {
                return _Handler.Control;
            }
        }

        protected bool _Enabled = true;

        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                _Enabled = value;
                AnythingChanged?.Invoke(this, null);
            }
        }

        protected LayerType _Type;

        public LayerType Type
        {
            get { return _Type; }
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

        public bool LogicPass
        {
            get { return true; } //Check every logic and return whether or not the layer is visible/enabled
        }

        /// <summary>
        /// 
        /// </summary>
        public Layer()
        {
            Logics = new ObservableCollection<LogicItem>();
        }

        public Layer(string name, LayerHandler handler = null) : this()
        {
            Name = name;
            if (handler != null)
                _Handler = handler;
        }

        public void SetProfile(ProfileManager profile)
        {
            _profile = profile;

            _Handler?.SetProfile(_profile);
        }
    }
}
