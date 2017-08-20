using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.HotlineMiami
{
    public class HMProfile : ApplicationProfile
    {
        public HMProfile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Movement", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.W, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D })
                    }
                }
                ),
                new Layer("Other Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.SPACE, Devices.DeviceKeys.LEFT_SHIFT, Devices.DeviceKeys.R, Devices.DeviceKeys.ESC })
                    }
                }
                )
            };
        }
    }
}
