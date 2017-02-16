using Aurora.Settings;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.Guild_Wars_2
{
    public class GW2Settings : ProfileSettings
    {
        //General
        public bool first_time_installed;

        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public GW2Settings()
        {
            //General
            IsEnabled = true;
            first_time_installed = false;

            Layers = new System.Collections.ObjectModel.ObservableCollection<Settings.Layers.Layer>()
            {
                new Settings.Layers.Layer("Movement", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.Q, Devices.DeviceKeys.E, Devices.DeviceKeys.V, Devices.DeviceKeys.R })
                    }
                }
                ),
                new Settings.Layers.Layer("Skills", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Green,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.TILDE, Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO })
                    }
                }
                ),
                new Settings.Layers.Layer("Targeting", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.T, Devices.DeviceKeys.TAB })
                    }
                }
                ),
                new Settings.Layers.Layer("User Interface", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Brown,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.O, Devices.DeviceKeys.Y, Devices.DeviceKeys.G, Devices.DeviceKeys.H, Devices.DeviceKeys.I, Devices.DeviceKeys.K, Devices.DeviceKeys.F12, Devices.DeviceKeys.F11, Devices.DeviceKeys.P, Devices.DeviceKeys.ENTER, Devices.DeviceKeys.M, Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.LEFT_ALT })
                    }
                }
                ),
                new Settings.Layers.Layer("Actions", new Settings.Layers.SolidColorLayerHandler()
                {
                    Properties = new Settings.Layers.LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.F, Devices.DeviceKeys.C })
                    }
                }
                )
            };

            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>();
        }
    }
}
