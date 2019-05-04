using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// A special operator that takes the given (x) input (between 0 and 1) and converts it to a waveform (y) between 0 and 1.
    /// </summary>
    [OverrideLogic("Wave Function", category: OverrideLogicCategory.Maths)]
    public class NumberWaveFunction : IEvaluatableNumber {

        /// <summary>Creates a new wave function evaluatable with the default parameters.</summary>
        public NumberWaveFunction() { }
        /// <summary>Creates a new wave function evaluatable with the given evaluatable and default wave type.</summary>
        public NumberWaveFunction(IEvaluatableNumber operand) { Operand = operand; }
        /// <summary>Creates a new wave function evaluatable with the given evaluatable and given wave type.</summary>
        public NumberWaveFunction(IEvaluatableNumber operand, WaveFunctionType type) { Operand = operand; WaveFunc = type; }

        /// <summary>The number that will be used as a basis (sometimes the x value) for the wave function.</summary>
        public IEvaluatableNumber Operand { get; set; } = new NumberConstant();
        /// <summary>The type of wave to generate.</summary>
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

        /// <summary>
        /// Evaluates this wave function generator using the result of the operand and the given wave type.
        /// </summary>
        public double Evaluate(IGameState gameState) {
            var op = Operand.Evaluate(gameState);
            switch (WaveFunc) {
                // The wave functions are generated on https://www.desmos.com/calculator/x9xl6m9ryf
                case WaveFunctionType.Sine: return .5 * (Math.Sin((op + .75) * 2 * Math.PI) + 1); // Convert the function to take a value from 0-1 and return 0-1. This saves the user from doing complex radian conversion
                case WaveFunctionType.Triangle: return 2 * Math.Abs(((op + .5) % 1) - .5); // Make a triangle wave that starts at 0, at 0.5 it returns 1 then at 1 returns 0.
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
        Sine,
        Triangle
    }
}
