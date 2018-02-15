using UnityEditor;

namespace MadMaps.Roads
{
    [CustomEditor(typeof (ConnectionComponent), true)]
    [CanEditMultipleObjects]
    public class ConnectionComponentGUI : Editor
    {
        private SerializedProperty _overridePriority;
        private SerializedProperty _priority;
        private SerializedProperty _connection;

        public void OnEnable()
        {
            _overridePriority = serializedObject.FindProperty("OverridePriority");
            _priority = serializedObject.FindProperty("Priority");
            _connection = serializedObject.FindProperty("NodeConnection");
        }

        public override void OnInspectorGUI()
        {
            foreach (var o in targets)
            {
                var cc = o as ConnectionComponent;
                cc.Think();
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_connection);
            EditorGUILayout.PropertyField(_overridePriority);
            if (_overridePriority.boolValue || _overridePriority.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_priority);
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            //base.OnInspectorGUI();
        }
    }
}