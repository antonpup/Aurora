using Aurora.Devices;
using Aurora.Profiles;
using Aurora.Utils;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Settings.Layers {

    [LayerHandlerMeta(Name = "Particle (Interactive)", IsDefault = true)]
    public class InteractiveParticleLayerHandler : SimpleParticleLayerHandler {

        private readonly HashSet<DeviceKeys> awaitingKeys = new HashSet<DeviceKeys>();

        public InteractiveParticleLayerHandler() {
            Global.InputEvents.KeyDown += KeyDown;
        }

        private void KeyDown(object sender, SharpDX.RawInput.KeyboardInputEventArgs e) {
            awaitingKeys.Add(e.GetDeviceKey());
        }

        protected override void SpawnParticles(double dt) {
            foreach (var key in awaitingKeys) {
                Properties._Sequence = new KeySequence(new[] { key });
                var count = rnd.Next(Properties.MinSpawnAmount, Properties.MaxSpawnAmount);
                for (var i = 0; i < count; i++)
                    SpawnParticle();
            }
            awaitingKeys.Clear();
        }
    }
}
