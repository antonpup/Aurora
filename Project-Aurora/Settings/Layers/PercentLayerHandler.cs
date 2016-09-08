using Aurora.EffectsEngine;
using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Layers
{
    public class PercentLayerHandler : LayerHandler
    {
        public string VariablePath;
        public string MaxVariablePath;
        public Color ProgressColor;
        public Color BackgroundColor;
        public PercentEffectType PercentType;
        public double BlinkThreshold;
        public bool BlinkDirection;


        public PercentLayerHandler()
        {
            _Control = new Control_PercentLayer(this);

            _Type = LayerType.Percent;
        }

        public override EffectLayer Render(GameState state)
        {
            EffectLayer percent_layer = new EffectLayer();

            double value = 0;
            if (!double.TryParse(VariablePath, out value) && !string.IsNullOrWhiteSpace(VariablePath))
            {
                try
                {
                    value = Convert.ToDouble(Utils.GameStateUtils.RetrieveGameStateParameter(state, VariablePath));
                }
                catch(Exception exc)
                {
                    value = 0;
                }
            }
                

            double maxvalue = 0;
            if (!double.TryParse(MaxVariablePath, out maxvalue) && !string.IsNullOrWhiteSpace(MaxVariablePath))
            {
                try
                {
                    maxvalue = Convert.ToDouble(Utils.GameStateUtils.RetrieveGameStateParameter(state, MaxVariablePath));
                }
                catch (Exception exc)
                {
                    maxvalue = 0;
                }
            }

            percent_layer.PercentEffect(ProgressColor, BackgroundColor, AffectedSequence, value, maxvalue, PercentType, BlinkThreshold, BlinkDirection);

            return percent_layer;
        }

        public override void SetProfile(ProfileManager profile)
        {
            (_Control as Control_PercentLayer).SetProfile(profile);
        }
    }
}
