using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;
using Aurora.Utils;
using System.ComponentModel;

namespace Aurora.Settings.Layers
{
    public enum ShortcutAssistantPresentationType
    {
        [Description("Show All Keys")]
        Default = 0,

        [Description("Progressive Suggestion")]
        ProgressiveSuggestion = 1
    }

    public class ShortcutAssistantLayerHandlerProperties : LayerHandlerProperties<ShortcutAssistantLayerHandlerProperties>
    {
        [Overrides.LogicOverridable("Dim Background")]
        public bool? _DimBackground { get; set; }

        [JsonIgnore]
        public bool DimBackground { get { return (Logic._DimBackground ?? _DimBackground) ?? false; } }

        [JsonIgnore]
        public bool? __MergeModifierKey { get; set; }

        public bool? _MergeModifierKey { get { return __MergeModifierKey; } set { __MergeModifierKey = value; ShortcutKeysInvalidated = true; } }

        [JsonIgnore]
        public bool MergeModifierKey { get { return (Logic._MergeModifierKey ?? _MergeModifierKey) ?? default(bool); } }

        [JsonIgnore]
        public bool? __LeafShortcutAlwaysOn { get; set; }

        public bool? _LeafShortcutAlwaysOn { get { return __LeafShortcutAlwaysOn; } set { __LeafShortcutAlwaysOn = value; ShortcutKeysInvalidated = true; } }

        [JsonIgnore]
        public bool LeafShortcutAlwaysOn { get { return (Logic._LeafShortcutAlwaysOn ?? _LeafShortcutAlwaysOn) ?? default(bool); } }

        [Overrides.LogicOverridable("Dim Color")]
        public Color? _DimColor { get; set; }

        [JsonIgnore]
        public Color DimColor { get { return (Logic._DimColor ?? _DimColor) ?? Color.Empty; } }

        [JsonIgnore]
        private Keybind[] __ShortcutKeys = new Keybind[] { };

        public Keybind[] _ShortcutKeys
        {
            get { return __ShortcutKeys; }
            set { __ShortcutKeys = value; ShortcutKeysInvalidated = true; }
        }

        [JsonIgnore]
        public Keybind[] ShortcutKeys { get { return _ShortcutKeys; } }

        [JsonIgnore]
        public bool ShortcutKeysInvalidated = true;

        [JsonIgnore]
        private Tree<Keys> _ShortcutKeysTree = new Tree<Keys>(Keys.None);

        [JsonIgnore]
        public Tree<Keys> ShortcutKeysTree
        {
            get
            {
                if (ShortcutKeysInvalidated)
                {
                    _ShortcutKeysTree = new Tree<Keys>(Keys.None);

                    foreach (Keybind keyb in ShortcutKeys)
                    {
                        Keys[] keys = keyb.ToArray();

                        if (MergeModifierKey)
                        {
                            for (int i = 0; i < keys.Length; i++)
                                keys[i] = KeyUtils.GetStandardKey(keys[i]);
                        }

                        _ShortcutKeysTree.AddBranch(keys);
                    }

                    ShortcutKeysInvalidated = false;
                }

                return _ShortcutKeysTree;
            }
        }

        public ShortcutAssistantPresentationType? _PresentationType { get; set; }

        [JsonIgnore]
        public ShortcutAssistantPresentationType PresentationType { get { return Logic._PresentationType ?? _PresentationType ?? ShortcutAssistantPresentationType.Default; } }

        public ShortcutAssistantLayerHandlerProperties() : base() { }

        public ShortcutAssistantLayerHandlerProperties(bool empty = false) : base(empty) { }

        public override void Default()
        {
            base.Default();
            _DimBackground = true;
            _DimColor = Color.FromArgb(169, 0, 0, 0);
            _PresentationType = ShortcutAssistantPresentationType.Default;
            _MergeModifierKey = false;
        }
    }

    public class ShortcutAssistantLayerHandler : LayerHandler<ShortcutAssistantLayerHandlerProperties>
    {

        public ShortcutAssistantLayerHandler() : base("Shortcut Assistant")
        {
        }

        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_ShortcutAssistantLayer(this);
        }

        /// <summary>
        /// Check if layer effect is active based on what keys are pressed. 
        /// Layer is considered active if the first pressed key is a child of the root of Properties.ShortcutKeysTree
        /// </summary>
        /// <returns>true if layer is active</returns>
        protected bool IsLayerActive()
        {
            Keys[] heldKeys = Global.InputEvents.PressedKeys;

            if (heldKeys.Length > 0)
            {
                Keys keyToCheck = heldKeys.First();

                if (Properties.ShortcutKeysTree.ContainsItem(Properties.MergeModifierKey ? KeyUtils.GetStandardKey(keyToCheck) : keyToCheck) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public Keys[] MatchHeldKeysToShortcutTree(Keys[] heldKeys, Tree<Keys> shortcuts)
        {
            Tree<Keys> currentShortcutNode = shortcuts;
            Keys[] heldKeysToHighlight = { };

            foreach (var key in heldKeys)
            {
                Tree<Keys> child = currentShortcutNode.ContainsItem(key);
                if (child != null)
                {
                    currentShortcutNode = child;
                    heldKeysToHighlight = heldKeysToHighlight.Append(key).ToArray();
                }
                else
                {
                    break;
                }
            }

            return heldKeysToHighlight;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            if (IsLayerActive() == false)
            {
                return EffectLayer.EmptyLayer;
            }

            // The layer is active. At this point we have at least 1 key to highlight

            Keys[] heldKeys = Global.InputEvents.PressedKeys;
            Keys[] heldKeysToHighlight = MatchHeldKeysToShortcutTree(heldKeys, Properties.ShortcutKeysTree); // This is also the path in shortcut tree

            Tree<Keys> currentShortcutNode = Properties.ShortcutKeysTree.GetNodeByPath(heldKeysToHighlight);
            if (Properties.LeafShortcutAlwaysOn && currentShortcutNode.IsLeaf)
            {
                // Go down one level
                currentShortcutNode = Properties.ShortcutKeysTree.GetNodeByPath(heldKeysToHighlight.Take(heldKeysToHighlight.Length - 1).ToArray());
            }

            // Convert to DeviceKeys
            Devices.DeviceKeys[] selectedKeys = BuildSelectedKeys(currentShortcutNode, heldKeysToHighlight);
            Devices.DeviceKeys[] backgroundKeys = KeyUtils.GetDeviceAllKeys().Except(selectedKeys).ToArray();

            // Display keys
            ApplyKeyColors(EffectLayer, selectedKeys, backgroundKeys);

            return EffectLayer;
        }

        protected Devices.DeviceKeys[] BuildSelectedKeys(Tree<Keys> currentShortcutNode, Keys[] previousShortcutKeys)
        {
            Keys[] nextPossibleShortcutKeys;
            switch (Properties.PresentationType)
            {
                default:
                case ShortcutAssistantPresentationType.Default:
                    nextPossibleShortcutKeys = currentShortcutNode.GetAllChildren();
                    break;
                case ShortcutAssistantPresentationType.ProgressiveSuggestion:
                    nextPossibleShortcutKeys = currentShortcutNode.GetChildren();
                    break;
            }

            return Utils.KeyUtils.GetDeviceKeys(nextPossibleShortcutKeys, true, !Console.NumberLock)
                        .Concat(Utils.KeyUtils.GetDeviceKeys(previousShortcutKeys, true)).ToArray();
        }

        protected void ApplyKeyColors(EffectLayer layer, Devices.DeviceKeys[] selectedKeys, Devices.DeviceKeys[] backgroundKeys)
        {
            if (backgroundKeys != null && Properties.DimBackground)
            {
                layer.Set(backgroundKeys, Properties.DimColor);
            }

            if (selectedKeys != null)
            {
                layer.Set(selectedKeys, Properties.PrimaryColor);
            }
        }
    }
}
