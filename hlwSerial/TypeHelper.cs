using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hlwSerial
{
    public static class TypeHelper
    {
        /// <summary>
        /// Gives a shortened assemblyQualifiedName of the type, with only its fullName and the name of its assembly. Does the same for its generic type parameters if it has any.
        /// </summary>
        /// <param name="type">The type of which you'll get the name</param>
        /// <param name="inBrackets">default to false. Put true if the result should be surrounded by brackets in the case of being a generic type parameter.You shouldn't have to set it to true yourself.</param>
        /// <returns></returns>
        public static string GetShortTypeName(this Type type, bool inBrackets = false)
        {
            if (type.IsGenericType) return type.GetShortGenericName(inBrackets);
            if (inBrackets) return $"[{type.FullName}, {type.Assembly.GetName().Name}]";
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        /// <summary>
        /// Private function that will be called by the GetShortTypeName method if the type tested is generic.
        /// </summary>
        /// <param name="type">The type of which you'll get the name</param>
        /// <param name="inBrackets">default to false. Put true if the result should be surrounded by brackets in the case of being a generic type parameter. You shouldn't have to use this.</param>
        /// <returns></returns>
        private static string GetShortGenericName(this Type type, bool inBrackets = false)
        {
            if (inBrackets)
                return $"[{type.GetGenericTypeDefinition().FullName}[{string.Join(", ", type.GenericTypeArguments.Select(a => a.GetShortTypeName(true)))}], {type.Assembly.GetName().Name}]";
            else
                return $"{type.GetGenericTypeDefinition().FullName}[{string.Join(", ",type.GenericTypeArguments.Select(a=> a.GetShortTypeName(true)))}], {type.Assembly.GetName().Name}";
        }
    }
}
