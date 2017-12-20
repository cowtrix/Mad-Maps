using System;
using Dingo.Common;
using UnityEditor;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    [Serializable]
    public abstract class WorldStampCreatorLayer
    {
        public bool GUIExpanded;
        public bool Enabled = true;

        public virtual bool NeedsRecapture { get; set; }
        public virtual bool ManuallyRecapturable { get { return true; } }

        protected abstract GUIContent Label { get; }
        protected abstract bool HasDataPreview { get; }

        public WorldStampCreatorLayer()
        {
            NeedsRecapture = true;
        }

        public void DrawGUI(WorldStampCreator parent)
        {
            EditorExtensions.Seperator();

            EditorGUILayout.BeginHorizontal();
            GUIExpanded = EditorGUILayout.Foldout(GUIExpanded, Label);

            bool dataResult = false;
            if (HasDataPreview)
            {
                var previewContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
                previewContent.tooltip = "Preview In Window";
                if (GUILayout.Button(previewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(16)))
                {
                    PreviewInDataInspector();
                }
            }

            Enabled = EditorGUILayout.Toggle(new GUIContent(string.Empty, Enabled ? "Enable Capture" : "Disable Capture"), Enabled, GUILayout.Width(20));

            var sceneviewContent = EditorGUIUtility.IconContent("Terrain Icon");
            sceneviewContent.tooltip = parent.SceneGUIOwner == this ? "Clear Preview" : "Preview On Terrain";
            GUI.color = parent.SceneGUIOwner == this ? Color.white : Color.gray;
            if (GUILayout.Button(sceneviewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
            {
                parent.SceneGUIOwner = parent.SceneGUIOwner == this ? null : this;
            }
            GUI.color = Color.white;

            GUI.enabled = parent.Terrain != null;
            GUI.color = NeedsRecapture ? Color.red : Color.white;
            if (ManuallyRecapturable && GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Refresh"), EditorStyles.boldLabel, GUILayout.Width(20), GUILayout.Height(16)))
            {
                Capture(parent.Terrain, parent.Bounds);
            }
            GUI.color = Color.white;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (GUIExpanded)
            {
                OnExpandedGUI(parent);
            }
        }

        public void Capture(Terrain terrain, Bounds bounds)
        {
            if (terrain == null || bounds.size == Vector3.zero)
            {
                return;
            }
            CaptureInternal(terrain, bounds);
            NeedsRecapture = false;
        }

        public void PreviewInScene(WorldStampCreator parent)
        {
            if (parent.Terrain == null || parent.Bounds.size == Vector3.zero || (ManuallyRecapturable && NeedsRecapture))
            {
                return;
            }
            PreviewInSceneInternal(parent);
        }
        
        public abstract void PreviewInDataInspector();
        public abstract void Commit(WorldStampData data);

        protected abstract void PreviewInSceneInternal(WorldStampCreator parent);
        protected abstract void CaptureInternal(Terrain terrain, Bounds bounds);
        protected abstract void OnExpandedGUI(WorldStampCreator parent);
    }
}