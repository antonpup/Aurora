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
        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_ShortcutAssistantLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer sc_assistant_layer = new EffectLayer("Shortcut Assistant");

            Keys[] heldKeys = Global.InputEvents.PressedKeys;

            Tree<Keys> _childKeys = Properties.ShortcutKeysTree;
            foreach (var key in heldKeys)
            {
                if(_childKeys != null)
                    _childKeys = _childKeys.ContainsItem(Properties.MergeModifierKey ? KeyUtils.GetStandardKey(key) : key);
            }

            if(_childKeys != null && _childKeys.Item != Keys.None)
            {
                Keys[] shortcutKeys;

                if (Properties.PresentationType == ShortcutAssistantPresentationType.ProgressiveSuggestion)
                    shortcutKeys = _childKeys.GetChildren();
                else
                    shortcutKeys = _childKeys.GetAllChildren();

                if(shortcutKeys.Length > 0)
                {

                    Devices.DeviceKeys[] selectedKeys = Utils.KeyUtils.GetDeviceKeys(shortcutKeys, true, !Console.NumberLock)
                        .Concat(Utils.KeyUtils.GetDeviceKeys(heldKeys, true)).ToArray();

                    if (Properties.DimBackground)
                    {
                        Devices.DeviceKeys[] backgroundKeys = Utils.KeyUtils.GetDeviceAllKeys().Except(selectedKeys).ToArray();
                        sc_assistant_layer.Set(backgroundKeys, Properties.DimColor);
                        //sc_assistant_layer.Fill(Properties.DimColor);
                    }

                    sc_assistant_layer.Set(selectedKeys, Properties.PrimaryColor);
                   
                }
            }

            return sc_assistant_layer;
        }
    }
}
