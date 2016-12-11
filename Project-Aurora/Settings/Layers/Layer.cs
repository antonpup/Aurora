﻿using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public enum LayerType
    {
        [Description("Default Layer")]
        Default = 0,

        [Description("Solid Color Layer")]
        Solid = 100,

        [Description("Solid Fill Color Layer")]
        SolidFilled = 110,

        [Description("Percent Effect Layer")]
        Percent = 200,

        [Description("Interactive Layer")]
        Interactive = 300,

        [Description("Shortcut Assistant Layer")]
        ShortcutAssistant = 400,

        [Description("Equalizer Layer")]
        Equalizer = 500,

        [Description("Ambilight Layer")]
        Ambilight = 600,

        [Description("Lock Color Layer")]
        LockColor = 700,

        [Description("Dota 2 Background Layer")]
        Dota2Background = 800,

        [Description("Dota 2 Respawn Layer")]
        Dota2Respawn = 801,

        [Description("Dota 2 Abilies Layer")]
        Dota2Abilities = 802,

        [Description("Dota 2 Items Layer")]
        Dota2Items = 803,

        [Description("Dota 2 Hero Abiliy Effects Layer")]
        Dota2HeroAbiltiyEffects = 804,

        [Description("Dota 2 Killstreak Layer")]
        Dota2Killstreak = 805,

    }

    /// <summary>
    /// A class representing a default settings layer
    /// </summary>
    public class Layer
    {
        private ProfileManager _profile;

        [JsonIgnore]
        public ProfileManager AssociatedProfile { get { return _profile; } }

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

        private ILayerHandler _Handler = new DefaultLayerHandler();

        public ILayerHandler Handler
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
            // Basic colour changing logic
            /* {
                new LogicItem {
                    Action = new Tuple<LogicItem.ActionType, object>(
                        LogicItem.ActionType.SetProperty,
                        new Tuple<string, object>(
                            "_PrimaryColor",
                            new RealColor(Color.FromArgb(Color.Blue.A, Color.Blue.R, Color.Blue.G, Color.Blue.B))
                       )
                    ),
                    ReferenceComparisons = new Dictionary<string, Tuple<LogicItem.LogicOperator, object>>
                    {
                        {
                            "LocalPCInfo/CurrentSecond",
                            new Tuple<LogicItem.LogicOperator, object>(
                                LogicItem.LogicOperator.GreaterThan,
                                45
                            )
                        }
                    }
                },
                new LogicItem {
                    Action = new Tuple<LogicItem.ActionType, object>(
                        LogicItem.ActionType.SetProperty,
                        new Tuple<string, object>(
                            "_PrimaryColor",
                            new RealColor(Color.FromArgb(Color.Red.A, Color.Red.R, Color.Red.G, Color.Red.B))
                        )
                    ),
                    ReferenceComparisons = new Dictionary<string, Tuple<LogicItem.LogicOperator, object>>
                    {
                        {
                            "LocalPCInfo/CurrentSecond",
                            new Tuple<LogicItem.LogicOperator, object>(
                                LogicItem.LogicOperator.LessThanOrEqual,
                                45
                            )
                        }
                    }
                }
            };*/
        }

        public Layer(string name, ILayerHandler handler = null) : this()
        {
            Name = name;
            if (handler != null)
                _Handler = handler;
        }

        public EffectLayer Render(IGameState gs)
        {
            foreach(LogicItem logic in _Logics)
            {
                logic.Check(gs, this._Handler);   
            }
            return this._Handler.Render(gs);
        }

        public void SetProfile(ProfileManager profile)
        {
            _profile = profile;

            _Handler?.SetProfile(_profile);
        }
    }
}
