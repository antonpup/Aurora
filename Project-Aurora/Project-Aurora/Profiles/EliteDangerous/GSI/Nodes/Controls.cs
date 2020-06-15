using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aurora.Devices;
using CSScriptLibrary;
using Microsoft.Scripting.Utils;

namespace Aurora.Profiles.EliteDangerous.GSI.Nodes
{
    public static class Command
    {
        public const string MouseReset = "MouseReset";
        public const string YawLeftButton = "YawLeftButton";
        public const string YawRightButton = "YawRightButton";
        public const string YawToRollButton = "YawToRollButton";
        public const string RollLeftButton = "RollLeftButton";
        public const string RollRightButton = "RollRightButton";
        public const string PitchUpButton = "PitchUpButton";
        public const string PitchDownButton = "PitchDownButton";
        public const string LeftThrustButton = "LeftThrustButton";
        public const string RightThrustButton = "RightThrustButton";
        public const string UpThrustButton = "UpThrustButton";
        public const string DownThrustButton = "DownThrustButton";
        public const string ForwardThrustButton = "ForwardThrustButton";
        public const string BackwardThrustButton = "BackwardThrustButton";
        public const string UseAlternateFlightValuesToggle = "UseAlternateFlightValuesToggle";
        public const string ToggleReverseThrottleInput = "ToggleReverseThrottleInput";

        public const string ForwardKey = "ForwardKey";
        public const string BackwardKey = "BackwardKey";
        public const string SetSpeedMinus100 = "SetSpeedMinus100";
        public const string SetSpeedMinus75 = "SetSpeedMinus75";
        public const string SetSpeedMinus50 = "SetSpeedMinus50";
        public const string SetSpeedMinus25 = "SetSpeedMinus25";
        public const string SetSpeedZero = "SetSpeedZero";
        public const string SetSpeed25 = "SetSpeed25";
        public const string SetSpeed50 = "SetSpeed50";
        public const string SetSpeed75 = "SetSpeed75";
        public const string SetSpeed100 = "SetSpeed100";

        public const string YawLeftButton_Landing = "YawLeftButton_Landing";
        public const string YawRightButton_Landing = "YawRightButton_Landing";
        public const string PitchUpButton_Landing = "PitchUpButton_Landing";
        public const string PitchDownButton_Landing = "PitchDownButton_Landing";
        public const string RollLeftButton_Landing = "RollLeftButton_Landing";
        public const string RollRightButton_Landing = "RollRightButton_Landing";
        public const string LeftThrustButton_Landing = "LeftThrustButton_Landing";
        public const string RightThrustButton_Landing = "RightThrustButton_Landing";
        public const string UpThrustButton_Landing = "UpThrustButton_Landing";
        public const string DownThrustButton_Landing = "DownThrustButton_Landing";
        public const string ForwardThrustButton_Landing = "ForwardThrustButton_Landing";
        public const string BackwardThrustButton_Landing = "BackwardThrustButton_Landing";

        public const string ToggleFlightAssist = "ToggleFlightAssist";

        public const string UseBoostJuice = "UseBoostJuice";

        public const string HyperSuperCombination = "HyperSuperCombination";
        public const string Supercruise = "Supercruise";
        public const string Hyperspace = "Hyperspace";

        public const string DisableRotationCorrectToggle = "DisableRotationCorrectToggle";
        public const string OrbitLinesToggle = "OrbitLinesToggle";

        public const string SelectTarget = "SelectTarget";
        public const string CycleNextTarget = "CycleNextTarget";
        public const string CyclePreviousTarget = "CyclePreviousTarget";

        public const string SelectHighestThreat = "SelectHighestThreat";
        public const string CycleNextHostileTarget = "CycleNextHostileTarget";
        public const string CyclePreviousHostileTarget = "CyclePreviousHostileTarget";

        public const string TargetWingman0 = "TargetWingman0";
        public const string TargetWingman1 = "TargetWingman1";
        public const string TargetWingman2 = "TargetWingman2";
        public const string SelectTargetsTarget = "SelectTargetsTarget";
        public const string WingNavLock = "WingNavLock";

        public const string CycleNextSubsystem = "CycleNextSubsystem";
        public const string CyclePreviousSubsystem = "CyclePreviousSubsystem";

        public const string TargetNextRouteSystem = "TargetNextRouteSystem";

        public const string PrimaryFire = "PrimaryFire";
        public const string SecondaryFire = "SecondaryFire";
        public const string CycleFireGroupNext = "CycleFireGroupNext";
        public const string CycleFireGroupPrevious = "CycleFireGroupPrevious";
        public const string DeployHardpointToggle = "DeployHardpointToggle";

        public const string ToggleButtonUpInput = "ToggleButtonUpInput"; //Silent running
        public const string DeployHeatSink = "DeployHeatSink";

        public const string ShipSpotLightToggle = "ShipSpotLightToggle";
        public const string RadarIncreaseRange = "RadarIncreaseRange";
        public const string RadarDecreaseRange = "RadarDecreaseRange";

        public const string IncreaseEnginesPower = "IncreaseEnginesPower";
        public const string IncreaseWeaponsPower = "IncreaseWeaponsPower";
        public const string IncreaseSystemsPower = "IncreaseSystemsPower";
        public const string ResetPowerDistribution = "ResetPowerDistribution";

        public const string HMDReset = "HMDReset";
        public const string ToggleCargoScoop = "ToggleCargoScoop";

        public const string EjectAllCargo = "EjectAllCargo";

        public const string LandingGearToggle = "LandingGearToggle";
        public const string MicrophoneMute = "MicrophoneMute";

        public const string UseShieldCell = "UseShieldCell";
        public const string FireChaffLauncher = "FireChaffLauncher";
        public const string ChargeECM = "ChargeECM";

        public const string WeaponColourToggle = "WeaponColourToggle";
        public const string EngineColourToggle = "EngineColourToggle";

        public const string NightVisionToggle = "NightVisionToggle";

        public const string UIFocus = "UIFocus";
        public const string FocusLeftPanel = "FocusLeftPanel";
        public const string FocusCommsPanel = "FocusCommsPanel";
        public const string QuickCommsPanel = "QuickCommsPanel";
        public const string FocusRadarPanel = "FocusRadarPanel";
        public const string FocusRightPanel = "FocusRightPanel";

        public const string GalaxyMapOpen = "GalaxyMapOpen";
        public const string SystemMapOpen = "SystemMapOpen";

        public const string ShowPGScoreSummaryInput = "ShowPGScoreSummaryInput";

        public const string HeadLookToggle = "HeadLookToggle";
        public const string Pause = "Pause";
        public const string FriendsMenu = "FriendsMenu";
        public const string OpenCodexGoToDiscovery = "OpenCodexGoToDiscovery";

        public const string PlayerHUDModeToggle = "PlayerHUDModeToggle";
        public const string ExplorationFSSEnter = "ExplorationFSSEnter";

        public const string UI_Up = "UI_Up";
        public const string UI_Down = "UI_Down";
        public const string UI_Left = "UI_Left";
        public const string UI_Right = "UI_Right";
        public const string UI_Select = "UI_Select";
        public const string UI_Back = "UI_Back";
        public const string UI_Toggle = "UI_Toggle";
        public const string CycleNextPanel = "CycleNextPanel";
        public const string CyclePreviousPanel = "CyclePreviousPanel";
        public const string CycleNextPage = "CycleNextPage";
        public const string CyclePreviousPage = "CyclePreviousPage";

        public const string HeadLookReset = "HeadLookReset";
        public const string HeadLookPitchUp = "HeadLookPitchUp";
        public const string HeadLookPitchDown = "HeadLookPitchDown";
        public const string HeadLookYawLeft = "HeadLookYawLeft";
        public const string HeadLookYawRight = "HeadLookYawRight";

        public const string CamPitchUp = "CamPitchUp";
        public const string CamPitchDown = "CamPitchDown";
        public const string CamYawLeft = "CamYawLeft";
        public const string CamYawRight = "CamYawRight";
        public const string CamTranslateForward = "CamTranslateForward";
        public const string CamTranslateBackward = "CamTranslateBackward";
        public const string CamTranslateLeft = "CamTranslateLeft";
        public const string CamTranslateRight = "CamTranslateRight";
        public const string CamTranslateUp = "CamTranslateUp";
        public const string CamTranslateDown = "CamTranslateDown";
        public const string CamZoomIn = "CamZoomIn";
        public const string CamZoomOut = "CamZoomOut";
        public const string CamTranslateZHold = "CamTranslateZHold";

        public const string GalaxyMapHome = "GalaxyMapHome";

        public const string ToggleDriveAssist = "ToggleDriveAssist";

        public const string SteerLeftButton = "SteerLeftButton";
        public const string SteerRightButton = "SteerRightButton";
        public const string BuggyRollLeftButton = "BuggyRollLeftButton";
        public const string BuggyRollRightButton = "BuggyRollRightButton";
        public const string BuggyPitchUpButton = "BuggyPitchUpButton";
        public const string BuggyPitchDownButton = "BuggyPitchDownButton";
        public const string VerticalThrustersButton = "VerticalThrustersButton";

        public const string BuggyPrimaryFireButton = "BuggyPrimaryFireButton";
        public const string BuggySecondaryFireButton = "BuggySecondaryFireButton";
        public const string AutoBreakBuggyButton = "AutoBreakBuggyButton";

        public const string HeadlightsBuggyButton = "HeadlightsBuggyButton";

        public const string ToggleBuggyTurretButton = "ToggleBuggyTurretButton";

        public const string BuggyCycleFireGroupNext = "BuggyCycleFireGroupNext";
        public const string BuggyCycleFireGroupPrevious = "BuggyCycleFireGroupPrevious";

        public const string SelectTarget_Buggy = "SelectTarget_Buggy";

        public const string BuggyTurretYawLeftButton = "BuggyTurretYawLeftButton";
        public const string BuggyTurretYawRightButton = "BuggyTurretYawRightButton";
        public const string BuggyTurretPitchUpButton = "BuggyTurretPitchUpButton";
        public const string BuggyTurretPitchDownButton = "BuggyTurretPitchDownButton";
        public const string BuggyToggleReverseThrottleInput = "BuggyToggleReverseThrottleInput";

        public const string IncreaseSpeedButtonMax = "IncreaseSpeedButtonMax";
        public const string DecreaseSpeedButtonMax = "DecreaseSpeedButtonMax";
        public const string IncreaseEnginesPower_Buggy = "IncreaseEnginesPower_Buggy";

        public const string IncreaseWeaponsPower_Buggy = "IncreaseWeaponsPower_Buggy";
        public const string IncreaseSystemsPower_Buggy = "IncreaseSystemsPower_Buggy";

        public const string ResetPowerDistribution_Buggy = "ResetPowerDistribution_Buggy";

        public const string ToggleCargoScoop_Buggy = "ToggleCargoScoop_Buggy";

        public const string EjectAllCargo_Buggy = "EjectAllCargo_Buggy";

        public const string RecallDismissShip = "RecallDismissShip";

        public const string UIFocus_Buggy = "UIFocus_Buggy";
        public const string FocusLeftPanel_Buggy = "FocusLeftPanel_Buggy";
        public const string FocusCommsPanel_Buggy = "FocusCommsPanel_Buggy";
        public const string QuickCommsPanel_Buggy = "QuickCommsPanel_Buggy";
        public const string FocusRadarPanel_Buggy = "FocusRadarPanel_Buggy";
        public const string FocusRightPanel_Buggy = "FocusRightPanel_Buggy";

        public const string GalaxyMapOpen_Buggy = "GalaxyMapOpen_Buggy";
        public const string SystemMapOpen_Buggy = "SystemMapOpen_Buggy";
        public const string OpenCodexGoToDiscovery_Buggy = "OpenCodexGoToDiscovery_Buggy";

        public const string PlayerHUDModeToggle_Buggy = "PlayerHUDModeToggle_Buggy";
        public const string HeadLookToggle_Buggy = "HeadLookToggle_Buggy";

        public const string MultiCrewToggleMode = "MultiCrewToggleMode";

        public const string MultiCrewPrimaryFire = "MultiCrewPrimaryFire";
        public const string MultiCrewSecondaryFire = "MultiCrewSecondaryFire";

        public const string MultiCrewPrimaryUtilityFire = "MultiCrewPrimaryUtilityFire";
        public const string MultiCrewSecondaryUtilityFire = "MultiCrewSecondaryUtilityFire";

        public const string MultiCrewThirdPersonYawLeftButton = "MultiCrewThirdPersonYawLeftButton";
        public const string MultiCrewThirdPersonYawRightButton = "MultiCrewThirdPersonYawRightButton";
        public const string MultiCrewThirdPersonPitchUpButton = "MultiCrewThirdPersonPitchUpButton";
        public const string MultiCrewThirdPersonPitchDownButton = "MultiCrewThirdPersonPitchDownButton";
        public const string MultiCrewThirdPersonFovOutButton = "MultiCrewThirdPersonFovOutButton";
        public const string MultiCrewThirdPersonFovInButton = "MultiCrewThirdPersonFovInButton";

        public const string MultiCrewCockpitUICycleForward = "MultiCrewCockpitUICycleForward";
        public const string MultiCrewCockpitUICycleBackward = "MultiCrewCockpitUICycleBackward";

        public const string OrderRequestDock = "OrderRequestDock";
        public const string OrderDefensiveBehaviour = "OrderDefensiveBehaviour";
        public const string OrderAggressiveBehaviour = "OrderAggressiveBehaviour";

        public const string OrderFocusTarget = "OrderFocusTarget";
        public const string OrderHoldFire = "OrderHoldFire";
        public const string OrderHoldPosition = "OrderHoldPosition";
        public const string OrderFollow = "OrderFollow";

        public const string OpenOrders = "OpenOrders";

        public const string PhotoCameraToggle = "PhotoCameraToggle";
        public const string PhotoCameraToggle_Buggy = "PhotoCameraToggle_Buggy";

        public const string VanityCameraScrollLeft = "VanityCameraScrollLeft";
        public const string VanityCameraScrollRight = "VanityCameraScrollRight";
        public const string ToggleFreeCam = "ToggleFreeCam";
        public const string VanityCameraOne = "VanityCameraOne";
        public const string VanityCameraTwo = "VanityCameraTwo";
        public const string VanityCameraThree = "VanityCameraThree";
        public const string VanityCameraFour = "VanityCameraFour";
        public const string VanityCameraFive = "VanityCameraFive";
        public const string VanityCameraSix = "VanityCameraSix";
        public const string VanityCameraSeven = "VanityCameraSeven";
        public const string VanityCameraEight = "VanityCameraEight";
        public const string VanityCameraNine = "VanityCameraNine";
        public const string FreeCamToggleHUD = "FreeCamToggleHUD";

        public const string FreeCamSpeedInc = "FreeCamSpeedInc";
        public const string FreeCamSpeedDec = "FreeCamSpeedDec";
        public const string ToggleReverseThrottleInputFreeCam = "ToggleReverseThrottleInputFreeCam";
        public const string MoveFreeCamForward = "MoveFreeCamForward";
        public const string MoveFreeCamBackwards = "MoveFreeCamBackwards";
        public const string MoveFreeCamRight = "MoveFreeCamRight";
        public const string MoveFreeCamLeft = "MoveFreeCamLeft";
        public const string MoveFreeCamUp = "MoveFreeCamUp";
        public const string MoveFreeCamDown = "MoveFreeCamDown";
        public const string PitchCameraUp = "PitchCameraUp";
        public const string PitchCameraDown = "PitchCameraDown";
        public const string YawCameraLeft = "YawCameraLeft";
        public const string YawCameraRight = "YawCameraRight";
        public const string RollCameraLeft = "RollCameraLeft";
        public const string RollCameraRight = "RollCameraRight";
        public const string ToggleRotationLock = "ToggleRotationLock";
        public const string FixCameraRelativeToggle = "FixCameraRelativeToggle";
        public const string FixCameraWorldToggle = "FixCameraWorldToggle";
        public const string QuitCamera = "QuitCamera";
        public const string ToggleAdvanceMode = "ToggleAdvanceMode";
        public const string FreeCamZoomIn = "FreeCamZoomIn";
        public const string FreeCamZoomOut = "FreeCamZoomOut";
        public const string FStopDec = "FStopDec";
        public const string FStopInc = "FStopInc";
        public const string CommanderCreator_Undo = "CommanderCreator_Undo";
        public const string CommanderCreator_Redo = "CommanderCreator_Redo";
        public const string CommanderCreator_Rotation_MouseToggle = "CommanderCreator_Rotation_MouseToggle";
        public const string GalnetAudio_Play_Pause = "GalnetAudio_Play_Pause";
        public const string GalnetAudio_SkipForward = "GalnetAudio_SkipForward";
        public const string GalnetAudio_SkipBackward = "GalnetAudio_SkipBackward";
        public const string GalnetAudio_ClearQueue = "GalnetAudio_ClearQueue";
        public const string ExplorationFSSCameraPitchIncreaseButton = "ExplorationFSSCameraPitchIncreaseButton";
        public const string ExplorationFSSCameraPitchDecreaseButton = "ExplorationFSSCameraPitchDecreaseButton";
        public const string ExplorationFSSCameraYawIncreaseButton = "ExplorationFSSCameraYawIncreaseButton";
        public const string ExplorationFSSCameraYawDecreaseButton = "ExplorationFSSCameraYawDecreaseButton";
        public const string ExplorationFSSZoomIn = "ExplorationFSSZoomIn";
        public const string ExplorationFSSZoomOut = "ExplorationFSSZoomOut";
        public const string ExplorationFSSMiniZoomIn = "ExplorationFSSMiniZoomIn";
        public const string ExplorationFSSMiniZoomOut = "ExplorationFSSMiniZoomOut";
        public const string ExplorationFSSRadioTuningX_Increase = "ExplorationFSSRadioTuningX_Increase";
        public const string ExplorationFSSRadioTuningX_Decrease = "ExplorationFSSRadioTuningX_Decrease";
        public const string ExplorationFSSDiscoveryScan = "ExplorationFSSDiscoveryScan";
        public const string ExplorationFSSQuit = "ExplorationFSSQuit";
        public const string ExplorationFSSTarget = "ExplorationFSSTarget";
        public const string ExplorationFSSShowHelp = "ExplorationFSSShowHelp";
        public const string ExplorationSAAChangeScannedAreaViewToggle = "ExplorationSAAChangeScannedAreaViewToggle";
        public const string ExplorationSAAExitThirdPerson = "ExplorationSAAExitThirdPerson";
        public const string SAAThirdPersonYawLeftButton = "SAAThirdPersonYawLeftButton";
        public const string SAAThirdPersonYawRightButton = "SAAThirdPersonYawRightButton";
        public const string SAAThirdPersonPitchUpButton = "SAAThirdPersonPitchUpButton";
        public const string SAAThirdPersonPitchDownButton = "SAAThirdPersonPitchDownButton";
        public const string SAAThirdPersonFovOutButton = "SAAThirdPersonFovOutButton";
        public const string SAAThirdPersonFovInButton = "SAAThirdPersonFovInButton";
    }

    public class Bind
    {
        public static Dictionary<string, DeviceKeys> eliteKeyToDeviceKey = new Dictionary<string, DeviceKeys>()
        {
            {"Key_Escape", DeviceKeys.ESC},
            {"Key_F1", DeviceKeys.F1},
            {"Key_F2", DeviceKeys.F2},
            {"Key_F3", DeviceKeys.F3},
            {"Key_F4", DeviceKeys.F4},
            {"Key_F5", DeviceKeys.F5},
            {"Key_F6", DeviceKeys.F6},
            {"Key_F7", DeviceKeys.F7},
            {"Key_F8", DeviceKeys.F8},
            {"Key_F9", DeviceKeys.F9},
            {"Key_F10", DeviceKeys.F10},
            {"Key_F11", DeviceKeys.F11},
            {"Key_F12", DeviceKeys.F12},
            {"Key_PrintScreen", DeviceKeys.PRINT_SCREEN},
            {"Key_ScrollLock", DeviceKeys.SCROLL_LOCK},
            {"Key_PauseBreak", DeviceKeys.PAUSE_BREAK},
            {"Key_Grave", DeviceKeys.TILDE},
            {"Key_1", DeviceKeys.ONE},
            {"Key_2", DeviceKeys.TWO},
            {"Key_3", DeviceKeys.THREE},
            {"Key_4", DeviceKeys.FOUR},
            {"Key_5", DeviceKeys.FIVE},
            {"Key_6", DeviceKeys.SIX},
            {"Key_7", DeviceKeys.SEVEN},
            {"Key_8", DeviceKeys.EIGHT},
            {"Key_9", DeviceKeys.NINE},
            {"Key_0", DeviceKeys.ZERO},
            {"Key_Minus", DeviceKeys.MINUS},
            {"Key_Equals", DeviceKeys.EQUALS},
            {"Key_Backspace", DeviceKeys.BACKSPACE},
            {"Key_Insert", DeviceKeys.INSERT},
            {"Key_Home", DeviceKeys.HOME},
            {"Key_PageUp", DeviceKeys.PAGE_UP},
            {"Key_Numpad_Lock", DeviceKeys.NUM_LOCK},
            {"Key_Numpad_Divide", DeviceKeys.NUM_SLASH},
            {"Key_Numpad_Multiply", DeviceKeys.NUM_ASTERISK},
            {"Key_Numpad_Subtract", DeviceKeys.NUM_MINUS},
            {"Key_Tab", DeviceKeys.TAB},
            {"Key_Q", DeviceKeys.Q},
            {"Key_W", DeviceKeys.W},
            {"Key_E", DeviceKeys.E},
            {"Key_R", DeviceKeys.R},
            {"Key_T", DeviceKeys.T},
            {"Key_Y", DeviceKeys.Y},
            {"Key_U", DeviceKeys.U},
            {"Key_I", DeviceKeys.I},
            {"Key_O", DeviceKeys.O},
            {"Key_P", DeviceKeys.P},
            {"Key_LeftBracket", DeviceKeys.OPEN_BRACKET},
            {"Key_RightBracket", DeviceKeys.CLOSE_BRACKET},
            {"Key_BackSlash", DeviceKeys.BACKSLASH},
            {"Key_Delete", DeviceKeys.DELETE},
            {"Key_End", DeviceKeys.END},
            {"Key_PageDown", DeviceKeys.PAGE_DOWN},
            {"Key_Numpad_7", DeviceKeys.NUM_SEVEN},
            {"Key_Numpad_8", DeviceKeys.NUM_EIGHT},
            {"Key_Numpad_9", DeviceKeys.NUM_NINE},
            {"Key_Numpad_Add", DeviceKeys.NUM_PLUS},
            {"Key_CapsLock", DeviceKeys.CAPS_LOCK},
            {"Key_A", DeviceKeys.A},
            {"Key_S", DeviceKeys.S},
            {"Key_D", DeviceKeys.D},
            {"Key_F", DeviceKeys.F},
            {"Key_G", DeviceKeys.G},
            {"Key_H", DeviceKeys.H},
            {"Key_J", DeviceKeys.J},
            {"Key_K", DeviceKeys.K},
            {"Key_L", DeviceKeys.L},
            {"Key_SemiColon", DeviceKeys.SEMICOLON},
            {"Key_Apostrophe", DeviceKeys.APOSTROPHE},
            {"Key_Enter", DeviceKeys.ENTER},
            {"Key_Numpad_4", DeviceKeys.NUM_FOUR},
            {"Key_Numpad_5", DeviceKeys.NUM_FIVE},
            {"Key_Numpad_6", DeviceKeys.NUM_SIX},
            {"Key_LeftShift", DeviceKeys.LEFT_SHIFT},
            {"Key_Z", DeviceKeys.Z},
            {"Key_X", DeviceKeys.X},
            {"Key_C", DeviceKeys.C},
            {"Key_V", DeviceKeys.V},
            {"Key_B", DeviceKeys.B},
            {"Key_N", DeviceKeys.N},
            {"Key_M", DeviceKeys.M},
            {"Key_Comma", DeviceKeys.COMMA},
            {"Key_Period", DeviceKeys.PERIOD},
            {"Key_Slash", DeviceKeys.FORWARD_SLASH},
            {"Key_RightShift", DeviceKeys.RIGHT_SHIFT},
            {"Key_UpArrow", DeviceKeys.ARROW_UP},
            {"Key_Numpad_1", DeviceKeys.NUM_ONE},
            {"Key_Numpad_2", DeviceKeys.NUM_TWO},
            {"Key_Numpad_3", DeviceKeys.NUM_THREE},
            {"Key_Numpad_Enter", DeviceKeys.NUM_ENTER},
            {"Key_LeftControl", DeviceKeys.LEFT_CONTROL},
            {"Key_LeftWindows", DeviceKeys.LEFT_WINDOWS},
            {"Key_LeftAlt", DeviceKeys.LEFT_ALT},
            {"Key_Space", DeviceKeys.SPACE},
            {"Key_RightAlt", DeviceKeys.RIGHT_ALT},
            {"Key_RightWindows", DeviceKeys.RIGHT_WINDOWS},
            {"Key_Application_select", DeviceKeys.APPLICATION_SELECT},
            {"Key_RightControl", DeviceKeys.RIGHT_CONTROL},
            {"Key_LeftArrow", DeviceKeys.ARROW_LEFT},
            {"Key_DownArrow", DeviceKeys.ARROW_DOWN},
            {"Key_RightArrow", DeviceKeys.ARROW_RIGHT},
            {"Key_Numpad_0", DeviceKeys.NUM_ZERO},
            {"Key_Numpad_Decimal", DeviceKeys.NUM_PERIOD}
        };

        public class Mapping
        {
            public DeviceKeys key = DeviceKeys.NONE;
            public List<DeviceKeys> modifiers = new List<DeviceKeys>();

            public void SetKey(string eliteKey)
            {
                if (Bind.eliteKeyToDeviceKey.ContainsKey(eliteKey))
                {
                    key = Bind.eliteKeyToDeviceKey[eliteKey];
                }
            }

            public void AddModifier(string eliteKey)
            {
                if (Bind.eliteKeyToDeviceKey.ContainsKey(eliteKey))
                {
                    modifiers.Add(Bind.eliteKeyToDeviceKey[eliteKey]);
                }
            }

            public bool HasKey()
            {
                return key != null;
            }
        }

        private string command;
        public HashSet<Mapping> mappings = new HashSet<Mapping>();

        public Bind(string command)
        {
            this.command = command;
        }

        public void AddMapping(Mapping mapping)
        {
            mappings.Add(mapping);
        }
    }

    public class ControlGroupSet : NeedsGameState
    {
        public ControlGroup[] controlGroups;

        public ControlGroupSet(ControlGroupSet copyFromSet, ControlGroup[] controlGroups,
            GameStateCondition neededGameStateCondition = null)
        {
            List<ControlGroup> controlGroupList = new List<ControlGroup>();
            controlGroupList.AddRange(copyFromSet.controlGroups);
            controlGroupList.AddRange(controlGroups);

            this.controlGroups = controlGroupList.ToArray();

            if (neededGameStateCondition != null)
            {
                this.NeededGameStateCondition = neededGameStateCondition;
            }
            else if (copyFromSet.NeededGameStateCondition != null)
            {
                this.NeededGameStateCondition = copyFromSet.NeededGameStateCondition;
            }
        }

        public ControlGroupSet(ControlGroup[] controlGroups, GameStateCondition neededGameStateCondition = null) : base(
            neededGameStateCondition)
        {
            this.controlGroups = controlGroups;
        }
    }

    public class ControlGroup : NeedsGameState
    {
        public string colorGroupName = null;
        public List<string> commands;

        public ControlGroup(string[] commands) : this(commands, null)
        {
        }

        public ControlGroup(string[] commands, GameStateCondition neededGameStateCondition) : this(null, commands,
            neededGameStateCondition)
        {
        }

        public ControlGroup(string colorGroupName, string[] commands,
            GameStateCondition neededGameStateCondition) : base(
            neededGameStateCondition)
        {
            this.colorGroupName = colorGroupName;
            this.commands = commands.ToList();
        }
    }

    /// <summary>
    /// Class representing current controls configuration
    /// </summary>
    public class Controls : Node
    {
        public HashSet<string> modifierKeys = new HashSet<string>();
        public Dictionary<string, Bind> commandToBind = new Dictionary<string, Bind>();
        public Dictionary<Bind, string> bindToCommand = new Dictionary<Bind, string>();
    }
}