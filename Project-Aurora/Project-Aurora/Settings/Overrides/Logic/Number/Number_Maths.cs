using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Aurora.Profiles;
using Newtonsoft.Json;
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Evaluatable that performs a binary mathematical operation on two operands.
    /// </summary>
    [OverrideLogic("Arithmetic Operation", category: OverrideLogicCategory.Maths)]
    public class NumberMathsOperation : IEvaluatableNumber {

        /// <summary>Creates a new maths operation that has no values pre-set.</summary>
        public NumberMathsOperation() { }
        /// <summary>Creates a new evaluatable that returns the result of the two given numbers added together.</summary>
        public NumberMathsOperation(double value1, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); }
        /// <summary>Creates a new evaluatable that returns the result of the two given numbers with the given operator.</summary>
        public NumberMathsOperation(double value1, MathsOperator op, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); Operator = op; }
        /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number added together.</summary>
        public NumberMathsOperation(IEvaluatableNumber eval, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); }
        /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number with the given operator.</summary>
        public NumberMathsOperation(IEvaluatableNumber eval, MathsOperator op, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); Operator = op; }
        /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables added together.</summary>
        public NumberMathsOperation(IEvaluatableNumber eval1, IEvaluatableNumber eval2) { Operand1 = eval1; Operand2 = eval2; }
        /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables with the given operator.</summary>
        public NumberMathsOperation(IEvaluatableNumber eval1, MathsOperator op, IEvaluatableNumber eval2) { Operand1 = eval1; Operand2 = eval2; Operator = op; }

        // The operands and the operator
        public IEvaluatableNumber Operand1 { get; set; } = new NumberConstant();
        public IEvaluatableNumber Operand2 { get; set; } = new NumberConstant();
        public MathsOperator Operator { get; set; } = MathsOperator.Add;
        
        // The control allowing the user to edit the evaluatable
        [JsonIgnore]
        private Control_BinaryOperationHolder control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new Control_BinaryOperationHolder(application, EvaluatableType.Number, typeof(MathsOperator));
                control.SetBinding(Control_BinaryOperationHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay });
                control.SetBinding(Control_BinaryOperationHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay });
                control.SetBinding(Control_BinaryOperationHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });
            }
            return control;
        }

        /// <summary>Resolves the two operands and then compares them using the user specified operator</summary>
        public double Evaluate(IGameState gameState) {
            var op1 = Operand1.Evaluate(gameState);
            var op2 = Operand2.Evaluate(gameState);
            switch (Operator) {
                case MathsOperator.Add: return op1 + op2;
                case MathsOperator.Sub: return op1 - op2;
                case MathsOperator.Mul: return op1 * op2;
                case MathsOperator.Div: return op2 == 0 ? 0 : op1 / op2; // Return 0 if user tried to divide by zero. Easier than having to deal with Infinity (which C# returns).
                case MathsOperator.Mod: return op2 == 0 ? 0 : op1 % op2;
                default: return 0;
            }
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Updates the user control and the operands with a new application context.</summary>
        public void SetApplication(Application application) {
            control?.SetApplication(application);
            Operand1?.SetApplication(application);
            Operand2?.SetApplication(application);
        }

        /// <summary>Creates a copy of this maths operation.</summary>
        public IEvaluatableNumber Clone() => new NumberMathsOperation { Operand1 = Operand1.Clone(), Operand2 = Operand2.Clone(), Operator = Operator };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    
    /// <summary>
    /// Returns the absolute value of the given evaluatable.
    /// </summary>
    [OverrideLogic("Absolute", category: OverrideLogicCategory.Maths)]
    public class NumberAbsValue : IEvaluatableNumber {

        /// <summary>Creates a new absolute operation with the default operand.</summary>
        public NumberAbsValue() { }
        /// <summary>Creates a new absolute evaluatable with the given operand.</summary>
        public NumberAbsValue(IEvaluatableNumber op) { Operand = op; }

        /// <summary>The operand to absolute.</summary>
        public IEvaluatableNumber Operand { get; set; } = new NumberConstant();

        // Get the control allowing the user to set the operand
        [JsonIgnore]
        private Control_NumericUnaryOpHolder control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new Control_NumericUnaryOpHolder(application, "Absolute");
                control.SetBinding(Control_NumericUnaryOpHolder.OperandProperty, new Binding("Operand") { Source = this, Mode = BindingMode.TwoWay });
            }
            return control;
        }

        /// <summary>Evaluate the operand and return the absolute value of it.</summary>
        public double Evaluate(IGameState gameState) => Math.Abs(Operand.Evaluate(gameState));
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) => Operand?.SetApplication(application);

        public IEvaluatableNumber Clone() => new NumberAbsValue { Operand = Operand.Clone() };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }



    /// <summary>
    /// Evaluatable that compares two numerical evaluatables and returns a boolean depending on the comparison.
    /// </summary>
    [OverrideLogic("Arithmetic Comparison", category: OverrideLogicCategory.Maths)]
    public class BooleanMathsComparison : IEvaluatableBoolean {

        /// <summary>Creates a new maths comparison that has no values pre-set.</summary>
        public BooleanMathsComparison() { }
        /// <summary>Creates a new evaluatable that returns whether or not the two given numbers are equal.</summary>
        public BooleanMathsComparison(double value1, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); }
        /// <summary>Creates a new evaluatable that returns the result of the two given numbers compared using the given operator.</summary>
        public BooleanMathsComparison(double value1, ComparisonOperator op, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); Operator = op; }
        /// <summary>Creates a new evaluatable that returns whether or not the given evaluatable and given number are equal.</summary>
        public BooleanMathsComparison(IEvaluatableNumber eval, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); }
        /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number when compared using the given operator.</summary>
        public BooleanMathsComparison(IEvaluatableNumber eval, ComparisonOperator op, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); Operator = op; }
        /// <summary>Creates a new evaluatable that returns the whether or not the two given evaluatables are equal.</summary>
        public BooleanMathsComparison(IEvaluatableNumber eval1, IEvaluatableNumber eval2) { Operand1 = eval1; Operand2 = eval2; }
        /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables when compared using the given operator.</summary>
        public BooleanMathsComparison(IEvaluatableNumber eval1, ComparisonOperator op, IEvaluatableNumber eval2) { Operand1 = eval1; Operand2 = eval2; Operator = op; }

        // The operands and the operator
        public IEvaluatableNumber Operand1 { get; set; } = new NumberConstant();
        public IEvaluatableNumber Operand2 { get; set; } = new NumberConstant();
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.EQ;

        // The control allowing the user to edit the evaluatable
        [JsonIgnore]
        private Control_BinaryOperationHolder control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new Control_BinaryOperationHolder(application, EvaluatableType.Number, typeof(ComparisonOperator));
                control.SetBinding(Control_BinaryOperationHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay });
                control.SetBinding(Control_BinaryOperationHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay });
                control.SetBinding(Control_BinaryOperationHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });
            }
            return control;
        }

        /// <summary>Resolves the two operands and then compares them with the user-specified operator.</summary>
        public bool Evaluate(IGameState gameState) {
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
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Updates the user control and the operands with a new application context.</summary>
        public void SetApplication(Application application) {
            control?.SetApplication(application);
            Operand1?.SetApplication(application);
            Operand2?.SetApplication(application);
        }

        /// <summary>Creates a copy of this mathematical comparison.</summary>
        public IEvaluatableBoolean Clone() => new BooleanMathsComparison { Operand1 = Operand1.Clone(), Operand2 = Operand2.Clone() };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }



    /// <summary>
    /// Evaluatable that takes a number in a given range and maps it onto another range.
    /// </summary>
    [OverrideLogic("Numeric Map", category: OverrideLogicCategory.Maths)]
    public class NumberMap : IEvaluatableNumber {

        /// <summary>Creates a new numeric map with the default constant parameters.</summary>
        public NumberMap() { }
        /// <summary>Creates a new numeric map to map the given value with the given constant range onto the range 0 → 1.</summary>
        public NumberMap(IEvaluatableNumber value, double fromMin, double fromMax) : this(value, new NumberConstant(fromMin), new NumberConstant(fromMax)) { }
        /// <summary>Creates a new numeric map to map the given value with the given dynamic range onto the range 0 → 1.</summary>
        public NumberMap(IEvaluatableNumber value, IEvaluatableNumber fromMin, IEvaluatableNumber fromMax) { Value = value; FromMin = fromMin; FromMax = fromMax; }
        /// <summary>Creates a new numeric map to map the given value with the given constant from range onto the given constant to range.</summary>
        public NumberMap(IEvaluatableNumber value, double fromMin, double fromMax, double toMin, double toMax) : this(value, new NumberConstant(fromMin), new NumberConstant(fromMax), new NumberConstant(toMin), new NumberConstant(toMax)) { }
        /// <summary>Creates a new numeric map to map the given value with the given dynamic from range onto the given constant to range.</summary>
        public NumberMap(IEvaluatableNumber value, IEvaluatableNumber fromMin, IEvaluatableNumber fromMax, double toMin, double toMax) : this(value, fromMin, fromMax, new NumberConstant(toMin), new NumberConstant(toMax)) { }
        /// <summary>Creates a new numeric map to map the given value with the given dynamic from range onto the given dynamic to range.</summary>
        public NumberMap(IEvaluatableNumber value, IEvaluatableNumber fromMin, IEvaluatableNumber fromMax, IEvaluatableNumber toMin, IEvaluatableNumber toMax) { Value = value; FromMin = fromMin; FromMax = fromMax; ToMin = toMin; ToMax = toMax; }

        // The value to run through the map
        public IEvaluatableNumber Value { get; set; } = new NumberConstant(25);
        // The values representing the starting range of the map
        public IEvaluatableNumber FromMin { get; set; } = new NumberConstant(0);
        public IEvaluatableNumber FromMax { get; set; } = new NumberConstant(100);
        // The values representing the end range of the map
        public IEvaluatableNumber ToMin { get; set; } = new NumberConstant(0);
        public IEvaluatableNumber ToMax { get; set; } = new NumberConstant(1);

        // The control to edit the map parameters
        [JsonIgnore]
        private Control_NumericMap control;
        public Visual GetControl(Application application) => control ?? (control = new Control_NumericMap(this, application));

        /// <summary>Evaluate the from range and to range and return the value in the new range.</summary>
        public double Evaluate(IGameState gameState) {
            // Evaluate all components
            double value = Value.Evaluate(gameState);
            double fromMin = FromMin.Evaluate(gameState), fromMax = FromMax.Evaluate(gameState);
            double toMin = ToMin.Evaluate(gameState), toMax = ToMax.Evaluate(gameState);

            // Perform actual equation
            return Utils.MathUtils.Clamp((value - fromMin) * ((toMax - toMin) / (fromMax - fromMin)) + toMin, Math.Min(toMin, toMax), Math.Max(toMin, toMax));
            // Here is an example of it running: https://www.desmos.com/calculator/nzbiiz7vxv
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary> Updates the applications on all sub evaluatables.</summary>
        public void SetApplication(Application application) {
            Value?.SetApplication(application);
            FromMin?.SetApplication(application);
            ToMin?.SetApplication(application);
            FromMax?.SetApplication(application);
            ToMax?.SetApplication(application);
        }

        public IEvaluatableNumber Clone() => new NumberMap { Value = Value.Clone(), FromMin = FromMin.Clone(), ToMin = ToMin.Clone(), FromMax = FromMax.Clone(), ToMax = ToMax.Clone() };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }



    /// <summary>
    /// Evaluatable that resolves to a numerical constant.
    /// </summary>
    [OverrideLogic("Number Constant", category: OverrideLogicCategory.Maths)]
    public class NumberConstant : IEvaluatableNumber {

        /// <summary>Creates a new constant with the zero as the constant value.</summary>
        public NumberConstant() { }
        /// <summary>Creats a new constant with the given value as the constant value.</summary>
        public NumberConstant(double value) { Value = value; }

        // The constant value
        public double Value { get; set; }

        // The control allowing the user to edit the number value
        [JsonIgnore]
        private DoubleUpDown control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new DoubleUpDown { Margin = new System.Windows.Thickness(0, 0, 0, 6) };
                control.SetBinding(DoubleUpDown.ValueProperty, new Binding("Value") { Source = this });
            }
            return control;
        }

        /// <summary>Simply returns the constant value specified by the user</summary>
        public double Evaluate(IGameState gameState) => Value;
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Does nothing - this evaluatable is application-independant.</summary>
        public void SetApplication(Application application) { }

        /// <summary>Creates a copy of this number constant</summary>
        public IEvaluatableNumber Clone() => new NumberConstant { Value = Value };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
