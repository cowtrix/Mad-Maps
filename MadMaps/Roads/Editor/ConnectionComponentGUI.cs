using UnityEditor;

namespace MadMaps.Roads
{
    [CustomEditor(typeof (ConnectionComponent), true)]
    [CanEditMultipleObjects]
    public class ConnectionComponentGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            foreach (var o in targets)
            {
                var cc = o as ConnectionComponent;
                cc.Think();
            }
            base.OnInspectorGUI();
        }
    }
}