using Aurora.Devices;
using Aurora.EffectsEngine.Animations;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify
{
    public class SpotifyProfile : ApplicationProfile
    {
        public SpotifyProfile() : base() { }

        public override void Reset()
        {
            base.Reset();

            OverlayLayers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                GetLayer("Thief Main", DeviceKeys.TILDE, "Colors/ColorThief/Main/"),
                GetLayer("Thief 1", DeviceKeys.ONE,   "Colors/ColorThief/Palette/Color1/"),
                GetLayer("Thief 2", DeviceKeys.TWO,   "Colors/ColorThief/Palette/Color2/"),
                GetLayer("Thief 3", DeviceKeys.THREE, "Colors/ColorThief/Palette/Color3/"),
                GetLayer("Thief 4", DeviceKeys.FOUR,  "Colors/ColorThief/Palette/Color4/"),
                GetLayer("Thief 5", DeviceKeys.FIVE,  "Colors/ColorThief/Palette/Color5/"),

                GetLayer("Old 1", DeviceKeys.Q, "Colors/Desaturated/"),
                GetLayer("Old 2", DeviceKeys.W, "Colors/LightVibrant/"),
                GetLayer("Old 3", DeviceKeys.E, "Colors/Prominent/"),
                GetLayer("Old 4", DeviceKeys.R, "Colors/Vibrant/"),
                GetLayer("Old 5", DeviceKeys.T, "Colors/VibrantNonAlarming/"),

                new Layer("black", new SolidFillLayerHandler()
                {
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Black
                    }
                })
            };    
        }

        Layer GetLayer(string name, DeviceKeys key, string id) =>
                new Layer(
                    name, 
                    new SolidColorLayerHandler()
                    {
                        Properties = new LayerHandlerProperties()
                        {
                            _PrimaryColor = Color.Transparent,
                            _Sequence = new KeySequence(new DeviceKeys[] { key })
                        }
                    },
                    new OverrideLogicBuilder()
                    .SetDynamicColor("_PrimaryColor", new NumberConstant(1),
                        new NumberGSINumeric(id + "Red"),
                        new NumberGSINumeric(id + "Green"),
                        new NumberGSINumeric(id + "Blue"))
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/Playing"))
                );
    }
}