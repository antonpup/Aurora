using Aurora.EffectsEngine;
using Aurora.Profiles.Borderlands2.GSI;
using Aurora.Profiles.Borderlands2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.Borderlands2.Layers
{
    public class Borderlands2BackgroundLayerHandlerProperties : LayerHandlerProperties2Color<Borderlands2BackgroundLayerHandlerProperties>
    {
        public Color? _ColorHealth { get; set; }

        [JsonIgnore]
        public Color ColorHeath { get { return Logic._ColorHealth ?? _ColorHealth ?? Color.Empty; } }

        public Color? _ColorHealthLow { get; set; }

        [JsonIgnore]
        public Color ColorHealthLow { get { return Logic._ColorHealthLow ?? _ColorHealthLow ?? Color.Empty; } }

        public Color? _ColorShield { get; set; }

        [JsonIgnore]
        public Color ColorShield { get { return Logic._ColorShield ?? _ColorShield ?? Color.Empty; } }

        public Color? _ColorShieldLow { get; set; }

        [JsonIgnore]
        public Color ColorShieldLow { get { return Logic._ColorShieldLow ?? _ColorShieldLow ?? Color.Empty; } }

        public Color? _ColorBackground { get; set; }

        [JsonIgnore]
        public Color ColorBackground { get { return Logic._ColorBackground ?? _ColorBackground ?? Color.Empty; } }

        public Color? _ColorBackgroundDeath { get; set; }

        [JsonIgnore]
        public Color ColorBackgroundDeath { get { return Logic._ColorBackgroundDeath ?? _ColorBackgroundDeath ?? Color.Empty; } }

        public bool? _ShowHealthStatus { get; set; }

        [JsonIgnore]
        public bool ShowHealthStatus { get { return Logic._ShowHealthStatus ?? _ShowHealthStatus ?? false; } }

        public bool? _ShowShieldStatus { get; set; }

        [JsonIgnore]
        public bool ShowShieldStatus { get { return Logic._ShowShieldStatus ?? _ShowShieldStatus ?? false; } }
        
        public Borderlands2BackgroundLayerHandlerProperties() : base() { }

        public Borderlands2BackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._ColorHealth = Color.Red;
            this._ColorHealthLow = Color.DarkRed;
            this._ColorShield = Color.Cyan;
            this._ColorShieldLow = Color.DarkCyan;
            this._ColorBackground = Color.LightGoldenrodYellow;
            this._ColorBackgroundDeath = Color.IndianRed;
            this._ShowHealthStatus = true;
            this._ShowShieldStatus = true;
        }

    }

    public class Borderlands2BackgroundLayerHandler : LayerHandler<Borderlands2BackgroundLayerHandlerProperties>
    {
        public Borderlands2BackgroundLayerHandler() : base()
        {
            _ID = "Borderlands2Background";
        }

        protected override UserControl CreateControl()
        {
            return new Control_Borderlands2BackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer("Borderlands 2 - Background");

            KeySequence KeySequenceHealth = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.TILDE, Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR,
                Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE,
                Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS, Devices.DeviceKeys.BACKSPACE
            });
            KeySequence KeySequenceShield = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.ESC, Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3, Devices.DeviceKeys.F4,
                Devices.DeviceKeys.F5, Devices.DeviceKeys.F6, Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9,
                Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
            });

            if (state is GameState_Borderlands2)
            {
                GameState_Borderlands2 rlstate = state as GameState_Borderlands2;

                if (rlstate.Player.maximumHealth == 0.0f)
                {
                    //// Main Menu
                    bg_layer.Fill(Properties.ColorBackground);
                }
                else
                {
                    //// Not in Main Menu
                    if (rlstate.Player.currentHealth == 0.0f)
                    {
                        //// Dead
                        bg_layer.Fill(Properties.ColorBackgroundDeath);
                    }
                    else
                    {
                        //// Alive
                        bg_layer.Fill(Properties.ColorBackground);

                        //// Health Bar
                        if (Properties._ShowHealthStatus == true)
                        {
                            bg_layer.PercentEffect(Properties._ColorHealth.Value, Properties._ColorHealthLow.Value, KeySequenceHealth, rlstate.Player.currentHealth, rlstate.Player.maximumHealth, PercentEffectType.Progressive_Gradual);
                        }

                        //// Shield Bar
                        if (Properties._ShowShieldStatus == true && rlstate.Player.maximumShield > 0.0f)
                        {
                            bg_layer.PercentEffect(Properties._ColorShield.Value, Properties._ColorShieldLow.Value, KeySequenceShield, rlstate.Player.currentShield, rlstate.Player.maximumShield, PercentEffectType.Progressive_Gradual);
                        }
                    }
                }
            }

            return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Borderlands2BackgroundLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}