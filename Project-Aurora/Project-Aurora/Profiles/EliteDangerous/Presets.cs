using System.Collections.Generic;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;

namespace Aurora.Profiles.EliteDangerous
{
    public class ControlGroupSets
    {
        public static ControlGroupSet CONTROLS_MAIN = new ControlGroupSet(new[]
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
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET
            )),
            new ControlGroup("MovementSpeedColor", new[]
            {
                Command.SetSpeedMinus100, Command.SetSpeedMinus75, Command.SetSpeedMinus50,
                Command.SetSpeedMinus25, Command.AutoBreakBuggyButton
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE
            )),
            new ControlGroup("MovementSpeedColor", new[]
            {
                Command.OrderHoldPosition
            }, new GameStateCondition(callback: gameState => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("MovementSpeedColor", new[]
            {
                Command.UseBoostJuice
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.LANDING_GEAR | Flag.CARGO_SCOOP
            )),
            new ControlGroup("MovementSecondaryColor", new[]
            {
                Command.RollLeftButton, Command.RollRightButton, Command.PitchUpButton, Command.PitchDownButton
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET
            )),
            new ControlGroup("MovementSecondaryColor", new[]
            {
                Command.LeftThrustButton, Command.RightThrustButton, Command.UpThrustButton,
                Command.DownThrustButton,
                Command.ForwardThrustButton, Command.BackwardThrustButton
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE
            )),
            new ControlGroup("MovementSecondaryColor", new[]
            {
                Command.OrderFollow
            }, new GameStateCondition(callback: gameState => gameState.Journal.fighterStatus != FighterStatus.None)),
            new ControlGroup("UiColor", new[]
            {
                Command.FocusLeftPanel, Command.FocusCommsPanel, Command.QuickCommsPanel,
                Command.FocusRadarPanel, Command.FocusRightPanel, Command.UI_Select, Command.PlayerHUDModeToggle
            }),
            new ControlGroup("UiColor", new[]
            {
                Command.UI_Left, Command.UI_Right, Command.UI_Up, Command.UI_Down
            }, new GameStateCondition(flagsSet:
                Flag.DOCKED
            )),
            new ControlGroup("UiColor", new[]
            {
                Command.UI_Left, Command.UI_Right, Command.UI_Up, Command.UI_Down
            }, new GameStateCondition(flagsSet:
                Flag.LANDED_PLANET
            )),
            new ControlGroup("UiColor", new[]
            {
                Command.OrderAggressiveBehaviour
            }, new GameStateCondition(callback: gameState => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("NavigationColor", new[]
            {
                Command.GalaxyMapOpen, Command.SystemMapOpen, Command.TargetNextRouteSystem
            }),
            new ControlGroup("NavigationColor", new[]
            {
                Command.HyperSuperCombination, Command.Supercruise, Command.Hyperspace
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.MASS_LOCK | Flag.LANDING_GEAR | Flag.HARDPOINTS |
                             Flag.CARGO_SCOOP
            )),
            new ControlGroup("NavigationColor", new[]
            {
                Command.OrderRequestDock
            }, new GameStateCondition(callback: gameState => gameState.Journal.fighterStatus != FighterStatus.None)),
            new ControlGroup("ShipStuffColor", new[]
            {
                Command.ShipSpotLightToggle, Command.HeadlightsBuggyButton, Command.NightVisionToggle
            }),
            new ControlGroup("ShipStuffColor", new[]
            {
                Command.ToggleFlightAssist
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE
            )),

            new ControlGroup("ShipStuffColor", new[]
                {
                    Command.ToggleCargoScoop, Command.LandingGearToggle
                },
                new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.IN_FIGHTER)),

            new ControlGroup("ShipStuffColor", new[]
                {
                    Command.ToggleCargoScoop_Buggy
                },
                new GameStateCondition(flagsSet: Flag.IN_SRV,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.IN_FIGHTER)),

            new ControlGroup("DefenceColor", new[]
            {
                Command.IncreaseSystemsPower, Command.ChargeECM
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET
            )),
            new ControlGroup("DefenceColor", new[]
                {
                    Command.FireChaffLauncher
                },
                new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE,
                    callback: gameState => gameState.Journal.hasChaff)),
            new ControlGroup("DefenceColor", new[]
                {
                    Command.DeployHeatSink
                },
                new GameStateCondition(flagsSet: Flag.UNSPECIFIED, flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET,
                    callback: gameState => gameState.Journal.hasHeatSink)),
            new ControlGroup("DefenceColor", new[]
                {
                    Command.UseShieldCell
                },
                new GameStateCondition(flagsSet: Flag.UNSPECIFIED, flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET,
                    callback: gameState => gameState.Journal.hasShieldCellBank)),
            new ControlGroup("DefenceColor", new[]
            {
                Command.OrderDefensiveBehaviour
            }, new GameStateCondition(callback: gameState => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("OffenceColor", new[]
            {
                Command.CycleFireGroupPrevious
            }),
            new ControlGroup("OffenceColor", new[]
            {
                Command.IncreaseWeaponsPower, Command.CycleFireGroupNext, Command.SelectHighestThreat,
                Command.CycleNextSubsystem, Command.CyclePreviousSubsystem, Command.CycleNextHostileTarget,
                Command.CyclePreviousHostileTarget
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET
            )),
            new ControlGroup("OffenceColor", new[]
            {
                Command.DeployHardpointToggle
            }, new GameStateCondition(flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE
            )),
            new ControlGroup("OffenceColor", new[]
            {
                Command.OrderFocusTarget
            }, new GameStateCondition(callback: gameState => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("WingColor", new[]
            {
                Command.TargetWingman0, Command.TargetWingman1,
                Command.TargetWingman2, Command.SelectTargetsTarget, Command.WingNavLock
            }, new GameStateCondition(flagsSet: Flag.IN_WING
            )),
            new ControlGroup("WingColor", new[]
            {
                Command.OrderHoldFire
            }, new GameStateCondition(callback: gameState => gameState.Journal.fighterStatus != FighterStatus.None)),

            new ControlGroup("ModeEnableColor", new[]
                {
                    Command.ExplorationFSSEnter
                },
                new GameStateCondition(flagsSet: Flag.SUPERCRUISE | Flag.HUD_DISCOVERY_MODE,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET))
        }, new GameStateCondition(guiFocus: new[] {GuiFocus.NONE}));

        public static ControlGroupSet CONTROLS_SYSTEM_MAP = new ControlGroupSet(new[]
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
        }, new GameStateCondition(guiFocus: new[] {GuiFocus.MAP_SYSTEM}));

        public static ControlGroupSet CONTROLS_GALAXY_MAP = new ControlGroupSet(CONTROLS_SYSTEM_MAP, new[]
        {
            new ControlGroup("MovementSecondaryColor", new[]
            {
                Command.CamPitchUp, Command.CamPitchDown, Command.CamTranslateUp, Command.CamTranslateDown,
                Command.CamYawLeft, Command.CamYawRight
            })
        }, new GameStateCondition(guiFocus: new[] {GuiFocus.MAP_GALAXY, GuiFocus.MAP_ORRERY}));

        public static ControlGroupSet UI_PANELS = new ControlGroupSet(CONTROLS_MAIN, new[]
            {
                new ControlGroup("UiColor", new[]
                {
                    Command.UI_Left, Command.UI_Right, Command.UI_Up, Command.UI_Down,
                    Command.UI_Select, Command.UI_Back
                }),
                new ControlGroup("UiAltColor", new[]
                {
                    Command.CyclePreviousPanel, Command.CycleNextPanel
                })
            },
            new GameStateCondition(guiFocus: new[]
                {GuiFocus.STATION_SERVICES, GuiFocus.PANEL_NAV, GuiFocus.PANEL_COMS, GuiFocus.PANEL_ROLE, GuiFocus.PANEL_SYSTEMS}));
    }

    public class KeyPresets
    {
        public static Dictionary<string, GameStateCondition> BLINKING_KEYS =
            new Dictionary<string, GameStateCondition>()
            {
                {
                    Command.LandingGearToggle, new GameStateCondition(flagsSet:
                        Flag.LANDING_GEAR,
                        flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET
                    )
                },
                {
                    Command.DeployHardpointToggle, new GameStateCondition(flagsSet:
                        Flag.HARDPOINTS,
                        flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.IN_FIGHTER | Flag.IN_SRV
                    )
                },
                {
                    Command.ToggleCargoScoop, new GameStateCondition(flagsSet:
                        Flag.CARGO_SCOOP,
                        flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET
                    )
                },
                {
                    Command.ToggleCargoScoop_Buggy, new GameStateCondition(flagsSet:
                        Flag.CARGO_SCOOP,
                        flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET
                    )
                },
                {
                    Command.ShipSpotLightToggle, new GameStateCondition(flagsSet:
                        Flag.SHIP_LIGHTS
                    )
                },
                {
                    Command.HeadlightsBuggyButton, new GameStateCondition(flagsSet:
                        Flag.SHIP_LIGHTS
                    )
                },
                {
                    Command.NightVisionToggle, new GameStateCondition(flagsSet:
                        Flag.NIGHT_VISION
                    )
                },
                {
                    Command.AutoBreakBuggyButton, new GameStateCondition(flagsSet:
                        Flag.IN_SRV | Flag.SRV_HANDBRAKE
                    )
                }
            };
    }
}