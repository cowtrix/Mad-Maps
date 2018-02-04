using System;
using MadMaps.Common;
using MadMaps.Common.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace MadMaps.WorldStamp.Authoring
{
    [Serializable]
    public class HeightmapDataCreator : WorldStampCreatorLayer
    {
        [NonSerialized]
        public Serializable2DFloatArray Heights;

        public bool AutoZeroLevel = true;

        [NonSerialized]
        [Common.GenericEditor.ShowIf("AutoZeroLevel", false)]
        public float ZeroLevel = 0;

        private WorldStampPreview _preview;

        private bool _dirty;

        public override GUIContent Label
        {
            get { return new GUIContent("Heightmap");}
        }

        protected override bool HasDataPreview
        {
            get { return true; }
        }

        protected override void CaptureInternal(Terrain terrain, Bounds bounds)
        {
            var min = terrain.WorldToHeightmapCoord(bounds.min, TerrainX.RoundType.Floor);
            var max = terrain.WorldToHeightmapCoord(bounds.max, TerrainX.RoundType.Floor);

            int width = max.x - min.x;
            int height = max.z - min.z;

            float avgMarginHeight = 0; // If we want, we can have the stamp try to automatically find a good zero level by averaging the heights around the edge of the stamp
            int marginCount = 0;

            Heights = new Serializable2DFloatArray(width + 1, height + 1);

            var sampleHeights = terrain.terrainData.GetHeights(min.x, min.z, width + 1, height + 1);
            for (var dx = 0; dx <= width; ++dx)
            {
                for (var dz = 0; dz <= height; ++dz)
                {
                    var sample = sampleHeights[dz, dx];
                    Heights[dx, dz] = sample;

                    if (dx == 0 || dx == width || dz == 0 || dz == height)
                    {
                        avgMarginHeight += sample;
                        marginCount++;
                    }
                }
            }
            if (AutoZeroLevel)
            {
                ZeroLevel = avgMarginHeight / marginCount;
            }
            for (var dx = 0; dx <= width; ++dx)
            {
                for (var dz = 0; dz <= height; ++dz)
                {
                    Heights[dx, dz] -= ZeroLevel;
                }
            }
            _dirty = true;
        }

        public override void Clear()
        {
            Heights.Clear();
        }

        protected override void CommitInternal(WorldStampData data, WorldStamp stamp)
        {
            data.Heights = Heights.JSONClone();
            data.ZeroLevel = ZeroLevel;
        }

        protected override void OnExpandedGUI(WorldStampCreator parent)
        {
            EditorGUI.BeginChangeCheck();
            AutoZeroLevel = EditorGUILayout.Toggle("Auto Zero Level", AutoZeroLevel);
            if (!AutoZeroLevel)
            {
                EditorGUI.indentLevel++;
                var newHeightMin = EditorGUILayout.Slider("Zero Level", ZeroLevel, 0, 1);
                EditorGUI.indentLevel--;
                if (newHeightMin != ZeroLevel)
                {
                    ZeroLevel = newHeightMin;
                    NeedsRecapture = true;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                NeedsRecapture = true;
            }
        }

        protected override void PreviewInSceneInternal(WorldStampCreator parent)
        {
            var bounds = parent.Template.Bounds;
            if (_preview == null || _preview.IsDisposed())
            {
                _preview = new WorldStampPreview();
                _dirty = true;
            }
            if (_dirty)
            {
                _preview.Invalidate(
                    Heights, () => bounds.size, () => bounds.center.xz().x0z(bounds.min.y + 2 + ZeroLevel * bounds.size.y), () => Vector3.one,
                    () => Quaternion.identity, () => bounds.size, true, null, null,
                    () => parent.SceneGUIOwner == this, 128);
                _dirty = false;
            }

            HandleExtensions.DrawWireCube(bounds.center.xz().x0z(bounds.min.y + ZeroLevel * bounds.size.y), bounds.size.xz().x0z() / 2, Quaternion.identity, Color.cyan);
        }

        public override void PreviewInDataInspector()
        {
            DataInspector.SetData(Heights);
        }
    }
}
#endif