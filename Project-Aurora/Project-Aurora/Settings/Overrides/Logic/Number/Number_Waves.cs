using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// 
    /// </summary>
    [OverrideLogic("Wave Function", category: OverrideLogicCategory.Maths)]
    public class NumberWaveFunction : IEvaluatableNumber {

        public IEvaluatableNumber Operand { get; set; } = new NumberConstant();
        public WaveFunctionType WaveFunc { get; set; } = WaveFunctionType.Sine;

        [JsonIgnore]
        private Control_NumericUnaryOpHolder control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new Control_NumericUnaryOpHolder(application, typeof(WaveFunctionType));
                control.SetBinding(Control_NumericUnaryOpHolder.OperandProperty, new Binding("Operand") { Source = this, Mode = BindingMode.TwoWay });
                control.SetBinding(Control_NumericUnaryOpHolder.SelectedOperatorProperty, new Binding("WaveFunc") { Source = this, Mode = BindingMode.TwoWay });
            }
            return control;
        }

        public double Evaluate(IGameState gameState) {
            var op = Operand.Evaluate(gameState);
            switch (WaveFunc) {
                case WaveFunctionType.Sine: return .5 * (Math.Sin(op * 2 * Math.PI) + 1); // Convert the function to take a value from 0-1 and return 0-1. This saves the user from doing complex radian conversion
                default: return 0;
            }
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            control?.SetApplication(application);
            Operand?.SetApplication(application);
        }

        public IEvaluatableNumber Clone() => new NumberWaveFunction { Operand = Operand.Clone() };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    public enum WaveFunctionType {
        Sine
    }
}
