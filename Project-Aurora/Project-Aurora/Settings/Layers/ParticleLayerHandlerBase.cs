using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using static System.Linq.Expressions.Expression;

namespace Aurora.Settings.Layers {

    /// <summary>
    /// The base properties for the base particle layer.
    /// </summary>
    public class ParticleLayerPropertiesBase<TSelf> : LayerHandlerProperties<TSelf>, INotifyPropertyChanged where TSelf : ParticleLayerPropertiesBase<TSelf> {
        
        // Whether or not the particles will spawn. This allows the particle system to be turned off without disabling it (thereby not hiding already spawned particles).
        // This can only be done by the overrides system.
        [LogicOverridable("Enable Particle Spawning")] public bool? _SpawningEnabled { get; set; }
        [JsonIgnore] public bool SpawningEnabled => Logic._SpawningEnabled ?? true;

        public event PropertyChangedEventHandler PropertyChanged;

        public ParticleLayerPropertiesBase() : base() { }
        public ParticleLayerPropertiesBase(bool empty = false) : base(empty) { }

        public override void Default() {
            base.Default();
            _Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
        }
    }


    /// <summary>
    /// The base particle layer handler which manages and renders a collection of particles.<para/>
    /// The behaviour of the particle is determined by the particle implementation.
    /// </summary>
    /// <typeparam name="TParticle"></typeparam>
    /// <typeparam name="TProperties"></typeparam>
    public abstract class ParticleLayerHandlerBase<TParticle, TProperties> : LayerHandler<TProperties>, INotifyRender
        where TParticle : IParticle<TProperties>
        where TProperties : ParticleLayerPropertiesBase<TProperties> {

        // Lazy-instantiated lambda that creates a new particle (since using Activator.CreateInstance can be slow)
        private static readonly Lazy<Func<TProperties, TParticle>> spawnLambda = new Lazy<Func<TProperties, TParticle>>(() => {
            var ctor = typeof(TParticle).GetConstructor(new[] { typeof(TProperties) });
            var tpropParam = Parameter(typeof(TProperties));
            return Lambda<Func<TProperties, TParticle>>(
                New(ctor, tpropParam),
                tpropParam
            ).Compile();
        });

        protected static readonly Random rnd = new Random();
        protected readonly Stopwatch stopwatch = new Stopwatch(); // Stopwatch to determine time difference since last render
        protected readonly List<TParticle> particles = new List<TParticle>(); // All the currently active "alive" particles

        public event EventHandler<Bitmap> LayerRender; // Fires whenever the layer is rendered

        protected override abstract UserControl CreateControl();

        public override EffectLayer Render(IGameState gameState) {
            var layer = new EffectLayer();

            // Get elapsed time since last render
            var dt = stopwatch.ElapsedMilliseconds / 1000d;
            stopwatch.Restart();

            // Update and render all particles
            using (var gfx = layer.GetGraphics()) {
                foreach (var particle in particles) {
                    particle.Update(dt, Properties, gameState);
                    if (particle.IsAlive(Properties, gameState))
                        particle.Render(gfx, Properties, gameState);
                }
            }

            // Spawn new particles if required
            SpawnParticles(dt);

            // Remove any particles that have expired
            particles.RemoveAll(p => !p.IsAlive(Properties, gameState));

            // Call the render event
            LayerRender?.Invoke(this, layer.GetBitmap());

            return layer;
        }

        /// <summary>
        /// Performs the logic to spawn the particles. Determines whether or not the particles should be spawned and
        /// how many, then calls <see cref="SpawnParticle"/> for each.
        /// </summary>
        /// <param name="deltaTime">The time (in seconds) since the last call.</param>
        protected abstract void SpawnParticles(double deltaTime);

        /// <summary>
        /// Creates a new particle and returns it. By default, calls a constructor on <typeparamref name="TParticle"/> with a single <typeparamref name="TParticle"/> parameter.
        /// </summary>
        protected virtual void SpawnParticle() => particles.Add(spawnLambda.Value(Properties));

        /// <summary>
        /// Picks a random floating point number between the two given numbers.
        /// </summary>
        protected static float RandomBetween(float a, float b) => (float)((rnd.NextDouble() * (b - a)) + a);
    }


    /// <summary>
    /// Interface for the particle data class.
    /// </summary>
    /// <typeparam name="TProperties">The layer handler properties type that is passed to the particle.</typeparam>
    public interface IParticle<TProperties> where TProperties : ParticleLayerPropertiesBase<TProperties> {

        /// <summary>Updates the data of the particle (e.g. position, velocity).</summary>
        /// <param name="deltaTime">The time (in seconds) since the last update.</param>
        void Update(double deltaTime, TProperties properties, IGameState gameState);

        /// <summary>Renders the particle to the given graphics context.</summary>
        void Render(Graphics gfx, TProperties properties, IGameState gameState);

        /// <summary>Determines if the particle is alive. A particle that is not alive will be removed from the canvas.</summary>
        bool IsAlive(TProperties properties, IGameState gameState);
    }
}
