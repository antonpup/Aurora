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

        // The operands and the operator
        public IEvaluatableNumber Operand1 { get; set; } = new NumberConstant();
        public IEvaluatableNumber Operand2 { get; set; } = new NumberConstant();
        public MathsOperator Operator { get; set; } = MathsOperator.Add;
        
        // The control allowing the user to edit the evaluatable
        [JsonIgnore]
        private Control_NumericBinaryOpHolder control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new Control_NumericBinaryOpHolder(application, typeof(MathsOperator));
                control.SetBinding(Control_NumericBinaryOpHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay });
                control.SetBinding(Control_NumericBinaryOpHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay });
                control.SetBinding(Control_NumericBinaryOpHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });
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

        // The operands and the operator
        public IEvaluatableNumber Operand1 { get; set; } = new NumberConstant();
        public IEvaluatableNumber Operand2 { get; set; } = new NumberConstant();
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.EQ;

        // The control allowing the user to edit the evaluatable
        [JsonIgnore]
        private Control_NumericBinaryOpHolder control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new Control_NumericBinaryOpHolder(application, typeof(ComparisonOperator));
                control.SetBinding(Control_NumericBinaryOpHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay });
                control.SetBinding(Control_NumericBinaryOpHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay });
                control.SetBinding(Control_NumericBinaryOpHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });
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
