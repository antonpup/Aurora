using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.Profiles;
using DCD = Aurora.Settings.Overrides.Logic.DynamicConstructorDefinition;
using DCPD = Aurora.Settings.Overrides.Logic.DynamicConstructorParamDefinition;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// The override dynamic logic creates a values dynamically based on some given IEvaluatables.
    /// This differs from the lookup table as non-discrete values can be used instead.
    /// </summary>
    public class OverrideDynamicValue : IOverrideLogic {

        #region Constructors
        /// <summary>
        /// Creates a new OverrideDynamicValue for the specified type of property.
        /// <paramref name="type">The type of property being edited. E.G. for the "Enabled" property, the type will be `typeof(bool)`</paramref>
        /// </summary>
        public OverrideDynamicValue(Type type) {
            VarType = type;
            
            // Create a new set of constructor parameters by taking all the defined values in the typeDynamicDefMap for this type, then creating
            // a new instance of the default IEvaluatable for each parameter. E.G. for a parameter specified as EvaluatableType.Boolean, a new true
            // constant will be put in the constructor parameters dictionary.
            ConstructorParameters = typeDynamicDefMap.ContainsKey(type)
                ? typeDynamicDefMap[type].constructorParameters.ToDictionary(dcpd => dcpd.name, dcpd => EvaluatableTypeResolver.GetDefault(dcpd.type))
                : null;
        }

        /// <summary>
        /// Creates a new OverrideDynamicValue for the specified type of property with an existing set of constructor parameters.
        /// This constructor is the one called by the JSON deserializer so that it doesn't re-create the constructor parameters from the given type.
        /// <paramref name="type">The type of property being edited. E.G. for the "Enabled" property, the type will be `typeof(bool)`</paramref>
        /// <paramref name="constructorParameters">A collection of parameters that will be evaluated to construct the type.</paramref> 
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        public OverrideDynamicValue(Type type, Dictionary<string, IEvaluatable> constructorParameters) {
            VarType = type;
            ConstructorParameters = constructorParameters;
        }
        #endregion

        /// <summary>Event for when a property changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        /// <summary>The type of variable being handled by the dynamic value logic.</summary>
        public Type VarType { get; set; }

        /// <summary>A dictionary of all parameters to be passed to the constructing method for this dynamic value property.
        /// The key is the name of the variable and the value is the IEvaluatable instance as specified in the typeDynamicDefMap.</summary>
        public Dictionary<string, IEvaluatable> ConstructorParameters { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates the DynamicConstructor logic with the given gamestate.
        /// </summary>
        public object Evaluate(IGameState gameState) => typeDynamicDefMap.ContainsKey(VarType)
            // If this is a valid type (i.e. supported by the dynamic constructor), then call the constructor method with the results of the IEvaluatables as the parameter
            ? typeDynamicDefMap[VarType].dynamicConstructor(ConstructorParameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Evaluate(gameState)))
            // If it's not valid, simply return null (which will mean there is no override).
            : null;

        /// <summary>
        /// Creates the control that is used to edit the IEvaluatables used as parameters for this DynamicValue logic
        /// </summary>
        public Visual GetControl(Application application) => typeDynamicDefMap.ContainsKey(VarType)
            // If this has a valid type (i.e. supported by the dynamic constructor), then create the control and pass in `this` and `application` for context
            ? new Control_OverrideDynamicValue(this, application)
            // If it is an invalid type, then simply show a red warning message
            : (Visual)new Label { Content = "This property type is not supported with the dynamic value editor. Sorry :(", Foreground = Brushes.Red, Margin = new System.Windows.Thickness(6) };
        #endregion

        /// <summary>
        /// Dictionary map that contains a list of all supported types (that can be dynamically constructed) along with their constructor definitions.
        /// Each item in this dictionary represents a type that can be constructed using the dynamic value logic. The DCD contains a list of parameters
        /// that are required for the constructor (including what type they are) and also a constructor function which is passed these RESOLVED
        /// parameters each frame.
        /// </summary>
        internal static readonly Dictionary<Type, DCD> typeDynamicDefMap = new Dictionary<Type, DCD> {
            // Boolean
            { typeof(bool), new DCD(p => p["Value"], new[]{ new DCPD("Value", EvaluatableType.Boolean) }) },

            // Numeric
            { typeof(int), new DCD(p => Convert.ToInt32(p["Value"]), new[]{ new DCPD("Value", EvaluatableType.Number) }) },
            { typeof(long), new DCD(p => Convert.ToInt64(p["Value"]), new[]{ new DCPD("Value", EvaluatableType.Number) }) },
            { typeof(float), new DCD(p => Convert.ToSingle(p["Value"]), new[]{ new DCPD("Value", EvaluatableType.Number) }) },
            { typeof(double), new DCD(p => p["Value"], new[]{ new DCPD("Value", EvaluatableType.Number) }) },

            // Special
            { typeof(System.Drawing.Color), new DCD(
                p => System.Drawing.Color.FromArgb(ToColorComp(p["Alpha"]), ToColorComp(p["Red"]), ToColorComp(p["Green"]), ToColorComp(p["Blue"])),
                new[] {
                    new DCPD("Alpha", EvaluatableType.Number, "A value between 0 (transparent) and 1 (opaque) for the transparency of the color."),
                    new DCPD("Red", EvaluatableType.Number, "A value between 0 and 1 for the amount of red in the color."),
                    new DCPD("Green", EvaluatableType.Number, "A value between 0 and 1 for the amount of green in the color."),
                    new DCPD("Blue", EvaluatableType.Number, "A value between 0 and 1 for the amount of blue in the color.")
                }
            ) },

            { typeof(KeySequence), new DCD(
                p => new KeySequence(new FreeFormObject(Convert.ToSingle(p["X"]), Convert.ToSingle(p["Y"]), Convert.ToSingle(p["Width"]), Convert.ToSingle(p["Height"]), Convert.ToSingle(p["Angle"]))),
                new[] {
                    new DCPD("X", EvaluatableType.Number),
                    new DCPD("Y", EvaluatableType.Number),
                    new DCPD("Width", EvaluatableType.Number),
                    new DCPD("Height", EvaluatableType.Number),
                    new DCPD("Angle", EvaluatableType.Number)
                }
            ) }
        };

        #region Dynamic Constructor Helper Methods
        /// <summary>Converts a double object (from 0-1) into a color component (int between 0 and 255).</summary>
        private static int ToColorComp(object c) => Convert.ToInt32(Utils.MathUtils.Clamp((double)c, 0, 1) * 255);
        #endregion
    }

    /// <summary>
    /// Struct which defines a constructor that can be used to dynamically create a value using one or more IEvaluatables.
    /// </summary>
    struct DynamicConstructorDefinition {
        /// <summary>The function that takes parameters and should return a object created from those parameters.</summary>
        public Func<Dictionary<string, object>, object> dynamicConstructor;
        /// <summary>The type of parameters expected by this constructor method.</summary>
        public DCPD[] constructorParameters;

        public DynamicConstructorDefinition(Func<Dictionary<string, object>, object> constructor, DCPD[] parameters) {
            dynamicConstructor = constructor;
            constructorParameters = parameters;
        }
    }

    struct DynamicConstructorParamDefinition {
        /// <summary>Parameter name.</summary>
        public string name;
        /// <summary>The type of variable this parameter is.</summary>
        public EvaluatableType type;
        /// <summary>A simple description of the parameter for the user.</summary>
        public string description;

        public DynamicConstructorParamDefinition(string name, EvaluatableType type, string description=null) {
            this.name = name;
            this.type = type;
            this.description = description;
        }
    }
}
