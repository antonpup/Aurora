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
    public class Dota2HeroAbilityEffectsLayerHandlerProperties : LayerHandlerProperties2Color<Dota2HeroAbilityEffectsLayerHandlerProperties>
    {
        public Dota2HeroAbilityEffectsLayerHandlerProperties() : base() { }

        public Dota2HeroAbilityEffectsLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
        }
    }

    public class Dota2HeroAbilityEffectsLayerHandler : LayerHandler<Dota2HeroAbilityEffectsLayerHandlerProperties>
    {
        enum Dota2AbilityEffects
        {
            None,
            abaddon_death_coil,
            abaddon_borrowed_time,
            alchemist_acid_spray,
            ancient_apparition_ice_vortex,
            ancient_apparition_ice_blast,
            antimage_blink,
            antimage_mana_void,
            axe_berserkers_call,
            beastmaster_primal_roar,
            brewmaster_thunder_clap,
            centaur_hoof_stomp,
            chaos_knight_chaos_bolt,
            crystal_maiden_crystal_nova,
            doom_bringer_scorched_earth,
            dragon_knight_breathe_fire,
            earthshaker_fissure,
            earthshaker_echo_slam,
            elder_titan_earth_splitter,

            kunkka_torrent,
            kunkka_ghostship,
            legion_commander_overwhelming_odds,
            life_stealer_rage,
            magnataur_shockwave,
            omniknight_purification,
            omniknight_repel,
            sandking_epicenter,
            slardar_slithereen_crush,

            lina_dragon_slave,
            lina_light_strike_array,
            lina_laguna_blade,
            nevermore_shadowraze,
            nevermore_requiem,
            rattletrap_rocket_flare,
            razor_plasma_field,
            riki_smoke_screen,
            zuus_arc_lightning,
            zuus_lightning_bolt,
            zuus_thundergods_wrath

        }

        private AnimationTrack razor_plasma_field_track;
        private AnimationTrack crystal_maiden_crystal_nova_track;
        private AnimationTrack riki_smoke_screen_track;
        private AnimationTrack lina_dragon_slave_track;
        private AnimationTrack lina_light_strike_array_track;
        private AnimationTrack lina_laguna_blade_track;
        private AnimationTrack abaddon_death_coil_track;
        private AnimationTrack nevermore_shadowraze_track;
        private AnimationTrack nevermore_requiem_track;
        private AnimationTrack zuus_arc_lightning_track;
        private AnimationTrack zuus_lightning_bolt_track;
        private AnimationTrack zuus_lightning_bolt_shade_track;
        private AnimationTrack antimage_blink_track;
        private AnimationTrack antimage_mana_void_track;
        private AnimationTrack antimage_mana_void_core_track;
        private AnimationTrack ancient_apparition_ice_blast_track;
        private AnimationTrack axe_berserkers_call_track;
        private AnimationTrack beastmaster_primal_roar_track;
        private AnimationTrack brewmaster_thunder_clap_track;
        private AnimationTrack centaur_hoof_stomp_track;
        private AnimationMix chaos_knight_chaos_bolt_mix;
        private AnimationTrack rattletrap_rocket_flare_track;
        private AnimationTrack dragon_knight_breathe_fire_track;
        private AnimationTrack elder_titan_earth_splitter_track;
        private AnimationMix kunkka_torrent_mix;
        private AnimationTrack kunkka_ghostship_track;
        private AnimationTrack legion_commander_overwhelming_odds_track;
        private AnimationTrack life_stealer_rage_track;
        private AnimationTrack magnataur_shockwave_track;
        private AnimationTrack omniknight_purification_track;
        private AnimationTrack omniknight_repel_track;
        private AnimationMix sandking_epicenter_mix;
        private AnimationTrack slardar_slithereen_crush_track;

        private long previoustime = 0;
        private long currenttime = 0;

        private float getDeltaTime()
        {
            return (currenttime - previoustime) / 1000.0f;
        }

        private static float abiltiyeffect_keyframe = 0.0f;
        private static Dota2AbilityEffects currentabilityeffect = Dota2AbilityEffects.None;
        private static float abilityeffect_time = 0.0f;

        private Random randomizer = new Random();

        private static Abilities_Dota2 abilities;

        protected override UserControl CreateControl()
        {
            return new Control_Dota2HeroAbilityEffectsLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            previoustime = currenttime;
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            if (currenttime - previoustime > 300000 || (currenttime == 0 && previoustime == 0))
                UpdateAnimations();

            EffectLayer ability_effects_layer = new EffectLayer("Dota 2 - Ability Effects");

            if (state is GameState_Dota2)
            {
                GameState_Dota2 dota2state = state as GameState_Dota2;

                //Preparations
                if (abilities != null && dota2state.Abilities.Count == abilities.Count)
                {
                    for (int ability_id = 0; ability_id < dota2state.Abilities.Count; ability_id++)
                    {
                        Ability ability = dota2state.Abilities[ability_id];

                        if (!ability.CanCast && abilities[ability_id].CanCast)
                        {
                            //Casted ability
                            if (ability.Name.Equals("razor_plasma_field"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.razor_plasma_field;
                                abilityeffect_time = 2.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("crystal_maiden_crystal_nova"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.crystal_maiden_crystal_nova;
                                abilityeffect_time = 1.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("riki_smoke_screen"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.riki_smoke_screen;
                                abilityeffect_time = 6.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("lina_dragon_slave"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.lina_dragon_slave;
                                abilityeffect_time = 1.25f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("lina_light_strike_array"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.lina_light_strike_array;
                                abilityeffect_time = 2.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("lina_laguna_blade"))
                            {
                                lina_laguna_blade_track = new AnimationTrack("Lina Laguna Blade", 0.5f);

                                PointF laguna_point1 = new PointF(0, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                PointF laguna_point2 = new PointF(Effects.canvas_width_center + 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                PointF laguna_point3 = new PointF(Effects.canvas_width_center + 6.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                PointF laguna_point4 = new PointF(Effects.canvas_width_center + 9.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));

                                lina_laguna_blade_track.SetFrame(0.0f,
                                    new AnimationLines(
                                        new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), laguna_point1, Color.FromArgb(255, 255, 255), 2),
                                            new AnimationLine(laguna_point1, laguna_point2, Color.FromArgb(255, 255, 255), Color.FromArgb(170, 170, 255), 3),
                                            new AnimationLine(laguna_point2, laguna_point3, Color.FromArgb(170, 170, 255), Color.FromArgb(85, 85, 255), 3),
                                            new AnimationLine(laguna_point3, laguna_point4, Color.FromArgb(85, 85, 255), Color.FromArgb(0, 0, 255), 5),
                                        }
                                        )
                                    );

                                lina_laguna_blade_track.SetFrame(0.45f,
                                    new AnimationLines(
                                        new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), laguna_point1, Color.FromArgb(255, 255, 255), 2),
                                            new AnimationLine(laguna_point1, laguna_point2, Color.FromArgb(255, 255, 255), Color.FromArgb(170, 170, 255), 3),
                                            new AnimationLine(laguna_point2, laguna_point3, Color.FromArgb(170, 170, 255), Color.FromArgb(85, 85, 255), 3),
                                            new AnimationLine(laguna_point3, laguna_point4, Color.FromArgb(85, 85, 255), Color.FromArgb(0, 0, 255), 5),
                                        }
                                        )
                                    );

                                lina_laguna_blade_track.SetFrame(0.5f,
                                    new AnimationLines(
                                        new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), laguna_point1, Color.FromArgb(0, 255, 255, 255), 2),
                                            new AnimationLine(laguna_point1, laguna_point2, Color.FromArgb(0, 255, 255, 255), Color.FromArgb(0, 170, 170, 255), 3),
                                            new AnimationLine(laguna_point2, laguna_point3, Color.FromArgb(0, 170, 170, 255), Color.FromArgb(0, 85, 85, 255), 3),
                                            new AnimationLine(laguna_point3, laguna_point4, Color.FromArgb(0, 85, 85, 255), Color.FromArgb(0, 0, 0, 255), 5),
                                        }
                                        )
                                    );

                                currentabilityeffect = Dota2AbilityEffects.lina_laguna_blade;
                                abilityeffect_time = 0.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("abaddon_death_coil"))
                            {
                                abaddon_death_coil_track = new AnimationTrack("Abaddon Dealth Coil", 0.5f);

                                PointF death_coil_point1 = new PointF(Effects.canvas_width_center - 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                PointF death_coil_point2 = new PointF(Effects.canvas_width_center, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                PointF death_coil_point3 = new PointF(Effects.canvas_width_center + 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                PointF death_coil_point4 = new PointF(Effects.canvas_width_center + 9.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));

                                abaddon_death_coil_track.SetFrame(0.0f,
                                    new AnimationLines(
                                        new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), death_coil_point1, Color.FromArgb(0, 160, 210), 2),
                                            new AnimationLine(death_coil_point1, death_coil_point2, Color.FromArgb(0, 160, 210), 3),
                                            new AnimationLine(death_coil_point2, death_coil_point3, Color.FromArgb(0, 160, 210), 3),
                                            new AnimationLine(death_coil_point3, death_coil_point4, Color.FromArgb(0, 160, 210), 5),
                                        }
                                        )
                                    );

                                abaddon_death_coil_track.SetFrame(0.45f,
                                    new AnimationLines(
                                        new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), death_coil_point1, Color.FromArgb(0, 160, 210), 2),
                                            new AnimationLine(death_coil_point1, death_coil_point2, Color.FromArgb(0, 160, 210), 3),
                                            new AnimationLine(death_coil_point2, death_coil_point3, Color.FromArgb(0, 160, 210), 3),
                                            new AnimationLine(death_coil_point3, death_coil_point4, Color.FromArgb(0, 160, 210), 5),
                                        }
                                        )
                                    );

                                abaddon_death_coil_track.SetFrame(0.5f,
                                    new AnimationLines(
                                        new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), death_coil_point1, Color.FromArgb(0, 0, 160, 210), 2),
                                            new AnimationLine(death_coil_point1, death_coil_point2, Color.FromArgb(0, 0, 160, 210), 3),
                                            new AnimationLine(death_coil_point2, death_coil_point3, Color.FromArgb(0, 0, 160, 210), 3),
                                            new AnimationLine(death_coil_point3, death_coil_point4, Color.FromArgb(0, 0, 160, 210), 5),
                                        }
                                        )
                                    );

                                currentabilityeffect = Dota2AbilityEffects.abaddon_death_coil;
                                abilityeffect_time = 0.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("abaddon_borrowed_time"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.abaddon_borrowed_time;

                                if (ability.Level == 1)
                                    abilityeffect_time = 4.0f;
                                else if (ability.Level == 2)
                                    abilityeffect_time = 5.0f;
                                else if (ability.Level == 3)
                                    abilityeffect_time = 6.0f;
                                else
                                    abilityeffect_time = 6.0f;

                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("nevermore_shadowraze1") || ability.Name.Equals("nevermore_shadowraze2") || ability.Name.Equals("nevermore_shadowraze3"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.nevermore_shadowraze;
                                abilityeffect_time = 0.7f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("nevermore_requiem"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.nevermore_requiem;
                                abilityeffect_time = 2.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("zuus_arc_lightning"))
                            {
                                zuus_arc_lightning_track = new AnimationTrack("Zeus Arc Lightning", 0.5f);

                                PointF zuus_lightning_point1 = new PointF(Effects.canvas_width_center - 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                PointF zuus_lightning_point2 = new PointF(Effects.canvas_width_center, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                PointF zuus_lightning_point3 = new PointF(Effects.canvas_width_center + 3.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                                PointF zuus_lightning_point4 = new PointF(Effects.canvas_width_center + 9.0f, Effects.canvas_height_center + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));

                                zuus_arc_lightning_track.SetFrame(0.0f,
                                    new AnimationLines(
                                        new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), zuus_lightning_point1, Color.FromArgb(0, 205, 255), 2),
                                            new AnimationLine(zuus_lightning_point1, zuus_lightning_point2, Color.FromArgb(0, 205, 255), 3),
                                            new AnimationLine(zuus_lightning_point2, zuus_lightning_point3, Color.FromArgb(0, 205, 255) , 3),
                                            new AnimationLine(zuus_lightning_point3, zuus_lightning_point4, Color.FromArgb(0, 205, 255), 5),
                                        }
                                        )
                                    );

                                zuus_arc_lightning_track.SetFrame(0.45f,
                                    new AnimationLines(
                                        new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), zuus_lightning_point1, Color.FromArgb(0, 205, 255), 2),
                                            new AnimationLine(zuus_lightning_point1, zuus_lightning_point2, Color.FromArgb(0, 205, 255), 3),
                                            new AnimationLine(zuus_lightning_point2, zuus_lightning_point3, Color.FromArgb(0, 205, 255) , 3),
                                            new AnimationLine(zuus_lightning_point3, zuus_lightning_point4, Color.FromArgb(0, 205, 255), 5),
                                        }
                                        )
                                    );

                                zuus_arc_lightning_track.SetFrame(0.5f,
                                    new AnimationLines(
                                        new AnimationLine[] {
                                            new AnimationLine(new PointF(0, Effects.canvas_height_center), zuus_lightning_point1, Color.FromArgb(0, 0, 205, 255), 2),
                                            new AnimationLine(zuus_lightning_point1, zuus_lightning_point2, Color.FromArgb(0, 0, 205, 255), 3),
                                            new AnimationLine(zuus_lightning_point2, zuus_lightning_point3, Color.FromArgb(0, 0, 205, 255), 3),
                                            new AnimationLine(zuus_lightning_point3, zuus_lightning_point4, Color.FromArgb(0, 0, 205, 255), 5),
                                        }
                                        )
                                    );

                                currentabilityeffect = Dota2AbilityEffects.zuus_arc_lightning;
                                abilityeffect_time = 0.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("zuus_lightning_bolt"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.zuus_lightning_bolt;
                                abilityeffect_time = 0.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("zuus_thundergods_wrath"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.zuus_thundergods_wrath;
                                abilityeffect_time = 0.25f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("antimage_blink"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.antimage_blink;
                                abilityeffect_time = 0.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("antimage_mana_void"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.antimage_mana_void;
                                abilityeffect_time = 0.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("ancient_apparition_ice_vortex"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.ancient_apparition_ice_vortex;
                                abilityeffect_time = 0.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("ancient_apparition_ice_blast"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.ancient_apparition_ice_blast;
                                abilityeffect_time = 1.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("alchemist_acid_spray"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.alchemist_acid_spray;
                                abilityeffect_time = 16.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("axe_berserkers_call"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.axe_berserkers_call;
                                abilityeffect_time = 0.7f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("beastmaster_primal_roar"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.beastmaster_primal_roar;
                                abilityeffect_time = 1.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("brewmaster_thunder_clap"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.brewmaster_thunder_clap;
                                abilityeffect_time = 1.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("centaur_hoof_stomp"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.centaur_hoof_stomp;
                                abilityeffect_time = 1.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("chaos_knight_chaos_bolt"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.chaos_knight_chaos_bolt;
                                abilityeffect_time = 1.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("rattletrap_rocket_flare"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.rattletrap_rocket_flare;
                                abilityeffect_time = 0.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("doom_bringer_scorched_earth"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.doom_bringer_scorched_earth;

                                if (ability.Level == 2)
                                    abilityeffect_time = 12.0f;
                                else if (ability.Level == 3)
                                    abilityeffect_time = 14.0f;
                                else if (ability.Level == 4)
                                    abilityeffect_time = 16.0f;
                                else
                                    abilityeffect_time = 10.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("dragon_knight_breathe_fire"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.dragon_knight_breathe_fire;
                                abilityeffect_time = 1.25f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("earthshaker_fissure"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.earthshaker_fissure;
                                abilityeffect_time = 0.25f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("earthshaker_echo_slam"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.earthshaker_echo_slam;
                                abilityeffect_time = 0.25f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("elder_titan_earth_splitter"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.elder_titan_earth_splitter;
                                abilityeffect_time = 4.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("kunkka_torrent"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.kunkka_torrent;
                                abilityeffect_time = 4.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("kunkka_ghostship"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.kunkka_ghostship;
                                abilityeffect_time = 2.7f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("legion_commander_overwhelming_odds"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.legion_commander_overwhelming_odds;
                                abilityeffect_time = 1.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("life_stealer_rage"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.life_stealer_rage;

                                if (ability.Level == 1)
                                    abilityeffect_time = 3.0f;
                                else if (ability.Level == 2)
                                    abilityeffect_time = 4.0f;
                                else if (ability.Level == 3)
                                    abilityeffect_time = 5.0f;
                                if (ability.Level == 4)
                                    abilityeffect_time = 6.0f;

                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("magnataur_shockwave"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.magnataur_shockwave;
                                abilityeffect_time = 1.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("omniknight_purification"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.omniknight_purification;
                                abilityeffect_time = 1.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("omniknight_repel"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.omniknight_repel;

                                if (ability.Level == 1)
                                    abilityeffect_time = 6.0f;
                                else if (ability.Level == 2)
                                    abilityeffect_time = 8.0f;
                                else if (ability.Level == 3)
                                    abilityeffect_time = 10.0f;
                                if (ability.Level == 4)
                                    abilityeffect_time = 12.0f;

                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("sandking_epicenter"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.sandking_epicenter;
                                abilityeffect_time = 5.0f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else if (ability.Name.Equals("slardar_slithereen_crush"))
                            {
                                currentabilityeffect = Dota2AbilityEffects.slardar_slithereen_crush;
                                abilityeffect_time = 0.5f;
                                abiltiyeffect_keyframe = 0.0f;
                            }
                            else
                            {
                                if (Global.isDebug)
                                    System.Diagnostics.Debug.WriteLine("Unknown Ability: " + ability.Name);
                            }

                        }
                    }
                }

                abilities = dota2state.Abilities;


                //Begin rendering

                if (abiltiyeffect_keyframe >= abilityeffect_time)
                {
                    currentabilityeffect = Dota2AbilityEffects.None;
                    abiltiyeffect_keyframe = 0.0f;
                }

                float mid_x = Effects.canvas_width / 2.0f;
                float mid_y = Effects.canvas_height / 2.0f;


                if (currentabilityeffect == Dota2AbilityEffects.razor_plasma_field)
                {
                    razor_plasma_field_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.crystal_maiden_crystal_nova)
                {
                    crystal_maiden_crystal_nova_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.riki_smoke_screen)
                {
                    riki_smoke_screen_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.lina_dragon_slave)
                {
                    lina_dragon_slave_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.lina_light_strike_array)
                {
                    lina_light_strike_array_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.lina_laguna_blade)
                {
                    lina_laguna_blade_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();

                    //Kept to remember the times without AnimationTracks. The pain was real.
                    /*
                    if (abiltiyeffect_keyframe == 0.0f)
                    {
                        laguna_point1 = new EffectPoint(mid_x, mid_y + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                        laguna_point2 = new EffectPoint(mid_x + 3.0f, mid_y + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                        laguna_point3 = new EffectPoint(mid_x + 6.0f, mid_y + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                        laguna_point4 = new EffectPoint(mid_x + 9.0f, mid_y + ((randomizer.Next() % 2 == 0 ? 1.0f : -1.0f) * 6.0f * (float)randomizer.NextDouble()));
                    }

                    float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                    float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                    ColorSpectrum lina_blade_spectrum_step1 = new ColorSpectrum(
                        Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(255, 255, 255), alpha_percent)
                        );
                    ColorSpectrum lina_blade_spectrum_step2 = new ColorSpectrum(
                        Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(255, 255, 255), alpha_percent),
                        Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(170, 170, 255), alpha_percent)
                        );
                    ColorSpectrum lina_blade_spectrum_step3 = new ColorSpectrum(
                        Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(170, 170, 255), alpha_percent),
                        Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(85, 85, 255), alpha_percent)
                        );
                    ColorSpectrum lina_blade_spectrum_step4 = new ColorSpectrum(
                        Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(85, 85, 255), alpha_percent),
                        Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(0, 0, 255), alpha_percent)
                        );


                    EffectFunction linefunc1 = new EffectLine(new EffectPoint(0, mid_y), laguna_point1, true);
                    EffectColorFunction line1 = new EffectColorFunction(linefunc1, lina_blade_spectrum_step1, 2);
                    ability_effects_layer.AddPostFunction(line1);

                    EffectFunction linefunc2 = new EffectLine(laguna_point1, laguna_point2, true);
                    EffectColorFunction line2 = new EffectColorFunction(linefunc2, lina_blade_spectrum_step2, 3);
                    ability_effects_layer.AddPostFunction(line2);

                    EffectFunction linefunc3 = new EffectLine(laguna_point2, laguna_point3, true);
                    EffectColorFunction line3 = new EffectColorFunction(linefunc3, lina_blade_spectrum_step3, 3);
                    ability_effects_layer.AddPostFunction(line3);

                    EffectFunction linefunc4 = new EffectLine(laguna_point3, laguna_point4, true);
                    EffectColorFunction line4 = new EffectColorFunction(linefunc4, lina_blade_spectrum_step4, 5);
                    ability_effects_layer.AddPostFunction(line4);
                    */
                }
                else if (currentabilityeffect == Dota2AbilityEffects.abaddon_death_coil)
                {
                    abaddon_death_coil_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.abaddon_borrowed_time)
                {
                    float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                    float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                    float fluctuatiuons = (float)Math.Sin(((double)progress) * Math.PI * 1.0f) + 0.75f;

                    Color color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(0, 205, 255), fluctuatiuons * alpha_percent);

                    ability_effects_layer.Fill(color);
                }
                else if (currentabilityeffect == Dota2AbilityEffects.nevermore_shadowraze)
                {
                    nevermore_shadowraze_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.nevermore_requiem)
                {
                    nevermore_requiem_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.zuus_arc_lightning)
                {
                    zuus_arc_lightning_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.zuus_lightning_bolt)
                {
                    zuus_lightning_bolt_shade_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    zuus_lightning_bolt_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.zuus_thundergods_wrath)
                {
                    float x_offset = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                    float alpha_percent = (x_offset >= 0.85f ? 1.0f + (1.0f - (1.17f) * x_offset) : 1.0f);

                    ability_effects_layer.Fill(Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(0, 205, 255), alpha_percent));
                }
                else if (currentabilityeffect == Dota2AbilityEffects.antimage_blink)
                {
                    antimage_blink_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.antimage_mana_void)
                {
                    antimage_mana_void_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    antimage_mana_void_core_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.ancient_apparition_ice_vortex)
                {
                    float x_offset = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                    float alpha_percent = (x_offset >= 0.85f ? 1.0f + (1.0f - (1.17f) * x_offset) : 1.0f);

                    ability_effects_layer.Fill(Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 200, 255), alpha_percent));
                }
                else if (currentabilityeffect == Dota2AbilityEffects.ancient_apparition_ice_blast)
                {
                    ancient_apparition_ice_blast_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.alchemist_acid_spray)
                {
                    float alpha_percent = (float)Math.Pow(Math.Sin(((double)abiltiyeffect_keyframe / (abilityeffect_time / 16)) * Math.PI), 2.0);

                    ability_effects_layer.Fill(Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(140, 160, 0), (alpha_percent < 0.2f ? 0.2f : alpha_percent)));

                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.axe_berserkers_call)
                {
                    axe_berserkers_call_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.beastmaster_primal_roar)
                {
                    beastmaster_primal_roar_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.brewmaster_thunder_clap)
                {
                    brewmaster_thunder_clap_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.centaur_hoof_stomp)
                {
                    centaur_hoof_stomp_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.chaos_knight_chaos_bolt)
                {
                    chaos_knight_chaos_bolt_mix.Draw(ability_effects_layer.GetGraphics(), abiltiyeffect_keyframe);
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.rattletrap_rocket_flare)
                {
                    rattletrap_rocket_flare_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.doom_bringer_scorched_earth)
                {
                    float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                    float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                    float fluctuatiuons = (float)Math.Sin(((double)progress) * Math.PI * 1.0f) + 0.75f;

                    Color color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 60, 0), fluctuatiuons * alpha_percent);

                    ability_effects_layer.Fill(color);
                }
                else if (currentabilityeffect == Dota2AbilityEffects.dragon_knight_breathe_fire)
                {
                    dragon_knight_breathe_fire_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.earthshaker_fissure)
                {
                    float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                    float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                    float fluctuatiuons = (float)Math.Sin(((double)progress) * Math.PI * 1.0f) + 0.75f;

                    Color color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 60, 0), fluctuatiuons * alpha_percent);

                    ability_effects_layer.Fill(color);
                }
                else if (currentabilityeffect == Dota2AbilityEffects.earthshaker_echo_slam)
                {
                    float progress = (abiltiyeffect_keyframe += getDeltaTime()) / abilityeffect_time;

                    float alpha_percent = (progress >= 0.90f ? 1.0f + (1.0f - (1.11f) * progress) : 1.0f);

                    float fluctuatiuons = (float)Math.Sin(((double)progress) * Math.PI * 1.0f) + 0.75f;

                    Color color = Utils.ColorUtils.MultiplyColorByScalar(Color.FromArgb(200, 60, 0), fluctuatiuons * alpha_percent);

                    ability_effects_layer.Fill(color);
                }
                else if (currentabilityeffect == Dota2AbilityEffects.elder_titan_earth_splitter)
                {
                    elder_titan_earth_splitter_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.kunkka_torrent)
                {
                    kunkka_torrent_mix.Draw(ability_effects_layer.GetGraphics(), abiltiyeffect_keyframe);
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.kunkka_ghostship)
                {
                    kunkka_ghostship_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.legion_commander_overwhelming_odds)
                {
                    legion_commander_overwhelming_odds_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.life_stealer_rage)
                {
                    life_stealer_rage_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.magnataur_shockwave)
                {
                    magnataur_shockwave_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.omniknight_purification)
                {
                    omniknight_purification_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.omniknight_repel)
                {
                    omniknight_repel_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.sandking_epicenter)
                {
                    sandking_epicenter_mix.Draw(ability_effects_layer.GetGraphics(), abiltiyeffect_keyframe);
                    abiltiyeffect_keyframe += getDeltaTime();
                }
                else if (currentabilityeffect == Dota2AbilityEffects.slardar_slithereen_crush)
                {
                    slardar_slithereen_crush_track.GetFrame(abiltiyeffect_keyframe).Draw(ability_effects_layer.GetGraphics());
                    abiltiyeffect_keyframe += getDeltaTime();
                }
            }

            return ability_effects_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_Dota2HeroAbilityEffectsLayer).SetProfile(profile);
            base.SetApplication(profile);
        }

        public void UpdateAnimations()
        {
            razor_plasma_field_track = new AnimationTrack("Razor Plasma Field", 2.0f);
            razor_plasma_field_track.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 200, 255), 3)
                );
            razor_plasma_field_track.SetFrame(1.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 200, 255), 3)
                );
            razor_plasma_field_track.SetFrame(2.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 200, 255), 3)
                );


            crystal_maiden_crystal_nova_track = new AnimationTrack("CM Crystal Nova", 1.0f);
            crystal_maiden_crystal_nova_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_height / 2.0f, Color.FromArgb(0, 200, 255))
                );
            crystal_maiden_crystal_nova_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f / 2.0f, Color.FromArgb(0, 200, 255))
                );
            crystal_maiden_crystal_nova_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 200, 255))
                );

            riki_smoke_screen_track = new AnimationTrack("Riki Smoke Screen", 6.5f);
            riki_smoke_screen_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_height / 2.0f, Color.FromArgb(163, 70, 255))
                );
            riki_smoke_screen_track.SetFrame(5.525f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f / 2.0f, Color.FromArgb(163, 70, 255))
                );
            riki_smoke_screen_track.SetFrame(6.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 163, 70, 255))
                );

            lina_dragon_slave_track = new AnimationTrack("Lina Dragon Slave", 1.25f);
            lina_dragon_slave_track.SetFrame(0.0f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.10f, Color.FromArgb(255, 80, 0))
                );
            lina_dragon_slave_track.SetFrame(0.9375f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f, Color.FromArgb(255, 80, 0))
                );
            lina_dragon_slave_track.SetFrame(1.25f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 80, 0))
                );

            lina_light_strike_array_track = new AnimationTrack("Lina Light Strike", 2.0f);
            lina_light_strike_array_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 255, 80, 0))
                );
            lina_light_strike_array_track.SetFrame(0.49f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 255, 80, 0))
                );
            lina_light_strike_array_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.1f / 2.0f, Color.FromArgb(255, 80, 0))
                );
            lina_light_strike_array_track.SetFrame(1.25f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f / 2.0f, Color.FromArgb(255, 80, 0))
                );
            lina_light_strike_array_track.SetFrame(2.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 255, 80, 0))
                );

            nevermore_shadowraze_track = new AnimationTrack("Shadow Fiend Raze", 0.7f);
            nevermore_shadowraze_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f / 2.0f, Color.FromArgb(255, 0, 0))
                );
            nevermore_shadowraze_track.SetFrame(0.595f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.075f / 2.0f, Color.FromArgb(255, 0, 0))
                );
            nevermore_shadowraze_track.SetFrame(0.7f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 255, 0, 0))
                );

            nevermore_requiem_track = new AnimationTrack("Shadow Field Requiem", 2.0f);
            nevermore_requiem_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 0, 0))
                );
            nevermore_requiem_track.SetFrame(1.7f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.85f / 2.0f, Color.FromArgb(255, 0, 0))
                );
            nevermore_requiem_track.SetFrame(2.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 255, 0, 0))
                );

            zuus_lightning_bolt_track = new AnimationTrack("Zeus Lighting Bolt", 0.5f);
            zuus_lightning_bolt_track.SetFrame(0.0f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(0, 205, 255), 15)
                );
            zuus_lightning_bolt_track.SetFrame(0.425f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(0, 205, 255), 15)
                );
            zuus_lightning_bolt_track.SetFrame(0.5f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(0, 0, 205, 255), 15)
                );

            zuus_lightning_bolt_shade_track = new AnimationTrack("Zeus Lighting Bolt Shade", 0.5f);
            zuus_lightning_bolt_shade_track.SetFrame(0.0f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(180, 0, 205, 255), 20)
                );
            zuus_lightning_bolt_shade_track.SetFrame(0.425f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(180, 0, 205, 255), 20)
                );
            zuus_lightning_bolt_shade_track.SetFrame(0.5f,
                new AnimationLine(new PointF(Effects.canvas_width_center, 0), new PointF(Effects.canvas_width_center, Effects.canvas_height), Color.FromArgb(0, 0, 205, 255), 20)
                );

            antimage_blink_track = new AnimationTrack("Anti-mage Blink", 0.5f);
            antimage_blink_track.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(128, 0, 255), 3)
                );
            antimage_blink_track.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(128, 0, 255), 3)
                );

            antimage_mana_void_track = new AnimationTrack("Anti-mage Void", 0.5f);
            antimage_mana_void_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.10f, Color.FromArgb(0, 0, 255))
                );
            antimage_mana_void_track.SetFrame(0.425f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.85f, Color.FromArgb(0, 0, 255))
                );
            antimage_mana_void_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 0, 0, 255))
                );

            antimage_mana_void_core_track = new AnimationTrack("Anti-mage Void Core", 0.5f);
            antimage_mana_void_core_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 255, 255))
                );
            antimage_mana_void_core_track.SetFrame(0.425f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.25f, Color.FromArgb(255, 255, 255))
                );
            antimage_mana_void_core_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(0, 255, 255, 255))
                );

            ancient_apparition_ice_blast_track = new AnimationTrack("AA Ice Blast", 1.0f);
            ancient_apparition_ice_blast_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(200, 200, 255))
                );
            ancient_apparition_ice_blast_track.SetFrame(0.85f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.85f, Color.FromArgb(200, 200, 255))
                );
            ancient_apparition_ice_blast_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 200, 200, 255))
                );

            axe_berserkers_call_track = new AnimationTrack("Axe Berserker", 0.7f);
            axe_berserkers_call_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 50, 0))
                );
            axe_berserkers_call_track.SetFrame(0.595f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.85f, Color.FromArgb(255, 50, 0))
                );
            axe_berserkers_call_track.SetFrame(0.7f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 50, 0))
                );

            beastmaster_primal_roar_track = new AnimationTrack("BM Primal Roar", 1.0f);
            beastmaster_primal_roar_track.SetFrame(0.0f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, 0, Color.FromArgb(255, 200, 100))
                );
            beastmaster_primal_roar_track.SetFrame(0.75f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f, Color.FromArgb(255, 200, 100))
                );
            beastmaster_primal_roar_track.SetFrame(1.0f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 200, 100))
                );

            brewmaster_thunder_clap_track = new AnimationTrack("Brewmaster Thunder Clap", 1.5f);
            brewmaster_thunder_clap_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(170, 90, 0))
                );
            brewmaster_thunder_clap_track.SetFrame(0.75f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(170, 90, 0))
                );
            brewmaster_thunder_clap_track.SetFrame(1.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 170, 90, 0))
                );

            centaur_hoof_stomp_track = new AnimationTrack("Centaur Stomp", 1.0f);
            centaur_hoof_stomp_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 50, 0))
                );
            centaur_hoof_stomp_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(255, 50, 0))
                );
            centaur_hoof_stomp_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 50, 0))
                );

            chaos_knight_chaos_bolt_mix = new AnimationMix();

            AnimationTrack chaos_knight_chaos_bolt_projectile_path = new AnimationTrack("Chaos Knight Bolt - Projectile Path", 0.5f);
            chaos_knight_chaos_bolt_projectile_path.SetFrame(0.0f,
                new AnimationLine(new PointF(0, Effects.canvas_height_center), new PointF(0, Effects.canvas_height_center), Color.FromArgb(255, 70, 0), 3)
                );
            chaos_knight_chaos_bolt_projectile_path.SetFrame(0.25f,
                new AnimationLine(new PointF(0, Effects.canvas_height_center), new PointF(Effects.canvas_width_center, Effects.canvas_height_center), Color.FromArgb(255, 70, 0), 3)
                );
            chaos_knight_chaos_bolt_projectile_path.SetFrame(0.5f,
                new AnimationLine(new PointF(Effects.canvas_width_center, Effects.canvas_height_center), new PointF(Effects.canvas_width_center, Effects.canvas_height_center), Color.FromArgb(0, 255, 70, 0), 3)
                );
            chaos_knight_chaos_bolt_mix.AddTrack(chaos_knight_chaos_bolt_projectile_path);

            AnimationTrack chaos_knight_chaos_bolt_projectile = new AnimationTrack("Chaos Knight Bolt - Projectile", 0.25f, 0.25f);
            chaos_knight_chaos_bolt_projectile.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(175, 0, 0))
                );
            chaos_knight_chaos_bolt_projectile.SetFrame(0.25f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(0, 175, 0, 0))
                );
            chaos_knight_chaos_bolt_mix.AddTrack(chaos_knight_chaos_bolt_projectile);

            rattletrap_rocket_flare_track = new AnimationTrack("Clockwork Rocket Flare", 0.5f);
            rattletrap_rocket_flare_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 80, 0))
                );
            rattletrap_rocket_flare_track.SetFrame(0.25f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.25f, Color.FromArgb(255, 80, 0))
                );
            rattletrap_rocket_flare_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.5f, Color.FromArgb(0, 255, 80, 0))
                );

            dragon_knight_breathe_fire_track = new AnimationTrack("Dragon Knight Breathe", 1.25f);
            dragon_knight_breathe_fire_track.SetFrame(0.0f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.10f, Color.FromArgb(255, 80, 0))
                );
            dragon_knight_breathe_fire_track.SetFrame(0.9375f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest * 0.75f, Color.FromArgb(255, 80, 0))
                );
            dragon_knight_breathe_fire_track.SetFrame(1.25f,
                new AnimationFilledCircle(0, Effects.canvas_height_center, Effects.canvas_biggest, Color.FromArgb(0, 255, 80, 0))
                );

            elder_titan_earth_splitter_track = new AnimationTrack("Elder Titan Earth Splitter", 1.0f, 3.0f);
            elder_titan_earth_splitter_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 255, 220))
                );
            elder_titan_earth_splitter_track.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 0, 255, 220))
                );

            kunkka_torrent_mix = new AnimationMix();
            AnimationTrack kunkka_torrent_bg_track = new AnimationTrack("Kunka Torrent BG", 4.0f);
            kunkka_torrent_bg_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 0, 0))
                );
            kunkka_torrent_bg_track.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 60, 80))
                );
            kunkka_torrent_bg_track.SetFrame(3.6f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 60, 80))
                );
            kunkka_torrent_bg_track.SetFrame(4.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 0, 60, 80))
                );

            AnimationTrack kunkka_torrent_spash_track = new AnimationTrack("Kunka Torrent Splash", 2.4f, 1.6f);

            kunkka_torrent_spash_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.1f / 2.0f, Color.FromArgb(0, 220, 245))
                );
            kunkka_torrent_spash_track.SetFrame(2.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest * 0.83f / 2.0f, Color.FromArgb(0, 220, 245))
                );
            kunkka_torrent_spash_track.SetFrame(2.4f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 220, 245))
                );

            kunkka_torrent_mix.AddTrack(kunkka_torrent_bg_track);
            kunkka_torrent_mix.AddTrack(kunkka_torrent_spash_track);


            kunkka_ghostship_track = new AnimationTrack("Kunka Ghostship", 2.7f);

            kunkka_ghostship_track.SetFrame(0.0f,
                    new AnimationFilledCircle(-(Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 220, 245))
                    );
            kunkka_ghostship_track.SetFrame(2.3f,
                new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f) * 0.85f, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 220, 245))
                );
            kunkka_ghostship_track.SetFrame(2.7f,
                new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 220, 245))
                );

            legion_commander_overwhelming_odds_track = new AnimationTrack("Legion Commander Overwhelming Odds", 1.0f);
            legion_commander_overwhelming_odds_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 255, 145, 0))
                );
            legion_commander_overwhelming_odds_track.SetFrame(0.3f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 145, 0))
                );
            legion_commander_overwhelming_odds_track.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 145, 0))
                );
            legion_commander_overwhelming_odds_track.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(0, 255, 145, 0))
                );

            life_stealer_rage_track = new AnimationTrack("Life Stealer Rage", 6.0f);
            life_stealer_rage_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(1.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(2.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(2.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(3.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(3.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(4.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(4.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(5.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );
            life_stealer_rage_track.SetFrame(5.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(130, 0, 0))
                );
            life_stealer_rage_track.SetFrame(6.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(175, 0, 0))
                );

            magnataur_shockwave_track = new AnimationTrack("Magnataur Shockwave", 1.0f);
            magnataur_shockwave_track.SetFrame(0.0f,
                    new AnimationFilledCircle(-(Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 205, 255))
                    );
            magnataur_shockwave_track.SetFrame(0.9f,
                new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f) * 0.9f, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 205, 255))
                );
            magnataur_shockwave_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width + (Effects.canvas_biggest / 2.0f), Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 205, 255))
                );

            omniknight_purification_track = new AnimationTrack("Omniknight Purification", 1.0f);
            omniknight_purification_track.SetFrame(0.0f,
                    new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0))
                    );
            omniknight_purification_track.SetFrame(0.8f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0))
                );
            omniknight_purification_track.SetFrame(1.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 255, 160, 0))
                );

            omniknight_repel_track = new AnimationTrack("Omniknight Repel", 12.0f);
            omniknight_repel_track.SetFrame(0.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(0.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(1.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(1.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(2.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(2.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(3.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(3.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(4.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(4.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(5.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(5.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(6.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(6.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(7.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(7.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(8.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(8.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(9.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(9.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(10.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(10.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(11.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );
            omniknight_repel_track.SetFrame(11.5f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(150, 150, 255))
                );
            omniknight_repel_track.SetFrame(12.0f,
                new AnimationFilledRectangle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_width, Effects.canvas_height, Color.FromArgb(255, 255, 255))
                );

            sandking_epicenter_mix = new AnimationMix();
            AnimationTrack sandking_epicenter_wave0 = new AnimationTrack("Sandsking Epicenter Wave0", 0.5f);
            sandking_epicenter_wave0.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(115, 255, 0), 4)
                );
            sandking_epicenter_wave0.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(115, 255, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave1 = new AnimationTrack("Sandsking Epicenter Wave1", 0.5f, 2.0f);
            sandking_epicenter_wave1.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave1.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave2 = new AnimationTrack("Sandsking Epicenter Wave2", 0.5f, 2.16f);
            sandking_epicenter_wave2.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave2.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave3 = new AnimationTrack("Sandsking Epicenter Wave3", 0.5f, 2.32f);
            sandking_epicenter_wave3.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave3.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave4 = new AnimationTrack("Sandsking Epicenter Wave4", 0.5f, 2.48f);
            sandking_epicenter_wave4.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave4.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave5 = new AnimationTrack("Sandsking Epicenter Wave5", 0.5f, 2.64f);
            sandking_epicenter_wave5.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave5.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave6 = new AnimationTrack("Sandsking Epicenter Wave6", 0.5f, 2.8f);
            sandking_epicenter_wave6.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave6.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave7 = new AnimationTrack("Sandsking Epicenter Wave7", 0.5f, 2.96f);
            sandking_epicenter_wave7.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave7.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave8 = new AnimationTrack("Sandsking Epicenter Wave8", 0.5f, 3.12f);
            sandking_epicenter_wave8.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave8.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave9 = new AnimationTrack("Sandsking Epicenter Wave9", 0.5f, 3.28f);
            sandking_epicenter_wave9.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave9.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave10 = new AnimationTrack("Sandsking Epicenter Wave10", 0.5f, 3.44f);
            sandking_epicenter_wave10.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave10.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave11 = new AnimationTrack("Sandsking Epicenter Wave11", 0.5f, 3.6f);
            sandking_epicenter_wave11.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave11.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );
            AnimationTrack sandking_epicenter_wave12 = new AnimationTrack("Sandsking Epicenter Wave12", 0.5f, 3.76f);
            sandking_epicenter_wave12.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave12.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave13 = new AnimationTrack("Sandsking Epicenter Wave13", 0.5f, 3.92f);
            sandking_epicenter_wave13.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave13.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave14 = new AnimationTrack("Sandsking Epicenter Wave14", 0.5f, 4.08f);
            sandking_epicenter_wave14.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave14.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave15 = new AnimationTrack("Sandsking Epicenter Wave15", 0.5f, 4.24f);
            sandking_epicenter_wave15.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave15.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave16 = new AnimationTrack("Sandsking Epicenter Wave16", 0.5f, 4.4f);
            sandking_epicenter_wave16.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave16.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave17 = new AnimationTrack("Sandsking Epicenter Wave17", 0.5f, 4.56f);
            sandking_epicenter_wave17.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave17.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave18 = new AnimationTrack("Sandsking Epicenter Wave18", 0.5f, 4.72f);
            sandking_epicenter_wave18.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave18.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave19 = new AnimationTrack("Sandsking Epicenter Wave19", 0.5f, 4.88f);
            sandking_epicenter_wave19.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave19.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            AnimationTrack sandking_epicenter_wave20 = new AnimationTrack("Sandsking Epicenter Wave20", 0.5f, 5f);
            sandking_epicenter_wave20.SetFrame(0.0f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(255, 160, 0), 4)
                );
            sandking_epicenter_wave20.SetFrame(0.5f,
                new AnimationCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(255, 160, 0), 4)
                );

            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave0);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave1);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave2);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave3);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave4);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave5);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave6);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave7);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave8);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave9);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave10);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave11);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave12);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave13);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave14);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave15);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave16);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave17);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave18);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave19);
            sandking_epicenter_mix.AddTrack(sandking_epicenter_wave20);


            slardar_slithereen_crush_track = new AnimationTrack("Slardar SMASH!", 0.5f);
            slardar_slithereen_crush_track.SetFrame(0.0f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, 0, Color.FromArgb(0, 150, 255))
                );
            slardar_slithereen_crush_track.SetFrame(0.45f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, (Effects.canvas_biggest / 2.0f) * 0.9f, Color.FromArgb(0, 150, 255))
                );
            slardar_slithereen_crush_track.SetFrame(0.5f,
                new AnimationFilledCircle(Effects.canvas_width_center, Effects.canvas_height_center, Effects.canvas_biggest / 2.0f, Color.FromArgb(0, 0, 150, 255))
                );

        }
    }
}
