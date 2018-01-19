using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Dingo.Common.GenericEditor
{
    

    public static class GenericEditor
    {
        public static Dictionary<string, bool> ExpandedFieldCache = new Dictionary<string, bool>();

        private static Dictionary<Type, IGenericDrawer> _activeDrawers; // Mapping of IGeneriDrawer types to type
        private static Dictionary<Type, IGenericDrawer> _drawerTypeMapping = new Dictionary<Type, IGenericDrawer>();    // Mapping of all types to their drawer
        private static Dictionary<Type, List<FieldInfo>> _typeFieldCache = new Dictionary<Type, List<FieldInfo>>();

        public static GUIContent DeleteContent
        {
            get
            {
                return new GUIContent("X", "Delete");
            }
        }

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
                var allTypes = typeof(IGenericDrawer).GetAllTypesImplementingInterface();
                foreach(var t in allTypes)
                {
                    if (t.IsAbstract)
                    {
                        continue;
                    }
                    _activeDrawers.Add(t, null);
                }
            }

            // Interface is type = 0
            // Subclass = distance from actual class
            // Exact match - instant return
            Type bestDrawer = null;
            int bestScore = int.MinValue;
            foreach(var mapping in _activeDrawers)
            {
                var targetType = mapping.Key;
                var interfaces = targetType.GetInterfaces();
                foreach(var interfaceType in interfaces)
                {
                    var genArgs = interfaceType.GetGenericArguments();
                    if (genArgs.Length != 1)
                    {
                        continue;
                    }

                    var genericArg = genArgs[0];
                    if (genericArg == type)
                    {
                        bestDrawer = targetType;
                        break;
                    }
                    if (!genericArg.IsAssignableFrom(type))
                    {
                        continue;
                    }
                    var distance = type.GetInheritanceDistance(genericArg);
                    if (distance > bestScore)
                    {
                        bestDrawer = targetType;
                    }
                } 
            }
            if (bestDrawer == null)
            {
                return null;
            }
            if (!_activeDrawers.TryGetValue(bestDrawer, out drawer) || drawer == null)
            {  
                drawer = (IGenericDrawer) Activator.CreateInstance(bestDrawer);
            }
            _drawerTypeMapping[type] = drawer;
            return drawer;
        }

        private static List<FieldInfo> GetFields(Type type)
        {
            List<FieldInfo> result;
            if (!_typeFieldCache.TryGetValue(type, out result) || result == null)
            {
                var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                result = new List<FieldInfo>();
                foreach (var fieldInfo in allFields)
                {
                    var attributes = fieldInfo.GetCustomAttributes(true);
                    if (attributes.Any(o => o is HideInInspector))
                    {
                        continue;
                    }
                    if (fieldInfo.IsPrivate && !attributes.Any(o => o is SerializeField))
                    {
                        continue;
                    }
                    result.Add(fieldInfo);
                }
                _typeFieldCache[type] = result;
            }
            return result;
        }

        public static object DrawGUI(object target, string label = "", Type targetType = null, FieldInfo fieldInfo = null, object context = null)
        {
            if (targetType == null && target == null && fieldInfo == null)
            {
                Debug.LogError("Insufficient information to determine type.");
                return null;
            }

            if (targetType == null)
            {
                targetType = target != null ? target.GetType() : fieldInfo.FieldType;
            }

            var drawer = GetDrawer(targetType);
            if (context != null && drawer != null)
            {
                target = drawer.DrawGUI(target, label, targetType, fieldInfo, context);
            }
            else
            {
                if (fieldInfo != null)
                {
                    EditorGUI.indentLevel++;
                }
                var fields = GetFields(targetType);
                foreach (var field in fields)
                {
                    var subObj = field.GetValue(target);
                    subObj = DrawGUI(subObj, field.Name, subObj != null ? subObj.GetType() : field.FieldType, field, target);
                    field.SetValue(target, subObj);
                }
                if (fieldInfo != null)
                {
                    EditorGUI.indentLevel--;
                }
            }

            var unityObj = target as UnityEngine.Object;
            if (unityObj)
            {
                EditorUtility.SetDirty(unityObj);
            }

            return target;
        }
    }
}