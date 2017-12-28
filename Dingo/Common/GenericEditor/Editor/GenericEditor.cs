using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Dingo.Roads;
using System;
using Dingo.Terrains;
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
        private static Dictionary<Type, IGenericDrawer> _activeDrawers; // Contains all types that implement IGenericDrawer
        private static Dictionary<Type, IGenericDrawer> _drawerTypeMapping = new Dictionary<Type, IGenericDrawer>();
        
        private static IGenericDrawer GetDrawer(Type type)
        {
            IGenericDrawer drawer;
            if(_drawerTypeMapping.TryGetValue(type, out drawer) && drawer != null)
            {
                return drawer;
            }
            if(_activeDrawers == null)
            {
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
                
            }
        }

        public static void DrawGUI(object target, string label = "", Type targetType = null, FieldInfo fieldInfo = null, object context = null)
        {
        }
    }
}