using Aurora.Devices;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.Minecraft.Layers;
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

namespace Aurora.Profiles.Discord {
    public class DiscordProfile : ApplicationProfile {
        public override void Reset() {
            base.Reset();

            OverlayLayers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Mic Mute", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4 })
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("User/SelfMute"))),

                new Layer("Deafen", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.LimeGreen,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8 })
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("User/SelfDeafen"))),

                new Layer("Mentions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12 })
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("User/Mentions"))),

                new Layer("Unread Messages", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.LightBlue,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.PRINT_SCREEN, DeviceKeys.SCROLL_LOCK, DeviceKeys.PAUSE_BREAK })
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("User/UnreadMessages"))),
            };
        }
    }
}
