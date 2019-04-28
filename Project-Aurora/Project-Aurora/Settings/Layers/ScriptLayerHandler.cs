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

    public class ScriptLayerHandler : LayerHandler<ScriptLayerHandlerProperties>, INotifyPropertyChanged
    {
        internal Application profileManager;

        public event PropertyChangedEventHandler PropertyChanged;

        private Exception scriptException = null;

        [JsonIgnore]
        public Exception ScriptException { get { return scriptException; }
            private set
            {
                bool diff = !(scriptException?.Equals(value) ?? false);
                scriptException = value;
                if (diff)
                    InvokePropertyChanged();
            }
        }

        public ScriptLayerHandler() : base()
        {
            _ID = "Script";
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer layer = null;

            if (this.IsScriptValid)
            {
                try
                {
                    IEffectScript script = this.profileManager.EffectScripts[this.Properties.Script];
                    object script_layers = script.UpdateLights(Properties.ScriptProperties, gamestate);
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
            if (IsScriptValid)
            {
                return (VariableRegistry)profileManager.EffectScripts[this.Properties._Script].Properties.Clone();
            }

            return null;
        }

        public void OnScriptChanged()
        {
            VariableRegistry varRegistry = GetScriptPropertyRegistry();
            if (varRegistry != null)
                Properties.ScriptProperties.Combine(varRegistry, true);
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

        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
