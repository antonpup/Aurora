using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using Aurora.Profiles.LeagueOfLegends.GSI.Nodes;
using DK = Aurora.Devices.DeviceKeys;


namespace Aurora.Profiles.LeagueOfLegends
{
    public class LoLGSIProfile : ApplicationProfile
    {
        public LoLGSIProfile() : base()
        {

        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Dead", new SolidFillLayerHandler()
                {
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(200,0,0,0)
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled",new BooleanGSIBoolean("Player/IsDead"))),
                new Layer("Spell D", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new DK[] { DK.D})
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(255, 204, 0), new BooleanGSIEnum("Player/SpellD", SummonerSpell.Flash))
                        .AddEntry(Color.Red, new BooleanGSIEnum("Player/SpellD", SummonerSpell.Ignite))
                        .AddEntry(Color.Green, new BooleanGSIEnum("Player/SpellD", SummonerSpell.Heal))
                        .AddEntry(Color.Purple, new BooleanGSIEnum("Player/SpellD", SummonerSpell.Teleport))
                        .AddEntry(Color.Orange, new BooleanGSIEnum("Player/SpellD", SummonerSpell.Barrier))
                )),
                new Layer("Spell F", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Transparent,
                        _Sequence = new KeySequence(new DK[] { DK.F})
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(255, 204, 0), new BooleanGSIEnum("Player/SpellF", SummonerSpell.Flash))
                        .AddEntry(Color.Red, new BooleanGSIEnum("Player/SpellF", SummonerSpell.Ignite))
                        .AddEntry(Color.Green, new BooleanGSIEnum("Player/SpellF", SummonerSpell.Heal))
                        .AddEntry(Color.Purple, new BooleanGSIEnum("Player/SpellF", SummonerSpell.Teleport))
                        .AddEntry(Color.Orange, new BooleanGSIEnum("Player/SpellF", SummonerSpell.Barrier))
                )),
                new Layer("Ability Q", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new DK[] { DK.Q})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Abilities/Q/Level", ComparisonOperator.NEQ, 0))),
                new Layer("Ability W", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new DK[] { DK.W})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Abilities/W/Level", ComparisonOperator.NEQ, 0))),
                new Layer("Ability E", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new DK[] { DK.E})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Abilities/E/Level", ComparisonOperator.NEQ, 0))),
                new Layer("Ability R", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence(new DK[] { DK.R})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Abilities/R/Level", ComparisonOperator.NEQ, 0))),
                new Layer("Item 1", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new DK[] { DK.ONE})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Items/Slot1/ItemID", ComparisonOperator.NEQ, 0))),
                new Layer("Item 2", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new DK[] { DK.TWO})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Items/Slot2/ItemID", ComparisonOperator.NEQ, 0))),
                new Layer("Item 3", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new DK[] { DK.THREE})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Items/Slot3/ItemID", ComparisonOperator.NEQ, 0))),
                new Layer("Trinket", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new DK[] { DK.FOUR})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Items/Trinket/ItemID", ComparisonOperator.NEQ, 0))),
                new Layer("Item 4", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new DK[] { DK.FIVE})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Items/Slot4/ItemID", ComparisonOperator.NEQ, 0))),
                new Layer("Item 5", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new DK[] { DK.SIX})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Items/Slot5/ItemID", ComparisonOperator.NEQ, 0))),
                new Layer("Item 6", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.White,
                        _Sequence = new KeySequence(new DK[] { DK.SEVEN})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Player/Items/Slot6/ItemID", ComparisonOperator.NEQ, 0))),
                new Layer("Health", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Green,
                        _SecondaryColor = Color.Black,
                        _Sequence = new KeySequence(new DK[]{ DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12 }),
                        _VariablePath = "Player/ChampionStats/HealthCurrent",
                        _MaxVariablePath = "Player/ChampionStats/HealthMax"
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Match/InGame"))),
                new Layer("Resource", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Blue,
                        _SecondaryColor = Color.Black,
                        _Sequence = new KeySequence(new DK[]{ DK.Z, DK.X, DK.C, DK.V, DK.B, DK.N, DK.M, DK.COMMA, DK.PERIOD, DK.FORWARD_SLASH }),
                        _VariablePath = "Player/ChampionStats/ResourceCurrent",
                        _MaxVariablePath = "Player/ChampionStats/ResourceMax"
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanAnd(new IEvaluatable<bool>[] { new BooleanGSIBoolean("Match/InGame"), new BooleanNot( new BooleanGSIEnum("Player/ChampionStats/ResourceType", ResourceType.None)) }))),
                new Layer("Recall", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(79, 234, 255),
                        _Sequence = new KeySequence(new DK[] { DK.B})
                    }
                }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Match/InGame"))),
                new Layer("Background", new Layers.LoLBackgroundLayerHandler()),
            };
        }
    }
}
