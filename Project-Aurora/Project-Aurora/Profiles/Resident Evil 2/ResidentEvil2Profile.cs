using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
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
                new Layer("Poison Indicator", new ConditionalLayerHandler{
                    Properties = new ConditionalLayerProperties
                    {
                        _ConditionPath = "Player/Poison",
                        _PrimaryColor = Color.FromArgb(255, 128, 0, 128),
                        _SecondaryColor = Color.FromArgb(0, 0, 0, 0),
                        _Sequence = new KeySequence(new FreeFormObject(0, 140, 840, 80))
                    }
                }),
                new Layer("Status Indicator", new Layers.ResidentEvil2HealthLayerHandler())
            };
        }
    }
}
