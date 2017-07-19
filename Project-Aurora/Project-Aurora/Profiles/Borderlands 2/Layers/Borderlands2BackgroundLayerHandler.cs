using Aurora.EffectsEngine;
using Aurora.Profiles.Borderlands2.GSI;
using Aurora.Profiles.Borderlands2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.Borderlands2.Layers
{
    public class Borderlands2BackgroundLayerHandlerProperties : LayerHandlerProperties2Color<Borderlands2BackgroundLayerHandlerProperties>
    {
        public Color? _ColorBackground { get; set; }

        [JsonIgnore]
        public Color ColorBackground { get { return Logic._ColorBackground ?? _ColorBackground ?? Color.Empty; } }
        
        public Borderlands2BackgroundLayerHandlerProperties() : base() { }

        public Borderlands2BackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            
            this._ColorBackground = Color.LightGoldenrodYellow;
        }

    }

    public class Borderlands2BackgroundLayerHandler : LayerHandler<Borderlands2BackgroundLayerHandlerProperties>
    {
        public Borderlands2BackgroundLayerHandler() : base()
        {
            _ID = "Borderlands2Background";
        }

        protected override UserControl CreateControl()
        {
            return new Control_Borderlands2BackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer("Borderlands 2 - Background");
            

            if (state is GameState_Borderlands2)
            {
                GameState_Borderlands2 rlstate = state as GameState_Borderlands2;

                if (rlstate.Player.maximumHealth == 0.0f)
                {
                    //// Main Menu
                    bg_layer.Fill(Properties.ColorBackground);
                }
                else
                {
                    //// Not in Main Menu
                    if (rlstate.Player.currentHealth == 0.0f)
                    {
                        //// Dead
                        bg_layer.Fill(Properties.ColorBackground);
                    }
                    else
                    {
                        //// Alive
                        bg_layer.Fill(Properties.ColorBackground);
                    }
                }
            }

            return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Borderlands2BackgroundLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}