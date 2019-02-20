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
