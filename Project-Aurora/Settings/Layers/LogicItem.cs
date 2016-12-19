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
    
    public class RealColor
    {
        public System.Drawing.Color Color { get; set; }

        public RealColor()
        {

        }

        public RealColor(System.Drawing.Color color)
        {
            this.Color = color;
        }
    }

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

    public class LogicItem
    {
        public enum ActionType
        {
            PlayAnimation,
            SetProperty,
        }

        

        public static Dictionary<LogicOperator, Func<dynamic, dynamic, bool>> OperatorFuncs = new Dictionary<LogicOperator, Func<dynamic, dynamic, bool>>
        {
            { LogicOperator.GreaterThan, Operator.GreaterThan<dynamic> },
            { LogicOperator.GreaterThanOrEqual, Operator.GreaterThanOrEqual<dynamic> },
            { LogicOperator.LessThan, Operator.LessThan<dynamic> },
            { LogicOperator.LessThanOrEqual, Operator.LessThanOrEqual<dynamic> },
            { LogicOperator.Equal, Operator.Equal<dynamic> },
            { LogicOperator.NotEqual, Operator.NotEqual<dynamic> }
        };

        public Dictionary<string, Tuple<LogicOperator, object>> ReferenceComparisons { get; set; } = new Dictionary<string, Tuple<LogicOperator, object>>();

        public Tuple<ActionType, object> Action { get; set; }

        public void Check(IGameState gs, ILayerHandler handler)
        {
            if (Action == null || ReferenceComparisons == null)
                return;

            foreach (KeyValuePair<string, Tuple<LogicOperator, object>> kvp in ReferenceComparisons)
            {
                dynamic var = GameStateUtils.RetrieveGameStateParameter(gs, kvp.Key);

                if (var == null)
                    return;

                Console.WriteLine("Got him");

                dynamic comparison = kvp.Value.Item2;
                bool valid = false;

                switch(kvp.Value.Item1)
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
                        var = ((RealColor)var).Color;
                    ((ILogic)handler.Properties).Logic.SetValueFromString(str, var);
                    //handler.Properties._PrimaryColor = (Color)Action.Item2;
                    break;
                default:
                    break;
            }
        }
    }
}
