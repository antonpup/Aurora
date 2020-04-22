using Aurora.Profiles;
using Aurora.Utils;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Logic that compares two strings using a selection of operators.
    /// </summary>
    [Evaluatable("String Comparison", category: EvaluatableCategory.String)]
    public class StringComparison : IEvaluatable<bool> {

        // Operands and operator
        public IEvaluatable<string> Operand1 { get; set; } = new StringConstant();
        public IEvaluatable<string> Operand2 { get; set; } = new StringConstant();
        public StringComparisonOperator Operator { get; set; } = StringComparisonOperator.Equal;
        public bool CaseInsensitive { get; set; } = false;

        // Control allowing the user to edit the comparison
        public Visual GetControl() => new Control_BinaryOperationHolder(typeof(string), typeof(StringComparisonOperator))
            .WithBinding(Control_BinaryOperationHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay })
            .WithBinding(Control_BinaryOperationHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });

        /// <summary>Compares the two strings with the given operator</summary>
        public bool Evaluate(IGameState gameState) {
            var op1 = Operand1.Evaluate(gameState);
            var op2 = Operand2.Evaluate(gameState);

            if (CaseInsensitive) {
                op1 = op1.ToLower();
                op2 = op2.ToLower();
            }

            switch (Operator) {
                case StringComparisonOperator.Equal: return op1 == op2;
                case StringComparisonOperator.NotEqual: return op1 != op2;
                case StringComparisonOperator.Before: return string.Compare(op1, op2) < 0;
                case StringComparisonOperator.After: return string.Compare(op1, op2) > 0;
                case StringComparisonOperator.EqualLength: return op1.Length == op2.Length;
                case StringComparisonOperator.ShorterThan: return op1.Length < op2.Length;
                case StringComparisonOperator.LongerThan: return op1.Length > op2.Length;
                case StringComparisonOperator.StartsWith: return op1.StartsWith(op2);
                case StringComparisonOperator.EndsWith: return op1.EndsWith(op2);
                case StringComparisonOperator.Contains: return op1.Contains(op2);
                default: return false;
            }
        }
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Clones this StringComparison.</summary>
        public IEvaluatable<bool> Clone() => new StringComparison { Operand1 = Operand1.Clone(), Operand2 = Operand2.Clone(), Operator = Operator, CaseInsensitive = CaseInsensitive };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }
}
