using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.Dota_2.GSI;
using Aurora.Profiles.Dota_2.GSI.Nodes;
using Aurora.Settings.Layers;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Profiles.Dota_2.Layers
{
    public class Dota2HeroAbilityEffectsLayerHandlerProperties : LayerHandlerProperties2Color<Dota2HeroAbilityEffectsLayerHandlerProperties>
    {
        public Dota2HeroAbilityEffectsLayerHandlerProperties() : base() { }

        public Dota2HeroAbilityEffectsLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }
    }

    public class Dota2HeroAbilityEffectsLayerHandler : LayerHandler<Dota2HeroAbilityEffectsLayerHandlerProperties>
    {
        private enum Dota2AbilityEffects
        {
            None,
            AbaddonDeathCoil,
            AbaddonBorrowedTime,
            AlchemistAcidSpray,
            AncientApparitionIceVortex,
            AncientApparitionIceBlast,
            AntimageBlink,
            AntimageManaVoid,
            AxeBerserkersCall,
            BeastmasterPrimalRoar,
            BrewmasterThunderClap,
            CentaurHoofStomp,
            ChaosKnightChaosBolt,
            CrystalMaidenCrystalNova,
            DoomBringerScorchedEarth,
            DragonKnightBreatheFire,
            EarthshakerFissure,
            EarthshakerEchoSlam,
            ElderTitanEarthSplitter,

            KunkkaTorrent,
            KunkkaGhostship,
            LegionCommanderOverwhelmingOdds,
            LifeStealerRage,
            MagnataurShockwave,
            OmniknightPurification,
            OmniknightRepel,
            SandkingEpicenter,
            SlardarSlithereenCrush,

            LinaDragonSlave,
            LinaLightStrikeArray,
            LinaLagunaBlade,
            NevermoreShadowraze,
            NevermoreRequiem,
            RattletrapRocketFlare,
            RazorPlasmaField,
            RikiSmokeScreen,
            ZuusArcLightning,
            ZuusLightningBolt,
            ZuusThundergodsWrath

        }

        private AnimationTrack _razorPlasmaFieldTrack;
        private AnimationTrack _crystalMaidenCrystalNovaTrack;
        private AnimationTrack _rikiSmokeScreenTrack;
        private AnimationTrack _linaDragonSlaveTrack;
        private AnimationTrack _linaLightStrikeArrayTrack;
        private AnimationTrack _linaLagunaBladeTrack;
        private AnimationTrack _abaddonDeathCoilTrack;
        private AnimationTrack _nevermoreShadowrazeTrack;
        private AnimationTrack _nevermoreRequiemTrack;
        private AnimationTrack _zuusArcLightningTrack;
        private AnimationTrack _zuusLightningBoltTrack;
        private AnimationTrack _zuusLightningBoltShadeTrack;
        private AnimationTrack _antimageBlinkTrack;
        private AnimationTrack _antimageManaVoidTrack;
        private AnimationTrack _antimageManaVoidCoreTrack;
        private AnimationTrack _ancientApparitionIceBlastTrack;
        private AnimationTrack _axeBerserkersCallTrack;
        private AnimationTrack _beastmasterPrimalRoarTrack;
        private AnimationTrack _brewmasterThunderClapTrack;
        private AnimationTrack _centaurHoofStompTrack;
        private AnimationMix _chaosKnightChaosBoltMix;
        private AnimationTrack _rattletrapRocketFlareTrack;
        private AnimationTrack _dragonKnightBreatheFireTrack;
        private AnimationTrack _elderTitanEarthSplitterTrack;
        private AnimationMix _kunkkaTorrentMix;
        private AnimationTrack _kunkkaGhostshipTrack;
        private AnimationTrack _legionCommanderOverwhelmingOddsTrack;
        private AnimationTrack _lifeStealerRageTrack;
        private AnimationTrack _magnataurShockwaveTrack;
        private AnimationTrack _omniknightPurificationTrack;
        private AnimationTrack _omniknightRepelTrack;
        private AnimationMix _sandkingEpicenterMix;
        private AnimationTrack _slardarSlithereenCrushTrack;

        private long _previousTime;
        private long _currentTime;

        private float GetDeltaTime()
        {
            return (_currentTime - _previousTime) / 1000.0f;
        }

        private float _abilityEffectKeyframe;
        private Dota2AbilityEffects _currentAbilityEffect = Dota2AbilityEffects.None;
        private float _abilityEffectTime;

        private readonly Random _randomizer = new();

        private static Abilities_Dota2 _abilities;

        public Dota2HeroAbilityEffectsLayerHandler(): base("Dota 2 - Ability Effects")
        {
            WeakEventManager<Effects, CanvasChangedArgs>.AddHandler(null, nameof(Effects.CanvasChanged), Effects_CanvasChanged);
            UpdateAnimations();
        }

        private void Effects_CanvasChanged(object? sender, CanvasChangedArgs e)
        {
            UpdateAnimations();
        }

        protected override UserControl CreateControl()
        {
            return new Control_Dota2HeroAbilityEffectsLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            _previousTime = _currentTime;
            _currentTime = Utils.Time.GetMillisecondsSinceEpoch();

            if (state is not GameState_Dota2 dota2State) return EffectLayer.EmptyLayer;

            //Preparations
            if (_abilities != null && dota2State.Abilities.Count == _abilities.Count)
            {
                for (var abilityId = 0; abilityId < dota2State.Abilities.Count; abilityId++)
                {
                    var ability = dota2State.Abilities[abilityId];

                    if (ability.CanCast || !_abilities[abilityId].CanCast) continue;
                    switch (ability.Name)
                    {
                        //Casted ability
                        case "razor_plasma_field":
                            _currentAbilityEffect = Dota2AbilityEffects.RazorPlasmaField;
                            _abilityEffectTime = 2.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "crystal_maiden_crystal_nova":
                            _currentAbilityEffect = Dota2AbilityEffects.CrystalMaidenCrystalNova;
                            _abilityEffectTime = 1.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "riki_smoke_screen":
                            _currentAbilityEffect = Dota2AbilityEffects.RikiSmokeScreen;
                            _abilityEffectTime = 6.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "lina_dragon_slave":
                            _currentAbilityEffect = Dota2AbilityEffects.LinaDragonSlave;
                            _abilityEffectTime = 1.25f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "lina_light_strike_array":
                            _currentAbilityEffect = Dota2AbilityEffects.LinaLightStrikeArray;
                            _abilityEffectTime = 2.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "lina_laguna_blade":
                        {
                            _linaLagunaBladeTrack = new AnimationTrack("Lina Laguna Blade", 0.5f);

                            var lagunaPoint1 = new PointF(0, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));
                            var lagunaPoint2 = new PointF(Effects.CanvasWidthCenter + 3.0f, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));
                            var lagunaPoint3 = new PointF(Effects.CanvasWidthCenter + 6.0f, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));
                            var lagunaPoint4 = new PointF(Effects.CanvasWidthCenter + 9.0f, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));

                            _linaLagunaBladeTrack.SetFrame(0.0f,
                                new AnimationLines(
                                    new AnimationLine[] {
                                        new(new PointF(0, Effects.CanvasHeightCenter), lagunaPoint1, Color.FromArgb(255, 255, 255), 2),
                                        new(lagunaPoint1, lagunaPoint2, Color.FromArgb(255, 255, 255), Color.FromArgb(170, 170, 255), 3),
                                        new(lagunaPoint2, lagunaPoint3, Color.FromArgb(170, 170, 255), Color.FromArgb(85, 85, 255), 3),
                                        new(lagunaPoint3, lagunaPoint4, Color.FromArgb(85, 85, 255), Color.FromArgb(0, 0, 255), 5),
                                    }
                                )
                            );

                            _linaLagunaBladeTrack.SetFrame(0.45f,
                                new AnimationLines(
                                    new AnimationLine[] {
                                        new(new PointF(0, Effects.CanvasHeightCenter), lagunaPoint1, Color.FromArgb(255, 255, 255), 2),
                                        new(lagunaPoint1, lagunaPoint2, Color.FromArgb(255, 255, 255), Color.FromArgb(170, 170, 255), 3),
                                        new(lagunaPoint2, lagunaPoint3, Color.FromArgb(170, 170, 255), Color.FromArgb(85, 85, 255), 3),
                                        new(lagunaPoint3, lagunaPoint4, Color.FromArgb(85, 85, 255), Color.FromArgb(0, 0, 255), 5),
                                    }
                                )
                            );

                            _linaLagunaBladeTrack.SetFrame(0.5f,
                                new AnimationLines(
                                    new AnimationLine[] {
                                        new(new PointF(0, Effects.CanvasHeightCenter), lagunaPoint1, Color.FromArgb(0, 255, 255, 255), 2),
                                        new(lagunaPoint1, lagunaPoint2, Color.FromArgb(0, 255, 255, 255), Color.FromArgb(0, 170, 170, 255), 3),
                                        new(lagunaPoint2, lagunaPoint3, Color.FromArgb(0, 170, 170, 255), Color.FromArgb(0, 85, 85, 255), 3),
                                        new(lagunaPoint3, lagunaPoint4, Color.FromArgb(0, 85, 85, 255), Color.FromArgb(0, 0, 0, 255), 5),
                                    }
                                )
                            );

                            _currentAbilityEffect = Dota2AbilityEffects.LinaLagunaBlade;
                            _abilityEffectTime = 0.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        }
                        case "abaddon_death_coil":
                        {
                            _abaddonDeathCoilTrack = new AnimationTrack("Abaddon Dealth Coil", 0.5f);

                            var deathCoilPoint1 = new PointF(Effects.CanvasWidthCenter - 3.0f, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));
                            var deathCoilPoint2 = new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));
                            var deathCoilPoint3 = new PointF(Effects.CanvasWidthCenter + 3.0f, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));
                            var deathCoilPoint4 = new PointF(Effects.CanvasWidthCenter + 9.0f, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));

                            _abaddonDeathCoilTrack.SetFrame(0.0f,
                                new AnimationLines(
                                    new AnimationLine[] {
                                        new(new PointF(0, Effects.CanvasHeightCenter), deathCoilPoint1, Color.FromArgb(0, 160, 210), 2),
                                        new(deathCoilPoint1, deathCoilPoint2, Color.FromArgb(0, 160, 210), 3),
                                        new(deathCoilPoint2, deathCoilPoint3, Color.FromArgb(0, 160, 210), 3),
                                        new(deathCoilPoint3, deathCoilPoint4, Color.FromArgb(0, 160, 210), 5),
                                    }
                                )
                            );

                            _abaddonDeathCoilTrack.SetFrame(0.45f,
                                new AnimationLines(
                                    new AnimationLine[] {
                                        new(new PointF(0, Effects.CanvasHeightCenter), deathCoilPoint1, Color.FromArgb(0, 160, 210), 2),
                                        new(deathCoilPoint1, deathCoilPoint2, Color.FromArgb(0, 160, 210), 3),
                                        new(deathCoilPoint2, deathCoilPoint3, Color.FromArgb(0, 160, 210), 3),
                                        new(deathCoilPoint3, deathCoilPoint4, Color.FromArgb(0, 160, 210), 5),
                                    }
                                )
                            );

                            _abaddonDeathCoilTrack.SetFrame(0.5f,
                                new AnimationLines(
                                    new AnimationLine[] {
                                        new(new PointF(0, Effects.CanvasHeightCenter), deathCoilPoint1, Color.FromArgb(0, 0, 160, 210), 2),
                                        new(deathCoilPoint1, deathCoilPoint2, Color.FromArgb(0, 0, 160, 210), 3),
                                        new(deathCoilPoint2, deathCoilPoint3, Color.FromArgb(0, 0, 160, 210), 3),
                                        new(deathCoilPoint3, deathCoilPoint4, Color.FromArgb(0, 0, 160, 210), 5),
                                    }
                                )
                            );

                            _currentAbilityEffect = Dota2AbilityEffects.AbaddonDeathCoil;
                            _abilityEffectTime = 0.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        }
                        case "abaddon_borrowed_time":
                        {
                            _currentAbilityEffect = Dota2AbilityEffects.AbaddonBorrowedTime;

                            _abilityEffectTime = ability.Level switch
                            {
                                1 => 4.0f,
                                2 => 5.0f,
                                3 => 6.0f,
                                _ => 6.0f
                            };

                            _abilityEffectKeyframe = 0.0f;
                            break;
                        }
                        case "nevermore_shadowraze1":
                        case "nevermore_shadowraze2":
                        case "nevermore_shadowraze3":
                            _currentAbilityEffect = Dota2AbilityEffects.NevermoreShadowraze;
                            _abilityEffectTime = 0.7f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "nevermore_requiem":
                            _currentAbilityEffect = Dota2AbilityEffects.NevermoreRequiem;
                            _abilityEffectTime = 2.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "zuus_arc_lightning":
                        {
                            _zuusArcLightningTrack = new AnimationTrack("Zeus Arc Lightning", 0.5f);

                            var zuusLightningPoint1 = new PointF(Effects.CanvasWidthCenter - 3.0f, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));
                            var zuusLightningPoint2 = new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));
                            var zuusLightningPoint3 = new PointF(Effects.CanvasWidthCenter + 3.0f, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));
                            var zuusLightningPoint4 = new PointF(Effects.CanvasWidthCenter + 9.0f, Effects.CanvasHeightCenter + ((_randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)_randomizer.NextDouble()));

                            _zuusArcLightningTrack.SetFrame(0.0f,
                                new AnimationLines(
                                    new AnimationLine[] {
                                        new(new PointF(0, Effects.CanvasHeightCenter), zuusLightningPoint1, Color.FromArgb(0, 205, 255), 2),
                                        new(zuusLightningPoint1, zuusLightningPoint2, Color.FromArgb(0, 205, 255), 3),
                                        new(zuusLightningPoint2, zuusLightningPoint3, Color.FromArgb(0, 205, 255) , 3),
                                        new(zuusLightningPoint3, zuusLightningPoint4, Color.FromArgb(0, 205, 255), 5),
                                    }
                                )
                            );

                            _zuusArcLightningTrack.SetFrame(0.45f,
                                new AnimationLines(
                                    new AnimationLine[] {
                                        new(new PointF(0, Effects.CanvasHeightCenter), zuusLightningPoint1, Color.FromArgb(0, 205, 255), 2),
                                        new(zuusLightningPoint1, zuusLightningPoint2, Color.FromArgb(0, 205, 255), 3),
                                        new(zuusLightningPoint2, zuusLightningPoint3, Color.FromArgb(0, 205, 255) , 3),
                                        new(zuusLightningPoint3, zuusLightningPoint4, Color.FromArgb(0, 205, 255), 5),
                                    }
                                )
                            );

                            _zuusArcLightningTrack.SetFrame(0.5f,
                                new AnimationLines(
                                    new AnimationLine[] {
                                        new(new PointF(0, Effects.CanvasHeightCenter), zuusLightningPoint1, Color.FromArgb(0, 0, 205, 255), 2),
                                        new(zuusLightningPoint1, zuusLightningPoint2, Color.FromArgb(0, 0, 205, 255), 3),
                                        new(zuusLightningPoint2, zuusLightningPoint3, Color.FromArgb(0, 0, 205, 255), 3),
                                        new(zuusLightningPoint3, zuusLightningPoint4, Color.FromArgb(0, 0, 205, 255), 5),
                                    }
                                )
                            );

                            _currentAbilityEffect = Dota2AbilityEffects.ZuusArcLightning;
                            _abilityEffectTime = 0.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        }
                        case "zuus_lightning_bolt":
                            _currentAbilityEffect = Dota2AbilityEffects.ZuusLightningBolt;
                            _abilityEffectTime = 0.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "zuus_thundergods_wrath":
                            _currentAbilityEffect = Dota2AbilityEffects.ZuusThundergodsWrath;
                            _abilityEffectTime = 0.25f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "antimage_blink":
                            _currentAbilityEffect = Dota2AbilityEffects.AntimageBlink;
                            _abilityEffectTime = 0.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "antimage_mana_void":
                            _currentAbilityEffect = Dota2AbilityEffects.AntimageManaVoid;
                            _abilityEffectTime = 0.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "ancient_apparition_ice_vortex":
                            _currentAbilityEffect = Dota2AbilityEffects.AncientApparitionIceVortex;
                            _abilityEffectTime = 0.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "ancient_apparition_ice_blast":
                            _currentAbilityEffect = Dota2AbilityEffects.AncientApparitionIceBlast;
                            _abilityEffectTime = 1.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "alchemist_acid_spray":
                            _currentAbilityEffect = Dota2AbilityEffects.AlchemistAcidSpray;
                            _abilityEffectTime = 16.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "axe_berserkers_call":
                            _currentAbilityEffect = Dota2AbilityEffects.AxeBerserkersCall;
                            _abilityEffectTime = 0.7f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "beastmaster_primal_roar":
                            _currentAbilityEffect = Dota2AbilityEffects.BeastmasterPrimalRoar;
                            _abilityEffectTime = 1.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "brewmaster_thunder_clap":
                            _currentAbilityEffect = Dota2AbilityEffects.BrewmasterThunderClap;
                            _abilityEffectTime = 1.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "centaur_hoof_stomp":
                            _currentAbilityEffect = Dota2AbilityEffects.CentaurHoofStomp;
                            _abilityEffectTime = 1.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "chaos_knight_chaos_bolt":
                            _currentAbilityEffect = Dota2AbilityEffects.ChaosKnightChaosBolt;
                            _abilityEffectTime = 1.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "rattletrap_rocket_flare":
                            _currentAbilityEffect = Dota2AbilityEffects.RattletrapRocketFlare;
                            _abilityEffectTime = 0.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "doom_bringer_scorched_earth":
                        {
                            _currentAbilityEffect = Dota2AbilityEffects.DoomBringerScorchedEarth;

                            _abilityEffectTime = ability.Level switch
                            {
                                2 => 12.0f,
                                3 => 14.0f,
                                4 => 16.0f,
                                _ => 10.0f
                            };
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        }
                        case "dragon_knight_breathe_fire":
                            _currentAbilityEffect = Dota2AbilityEffects.DragonKnightBreatheFire;
                            _abilityEffectTime = 1.25f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "earthshaker_fissure":
                            _currentAbilityEffect = Dota2AbilityEffects.EarthshakerFissure;
                            _abilityEffectTime = 0.25f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "earthshaker_echo_slam":
                            _currentAbilityEffect = Dota2AbilityEffects.EarthshakerEchoSlam;
                            _abilityEffectTime = 0.25f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "elder_titan_earth_splitter":
                            _currentAbilityEffect = Dota2AbilityEffects.ElderTitanEarthSplitter;
                            _abilityEffectTime = 4.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "kunkka_torrent":
                            _currentAbilityEffect = Dota2AbilityEffects.KunkkaTorrent;
                            _abilityEffectTime = 4.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "kunkka_ghostship":
                            _currentAbilityEffect = Dota2AbilityEffects.KunkkaGhostship;
                            _abilityEffectTime = 2.7f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "legion_commander_overwhelming_odds":
                            _currentAbilityEffect = Dota2AbilityEffects.LegionCommanderOverwhelmingOdds;
                            _abilityEffectTime = 1.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "life_stealer_rage":
                        {
                            _currentAbilityEffect = Dota2AbilityEffects.LifeStealerRage;

                            _abilityEffectTime = ability.Level switch
                            {
                                1 => 3.0f,
                                2 => 4.0f,
                                3 => 5.0f,
                                4 => 6.0f,
                                _ => _abilityEffectTime
                            };

                            _abilityEffectKeyframe = 0.0f;
                            break;
                        }
                        case "magnataur_shockwave":
                            _currentAbilityEffect = Dota2AbilityEffects.MagnataurShockwave;
                            _abilityEffectTime = 1.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "omniknight_purification":
                            _currentAbilityEffect = Dota2AbilityEffects.OmniknightPurification;
                            _abilityEffectTime = 1.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "omniknight_repel":
                            _currentAbilityEffect = Dota2AbilityEffects.OmniknightRepel;

                            _abilityEffectTime = ability.Level switch
                            {
                                1 => 6.0f,
                                2 => 8.0f,
                                3 => 10.0f,
                                4 => 12.0f,
                                _ => _abilityEffectTime
                            };

                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "sandking_epicenter":
                            _currentAbilityEffect = Dota2AbilityEffects.SandkingEpicenter;
                            _abilityEffectTime = 5.0f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        case "slardar_slithereen_crush":
                            _currentAbilityEffect = Dota2AbilityEffects.SlardarSlithereenCrush;
                            _abilityEffectTime = 0.5f;
                            _abilityEffectKeyframe = 0.0f;
                            break;
                        default:
                        {
                            if (Global.isDebug)
                                System.Diagnostics.Debug.WriteLine("Unknown Ability: " + ability.Name);
                            break;
                        }
                    }
                }
            }

            _abilities = dota2State.Abilities;

            //Begin rendering

            if (_abilityEffectKeyframe >= _abilityEffectTime)
            {
                _currentAbilityEffect = Dota2AbilityEffects.None;
                _abilityEffectKeyframe = 0.0f;
                EffectLayer.Clear();
            }

            EffectLayer.Clear();
            switch (_currentAbilityEffect)
            {
                case Dota2AbilityEffects.RazorPlasmaField:
                    _razorPlasmaFieldTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.CrystalMaidenCrystalNova:
                    _crystalMaidenCrystalNovaTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.RikiSmokeScreen:
                    _rikiSmokeScreenTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.LinaDragonSlave:
                    _linaDragonSlaveTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.LinaLightStrikeArray:
                    _linaLightStrikeArrayTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.LinaLagunaBlade:
                    _linaLagunaBladeTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.AbaddonDeathCoil:
                    _abaddonDeathCoilTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.AbaddonBorrowedTime:
                {
                    var progress = (_abilityEffectKeyframe += GetDeltaTime()) / _abilityEffectTime;
                    var alphaPercent = progress >= 0.90f ? 1.0f + (1.0f - 1.11f * progress) : 1.0f;
                    var fluctuations = (float)Math.Sin(progress * Math.PI * 1.0f) + 0.75f;

                    var color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(0, 205, 255), fluctuations * alphaPercent);

                    EffectLayer.FillOver(color);
                    break;
                }
                case Dota2AbilityEffects.NevermoreShadowraze:
                    _nevermoreShadowrazeTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.NevermoreRequiem:
                    _nevermoreRequiemTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.ZuusArcLightning:
                    _zuusArcLightningTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.ZuusLightningBolt:
                    _zuusLightningBoltShadeTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _zuusLightningBoltTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.ZuusThundergodsWrath:
                {
                    var xOffset = (_abilityEffectKeyframe += GetDeltaTime()) / _abilityEffectTime;
                    var alphaPercent = (xOffset >= 0.85f ? 1.0f + (1.0f - (1.17f) * xOffset) : 1.0f);

                    EffectLayer.FillOver(Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(0, 205, 255), alphaPercent));
                    break;
                }
                case Dota2AbilityEffects.AntimageBlink:
                    _antimageBlinkTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.AntimageManaVoid:
                    _antimageManaVoidTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _antimageManaVoidCoreTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.AncientApparitionIceVortex:
                {
                    var xOffset = (_abilityEffectKeyframe += GetDeltaTime()) / _abilityEffectTime;
                    var alphaPercent = (xOffset >= 0.85f ? 1.0f + (1.0f - (1.17f) * xOffset) : 1.0f);

                    EffectLayer.FillOver(Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 200, 255), alphaPercent));
                    break;
                }
                case Dota2AbilityEffects.AncientApparitionIceBlast:
                    _ancientApparitionIceBlastTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.AlchemistAcidSpray:
                {
                    var alphaPercent = (float)Math.Pow(Math.Sin(((double)_abilityEffectKeyframe / (_abilityEffectTime / 16)) * Math.PI), 2.0);

                    EffectLayer.FillOver(Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(140, 160, 0), (alphaPercent < 0.2f ? 0.2f : alphaPercent)));

                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                }
                case Dota2AbilityEffects.AxeBerserkersCall:
                    _axeBerserkersCallTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.BeastmasterPrimalRoar:
                    _beastmasterPrimalRoarTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.BrewmasterThunderClap:
                    _brewmasterThunderClapTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.CentaurHoofStomp:
                    _centaurHoofStompTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.ChaosKnightChaosBolt:
                    _chaosKnightChaosBoltMix.Draw(EffectLayer.GetGraphics(), _abilityEffectKeyframe);
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.RattletrapRocketFlare:
                    _rattletrapRocketFlareTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.DoomBringerScorchedEarth:
                {
                    var progress = (_abilityEffectKeyframe += GetDeltaTime()) / _abilityEffectTime;
                    var alphaPercent = progress >= 0.90f ? 1.0f + (1.0f - 1.11f * progress) : 1.0f;
                    var fluctuations = (float)Math.Sin(progress * Math.PI * 1.0f) + 0.75f;

                    Color color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 60, 0), fluctuations * alphaPercent);

                    EffectLayer.FillOver(color);
                    break;
                }
                case Dota2AbilityEffects.DragonKnightBreatheFire:
                    _dragonKnightBreatheFireTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.EarthshakerFissure:
                {
                    var progress = (_abilityEffectKeyframe += GetDeltaTime()) / _abilityEffectTime;
                    var alphaPercent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);
                    var fluctuations = (float)Math.Sin(progress * Math.PI * 1.0f) + 0.75f;

                    var color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 60, 0), fluctuations * alphaPercent);

                    EffectLayer.FillOver(color);
                    break;
                }
                case Dota2AbilityEffects.EarthshakerEchoSlam:
                {
                    var progress = (_abilityEffectKeyframe += GetDeltaTime()) / _abilityEffectTime;
                    var alphaPercent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);
                    var fluctuations = (float)Math.Sin(progress * Math.PI * 1.0f) + 0.75f;

                    var color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 60, 0), fluctuations * alphaPercent);

                    EffectLayer.FillOver(color);
                    break;
                }
                case Dota2AbilityEffects.ElderTitanEarthSplitter:
                    _elderTitanEarthSplitterTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.KunkkaTorrent:
                    _kunkkaTorrentMix.Draw(EffectLayer.GetGraphics(), _abilityEffectKeyframe);
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.KunkkaGhostship:
                    _kunkkaGhostshipTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.LegionCommanderOverwhelmingOdds:
                    _legionCommanderOverwhelmingOddsTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.LifeStealerRage:
                    _lifeStealerRageTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.MagnataurShockwave:
                    _magnataurShockwaveTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.OmniknightPurification:
                    _omniknightPurificationTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.OmniknightRepel:
                    _omniknightRepelTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.SandkingEpicenter:
                    _sandkingEpicenterMix.Draw(EffectLayer.GetGraphics(), _abilityEffectKeyframe);
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
                case Dota2AbilityEffects.SlardarSlithereenCrush:
                    _slardarSlithereenCrushTrack.GetFrame(_abilityEffectKeyframe).Draw(EffectLayer.GetGraphics());
                    _abilityEffectKeyframe += GetDeltaTime();
                    break;
            }

            return EffectLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2HeroAbilityEffectsLayer)?.SetProfile(profile);
            base.SetApplication(profile);
        }

        private void UpdateAnimations()
        {
            _razorPlasmaFieldTrack = new AnimationTrack("Razor Plasma Field", 2.0f);
            _razorPlasmaFieldTrack.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(0, 200, 255), 3)
                );
            _razorPlasmaFieldTrack.SetFrame(1.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 200, 255), 3)
                );
            _razorPlasmaFieldTrack.SetFrame(2.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(0, 200, 255), 3)
                );


            _crystalMaidenCrystalNovaTrack = new AnimationTrack("CM Crystal Nova", 1.0f);
            _crystalMaidenCrystalNovaTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasHeight / 2.0f, Color.FromArgb(0, 200, 255))
                );
            _crystalMaidenCrystalNovaTrack.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.75f / 2.0f, Color.FromArgb(0, 200, 255))
                );
            _crystalMaidenCrystalNovaTrack.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 0, 200, 255))
                );

            _rikiSmokeScreenTrack = new AnimationTrack("Riki Smoke Screen", 6.5f);
            _rikiSmokeScreenTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasHeight / 2.0f, Color.FromArgb(163, 70, 255))
                );
            _rikiSmokeScreenTrack.SetFrame(5.525f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.75f / 2.0f, Color.FromArgb(163, 70, 255))
                );
            _rikiSmokeScreenTrack.SetFrame(6.5f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 163, 70, 255))
                );

            _linaDragonSlaveTrack = new AnimationTrack("Lina Dragon Slave", 1.25f);
            _linaDragonSlaveTrack.SetFrame(0.0f,
                new AnimationFilledCircle(0, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.10f, Color.FromArgb(255, 80, 0))
                );
            _linaDragonSlaveTrack.SetFrame(0.9375f,
                new AnimationFilledCircle(0, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.75f, Color.FromArgb(255, 80, 0))
                );
            _linaDragonSlaveTrack.SetFrame(1.25f,
                new AnimationFilledCircle(0, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.FromArgb(0, 255, 80, 0))
                );

            _linaLightStrikeArrayTrack = new AnimationTrack("Lina Light Strike", 2.0f);
            _linaLightStrikeArrayTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(0, 255, 80, 0))
                );
            _linaLightStrikeArrayTrack.SetFrame(0.49f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(0, 255, 80, 0))
                );
            _linaLightStrikeArrayTrack.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.1f / 2.0f, Color.FromArgb(255, 80, 0))
                );
            _linaLightStrikeArrayTrack.SetFrame(1.25f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.5f / 2.0f, Color.FromArgb(255, 80, 0))
                );
            _linaLightStrikeArrayTrack.SetFrame(2.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 255, 80, 0))
                );

            _nevermoreShadowrazeTrack = new AnimationTrack("Shadow Fiend Raze", 0.7f);
            _nevermoreShadowrazeTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.5f / 2.0f, Color.FromArgb(255, 0, 0))
                );
            _nevermoreShadowrazeTrack.SetFrame(0.595f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.075f / 2.0f, Color.FromArgb(255, 0, 0))
                );
            _nevermoreShadowrazeTrack.SetFrame(0.7f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(0, 255, 0, 0))
                );

            _nevermoreRequiemTrack = new AnimationTrack("Shadow Field Requiem", 2.0f);
            _nevermoreRequiemTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 0, 0))
                );
            _nevermoreRequiemTrack.SetFrame(1.7f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.85f / 2.0f, Color.FromArgb(255, 0, 0))
                );
            _nevermoreRequiemTrack.SetFrame(2.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 255, 0, 0))
                );

            _zuusLightningBoltTrack = new AnimationTrack("Zeus Lighting Bolt", 0.5f);
            _zuusLightningBoltTrack.SetFrame(0.0f,
                new AnimationLine(new PointF(Effects.CanvasWidthCenter, 0), new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeight), Color.FromArgb(0, 205, 255), 15)
                );
            _zuusLightningBoltTrack.SetFrame(0.425f,
                new AnimationLine(new PointF(Effects.CanvasWidthCenter, 0), new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeight), Color.FromArgb(0, 205, 255), 15)
                );
            _zuusLightningBoltTrack.SetFrame(0.5f,
                new AnimationLine(new PointF(Effects.CanvasWidthCenter, 0), new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeight), Color.FromArgb(0, 0, 205, 255), 15)
                );

            _zuusLightningBoltShadeTrack = new AnimationTrack("Zeus Lighting Bolt Shade", 0.5f);
            _zuusLightningBoltShadeTrack.SetFrame(0.0f,
                new AnimationLine(new PointF(Effects.CanvasWidthCenter, 0), new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeight), Color.FromArgb(180, 0, 205, 255), 20)
                );
            _zuusLightningBoltShadeTrack.SetFrame(0.425f,
                new AnimationLine(new PointF(Effects.CanvasWidthCenter, 0), new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeight), Color.FromArgb(180, 0, 205, 255), 20)
                );
            _zuusLightningBoltShadeTrack.SetFrame(0.5f,
                new AnimationLine(new PointF(Effects.CanvasWidthCenter, 0), new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeight), Color.FromArgb(0, 0, 205, 255), 20)
                );

            _antimageBlinkTrack = new AnimationTrack("Anti-mage Blink", 0.5f);
            _antimageBlinkTrack.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(128, 0, 255), 3)
                );
            _antimageBlinkTrack.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.FromArgb(128, 0, 255), 3)
                );

            _antimageManaVoidTrack = new AnimationTrack("Anti-mage Void", 0.5f);
            _antimageManaVoidTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.10f, Color.FromArgb(0, 0, 255))
                );
            _antimageManaVoidTrack.SetFrame(0.425f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.85f, Color.FromArgb(0, 0, 255))
                );
            _antimageManaVoidTrack.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.FromArgb(0, 0, 0, 255))
                );

            _antimageManaVoidCoreTrack = new AnimationTrack("Anti-mage Void Core", 0.5f);
            _antimageManaVoidCoreTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 255, 255))
                );
            _antimageManaVoidCoreTrack.SetFrame(0.425f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.25f, Color.FromArgb(255, 255, 255))
                );
            _antimageManaVoidCoreTrack.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.5f, Color.FromArgb(0, 255, 255, 255))
                );

            _ancientApparitionIceBlastTrack = new AnimationTrack("AA Ice Blast", 1.0f);
            _ancientApparitionIceBlastTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(200, 200, 255))
                );
            _ancientApparitionIceBlastTrack.SetFrame(0.85f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.85f, Color.FromArgb(200, 200, 255))
                );
            _ancientApparitionIceBlastTrack.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.FromArgb(0, 200, 200, 255))
                );

            _axeBerserkersCallTrack = new AnimationTrack("Axe Berserker", 0.7f);
            _axeBerserkersCallTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 50, 0))
                );
            _axeBerserkersCallTrack.SetFrame(0.595f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.85f, Color.FromArgb(255, 50, 0))
                );
            _axeBerserkersCallTrack.SetFrame(0.7f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.FromArgb(0, 255, 50, 0))
                );

            _beastmasterPrimalRoarTrack = new AnimationTrack("BM Primal Roar", 1.0f);
            _beastmasterPrimalRoarTrack.SetFrame(0.0f,
                new AnimationFilledCircle(0, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 200, 100))
                );
            _beastmasterPrimalRoarTrack.SetFrame(0.75f,
                new AnimationFilledCircle(0, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.75f, Color.FromArgb(255, 200, 100))
                );
            _beastmasterPrimalRoarTrack.SetFrame(1.0f,
                new AnimationFilledCircle(0, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.FromArgb(0, 255, 200, 100))
                );

            _brewmasterThunderClapTrack = new AnimationTrack("Brewmaster Thunder Clap", 1.5f);
            _brewmasterThunderClapTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(170, 90, 0))
                );
            _brewmasterThunderClapTrack.SetFrame(0.75f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.5f, Color.FromArgb(170, 90, 0))
                );
            _brewmasterThunderClapTrack.SetFrame(1.5f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.FromArgb(0, 170, 90, 0))
                );

            _centaurHoofStompTrack = new AnimationTrack("Centaur Stomp", 1.0f);
            _centaurHoofStompTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 50, 0))
                );
            _centaurHoofStompTrack.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.5f, Color.FromArgb(255, 50, 0))
                );
            _centaurHoofStompTrack.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.FromArgb(0, 255, 50, 0))
                );

            _chaosKnightChaosBoltMix = new AnimationMix();

            var chaosKnightChaosBoltProjectilePath = new AnimationTrack("Chaos Knight Bolt - Projectile Path", 0.5f);
            chaosKnightChaosBoltProjectilePath.SetFrame(0.0f,
                new AnimationLine(new PointF(0, Effects.CanvasHeightCenter), new PointF(0, Effects.CanvasHeightCenter), Color.FromArgb(255, 70, 0), 3)
                );
            chaosKnightChaosBoltProjectilePath.SetFrame(0.25f,
                new AnimationLine(new PointF(0, Effects.CanvasHeightCenter), new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter), Color.FromArgb(255, 70, 0), 3)
                );
            chaosKnightChaosBoltProjectilePath.SetFrame(0.5f,
                new AnimationLine(new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter), new PointF(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter), Color.FromArgb(0, 255, 70, 0), 3)
                );
            _chaosKnightChaosBoltMix.AddTrack(chaosKnightChaosBoltProjectilePath);

            var chaosKnightChaosBoltProjectile = new AnimationTrack("Chaos Knight Bolt - Projectile", 0.25f, 0.25f);
            chaosKnightChaosBoltProjectile.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(175, 0, 0))
                );
            chaosKnightChaosBoltProjectile.SetFrame(0.25f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.5f, Color.FromArgb(0, 175, 0, 0))
                );
            _chaosKnightChaosBoltMix.AddTrack(chaosKnightChaosBoltProjectile);

            _rattletrapRocketFlareTrack = new AnimationTrack("Clockwork Rocket Flare", 0.5f);
            _rattletrapRocketFlareTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 80, 0))
                );
            _rattletrapRocketFlareTrack.SetFrame(0.25f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.25f, Color.FromArgb(255, 80, 0))
                );
            _rattletrapRocketFlareTrack.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.5f, Color.FromArgb(0, 255, 80, 0))
                );

            _dragonKnightBreatheFireTrack = new AnimationTrack("Dragon Knight Breathe", 1.25f);
            _dragonKnightBreatheFireTrack.SetFrame(0.0f,
                new AnimationFilledCircle(0, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.10f, Color.FromArgb(255, 80, 0))
                );
            _dragonKnightBreatheFireTrack.SetFrame(0.9375f,
                new AnimationFilledCircle(0, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.75f, Color.FromArgb(255, 80, 0))
                );
            _dragonKnightBreatheFireTrack.SetFrame(1.25f,
                new AnimationFilledCircle(0, Effects.CanvasHeightCenter, Effects.CanvasBiggest, Color.FromArgb(0, 255, 80, 0))
                );

            _elderTitanEarthSplitterTrack = new AnimationTrack("Elder Titan Earth Splitter", 1.0f, 3.0f);
            _elderTitanEarthSplitterTrack.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(0, 255, 220))
                );
            _elderTitanEarthSplitterTrack.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(0, 0, 255, 220))
                );

            _kunkkaTorrentMix = new AnimationMix();
            var kunkkaTorrentBgTrack = new AnimationTrack("Kunka Torrent BG", 4.0f);
            kunkkaTorrentBgTrack.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(0, 0, 0))
                );
            kunkkaTorrentBgTrack.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(0, 60, 80))
                );
            kunkkaTorrentBgTrack.SetFrame(3.6f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(0, 60, 80))
                );
            kunkkaTorrentBgTrack.SetFrame(4.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(0, 0, 60, 80))
                );

            var kunkkaTorrentSpashTrack = new AnimationTrack("Kunka Torrent Splash", 2.4f, 1.6f);

            kunkkaTorrentSpashTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.1f / 2.0f, Color.FromArgb(0, 220, 245))
                );
            kunkkaTorrentSpashTrack.SetFrame(2.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest * 0.83f / 2.0f, Color.FromArgb(0, 220, 245))
                );
            kunkkaTorrentSpashTrack.SetFrame(2.4f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 0, 220, 245))
                );

            _kunkkaTorrentMix.AddTrack(kunkkaTorrentBgTrack);
            _kunkkaTorrentMix.AddTrack(kunkkaTorrentSpashTrack);


            _kunkkaGhostshipTrack = new AnimationTrack("Kunka Ghostship", 2.7f);

            _kunkkaGhostshipTrack.SetFrame(0.0f,
                    new AnimationFilledCircle(-(Effects.CanvasBiggest / 2.0f), Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 220, 245))
                    );
            _kunkkaGhostshipTrack.SetFrame(2.3f,
                new AnimationFilledCircle(Effects.CanvasWidth + (Effects.CanvasBiggest / 2.0f) * 0.85f, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 220, 245))
                );
            _kunkkaGhostshipTrack.SetFrame(2.7f,
                new AnimationFilledCircle(Effects.CanvasWidth + (Effects.CanvasBiggest / 2.0f), Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 0, 220, 245))
                );

            _legionCommanderOverwhelmingOddsTrack = new AnimationTrack("Legion Commander Overwhelming Odds", 1.0f);
            _legionCommanderOverwhelmingOddsTrack.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(0, 255, 145, 0))
                );
            _legionCommanderOverwhelmingOddsTrack.SetFrame(0.3f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 145, 0))
                );
            _legionCommanderOverwhelmingOddsTrack.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 145, 0))
                );
            _legionCommanderOverwhelmingOddsTrack.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(0, 255, 145, 0))
                );

            _lifeStealerRageTrack = new AnimationTrack("Life Stealer Rage", 6.0f);
            _lifeStealerRageTrack.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(175, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(130, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(175, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(1.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(130, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(2.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(175, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(2.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(130, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(3.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(175, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(3.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(130, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(4.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(175, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(4.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(130, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(5.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(175, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(5.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(130, 0, 0))
                );
            _lifeStealerRageTrack.SetFrame(6.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(175, 0, 0))
                );

            _magnataurShockwaveTrack = new AnimationTrack("Magnataur Shockwave", 1.0f);
            _magnataurShockwaveTrack.SetFrame(0.0f,
                    new AnimationFilledCircle(-(Effects.CanvasBiggest / 2.0f), Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 205, 255))
                    );
            _magnataurShockwaveTrack.SetFrame(0.9f,
                new AnimationFilledCircle(Effects.CanvasWidth + (Effects.CanvasBiggest / 2.0f) * 0.9f, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 205, 255))
                );
            _magnataurShockwaveTrack.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.CanvasWidth + (Effects.CanvasBiggest / 2.0f), Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 0, 205, 255))
                );

            _omniknightPurificationTrack = new AnimationTrack("Omniknight Purification", 1.0f);
            _omniknightPurificationTrack.SetFrame(0.0f,
                    new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0))
                    );
            _omniknightPurificationTrack.SetFrame(0.8f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0))
                );
            _omniknightPurificationTrack.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 255, 160, 0))
                );

            _omniknightRepelTrack = new AnimationTrack("Omniknight Repel", 12.0f);
            _omniknightRepelTrack.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(1.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(2.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(2.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(3.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(3.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(4.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(4.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(5.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(5.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(6.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(6.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(7.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(7.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(8.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(8.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(9.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(9.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(10.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(10.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(11.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );
            _omniknightRepelTrack.SetFrame(11.5f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(150, 150, 255))
                );
            _omniknightRepelTrack.SetFrame(12.0f,
                new AnimationFilledRectangle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasWidth, Effects.CanvasHeight, Color.FromArgb(255, 255, 255))
                );

            _sandkingEpicenterMix = new AnimationMix();
            var sandkingEpicenterWave1 = new AnimationTrack("Sandsking Epicenter Wave1", 0.5f, 0.0f);
            sandkingEpicenterWave1.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave1.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave2 = new AnimationTrack("Sandsking Epicenter Wave2", 0.5f, 0.16f);
            sandkingEpicenterWave2.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave2.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave3 = new AnimationTrack("Sandsking Epicenter Wave3", 0.5f, 0.32f);
            sandkingEpicenterWave3.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave3.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave4 = new AnimationTrack("Sandsking Epicenter Wave4", 0.5f, 0.48f);
            sandkingEpicenterWave4.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave4.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave5 = new AnimationTrack("Sandsking Epicenter Wave5", 0.5f, 0.64f);
            sandkingEpicenterWave5.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave5.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave6 = new AnimationTrack("Sandsking Epicenter Wave6", 0.5f, 0.8f);
            sandkingEpicenterWave6.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave6.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave7 = new AnimationTrack("Sandsking Epicenter Wave7", 0.5f, 0.96f);
            sandkingEpicenterWave7.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave7.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave8 = new AnimationTrack("Sandsking Epicenter Wave8", 0.5f, 1.12f);
            sandkingEpicenterWave8.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave8.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave9 = new AnimationTrack("Sandsking Epicenter Wave9", 0.5f, 1.28f);
            sandkingEpicenterWave9.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave9.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave10 = new AnimationTrack("Sandsking Epicenter Wave10", 0.5f, 1.44f);
            sandkingEpicenterWave10.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave10.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave11 = new AnimationTrack("Sandsking Epicenter Wave11", 0.5f, 1.6f);
            sandkingEpicenterWave11.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave11.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );
            var sandkingEpicenterWave12 = new AnimationTrack("Sandsking Epicenter Wave12", 0.5f, 1.76f);
            sandkingEpicenterWave12.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave12.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave13 = new AnimationTrack("Sandsking Epicenter Wave13", 0.5f, 1.92f);
            sandkingEpicenterWave13.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave13.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave14 = new AnimationTrack("Sandsking Epicenter Wave14", 0.5f, 2.08f);
            sandkingEpicenterWave14.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave14.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave15 = new AnimationTrack("Sandsking Epicenter Wave15", 0.5f, 2.24f);
            sandkingEpicenterWave15.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave15.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave16 = new AnimationTrack("Sandsking Epicenter Wave16", 0.5f, 2.4f);
            sandkingEpicenterWave16.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave16.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave17 = new AnimationTrack("Sandsking Epicenter Wave17", 0.5f, 2.56f);
            sandkingEpicenterWave17.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave17.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave18 = new AnimationTrack("Sandsking Epicenter Wave18", 0.5f, 2.72f);
            sandkingEpicenterWave18.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave18.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave19 = new AnimationTrack("Sandsking Epicenter Wave19", 0.5f, 2.88f);
            sandkingEpicenterWave19.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave19.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            var sandkingEpicenterWave20 = new AnimationTrack("Sandsking Epicenter Wave20", 0.5f, 3f);
            sandkingEpicenterWave20.SetFrame(0.0f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandkingEpicenterWave20.SetFrame(0.5f,
                new AnimationCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave1);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave2);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave3);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave4);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave5);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave6);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave7);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave8);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave9);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave10);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave11);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave12);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave13);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave14);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave15);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave16);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave17);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave18);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave19);
            _sandkingEpicenterMix.AddTrack(sandkingEpicenterWave20);


            _slardarSlithereenCrushTrack = new AnimationTrack("Slardar SMASH!", 0.5f);
            _slardarSlithereenCrushTrack.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, 0, Color.FromArgb(0, 150, 255))
                );
            _slardarSlithereenCrushTrack.SetFrame(0.45f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, (Effects.CanvasBiggest / 2.0f) * 0.9f, Color.FromArgb(0, 150, 255))
                );
            _slardarSlithereenCrushTrack.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.CanvasWidthCenter, Effects.CanvasHeightCenter, Effects.CanvasBiggest / 2.0f, Color.FromArgb(0, 0, 150, 255))
                );

        }

        public override void Dispose()
        {
            base.Dispose();
            
            WeakEventManager<Effects, CanvasChangedArgs>.RemoveHandler(null, nameof(Effects.CanvasChanged), Effects_CanvasChanged);
        }
    }
}
