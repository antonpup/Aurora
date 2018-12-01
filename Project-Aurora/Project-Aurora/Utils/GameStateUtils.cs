using Aurora.Profiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class GameStateUtils
    {
        static Dictionary<Type, bool> AdditionalAllowedTypes = new Dictionary<Type, bool>
        {
            { typeof(string), true },
        };

        public static Dictionary<string, Tuple<Type, Type>> ReflectGameStateParameters(Type typ)
        {
            Dictionary<string, Tuple<Type, Type>> parameters = new Dictionary<string, Tuple<Type, Type>>();

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

                if (prop_type.IsPrimitive || AdditionalAllowedTypes.ContainsKey(prop_type))
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
                        Dictionary<string, Tuple<Type, Type>> sub_params;

                        if (prop_type == typ)
                            sub_params = new Dictionary<string, Tuple<Type, Type>>(parameters);
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

        public static object RetrieveGameStateParameter(IGameState state, string parameter_path, params object[] input_values)
        {
            if (Global.isDebug)
                return _RetrieveGameStateParameter(state, parameter_path, input_values);
            else
            {
                try
                {
                    return _RetrieveGameStateParameter(state, parameter_path, input_values);
                }
                catch (Exception exc)
                {
                    Global.logger.Error($"Exception: {exc}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Attempts to get a double value from the game state with the given path.
        /// Returns 0 if an error occurs
        /// </summary>
        public static double TryGetDoubleFromState(IGameState state, string path) {
            if (!double.TryParse(path, out double value) && !string.IsNullOrWhiteSpace(path)) {
                try {
                    value = Convert.ToDouble(RetrieveGameStateParameter(state, path));
                } catch (Exception exc) {
                    value = 0;
                    if (Global.isDebug)
                        throw exc;
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


        /*public static object RetrieveGameStateParameter(GameState state, string parameter_path, Type type = null)
        {


            return null;
        }*/
    }
}
