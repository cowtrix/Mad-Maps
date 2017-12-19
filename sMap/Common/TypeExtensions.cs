using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace sMap.Common
{
    public static class TypeExtensions
    {
        public static List<Type> GetAllChildTypes(this Type type)
        {
            List<Type> result = new List<Type>();

            Assembly assembly = Assembly.GetAssembly(type);
            Type[] allTypes = assembly.GetTypes();
            for (int i = 0; i < allTypes.Length; i++)
                if (allTypes[i].IsSubclassOf(type)) result.Add(allTypes[i]); //nb: IsAssignableFrom will return derived classes

            return result;
        }

        public static List<Type> GetAllTypesImplementingInterface(this Type interfaceType, Type assemblyType = null)
        {
            if (!interfaceType.IsInterface)
            {
                throw new Exception("Must be an interface type!");
            }
            var result = new List<Type>();

            var assembly = Assembly.GetAssembly(assemblyType ?? interfaceType);
            var allTypes = assembly.GetTypes();
            for (var i = 0; i < allTypes.Length; i++)
            {
                if (allTypes[i].GetInterfaces().Contains(interfaceType))
                {
                    result.Add(allTypes[i]);
                }
            }

            return result;
        }
    }
}