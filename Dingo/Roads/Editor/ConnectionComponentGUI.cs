using UnityEditor;

namespace Dingo.Roads
{
    [CustomEditor(typeof (ConnectionComponent), true)]
    [CanEditMultipleObjects]
    public class ConnectionComponentGUI : Editor
    {
        /*public SerializedProperty _configuration;

        public void OnEnable()
        {
            _configuration = serializedObject.FindProperty("Configuration");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Rebake"))
            {
                foreach (var o in targets)
                {
                    var comp = o as ConnectionComponent;
                    comp.Initialize();
                    comp.OnPrebake();
                    comp.OnRebake();
                    comp.OnPostBake();
                }
            }
        }*/
    }
}