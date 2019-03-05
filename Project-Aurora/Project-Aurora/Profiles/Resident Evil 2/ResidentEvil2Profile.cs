using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Aurora.Profiles.ResidentEvil2
{
    public class ResidentEvil2Profile : ApplicationProfile
    {
        public ResidentEvil2Profile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Rank Indicator", new Layers.ResidentEvil2RankLayerHandler()),
                new Layer("Poison Indicator", new SolidColorLayerHandler{
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(255, 128, 0, 128),
                        _Sequence = new KeySequence(new FreeFormObject(0, 140, 840, 80))
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/Poison"))),
                new Layer("Status Indicator", new Layers.ResidentEvil2HealthLayerHandler())
            };
        }
    }
}
