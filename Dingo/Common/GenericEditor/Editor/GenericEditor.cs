using System.Collections.Generic;
using System;
using System.Reflection;

namespace Dingo.Common.GenericEditor
{
    public class GenericEditorAttribute : Attribute
    {
        public Type TargetType;
        public GenericEditorAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }

    public static class GenericEditor
    {
        private static Dictionary<Type, IGenericDrawer> _activeDrawers; // Mapping of IGeneriDrawer types to type
        private static Dictionary<Type, IGenericDrawer> _drawerTypeMapping = new Dictionary<Type, IGenericDrawer>();    // Mapping of all types to their drawer
        
        private static IGenericDrawer GetDrawer(Type type)
        {
            IGenericDrawer drawer;
            if(_drawerTypeMapping.TryGetValue(type, out drawer) && drawer != null)
            {
                return drawer;
            }

            if(_activeDrawers == null)
            {
                _activeDrawers = new Dictionary<Type, IGenericDrawer>();
                var allTypes = TypeExtensions.GetAllTypesImplementingInterface(typeof(IGenericDrawer));
                foreach(var t in allTypes)
                {
                    _activeDrawers.Add(t, null);
                }
            }

            // Interface is type = 0
            // Subclass = distance from actual class
            // Exact match - instant return
            Type bestDrawer = null;
            int bestScore = 0;
            foreach(var mapping in _activeDrawers)
            {
                var targetType = mapping.Key;
                var interfaces = targetType.GetInterfaces();
                foreach(var interfaceType in interfaces)
                {
                    if (!interfaceType.IsAssignableFrom(typeof(ITypedGenericDrawer<>)))
                    {
                        continue;
                    }
                    var genericArg = interfaceType.GetGenericArguments()[0];
                    if (genericArg == type)
                    {
                        
                    }
                    if(bestScore == 0 && genericArg.IsAssignableFrom(type))
                    {
                        bestDrawer = interfaceType;
                    }
                    
                }
            }

            if (bestDrawer == null)
            {
                throw new Exception("Failed to find drawer for type " + type);
            }
            if (!_activeDrawers.TryGetValue(bestDrawer, out drawer))
            {
                drawer = Activator.CreateInstance(bestDrawer);
            }
            _drawerTypeMapping[type] = drawer;
        }

        public static void DrawGUI(object target, string label = "", Type targetType = null, FieldInfo fieldInfo = null, object context = null)
        {
        }
    }
}