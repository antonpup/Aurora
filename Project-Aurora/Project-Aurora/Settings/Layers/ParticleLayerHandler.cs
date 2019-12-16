using Aurora.EffectsEngine;
using Aurora.Profiles;
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
    /// Properties for the <see cref="ParticleLayerHandler"/>.
    /// </summary>
    public class ParticleLayerProperties : LayerHandlerProperties<ParticleLayerProperties>, INotifyPropertyChanged {

        // The shortest time (in seconds) between particles spawning. A random time between this value and "MaxSpawnTime" will be chosen.
        private float? minSpawnTime;
        public float? _MinSpawnTime { get => minSpawnTime; set => SetAndNotify(ref minSpawnTime, value); }
        [JsonIgnore] public float MinSpawnTime => Logic._MinSpawnTime ?? _MinSpawnTime ?? .5f;

        // The longest time (in seconds) between particles spawning. A random time from "MinSpawnTime" up to this value will be chosen.
        private float? maxSpawnTime;
        public float? _MaxSpawnTime { get => maxSpawnTime; set => SetAndNotify(ref maxSpawnTime, value); }
        [JsonIgnore] public float MaxSpawnTime => Logic._MaxSpawnTime ?? _MaxSpawnTime ?? 1f;

        // The smallest quantity of particles that will spawn at a time. A random value between this value and "MaxSpawnAmount" will be chosen.
        private int? minSpawnAmount;
        public int? _MinSpawnAmount { get => minSpawnAmount; set => SetAndNotify(ref minSpawnAmount, value); }
        [JsonIgnore] public int MinSpawnAmount => Logic._MinSpawnAmount ?? _MinSpawnAmount ?? 1;

        // The largest quantity of particles that will spawn at a time. A random value between "MinSpawnAmount" and this value will be chosen.
        private int? maxSpawnAmount;
        public int? _MaxSpawnAmount { get => maxSpawnAmount; set => SetAndNotify(ref maxSpawnAmount, value); }
        [JsonIgnore] public int MaxSpawnAmount => Logic._MaxSpawnAmount ?? _MaxSpawnAmount ?? 1;

        // The smallest possible initial horizontal velocity of the spawned particles. A random value between this value and "MaxInitialVelocityX" will be chosen for each particle.
        private float? minInitialVelocityX;
        public float? _MinInitialVelocityX { get => minInitialVelocityX; set => SetAndNotify(ref minInitialVelocityX, value); }
        [JsonIgnore] public float MinInitialVelocityX => Logic._MinInitialVelocityX ?? _MinInitialVelocityX ?? 0f;

        // The largest possible initial horizontal velocity of the spawned particles. A random value between "MinInitialVelocityX" and this value will be chosen for each particle.
        private float? maxInitialVelocityX;
        public float? _MaxInitialVelocityX { get => maxInitialVelocityX; set => SetAndNotify(ref maxInitialVelocityX, value); }
        [JsonIgnore] public float MaxInitialVelocityX => Logic._MaxInitialVelocityX ?? _MaxInitialVelocityX ?? 0f;

        // The smallest possible initial vertical velocity of the spawned particles. A random value between this value and "MaxInitialVelocityY" will be chosen for each particle.
        private float? minInitialVelocityY;
        public float? _MinInitialVelocityY { get => minInitialVelocityY; set => SetAndNotify(ref minInitialVelocityY, value); }
        [JsonIgnore] public float MinInitialVelocityY => Logic._MinInitialVelocityY ?? _MinInitialVelocityY ?? 0f;

        // The largest possible initial vertical velocity of the spawned particles. A random value between "MinInitialVelocityY" and this value will be chosen for each particle.
        private float? maxInitialVelocityY;
        public float? _MaxInitialVelocityY { get => maxInitialVelocityY; set => SetAndNotify(ref maxInitialVelocityY, value); }
        [JsonIgnore] public float MaxInitialVelocityY => Logic._MaxInitialVelocityY ?? _MaxInitialVelocityY ?? 0f;

        // The minimum possible lifetime of the particles (in seconds). A random lifetime between this number and "MaxLifetime" will be chosen.
        private float? minLifetime;
        public float? _MinLifetime { get => minLifetime; set => SetAndNotify(ref minLifetime, value); }
        [JsonIgnore] public float MinLifetime => Logic._MinLifetime ?? _MinLifetime ?? 3f;

        // The maximum possible lifetime of the particles (in seconds). A random lifetime between from "MinLifetime" up to this number will be chosen.
        private float? maxLifetime;
        public float? _MaxLifetime { get => maxLifetime; set => SetAndNotify(ref maxLifetime, value); }
        [JsonIgnore] public float MaxLifetime => Logic._MaxLifetime ?? _MaxLifetime ?? 3f;

        // The amount the speed of the particle in the horizontal direction will change per second.
        private float? accelerationX;
        public float? _AccelerationX { get => accelerationX; set => SetAndNotify(ref accelerationX, value); }
        [JsonIgnore] public float AccelerationX => Logic._AccelerationX ?? _AccelerationX ?? 0f;

        // The amount the speed of the particle in the vertical direction will change per second.
        private float? accelerationY;
        public float? _AccelerationY { get => accelerationY; set => SetAndNotify(ref accelerationY, value); }
        [JsonIgnore] public float AccelerationY => Logic._AccelerationY ?? _AccelerationY ?? -1f;

        // The amount of velocity per second the particle loses in the horizontal direction as a percentage of its current velocity
        private float? dragX;
        public float? _DragX { get => dragX; set => SetAndNotify(ref dragX, value); }
        [JsonIgnore] public float DragX => Logic._DragX ?? _DragX ?? 0;

        // The amount of velocity per second the particle loses in the vertical direction as a percentage of its current velocity
        private float? dragY;
        public float? _DragY { get => dragY; set => SetAndNotify(ref dragY, value); }
        [JsonIgnore] public float DragY => Logic._DragY ?? _DragY ?? 0;

        // Where the particles will spawn from
        private ParticleSpawnLocations? spawnLocation;
        public ParticleSpawnLocations? _SpawnLocation { get => spawnLocation; set => SetAndNotify(ref spawnLocation, value); }
        [JsonIgnore] public ParticleSpawnLocations SpawnLocation => Logic._SpawnLocation ?? _SpawnLocation ?? ParticleSpawnLocations.BottomEdge;

        // Whether or not the particles will spawn. This allows the particle system to be turned off without disabling it (thereby not hiding already spawned particles).
        // This can only be done by the overrides system.
        [LogicOverridable("Enable Particle Spawning")] public bool? _SpawningEnabled { get; set; }
        [JsonIgnore] public bool SpawningEnabled => Logic._SpawningEnabled ?? true;

        // The color gradient stops for the particle. Not using a linear brush here because:
        //   1) there are multithreading issues when trying to access a Media brush's gradient collection since it belongs to the UI thread
        //   2) We don't actually need the gradient as a brush since we're not drawing particles as gradients, only a solid color based on their lifetime, so we only need to access the color stops
        private List<(Color color, float offset)> particleColorStops;
        public List<(Color color, float offset)> _ParticleColorStops { get => particleColorStops; set => SetAndNotify(ref particleColorStops, value); }
        [JsonIgnore] public List<(Color color, float offset)> ParticleColorStops => Logic._ParticleColorStops ?? _ParticleColorStops ?? defaultParticleColor;

        private static readonly List<(Color, float)> defaultParticleColor = new List<(Color, float)> {
            (Color.White, 0f),
            (Color.FromArgb(0, Color.White), 1f)
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public ParticleLayerProperties() : base() { }
        public ParticleLayerProperties(bool empty = false) : base(empty) { }

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
            _Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
        }

        private void SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propName = null) {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }


    /// <summary>
    /// A layer that constantly renders randomised particles.
    /// </summary>
    public class ParticleLayerHandler : LayerHandler<ParticleLayerProperties>, INotifyRender {

        private static readonly Random rnd = new Random();
        private readonly Stopwatch stopwatch = new Stopwatch(); // Stopwatch to determine time difference since last render
        private readonly List<Particle> particles = new List<Particle>(); // All the currently active "alive" particles
        private double nextSpawnInterval = 0; // How many seconds until the next set of particles should spawn

        public event EventHandler<Bitmap> LayerRender; // Fires whenever the layer is rendered

        public ParticleLayerHandler() {
            _ID = "Particle";
        }

        protected override UserControl CreateControl() => new Control_ParticleLayer(this);

        public override EffectLayer Render(IGameState gamestate) {
            var layer = new EffectLayer();

            // Get elapsed time since last render
            var deltaTime = stopwatch.ElapsedMilliseconds / 1000d;
            stopwatch.Restart();
            
            if (particles.Count > 0) {
                // Order all the gradient stops of the current particle color
                var stops = Properties.ParticleColorStops.OrderBy(s => s.offset).ToArray();

                // Gets the color of a given particle, based on the current particle brush.
                Color GetParticleColor(Particle p) {
                    var offset = p.Lifetime / p.MaxLifetime;

                    // Check if any GradientStops are exactly at the requested offset. If so, return that
                    var exact = stops.Where(s => s.offset == offset);
                    if (exact.Count() == 1) return exact.Single().color;

                    // Check if the requested offset is outside of bounds of the offset range. If so, return the nearest offset
                    if (offset <= stops.First().offset) return stops.First().color;
                    if (offset >= stops.Last().offset) return stops.Last().color;

                    // Find the two stops either side of the requsted offset
                    var left = stops.Last(s => s.offset < offset);
                    var right = stops.First(s => s.offset > offset);

                    // Return the blended color that is the correct ratio between left and right
                    return ColorUtils.BlendColors(left.color, right.color, (offset - left.offset) / (right.offset - left.offset));
                }

                // Update and render all the current particles
                using (var gfx = layer.GetGraphics()) {
                    foreach (var particle in particles) {
                        particle.Lifetime += deltaTime;
                        if (particle.Lifetime < particle.MaxLifetime) { // Only bother to render particle if less than max life. (Cannot delete yet as it causes an exception due to the collection being modified)
                            UpdateParticle(particle, deltaTime);
                            gfx.FillEllipse(new SolidBrush(GetParticleColor(particle)), particle.DrawRect);
                        }
                    }
                }
            }

            // Spawn new particles if required
            if (Properties.SpawningEnabled) {
                nextSpawnInterval -= deltaTime;
                if (nextSpawnInterval < 0) {
                    var count = rnd.Next(Properties.MinSpawnAmount, Properties.MaxSpawnAmount + 1); // + 1 because max is exclusive
                    for (var i = 0; i < count; i++)
                        SpawnParticle();
                    nextSpawnInterval = RandomBetween(Properties.MinSpawnTime, Properties.MaxSpawnTime);
                }
            }

            // Remove any particles that have expired
            particles.RemoveAll(p => p.Lifetime > p.MaxLifetime);

            // Call the render event
            LayerRender?.Invoke(this, layer.GetBitmap());

            return layer;
        }

        /// <summary>
        /// Creates a new single particle, whose inital settings are based on the current Properties.
        /// </summary>
        private void SpawnParticle() {
            var ar = Properties.Sequence.GetAffectedRegion();
            particles.Add(new Particle {
                MaxLifetime = RandomBetween(Properties.MinLifetime, Properties.MaxLifetime),
                VelocityX = RandomBetween(Properties.MinInitialVelocityX, Properties.MaxInitialVelocityX),
                VelocityY = RandomBetween(Properties.MinInitialVelocityY, Properties.MaxInitialVelocityY),
                PositionX
                    = Properties.SpawnLocation == ParticleSpawnLocations.LeftEdge ? 0 // For left edge, X should start at 0
                    : Properties.SpawnLocation == ParticleSpawnLocations.RightEdge ? Effects.canvas_width // For right edge, X should start at maximum width
                    : Properties.SpawnLocation == ParticleSpawnLocations.Region ? ar.Left + (float)(rnd.NextDouble() * ar.Width)// For region, randomly choose X in region
                    : (float)(rnd.NextDouble() * Effects.canvas_width), // For top, bottom or random, randomly choose an X value
                PositionY
                    = Properties.SpawnLocation == ParticleSpawnLocations.TopEdge ? 0 // For top edge, Y should start at 0
                    : Properties.SpawnLocation == ParticleSpawnLocations.BottomEdge ? Effects.canvas_height // For bottom edge, Y should start at maximum height
                    : Properties.SpawnLocation == ParticleSpawnLocations.Region ? ar.Top + (float)(rnd.NextDouble() * ar.Height)// For region, randomly choose Y in region
                    : (float)(rnd.NextDouble() * Effects.canvas_height), // For left, right or random, randomly choose a Y value
            });
        }

        /// <summary>
        /// Updates the velocity and position of the given particle.
        /// </summary>
        private void UpdateParticle(Particle p, double deltaTime) {
            p.VelocityX += (float)(Properties.AccelerationX * deltaTime);
            p.VelocityY += (float)(Properties.AccelerationY * deltaTime);
            p.VelocityX *= (float)Math.Pow(1 - Properties.DragX, deltaTime); // By powering the drag to the deltaTime, we ensure that the results are fairly consistent over different time deltas.
            p.VelocityY *= (float)Math.Pow(1 - Properties.DragY, deltaTime); // Doing it once over a second won't be 100% the same as doing it twice over a second if acceleration is present, but it should be close enough that it won't be noticed under most cicrumstances
            p.PositionX += p.VelocityX;
            p.PositionY += p.VelocityY;
        }

        /// <summary>
        /// Picks a random floating point number between the two given numbers.
        /// </summary>
        private static float RandomBetween(float a, float b) => (float)((rnd.NextDouble() * (b - a)) + a);


        /// <summary>
        /// A class that holds the data for a single particle.
        /// </summary>
        private class Particle {

            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float VelocityX { get; set; }
            public float VelocityY { get; set; }

            /// <summary>The rectangle that bounds the rendered particle's circle.</summary>
            // TODO: Add resize options for particles
            public RectangleF DrawRect => new RectangleF(PositionX - 3, PositionY - 3, 6, 6);

            /// <summary>Current lifetime.</summary>
            public double Lifetime { get; set; }

            /// <summary>Target maximum lifetime.</summary>
            public double MaxLifetime { get; set; }
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
        public static ReadOnlyDictionary<string, Action<ParticleLayerProperties>> Presets { get; } = new ReadOnlyDictionary<string, Action<ParticleLayerProperties>>(
            new Dictionary<string, Action<ParticleLayerProperties>> {
                { "Fire", p => {
                    p._SpawnLocation = ParticleSpawnLocations.BottomEdge;
                    p._ParticleColorStops = new List<(Color color, float offset)> {
                        (Color.Yellow, 0f),
                        (Color.FromArgb(128, Color.Red), 0.6f),
                        (Color.FromArgb(0, Color.Black), 1f)
                    };
                    p._MinSpawnTime = p._MaxSpawnTime = .05f;
                    p._MinSpawnAmount = 4; p._MaxSpawnAmount = 6;
                    p._MinLifetime = .5f; p._MaxLifetime = 2;
                    p._MinInitialVelocityX = p._MaxInitialVelocityX = 0;
                    p._MinInitialVelocityY = -1.3f; p._MaxInitialVelocityY = -0.8f;
                    p._AccelerationX = 0;
                    p._AccelerationY = 0.5f;
                } },
                { "Matrix", p => {
                    p._SpawnLocation = ParticleSpawnLocations.TopEdge;
                    p._ParticleColorStops = new List<(Color color, float offset)> {
                        (Color.FromArgb(0,255,0), 0f),
                        (Color.FromArgb(0,255,0), 1f)
                    };
                    p._MinSpawnTime = .1f; p._MaxSpawnTime = .2f;
                    p._MinSpawnAmount = 1; p._MaxSpawnAmount = 2;
                    p._MinLifetime = p._MaxLifetime = 1;
                    p._MinInitialVelocityX = p._MaxInitialVelocityX = 0;
                    p._MinInitialVelocityY = p._MaxInitialVelocityY = 3;
                    p._AccelerationX = 0;
                    p._AccelerationY = 0;
                } }
            }
        );
    }
}
