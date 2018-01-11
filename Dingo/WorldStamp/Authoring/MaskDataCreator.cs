using Dingo.WorldStamp;
using System;
using System.IO;
using Dingo.Common;
using Dingo.Common.Collections;
using Dingo.Common.Painter;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using EditorCellPainter;
#endif

namespace Dingo.WorldStamp.Authoring
{
    [Serializable]
    public class MaskDataCreator : WorldStampCreatorLayer
    {
        public float GridSize = 5;
        public WorldStampMask Mask = new WorldStampMask();
        public Bounds LastBounds;

        private static float MinMaskRes = 4;
        private static float MaskResolution = 128;

        private GridManagerInt GridManager
        {
            get
            {
                if (__gridManager == null || __gridManager.GRID_SIZE != GridSize)
                {
                    __gridManager = new GridManagerInt(GridSize);
                }
                return __gridManager;
            }
        }
        private GridManagerInt __gridManager;

#if UNITY_EDITOR
        private Painter _maskPainter;
#endif
        
        private void ResetMask(Bounds bounds)
        {
            GridSize = Math.Max(MinMaskRes, Math.Max(bounds.size.x, bounds.size.z) / MaskResolution);
            Mask.Clear();
            for (var u = 0f; u < bounds.size.x; u += GridSize)
            {
                for (var v = 0f; v < bounds.size.z; v += GridSize)
                {
                    var cell = GridManager.GetCell(new Vector3(u, 0, v));
                    var cellMax = GridManager.GetCellMax(cell).x0z() + bounds.min;
                    var cellMin = GridManager.GetCellCenter(cell).x0z() + bounds.min;
                    if (!bounds.Contains(cellMax) || !bounds.Contains(cellMin))
                    {
                        continue;
                    }
                    Mask.SetValue(cell, 1);
                }
            }
            LastBounds = bounds;
        }

        private void FillMaskFromMinY(Bounds bounds, Serializable2DFloatArray heights)
        {
            Mask.Clear();
            for (var u = 0f; u < bounds.size.x; u += GridSize)
            {
                for (var v = 0f; v < bounds.size.z; v += GridSize)
                {
                    var cell = GridManager.GetCell(new Vector3(u, 0, v));
                    var cellMax = GridManager.GetCellMax(cell).x0z() + bounds.min;
                    var cellMin = GridManager.GetCellCenter(cell).x0z() + bounds.min;
                    if (!bounds.Contains(cellMax) || !bounds.Contains(cellMin))
                    {
                        continue;
                    }

                    var h = heights.BilinearSample(new Vector2(v / bounds.size.z, u / bounds.size.x));
                    Mask.SetValue(cell, h > 0 ? 1 : 0);
                }
            }
        }

        public override GUIContent Label
        {
            get { return new GUIContent("Mask"); }
        }

        protected override bool HasDataPreview
        {
            get { return false; }
        }

        public override bool ManuallyRecapturable
        {
            get { return false; }
        }

        protected override void CaptureInternal(Terrain terrain, Bounds bounds)
        {
            if (LastBounds != bounds)
            {
                ResetMask(bounds);
            }
        }

#if UNITY_EDITOR
        protected override void PreviewInSceneInternal(WorldStampCreator parent)
        {
            var bounds = parent.Template.Bounds;
            if (_maskPainter == null)
            {
                _maskPainter = new Common.Painter.Painter(Mask, GridManager);
                _maskPainter.Ramp = new Gradient()
                {
                    colorKeys = new[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.black, 0.001f), new GradientColorKey(Color.black, 1), },
                    alphaKeys = new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1), }
                };
                _maskPainter.Rect = new Rect(bounds.min.xz(), bounds.size.xz());
            }
            else
            {
                GridSize = Math.Max(MinMaskRes, Math.Max(bounds.size.x, bounds.size.z) / MaskResolution);
                _maskPainter.GridManager = GridManager;
                _maskPainter.Canvas = Mask;
                _maskPainter.MaxValue = 1;
                _maskPainter.MinValue = 0;
                _maskPainter.Rect = new Rect(Vector2.zero, bounds.size.xz());
                _maskPainter.TRS = Matrix4x4.TRS(bounds.min, Quaternion.identity, Vector3.one);
                _maskPainter.Repaint();
                _maskPainter.PaintingEnabled = true;
                _maskPainter.OnSceneGUI();
            }
        }

        override public void DrawGUI(WorldStampCreator parent)
        {
            EditorExtensions.Seperator();
            EditorGUILayout.BeginHorizontal();

            GUIExpanded = EditorGUILayout.Foldout(GUIExpanded, Label);
            var previewContent = new GUIContent("Edit");
            previewContent.tooltip = "Edit the mask for this stamp.";
            GUI.color = parent.SceneGUIOwner == this ? Color.green : Color.white;
            if (GUILayout.Button(previewContent, EditorStyles.miniButton, GUILayout.Width(60), GUILayout.Height(16)))
            {
                parent.SceneGUIOwner = parent.SceneGUIOwner == this ? null : this;
                if (parent.Template.Bounds != LastBounds)
                {
                    ResetMask(parent.Template.Bounds);
                }
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            if (GUIExpanded)
            {
                GUI.enabled = Enabled;
                EditorGUI.indentLevel++;
                OnExpandedGUI(parent);
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }
        }

        protected override void OnExpandedGUI(WorldStampCreator parent)
        {
            if (EditorGUILayoutX.IndentedButton("Reset"))
            {
                ResetMask(parent.Template.Bounds);
            }
            if (EditorGUILayoutX.IndentedButton("Fill From Min Y"))
            {
                FillMaskFromMinY(parent.Template.Bounds, parent.GetCreator<HeightmapDataCreator>().Heights);
            }
            if (EditorGUILayoutX.IndentedButton("Load From Texture"))
            {
                LoadFromTexture(parent);
            }
        }

        private void LoadFromTexture(WorldStampCreator parent)
        {
            var path = EditorUtility.OpenFilePanel("Load Texture Into Mask", "Assets", "png");
            if (!string.IsNullOrEmpty(path))
            {
                var tex = new Texture2D(0, 0);
                tex.LoadImage(File.ReadAllBytes(path));
                GridSize = Math.Max(MinMaskRes, Math.Max(parent.Template.Bounds.size.x, parent.Template.Bounds.size.z) / MaskResolution);
                Mask.Clear();
                for (var u = 0f; u < parent.Template.Bounds.size.x; u += GridSize)
                {
                    for (var v = 0f; v < parent.Template.Bounds.size.z; v += GridSize)
                    {
                        var cell = GridManager.GetCell(new Vector3(u, 0, v));
                        var cellMax = GridManager.GetCellMax(cell).x0z() + parent.Template.Bounds.min;
                        var cellMin = GridManager.GetCellCenter(cell).x0z() + parent.Template.Bounds.min;
                        if (!parent.Template.Bounds.Contains(cellMax) || !parent.Template.Bounds.Contains(cellMin))
                        {
                            continue;
                        }
                        var val = tex.GetPixelBilinear(u / parent.Template.Bounds.size.x, v / parent.Template.Bounds.size.z).grayscale;
                        Mask.SetValue(cell, val);
                    }
                }
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }
#endif

        public override void PreviewInDataInspector()
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            Mask.Clear();
        }

        protected override void CommitInternal(WorldStampData data, WorldStamp stamp)
        {
            Mask.OnBeforeSerialize();
            data.Mask = Mask.JSONClone();
            data.GridSize = GridSize;
        }

        public override bool CanDisable
        {
            get { return false; }
        }
    }
}
