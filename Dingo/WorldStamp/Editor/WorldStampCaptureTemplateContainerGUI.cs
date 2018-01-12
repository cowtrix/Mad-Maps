using System.Linq;
using Dingo.Common;
using Dingo.Common.Painter;
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

                if (w.Template.Terrain == null)
                {
                    w.Template.Terrain = Terrain.activeTerrain;
                }

                var newBounds = new Bounds(wsct.transform.position, wsct.Size);
                newBounds = WorldStampCreator.ClampBounds(w.Template.Terrain, newBounds);
                
                w.Template.Bounds = newBounds;
                var mask = w.GetCreator<MaskDataCreator>();
                mask.SetMaskFromArray(w, wsct.Mask);
                mask.LastBounds = newBounds;
            }
            DrawDefaultInspector();
        }
    }
}