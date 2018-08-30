using System.Collections.Generic;
using Aurora.EffectsEngine;
using NAudio.CoreAudioApi;
using System.Drawing;
using Aurora.Settings.Layers;
using System;

namespace Aurora.Profiles.Overlays
{
    public class Event_VolumeOverlay : LightEvent
    {
        MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
        MMDevice defaultDevice = null;

        public Event_VolumeOverlay()
        {
            //We only need to get the default device once as a new instance of this class is created each time it is being displayed and we don't really need to worry about the device changing in the small amount of time this is being displayed.
            //defaultDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        }

        public override bool IsEnabled
        {
            get { return Global.Configuration.volume_overlay_settings.enabled; }
        }

        public override void UpdateLights(EffectFrame frame)
        {
            //if (Global.Configuration.volume_overlay_settings.enabled)
            //{
            if (defaultDevice == null)
                defaultDevice = devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

                if (Global.Configuration.volume_overlay_settings.dim_background)
                    layers.Enqueue(new EffectLayer("Overlay - Volume Base", Global.Configuration.volume_overlay_settings.dim_color));

                EffectLayer volume_bar = new EffectLayer("Overlay - Volume Bar");
                float currentVolume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
                ColorSpectrum volume_spec = new ColorSpectrum(Global.Configuration.volume_overlay_settings.low_color, Global.Configuration.volume_overlay_settings.high_color);
                volume_spec.SetColorAt(0.75f, Global.Configuration.volume_overlay_settings.med_color);

                volume_bar.PercentEffect(volume_spec, Global.Configuration.volume_overlay_settings.sequence, currentVolume, 1.0f);

                layers.Enqueue(volume_bar);

                frame.AddOverlayLayers(layers.ToArray());
            //}
        }

        public override void SetGameState(IGameState new_game_state)
        {
            //No need to do anything... This doesn't have any gamestates.
            //UpdateLights(frame);
        }

        public override void Dispose()
        {
            this.defaultDevice?.Dispose();
            this.devEnum?.Dispose();
        }
    }
}
