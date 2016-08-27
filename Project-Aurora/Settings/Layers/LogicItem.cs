using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Layers
{
    public class AssociatedOperatorAttribute : Attribute
    {
        public string OperatorMethodName { get; set; }

        public AssociatedOperatorAttribute(string op_name)
        {
            OperatorMethodName = op_name;
        }
    }

    public enum LogicOperator
    {
        [AssociatedOperator("op_GreaterThan")]
        GreaterThan,
        [AssociatedOperator("op_GreaterThanOrEqual")]
        GreaterThanOrEqual,
        [AssociatedOperator("op_LessThan")]
        LessThan,
        [AssociatedOperator("op_LessThanOrEqual")]
        LessThanOrEqual,
        [AssociatedOperator("op_Equality")]
        Equal,
        [AssociatedOperator("op_Inequality")]
        NotEqual
    }

    public enum ActionType
    {
        PlayAnimation,
        SetColor,
        SetAlpha,
        SetKeyColor,
    }

    public struct LogicItem
    {
        public Dictionary<string, List<Tuple<LogicOperator, object>>> ReferenceComparisons { get; set; }

        public List<Tuple<ActionType, object>> Actions { get; set; }
    }
}
