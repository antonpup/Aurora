using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aurora.Settings.Overrides;

namespace Aurora.Settings.Layers
{
    public class ScriptLayerHandlerProperties : LayerHandlerProperties<ScriptLayerHandlerProperties>
    {
        public string _Script { get; set; } = null;

        [JsonIgnore]
        public string Script { get { return Logic._Script ?? _Script ?? String.Empty; } }

        /*public ScriptSettings _ScriptSettings { get; set; } = null;

        [JsonIgnore]
        public ScriptSettings ScriptSettings { get { return Logic._ScriptSettings ?? _ScriptSettings; } }*/

        public VariableRegistry _ScriptProperties { get; set; } = null;

        [JsonIgnore]
        public VariableRegistry ScriptProperties { get { return Logic._ScriptProperties ?? _ScriptProperties; } }

        public ScriptLayerHandlerProperties() : base() { }

        public ScriptLayerHandlerProperties(bool empty = false) : base(empty) { }

        public override void Default()
        {
            base.Default();
            _ScriptProperties = new VariableRegistry();
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_Sequence")]
    public class ScriptLayerHandler : LayerHandler<ScriptLayerHandlerProperties>, INotifyPropertyChanged
    {
        internal Application ProfileManager;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public Exception ScriptException { get; private set; }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer layer = null;

            if (!IsScriptValid) return EffectLayer.EmptyLayer;
            try
            {
                var script = ProfileManager.EffectScripts[Properties.Script];
                var scriptLayers = script.UpdateLights(Properties.ScriptProperties, gamestate);
                switch (scriptLayers)
                {
                    case EffectLayer layers:
                        layer = layers;
                        break;
                    case EffectLayer[] effectLayers:
                    {
                        layer = effectLayers.First();
                        for (var i = 1; i < effectLayers.Length; i++)
                            layer += effectLayers[i];
                        //foreach (var layer in (script_layers as EffectLayer[]))
                        //  layers.Enqueue(layer);
                        break;
                    }
                }
                ScriptException = null;
            }
            catch(Exception exc)
            {
                Global.logger.Error($"Effect script with key {Properties.Script} encountered an error", exc);
                ScriptException = exc;
            }

            return layer ?? EffectLayer.EmptyLayer;
        }

        public VariableRegistry GetScriptPropertyRegistry()
        {
            if (IsScriptValid)
            {
                return (VariableRegistry)ProfileManager.EffectScripts[Properties._Script].Properties.Clone();
            }

            return null;
        }

        public void OnScriptChanged()
        {
            var varRegistry = GetScriptPropertyRegistry();
            if (varRegistry != null)
                Properties.ScriptProperties.Combine(varRegistry, true);
        }

        [JsonIgnore]
        public bool IsScriptValid => ProfileManager?.EffectScripts?.ContainsKey(Properties.Script) ?? false;

        public override void SetApplication(Application profile)
        {
            ProfileManager = profile;
            (_Control as Control_ScriptLayer)?.SetProfile(profile);
            OnScriptChanged();
        }

        protected override UserControl CreateControl()
        {
            return new Control_ScriptLayer(this);
        }
    }
}
