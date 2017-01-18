using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.Dota_2.GSI;
using Aurora.Profiles.Dota_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.Dota_2.Layers
{
    public class Dota2ItemEffectsLayerHandlerProperties : LayerHandlerProperties2Color<Dota2ItemEffectsLayerHandlerProperties>
    {
        public Dota2ItemEffectsLayerHandlerProperties() : base() { }

        public Dota2ItemEffectsLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
        }
    }

    public class Dota2ItemEffectsLayerHandler : LayerHandler<Dota2ItemEffectsLayerHandlerProperties>
    {
        enum Dota2ItemEffects
        {
            None,
            dagon
        }

        private AnimationTrack dagon_track;
        
        private long previoustime = 0;
        private long currenttime = 0;

        private float getDeltaTime()
        {
            return (currenttime - previoustime) / 1000.0f;
        }

        private static float itemeffect_keyframe = 0.0f;
        private static Dota2ItemEffects currentitemeffect = Dota2ItemEffects.None;
        private static float itemeffect_time = 0.0f;

        private Random randomizer = new Random();

        private static Items_Dota2 items;

        public Dota2ItemEffectsLayerHandler() : base()
        {
            _Type = LayerType.Dota2Items;
        }

        protected override UserControl CreateControl()
        {
            return new Control_Dota2ItemEffectsLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            if (currenttime - previoustime > 300000 || (currenttime == 0 && previoustime == 0))
                UpdateAnimations();

            EffectLayer item_effects_layer = new EffectLayer("Dota 2 - Item Effects");

            if (state is GameState_Dota2)
            {
                GameState_Dota2 dota2state = state as GameState_Dota2;

                //Preparations
                if (items != null)
                {
                    //For each of your heroes abilities:
                    for (int item_id = 0; item_id < dota2state.Items.InventoryCount; item_id++)
                    {
                        Item item = dota2state.Items.InventoryItems[item_id];
                        //Detect if item has been cast
                        if (!item.CanCast && items[item_id].CanCast)
                        {
                            //Casted item
                            currentitemeffect = itemProps[item.Name](item);
                        }
                    }
                }

                items = dota2state.Items;
                //Begin rendering
                //Check if rendering is required
                if (itemeffect_keyframe >= itemeffect_time)
                {
                    currentitemeffect = Dota2ItemEffects.None;
                    itemeffect_keyframe = 0.0f;
                }

                //Set up some useful keyboard grid positions
                float mid_x = Effects.canvas_width / 2.0f;
                float mid_y = Effects.canvas_height / 2.0f;

                //Render effect for item cast
                switch (currentitemeffect)
                {
                    case Dota2ItemEffects.dagon:
                        dagon_track.GetFrame(itemeffect_keyframe).Draw(item_effects_layer.GetGraphics());
                        itemeffect_keyframe += getDeltaTime();
                        break;
                }
            }

            return item_effects_layer;
        }

        public override void SetProfile(ProfileManager profile)
        {
            (Control as Control_Dota2ItemEffectsLayer).SetProfile(profile);
        }

        public void UpdateAnimations()
        {
            // Dagon 5
            dagon_track = new AnimationTrack("Dagon 5", 0.5f);
            dagon_track.SetFrame(0.0f, new AnimationFilledCircle(-(Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 0, 0)));
            dagon_track.SetFrame(0.3f, new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f) * 0.9f, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 0, 0)));
            dagon_track.SetFrame(0.5f, new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 255, 0, 0)));
        }

        private static Dota2ItemEffects dagonProps(Item item)
        {
            itemeffect_time = 0.5f;
            itemeffect_keyframe = 0.0f;
            return Dota2ItemEffects.dagon;
        }
        
        private static Dota2ItemEffects defaultProps(Item item)
        {
            if (Global.isDebug) System.Diagnostics.Debug.WriteLine("Unknown Item: " + item.Name);
            return Dota2ItemEffects.None;
        }

        Dictionary<String, Func<Item, Dota2ItemEffects>> itemProps = new Dictionary<String, Func<Item, Dota2ItemEffects>>{
            {"item_dagon_5", dagonProps}
        };
    }
}
