using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.Minecraft.GSI;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.Minecraft.Layers {

    public class MinecraftRainLayerHandler : LayerHandler<LayerHandlerProperties> {

        private List<Droplet> raindrops = new List<Droplet>();
        private Random rnd = new Random();
        private int frame = 0;

        public MinecraftRainLayerHandler() : base() {
            _ID = "MinecraftRainLayer";
        }

        protected override UserControl CreateControl() {
            return new Control_NoOptions();
        }

        private void CreateRainDrop() {
            float randomX = (float)rnd.NextDouble() * Effects.canvas_width;
            raindrops.Add(new Droplet() {
                mix = new AnimationMix(new[] {
                    new AnimationTrack("raindrop", 0)
                        .SetFrame(0, new AnimationFilledRectangle(randomX, 0, 3, 6, Color.Cyan))
                        .SetFrame(1, new AnimationFilledRectangle(randomX + 5, Effects.canvas_height, 2, 4, Color.Cyan))
                }),
                time = 0
            });
        }

        public override EffectLayer Render(IGameState gamestate) {
            EffectLayer layer = new EffectLayer("Minecraft Rain Layer");

            if (!(gamestate is GameState_Minecraft)) return layer;

            // Add more droplets based on the intensity
            float strength = (gamestate as GameState_Minecraft).World.RainStrength;
            if (strength > 0) {
                frame++;
                if (frame > (1 - strength) * 10) {
                    CreateRainDrop();
                    if (strength > 0.8) CreateRainDrop(); // Create 2 per frame if it's very intense rain
                    frame = 0;
                }
            }

            // Render all droplets
            foreach (var droplet in raindrops) {
                droplet.mix.Draw(layer.GetGraphics(), droplet.time);
                droplet.time += .1f;
            }

            // Remove any expired droplets
            raindrops.RemoveAll(droplet => droplet.time >= 1);

            return layer;
        }
    }

    internal class Droplet {
        internal AnimationMix mix;
        internal float time;
    }
}
