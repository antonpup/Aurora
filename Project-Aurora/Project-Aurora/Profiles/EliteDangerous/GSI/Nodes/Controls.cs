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
        public static readonly string MouseReset = "MouseReset";
        public static readonly string YawLeftButton = "YawLeftButton";
        public static readonly string YawRightButton = "YawRightButton";
        public static readonly string YawToRollButton = "YawToRollButton";
        public static readonly string RollLeftButton = "RollLeftButton";
        public static readonly string RollRightButton = "RollRightButton";
        public static readonly string PitchUpButton = "PitchUpButton";
        public static readonly string PitchDownButton = "PitchDownButton";
        public static readonly string LeftThrustButton = "LeftThrustButton";
        public static readonly string RightThrustButton = "RightThrustButton";
        public static readonly string UpThrustButton = "UpThrustButton";
        public static readonly string DownThrustButton = "DownThrustButton";
        public static readonly string ForwardThrustButton = "ForwardThrustButton";
        public static readonly string BackwardThrustButton = "BackwardThrustButton";
        public static readonly string UseAlternateFlightValuesToggle = "UseAlternateFlightValuesToggle";
        public static readonly string ToggleReverseThrottleInput = "ToggleReverseThrottleInput";
        public static readonly string ForwardKey = "ForwardKey";
        public static readonly string BackwardKey = "BackwardKey";
        public static readonly string SetSpeedMinus100 = "SetSpeedMinus100";
        public static readonly string SetSpeedMinus75 = "SetSpeedMinus75";
        public static readonly string SetSpeedMinus50 = "SetSpeedMinus50";
        public static readonly string SetSpeedMinus25 = "SetSpeedMinus25";
        public static readonly string SetSpeedZero = "SetSpeedZero";
        public static readonly string SetSpeed25 = "SetSpeed25";
        public static readonly string SetSpeed50 = "SetSpeed50";
        public static readonly string SetSpeed75 = "SetSpeed75";
        public static readonly string SetSpeed100 = "SetSpeed100";
        public static readonly string YawLeftButton_Landing = "YawLeftButton_Landing";
        public static readonly string YawRightButton_Landing = "YawRightButton_Landing";
        public static readonly string PitchUpButton_Landing = "PitchUpButton_Landing";
        public static readonly string PitchDownButton_Landing = "PitchDownButton_Landing";
        public static readonly string RollLeftButton_Landing = "RollLeftButton_Landing";
        public static readonly string RollRightButton_Landing = "RollRightButton_Landing";
        public static readonly string LeftThrustButton_Landing = "LeftThrustButton_Landing";
        public static readonly string RightThrustButton_Landing = "RightThrustButton_Landing";
        public static readonly string UpThrustButton_Landing = "UpThrustButton_Landing";
        public static readonly string DownThrustButton_Landing = "DownThrustButton_Landing";
        public static readonly string ForwardThrustButton_Landing = "ForwardThrustButton_Landing";
        public static readonly string BackwardThrustButton_Landing = "BackwardThrustButton_Landing";
        public static readonly string ToggleFlightAssist = "ToggleFlightAssist";
        public static readonly string UseBoostJuice = "UseBoostJuice";
        public static readonly string HyperSuperCombination = "HyperSuperCombination";
        public static readonly string Supercruise = "Supercruise";
        public static readonly string Hyperspace = "Hyperspace";
        public static readonly string DisableRotationCorrectToggle = "DisableRotationCorrectToggle";
        public static readonly string OrbitLinesToggle = "OrbitLinesToggle";
        public static readonly string SelectTarget = "SelectTarget";
        public static readonly string CycleNextTarget = "CycleNextTarget";
        public static readonly string CyclePreviousTarget = "CyclePreviousTarget";
        public static readonly string SelectHighestThreat = "SelectHighestThreat";
        public static readonly string CycleNextHostileTarget = "CycleNextHostileTarget";
        public static readonly string CyclePreviousHostileTarget = "CyclePreviousHostileTarget";
        public static readonly string TargetWingman0 = "TargetWingman0";
        public static readonly string TargetWingman1 = "TargetWingman1";
        public static readonly string TargetWingman2 = "TargetWingman2";
        public static readonly string SelectTargetsTarget = "SelectTargetsTarget";
        public static readonly string WingNavLock = "WingNavLock";
        public static readonly string CycleNextSubsystem = "CycleNextSubsystem";
        public static readonly string CyclePreviousSubsystem = "CyclePreviousSubsystem";
        public static readonly string TargetNextRouteSystem = "TargetNextRouteSystem";
        public static readonly string PrimaryFire = "PrimaryFire";
        public static readonly string SecondaryFire = "SecondaryFire";
        public static readonly string CycleFireGroupNext = "CycleFireGroupNext";
        public static readonly string CycleFireGroupPrevious = "CycleFireGroupPrevious";
        public static readonly string DeployHardpointToggle = "DeployHardpointToggle";
        public static readonly string ToggleButtonUpInput = "ToggleButtonUpInput";
        public static readonly string DeployHeatSink = "DeployHeatSink";
        public static readonly string ShipSpotLightToggle = "ShipSpotLightToggle";
        public static readonly string RadarIncreaseRange = "RadarIncreaseRange";
        public static readonly string RadarDecreaseRange = "RadarDecreaseRange";
        public static readonly string IncreaseEnginesPower = "IncreaseEnginesPower";
        public static readonly string IncreaseWeaponsPower = "IncreaseWeaponsPower";
        public static readonly string IncreaseSystemsPower = "IncreaseSystemsPower";
        public static readonly string ResetPowerDistribution = "ResetPowerDistribution";
        public static readonly string HMDReset = "HMDReset";
        public static readonly string ToggleCargoScoop = "ToggleCargoScoop";
        public static readonly string EjectAllCargo = "EjectAllCargo";
        public static readonly string LandingGearToggle = "LandingGearToggle";
        public static readonly string MicrophoneMute = "MicrophoneMute";
        public static readonly string UseShieldCell = "UseShieldCell";
        public static readonly string FireChaffLauncher = "FireChaffLauncher";
        public static readonly string ChargeECM = "ChargeECM";
        public static readonly string WeaponColourToggle = "WeaponColourToggle";
        public static readonly string EngineColourToggle = "EngineColourToggle";
        public static readonly string NightVisionToggle = "NightVisionToggle";
        public static readonly string UIFocus = "UIFocus";
        public static readonly string FocusLeftPanel = "FocusLeftPanel";
        public static readonly string FocusCommsPanel = "FocusCommsPanel";
        public static readonly string QuickCommsPanel = "QuickCommsPanel";
        public static readonly string FocusRadarPanel = "FocusRadarPanel";
        public static readonly string FocusRightPanel = "FocusRightPanel";
        public static readonly string GalaxyMapOpen = "GalaxyMapOpen";
        public static readonly string SystemMapOpen = "SystemMapOpen";
        public static readonly string ShowPGScoreSummaryInput = "ShowPGScoreSummaryInput";
        public static readonly string HeadLookToggle = "HeadLookToggle";
        public static readonly string Pause = "Pause";
        public static readonly string FriendsMenu = "FriendsMenu";
        public static readonly string OpenCodexGoToDiscovery = "OpenCodexGoToDiscovery";
        public static readonly string PlayerHUDModeToggle = "PlayerHUDModeToggle";
        public static readonly string UI_Up = "UI_Up";
        public static readonly string UI_Down = "UI_Down";
        public static readonly string UI_Left = "UI_Left";
        public static readonly string UI_Right = "UI_Right";
        public static readonly string UI_Select = "UI_Select";
        public static readonly string UI_Back = "UI_Back";
        public static readonly string UI_Toggle = "UI_Toggle";
        public static readonly string CycleNextPanel = "CycleNextPanel";
        public static readonly string CyclePreviousPanel = "CyclePreviousPanel";
        public static readonly string CycleNextPage = "CycleNextPage";
        public static readonly string CyclePreviousPage = "CyclePreviousPage";
        public static readonly string HeadLookReset = "HeadLookReset";
        public static readonly string HeadLookPitchUp = "HeadLookPitchUp";
        public static readonly string HeadLookPitchDown = "HeadLookPitchDown";
        public static readonly string HeadLookYawLeft = "HeadLookYawLeft";
        public static readonly string HeadLookYawRight = "HeadLookYawRight";
        public static readonly string CamPitchUp = "CamPitchUp";
        public static readonly string CamPitchDown = "CamPitchDown";
        public static readonly string CamYawLeft = "CamYawLeft";
        public static readonly string CamYawRight = "CamYawRight";
        public static readonly string CamTranslateForward = "CamTranslateForward";
        public static readonly string CamTranslateBackward = "CamTranslateBackward";
        public static readonly string CamTranslateLeft = "CamTranslateLeft";
        public static readonly string CamTranslateRight = "CamTranslateRight";
        public static readonly string CamTranslateUp = "CamTranslateUp";
        public static readonly string CamTranslateDown = "CamTranslateDown";
        public static readonly string CamZoomIn = "CamZoomIn";
        public static readonly string CamZoomOut = "CamZoomOut";
        public static readonly string CamTranslateZHold = "CamTranslateZHold";
        public static readonly string ToggleDriveAssist = "ToggleDriveAssist";
        public static readonly string SteerLeftButton = "SteerLeftButton";
        public static readonly string SteerRightButton = "SteerRightButton";
        public static readonly string BuggyRollLeftButton = "BuggyRollLeftButton";
        public static readonly string BuggyRollRightButton = "BuggyRollRightButton";
        public static readonly string BuggyPitchUpButton = "BuggyPitchUpButton";
        public static readonly string BuggyPitchDownButton = "BuggyPitchDownButton";
        public static readonly string VerticalThrustersButton = "VerticalThrustersButton";
        public static readonly string BuggyPrimaryFireButton = "BuggyPrimaryFireButton";
        public static readonly string BuggySecondaryFireButton = "BuggySecondaryFireButton";
        public static readonly string AutoBreakBuggyButton = "AutoBreakBuggyButton";
        public static readonly string HeadlightsBuggyButton = "HeadlightsBuggyButton";
        public static readonly string ToggleBuggyTurretButton = "ToggleBuggyTurretButton";
        public static readonly string SelectTarget_Buggy = "SelectTarget_Buggy";
        public static readonly string BuggyTurretYawLeftButton = "BuggyTurretYawLeftButton";
        public static readonly string BuggyTurretYawRightButton = "BuggyTurretYawRightButton";
        public static readonly string BuggyTurretPitchUpButton = "BuggyTurretPitchUpButton";
        public static readonly string BuggyTurretPitchDownButton = "BuggyTurretPitchDownButton";
        public static readonly string BuggyToggleReverseThrottleInput = "BuggyToggleReverseThrottleInput";
        public static readonly string IncreaseSpeedButtonMax = "IncreaseSpeedButtonMax";
        public static readonly string DecreaseSpeedButtonMax = "DecreaseSpeedButtonMax";
        public static readonly string IncreaseEnginesPower_Buggy = "IncreaseEnginesPower_Buggy";
        public static readonly string IncreaseWeaponsPower_Buggy = "IncreaseWeaponsPower_Buggy";
        public static readonly string IncreaseSystemsPower_Buggy = "IncreaseSystemsPower_Buggy";
        public static readonly string ResetPowerDistribution_Buggy = "ResetPowerDistribution_Buggy";
        public static readonly string ToggleCargoScoop_Buggy = "ToggleCargoScoop_Buggy";
        public static readonly string EjectAllCargo_Buggy = "EjectAllCargo_Buggy";
        public static readonly string RecallDismissShip = "RecallDismissShip";
        public static readonly string UIFocus_Buggy = "UIFocus_Buggy";
        public static readonly string FocusLeftPanel_Buggy = "FocusLeftPanel_Buggy";
        public static readonly string FocusCommsPanel_Buggy = "FocusCommsPanel_Buggy";
        public static readonly string QuickCommsPanel_Buggy = "QuickCommsPanel_Buggy";
        public static readonly string FocusRadarPanel_Buggy = "FocusRadarPanel_Buggy";
        public static readonly string FocusRightPanel_Buggy = "FocusRightPanel_Buggy";
        public static readonly string GalaxyMapOpen_Buggy = "GalaxyMapOpen_Buggy";
        public static readonly string SystemMapOpen_Buggy = "SystemMapOpen_Buggy";
        public static readonly string HeadLookToggle_Buggy = "HeadLookToggle_Buggy";
        public static readonly string MultiCrewToggleMode = "MultiCrewToggleMode";
        public static readonly string MultiCrewPrimaryFire = "MultiCrewPrimaryFire";
        public static readonly string MultiCrewSecondaryFire = "MultiCrewSecondaryFire";
        public static readonly string MultiCrewPrimaryUtilityFire = "MultiCrewPrimaryUtilityFire";
        public static readonly string MultiCrewSecondaryUtilityFire = "MultiCrewSecondaryUtilityFire";
        public static readonly string MultiCrewThirdPersonYawLeftButton = "MultiCrewThirdPersonYawLeftButton";
        public static readonly string MultiCrewThirdPersonYawRightButton = "MultiCrewThirdPersonYawRightButton";
        public static readonly string MultiCrewThirdPersonPitchUpButton = "MultiCrewThirdPersonPitchUpButton";
        public static readonly string MultiCrewThirdPersonPitchDownButton = "MultiCrewThirdPersonPitchDownButton";
        public static readonly string MultiCrewThirdPersonFovOutButton = "MultiCrewThirdPersonFovOutButton";
        public static readonly string MultiCrewThirdPersonFovInButton = "MultiCrewThirdPersonFovInButton";
        public static readonly string MultiCrewCockpitUICycleForward = "MultiCrewCockpitUICycleForward";
        public static readonly string MultiCrewCockpitUICycleBackward = "MultiCrewCockpitUICycleBackward";
        public static readonly string OrderRequestDock = "OrderRequestDock";
        public static readonly string OrderDefensiveBehaviour = "OrderDefensiveBehaviour";
        public static readonly string OrderAggressiveBehaviour = "OrderAggressiveBehaviour";
        public static readonly string OrderFocusTarget = "OrderFocusTarget";
        public static readonly string OrderHoldFire = "OrderHoldFire";
        public static readonly string OrderHoldPosition = "OrderHoldPosition";
        public static readonly string OrderFollow = "OrderFollow";
        public static readonly string OpenOrders = "OpenOrders";
        public static readonly string PhotoCameraToggle = "PhotoCameraToggle";
        public static readonly string PhotoCameraToggle_Buggy = "PhotoCameraToggle_Buggy";
        public static readonly string VanityCameraScrollLeft = "VanityCameraScrollLeft";
        public static readonly string VanityCameraScrollRight = "VanityCameraScrollRight";
        public static readonly string ToggleFreeCam = "ToggleFreeCam";
        public static readonly string VanityCameraOne = "VanityCameraOne";
        public static readonly string VanityCameraTwo = "VanityCameraTwo";
        public static readonly string VanityCameraThree = "VanityCameraThree";
        public static readonly string VanityCameraFour = "VanityCameraFour";
        public static readonly string VanityCameraFive = "VanityCameraFive";
        public static readonly string VanityCameraSix = "VanityCameraSix";
        public static readonly string VanityCameraSeven = "VanityCameraSeven";
        public static readonly string VanityCameraEight = "VanityCameraEight";
        public static readonly string VanityCameraNine = "VanityCameraNine";
        public static readonly string FreeCamToggleHUD = "FreeCamToggleHUD";
        public static readonly string FreeCamSpeedInc = "FreeCamSpeedInc";
        public static readonly string FreeCamSpeedDec = "FreeCamSpeedDec";
        public static readonly string ToggleReverseThrottleInputFreeCam = "ToggleReverseThrottleInputFreeCam";
        public static readonly string MoveFreeCamForward = "MoveFreeCamForward";
        public static readonly string MoveFreeCamBackwards = "MoveFreeCamBackwards";
        public static readonly string MoveFreeCamRight = "MoveFreeCamRight";
        public static readonly string MoveFreeCamLeft = "MoveFreeCamLeft";
        public static readonly string MoveFreeCamUp = "MoveFreeCamUp";
        public static readonly string MoveFreeCamDown = "MoveFreeCamDown";
        public static readonly string PitchCameraUp = "PitchCameraUp";
        public static readonly string PitchCameraDown = "PitchCameraDown";
        public static readonly string YawCameraLeft = "YawCameraLeft";
        public static readonly string YawCameraRight = "YawCameraRight";
        public static readonly string RollCameraLeft = "RollCameraLeft";
        public static readonly string RollCameraRight = "RollCameraRight";
        public static readonly string ToggleRotationLock = "ToggleRotationLock";
        public static readonly string FixCameraRelativeToggle = "FixCameraRelativeToggle";
        public static readonly string FixCameraWorldToggle = "FixCameraWorldToggle";
        public static readonly string QuitCamera = "QuitCamera";
        public static readonly string ToggleAdvanceMode = "ToggleAdvanceMode";
        public static readonly string FreeCamZoomIn = "FreeCamZoomIn";
        public static readonly string FreeCamZoomOut = "FreeCamZoomOut";
        public static readonly string FStopDec = "FStopDec";
        public static readonly string FStopInc = "FStopInc";
        public static readonly string CommanderCreator_Undo = "CommanderCreator_Undo";
        public static readonly string CommanderCreator_Redo = "CommanderCreator_Redo";
        public static readonly string CommanderCreator_Rotation_MouseToggle = "CommanderCreator_Rotation_MouseToggle";
        public static readonly string GalnetAudio_Play_Pause = "GalnetAudio_Play_Pause";
        public static readonly string GalnetAudio_SkipForward = "GalnetAudio_SkipForward";
        public static readonly string GalnetAudio_SkipBackward = "GalnetAudio_SkipBackward";
        public static readonly string GalnetAudio_ClearQueue = "GalnetAudio_ClearQueue";
        public static readonly string ExplorationFSSEnter = "ExplorationFSSEnter";

        public static readonly string ExplorationFSSCameraPitchIncreaseButton =
            "ExplorationFSSCameraPitchIncreaseButton";

        public static readonly string ExplorationFSSCameraPitchDecreaseButton =
            "ExplorationFSSCameraPitchDecreaseButton";

        public static readonly string ExplorationFSSCameraYawIncreaseButton = "ExplorationFSSCameraYawIncreaseButton";
        public static readonly string ExplorationFSSCameraYawDecreaseButton = "ExplorationFSSCameraYawDecreaseButton";
        public static readonly string ExplorationFSSZoomIn = "ExplorationFSSZoomIn";
        public static readonly string ExplorationFSSZoomOut = "ExplorationFSSZoomOut";
        public static readonly string ExplorationFSSMiniZoomIn = "ExplorationFSSMiniZoomIn";
        public static readonly string ExplorationFSSMiniZoomOut = "ExplorationFSSMiniZoomOut";
        public static readonly string ExplorationFSSRadioTuningX_Increase = "ExplorationFSSRadioTuningX_Increase";
        public static readonly string ExplorationFSSRadioTuningX_Decrease = "ExplorationFSSRadioTuningX_Decrease";
        public static readonly string ExplorationFSSDiscoveryScan = "ExplorationFSSDiscoveryScan";
        public static readonly string ExplorationFSSQuit = "ExplorationFSSQuit";
        public static readonly string ExplorationFSSTarget = "ExplorationFSSTarget";
        public static readonly string ExplorationFSSShowHelp = "ExplorationFSSShowHelp";

        public static readonly string ExplorationSAAChangeScannedAreaViewToggle =
            "ExplorationSAAChangeScannedAreaViewToggle";

        public static readonly string ExplorationSAAExitThirdPerson = "ExplorationSAAExitThirdPerson";
        public static readonly string SAAThirdPersonYawLeftButton = "SAAThirdPersonYawLeftButton";
        public static readonly string SAAThirdPersonYawRightButton = "SAAThirdPersonYawRightButton";
        public static readonly string SAAThirdPersonPitchUpButton = "SAAThirdPersonPitchUpButton";
        public static readonly string SAAThirdPersonPitchDownButton = "SAAThirdPersonPitchDownButton";
        public static readonly string SAAThirdPersonFovOutButton = "SAAThirdPersonFovOutButton";
        public static readonly string SAAThirdPersonFovInButton = "SAAThirdPersonFovInButton";
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
        public string colorGroupName;
        public Color color;
        public List<string> commands;

        public ControlGroup(string colorGroupName, string[] commands) : this(colorGroupName, commands, null)
        {
        }

        public ControlGroup(string colorGroupName, string[] commands, GameStateCondition neededGameStateCondition) : base(
            neededGameStateCondition)
        {
            this.colorGroupName = colorGroupName;
            this.commands = commands.ToList();
        }
    }

    /// <summary>
    /// Class representing current controls configuration
    /// </summary>
    public class Controls : Node<Controls>
    {
        public HashSet<string> modifierKeys;
        public Dictionary<string, Bind> commandToBind;
        public Dictionary<Bind, string> bindToCommand;
    }
}