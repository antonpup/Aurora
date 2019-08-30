using System.Collections.Generic;
using System.Drawing;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;
using Aurora.Profiles.EliteDangerous.Layers;

namespace Aurora.Profiles.EliteDangerous
{
    public class CommandColors
    {
        public static Dictionary<string, string> commadsToColorGroup;

        private static void FillDictionary(Dictionary<string, string[]> commandColors)
        {
            commadsToColorGroup = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string[]> colorGroup in commandColors)
            {
                foreach (string command in colorGroup.Value)
                {
                    commadsToColorGroup[command] = colorGroup.Key;
                }
            }
        }

        public static string GetColorGroupForCommand(string command)
        {
            return commadsToColorGroup.ContainsKey(command) ? commadsToColorGroup[command] : ColorGroup.None;
        }

        static CommandColors()
        {
            FillDictionary(new Dictionary<string, string[]>()
            {
                {
                    ColorGroup.MovementSpeedColor, new[]
                    {
                        Command.ForwardKey,
                        Command.BackwardKey,
                        Command.SetSpeedMinus100,
                        Command.SetSpeedMinus75,
                        Command.SetSpeedMinus50,
                        Command.SetSpeedMinus25,
                        Command.SetSpeedZero,
                        Command.SetSpeed25,
                        Command.SetSpeed50,
                        Command.SetSpeed75,
                        Command.SetSpeed100,

                        Command.UseBoostJuice,
                        Command.IncreaseEnginesPower,

                        Command.CamTranslateForward,
                        Command.CamTranslateBackward,
                        Command.CamTranslateLeft,
                        Command.CamTranslateRight,
                        Command.CamTranslateUp,
                        Command.CamTranslateDown,

                        Command.IncreaseSpeedButtonMax,
                        Command.DecreaseSpeedButtonMax,
                        Command.IncreaseEnginesPower_Buggy,

                        Command.OrderHoldPosition,

                        Command.FreeCamSpeedInc,
                        Command.FreeCamSpeedDec,

                        Command.MoveFreeCamForward,
                        Command.MoveFreeCamBackwards,
                    }
                },
                {
                    ColorGroup.MovementSecondaryColor, new[]
                    {
                        Command.YawLeftButton,
                        Command.YawRightButton,
                        Command.YawToRollButton,
                        Command.RollLeftButton,
                        Command.RollRightButton,
                        Command.PitchUpButton,
                        Command.PitchDownButton,
                        Command.LeftThrustButton,
                        Command.RightThrustButton,
                        Command.UpThrustButton,
                        Command.DownThrustButton,
                        Command.ForwardThrustButton,
                        Command.BackwardThrustButton,
                        Command.UseAlternateFlightValuesToggle,
                        Command.ToggleReverseThrottleInput,

                        Command.YawLeftButton_Landing,
                        Command.YawRightButton_Landing,
                        Command.PitchUpButton_Landing,
                        Command.PitchDownButton_Landing,
                        Command.RollLeftButton_Landing,
                        Command.RollRightButton_Landing,
                        Command.LeftThrustButton_Landing,
                        Command.RightThrustButton_Landing,
                        Command.UpThrustButton_Landing,
                        Command.DownThrustButton_Landing,
                        Command.ForwardThrustButton_Landing,
                        Command.BackwardThrustButton_Landing,

                        Command.EngineColourToggle,

                        Command.CamPitchUp,
                        Command.CamPitchDown,
                        Command.CamYawLeft,
                        Command.CamYawRight,

                        Command.CamZoomIn,
                        Command.CamZoomOut,
                        Command.CamTranslateZHold,

                        Command.SteerLeftButton,
                        Command.SteerRightButton,
                        Command.BuggyRollLeftButton,
                        Command.BuggyRollRightButton,
                        Command.BuggyPitchUpButton,
                        Command.BuggyPitchDownButton,
                        Command.VerticalThrustersButton,

                        Command.AutoBreakBuggyButton,

                        Command.BuggyTurretYawLeftButton,
                        Command.BuggyTurretYawRightButton,
                        Command.BuggyTurretPitchUpButton,
                        Command.BuggyTurretPitchDownButton,
                        Command.BuggyToggleReverseThrottleInput,

                        Command.MultiCrewThirdPersonYawLeftButton,
                        Command.MultiCrewThirdPersonYawRightButton,
                        Command.MultiCrewThirdPersonPitchUpButton,
                        Command.MultiCrewThirdPersonPitchDownButton,
                        Command.MultiCrewThirdPersonFovOutButton,
                        Command.MultiCrewThirdPersonFovInButton,

                        Command.OrderFollow,
                        Command.ToggleReverseThrottleInputFreeCam,

                        Command.MoveFreeCamRight,
                        Command.MoveFreeCamLeft,
                        Command.MoveFreeCamUp,
                        Command.MoveFreeCamDown,
                        Command.PitchCameraUp,
                        Command.PitchCameraDown,
                        Command.YawCameraLeft,
                        Command.YawCameraRight,
                        Command.RollCameraLeft,
                        Command.RollCameraRight,

                        Command.FreeCamZoomIn,
                        Command.FreeCamZoomOut,
                    }
                },
                {
                    ColorGroup.ShipStuffColor, new[]
                    {
                        Command.ToggleFlightAssist,
                        Command.DisableRotationCorrectToggle,
                        Command.OrbitLinesToggle,
                        Command.ShipSpotLightToggle,
                        Command.RadarIncreaseRange,
                        Command.RadarDecreaseRange,
                        Command.HMDReset,
                        Command.ToggleCargoScoop,
                        Command.LandingGearToggle,
                        Command.NightVisionToggle,

                        Command.ShowPGScoreSummaryInput,
                        Command.HeadLookToggle,
                        Command.Pause,

                        Command.ToggleDriveAssist,
                        Command.HeadlightsBuggyButton,
                        Command.ToggleCargoScoop_Buggy,

                        Command.RecallDismissShip,

                        Command.HeadLookToggle_Buggy,

                        Command.ToggleRotationLock,
                    }
                },
                {
                    ColorGroup.NavigationColor, new[]
                    {
                        Command.HyperSuperCombination,
                        Command.Supercruise,
                        Command.Hyperspace,
                        Command.TargetNextRouteSystem,

                        Command.GalaxyMapOpen,
                        Command.SystemMapOpen,
                        Command.OpenCodexGoToDiscovery,
                        Command.GalaxyMapHome,

                        Command.GalaxyMapOpen_Buggy,
                        Command.SystemMapOpen_Buggy,
                        Command.OpenCodexGoToDiscovery_Buggy,

                        Command.OrderRequestDock,
                    }
                },
                {
                    ColorGroup.UiColor, new[]
                    {
                        Command.UIFocus,
                        Command.FocusLeftPanel,
                        Command.FocusCommsPanel,
                        Command.QuickCommsPanel,
                        Command.FocusRadarPanel,
                        Command.FocusRightPanel,

                        Command.UI_Up,
                        Command.UI_Down,
                        Command.UI_Left,
                        Command.UI_Right,
                        Command.UI_Select,
                        Command.UI_Back,
                        Command.PlayerHUDModeToggle,

                        Command.UIFocus_Buggy,
                        Command.FocusLeftPanel_Buggy,
                        Command.FocusCommsPanel_Buggy,
                        Command.QuickCommsPanel_Buggy,
                        Command.FocusRadarPanel_Buggy,
                        Command.FocusRightPanel_Buggy,

                        Command.PlayerHUDModeToggle_Buggy,

                        Command.OrderAggressiveBehaviour,

                        Command.OpenOrders,
                    }
                },
                {
                    ColorGroup.UiAltColor, new[]
                    {
                        Command.UI_Toggle,
                        Command.CycleNextPanel,
                        Command.CyclePreviousPanel,
                        Command.CycleNextPage,
                        Command.CyclePreviousPage,
                        Command.SelectTarget,
                        Command.CycleNextTarget,
                        Command.CyclePreviousTarget,

                        Command.SelectTarget_Buggy,

                        Command.MultiCrewCockpitUICycleForward,
                        Command.MultiCrewCockpitUICycleBackward,
                    }
                },
                {
                    ColorGroup.WingColor, new[]
                    {
                        Command.TargetWingman0,
                        Command.TargetWingman1,
                        Command.TargetWingman2,
                        Command.SelectTargetsTarget,
                        Command.WingNavLock,
                        Command.MicrophoneMute,
                        Command.FriendsMenu,

                        Command.MultiCrewToggleMode,

                        Command.OrderHoldFire,
                    }
                },
                {
                    ColorGroup.OffenceColor, new[]
                    {
                        Command.SelectHighestThreat,
                        Command.CycleNextHostileTarget,
                        Command.CyclePreviousHostileTarget,

                        Command.CycleNextSubsystem,
                        Command.CyclePreviousSubsystem,

                        Command.PrimaryFire,
                        Command.SecondaryFire,
                        Command.CycleFireGroupNext,
                        Command.CycleFireGroupPrevious,
                        Command.DeployHardpointToggle,
                        Command.IncreaseWeaponsPower,

                        Command.WeaponColourToggle,

                        Command.BuggyPrimaryFireButton,
                        Command.BuggySecondaryFireButton,
                        Command.ToggleBuggyTurretButton,
                        Command.BuggyCycleFireGroupNext,
                        Command.BuggyCycleFireGroupPrevious,
                        Command.IncreaseWeaponsPower_Buggy,

                        Command.MultiCrewPrimaryFire,
                        Command.MultiCrewSecondaryFire,

                        Command.OrderFocusTarget,
                    }
                },
                {
                    ColorGroup.DefenceColor, new[]
                    {
                        Command.ToggleButtonUpInput,
                        Command.DeployHeatSink,
                        Command.IncreaseSystemsPower,
                        Command.UseShieldCell,
                        Command.FireChaffLauncher,
                        Command.ChargeECM,
                        Command.IncreaseSystemsPower_Buggy,

                        Command.MultiCrewPrimaryUtilityFire,
                        Command.MultiCrewSecondaryUtilityFire,

                        Command.OrderDefensiveBehaviour,
                    }
                },
                {
                    ColorGroup.HudModeCombatColor, new[]
                    {
                        Command.SelectHighestThreat,
                    }
                },
                {
                    ColorGroup.HudModeDiscoveryColor, new[]
                    {
                        Command.ExplorationFSSEnter,
                    }
                },
                {
                    ColorGroup.CameraColor, new[]
                    {
                        Command.HeadLookReset,
                        Command.HeadLookPitchUp,
                        Command.HeadLookPitchDown,
                        Command.HeadLookYawLeft,
                        Command.HeadLookYawRight,

                        Command.PhotoCameraToggle,
                        Command.PhotoCameraToggle_Buggy,
                        Command.VanityCameraScrollLeft,
                        Command.VanityCameraScrollRight,
                        Command.VanityCameraOne,
                        Command.VanityCameraTwo,
                        Command.VanityCameraThree,
                        Command.VanityCameraFour,
                        Command.VanityCameraFive,
                        Command.VanityCameraSix,
                        Command.VanityCameraSeven,
                        Command.VanityCameraEight,
                        Command.VanityCameraNine,
                        Command.FreeCamToggleHUD,

                        Command.FixCameraRelativeToggle,
                        Command.FixCameraWorldToggle,
                        Command.QuitCamera,
                        Command.ToggleAdvanceMode,

                        Command.FStopDec,
                        Command.FStopInc,
                    }
                },
                {
                    ColorGroup.OtherColor, new[]
                    {
                        Command.MouseReset,
                        Command.CommanderCreator_Undo,
                        Command.CommanderCreator_Redo,
                        Command.CommanderCreator_Rotation_MouseToggle,
                        Command.GalnetAudio_Play_Pause,
                        Command.GalnetAudio_SkipForward,
                        Command.GalnetAudio_SkipBackward,
                        Command.GalnetAudio_ClearQueue,
                        Command.ExplorationFSSCameraPitchIncreaseButton,
                        Command.ExplorationFSSCameraPitchDecreaseButton,
                        Command.ExplorationFSSCameraYawIncreaseButton,
                        Command.ExplorationFSSCameraYawDecreaseButton,
                        Command.ExplorationFSSZoomIn,
                        Command.ExplorationFSSZoomOut,
                        Command.ExplorationFSSMiniZoomIn,
                        Command.ExplorationFSSMiniZoomOut,
                        Command.ExplorationFSSRadioTuningX_Increase,
                        Command.ExplorationFSSRadioTuningX_Decrease,
                        Command.ExplorationFSSDiscoveryScan,
                        Command.ExplorationFSSQuit,
                        Command.ExplorationFSSTarget,
                        Command.ExplorationFSSShowHelp,
                        Command.ExplorationSAAChangeScannedAreaViewToggle,
                        Command.ExplorationSAAExitThirdPerson,
                        Command.SAAThirdPersonYawLeftButton,
                        Command.SAAThirdPersonYawRightButton,
                        Command.SAAThirdPersonPitchUpButton,
                        Command.SAAThirdPersonPitchDownButton,
                        Command.SAAThirdPersonFovOutButton,
                        Command.SAAThirdPersonFovInButton,
                    }
                },
            });
        }
    }

    public class ControlGroupSets
    {
        public static ControlGroupSet CONTROLS_MAIN = new ControlGroupSet(new[]
        {
            new ControlGroup(new[]
            {
                Command.PhotoCameraToggle, Command.PhotoCameraToggle_Buggy, Command.VanityCameraScrollLeft,
                Command.VanityCameraScrollRight, Command.ToggleFreeCam, Command.FreeCamToggleHUD,
                Command.FixCameraRelativeToggle, Command.FixCameraWorldToggle,

                Command.FocusLeftPanel, Command.FocusCommsPanel, Command.QuickCommsPanel,
                Command.FocusRadarPanel, Command.FocusRightPanel, Command.UI_Select,

                Command.GalaxyMapOpen, Command.SystemMapOpen, Command.TargetNextRouteSystem,

                Command.ShipSpotLightToggle, Command.HeadlightsBuggyButton, Command.NightVisionToggle,

                Command.CycleFireGroupPrevious
            }),

            new ControlGroup(ColorGroup.HudModeCombatColor, new[] { Command.PlayerHUDModeToggle}, new GameStateCondition(flagsNotSet: Flag.HUD_DISCOVERY_MODE)),
            new ControlGroup(ColorGroup.HudModeDiscoveryColor, new[] { Command.PlayerHUDModeToggle}, new GameStateCondition(Flag.HUD_DISCOVERY_MODE)),

            new ControlGroup(new[]
            {
                Command.ForwardKey, Command.BackwardKey, Command.IncreaseEnginesPower, Command.SetSpeedZero,
                Command.SetSpeed25, Command.SetSpeed50, Command.SetSpeed75, Command.SetSpeed100,

                Command.RollLeftButton, Command.RollRightButton, Command.PitchUpButton, Command.PitchDownButton,

                Command.IncreaseSystemsPower, Command.ChargeECM,

                Command.IncreaseWeaponsPower, Command.CycleFireGroupNext, Command.SelectHighestThreat,
                Command.CycleNextSubsystem, Command.CyclePreviousSubsystem, Command.CycleNextHostileTarget,
                Command.CyclePreviousHostileTarget, Command.DeployHardpointToggle
            }, new GameStateCondition(
                flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET
            )),

            new ControlGroup(new[]
            {
                Command.SetSpeedMinus100, Command.SetSpeedMinus75, Command.SetSpeedMinus50,
                Command.SetSpeedMinus25, Command.AutoBreakBuggyButton,

                Command.LeftThrustButton, Command.RightThrustButton, Command.UpThrustButton,
                Command.DownThrustButton,
                Command.ForwardThrustButton, Command.BackwardThrustButton,

                Command.ToggleFlightAssist
            }, new GameStateCondition(
                flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE
            )),

            new ControlGroup(new[]
            {
                Command.OrderRequestDock, Command.OrderHoldPosition, Command.OrderFollow
            }, new GameStateCondition(gameState => gameState.Journal.fighterStatus != FighterStatus.None)),
            new ControlGroup(new[]
            {
                Command.OrderDefensiveBehaviour, Command.OrderAggressiveBehaviour,
                Command.OrderFocusTarget, Command.OrderHoldFire
            }, new GameStateCondition(gameState => gameState.Journal.fighterStatus == FighterStatus.Launched)),

            new ControlGroup(new[]
            {
                Command.UseBoostJuice
            }, new GameStateCondition(
                flagsSet: Flag.UNSPECIFIED,
                flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.LANDING_GEAR | Flag.CARGO_SCOOP
            )),

            new ControlGroup(new[]
            {
                Command.UI_Left, Command.UI_Right, Command.UI_Up, Command.UI_Down
            }, new GameStateCondition(
                flagsSet: Flag.DOCKED
            )),
            new ControlGroup(new[]
            {
                Command.UI_Left, Command.UI_Right, Command.UI_Up, Command.UI_Down
            }, new GameStateCondition(
                flagsSet: Flag.LANDED_PLANET
            )),

            new ControlGroup(new[]
                {
                    Command.HyperSuperCombination, Command.Supercruise, Command.Hyperspace
                },
                new GameStateCondition(
                    flagsSet: Flag.UNSPECIFIED,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.MASS_LOCK | Flag.LANDING_GEAR |
                                 Flag.HARDPOINTS | Flag.CARGO_SCOOP
                )
            ),

            new ControlGroup(new[]
                {
                    Command.ToggleCargoScoop, Command.LandingGearToggle
                },
                new GameStateCondition(
                    flagsSet: Flag.UNSPECIFIED,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.IN_FIGHTER
                )
            ),

            new ControlGroup(new[]
                {
                    Command.ToggleCargoScoop_Buggy
                },
                new GameStateCondition(
                    flagsSet: Flag.IN_SRV,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE | Flag.IN_FIGHTER
                )
            ),

            new ControlGroup(new[]
                {
                    Command.FireChaffLauncher
                },
                new GameStateCondition(
                    flagsSet: Flag.UNSPECIFIED,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.SUPERCRUISE,
                    callback: gameState => gameState.Journal.hasChaff
                )
            ),

            new ControlGroup(new[]
                {
                    Command.DeployHeatSink
                },
                new GameStateCondition(
                    flagsSet: Flag.UNSPECIFIED,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET,
                    callback: gameState => gameState.Journal.hasHeatSink
                )
            ),

            new ControlGroup(new[]
                {
                    Command.UseShieldCell
                },
                new GameStateCondition(
                    flagsSet: Flag.UNSPECIFIED,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET,
                    callback: gameState => gameState.Journal.hasShieldCellBank
                )
            ),

            new ControlGroup(new[]
            {
                Command.TargetWingman0, Command.TargetWingman1,
                Command.TargetWingman2, Command.SelectTargetsTarget, Command.WingNavLock
            }, new GameStateCondition(flagsSet: Flag.IN_WING)),
            new ControlGroup(new[]
                {
                    Command.ExplorationFSSEnter
                },
                new GameStateCondition(flagsSet: Flag.SUPERCRUISE,
                    flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET))
        }, new GameStateCondition(guiFocus: new[]
        {
            GuiFocus.NONE, GuiFocus.STATION_SERVICES, GuiFocus.PANEL_NAV, GuiFocus.PANEL_COMS, GuiFocus.PANEL_ROLE,
            GuiFocus.PANEL_SYSTEMS
        }));

        public static ControlGroupSet CONTROLS_SYSTEM_MAP = new ControlGroupSet(new[]
        {
            new ControlGroup(new[]
            {
                Command.CamTranslateForward, Command.CamTranslateBackward, Command.CamTranslateLeft,
                Command.CamTranslateRight, Command.CamZoomIn, Command.CamZoomOut, Command.UI_Back, Command.UI_Select,
                Command.GalaxyMapOpen, Command.SystemMapOpen
            }),
        }, new GameStateCondition(guiFocus: new[] {GuiFocus.MAP_SYSTEM}));

        public static ControlGroupSet CONTROLS_GALAXY_MAP = new ControlGroupSet(CONTROLS_SYSTEM_MAP, new[]
        {
            new ControlGroup(new[]
            {
                Command.CamPitchUp, Command.CamPitchDown, Command.CamTranslateUp, Command.CamTranslateDown,
                Command.CamYawLeft, Command.CamYawRight
            })
        }, new GameStateCondition(guiFocus: new[] {GuiFocus.MAP_GALAXY, GuiFocus.MAP_ORRERY}));

        public static ControlGroupSet UI_PANELS = new ControlGroupSet(new[]
            {
                new ControlGroup(new[]
                {
                    Command.UI_Left, Command.UI_Right, Command.UI_Up, Command.UI_Down,
                    Command.UI_Select, Command.UI_Back
                }),
            },
            new GameStateCondition(guiFocus: new[]
            {
                GuiFocus.STATION_SERVICES, GuiFocus.PANEL_NAV, GuiFocus.PANEL_COMS, GuiFocus.PANEL_ROLE,
                GuiFocus.PANEL_SYSTEMS, GuiFocus.CODEX
            }));

        public static ControlGroupSet UI_PANEL_TABS = new ControlGroupSet(new[]
            {
                new ControlGroup(new[]
                {
                    Command.CyclePreviousPanel, Command.CycleNextPanel
                }),
            },
            new GameStateCondition(guiFocus: new[]
            {
                GuiFocus.PANEL_NAV, GuiFocus.PANEL_COMS, GuiFocus.PANEL_ROLE,  GuiFocus.PANEL_SYSTEMS
            }));
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
                        flagsNotSet: Flag.DOCKED | Flag.LANDED_PLANET | Flag.IN_FIGHTER | Flag.IN_SRV
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
                    Command.Supercruise, new GameStateCondition(flagsSet:
                        Flag.FSD_CHARGING
                    )
                },
                {
                    Command.Hyperspace, new GameStateCondition(flagsSet:
                        Flag.FSD_CHARGING
                    )
                },
                {
                    Command.HyperSuperCombination, new GameStateCondition(flagsSet:
                        Flag.FSD_CHARGING
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