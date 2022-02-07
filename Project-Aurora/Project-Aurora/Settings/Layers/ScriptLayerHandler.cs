﻿﻿using System;
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
using Aurora.Utils;

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
        internal Application profileManager;

        public IEffectScript ScriptInstance { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public Exception ScriptException { get; private set; }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer layer = null;

            if (this.IsScriptValid)
            {
                try
                {
                    object script_layers = ScriptInstance.UpdateLights(Properties.ScriptProperties, gamestate);
                    if (script_layers is EffectLayer)
                        layer = (EffectLayer)script_layers;
                    else if (script_layers is EffectLayer[])
                    {
                        EffectLayer[] layers = (EffectLayer[])script_layers;
                        layer = layers.First();
                        for (int i = 1; i < layers.Length; i++)
                            layer = layer + layers[i];
                        //foreach (var layer in (script_layers as EffectLayer[]))
                        //  layers.Enqueue(layer);
                    }
                    ScriptException = null;
                }
                catch(Exception exc)
                {
                    Global.logger.Error("Effect script with key {0} encountered an error. Exception: {1}", this.Properties.Script, exc);
                    ScriptException = exc;
                }
            }

            return layer ?? new EffectLayer();
        }

        public VariableRegistry GetScriptPropertyRegistry()
        {
            if (IsScriptValid) {
                var tempScriptInstance = profileManager.EffectScripts[this.Properties._Script].Invoke();
                return (VariableRegistry) tempScriptInstance.Properties.Clone();
            }

            return null;
        }

        public void OnScriptChanged() {
            VariableRegistry varRegistry = GetScriptPropertyRegistry();
            if (varRegistry != null) {
                Properties.ScriptProperties.Combine(varRegistry, true);
                
                // Create a new instance of the script and save it
                ScriptInstance ??= profileManager.EffectScripts[this.Properties._Script].Invoke();
            }
        }

        [JsonIgnore]
        public bool IsScriptValid { get { return profileManager?.EffectScripts?.ContainsKey(Properties.Script) ?? false; } }

        public override void SetApplication(Application profile)
        {
            profileManager = profile;
            (_Control as Control_ScriptLayer)?.SetProfile(profile);
            this.OnScriptChanged();
        }

        protected override UserControl CreateControl()
        {
            return new Control_ScriptLayer(this);
        }
    }
}
