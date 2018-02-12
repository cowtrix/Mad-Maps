using UnityEditor;
using UnityEngine;

namespace MadMaps.Common.GenericEditor
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinAttribute))]
    public class MinAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minAttr = attribute as MinAttribute;
            switch (property.type)
            {
                case "float":
                    property.floatValue = Mathf.Max(EditorGUI.FloatField(position, label, property.floatValue), minAttr.MinValue);
                    return;
                case "int":
                    property.intValue = Mathf.Max(EditorGUI.IntField(position, label, property.intValue), (int)minAttr.MinValue);
                    return;
            }
            GUI.Label(position, string.Format("Unsupported type {0} for MinAttribute", property.type));
        }
    }
#endif
}