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
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>
            {
                new Layer("Dead", new SolidFillLayerHandler()
                {
                    Properties = new SolidFillLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(200, 0, 0, 0)
                    }
                }, InGameAnd(new BooleanGSIBoolean("Player/IsDead"))),
                GetSpellLayer("D", DK.D),
                GetSpellLayer("F", DK.F),
                GetAbilityLayer("Q", DK.Q),
                GetAbilityLayer("W", DK.W),
                GetAbilityLayer("E", DK.E),
                GetAbilityLayer("R", DK.R),
                GetItemLayer("Item 1", "Player/Items/Slot1", DK.ONE),
                GetItemLayer("Item 2", "Player/Items/Slot2", DK.TWO),
                GetItemLayer("Item 3", "Player/Items/Slot3", DK.THREE),
                GetItemLayer("Trinket", "Player/Items/Trinket", DK.FOUR),
                GetItemLayer("Item 4", "Player/Items/Slot4", DK.FIVE),
                GetItemLayer("Item 5", "Player/Items/Slot5", DK.SIX),
                GetItemLayer("Item 6", "Player/Items/Slot6", DK.SEVEN),
                new Layer("Recall", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.FromArgb(79, 234, 255),
                        _Sequence = new KeySequence(new DK[] { DK.B })
                    }
                }, EnabledWhen(new BooleanGSIBoolean("Match/InGame"))),
                new Layer("Health", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Green,
                        _SecondaryColor = Color.Black,
                        _Sequence = new KeySequence(new DK[] { DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12 }),
                        _VariablePath = "Player/ChampionStats/HealthCurrent",
                        _MaxVariablePath = "Player/ChampionStats/HealthMax"
                    }
                }, EnabledWhen(new BooleanGSIBoolean("Match/InGame"))),
                new Layer("Resource", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor = Color.DarkBlue,
                        _SecondaryColor = Color.Black,
                        _Sequence = new KeySequence(new DK[] { DK.Z, DK.X, DK.C, DK.V, DK.B, DK.N, DK.M, DK.COMMA, DK.PERIOD, DK.FORWARD_SLASH }),
                        _VariablePath = "Player/ChampionStats/ResourceCurrent",
                        _MaxVariablePath = "Player/ChampionStats/ResourceMax"
                    }
                }, InGameAnd(new BooleanNot(new BooleanGSIEnum("Player/ChampionStats/ResourceType", ResourceType.None)))),
                new Layer("Background", new Layers.LoLBackgroundLayerHandler())
            };
        }

        public static OverrideLogicBuilder InGameAnd(Evaluatable<bool> eval)
        {
            return new OverrideLogicBuilder().SetDynamicBoolean("_Enabled",
                new BooleanAnd(
                    new Evaluatable<bool>[] {
                        new BooleanGSIBoolean("Match/InGame"),
                        eval
                    }
                )
            );
        }

        public static OverrideLogicBuilder EnabledWhen(Evaluatable<bool> condition)
        {
            return new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", condition);
        }

        public static Layer GetItemLayer(string name, string item, DK key)
        {
            return new Layer(name, new SolidColorLayerHandler()
            {
                Properties = new LayerHandlerProperties()
                {
                    _PrimaryColor = Color.Transparent,
                    _Sequence = new KeySequence(new DK[] { key })
                }
            },
            new OverrideLogicBuilder()
                .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Match/InGame"))
                .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                    .AddEntry(Color.White, new BooleanGSIBoolean(item + "/CanUse"))
                    .AddEntry(Color.FromArgb(75, 75, 75), new BooleanGSIBoolean(item + "/HasItem"))
                )
            );
        }

        public static Layer GetAbilityLayer(string ability, DK key)
        {
            return new Layer("Ability " + ability, new SolidColorLayerHandler()
            {
                Properties = new LayerHandlerProperties()
                {
                    _PrimaryColor = Color.Orange,
                    _Sequence = new KeySequence(new DK[] { key })
                }
            }, EnabledWhen(new BooleanGSIBoolean("Player/Abilities/" + ability + "/Learned")));
        }

        public static Layer GetSpellLayer(string spell, DK key)
        {
            return new Layer("Spell " + spell, new SolidColorLayerHandler()
            {
                Properties = new LayerHandlerProperties()
                {
                    _PrimaryColor = Color.Transparent,
                    _Sequence = new KeySequence(new DK[] { key })
                }
            }, new OverrideLogicBuilder()
                .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(255, 204, 0), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Flash))
                        .AddEntry(Color.FromArgb(255, 0, 0), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Ignite))
                        .AddEntry(Color.FromArgb(0, 255, 0), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Heal))
                        .AddEntry(Color.FromArgb(95, 0, 163), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Teleport))
                        .AddEntry(Color.FromArgb(240, 184, 72), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Barrier))
                        .AddEntry(Color.FromArgb(148, 97, 9), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Smite))
                        .AddEntry(Color.FromArgb(207, 214, 9), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Exhaust))
                        .AddEntry(Color.FromArgb(49, 247, 231), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Ghost))
                        .AddEntry(Color.FromArgb(11, 227, 166), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Cleanse))
                        .AddEntry(Color.FromArgb(126, 241, 247), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Mark))
                        .AddEntry(Color.FromArgb(199, 126, 247), new BooleanGSIEnum("Player/Spell" + spell, SummonerSpell.Dash))
                )
            );
        }
    }
}
