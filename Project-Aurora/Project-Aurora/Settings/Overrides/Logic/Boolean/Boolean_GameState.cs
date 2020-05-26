using Aurora.Profiles;
using Aurora.Utils;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that accesses a specific game state variable (of boolean type) and returns the state.
    /// </summary>
    [Evaluatable("Boolean State Variable", category: EvaluatableCategory.State)]
    public class BooleanGSIBoolean : Evaluatable<bool> {

        /// <summary>Creates an empty boolean state variable lookup.</summary>
        public BooleanGSIBoolean() { }
        /// <summary>Creates a evaluatable that returns the boolean variable at the given path.</summary>
        public BooleanGSIBoolean(string variablePath) { VariablePath = variablePath; }

        /// <summary>The path to the variable the user wants to evaluate.</summary>
        public string VariablePath { get; set; } = "";

        /// <summary>The control assigned to this condition. Stored as a reference
        /// so that the application be updated if required.</summary>
        [Newtonsoft.Json.JsonIgnore]
        private Control_ConditionGSIBoolean control;
        public override Visual GetControl() => control ?? (control = new Control_ConditionGSIBoolean(this));

        /// <summary>Fetches the given boolean value from the game state and returns it.</summary>
        protected override bool Execute(IGameState gameState) => gameState.GetBool(VariablePath);

        public override Evaluatable<bool> Clone() => new BooleanGSIBoolean { VariablePath = VariablePath };
    }



    /// <summary>
    /// Condition that accesses some specified game state variables (of numeric type) and returns a comparison between them.
    /// </summary>
    [Evaluatable("Numeric State Variable", category: EvaluatableCategory.State)]
    public class BooleanGSINumeric : Evaluatable<bool> {

        /// <summary>Creates a blank numeric game state lookup evaluatable.</summary>
        public BooleanGSINumeric() { }
        /// <summary>Creates a numeric game state lookup that returns true when the variable at the given path equals the given value.</summary>
        public BooleanGSINumeric(string path1, double val) { Operand1Path = path1; Operand2Path = val.ToString(); }
        /// <summary>Creates a numeric game state lookup that returns true when the variable at path1 equals the given variable at path2.</summary>
        public BooleanGSINumeric(string path1, string path2) { Operand1Path = path1; Operand2Path = path2; }
        /// <summary>Creates a numeric game state lookup that returns a boolean depending on the given operator's comparison between the variable at the given path and the value.</summary>
        public BooleanGSINumeric(string path1, ComparisonOperator op, double val) { Operand1Path = path1; Operand2Path = val.ToString(); Operator = op; }
        /// <summary>Creates a numeric game state lookup that returns a boolean depending on the given operator's comparison between the variable at path1 and the variable at path2.</summary>
        public BooleanGSINumeric(string path1, ComparisonOperator op, string path2) { Operand1Path = path1; Operand2Path = path2; Operator = op; }

        // Path to the two GSI variables (or numbers themselves) and the operator to compare them with
        public string Operand1Path { get; set; }
        public string Operand2Path { get; set; }
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.EQ;

        // Control assigned to this condition
        [Newtonsoft.Json.JsonIgnore]
        private Control_ConditionGSINumeric control;
        public override Visual GetControl() => control ?? (control = new Control_ConditionGSINumeric(this));

        /// <summary>Parses the numbers, compares the result, and returns the result.</summary>
        protected override bool Execute(IGameState gameState) {
            // Parse the operands (either as numbers or paths)
            double op1 = gameState.GetNumber(Operand1Path);
            double op2 = gameState.GetNumber(Operand2Path);

            // Evaluate the operands based on the selected operator and return the result.
            switch (Operator) {
                case ComparisonOperator.EQ: return op1 == op2;
                case ComparisonOperator.NEQ: return op1 != op2;
                case ComparisonOperator.LT: return op1 < op2;
                case ComparisonOperator.LTE: return op1 <= op2;
                case ComparisonOperator.GT: return op1 > op2;
                case ComparisonOperator.GTE: return op1 >= op2;
                default: return false;
            }
        }
        
        public override Evaluatable<bool> Clone() => new BooleanGSINumeric { Operand1Path = Operand1Path, Operand2Path = Operand2Path, Operator = Operator };
    }



    /// <summary>
    /// Condition that accesses a specified game state variable (of any enum type) and returns a comparison between it and a static enum of the same type.
    /// </summary>
    [Evaluatable("Enum State Variable", category: EvaluatableCategory.State)]
    public class BooleanGSIEnum : Evaluatable<bool> {

        /// <summary>Creates a blank enum game state lookup evaluatable.</summary>
        public BooleanGSIEnum() { }
        /// <summary>Creates an enum game state lookup that returns true when the variable at the given path equals the given enum.</summary>
        public BooleanGSIEnum(string path, Enum val) { StatePath = path; EnumValue = val; }

        // The path of the game state enum
        public string StatePath { get; set; }

        // The value to compare the GSI enum against.
        // Has to be converted using the TypeAnnotatedObjectConverter else the type won't be stored, only the number (which JSON then doesn't know how to serialise back)
        [Newtonsoft.Json.JsonConverter(typeof(TypeAnnotatedObjectConverter))]
        public Enum EnumValue { get; set; }

        // Control
        private Control_BooleanGSIEnum control;
        public override Visual GetControl() => control ?? (control = new Control_BooleanGSIEnum(this));

        /// <summary>Parses the numbers, compares the result, and returns the result.</summary>
        protected override bool Execute(IGameState gameState) {
            var @enum = gameState.GetEnum(StatePath);
            return @enum != null && @enum.Equals(EnumValue);
        }
        
        public override Evaluatable<bool> Clone() => new BooleanGSIEnum { StatePath = StatePath, EnumValue = EnumValue };
    }
}