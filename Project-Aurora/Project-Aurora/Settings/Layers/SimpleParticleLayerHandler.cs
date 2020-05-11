using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    /// <summary>
    /// Properties for the particle layer handler that handles "simple" particles. Simple particles are ones that just support velocity,
    /// acceleration and drag and whose alive state is determined by their life span.<para />
    /// This class can be overriden and the subclass should pass itself as the <typeparamref name="TSelf"/> parameter.
    /// </summary>
    public class SimpleParticleLayerProperties<TSelf> : ParticleLayerPropertiesBase<TSelf> where TSelf : SimpleParticleLayerProperties<TSelf> {

        // Override the default key sequence property so that we can make it trigger a property change notification
        [LogicOverridable("Spawn region")] public override KeySequence _Sequence { get; set; }

        // The shortest time (in seconds) between particles spawning. A random time between this value and "MaxSpawnTime" will be chosen.
        [LogicOverridable] public float? _MinSpawnTime { get; set; }
        [JsonIgnore] public float MinSpawnTime => Logic._MinSpawnTime ?? _MinSpawnTime ?? .5f;

        // The longest time (in seconds) between particles spawning. A random time from "MinSpawnTime" up to this value will be chosen.
        [LogicOverridable] public float? _MaxSpawnTime { get; set; }
        [JsonIgnore] public float MaxSpawnTime => Logic._MaxSpawnTime ?? _MaxSpawnTime ?? 1f;

        // The smallest quantity of particles that will spawn at a time. A random value between this value and "MaxSpawnAmount" will be chosen.
        [LogicOverridable] public int? _MinSpawnAmount { get; set; }
        [JsonIgnore] public int MinSpawnAmount => Logic._MinSpawnAmount ?? _MinSpawnAmount ?? 1;

        // The largest quantity of particles that will spawn at a time. A random value between "MinSpawnAmount" and this value will be chosen.
        [LogicOverridable] public int? _MaxSpawnAmount { get; set; }
        [JsonIgnore] public int MaxSpawnAmount => Logic._MaxSpawnAmount ?? _MaxSpawnAmount ?? 1;

        // The smallest possible initial horizontal velocity of the spawned particles. A random value between this value and "MaxInitialVelocityX" will be chosen for each particle.
        [LogicOverridable] public float? _MinInitialVelocityX { get; set; }
        [JsonIgnore] public float MinInitialVelocityX => Logic._MinInitialVelocityX ?? _MinInitialVelocityX ?? 0f;

        // The largest possible initial horizontal velocity of the spawned particles. A random value between "MinInitialVelocityX" and this value will be chosen for each particle.
        [LogicOverridable] public float? _MaxInitialVelocityX { get; set; }
        [JsonIgnore] public float MaxInitialVelocityX => Logic._MaxInitialVelocityX ?? _MaxInitialVelocityX ?? 0f;

        // The smallest possible initial vertical velocity of the spawned particles. A random value between this value and "MaxInitialVelocityY" will be chosen for each particle.
        [LogicOverridable] public float? _MinInitialVelocityY { get; set; }
        [JsonIgnore] public float MinInitialVelocityY => Logic._MinInitialVelocityY ?? _MinInitialVelocityY ?? 0f;

        // The largest possible initial vertical velocity of the spawned particles. A random value between "MinInitialVelocityY" and this value will be chosen for each particle.
        [LogicOverridable] public float? _MaxInitialVelocityY { get; set; }
        [JsonIgnore] public float MaxInitialVelocityY => Logic._MaxInitialVelocityY ?? _MaxInitialVelocityY ?? 0f;

        // The minimum possible lifetime of the particles (in seconds). A random lifetime between this number and "MaxLifetime" will be chosen.
        [LogicOverridable] public float? _MinLifetime { get; set; }
        [JsonIgnore] public float MinLifetime => Logic._MinLifetime ?? _MinLifetime ?? 3f;

        // The maximum possible lifetime of the particles (in seconds). A random lifetime between from "MinLifetime" up to this number will be chosen.
        [LogicOverridable] public float? _MaxLifetime { get; set; }
        [JsonIgnore] public float MaxLifetime => Logic._MaxLifetime ?? _MaxLifetime ?? 3f;

        // The amount the speed of the particle in the horizontal direction will change per second.
        [LogicOverridable] public float? _AccelerationX { get; set; }
        [JsonIgnore] public float AccelerationX => Logic._AccelerationX ?? _AccelerationX ?? 0f;

        // The amount the speed of the particle in the vertical direction will change per second.
        [LogicOverridable] public float? _AccelerationY { get; set; }
        [JsonIgnore] public float AccelerationY => Logic._AccelerationY ?? _AccelerationY ?? -1f;

        // The amount of velocity per second the particle loses in the horizontal direction as a percentage of its current velocity
        [LogicOverridable] public float? _DragX { get; set; }
        [JsonIgnore] public float DragX => Logic._DragX ?? _DragX ?? 0;

        // The amount of velocity per second the particle loses in the vertical direction as a percentage of its current velocity
        [LogicOverridable] public float? _DragY { get; set; }
        [JsonIgnore] public float DragY => Logic._DragY ?? _DragY ?? 0;

        // The smallest initial size of the particles
        [LogicOverridable] public float? _MinSize { get; set; }
        [JsonIgnore] public float MinSize => Logic._MinSize ?? _MinSize ?? 6;

        // The largest initial size of the particles
        [LogicOverridable] public float? _MaxSize { get; set; }
        [JsonIgnore] public float MaxSize => Logic._MaxSize ?? _MaxSize ?? 6;

        // The initial size of the particles
        [LogicOverridable] public float? _DeltaSize { get; set; }
        [JsonIgnore] public float DeltaSize => Logic._DeltaSize ?? _DeltaSize ?? 0;

        // Where the particles will spawn from
        public ParticleSpawnLocations? _SpawnLocation { get; set; }
        [JsonIgnore] public ParticleSpawnLocations SpawnLocation => Logic._SpawnLocation ?? _SpawnLocation ?? ParticleSpawnLocations.BottomEdge;

        // The color gradient stops for the particle. Note this is sorted by offset when set using _ParticleColorStops. Not using a linear brush here because:
        //   1) there are multithreading issues when trying to access a Media brush's gradient collection since it belongs to the UI thread
        //   2) We don't actually need the gradient as a brush since we're not drawing particles as gradients, only a solid color based on their lifetime, so we only need to access the color stops
        public ColorStopCollection _ParticleColorStops { get; set; }
        [JsonIgnore] public ColorStopCollection ParticleColorStops => Logic._ParticleColorStops ?? _ParticleColorStops ?? defaultParticleColor;

        // An override proxy for setting the particle color stops
        [JsonIgnore, LogicOverridable("Color over time")] public EffectBrush _ParticleBrush {
            get => new EffectBrush(_ParticleColorStops.ToMediaBrush());
            set => _ParticleColorStops = value == null ? null : ColorStopCollection.FromMediaBrush(value.GetMediaBrush());
        }

        private static readonly ColorStopCollection defaultParticleColor = new ColorStopCollection {
            {0f, Color.White },
            {1f,  Color.FromArgb(0, Color.White) }
        };

        public SimpleParticleLayerProperties() : base() { }
        public SimpleParticleLayerProperties(bool empty = false) : base(empty) { }

        public override void Default() {
            base.Default();
            _SpawnLocation = ParticleSpawnLocations.BottomEdge;
            _ParticleColorStops = defaultParticleColor;
            _MinSpawnTime = _MaxSpawnTime = .1f;
            _MinSpawnAmount = _MaxSpawnAmount = 1;
            _MinLifetime = 0; _MaxLifetime = 2;
            _MinInitialVelocityX = _MaxInitialVelocityX = 0;
            _MinInitialVelocityY = _MaxInitialVelocityY = -1;
            _AccelerationX = 0;
            _AccelerationY = .5f;
            _DragX = 0;
            _DragY = 0;
            _MinSize = 6; _MaxSize = 6;
            _DeltaSize = 0;
            _Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
        }
    }


    /// <summary>
    /// Properties for the particle layer handler that handles "simple" particles. Simple particles are ones that just support velocity,
    /// acceleration and drag and whose alive state is determined by their life span.
    /// </summary>
    public class SimpleParticleLayerProperties : SimpleParticleLayerProperties<SimpleParticleLayerProperties> {
        public SimpleParticleLayerProperties() : base() { }
        public SimpleParticleLayerProperties(bool empty = false) : base(empty) { }
    }


    /// <summary>
    /// Layer handler for the simple particles. It uses time-based spawning and despawning logic.
    /// </summary>
    [LayerHandlerMeta(Name = "Particle", IsDefault = true)]
    [LogicOverrideIgnoreProperty(nameof(LayerHandlerProperties._PrimaryColor))]
    public class SimpleParticleLayerHandler : ParticleLayerHandlerBase<SimpleParticle, SimpleParticleLayerProperties> {

        private double nextSpawnInterval = 0; // How many seconds until next set of particle spawns
        
        protected override UserControl CreateControl() => new Control_ParticleLayer(this);

        protected override void SpawnParticles(double dt) {
            if (Properties.SpawningEnabled) {
                nextSpawnInterval -= dt;
                if (nextSpawnInterval < 0) {
                    var count = rnd.Next(Properties.MinSpawnAmount, Properties.MaxSpawnAmount + 1);
                    for (var i = 0; i < count; i++)
                        SpawnParticle();
                    nextSpawnInterval = RandomBetween(Properties.MinSpawnTime, Properties.MaxSpawnTime);
                }
            }
        }
    }


    /// <summary>
    /// Particle definition that handles "simple" particles. Simple particles are ones that just support velocity,
    /// acceleration and drag and whose alive state is determined by their life span.
    /// </summary>
    public class SimpleParticle : IParticle<SimpleParticleLayerProperties> {

        private static readonly Random rnd = new Random();

        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Size { get; set; }
        public double Lifetime { get; set; }
        public double MaxLifetime { get; set; }

        /// <summary>
        /// When the particle is created, randomise it's position and velocity according to the properties and canvas size.
        /// </summary>
        public SimpleParticle(SimpleParticleLayerProperties properties) {
            var ar = properties.Sequence.GetAffectedRegion();
            MaxLifetime = (float)((rnd.NextDouble() * (properties.MaxLifetime - properties.MinLifetime)) + properties.MinLifetime);
            VelocityX = (float)((rnd.NextDouble() * (properties.MaxInitialVelocityX - properties.MinInitialVelocityX)) + properties.MinInitialVelocityX);
            VelocityY = (float)((rnd.NextDouble() * (properties.MaxInitialVelocityY - properties.MinInitialVelocityY)) + properties.MinInitialVelocityY);
            PositionX
                = properties.SpawnLocation == ParticleSpawnLocations.LeftEdge ? 0 // For left edge, X should start at 0
                : properties.SpawnLocation == ParticleSpawnLocations.RightEdge ? Effects.canvas_width // For right edge, X should start at maximum width
                : properties.SpawnLocation == ParticleSpawnLocations.Region ? ar.Left + (float)(rnd.NextDouble() * ar.Width)// For region, randomly choose X in region
                : (float)(rnd.NextDouble() * Effects.canvas_width); // For top, bottom or random, randomly choose an X value
            PositionY
                = properties.SpawnLocation == ParticleSpawnLocations.TopEdge ? 0 // For top edge, Y should start at 0
                : properties.SpawnLocation == ParticleSpawnLocations.BottomEdge ? Effects.canvas_height // For bottom edge, Y should start at maximum height
                : properties.SpawnLocation == ParticleSpawnLocations.Region ? ar.Top + (float)(rnd.NextDouble() * ar.Height)// For region, randomly choose Y in region
                : (float)(rnd.NextDouble() * Effects.canvas_height); // For left, right or random, randomly choose a Y value
            Size = (float)(rnd.NextDouble() * (properties.MaxSize - properties.MinSize)) + properties.MinSize;
        }

        /// <summary>
        /// The particle's life is based on it's <see cref="Lifetime"/> and <see cref="MaxLifetime"/> properties.
        /// </summary>
        public bool IsAlive(SimpleParticleLayerProperties properties, IGameState gameState) => Lifetime < MaxLifetime;

        public void Render(Graphics gfx, SimpleParticleLayerProperties properties, IGameState gameState) {
            var s2 = Size / 2;
            gfx.FillEllipse(new SolidBrush(properties.ParticleColorStops.GetColorAt((float)(Lifetime / MaxLifetime))), new RectangleF(PositionX - s2, PositionY - s2, Size, Size));
        }

        /// <summary>
        /// Update the velocity of the particle based on the acceleration and drag. Then, update the position based on the velocity.
        /// </summary>
        public void Update(double deltaTime, SimpleParticleLayerProperties properties, IGameState gameState) {
            Lifetime += deltaTime;
            VelocityX += (float)(properties.AccelerationX * deltaTime);
            VelocityY += (float)(properties.AccelerationY * deltaTime);
            VelocityX *= (float)Math.Pow(1 - properties.DragX, deltaTime); // By powering the drag to the deltaTime, we ensure that the results are fairly consistent over different time deltas.
            VelocityY *= (float)Math.Pow(1 - properties.DragY, deltaTime); // Doing it once over a second won't be 100% the same as doing it twice over a second if acceleration is present, but it should be close enough that it won't be noticed under most cicrumstances
            PositionX += VelocityX;
            PositionY += VelocityY;
            Size += (float)(properties.DeltaSize * deltaTime);
        }
    }


    /// <summary>
    /// An enum dictating possible spawn locations for the particles.
    /// </summary>
    public enum ParticleSpawnLocations {
        [Description("Top edge")] TopEdge,
        [Description("Right edge")] RightEdge,
        [Description("Bottom edge")] BottomEdge,
        [Description("Left edge")] LeftEdge,
        Region,
        Random
    }


    /// <summary>
    /// Class that holds some preset configurations for the ParticleLayerProperties class.
    /// </summary>
    public static class ParticleLayerPresets {
        public static ReadOnlyDictionary<string, Action<SimpleParticleLayerProperties>> Presets { get; } = new ReadOnlyDictionary<string, Action<SimpleParticleLayerProperties>>(
            new Dictionary<string, Action<SimpleParticleLayerProperties>> {
                { "Fire", p => {
                    p._SpawnLocation = ParticleSpawnLocations.BottomEdge;
                    p._ParticleColorStops = new ColorStopCollection {
                        { 0f, Color.Yellow },
                        { 0.6f, Color.FromArgb(128, Color.Red) },
                        { 1f, Color.FromArgb(0, Color.Black) }
                    };
                    p._MinSpawnTime = p._MaxSpawnTime = .05f;
                    p._MinSpawnAmount = 4; p._MaxSpawnAmount = 6;
                    p._MinLifetime = .5f; p._MaxLifetime = 2;
                    p._MinInitialVelocityX = p._MaxInitialVelocityX = 0;
                    p._MinInitialVelocityY = -1.3f; p._MaxInitialVelocityY = -0.8f;
                    p._AccelerationX = 0;
                    p._AccelerationY = 0.5f;
                    p._MinSize = 8; p._MaxSize = 12;
                    p._DeltaSize = -4;
                } },
                { "Matrix", p => {
                    p._SpawnLocation = ParticleSpawnLocations.TopEdge;
                    p._ParticleColorStops = new ColorStopCollection {
                        { 0f, Color.FromArgb(0,255,0) },
                        { 1f, Color.FromArgb(0,255,0) }
                    };
                    p._MinSpawnTime = .1f; p._MaxSpawnTime = .2f;
                    p._MinSpawnAmount = 1; p._MaxSpawnAmount = 2;
                    p._MinLifetime = p._MaxLifetime = 1;
                    p._MinInitialVelocityX = p._MaxInitialVelocityX = 0;
                    p._MinInitialVelocityY = p._MaxInitialVelocityY = 3;
                    p._AccelerationX = 0;
                    p._AccelerationY = 0;
                    p._MinSize = 6; p._MaxSize = 6;
                    p._DeltaSize = 0;
                } },
                { "Rain", p => {
                    p._SpawnLocation = ParticleSpawnLocations.TopEdge;
                    p._ParticleColorStops = new ColorStopCollection {
                        { 0f, Color.Cyan },
                        { 1f, Color.Cyan }
                    };
                    p._MinSpawnTime = .1f; p._MaxSpawnTime = .2f;
                    p._MinSpawnAmount = 1; p._MaxSpawnAmount = 2;
                    p._MinLifetime = p._MaxLifetime = 1;
                    p._MinInitialVelocityX = p._MaxInitialVelocityX = 0;
                    p._MinInitialVelocityY = p._MaxInitialVelocityY = 3;
                    p._AccelerationX = 0;
                    p._AccelerationY = 0;
                    p._MinSize = 2;
                    p._MaxSize = 4;
                    p._DeltaSize = 0;
                } }
            }
        );
    }
}
