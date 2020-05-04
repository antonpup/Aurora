using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Profiles.StardewValley
{
    public class StardewValleyProfile : ApplicationProfile
    {
        public override void Reset()
        {
            base.Reset();

            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Background/Season", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties {
                        _PrimaryColor = Color.Transparent,
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor",new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.ForestGreen,
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Spring))
                        .AddEntry(Color.Gold,
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Summer))
                        .AddEntry(Color.Brown,
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Fall))
                        .AddEntry(Color.DeepSkyBlue,
                                new BooleanGSIEnum("World/Season", GSI.Nodes.Seasons.Winter))
                    )
                ),
            };
        }

    }
}
