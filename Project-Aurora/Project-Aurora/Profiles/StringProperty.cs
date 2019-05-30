using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles
{
    public interface IStringProperty
    {
        object GetValueFromString(string name, object input = null);
        void SetValueFromString(string name, object value);
        IStringProperty Clone();
    }

    public class StringProperty<T> : IStringProperty
    {
        public static Dictionary<string, Tuple<Func<T, object>, Action<T, object>, Type>> PropertyLookup { get; set; } = null;
        public static object DictLock = new object();

        public StringProperty()
        {
            lock (DictLock)
            {
                if (PropertyLookup != null)
                    return;

                PropertyLookup = new Dictionary<string, Tuple<Func<T, object>, Action<T, object>, Type>>();

                Type typ = typeof(T);
                foreach (MemberInfo prop in typ.GetMembers())
                {
                    ParameterExpression paramExpression = Expression.Parameter(typ);
                    Func<T, object> getp;
                    switch (prop.MemberType)
                    {
                        case MemberTypes.Property:
                        case MemberTypes.Field:
                            Type t = Expression.GetFuncType(typ, typeof(object));

                            LambdaExpression exp = Expression.Lambda(
                                t,
                                Expression.Convert(
                                    Expression.PropertyOrField(paramExpression, prop.Name),
                                    typeof(object)
                                ),
                                paramExpression
                            );

                            getp = (Func<T, object>)exp.Compile();
                            break;
                        /*case MemberTypes.Property:
                            getp = (Func<T, object>)Delegate.CreateDelegate(
                                typeof(Func<T, object>),
                                ((PropertyInfo)prop).GetMethod
                            );

                            break;*/
                        default:
                            continue;
                    }



                    Action<T, object> setp = null;
                    if (!(prop.MemberType == MemberTypes.Property && ((PropertyInfo)prop).SetMethod == null))
                    {
                        ParameterExpression paramExpression2 = Expression.Parameter(typeof(object));
                        MemberExpression propertyGetterExpression = Expression.PropertyOrField(paramExpression, prop.Name);

                        Type var_type;
                        if (prop is PropertyInfo)
                            var_type = ((PropertyInfo)prop).PropertyType;
                        else
                            var_type = ((FieldInfo)prop).FieldType;

                        setp = Expression.Lambda<Action<T, object>>
                        (
                            Expression.Assign(propertyGetterExpression, Expression.ConvertChecked(paramExpression2, var_type)), paramExpression, paramExpression2
                        ).Compile();
                    }
                    if (!PropertyLookup.ContainsKey(prop.Name))
                    {
                        PropertyLookup.Add(prop.Name, new Tuple<Func<T, object>, Action<T, object>, Type>(getp, setp, typ));
                    }

                }
            }
        }

        public object GetValueFromString(string name, object input = null)
        {
            if (PropertyLookup.ContainsKey(name))
            {
                return PropertyLookup[name].Item1((T)(object)this);
            }

            /*Type t = obj.GetType();
            MemberInfo member;
            if ((member = input == null ? t.GetMember(name).FirstOrDefault() : t.GetMethod(name, new[] { input.GetType() })) != null)
            {
                if (member is FieldInfo)
                    return ((FieldInfo)member).GetValue(obj);
                else if (member is PropertyInfo)
                    return ((PropertyInfo)member).GetValue(obj);
                else if (member is MethodInfo)
                    return ((MethodInfo)member).Invoke(obj, new[] { input });
            }*/

            return null;
        }

        public void SetValueFromString(string name, object value)
        {
            if (PropertyLookup.ContainsKey(name))
            {
                PropertyLookup[name]?.Item2((T)(object)this, value);
            }
        }

        public IStringProperty Clone()
        {
            return (IStringProperty)this.MemberwiseClone();
        }
    }
}
