using System;
using MadMaps.Common;
using MadMaps.Common.GenericEditor;
using MadMaps.Terrains;
using UnityEngine;

namespace MadMaps.Roads.Connections
{
    public class RemoveVegetationStudioInstancess : ConnectionComponent, IOnBakeCallback
    {
        [Name("Vegetation Studio/Remove Vegetation")]
        public class Config : ConnectionConfigurationBase
        {
            public float InstanceRemoveDistance = 1;
            public string RegexMatch;

            public override Type GetMonoType()
            {
                return typeof(RemoveVegetationStudioInstancess);
            }
        }

        public void OnBake()
        {
            if (!Network)
            {
                Debug.LogError("Unable to find network! " + name, this);
                return;
            }
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
            var radius = config.InstanceRemoveDistance;

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

            var vsData = wrapper.GetCompoundVegetationStudioData(layer, true, flatCullbounds);
            for (int i = vsData.Count - 1; i >= 0; i--)
            {
                var vsInstance = vsData[i];
                var wPos = wrapper.Terrain.TreeToWorldPos(vsInstance.Position);
                if (!objectBounds.Contains(wPos) || !GeometryExtensions.BetweenPlanes(wPos, startPlane, endPlane))
                {
                    continue;
                }

                var ut = mainSpline.GetClosestUniformTimeOnSplineXZ(wPos.xz());
                var splinePos = mainSpline.GetUniformPointOnSpline(ut);
                var d = splinePos.xz() - wPos.xz();

                if (d.sqrMagnitude < config.InstanceRemoveDistance * config.InstanceRemoveDistance && !layer.VSRemovals.Contains(vsInstance.Guid))
                {
                    layer.VSRemovals.Add(vsInstance.Guid);
                    //DebugHelper.DrawPoint(wPos, .5f, Color.red, 20);
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