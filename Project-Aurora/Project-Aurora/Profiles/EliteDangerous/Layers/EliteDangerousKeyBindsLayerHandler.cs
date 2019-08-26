using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aurora.EffectsEngine;
using Aurora.Settings.Layers;
using System.Drawing;
using System.Windows.Controls;
using Aurora.Devices;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;
using CSScriptLibrary;

namespace Aurora.Profiles.EliteDangerous.Layers
{
    public class
        EliteDangerousKeyBindsHandlerProperties : LayerHandlerProperties2Color<EliteDangerousKeyBindsHandlerProperties>
    {
        public Color? _HudModeCombatColor { get; set; }

        public Color HudModeCombatColor
        {
            get { return Logic._HudModeCombatColor ?? _HudModeCombatColor ?? Color.Empty; }
        }

        public Color? _HudModeDiscoveryColor { get; set; }

        public Color HudModeDiscoveryColor
        {
            get { return Logic._HudModeDiscoveryColor ?? _HudModeDiscoveryColor ?? Color.Empty; }
        }

        public Color? _NoneColor { get; set; }

        public Color NoneColor
        {
            get { return Logic._NoneColor ?? _NoneColor ?? Color.Empty; }
        }

        public Color? _OtherColor { get; set; }

        public Color OtherColor
        {
            get { return Logic._OtherColor ?? _OtherColor ?? Color.Empty; }
        }

        public Color? _UiColor { get; set; }

        public Color UiColor
        {
            get { return Logic._UiColor ?? _UiColor ?? Color.Empty; }
        }

        public Color? _UiAltColor { get; set; }

        public Color UiAltColor
        {
            get { return Logic._UiAltColor ?? _UiAltColor ?? Color.Empty; }
        }

        public Color? _ShipStuffColor { get; set; }

        public Color ShipStuffColor
        {
            get { return Logic._ShipStuffColor ?? _ShipStuffColor ?? Color.Empty; }
        }

        public Color? _CameraColor { get; set; }

        public Color CameraColor
        {
            get { return Logic._CameraColor ?? _CameraColor ?? Color.Empty; }
        }

        public Color? _DefenceColor { get; set; }

        public Color DefenceColor
        {
            get { return Logic._DefenceColor ?? _DefenceColor ?? Color.Empty; }
        }

        public Color? _DefenceDimmedColor { get; set; }

        public Color DefenceDimmedColor
        {
            get { return Logic._DefenceDimmedColor ?? _DefenceDimmedColor ?? Color.Empty; }
        }

        public Color? _OffenceColor { get; set; }

        public Color OffenceColor
        {
            get { return Logic._OffenceColor ?? _OffenceColor ?? Color.Empty; }
        }

        public Color? _OffenceDimmedColor { get; set; }

        public Color OffenceDimmedColor
        {
            get { return Logic._OffenceDimmedColor ?? _OffenceDimmedColor ?? Color.Empty; }
        }

        public Color? _MovementSpeedColor { get; set; }

        public Color MovementSpeedColor
        {
            get { return Logic._MovementSpeedColor ?? _MovementSpeedColor ?? Color.Empty; }
        }

        public Color? _MovementSpeedDimmedColor { get; set; }

        public Color MovementSpeedDimmedColor
        {
            get { return Logic._MovementSpeedDimmedColor ?? _MovementSpeedDimmedColor ?? Color.Empty; }
        }

        public Color? _MovementSecondaryColor { get; set; }

        public Color MovementSecondaryColor
        {
            get { return Logic._MovementSecondaryColor ?? _MovementSecondaryColor ?? Color.Empty; }
        }

        public Color? _WingColor { get; set; }

        public Color WingColor
        {
            get { return Logic._WingColor ?? _WingColor ?? Color.Empty; }
        }

        public Color? _NavigationColor { get; set; }

        public Color NavigationColor
        {
            get { return Logic._NavigationColor ?? _NavigationColor ?? Color.Empty; }
        }

        public Color? _ModeEnableColor { get; set; }

        public Color ModeEnableColor
        {
            get { return Logic._ModeEnableColor ?? _ModeEnableColor ?? Color.Empty; }
        }

        public Color? _ModeDisableColor { get; set; }

        public Color ModeDisableColor
        {
            get { return Logic._ModeDisableColor ?? _ModeDisableColor ?? Color.Empty; }
        }

        public EliteDangerousKeyBindsHandlerProperties() : base()
        {
        }

        public EliteDangerousKeyBindsHandlerProperties(bool assign_default = false) : base(assign_default)
        {
        }

        public override void Default()
        {
            base.Default();
            this._HudModeCombatColor = Color.FromArgb(255, 80, 0);
            this._HudModeDiscoveryColor = Color.FromArgb(0, 160, 255);
            this._NoneColor = Color.FromArgb(0, 0, 0);
            this._OtherColor = Color.FromArgb(60, 0, 0);
            this._UiColor = Color.FromArgb(255, 80, 0);
            this._UiAltColor = Color.FromArgb(255, 115, 70);
            this._ShipStuffColor = Color.FromArgb(0, 255, 0);
            this._CameraColor = Color.FromArgb(71, 164, 79);
            this._DefenceColor = Color.FromArgb(0, 220, 255);
            this._DefenceDimmedColor = Color.FromArgb(0, 70, 80);
            this._OffenceColor = Color.FromArgb(255, 0, 0);
            this._OffenceDimmedColor = Color.FromArgb(100, 0, 0);
            this._MovementSpeedColor = Color.FromArgb(136, 0, 255);
            this._MovementSpeedDimmedColor = Color.FromArgb(50, 0, 100);
            this._MovementSecondaryColor = Color.FromArgb(255, 0, 255);
            this._WingColor = Color.FromArgb(0, 0, 255);
            this._NavigationColor = Color.FromArgb(255, 220, 0);
            this._ModeEnableColor = Color.FromArgb(153, 167, 255);
            this._ModeDisableColor = Color.FromArgb(61, 88, 156);
        }

        public Color GetColorByVariableName(string colorVariableName)
        {
            switch (@colorVariableName)
            {
                case "HudModeCombatColor": return HudModeCombatColor;
                case "HudModeDiscoveryColor": return HudModeDiscoveryColor;
                case "NoneColor": return NoneColor;
                case "OtherColor": return OtherColor;
                case "UiColor": return UiColor;
                case "UiAltColor": return UiAltColor;
                case "ShipStuffColor": return ShipStuffColor;
                case "CameraColor": return CameraColor;
                case "DefenceColor": return DefenceColor;
                case "DefenceDimmedColor": return DefenceDimmedColor;
                case "OffenceColor": return OffenceColor;
                case "OffenceDimmedColor": return OffenceDimmedColor;
                case "MovementSpeedColor": return MovementSpeedColor;
                case "MovementSpeedDimmedColor": return MovementSpeedDimmedColor;
                case "MovementSecondaryColor": return MovementSecondaryColor;
                case "WingColor": return WingColor;
                case "NavigationColor": return NavigationColor;
                case "ModeEnableColor": return ModeEnableColor;
                case "ModeDisableColor": return ModeDisableColor;
            }

            return NoneColor;
        }
    }

    public class EliteDangerousKeyBindsLayerHandler : LayerHandler<EliteDangerousKeyBindsHandlerProperties>
    {
        private int blinkSpeed = 20;
        public static GameState_EliteDangerous gameState = null;

        private static ControlGroupSet CONTROLS_MAIN = new ControlGroupSet(new ControlGroup[]
        {
            new ControlGroup("CameraColor", new[]
            {
                Command.PhotoCameraToggle, Command.PhotoCameraToggle_Buggy, Command.VanityCameraScrollLeft,
                Command.VanityCameraScrollRight, Command.ToggleFreeCam, Command.FreeCamToggleHUD,
                Command.FixCameraRelativeToggle, Command.FixCameraWorldToggle
            }),
            new ControlGroup("MovementSpeedColor", new[]
            {
                Command.ForwardKey, Command.BackwardKey, Command.IncreaseEnginesPower, Command.SetSpeedZero,
                Command.SetSpeed25, Command.SetSpeed50, Command.SetSpeed75, Command.SetSpeed100
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET
            )),
            new ControlGroup("MovementSpeedColor", new[]
            {
                Command.SetSpeedMinus100, Command.SetSpeedMinus75, Command.SetSpeedMinus50,
                Command.SetSpeedMinus25, Command.AutoBreakBuggyButton
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE
            )),
            new ControlGroup("MovementSpeedColor", new[]
            {
                Command.OrderHoldPosition
            }, new StatusState(() => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("MovementSpeedColor", new[]
            {
                Command.UseBoostJuice
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.LANDING_GEAR | Flag.CARGO_SCOOP
            )),
            new ControlGroup("MovementSecondaryColor", new[]
            {
                Command.RollLeftButton, Command.RollRightButton, Command.PitchUpButton, Command.PitchDownButton
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET
            )),
            new ControlGroup("MovementSecondaryColor", new[]
            {
                Command.LeftThrustButton, Command.RightThrustButton, Command.UpThrustButton,
                Command.DownThrustButton,
                Command.ForwardThrustButton, Command.BackwardThrustButton
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE
            )),
            new ControlGroup("MovementSecondaryColor", new[]
            {
                Command.OrderFollow
            }, new StatusState(() => gameState.Journal.fighterStatus != FighterStatus.None)),
            new ControlGroup("UiColor", new[]
            {
                Command.FocusLeftPanel, Command.FocusCommsPanel, Command.QuickCommsPanel,
                Command.FocusRadarPanel, Command.FocusRightPanel, Command.UI_Select, Command.PlayerHUDModeToggle
            }),
            new ControlGroup("UiColor", new[]
            {
                Command.UI_Left, Command.UI_Right, Command.UI_Up, Command.UI_Down
            }, new StatusState(
                Flag.DOCKED
            )),
            new ControlGroup("UiColor", new[]
            {
                Command.UI_Left, Command.UI_Right, Command.UI_Up, Command.UI_Down
            }, new StatusState(
                Flag.LANDED_PLANET
            )),
            new ControlGroup("UiColor", new[]
            {
                Command.OrderAggressiveBehaviour
            }, new StatusState(() => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("NavigationColor", new[]
            {
                Command.GalaxyMapOpen, Command.SystemMapOpen, Command.TargetNextRouteSystem
            }),
            new ControlGroup("NavigationColor", new[]
            {
                Command.HyperSuperCombination, Command.Supercruise, Command.Hyperspace
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET | Flag.MASS_LOCK | Flag.LANDING_GEAR | Flag.HARDPOINTS |
                Flag.CARGO_SCOOP
            )),
            new ControlGroup("NavigationColor", new[]
            {
                Command.OrderRequestDock
            }, new StatusState(() => gameState.Journal.fighterStatus != FighterStatus.None)),
            new ControlGroup("ShipStuffColor", new[]
            {
                Command.ShipSpotLightToggle, Command.HeadlightsBuggyButton, Command.NightVisionToggle
            }),
            new ControlGroup("ShipStuffColor", new[]
            {
                Command.ToggleFlightAssist
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE
            )),

            new ControlGroup("ShipStuffColor", new[]
            {
                Command.ToggleCargoScoop, Command.LandingGearToggle
            }, new StatusState(Flag.NONE, Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.IN_FIGHTER)),

            new ControlGroup("ShipStuffColor", new[]
            {
                Command.ToggleCargoScoop_Buggy
            }, new StatusState(Flag.IN_SRV, Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.IN_FIGHTER)),

            new ControlGroup("DefenceColor", new[]
            {
                Command.IncreaseSystemsPower, Command.ChargeECM
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET
            )),
            new ControlGroup("DefenceColor", new[]
                {
                    Command.FireChaffLauncher
                },
                new StatusState(Flag.NONE, Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE,
                    () => gameState.Journal.hasChaff)),
            new ControlGroup("DefenceColor", new[]
            {
                Command.DeployHeatSink
            }, new StatusState(Flag.NONE, Flag.DOCKED | Flag.LANDED_PLANET, () => gameState.Journal.hasHeatSink)),
            new ControlGroup("DefenceColor", new[]
                {
                    Command.UseShieldCell
                },
                new StatusState(Flag.NONE, Flag.DOCKED | Flag.LANDED_PLANET,
                    () => gameState.Journal.hasShieldCellBank)),
            new ControlGroup("DefenceColor", new[]
            {
                Command.OrderDefensiveBehaviour
            }, new StatusState(() => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("OffenceColor", new[]
            {
                Command.CycleFireGroupPrevious
            }),
            new ControlGroup("OffenceColor", new[]
            {
                Command.IncreaseWeaponsPower, Command.CycleFireGroupNext, Command.SelectHighestThreat,
                Command.CycleNextSubsystem, Command.CyclePreviousSubsystem, Command.CycleNextHostileTarget,
                Command.CyclePreviousHostileTarget
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET
            )),
            new ControlGroup("OffenceColor", new[]
            {
                Command.DeployHardpointToggle
            }, new StatusState(Flag.NONE,
                Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE
            )),
            new ControlGroup("OffenceColor", new[]
            {
                Command.OrderFocusTarget
            }, new StatusState(() => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("WingColor", new[]
            {
                Command.TargetWingman0, Command.TargetWingman1,
                Command.TargetWingman2, Command.SelectTargetsTarget, Command.WingNavLock
            }, new StatusState(Flag.IN_WING
            )),
            new ControlGroup("WingColor", new[]
            {
                Command.OrderHoldFire
            }, new StatusState(() => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("ModeEnableColor", new[]
            {
                Command.ExplorationFSSEnter
            }, new StatusState(Flag.SUPERCRUISE | Flag.HUD_DISCOVERY_MODE, Flag.DOCKED | Flag.LANDED_PLANET))
        }, new StatusState(GuiFocus.NONE));

        private static ControlGroupSet CONTROLS_SYSTEM_MAP = new ControlGroupSet(new[]
        {
            new ControlGroup("MovementSpeedColor", new[]
            {
                Command.CamTranslateForward, Command.CamTranslateBackward, Command.CamTranslateLeft,
                Command.CamTranslateRight
            }),
            new ControlGroup("MovementSecondaryColor", new[]
            {
                Command.CamZoomIn, Command.CamZoomOut
            }),
            new ControlGroup("UiColor", new[]
            {
                Command.UI_Back, Command.UI_Select
            }),
            new ControlGroup("NavigationColor", new[]
            {
                Command.GalaxyMapOpen, Command.SystemMapOpen
            })
        }, new StatusState(GuiFocus.MAP_SYSTEM));

        private static ControlGroupSet CONTROLS_GALAXY_MAP = new ControlGroupSet(CONTROLS_SYSTEM_MAP, new[]
        {
            new ControlGroup("MovementSecondaryColor", new[]
            {
                Command.CamPitchUp, Command.CamPitchDown, Command.CamTranslateUp, Command.CamTranslateDown,
                Command.CamYawLeft, Command.CamYawRight
            })
        }, new StatusState(GuiFocus.MAP_GALAXY));

        private ControlGroupSet[] controlGroupSets =
        {
            CONTROLS_MAIN,
            CONTROLS_SYSTEM_MAP,
            CONTROLS_GALAXY_MAP
        };

        private Dictionary<string, StatusState> blinkingKeys = new Dictionary<string, StatusState>()
        {
            {
                Command.LandingGearToggle, new StatusState(
                    Flag.LANDING_GEAR,
                    Flag.DOCKED | Flag.LANDED_PLANET
                )
            },
            {
                Command.DeployHardpointToggle, new StatusState(
                    Flag.HARDPOINTS,
                    Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.IN_FIGHTER | Flag.IN_SRV
                )
            },
            {
                Command.ToggleCargoScoop, new StatusState(
                    Flag.CARGO_SCOOP,
                    Flag.DOCKED | Flag.LANDED_PLANET
                )
            },
            {
                Command.ToggleCargoScoop_Buggy, new StatusState(
                    Flag.CARGO_SCOOP,
                    Flag.DOCKED | Flag.LANDED_PLANET
                )
            },
            {
                Command.ShipSpotLightToggle, new StatusState(
                    Flag.SHIP_LIGHTS
                )
            },
            {
                Command.HeadlightsBuggyButton, new StatusState(
                    Flag.SHIP_LIGHTS
                )
            },
            {
                Command.NightVisionToggle, new StatusState(
                    Flag.NIGHT_VISION
                )
            },
            {
                Command.AutoBreakBuggyButton, new StatusState(
                    Flag.IN_SRV | Flag.SRV_HANDBRAKE
                )
            }
        };

        public EliteDangerousKeyBindsLayerHandler() : base()
        {
            _ID = "EliteDangerousKeyBinds";
        }

        protected override UserControl CreateControl()
        {
            return new Control_EliteDangerousKeyBindsLayer(this);
        }

        private float GetBlinkStep()
        {
            float animationPosition =
                Utils.Time.GetMillisecondsSinceEpoch() % (10000L / blinkSpeed) / (10000.0f / blinkSpeed);
            float animationStep = animationPosition * 2;
            return animationStep > 1 ? 1F + (1F - animationStep) : animationStep;
        }

        private Color GetBlinkingColor(Color baseColor)
        {
            return Utils.ColorUtils.BlendColors(
                baseColor,
                Color.FromArgb(
                    0,
                    baseColor.R,
                    baseColor.G,
                    baseColor.B
                ),
                GetBlinkStep()
            );
        }

        public override EffectLayer Render(IGameState state)
        {
            gameState = state as GameState_EliteDangerous;
            GSI.Nodes.Controls controls = (state as GameState_EliteDangerous).Controls;

            EffectLayer keyBindsLayer = new EffectLayer("Elite: Dangerous - Key Binds");

            foreach (ControlGroupSet controlGroupSet in controlGroupSets)
            {
                if (!controlGroupSet.ConditionSatisfied(gameState.Status)) continue;

                foreach (ControlGroup controlGroup in controlGroupSet.controlGroups)
                {
                    controlGroup.color = Properties.GetColorByVariableName(controlGroup.colorGroupName);

                    if (!controlGroup.ConditionSatisfied(gameState.Status)) continue;

                    foreach (string command in controlGroup.commands)
                    {
                        if (!controls.commandToBind.ContainsKey(command)) continue;

                        bool blinkingKey = blinkingKeys.ContainsKey(command) &&
                                           blinkingKeys[command].ConditionSatisfied(gameState.Status);

                        foreach (Bind.Mapping mapping in controls.commandToBind[command].mappings)
                        {
                            keyBindsLayer.Set(mapping.key,
                                blinkingKey ? GetBlinkingColor(controlGroup.color) : controlGroup.color);
                        }
                    }
                }
            }

            return keyBindsLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_EliteDangerousKeyBindsLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}