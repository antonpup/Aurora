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
    public class ColorGroup
    {
        public const string None = "None";
        public const string OtherColor = "OtherColor";
        public const string HudModeCombatColor = "HudModeCombatColor";
        public const string HudModeDiscoveryColor = "HudModeDiscoveryColor";
        public const string UiColor = "UiColor";
        public const string UiAltColor = "UiAltColor";
        public const string ShipStuffColor = "ShipStuffColor";
        public const string CameraColor = "CameraColor";
        public const string DefenceColor = "DefenceColor";
        public const string OffenceColor = "OffenceColor";
        public const string MovementSpeedColor = "MovementSpeedColor";
        public const string MovementSecondaryColor = "MovementSecondaryColor";
        public const string WingColor = "WingColor";
        public const string NavigationColor = "NavigationColor";
        public const string ModeEnableColor = "ModeEnableColor";
        public const string ModeDisableColor = "ModeDisableColor";
    }

    public class
        EliteDangerousKeyBindsHandlerProperties : LayerHandlerProperties2Color<EliteDangerousKeyBindsHandlerProperties>
    {
        public Color? _OtherColor { get; set; }

        public Color OtherColor
        {
            get { return Logic._OtherColor ?? _OtherColor ?? Color.Empty; }
        }

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

        public Color? _OffenceColor { get; set; }

        public Color OffenceColor
        {
            get { return Logic._OffenceColor ?? _OffenceColor ?? Color.Empty; }
        }

        public Color? _MovementSpeedColor { get; set; }

        public Color MovementSpeedColor
        {
            get { return Logic._MovementSpeedColor ?? _MovementSpeedColor ?? Color.Empty; }
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
            this._OtherColor = Color.FromArgb(255, 255, 255);
            this._HudModeCombatColor = Color.FromArgb(255, 80, 0);
            this._HudModeDiscoveryColor = Color.FromArgb(0, 160, 255);
            this._UiColor = Color.FromArgb(255, 80, 0);
            this._UiAltColor = Color.FromArgb(255, 115, 70);
            this._ShipStuffColor = Color.FromArgb(0, 255, 0);
            this._CameraColor = Color.FromArgb(71, 164, 79);
            this._DefenceColor = Color.FromArgb(0, 220, 255);
            this._OffenceColor = Color.FromArgb(255, 0, 0);
            this._MovementSpeedColor = Color.FromArgb(136, 0, 255);
            this._MovementSecondaryColor = Color.FromArgb(255, 0, 255);
            this._WingColor = Color.FromArgb(0, 0, 255);
            this._NavigationColor = Color.FromArgb(255, 220, 0);
            this._ModeEnableColor = Color.FromArgb(153, 167, 255);
            this._ModeDisableColor = Color.FromArgb(61, 88, 156);
        }

        public Color GetColorByGroupName(string colorVariableName)
        {
            switch (@colorVariableName)
            {
                case ColorGroup.HudModeCombatColor: return HudModeCombatColor;
                case ColorGroup.HudModeDiscoveryColor: return HudModeDiscoveryColor;
                case ColorGroup.UiColor: return UiColor;
                case ColorGroup.UiAltColor: return UiAltColor;
                case ColorGroup.ShipStuffColor: return ShipStuffColor;
                case ColorGroup.CameraColor: return CameraColor;
                case ColorGroup.DefenceColor: return DefenceColor;
                case ColorGroup.OffenceColor: return OffenceColor;
                case ColorGroup.MovementSpeedColor: return MovementSpeedColor;
                case ColorGroup.MovementSecondaryColor: return MovementSecondaryColor;
                case ColorGroup.WingColor: return WingColor;
                case ColorGroup.NavigationColor: return NavigationColor;
                case ColorGroup.ModeEnableColor: return ModeEnableColor;
                case ColorGroup.ModeDisableColor: return ModeDisableColor;
                case ColorGroup.None: return Color.FromArgb(0, 0, 0, 0);
            }

            return OtherColor;
        }
    }

    public class EliteDangerousKeyBindsLayerHandler : LayerHandler<EliteDangerousKeyBindsHandlerProperties>
    {
        private class KeyBlendState
        {
            public Color colorFrom = Color.Empty;
            public Color colorTo = Color.Empty;
            public double transitionProgress = 0;

            public KeyBlendState(Color colorFrom, Color colorTo)
            {
                this.colorFrom = colorFrom;
                this.colorTo = colorTo;
            }

            public bool Finished()
            {
                return transitionProgress >= 1;
            }

            public bool Increment()
            {
                transitionProgress = Math.Min(1, transitionProgress + 0.08);

                return Finished();
            }

            public Color GetBlendedColor()
            {
                return ColorUtils.BlendColors(colorFrom, colorTo, transitionProgress);
            }

            public bool Equals(KeyBlendState blendState)
            {
                return colorFrom.Equals(blendState.colorFrom) && colorTo.Equals(blendState.colorTo);
            }
        }

        private ControlGroupSet[] controlGroupSets =
        {
            ControlGroupSets.CONTROLS_MAIN,
            ControlGroupSets.CONTROLS_SHIP,
            ControlGroupSets.CONTROLS_SRV,
            ControlGroupSets.CONTROLS_SYSTEM_MAP,
            ControlGroupSets.CONTROLS_GALAXY_MAP,
            ControlGroupSets.CONTROLS_FSS,
            ControlGroupSets.CONTROLS_ADS,
            ControlGroupSets.UI_PANELS,
            ControlGroupSets.UI_PANEL_TABS,
        };

        protected override UserControl CreateControl()
        {
            return new Control_EliteDangerousKeyBindsLayer(this);
        }

        private Dictionary<DeviceKeys, Color> currentKeyColors = new Dictionary<DeviceKeys, Color>();
        private Dictionary<DeviceKeys, KeyBlendState> keyBlendStates = new Dictionary<DeviceKeys, KeyBlendState>();
        private void SetKey(EffectLayer layer, DeviceKeys key, Color color)
        {
            if (keyBlendStates.ContainsKey(key))
            {
                keyBlendStates.Remove(key);
            }
            currentKeyColors[key] = color;
            layer.Set(key, color);
        }
        
        private void SetKeySmooth(EffectLayer layer, DeviceKeys key, Color color)
        {
            if (!currentKeyColors.ContainsKey(key))
            {
                currentKeyColors[key] = Color.Empty;
            }

            KeyBlendState blendState = new KeyBlendState(currentKeyColors[key], color);
            if (keyBlendStates.ContainsKey(key))
            {
                if (!keyBlendStates[key].colorTo.Equals(color))
                {
                    blendState.colorFrom = keyBlendStates[key].GetBlendedColor();
                    keyBlendStates[key] = blendState;
                }
            }
            else
            {
                keyBlendStates[key] = blendState;
            }

            keyBlendStates[key].Increment();
            if (keyBlendStates[key].Finished())
            {
                SetKey(layer, key, color);
            }
            else
            {
                layer.Set(key, keyBlendStates[key].GetBlendedColor());
            }
        }

        public override EffectLayer Render(IGameState state)
        {
            GameState_EliteDangerous gameState = state as GameState_EliteDangerous;
            GSI.Nodes.Controls controls = (state as GameState_EliteDangerous).Controls;

            EffectLayer keyBindsLayer = new EffectLayer("Elite: Dangerous - Key Binds");
            HashSet<DeviceKeys> leftoverBlendStates = new HashSet<DeviceKeys>(keyBlendStates.Keys);

            long currentTime = Utils.Time.GetMillisecondsSinceEpoch();

            if (gameState.Journal.fsdWaitingCooldown && gameState.Status.IsFlagSet(Flag.FSD_COOLDOWN))
            {
                gameState.Journal.fsdWaitingCooldown = false;
            }

            if (gameState.Journal.fsdWaitingSupercruise && gameState.Status.IsFlagSet(Flag.SUPERCRUISE))
            {
                gameState.Journal.fsdWaitingSupercruise = false;
            }

            Color newKeyColor;
            Dictionary<DeviceKeys, Color> smoothColorSets = new Dictionary<DeviceKeys, Color>();
            foreach (ControlGroupSet controlGroupSet in controlGroupSets)
            {
                if (!controlGroupSet.IsSatisfied(gameState)) continue;

                foreach (ControlGroup controlGroup in controlGroupSet.controlGroups)
                {
                    if (!controlGroup.IsSatisfied(gameState)) continue;

                    foreach (string command in controlGroup.commands)
                    {
                        if (!controls.commandToBind.ContainsKey(command)) continue;

                        bool keyWithEffect = KeyPresets.KEY_EFFECTS.ContainsKey(command);

                        foreach (Bind.Mapping mapping in controls.commandToBind[command].mappings)
                        {
                            bool allModifiersPressed = true;
                            foreach (DeviceKeys modifierKey in mapping.modifiers)
                            {
                                SetKey(keyBindsLayer, modifierKey, Properties.ShipStuffColor);
                                leftoverBlendStates.Remove(modifierKey);
                                if (Array.IndexOf(
                                        Global.InputEvents.PressedKeys,
                                        KeyUtils.GetFormsKey(modifierKey)
                                    ) == -1)
                                {
                                    allModifiersPressed = false;
                                    break;
                                }
                            }

                            if (!allModifiersPressed) continue;

                            newKeyColor = Properties.GetColorByGroupName(
                                controlGroup.colorGroupName ?? CommandColors.GetColorGroupForCommand(command)
                            );

                            if (keyWithEffect)
                            {
                                SetKey(keyBindsLayer, mapping.key, KeyPresets.KEY_EFFECTS[command](newKeyColor, gameState, currentTime));
                            }
                            else
                            {
                                smoothColorSets[mapping.key] = newKeyColor;
                            }

                            leftoverBlendStates.Remove(mapping.key);
                        }
                    }
                }
            }

            //Apply smooth transitions for keys
            foreach (KeyValuePair<DeviceKeys, Color> smoothKey in smoothColorSets)
            {
                SetKeySmooth(keyBindsLayer, smoothKey.Key, smoothKey.Value);
            }            
            
            //Fade out inactive keys
            foreach (DeviceKeys key in leftoverBlendStates)
            {
                SetKeySmooth(keyBindsLayer, key, Color.Empty);
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