using UnityEditor;
using Dingo.Common.GenericEditor;

namespace Dingo.Roads
{
    [CustomEditor(typeof(ConnectionConfiguration))]
    public class ConnectionConfigurationGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            GenericEditor.DrawGUI(target);
        }
    }
}