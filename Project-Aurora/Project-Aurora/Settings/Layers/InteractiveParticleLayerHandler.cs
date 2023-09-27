using System.Collections.Concurrent;
using Aurora.Modules.Inputs;
using Aurora.Profiles;
using Common.Devices;

namespace Aurora.Settings.Layers;

[LayerHandlerMeta(Name = "Particle (Interactive)", IsDefault = true)]
public sealed class InteractiveParticleLayerHandler : SimpleParticleLayerHandler {

    private readonly ConcurrentQueue<DeviceKeys> _awaitingKeys = new();

    public InteractiveParticleLayerHandler() {
        Global.InputEvents.KeyDown += KeyDown;
    }

    private void KeyDown(object? sender, KeyboardKeyEvent e) {
        _awaitingKeys.Enqueue(e.GetDeviceKey());
    }

    protected override void SpawnParticles(double dt) {
        foreach (var key in _awaitingKeys) {
            Properties._Sequence = new KeySequence(new[] { key });
            var count = Rnd.Next(Properties.MinSpawnAmount, Properties.MaxSpawnAmount);
            for (var i = 0; i < count; i++)
                SpawnParticle();
        }
        _awaitingKeys.Clear();
    }

    public override void Dispose()
    {
        base.Dispose();
        Global.InputEvents.KeyDown -= KeyDown;
    }
}