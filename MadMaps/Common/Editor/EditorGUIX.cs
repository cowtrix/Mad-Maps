#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MadMaps.Common
{
    public static class EditorGUIX
    {
        [MenuItem("Tools/Mad Maps/Documentation")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("http://lrtw.net/madmaps/");
        }

        [MenuItem("Tools/Mad Maps/Contact Support")]
        public static void ContactSupport()
        {
            Application.OpenURL("mailto:seandgfinnegan+madmaps@gmail.com");
        }

        public static void PropertyField(Rect labelRect, Rect propertyRect, SerializedProperty property)
        {
            EditorGUI.LabelField(labelRect, property.displayName);
            EditorGUI.PropertyField(propertyRect, property, GUIContent.none);
        }
    }
}

#endif