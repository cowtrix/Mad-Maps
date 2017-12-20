using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Dingo.Terrains;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;

namespace Dingo.Common
{
    public static class EditorExtensions
    {
        [MenuItem("CONTEXT/MeshCollider/Dump Info")]
        public static void DumpMeshColliderInfo(MenuCommand command)
        {
            var mc = command.context as MeshCollider;
            Debug.Log(mc.bounds);
        }

        [MenuItem("GameObject/Hierarchy/Alphabeticise")]
        public static void Alphabeticise()
        {
            var s = Selection.gameObjects;
            s = s.OrderBy(o => o.name).ToArray();
            foreach (var gameObject in s)
            {
                gameObject.transform.SetAsLastSibling();
            }
        }

        private static GUIStyle _seperator = new GUIStyle("box")
        {
            border = new RectOffset(0, 0, 1, 0),
            margin = new RectOffset(0, 0, 0, 1),
            padding = new RectOffset(0, 0, 0, 1)
        };

        public static string OpenFilePanel(string title, string extension, string directory = null, bool assetPath = true)
        {
            bool persistantString = string.IsNullOrEmpty(directory);
            if (persistantString)
            {
                directory = EditorPrefs.GetString("sMap_EditorExtensions_OpenFilePanel");
                if (!directory.Contains(Application.dataPath))
                {
                    directory = Application.dataPath + "/" + directory;
                }
            }
            Debug.Log("Directory was " + directory);
            var path = EditorUtility.OpenFilePanel(title, directory, extension);
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            if (assetPath)
            {
                path = path.Substring(path.IndexOf("Assets/", StringComparison.Ordinal));
            }
            if (!persistantString)
            {
                return path;
            }
            directory = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));
            if (assetPath)
            {
                directory = directory.Replace(Application.dataPath, string.Empty);
            }
            EditorPrefs.SetString("sMap_EditorExtensions_OpenFilePanel", directory);
            Debug.Log("Set persistent OpenFilePanel to " + directory);
            return path;
        }

        public static string SaveFilePanel(string title, string defaultName, string extension, string directory = null, bool assetPath = true)
        {
            bool persistantString = string.IsNullOrEmpty(directory);
            if (persistantString)
            {
                directory = EditorPrefs.GetString("sMap_EditorExtensions_OpenFilePanel");
                if (!directory.Contains(Application.dataPath))
                {
                    directory = Application.dataPath + "/" + directory;
                }
            }
            Debug.Log("Directory was " + directory);
            var path = EditorUtility.SaveFilePanel(title, directory, defaultName, extension);
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            if (assetPath)
            {
                path = path.Substring(path.IndexOf("Assets/", StringComparison.Ordinal));
            }
            if (!persistantString)
            {
                return path;
            }
            directory = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));
            if (assetPath)
            {
                directory = directory.Replace(Application.dataPath, string.Empty);
            }
            EditorPrefs.SetString("sMap_EditorExtensions_OpenFilePanel", directory);
            Debug.Log("Set persistent OpenFilePanel to " + directory);
            return path;
        }

        public static void Seperator()
        {
            GUILayout.Box(GUIContent.none, _seperator, GUILayout.Height(1), GUILayout.ExpandWidth(true));
        }

        public static void GenericSerializedProperty(Rect rect, GUIContent label, SerializedProperty obj)
        {
            //GUI.Label(rect, obj.type);
            if (obj.type == "float")
            {
                obj.floatValue = EditorGUI.FloatField(rect, label, obj.floatValue);
                return;
            }
            if (obj.type == "string")
            {
                obj.stringValue = EditorGUI.TextField(rect, label, obj.stringValue);
                return;
            }
            if (obj.type == "int")
            {
                obj.intValue = EditorGUI.IntField(rect, label, obj.intValue);
                return;
            }
            if (obj.type == "AnimationCurve")
            {
                obj.animationCurveValue = EditorGUI.CurveField(rect, label, obj.animationCurveValue);
                return;
            }
            if (obj.type == "LayerMask")
            {
                obj.intValue = LayerMaskFieldUtility.LayerMaskField(rect, label, obj.intValue, false);
                return;
            }
            if (obj.type == "Vector3Pair")
            {
                var firstRect = new Rect(rect.x, rect.y, rect.width, rect.height/2);
                EditorGUI.PropertyField(firstRect, obj.FindPropertyRelative("First"));
                var secondRect = new Rect(rect.x, rect.y + rect.height / 2, rect.width, rect.height / 2);
                EditorGUI.PropertyField(secondRect, obj.FindPropertyRelative("Second"));
                return;
            }
            if (obj.type == "PPtr<$SplatPrototypeWrapper>")
            {
                obj.objectReferenceValue = EditorGUI.ObjectField(rect, obj.objectReferenceValue, typeof(SplatPrototypeWrapper), true);
                return;
            }
            if(obj.type.Contains("PPtr<$"))
            {
                obj.objectReferenceValue = EditorGUI.ObjectField(rect, obj.objectReferenceValue, typeof(Object), true);
                return;
            }
            GUI.Label(rect, String.Format("DataType not implemented: {0}", obj.type));
        }

        public static object GetParent(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }
    }
}
#endif