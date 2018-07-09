using System.Collections;
using System.Collections.Generic;
using MadMaps.WorldStamps;
using UnityEditor;
using UnityEngine;

namespace MadMaps.WorldStamps
{
    [CustomEditor(typeof(SimpleTerrainPlane))]
    [CanEditMultipleObjects]
    public class SimpleTerrainPlaneGUI : Editor
    {
        private SerializedProperty _blendMode,
            _falloffMode,
            _falloffCurve,
            _falloffTexture,
            _layerName,
            _offset,
            _size,
            _objectsEnabled,
            _treesEnabled,
            _grassEnabled,
            _setSplat,
            _splat,
            _splatStrength,
            _priority;

        public void OnEnable()
        {
            _blendMode = serializedObject.FindProperty("BlendMode");
            _falloffMode = serializedObject.FindProperty("FalloffMode");
            _falloffCurve = serializedObject.FindProperty("Falloff");
            _falloffTexture = serializedObject.FindProperty("FalloffTexture");
            _layerName = serializedObject.FindProperty("LayerName");
            _offset = serializedObject.FindProperty("Offset");
            _size = serializedObject.FindProperty("AreaSize");
            _objectsEnabled = serializedObject.FindProperty("RemoveObjects");
            _treesEnabled = serializedObject.FindProperty("RemoveTrees");
            _grassEnabled = serializedObject.FindProperty("RemoveGrass");
            _setSplat = serializedObject.FindProperty("SetSplat");
            _splat = serializedObject.FindProperty("Splat");
            _splatStrength = serializedObject.FindProperty("SplatStrength");
            _priority = serializedObject.FindProperty("Priority");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_blendMode);
            EditorGUILayout.PropertyField(_falloffMode);
            EditorGUI.indentLevel++;
            if (_falloffMode.hasMultipleDifferentValues || _falloffMode.enumValueIndex == 0 ||
                _falloffMode.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(_falloffCurve);
            }
            if (_falloffMode.hasMultipleDifferentValues || _falloffMode.enumValueIndex == 2)
            {
                EditorGUILayout.PropertyField(_falloffTexture);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(_layerName);
            EditorGUILayout.PropertyField(_offset);
            EditorGUILayout.PropertyField(_size);
            EditorGUILayout.PropertyField(_objectsEnabled);
            EditorGUILayout.PropertyField(_treesEnabled);
            EditorGUILayout.PropertyField(_grassEnabled);

            EditorGUILayout.PropertyField(_setSplat);
            if(_setSplat.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_splat);
                EditorGUILayout.PropertyField(_splatStrength);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_priority);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
