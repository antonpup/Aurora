using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Aurora.Profiles;
using Aurora.Utils;
using MiscUtil;
using System.Drawing;

namespace Aurora.Settings.Layers
{
    /*public class AssociatedOperatorAttribute : Attribute
    {
        public string OperatorMethodName { get; set; }

        public AssociatedOperatorAttribute(string op_name)
        {
            OperatorMethodName = op_name;
        }
    }*/
    
    

    public enum LogicOperator
    {
        //[AssociatedOperator("op_GreaterThan")]
        GreaterThan,
        //[AssociatedOperator("op_GreaterThanOrEqual")]
        GreaterThanOrEqual,
        //[AssociatedOperator("op_LessThan")]
        LessThan,
        //[AssociatedOperator("op_LessThanOrEqual")]
        LessThanOrEqual,
        //[AssociatedOperator("op_Equality")]
        Equal,
        //[AssociatedOperator("op_Inequality")]
        NotEqual
    }

    public enum ActionType
    {
        PlayAnimation,
        SetProperty,
    }

    public class LogicItem
    {
        public static Dictionary<LogicOperator, Func<dynamic, dynamic, bool>> OperatorFuncs = new Dictionary<LogicOperator, Func<dynamic, dynamic, bool>>
        {
            { LogicOperator.GreaterThan, Operator.GreaterThan<dynamic> },
            { LogicOperator.GreaterThanOrEqual, Operator.GreaterThanOrEqual<dynamic> },
            { LogicOperator.LessThan, Operator.LessThan<dynamic> },
            { LogicOperator.LessThanOrEqual, Operator.LessThanOrEqual<dynamic> },
            { LogicOperator.Equal, Operator.Equal<dynamic> },
            { LogicOperator.NotEqual, Operator.NotEqual<dynamic> }
        };

        public List<Tuple<string, Tuple<LogicOperator, object>>> ReferenceComparisons { get; set; } = new List<Tuple<string, Tuple<LogicOperator, object>>>();

        public Tuple<ActionType, object> Action { get; set; } = null;

        public void Check(IGameState gs, ILayerHandler handler)
        {
            if (Action == null || ReferenceComparisons == null)
                return;

            foreach (Tuple<string, Tuple<LogicOperator, object>> kvp in ReferenceComparisons)
            {
                dynamic var = GameStateUtils.RetrieveGameStateParameter(gs, kvp.Item1);

                if (var == null)
                    return;

                Console.WriteLine("Got him");

                dynamic comparison = kvp.Item2.Item2;
                bool valid = false;

                switch(kvp.Item2.Item1)
                {
                    case LogicOperator.GreaterThan:
                        valid = Operator.GreaterThan(var, comparison);
                        break;
                    case LogicOperator.GreaterThanOrEqual:
                        valid = Operator.GreaterThanOrEqual(var, comparison);
                        break;
                    case LogicOperator.LessThan:
                        valid = Operator.LessThan(var, comparison);
                        break;
                    case LogicOperator.LessThanOrEqual:
                        valid = Operator.LessThanOrEqual(var, comparison);
                        break;
                    case LogicOperator.Equal:
                        valid = Operator.Equal(var, comparison);
                        break;
                    case LogicOperator.NotEqual:
                        valid = Operator.NotEqual(var, comparison);
                        break;
                    default:
                        break;
                }

                if (!valid)
                    return;
            }

            switch(Action.Item1)
            {
                case ActionType.SetProperty:
                    Tuple<string, object> vars = (Tuple<string, object>)Action.Item2;
                    string str = vars.Item1;
                    if (!str.StartsWith("_"))
                        str = "_" + str;
                    object var = vars.Item2;
                    if (var is RealColor)
                        var = ((RealColor)var).GetDrawingColor();
                    ((ILogic)handler.Properties).Logic.SetValueFromString(str, var);
                    //handler.Properties._PrimaryColor = (Color)Action.Item2;
                    break;
                default:
                    break;
            }
        }
        public override string ToString()
        {
            string str = "if ";

            if (ReferenceComparisons.Count > 0)
            {
                for (int i = 0; i < ReferenceComparisons.Count; i++)
                {
                    if (i > 0)
                        str += "and ";
                    var check = ReferenceComparisons[i];
                    str += $"{check.Item1} is {check.Item2.Item1.ToString()} {check.Item2.Item2.ToString()}";
                    
                }
            }
            else
                str += "Empty ";

            str += "then ";

            if (Action != null)
                str += $"{Action.Item1.ToString()} {Action.Item2}";
            else
                str += "Empty";

            return str;
        }
    }
}
