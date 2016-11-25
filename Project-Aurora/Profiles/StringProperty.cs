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
    }

    public class StringProperty<T> : IStringProperty
    {
        public static Dictionary<string, Func<T, object>> PropertyLookup { get; set; }

        public StringProperty()
        {
            if (PropertyLookup != null)
                return;

            PropertyLookup = new Dictionary<string, Func<T, object>>();

            Type typ = typeof(T);
            foreach (MemberInfo prop in typ.GetMembers())
            {
                switch (prop.MemberType)
                {
                    case MemberTypes.Property: //Currently going to use Property through this manner, the commented one out below is faster, had some issues getting it to work on this scale though
                    case MemberTypes.Field:
                        Type t = Expression.GetFuncType(typ, typeof(object));

                        var param = Expression.Parameter(typ);

                        PropertyLookup.Add(prop.Name, (Func<T, object>)Expression.Lambda(
                            t,
                            Expression.Convert(
                                Expression.PropertyOrField(param, prop.Name),
                                typeof(object)
                            ),
                            param
                        ).Compile());
                        break;
                    /*case MemberTypes.Property:
                        PropertyLookup.Add(prop.Name, (Func<T, object>)Delegate.CreateDelegate(
                            typeof(Func<T, object>),
                            ((PropertyInfo)prop).GetMethod
                        ));
                        break;*/
                    default:
                        continue;
                }
            }
        }
        public object GetValueFromString(string name, object input = null)
        {
            if (PropertyLookup.ContainsKey(name))
            {
                return PropertyLookup[name]((T)(object)this);
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
    }
}
