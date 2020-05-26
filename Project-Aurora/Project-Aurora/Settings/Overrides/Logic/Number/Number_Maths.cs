using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Aurora.Profiles;
using Aurora.Utils;
using Newtonsoft.Json;
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Evaluatable that performs a binary mathematical operation on two operands.
    /// </summary>
    [Evaluatable("Arithmetic Operation", category: EvaluatableCategory.Maths)]
    public class NumberMathsOperation : Evaluatable<double> {

        /// <summary>Creates a new maths operation that has no values pre-set.</summary>
        public NumberMathsOperation() { }
        /// <summary>Creates a new evaluatable that returns the result of the two given numbers added together.</summary>
        public NumberMathsOperation(double value1, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); }
        /// <summary>Creates a new evaluatable that returns the result of the two given numbers with the given operator.</summary>
        public NumberMathsOperation(double value1, MathsOperator op, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); Operator = op; }
        /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number added together.</summary>
        public NumberMathsOperation(Evaluatable<double> eval, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); }
        /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number with the given operator.</summary>
        public NumberMathsOperation(Evaluatable<double> eval, MathsOperator op, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); Operator = op; }
        /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables added together.</summary>
        public NumberMathsOperation(Evaluatable<double> eval1, Evaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; }
        /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables with the given operator.</summary>
        public NumberMathsOperation(Evaluatable<double> eval1, MathsOperator op, Evaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; Operator = op; }

        // The operands and the operator
        public Evaluatable<double> Operand1 { get; set; } = new NumberConstant();
        public Evaluatable<double> Operand2 { get; set; } = new NumberConstant();
        public MathsOperator Operator { get; set; } = MathsOperator.Add;
        
        public override Visual GetControl() => new Control_BinaryOperationHolder(typeof(double), typeof(MathsOperator))
            .WithBinding(Control_BinaryOperationHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Resolves the two operands and then compares them using the user specified operator</summary>
        protected override double Execute(IGameState gameState) {
            var op1 = Operand1.Evaluate(gameState);
            var op2 = Operand2.Evaluate(gameState);
            switch (Operator) {
                case MathsOperator.Add: return op1 + op2;
                case MathsOperator.Sub: return op1 - op2;
                case MathsOperator.Mul: return op1 * op2;
                case MathsOperator.Div when op2 != 0: return op1 / op2; // Return 0 if user tried to divide by zero.
                case MathsOperator.Mod when op2 != 0: return op1 % op2;
                default: return 0;
            }
        }
        
        /// <summary>Creates a copy of this maths operation.</summary>
        public override Evaluatable<double> Clone() => new NumberMathsOperation { Operand1 = Operand1.Clone(), Operand2 = Operand2.Clone(), Operator = Operator };
    }


    
    /// <summary>
    /// Returns the absolute value of the given evaluatable.
    /// </summary>
    [Evaluatable("Absolute", category: EvaluatableCategory.Maths)]
    public class NumberAbsValue : Evaluatable<double> {

        /// <summary>Creates a new absolute operation with the default operand.</summary>
        public NumberAbsValue() { }
        /// <summary>Creates a new absolute evaluatable with the given operand.</summary>
        public NumberAbsValue(Evaluatable<double> op) { Operand = op; }

        /// <summary>The operand to absolute.</summary>
        public Evaluatable<double> Operand { get; set; } = new NumberConstant();

        // Get the control allowing the user to set the operand
        public override Visual GetControl() => new Control_NumericUnaryOpHolder("Absolute")
            .WithBinding(Control_NumericUnaryOpHolder.OperandProperty, new Binding("Operand") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Evaluate the operand and return the absolute value of it.</summary>
        protected override double Execute(IGameState gameState) => Math.Abs(Operand.Evaluate(gameState));
        
        public override Evaluatable<double> Clone() => new NumberAbsValue { Operand = Operand.Clone() };
    }



    /// <summary>
    /// Evaluatable that compares two numerical evaluatables and returns a boolean depending on the comparison.
    /// </summary>
    [Evaluatable("Arithmetic Comparison", category: EvaluatableCategory.Maths)]
    public class BooleanMathsComparison : Evaluatable<bool> {

        /// <summary>Creates a new maths comparison that has no values pre-set.</summary>
        public BooleanMathsComparison() { }
        /// <summary>Creates a new evaluatable that returns whether or not the two given numbers are equal.</summary>
        public BooleanMathsComparison(double value1, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); }
        /// <summary>Creates a new evaluatable that returns the result of the two given numbers compared using the given operator.</summary>
        public BooleanMathsComparison(double value1, ComparisonOperator op, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); Operator = op; }
        /// <summary>Creates a new evaluatable that returns whether or not the given evaluatable and given number are equal.</summary>
        public BooleanMathsComparison(Evaluatable<double> eval, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); }
        /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number when compared using the given operator.</summary>
        public BooleanMathsComparison(Evaluatable<double> eval, ComparisonOperator op, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); Operator = op; }
        /// <summary>Creates a new evaluatable that returns the whether or not the two given evaluatables are equal.</summary>
        public BooleanMathsComparison(Evaluatable<double> eval1, Evaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; }
        /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables when compared using the given operator.</summary>
        public BooleanMathsComparison(Evaluatable<double> eval1, ComparisonOperator op, Evaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; Operator = op; }

        // The operands and the operator
        public Evaluatable<double> Operand1 { get; set; } = new NumberConstant();
        public Evaluatable<double> Operand2 { get; set; } = new NumberConstant();
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.EQ;

        // The control allowing the user to edit the evaluatable
        public override Visual GetControl() => new Control_BinaryOperationHolder(typeof(double), typeof(ComparisonOperator))
            .WithBinding(Control_BinaryOperationHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Resolves the two operands and then compares them with the user-specified operator.</summary>
        protected override bool Execute(IGameState gameState) {
            var op1 = Operand1.Evaluate(gameState);
            var op2 = Operand2.Evaluate(gameState);
            switch (Operator) {
                case ComparisonOperator.EQ: return op1 == op2;
                case ComparisonOperator.NEQ: return op1 != op2;
                case ComparisonOperator.GT: return op1 > op2;
                case ComparisonOperator.GTE: return op1 >= op2;
                case ComparisonOperator.LT: return op1 < op2;
                case ComparisonOperator.LTE: return op1 <= op2;
                default: return false;
            }
        }

        /// <summary>Creates a copy of this mathematical comparison.</summary>
        public override Evaluatable<bool> Clone() => new BooleanMathsComparison { Operand1 = Operand1.Clone(), Operand2 = Operand2.Clone(), Operator = Operator };
    }



    /// <summary>
    /// Evaluatable that takes a number in a given range and linearly interpolates it onto another range.
    /// </summary>
    [Evaluatable("Lerp", category: EvaluatableCategory.Maths)]
    public class NumberMap : Evaluatable<double> {

        /// <summary>Creates a new numeric map with the default constant parameters.</summary>
        public NumberMap() { }
        /// <summary>Creates a new numeric map to map the given value with the given constant range onto the range 0 → 1.</summary>
        public NumberMap(Evaluatable<double> value, double fromMin, double fromMax) : this(value, new NumberConstant(fromMin), new NumberConstant(fromMax)) { }
        /// <summary>Creates a new numeric map to map the given value with the given dynamic range onto the range 0 → 1.</summary>
        public NumberMap(Evaluatable<double> value, Evaluatable<double> fromMin, Evaluatable<double> fromMax) { Value = value; FromMin = fromMin; FromMax = fromMax; }
        /// <summary>Creates a new numeric map to map the given value with the given constant from range onto the given constant to range.</summary>
        public NumberMap(Evaluatable<double> value, double fromMin, double fromMax, double toMin, double toMax) : this(value, new NumberConstant(fromMin), new NumberConstant(fromMax), new NumberConstant(toMin), new NumberConstant(toMax)) { }
        /// <summary>Creates a new numeric map to map the given value with the given dynamic from range onto the given constant to range.</summary>
        public NumberMap(Evaluatable<double> value, Evaluatable<double> fromMin, Evaluatable<double> fromMax, double toMin, double toMax) : this(value, fromMin, fromMax, new NumberConstant(toMin), new NumberConstant(toMax)) { }
        /// <summary>Creates a new numeric map to map the given value with the given dynamic from range onto the given dynamic to range.</summary>
        public NumberMap(Evaluatable<double> value, Evaluatable<double> fromMin, Evaluatable<double> fromMax, Evaluatable<double> toMin, Evaluatable<double> toMax) { Value = value; FromMin = fromMin; FromMax = fromMax; ToMin = toMin; ToMax = toMax; }

        // The value to run through the map
        public Evaluatable<double> Value { get; set; } = new NumberConstant(25);
        // The values representing the starting range of the map
        public Evaluatable<double> FromMin { get; set; } = new NumberConstant(0);
        public Evaluatable<double> FromMax { get; set; } = new NumberConstant(100);
        // The values representing the end range of the map
        public Evaluatable<double> ToMin { get; set; } = new NumberConstant(0);
        public Evaluatable<double> ToMax { get; set; } = new NumberConstant(1);

        // The control to edit the map parameters
        public override Visual GetControl() => new Control_NumericMap(this);

        /// <summary>Evaluate the from range and to range and return the value in the new range.</summary>
        protected override double Execute(IGameState gameState) {
            // Evaluate all components
            double value = Value.Evaluate(gameState);
            double fromMin = FromMin.Evaluate(gameState), fromMax = FromMax.Evaluate(gameState);
            double toMin = ToMin.Evaluate(gameState), toMax = ToMax.Evaluate(gameState);

            // Perform actual equation
            return MathUtils.Clamp((value - fromMin) * ((toMax - toMin) / (fromMax - fromMin)) + toMin, Math.Min(toMin, toMax), Math.Max(toMin, toMax));
            // Here is an example of it running: https://www.desmos.com/calculator/nzbiiz7vxv
        }

        public override Evaluatable<double> Clone() => new NumberMap {
            Value = Value.Clone(),
            FromMin = FromMin.Clone(), FromMax = FromMax.Clone(),
            ToMin = ToMin.Clone(), ToMax = ToMax.Clone()
        };
    }



    /// <summary>
    /// Evaluatable that resolves to a numerical constant.
    /// </summary>
    [Evaluatable("Number Constant", category: EvaluatableCategory.Maths)]
    public class NumberConstant : Evaluatable<double> {

        /// <summary>Creates a new constant with the zero as the constant value.</summary>
        public NumberConstant() { }
        /// <summary>Creats a new constant with the given value as the constant value.</summary>
        public NumberConstant(double value) { Value = value; }

        // The constant value
        public double Value { get; set; }

        // The control allowing the user to edit the number value
        public override Visual GetControl() => new DoubleUpDown { VerticalAlignment = System.Windows.VerticalAlignment.Center }
            .WithBinding(DoubleUpDown.ValueProperty, new Binding("Value") { Source = this });

        /// <summary>Simply returns the constant value specified by the user</summary>
        protected override double Execute(IGameState gameState) => Value;

        public override Evaluatable<double> Clone() => new NumberConstant { Value = Value };
    }
}
