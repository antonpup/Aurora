using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles.ETS2.GSI;
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

namespace Aurora.Profiles.ETS2.Layers {
    public class ETS2BeaconLayerProperties : LayerHandlerProperties<ETS2BeaconLayerProperties> {
        public ETS2_BeaconStyle? _BeaconStyle { get; set; }

        [JsonIgnore]
        public ETS2_BeaconStyle BeaconStyle { get { return Logic._BeaconStyle ?? _BeaconStyle ?? ETS2_BeaconStyle.Simple_Flash; } }

        public float? _Speed { get; set; }

        [JsonIgnore]
        public float Speed { get { return Logic._Speed ?? _Speed ?? 1; } }

        public ETS2BeaconLayerProperties() : base() { }
        public ETS2BeaconLayerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default() {
            base.Default();

            this._Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8 });
            this._PrimaryColor = Color.FromArgb(255, 128, 0);
            this._BeaconStyle = ETS2_BeaconStyle.Fancy_Flash;
            this._Speed = 1f;
        }
    }

    public class ETS2BeaconLayerHandler : LayerHandler<ETS2BeaconLayerProperties> {

        private int frame = 0;

        public ETS2BeaconLayerHandler() : base() {
            _ID = "ETS2BlinkerIndicator";
        }

        protected override UserControl CreateControl() {
            return new Control_ETS2BeaconLayer(this);
        }

        /// <summary>Multiplies the Primary Color's alpha by this value and returns it.</summary>
        private Color PrimaryColorAlpha(double a) {
            int alpha = Utils.ColorUtils.ColorByteMultiplication(Properties.PrimaryColor.A, a);
            return Color.FromArgb(alpha, Properties.PrimaryColor);
        }

        public override EffectLayer Render(IGameState gamestate) {
            EffectLayer layer = new EffectLayer("ETS2 Beacon Layer");

            if (gamestate is GameState_ETS2 && (gamestate as GameState_ETS2).Truck.lightsBeaconOn) {
                switch (this.Properties.BeaconStyle) {
                    // Fades all assigned lights in and out together
                    case ETS2_BeaconStyle.Simple_Flash:
                        double multiplier = Math.Pow(Math.Sin(frame * Properties.Speed * Math.PI / 10), 2);
                        layer.Set(Properties.Sequence, PrimaryColorAlpha(multiplier));
                        frame = (frame + 1) % (int)(10 / Properties.Speed);
                        break;

                    // Flashes lights in a pattern similar to the ETS2 LED beacons
                    // Pattern: ###------###------#-#-#----#-#-#----#-#-#---- [2x###------, 3x#-#-#----] (# = on, - = off)
                    case ETS2_BeaconStyle.Fancy_Flash:
                        // To get the keyframe number we divide frame by 2 because it was too fast otherwise
                        int m10 = (frame / 2) % 9; // Mod 10 of the keyframe (10 is the size of a group)
                        bool on = (frame / 2) < 18
                            ? m10 < 4 // When in one of the first two groups, light up for the first 4 keyframes of that group
                            : (m10 == 0 || m10 == 2 || m10 == 4); // When in the last 3 groups, light up if the keyframe is 0th, 2nd or 4th of that group

                        if (on)
                            layer.Set(Properties.Sequence, Properties.PrimaryColor);

                        frame = (frame + 1) % 90; // 90 because there are 9 keyframes per group, 5 groups and each keyframe = 2 real frames (9 * 5 * 2)
                        break;

                    // Sets half the sequence on and half off, then swaps. If odd number of keys, first half will be bigger
                    case ETS2_BeaconStyle.Half_Alternating:
                        List<DeviceKeys> half;
                        if (frame < 5)
                            // First half
                            half = Properties.Sequence.keys.GetRange(0, (int)Math.Ceiling((double)Properties.Sequence.keys.Count / 2));
                        else
                            // Second half
                            half = Properties.Sequence.keys.GetRange((int)Math.Ceiling((double)Properties.Sequence.keys.Count / 2), (int)Math.Ceiling((double)Properties.Sequence.keys.Count / 2));

                        layer.Set(half.ToArray(), Properties.PrimaryColor);

                        frame = (frame + 1) % 10;
                        break;
                    
                    // The "on" key goes up and down the sequence
                    case ETS2_BeaconStyle.Side_To_Side:
                        int keyCount = Properties.Sequence.keys.Count;

                        int light = Math.Abs(((frame/2 + 1) % (keyCount * 2 - 2)) - keyCount + 2);
                        int prevLight = Math.Abs(frame/2 - keyCount + 2);
                        layer.Set(Properties.Sequence.keys[light], Properties.PrimaryColor);
                        layer.Set(Properties.Sequence.keys[prevLight], PrimaryColorAlpha(.5));

                        frame = (frame + 1) % ((keyCount - 1) * 4); // *4 because we want the pattern to go up and down (*2), and also each keyframe should take 2 real frames
                        break;
                }

            }  else // When the beacon is off, reset the frame counter so that the animation plays from the start
                frame = 0;

            return layer;
        }
    }
}
