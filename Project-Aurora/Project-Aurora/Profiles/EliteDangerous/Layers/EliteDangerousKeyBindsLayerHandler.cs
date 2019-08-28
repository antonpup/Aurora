using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aurora.EffectsEngine;
using Aurora.Settings.Layers;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Aurora.Devices;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Profiles.EliteDangerous.GSI.Nodes;
using Aurora.Utils;
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

        private ControlGroupSet[] controlGroupSets =
        {
            ControlGroupSets.CONTROLS_MAIN,
            ControlGroupSets.CONTROLS_SYSTEM_MAP,
            ControlGroupSets.CONTROLS_GALAXY_MAP
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
            GameState_EliteDangerous gameState = state as GameState_EliteDangerous;
           
            GSI.Nodes.Controls controls = (state as GameState_EliteDangerous).Controls;

            EffectLayer keyBindsLayer = new EffectLayer("Elite: Dangerous - Key Binds");

            foreach (ControlGroupSet controlGroupSet in controlGroupSets)
            {
                if (!controlGroupSet.IsSatisfied(gameState)) continue;

                foreach (ControlGroup controlGroup in controlGroupSet.controlGroups)
                {
                    controlGroup.color = Properties.GetColorByVariableName(controlGroup.colorGroupName);

                    if (!controlGroup.IsSatisfied(gameState)) continue;

                    foreach (string command in controlGroup.commands)
                    {
                        if (!controls.commandToBind.ContainsKey(command)) continue;

                        bool blinkingKey = KeyPresets.BLINKING_KEYS.ContainsKey(command) &&
                                           KeyPresets.BLINKING_KEYS[command].IsSatisfied(gameState);

                        foreach (Bind.Mapping mapping in controls.commandToBind[command].mappings)
                        {
                            bool allModifiersPressed = true;
                            foreach (DeviceKeys modifierKey in mapping.modifiers)
                            {
                                keyBindsLayer.Set(modifierKey, Properties.ShipStuffColor);
                                if (Array.IndexOf(Global.InputEvents.PressedKeys, KeyUtils.GetFormsKey(modifierKey)) == -1)
                                {
                                    allModifiersPressed = false;
                                    break;
                                }
                            }

                            if (!allModifiersPressed) continue;
                            
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