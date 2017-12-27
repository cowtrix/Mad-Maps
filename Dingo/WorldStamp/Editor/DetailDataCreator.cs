using System.Collections.Generic;
using System.Linq;
using Dingo.Common;
using Dingo.Common.Collections;
using Dingo.Terrains;
using Dingo.Terrains.Lookups;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    public class DetailDataCreator : WorldStampCreatorLayer
    {
        [HideInInspector]
        public List<CompressedDetailData> DetailData = new List<CompressedDetailData>();

        public List<DetailPrototypeWrapper> IgnoredDetails = new List<DetailPrototypeWrapper>();

        protected override GUIContent Label
        {
            get { return new GUIContent("Details"); }
        }

        protected override bool HasDataPreview
        {
            get { return true; }
        }

        protected override void CaptureInternal(Terrain terrain, Bounds bounds)
        {
            DetailData.Clear();

            var min = terrain.WorldToDetailCoord(bounds.min);
            var max = terrain.WorldToDetailCoord(bounds.max);

            int width = max.x - min.x;
            int height = max.z - min.z;

            var prototypes = terrain.terrainData.detailPrototypes;
            var wrappers = TerrainLayerUtilities.ResolvePrototypes(prototypes);

            if (prototypes.Length != wrappers.Count)
            {
                Debug.LogError("Failed to collect details - possibly you have detail configs that aren't wrapper assets?");
                return;
            }

            for (var i = 0; i < prototypes.Length; ++i)
            {
                var wrapper = wrappers[prototypes[i]];
                if (IgnoredDetails.Contains(wrapper))
                {
                    continue;
                }

                var sample = terrain.terrainData.GetDetailLayer(min.x, min.z, width, height, i);
                var data = new byte[width, height];
                int sum = 0;
                for (var dx = 0; dx < width; ++dx)
                {
                    for (var dz = 0; dz < height; ++dz)
                    {
                        var sampleData = sample[dz, dx];
                        data[dx, dz] = (byte)sampleData;
                        sum += sampleData;
                    }
                }
                if (sum > 0)
                {
                    DetailData.Add(new CompressedDetailData { Wrapper = wrapper, Data = new Serializable2DByteArray(data) });
                }
                else
                {
                    Debug.Log(string.Format("Ignored detail {0} as it appeared to be empty.", wrapper.name));
                }
            }
        }

        protected override void PreviewInSceneInternal(WorldStampCreator parent)
        {
            var bounds = parent.Bounds;
            int counter = 0;
            int res = 32;
            foreach (var kvp in DetailData)
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
            DataInspector.SetData(DetailData.Select(x => x.Data).ToList(), DetailData.Select(x => x.Wrapper).ToList());
        }

        public override void Clear()
        {
            DetailData.Clear();
        }

        protected override void CommitInternal(WorldStampData data)
        {
            data.DetailData = DetailData.JSONClone();
        }

        protected override void OnExpandedGUI(WorldStampCreator parent)
        {
            //AutoEditorWrapper.ListEditorNicer(string.Format("Ignored Details ({0})", IgnoredDetails.Count), IgnoredDetails, IgnoredDetails.GetType(), this);
        }
    }
}