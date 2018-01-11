using System.Linq;
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

                var newBounds = new Bounds(wsct.transform.position, wsct.Size);
                newBounds = WorldStampCreator.ClampBounds(w.Template.Terrain, newBounds);
                w.Template.Bounds = newBounds;
                w.GetCreator<MaskDataCreator>().SetMaskFromArray(w, wsct.Mask);
            }
            DrawDefaultInspector();
        }
    }
}