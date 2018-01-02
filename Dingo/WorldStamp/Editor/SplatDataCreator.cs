using System.Collections.Generic;
using System.Linq;
using Dingo.Common;
using Dingo.Common.Collections;
using Dingo.Terrains;
using Dingo.Terrains.Lookups;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    public class SplatDataCreator : WorldStampCreatorLayer
    {
        [HideInInspector]
        public List<CompressedSplatData> SplatData = new List<CompressedSplatData>();

        public List<SplatPrototypeWrapper> IgnoredSplats = new List<SplatPrototypeWrapper>();
        public bool _ignoredSplatsExpanded;

        protected override GUIContent Label
        {
            get { return new GUIContent("Splats"); }
        }

        protected override bool HasDataPreview
        {
            get { return true; }
        }

        protected override void CaptureInternal(Terrain terrain, Bounds bounds)
        {
            SplatData.Clear();
            var min = terrain.WorldToSplatCoord(bounds.min);
            var max = terrain.WorldToSplatCoord(bounds.max);

            int width = max.x - min.x;
            int height = max.z - min.z;
            
            var prototypes = terrain.terrainData.splatPrototypes;
            var wrappers = TerrainLayerUtilities.ResolvePrototypes(prototypes);

            if (prototypes.Length != wrappers.Count)
            {
                Debug.LogError("Failed to collect splats - possibly you have splat configs that aren't wrapper assets?");
                return;
            }

            var sampleSplats = terrain.terrainData.GetAlphamaps(min.x, min.z, width, height);

            for (var i = 0; i < prototypes.Length; ++i)
            {
                var wrapper = wrappers[prototypes[i]];
                if (wrapper == null || IgnoredSplats.Contains(wrapper))
                {
                    continue;
                }

                var data = new byte[width, height];
                float sum = 0;
                for (var dx = 0; dx < width; ++dx)
                {
                    for (var dz = 0; dz < height; ++dz)
                    {
                        var val = sampleSplats[dz, dx, i];
                        data[dx, dz] = (byte)Mathf.Clamp(val * 255f, 0, 255);
                        sum += val;
                    }
                }
                if (sum < 0.01f)
                {
                    Debug.Log(string.Format("Ignored splat {0} as it appeared to be empty.", wrapper.name));
                    continue;
                }

                SplatData.Add(new CompressedSplatData { Wrapper = wrapper, Data = new Serializable2DByteArray(data) });
            }
        }

        protected override void PreviewInSceneInternal(WorldStampCreator parent)
        {
            var bounds = parent.Bounds;
            int counter = 0;
            int res = 32;
            foreach (var kvp in SplatData)
            {
                int xStep = kvp.Data.Width / res;
                int zStep = kvp.Data.Height / res;
                Vector2 cellSize = new Vector2((xStep / (float)kvp.Data.Width) * bounds.size.x, (zStep / (float)kvp.Data.Height) * bounds.size.z);
                for (var u = 0; u < kvp.Data.Width; u += xStep)
                {
                    var fU = u / (float)kvp.Data.Width;
                    var wU = bounds.min.x + fU * bounds.size.x;
                    for (var v = 0; v < kvp.Data.Height; v += zStep)
                    {
                        var fV = v / (float)kvp.Data.Height;
                        var wV = bounds.min.z + fV * bounds.size.z;

                        var val = kvp.Data[u, v] / 255f;
                        HandleExtensions.DrawXZCell(new Vector3(wU, counter, wV), cellSize,
                            Quaternion.identity, ColorUtils.GetIndexColor(counter).WithAlpha(val));
                    }
                }
                counter++;
            }
        }

        public override void PreviewInDataInspector()
        {
            DataInspector.SetData(SplatData.Select(x => x.Data).ToList(), SplatData.Select(x => x.Wrapper).ToList());
        }

        public override void Clear()
        {
            SplatData.Clear();
        }

        protected override void CommitInternal(WorldStampData data)
        {
            data.SplatData = SplatData.JSONClone();
        }

        protected override void OnExpandedGUI(WorldStampCreator parent)
        {
            //AutoEditorWrapper.ListEditorNicer(string.Format("Ignored Splats ({0})", IgnoredSplats.Count), IgnoredSplats, IgnoredSplats.GetType(), this);
        }
    }
}