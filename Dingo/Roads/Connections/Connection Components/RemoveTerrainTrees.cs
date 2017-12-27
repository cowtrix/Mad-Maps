using System;
using Dingo.Common;
using Dingo.Terrains;
using Dingo.Common.GenericEditor;
using UnityEngine;

namespace Dingo.Roads.Connections
{
    public class RemoveTerrainTrees : ConnectionComponent, IOnBakeCallback
    {
        [Name("Terrain/Remove Trees")]
        public class Config : ConnectionConfigurationBase
        {
            public float TreeRemoveDistance = 1;
            public string RegexMatch;

            public override Type GetMonoType()
            {
                return typeof(RemoveTerrainTrees);
            }
        }

        public void OnBake()
        {
            if (!Network.RecalculateTerrain)
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
            foreach (var wrapper in terrainWrappers)
            {
                var layer = Network.GetLayer(wrapper, true);
                layer.BlendMode = TerrainLayer.ETerrainLayerBlendMode.Stencil;
                ProcessTerrainTrees(config, wrapper, layer);
            }
        }

        private void ProcessTerrainTrees(Config config, TerrainWrapper wrapper, TerrainLayer layer)
        {
            var mainSpline = NodeConnection.GetSpline();
            var radius = config.TreeRemoveDistance;

            // Create bounds to encapsulate spline (axis aligned)
            var bounds = mainSpline.GetApproximateBounds();
            bounds.Expand(radius * 2 * Vector3.one);

            // Create object bounds
            var objectBounds = mainSpline.GetApproximateXZObjectBounds();
            objectBounds.Expand(radius * 2 * Vector3.one);
            objectBounds.Expand(Vector3.up * 10000);

            var flatCullbounds = objectBounds.ToAxisBounds();
            //DebugHelper.DrawCube(flatCullbounds.center, flatCullbounds.extents, Quaternion.identity, Color.yellow, 20);
            //DebugHelper.DrawCube(objectBounds.center, objectBounds.extents, objectBounds.Rotation, Color.cyan, 20);

            Plane startPlane, endPlane;
            GenerateSplinePlanes(0, mainSpline, out startPlane, out endPlane);

            var trees = wrapper.GetCompoundTrees(layer, true, flatCullbounds);
            for (int i = trees.Count - 1; i >= 0; i--)
            {
                var treeInstance = trees[i];
                var wPos = wrapper.Terrain.TreeToWorldPos(treeInstance.Position);
                if (!objectBounds.Contains(wPos) || !GeometryExtensions.BetweenPlanes(wPos, startPlane, endPlane))
                {
                    continue;
                }

                var ut = mainSpline.GetClosestUniformTimeOnSplineXZ(wPos.xz());
                var splinePos = mainSpline.GetUniformPointOnSpline(ut);
                var d = splinePos.xz() - wPos.xz();

                if (d.sqrMagnitude < config.TreeRemoveDistance * config.TreeRemoveDistance && !layer.TreeRemovals.Contains(treeInstance.Guid))
                {
                    layer.TreeRemovals.Add(treeInstance.Guid);
                }
            }
        }

        private void GenerateSplinePlanes(float planeGive, SplineSegment mainSpline, out Plane startPlane, out Plane endPlane)
        {
            var startNodePos = mainSpline.FirstControlPoint.Position;
            var startNodeControl = NodeConnection.ThisNode.GetNodeControl(NodeConnection.NextNode).Flatten().normalized;
            startPlane = new Plane(startNodeControl, startNodePos + startNodeControl * planeGive);
            var endNodePos = mainSpline.SecondControlPoint.Position;
            var endNodeControl = NodeConnection.NextNode.GetNodeControl(NodeConnection.ThisNode).Flatten().normalized;
            endPlane = new Plane(endNodeControl, endNodePos + endNodeControl * planeGive);
        }
    }
}