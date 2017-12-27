using UnityEditor;

[CustomEditor(typeof(ConnectionConfiguration))]
public class ConnectionConfigurationGUI : Editor 
{
    public override void OnInspectorGUI()
    {
        AutoEditorWrapper.ShowAutoEditorGUI(target);
        EditorUtility.SetDirty(target);
    }
}
