using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
//using Aurora.Profiles.Slime_Rancher.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;

namespace Aurora.Profiles.Slime_Rancher
{

    public class SlimeRancherProfile : ApplicationProfile
    {

        public SlimeRancherProfile() : base() { }

        public override void Reset()
        {
            base.Reset();

            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>() {
                new Layer("Menu Fill", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties() {
                        _PrimaryColor = Color.Transparent,
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(228, 67, 108), new BooleanGSIEnum("GameState/State", GSI.Nodes.GameStateEnum.Menu))
                        .AddEntry(Color.FromArgb(2, 80, 146), new BooleanGSIEnum("GameState/State", GSI.Nodes.GameStateEnum.Loading))
                        .AddEntry(Color.FromArgb(242, 242, 242), new BooleanGSIBoolean("GameState/IsPaused"))
                    )
                ),

                new Layer("Slot Selected", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties() {
                        _PrimaryColor = Color.White,
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_Sequence", new OverrideLookupTableBuilder<KeySequence>()
                        .AddEntry(new KeySequence(new[] {DK.ONE}), new BooleanGSINumeric("VacPack/SellectedSlot",1))
                        .AddEntry(new KeySequence(new[] {DK.TWO}), new BooleanGSINumeric("VacPack/SellectedSlot",2))
                        .AddEntry(new KeySequence(new[] {DK.THREE}), new BooleanGSINumeric("VacPack/SellectedSlot",3))
                        .AddEntry(new KeySequence(new[] {DK.FOUR}), new BooleanGSINumeric("VacPack/SellectedSlot",4))
                        .AddEntry(new KeySequence(new[] {DK.FIVE}), new BooleanGSINumeric("VacPack/SellectedSlot",5))
                    )
                    .SetLookupTable("_Enabled", new OverrideLookupTableBuilder<bool>()
                        .AddEntry(false, new BooleanGSIBoolean("VacPack/InGadgetMode"))
                    )
                ),

                new Layer("Item Color", new SolidColorLayerHandler() {
                    Properties = new LayerHandlerProperties() {
                        _PrimaryColor = Color.Transparent,
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_Sequence", new OverrideLookupTableBuilder<KeySequence>()
                        .AddEntry(new KeySequence(new[] {DK.ONE}), new BooleanGSINumeric("VacPack/UseableSlots",1))
                        .AddEntry(new KeySequence(new[] {DK.ONE, DK.TWO}), new BooleanGSINumeric("VacPack/UseableSlots",2))
                        .AddEntry(new KeySequence(new[] {DK.ONE, DK.TWO, DK.THREE}), new BooleanGSINumeric("VacPack/UseableSlots",3))
                        .AddEntry(new KeySequence(new[] {DK.ONE, DK.TWO, DK.THREE, DK.FOUR}), new BooleanGSINumeric("VacPack/UseableSlots",4))
                        .AddEntry(new KeySequence(new[] {DK.ONE, DK.TWO, DK.THREE, DK.FOUR, DK.FIVE}), new BooleanGSINumeric("VacPack/UseableSlots",ComparisonOperator.GTE,5))
                    )
                    .SetLookupTable("_Enabled", new OverrideLookupTableBuilder<bool>()
                        .AddEntry(false, new BooleanGSIBoolean("VacPack/InGadgetMode"))
                    )
                    .SetDynamicColor("_PrimaryColor",
                    new NumberGSINumeric("VacPack/Color/SellectedSlot/Alpha"),
                    new NumberGSINumeric("VacPack/Color/SellectedSlot/Red"),
                    new NumberGSINumeric("VacPack/Color/SellectedSlot/Green"),
                    new NumberGSINumeric("VacPack/Color/SellectedSlot/Blue")
                    )
                ),

                new Layer("Health", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Health/Current",
                        _MaxVariablePath = "Player/Health/Max",
                        _PrimaryColor = Color.FromArgb(255, 17, 17),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.Q, DK.W, DK.E, DK.R, DK.T, DK.Y, DK.U, DK.I, DK.O, DK.P, DK.OPEN_BRACKET, DK.CLOSE_BRACKET
                        })
                    }
                }),

                new Layer("Energy", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Energy/Current",
                        _MaxVariablePath = "Player/Energy/Max",
                        _PrimaryColor = Color.FromArgb(9, 173, 233),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.A, DK.S, DK.D, DK.F, DK.G, DK.H, DK.J, DK.K, DK.L, DK.SEMICOLON, DK.APOSTROPHE
                        })
                    }
                }),

                new Layer("Radiation", new PercentLayerHandler() {
                    Properties = new PercentLayerHandlerProperties() {
                        _VariablePath = "Player/Radiation/Current",
                        _MaxVariablePath = "Player/Radiation/Max",
                        _PrimaryColor = Color.FromArgb(60, 233, 118),
                        _SecondaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new[] {
                            DK.Z, DK.X, DK.C, DK.V, DK.B, DK.N, DK.M, DK.COMMA, DK.PERIOD, DK.FORWARD_SLASH
                        })
                    }
                }),

                new Layer("Gadget Mode", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties() {
                        _PrimaryColor = Color.FromArgb(59,62,129)
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("VacPack/InGadgetMode"))
                ),

                new Layer("Location Color", new SolidFillLayerHandler() {
                    Properties = new SolidFillLayerHandlerProperties() {
                        _PrimaryColor = Color.Transparent,
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(214, 178, 133), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheRanch))
                        .AddEntry(Color.FromArgb(189,66,80), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheDryReef))
                        .AddEntry(Color.FromArgb(83,85,161), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheIndigoQuarry))
                        .AddEntry(Color.FromArgb(28,128,90), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheMossBlanket))
                        .AddEntry(Color.FromArgb(175,104,66), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheGlassDesert))
                        .AddEntry(Color.FromArgb(9,138,153), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheSlimeSea))
                        .AddEntry(Color.FromArgb(182,201,89), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheAncientRuins))
                        .AddEntry(Color.FromArgb(182,201,89), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheAncientRuinsCourtyard))
                        .AddEntry(Color.FromArgb(81,108,69), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheWilds))
                        .AddEntry(Color.FromArgb(128,146,35), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.OgdensRetreat))
                        .AddEntry(Color.FromArgb(192,192,192), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.NimbleValley))
                        .AddEntry(Color.FromArgb(107,82,170), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.MochisManor))
                        .AddEntry(Color.FromArgb(184,30,107), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.TheSlimeulation))
                        .AddEntry(Color.FromArgb(107,163,209), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.ViktorsWorkshop))
                        .AddEntry(Color.FromArgb(9,138,153), new BooleanGSIEnum("Location/Location", GSI.Nodes.SRZone.None))
                    )
                ),
            };
        }

    }
}