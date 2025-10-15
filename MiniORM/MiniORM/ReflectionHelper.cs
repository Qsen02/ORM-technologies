using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniORM
{
    internal static class ReflectionHelper
    {
        public static void ReplaceBackingField(object sourceObj, string propertyName, object targetObj) { 
            var bakcingField=sourceObj.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField)
                .First(f => f.Name == $"<{propertyName}>k_BackingField");
            bakcingField.SetValue(targetObj,propertyName);
        }

        public static bool HasAttribute<T>(this MemberInfo mi) where T:Attribute  { 
            var hasAttribute = mi.GetCustomAttribute<T>() != null;
            return hasAttribute;
        }
    }
}
