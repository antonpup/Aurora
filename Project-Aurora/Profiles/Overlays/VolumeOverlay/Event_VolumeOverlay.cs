using System.Collections.Generic;
using Aurora.EffectsEngine;
using NAudio.CoreAudioApi;
using System.Drawing;
using Aurora.Settings.Layers;

namespace Aurora.Profiles.Overlays
{
    public class Event_VolumeOverlay : LightEvent
    {
        public override bool IsEnabled()
        {
            return true;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            if (Global.Configuration.volume_overlay_settings.enabled)
            {
                Queue<EffectLayer> layers = new Queue<EffectLayer>();

                EffectLayer volume_bar = new EffectLayer("Overlay - Volume Bar");
                
                MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
                MMDevice defaultDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                float currentVolume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar;

                ColorSpectrum volume_spec = new ColorSpectrum(Global.Configuration.volume_overlay_settings.low_color, Global.Configuration.volume_overlay_settings.high_color);
                volume_spec.SetColorAt(0.75f, Global.Configuration.volume_overlay_settings.med_color);

                volume_bar.PercentEffect(volume_spec, Global.Configuration.volume_overlay_settings.sequence, currentVolume, 1.0f);

                if (Global.Configuration.volume_overlay_settings.dim_background)
                {
                    EffectLayer volume_black_base = new EffectLayer("Overlay - Volume Base");
                    volume_black_base.PercentEffect(new ColorSpectrum(Color.Black), Global.Configuration.volume_overlay_settings.sequence, 1, 1);
                    layers.Enqueue(volume_black_base);
                }
                layers.Enqueue(volume_bar);

                frame.AddOverlayLayers(layers.ToArray());
            }
        }

        public override void UpdateLights(EffectFrame frame, IGameState new_game_state)
        {
            //No need to do anything... This doesn't have any gamestates.
            UpdateLights(frame);
        }
    }
}
