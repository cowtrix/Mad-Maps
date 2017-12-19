using System;
using System.Text.RegularExpressions;
using ParadoxNotion.Design;
using sMap.Common;
using sMap.Terrains;
using UnityEngine;
using UnityEngine.Profiling;

namespace sMap.Roads
{
    public class RemoveTerrainObjects : ConnectionComponent, IOnBakeCallback
    {
        [Name("Terrain/Remove Objects")]
        public class Config : ConnectionConfigurationBase
        {
            //[Header("Trees")]
            //public float TreeRemoveDistance = 1;

            [Header("Objects")]
            public LayerMask ObjectRemovalMask = 1 << 21;
            public float ObjectRemoveDistance = 1;
            public string RegexMatch;

            public override Type GetMonoType()
            {
                return typeof(RemoveTerrainObjects);
            }
        }

        public bool ShowDebug;

        public void OnBake()
        {
            if (!RoadNetwork.LevelInstance.RecalculateTerrain)
            {
                return;
            }
            if (Configuration == null)
            {
                return;
            }
            var config = Configuration.GetConfig<Config>();
            if (config == null)
            {
                Debug.LogError("Invalid configuration! Expected ConnectionTerrainHeightConfiguration");
                return;
            }

            var terrainWrappers = TerrainLayerUtilities.CollectWrappers(NodeConnection.GetSpline().GetApproximateXZObjectBounds());
            for (int i = 0; i < terrainWrappers.Count; i++)
            {
                var wrapper = terrainWrappers[i];
                var layer = RoadNetwork.LevelInstance.GetLayer(wrapper, true);
                ProcessTerrainObjects(config, wrapper, layer);
            }
        }

        private void ProcessTerrainObjects(Config config, TerrainWrapper wrapper, TerrainLayer layer)
        {
            var spline = NodeConnection.GetSpline();
            var bounds = spline.GetApproximateXZObjectBounds();
            bounds.Expand(new Vector3(config.ObjectRemoveDistance, 1000, config.ObjectRemoveDistance));

            var tPos = wrapper.transform.position;
            var tSize = wrapper.Terrain.terrainData.size;

            var regex = new Regex(config.RegexMatch ?? string.Empty);

            var objects = wrapper.GetCompoundObjects(layer, true);
            foreach (var data in objects)
            {
                if (data.Prefab == null || !regex.IsMatch(data.Prefab.name))
                {
                    continue;
                }

                var wPos = tPos +
                           new Vector3(data.Position.x * tSize.x, data.Position.y, data.Position.z * tSize.z);
                if (!bounds.Contains(wPos))
                {
                    Debug.DrawLine(wPos, wPos + Vector3.up * 10, Color.cyan, 20);
                    continue;
                }

                var ut = spline.GetClosestUniformTimeOnSplineXZ(wPos.xz());
                var pointOnSpline = spline.GetUniformPointOnSpline(ut);

                var dist = (pointOnSpline.xz() - wPos.xz()).magnitude;
                if (dist < config.ObjectRemoveDistance)
                {
                    if (!layer.ObjectRemovals.Contains(data.Guid))
                    {
                        layer.ObjectRemovals.Add(data.Guid);
                    }
                    if (ShowDebug)
                        Debug.DrawLine(pointOnSpline, wPos, Color.red, 20);
                }
                else
                {
                    if (ShowDebug)
                        Debug.DrawLine(pointOnSpline, wPos, Color.green, 20);
                }
            }
        }
    }
}