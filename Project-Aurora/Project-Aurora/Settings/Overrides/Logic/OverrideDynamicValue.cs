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
                ? typeDynamicDefMap[type].constructorParameters.ToDictionary(kvp => kvp.Key, kvp => EvaluatableTypeResolver.GetDefault(kvp.Value))
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
        /// </summary>
        internal static readonly Dictionary<Type, DCD> typeDynamicDefMap = new Dictionary<Type, DCD> {
            // Boolean
            { typeof(bool), new DCD(p => p["Value"], new Dictionary<string, EvaluatableType>{ { "Value", EvaluatableType.Boolean } }) },

            // Numeric
            { typeof(int), new DCD(p => Convert.ToInt32(p["Value"]), new Dictionary<string, EvaluatableType>{ { "Value", EvaluatableType.Number } }) },
            { typeof(long), new DCD(p => Convert.ToInt64(p["Value"]), new Dictionary<string, EvaluatableType>{ { "Value", EvaluatableType.Number } }) },
            { typeof(float), new DCD(p => Convert.ToSingle(p["Value"]), new Dictionary<string, EvaluatableType>{ { "Value", EvaluatableType.Number } }) },
            { typeof(double), new DCD(@params => @params["Value"], new Dictionary<string, EvaluatableType>{ { "Value", EvaluatableType.Number } }) },

            // Special
            { typeof(System.Drawing.Color), new DCD(
                p => System.Drawing.Color.FromArgb(ToColorComp(p["Alpha"]), ToColorComp(p["Red"]), ToColorComp(p["Green"]), ToColorComp(p["Blue"])),
                new Dictionary<string, EvaluatableType>{
                    { "Alpha", EvaluatableType.Number }, { "Red", EvaluatableType.Number }, { "Green", EvaluatableType.Number }, { "Blue", EvaluatableType.Number }
                })
            }
        };

        #region Dynamic Constructor Helper Methods
        private static int ToColorComp(object c) => Convert.ToInt32(Utils.MathUtils.Clamp((double)c, 0, 1) * 255);
        #endregion
    }

    /// <summary>
    /// Struct which defines a constructor that can be used to dynamically create a value using a Evaluatable.
    /// </summary>
    struct DynamicConstructorDefinition {
        /// <summary>The function that takes parameters and should return a object created from those parameters.</summary>
        public Func<Dictionary<string, object>, object> dynamicConstructor;
        /// <summary>The type of parameters expected by this constructor method.</summary>
        public Dictionary<string, EvaluatableType> constructorParameters;

        public DynamicConstructorDefinition(Func<Dictionary<string, object>, object> constructor, Dictionary<string, EvaluatableType> parameters) {
            dynamicConstructor = constructor;
            constructorParameters = parameters;
        }
    }
}
