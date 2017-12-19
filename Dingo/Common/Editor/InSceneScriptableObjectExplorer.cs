using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dingo.Common
{
    public class InSceneScriptableObjectExplorer : EditorWindow
    {
        public static HashSet<Type> ExcludedTypes = new HashSet<Type>()
        {
            typeof(EditorWindow),
            typeof(Editor),
        }; 
        
        [MenuItem("Tools/Level/In-Scene Scriptable Objects")]
        public static void OpenWindow()
        {
            GetWindow<InSceneScriptableObjectExplorer>();
        }

        List<ScriptableObject> _objects = new List<ScriptableObject>();

        void OnGUI()
        {
            if (GUILayout.Button("Collect"))
            {
                _objects.Clear();
                _objects.AddRange(Resources.FindObjectsOfTypeAll<ScriptableObject>());
                for (int i = _objects.Count-1; i >= 0; i--)
                {
                    var scriptableObject = _objects[i];
                    if (scriptableObject.hideFlags != HideFlags.None)
                    {
                        _objects.RemoveAt(i);
                        continue;
                    }
                    if (AssetDatabase.Contains(scriptableObject))
                    {
                        _objects.RemoveAt(i);
                        continue;
                    }
                    foreach (var excludedType in ExcludedTypes)
                    {
                        if (excludedType.IsInstanceOfType(scriptableObject))
                        {
                            _objects.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            foreach (var scriptableObject in _objects)
            {
                EditorGUILayout.ObjectField(scriptableObject, typeof (ScriptableObject), true);
            }
        }
    }
}