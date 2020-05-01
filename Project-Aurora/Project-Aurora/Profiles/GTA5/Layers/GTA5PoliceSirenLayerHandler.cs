using Aurora.EffectsEngine;
using Aurora.Profiles.GTA5.GSI;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.GTA5.Layers
{
    public class GTA5PoliceSirenLayerHandlerProperties : LayerHandlerProperties2Color<GTA5PoliceSirenLayerHandlerProperties>
    {
        public Color? _LeftSirenColor { get; set; }

        [JsonIgnore]
        public Color LeftSirenColor { get { return Logic._LeftSirenColor ?? _LeftSirenColor ?? Color.Empty; } }

        public Color? _RightSirenColor { get; set; }

        [JsonIgnore]
        public Color RightSirenColor { get { return Logic._RightSirenColor ?? _RightSirenColor ?? Color.Empty; } }

        public GTA5_PoliceEffects? _SirenType { get; set; }

        [JsonIgnore]
        public GTA5_PoliceEffects SirenType { get { return Logic._SirenType ?? _SirenType ?? GTA5_PoliceEffects.Default; ; } }

        public KeySequence _LeftSirenSequence { get; set; }

        [JsonIgnore]
        public KeySequence LeftSirenSequence { get { return Logic._LeftSirenSequence ?? _LeftSirenSequence ?? new KeySequence(); } }

        public KeySequence _RightSirenSequence { get; set; }

        [JsonIgnore]
        public KeySequence RightSirenSequence { get { return Logic._RightSirenSequence ?? _RightSirenSequence ?? new KeySequence(); } }

        public bool? _PeripheralUse { get; set; }

        [JsonIgnore]
        public bool PeripheralUse { get { return Logic._PeripheralUse ?? _PeripheralUse ?? false; } }

        public GTA5PoliceSirenLayerHandlerProperties() : base() { }

        public GTA5PoliceSirenLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._LeftSirenColor = Color.FromArgb(255, 0, 0);
            this._RightSirenColor = Color.FromArgb(0, 0, 255);
            this._SirenType = GTA5_PoliceEffects.Default;
            this._LeftSirenSequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.F1, Devices.DeviceKeys.F2, Devices.DeviceKeys.F3,
                Devices.DeviceKeys.F4, Devices.DeviceKeys.F5, Devices.DeviceKeys.F6
            });
            this._RightSirenSequence = new KeySequence(new Devices.DeviceKeys[] {
                Devices.DeviceKeys.F7, Devices.DeviceKeys.F8, Devices.DeviceKeys.F9,
                Devices.DeviceKeys.F10, Devices.DeviceKeys.F11, Devices.DeviceKeys.F12
            });
            this._PeripheralUse = true;
        }

    }

    public class GTA5PoliceSirenLayerHandler : LayerHandler<GTA5PoliceSirenLayerHandlerProperties>
    {
        private Color left_siren_color = Color.Empty;
        private Color right_siren_color = Color.Empty;
        private int siren_keyframe = 0;

        protected override UserControl CreateControl()
        {
            return new Control_GTA5PoliceSirenLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer sirens_layer = new EffectLayer("GTA 5 - Police Sirens");

            if (state is GameState_GTA5)
            {
                GameState_GTA5 gta5state = state as GameState_GTA5;

                if (gta5state.HasCops)
                {
                    if (left_siren_color != gta5state.LeftSirenColor && right_siren_color != gta5state.RightSirenColor)
                        siren_keyframe++;

                    left_siren_color = gta5state.LeftSirenColor;
                    right_siren_color = gta5state.RightSirenColor;

                    Color lefts = Properties.LeftSirenColor;
                    Color rights = Properties.RightSirenColor;

                    //Switch sirens
                    switch (Properties.SirenType)
                    {
                        case GTA5_PoliceEffects.Alt_Full:
                            switch (siren_keyframe % 2)
                            {
                                case 1:
                                    rights = lefts;
                                    break;
                                default:
                                    lefts = rights;
                                    break;
                            }
                            siren_keyframe = siren_keyframe % 2;

                            if (Properties.PeripheralUse)
                                sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                            break;
                        case GTA5_PoliceEffects.Alt_Half:
                            switch (siren_keyframe % 2)
                            {
                                case 1:
                                    rights = lefts;
                                    lefts = Color.Black;

                                    if (Properties.PeripheralUse)
                                        sirens_layer.Set(Devices.DeviceKeys.Peripheral, rights);
                                    break;
                                default:
                                    lefts = rights;
                                    rights = Color.Black;

                                    if (Properties.PeripheralUse)
                                        sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                                    break;
                            }
                            siren_keyframe = siren_keyframe % 2;
                            break;
                        case GTA5_PoliceEffects.Alt_Full_Blink:
                            switch (siren_keyframe % 4)
                            {
                                case 2:
                                    rights = lefts;
                                    break;
                                case 0:
                                    lefts = rights;
                                    break;
                                default:
                                    lefts = Color.Black;
                                    rights = Color.Black;
                                    break;
                            }
                            siren_keyframe = siren_keyframe % 4;

                            if (Properties.PeripheralUse)
                                sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                            break;
                        case GTA5_PoliceEffects.Alt_Half_Blink:
                            switch (siren_keyframe % 8)
                            {
                                case 6:
                                    rights = lefts;
                                    lefts = Color.Black;

                                    if (Properties.PeripheralUse)
                                        sirens_layer.Set(Devices.DeviceKeys.Peripheral, rights);
                                    break;
                                case 4:
                                    rights = lefts;
                                    lefts = Color.Black;

                                    if (Properties.PeripheralUse)
                                        sirens_layer.Set(Devices.DeviceKeys.Peripheral, rights);
                                    break;
                                case 2:
                                    lefts = rights;
                                    rights = Color.Black;

                                    if (Properties.PeripheralUse)
                                        sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                                    break;
                                case 0:
                                    lefts = rights;
                                    rights = Color.Black;

                                    if (Properties.PeripheralUse)
                                        sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                                    break;
                                default:
                                    rights = Color.Black;
                                    lefts = Color.Black;

                                    if (Properties.PeripheralUse)
                                        sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                                    break;
                            }
                            siren_keyframe = siren_keyframe % 8;
                            break;
                        default:
                            switch (siren_keyframe % 2)
                            {
                                case 1:
                                    Color tempc = rights;
                                    rights = lefts;
                                    lefts = tempc;
                                    break;
                                default:
                                    break;
                            }
                            siren_keyframe = siren_keyframe % 2;

                            if (Properties.PeripheralUse)
                                sirens_layer.Set(Devices.DeviceKeys.Peripheral, lefts);
                            break;
                    }

                    sirens_layer.Set(Properties.LeftSirenSequence, lefts);
                    sirens_layer.Set(Properties.RightSirenSequence, rights);
                }
            }

            return sirens_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_GTA5PoliceSirenLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}