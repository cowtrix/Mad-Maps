using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dingo.Roads
{
    [CustomEditor(typeof(ConnectionTerrainConfiguration))]
    public class ConnectionTerrainConfigurationGUI : Editor
    {
        private SerializedProperty _priority;
        private SerializedProperty _heightFalloff, _height, _radius;
        private SerializedProperty _removeTrees, _treeRemoveDistance;

        private SerializedProperty _setSplats, _splatConfigs, _splatFalloff;
        private SerializedProperty _removeObjects, _objRemovalMask, _objRemoveDistance, _objRegex;
        private SerializedProperty _removeGrass, _grassFalloff;

        void OnEnable()
        {
            //_setHeight = serializedObject.FindProperty("SetHeight");
            _priority = serializedObject.FindProperty("Priority");
            _heightFalloff = serializedObject.FindProperty("HeightFalloff");
            _height = serializedObject.FindProperty("Height");
            _radius = serializedObject.FindProperty("Radius");

            _removeTrees = serializedObject.FindProperty("RemoveTrees");
            _treeRemoveDistance = serializedObject.FindProperty("TreeRemoveDistance");

            _setSplats = serializedObject.FindProperty("SetSplat");
            _splatFalloff = serializedObject.FindProperty("SplatFalloff");
            _splatConfigs = serializedObject.FindProperty("SplatConfigurations");

            _removeGrass = serializedObject.FindProperty("RemoveGrass");
            _grassFalloff = serializedObject.FindProperty("GrassFalloff");

            _removeObjects = serializedObject.FindProperty("RemoveObjects");
            _objRemovalMask = serializedObject.FindProperty("ObjectRemovalMask");
            _objRemoveDistance = serializedObject.FindProperty("ObjectRemoveDistance");
            _objRegex = serializedObject.FindProperty("RegexMatch");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_priority);
            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_height);
            EditorGUILayout.CurveField(_heightFalloff, Color.green, new Rect(0, 0, 1, 1));

            EditorGUILayout.PropertyField(_setSplats);
            if (_setSplats.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_splatFalloff);
                EditorGUILayout.PropertyField(_splatConfigs, true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_removeGrass);
            if (_removeGrass.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.CurveField(_grassFalloff, Color.green, new Rect(0, 0, 1, 1));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_removeTrees);
            if (_removeTrees.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Slider(_treeRemoveDistance, 0, _radius.floatValue);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_removeObjects);
            if (_removeObjects.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Slider(_objRemoveDistance, 0, _radius.floatValue);
                EditorGUILayout.PropertyField(_objRemovalMask);
                EditorGUILayout.PropertyField(_objRegex);
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
