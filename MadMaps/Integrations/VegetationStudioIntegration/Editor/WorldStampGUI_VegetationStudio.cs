#if VEGETATION_STUDIO
using System.Collections.Generic;
using System.Linq;
using MadMaps.Common;
using MadMaps.Common.Painter;
using MadMaps.Terrains;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MadMaps.WorldStamp
{
    public partial class WorldStampGUI
    {        
        private bool _vegetationStudioExpanded;
        private SerializedProperty _vegetationStudioEnabled;
        private SerializedProperty _stencilVSData;
        private SerializedProperty _removeExistingVSData;     

        private void DoVegetationStudioSection()
        {
            WorldStamp singleInstance = targets.Length == 1 ? target as WorldStamp : null;
            bool canWrite = !singleInstance || singleInstance.Data.VSData.Count > 0;
            DoHeader("Vegetation Studio", ref _vegetationStudioExpanded, _vegetationStudioEnabled, canWrite);
            if (_vegetationStudioExpanded)
            {
                EditorGUI.indentLevel++;     
                EditorGUILayout.PropertyField(_removeExistingVSData);
                GUI.enabled = canWrite;
                EditorGUILayout.PropertyField(_stencilVSData);
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }
        }
   
    }
}
#endif