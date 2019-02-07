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
                new Layer("Rank Indicator Layer", new Layers.ResidentEvil2RankLayerHandler()),
                new Layer("Status Indicator Layer", new Layers.ResidentEvil2HealthLayerHandler())
            };
        }
    }
}
