using Dingo.Common;
using Dingo.Common.Collections;
using ParadoxNotion.Design;
using UnityEditor;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    public class HeightmapLayer : WorldStampCreatorLayer
    {
        public Serializable2DFloatArray Heights;
        public bool AutoHeightMin = true;
        [ShowIf("AutoHeightMin", false)]
        public float HeightMin = 0;
        public int DisplayRes = 16;

        public int MaxDisplayRes { get; private set; }

        private bool _dirty;

        protected override GUIContent Label
        {
            get { return new GUIContent("Heighmap");}
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

            float minHeight = float.MaxValue;
            Heights = new Serializable2DFloatArray(width + 1, height + 1);

            var sampleHeights = terrain.terrainData.GetHeights(min.x, min.z, width + 1, height + 1);
            for (var dx = 0; dx <= width; ++dx)
            {
                for (var dz = 0; dz <= height; ++dz)
                {
                    var sample = sampleHeights[dz, dx];
                    if (sample < minHeight)
                    {
                        minHeight = sample;
                    }
                    Heights[dx, dz] = sample;
                }
            }
            if (AutoHeightMin)
            {
                HeightMin = minHeight;
            }
            if (HeightMin > 0)
            {
                for (var dx = 0; dx <= width; ++dx)
                {
                    for (var dz = 0; dz <= height; ++dz)
                    {
                        Heights[dx, dz] -= HeightMin;
                    }
                }
            }
            MaxDisplayRes =
                Mathf.CeilToInt(Mathf.Max(
                    ((bounds.size.x / terrain.terrainData.size.x) * terrain.terrainData.alphamapResolution),
                    terrain.terrainData.size.z / bounds.size.z)
                    );
            _dirty = true;
        }

        public override void Commit(WorldStampData data)
        {
            DataInspector.SetData(Heights);
        }

        protected override void OnExpandedGUI(WorldStampCreator parent)
        {
            AutoHeightMin = EditorGUILayout.Toggle("Auto Min Height", AutoHeightMin);
            if (!AutoHeightMin)
            {
                var newHeightMin = EditorGUILayout.Slider("Min Height", HeightMin, 0, 1);
                if (newHeightMin != HeightMin)
                {
                    HeightMin = newHeightMin;
                    NeedsRecapture = true;
                }
            }
            DisplayRes = EditorGUILayout.IntSlider("Preview Resolution", DisplayRes, 8, MaxDisplayRes);
        }


        private WorldStampPreview _preview;

        protected override void PreviewInSceneInternal(WorldStampCreator parent)
        {
            var bounds = parent.Bounds;
            if (_preview == null || _preview.IsDisposed())
            {
                _preview = new WorldStampPreview();
                _dirty = true;
            }
            if (_dirty)
            {
                _preview.Invalidate(
                    Heights, () => bounds.size, () => bounds.center.x0z(bounds.min.y + 2), () => Vector3.one,
                    () => Quaternion.identity, () => bounds.size, true, null, null,
                    () => parent.SceneGUIOwner == this, 64);
                _dirty = false;
            }
            
            /*var boundsMin = bounds.min;
            var boundsSize = bounds.size;
            var tHeight = terrain.terrainData.size.y;
            var tYOffset = terrain.GetPosition().y;

            var heightMin = HeightMin * tHeight + tYOffset;
            Handles.color = Color.blue;
            Handles.DrawWireCube(bounds.center.xz().x0z(heightMin), bounds.size.xz().x0z());

            var step = (1 / (float)DisplayRes);
            var aspect = bounds.size.x / bounds.size.z;
            var stepX = step;
            var stepZ = step * aspect;
            if (bounds.size.x > bounds.size.z)
            {
                stepX = step * aspect;
                stepZ = step;
            }

            Handles.color = Color.white.WithAlpha(.4f);
            for (var dz = 0f; dz <= 1; dz += stepZ)
            {
                if (dz > 1)
                {
                    dz = 1;
                }
                var pz = dz * boundsSize.z;
                var pNeighbour1z = Mathf.Clamp01(dz + stepZ);

                for (var dx = 0f; dx <= 1; dx += stepX)
                {
                    if (dx > 1)
                    {
                        dx = 1;
                    }
                    var px = dx * boundsSize.x;
                    var pNeighbour1x = Mathf.Clamp01(dx + stepX);

                    var height = tYOffset + ((Heights.BilinearSample(new Vector2(dx, dz)) + HeightMin) * terrain.terrainData.size.y);
                    //var maskVal = _tempData.Mask.GetBilinear(_tempData.GridManager, new Vector3(px, 0, pz));
                    //height = Mathf.Lerp(heightMin, height, maskVal);

                    var p = new Vector3(boundsMin.x + px, height, boundsMin.z + pz);
                    Handles.CubeCap(-1, p, Quaternion.identity, 1);

                    if (dz > 0)
                    {
                        var heightN1 = tYOffset + (Heights.BilinearSample(new Vector2(dx, dz + stepZ)) + HeightMin) * tHeight;
                        //maskVal = _tempData.Mask.GetBilinear(_tempData.GridManager, new Vector3(pNeighbour1x * boundsSize.x, 0, pz));
                        //heightN1 = Mathf.Lerp(heightMin, heightN1, maskVal);
                        var p1 = new Vector3(boundsMin.x + pNeighbour1x * boundsSize.x, heightN1, boundsMin.z + pz);
                        Handles.DrawLine(p, p1);
                    }
                    if (dx > 0)
                    {
                        var heightN3 = tYOffset + (Heights.BilinearSample(new Vector2(dx + stepZ, dz)) + HeightMin) * tHeight;
                        //maskVal = _tempData.Mask.GetBilinear(_tempData.GridManager, new Vector3(px, 0, pNeighbour1z * boundsSize.z));
                        //heightN3 = Mathf.Lerp(heightMin, heightN3, maskVal);
                        var p3 = new Vector3(boundsMin.x + px, heightN3, boundsMin.z + pNeighbour1z * boundsSize.z);
                        Handles.DrawLine(p, p3);
                    }
                }
            }*/
        }

        public override void PreviewInDataInspector()
        {
            DataInspector.SetData(Heights);
        }
    }
}