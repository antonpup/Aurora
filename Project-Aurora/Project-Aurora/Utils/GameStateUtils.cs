using Aurora.Profiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GamestateParameterLookup = System.Collections.Generic.Dictionary<string, System.Tuple<System.Type, System.Type>>;

namespace Aurora.Utils
{
    public static class GameStateUtils
    {
        static Dictionary<Type, bool> AdditionalAllowedTypes = new Dictionary<Type, bool>
        {
            { typeof(string), true },
        };

        public static GamestateParameterLookup ReflectGameStateParameters(Type typ)
        {
            var parameters = new GamestateParameterLookup();

            foreach (MemberInfo prop in typ.GetFields().Cast<MemberInfo>().Concat(typ.GetProperties().Cast<MemberInfo>()).Concat(typ.GetMethods().Where(m => !m.IsSpecialName).Cast<MemberInfo>()))
            {
                if (prop.GetCustomAttribute(typeof(GameStateIgnoreAttribute)) != null)
                    continue;

                Type prop_type;
                Type prop_param_type = null;
                switch (prop.MemberType)
                {
                    case MemberTypes.Field:
                        prop_type = ((FieldInfo)prop).FieldType;
                        break;
                    case MemberTypes.Property:
                        prop_type = ((PropertyInfo)prop).PropertyType;
                        break;

                    /* Why is this here? There is no way of passing parameters to methods from the game state UI?? */
                    case MemberTypes.Method:
                        //if (((MethodInfo)prop).IsSpecialName)
                            //continue;

                        prop_type = ((MethodInfo)prop).ReturnType;

                        if (prop.Name.Equals("Equals") || prop.Name.Equals("GetType") || prop.Name.Equals("ToString") || prop.Name.Equals("GetHashCode"))
                            continue;

                        if (((MethodInfo)prop).GetParameters().Count() == 0)
                            prop_param_type = null;
                        else if (((MethodInfo)prop).GetParameters().Count() == 1)
                            prop_param_type = ((MethodInfo)prop).GetParameters()[0].ParameterType;
                        else
                        {
                            //Warning: More than 1 parameter!
                            Console.WriteLine();
                        }

                        break;
                    default:
                        continue;
                }

                if(prop.Name.Equals("Abilities"))
                    Console.WriteLine();

                Type temp = null;

                if (prop_type.IsPrimitive || prop_type.IsEnum || AdditionalAllowedTypes.ContainsKey(prop_type))
                {
                    parameters.Add(prop.Name, new Tuple<Type, Type>(prop_type, prop_param_type));
                }
                else if (prop_type.IsArray || prop_type.GetInterfaces().Any(t =>
                {
                    return t == typeof(IEnumerable) || t == typeof(IList) || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>) && (temp = t.GenericTypeArguments[0]) != null);
                }))
                {
                    RangeAttribute attribute;
                    if ((attribute = prop.GetCustomAttribute(typeof(RangeAttribute)) as RangeAttribute) != null)
                    {
                        var sub_params = ReflectGameStateParameters(temp ?? prop_type.GetElementType());

                        for (int i = attribute.Start; i <= attribute.End; i++)
                        {
                            foreach (var sub_param in sub_params)
                                parameters.Add(prop.Name + "/" + i + "/" + sub_param.Key, sub_param.Value);
                        }
                    }
                    else
                    {
                        //warning
                        Console.WriteLine();
                    }
                }
                else if (prop_type.IsClass)
                {
                    if (prop.MemberType == MemberTypes.Method)
                    {
                        parameters.Add(prop.Name, new Tuple<Type, Type>(prop_type, prop_param_type));
                    }
                    else
                    {
                        GamestateParameterLookup sub_params;

                        if (prop_type == typ)
                            sub_params = new GamestateParameterLookup(parameters);
                        else
                            sub_params = ReflectGameStateParameters(prop_type);

                        foreach (var sub_param in sub_params)
                            parameters.Add(prop.Name + "/" + sub_param.Key, sub_param.Value);
                    }
                }
            }

            return parameters;
        }

        private static object _RetrieveGameStateParameter(IGameState state, string parameter_path, params object[] input_values)
        {
            string[] parameters = parameter_path.Split('/');

            object val = null;
            IStringProperty property_object = state as IStringProperty;
            int index_pos = 0;

            for (int x = 0; x < parameters.Count(); x++)
            {
                if (property_object == null)
                    return val;

                string param = parameters[x];

                //Following needs validation
                //If next param is placeholder then take the appropriate input value from the input_values array
                val = property_object.GetValueFromString(param);

                if (val == null)
                    throw new ArgumentNullException($"Failed to get value {parameter_path}, failed at '{param}'");

                Type property_type = property_object.GetType();
                Type temp = null;
                if (x < parameters.Length - 1 && (property_type.IsArray || property_type.GetInterfaces().Any(t =>
                {
                    return t == typeof(IEnumerable) || t == typeof(IList) || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>) && (temp = t.GenericTypeArguments[0]) != null);
                })) && int.TryParse(parameters[x + 1], out index_pos))
                {
                    x++;
                    Type child_type = temp ?? property_type.GetElementType();
                    IEnumerable<object> array = (IEnumerable<object>)property_object;

                    if (array.Count() > index_pos)
                        val = array.ElementAt(index_pos);
                    else
                        val = Activator.CreateInstance(child_type);

                }
                property_object = val as IStringProperty;
            }

            return val;
        }

        public static object RetrieveGameStateParameter(IGameState state, string parameter_path, params object[] input_values) {
            try {
                return _RetrieveGameStateParameter(state, parameter_path, input_values);
            } catch (Exception exc) {
                Global.logger.Error($"Exception: {exc}");
                if (Global.isDebug)
                    throw exc;
                return null;
            }
        }

        /// <summary>
        /// Attempts to get a double value from the game state with the given path.
        /// Will handle converting string literal numbers (e.g. "10") into a double.
        /// Returns 0 if an error occurs
        /// </summary>
        public static double TryGetDoubleFromState(IGameState state, string path) {
            if (!double.TryParse(path, out double value) && !string.IsNullOrWhiteSpace(path)) {
                try {
                    value = Convert.ToDouble(RetrieveGameStateParameter(state, path));
                } catch {
                    value = 0;
                }
            }
            return value;
        }

        /// <summary>
        /// Attempts to get a boolean value from the game state with the given path.
        /// Returns false if an error occurs.
        /// </summary>
        public static bool TryGetBoolFromState(IGameState state, string path) {
            bool value = false;
            if (!string.IsNullOrWhiteSpace(path)) {
                try {
                    value = Convert.ToBoolean(RetrieveGameStateParameter(state, path));
                } catch { }
            }
            return value;
        }

        /// <summary>
        /// Attempts to retrieve an enum value from the game state with the given path.
        /// Returns null if unable to get the value from the state.
        /// </summary>
        public static Enum TryGetEnumFromState(IGameState state, string path) {
            if (!string.IsNullOrWhiteSpace(path)) {
                try {
                    return RetrieveGameStateParameter(state, path) as Enum;
                } catch { }
            }
            return null;
        }


        #region ParameterLookup Extensions
        /// <summary>
        /// Checks if the given parameter is valid for the current parameter lookup.
        /// </summary>
        public static bool IsValidParameter(this GamestateParameterLookup parameterLookup, string parameter)
            => parameterLookup.ContainsKey(parameter);

        /// <summary>
        /// Checks if the given parameter is valid for the current parameter lookup and if the type of parameter matches the given <see cref="PropertyType"/>.
        /// </summary>
        public static bool IsValidParameter(this GamestateParameterLookup parameterLookup, string parameter, PropertyType type)
            => parameterLookup.TryGetValue(parameter, out var paramDef) && PropertyTypePredicate[type](new KeyValuePair<string, Tuple<Type, Type>>(parameter, paramDef));

        /// <summary>
        /// Gets a list of the names of all variables (inc. path) of a certain type in this parameter map.
        /// </summary>
        public static IEnumerable<string> GetParameters(this GamestateParameterLookup parameterLookup, PropertyType type)
            => parameterLookup.Where(PropertyTypePredicate[type]).Select(kvp => kvp.Key);

        static Dictionary<PropertyType, Func<KeyValuePair<string, Tuple<Type, Type>>, bool>> PropertyTypePredicate { get; } = new Dictionary<PropertyType, Func<KeyValuePair<string, Tuple<Type, Type>>, bool>> {
            [PropertyType.None] = _ => false,
            [PropertyType.Number] = kvp => TypeUtils.IsNumericType(kvp.Value.Item1),
            [PropertyType.Boolean] = kvp => Type.GetTypeCode(kvp.Value.Item1) == TypeCode.Boolean,
            [PropertyType.String] = kvp => Type.GetTypeCode(kvp.Value.Item1) == TypeCode.String,
            [PropertyType.Enum] = kvp => kvp.Value.Item1.IsEnum
        };
        #endregion
    }

    /// <summary>
    /// Available types that can be handled by the gamestate parameter methods.
    /// </summary>
    public enum PropertyType { None, Number, Boolean, String, Enum }
}
