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
                w.Template.Dispose();
                w.Template = wsct.Template.Clone();
                Selection.activeGameObject = null;
                /*
                var mask2 = w.Template.Creators.First(layer => layer is MaskDataCreator) as MaskDataCreator;
                var mask1 = wsct.Template.Creators.First(layer => layer is MaskDataCreator) as MaskDataCreator;
                Debug.Log("Prefab avg:" + mask1.Mask.AvgValue());
                Debug.Log("Window avg:" + mask2.Mask.AvgValue());*/
            }
        }
    }
}