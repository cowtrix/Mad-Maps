using Dingo.Common;
using UnityEditor;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    [CustomEditor(typeof(WorldStampCaptureTemplateContainer))]
    public class WorldStampCaptureTemplateContainerGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            var wsct = target as WorldStampCaptureTemplateContainer;
            if (GUILayout.Button("Set as Capture Settings"))
            {
                var w = EditorWindow.GetWindow<WorldStampCreator>();
                w.Template = wsct.Template.Clone();
                Selection.activeGameObject = null;
            }
        }
    }
}