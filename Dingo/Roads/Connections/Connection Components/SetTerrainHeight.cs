using System;
using Dingo.Common;
using Dingo.Common.Collections;
using Dingo.Terrains;
using Dingo.Common.GenericEdtitor;
using UnityEngine;

namespace Dingo.Roads.Connections
{
    public class SetTerrainHeight : ConnectionComponent, IOnBakeCallback
    {
        [Name("Terrain/Set Height")]
        public class Config : ConnectionConfigurationBase
        {
            public AnimationCurve Falloff = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 1), new Keyframe(1, 0));
            public AnimationCurve Height = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
            public float Radius = 10;

            public override Type GetMonoType()
            {
                return typeof(SetTerrainHeight);
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
                ProcessTerrainHeight(config, wrapper, layer);
            }
        }

        private int GetStencilKey()
        {
            return Mathf.Max(1, GetPriority());
            //return Network.Nodes.IndexOf(NodeConnection.ThisNode) + 1;
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

        private void ProcessTerrainHeight(Config config, TerrainWrapper wrapper, TerrainLayer layer)
        {
            var terrain = wrapper.Terrain;
            var terrainPos = wrapper.Terrain.GetPosition();
            var terrainSize = wrapper.Terrain.terrainData.size;
            var heightRes = terrain.terrainData.heightmapResolution;

            var mainSpline = NodeConnection.GetSpline();
            var radius = config.Radius;
            var falloffCurve = config.Falloff;
            var heightCurve = config.Height;

            // Create bounds to encapsulate spline (axis aligned)
            var bounds = mainSpline.GetApproximateBounds();
            bounds.Expand(radius * 2 * Vector3.one);

            // Create object bounds
            var objectBounds = mainSpline.GetApproximateXZObjectBounds();
            objectBounds.Expand(radius * 2 * Vector3.one);
            objectBounds.Expand(Vector3.up * 10000);

            // Early cull
            var axisBounds = objectBounds.ToAxisBounds();
            var terrainBounds = terrain.GetComponent<Collider>().bounds;
            terrainBounds.Expand(Vector3.up * 10000);
            if (!terrainBounds.Intersects(axisBounds))
            {
                return;
            }

            // Get matrix space min/max
            var matrixMin = terrain.WorldToHeightmapCoord(bounds.min, TerrainX.RoundType.Floor);
            var matrixMax = terrain.WorldToHeightmapCoord(bounds.max, TerrainX.RoundType.Ceil);

            var xDelta = matrixMax.x - matrixMin.x;
            var zDelta = matrixMax.z - matrixMin.z;

            var floatArraySize = new Common.Coord(
                Mathf.Min(xDelta, terrain.terrainData.heightmapResolution - matrixMin.x),
                Mathf.Min(zDelta, terrain.terrainData.heightmapResolution - matrixMin.z));

            float planeGive = -(wrapper.Terrain.terrainData.size.x / wrapper.Terrain.terrainData.heightmapResolution);
            Plane startPlane, endPlane;
            GenerateSplinePlanes(planeGive, mainSpline, out startPlane, out endPlane);

            /*ar baseHeights = wrapper.GetCompoundHeights(layer, matrixMin.x, matrixMin.z, floatArraySize.x,
                floatArraySize.z, heightRes);*/
            var layerHeights = layer.GetHeights(matrixMin.x, matrixMin.z, floatArraySize.x, floatArraySize.z, heightRes) ??
                               new Serializable2DFloatArray(floatArraySize.x, floatArraySize.z);

            var stencilKey = GetStencilKey();
            if (layer.Stencil == null)
            {
                layer.Stencil = new Serializable2DFloatArray(heightRes, heightRes);
            }

            //DebugHelper.DrawCube(objectBounds.center, objectBounds.extents, objectBounds.Rotation, Color.blue, 20);
            Profiler.BeginSample("Main Loop");
            for (var dz = 0; dz < floatArraySize.z; ++dz)
            {
                for (var dx = 0; dx < floatArraySize.x; ++dx)
                {
                    var coordX = matrixMin.x + dx;
                    var coordZ = matrixMin.z + dz;

                    var worldPos = terrain.HeightmapCoordToWorldPos(new Common.Coord(coordX, coordZ));
                    worldPos = new Vector3(worldPos.x, objectBounds.center.y, worldPos.z);
                    if (!terrain.ContainsPointXZ(worldPos) ||
                        !objectBounds.Contains(worldPos) ||
                        !GeometryExtensions.BetweenPlanes(worldPos, startPlane, endPlane))
                    {
                        // Cull if we're outside of the approx bounds
                        continue;
                    }

                    var uniformT = mainSpline.GetClosestUniformTimeOnSplineXZ(worldPos.xz()); // Expensive!
                    var closestOnSpline = mainSpline.GetUniformPointOnSpline(uniformT);
                    var normalizedFlatDistToSpline = (worldPos - closestOnSpline).xz().magnitude / (radius);
                    if (normalizedFlatDistToSpline >= 1)
                    {
                        continue;
                    }

                    var maskValue = Mathf.Clamp01(falloffCurve.Evaluate(normalizedFlatDistToSpline));
                    //DebugHelper.DrawPoint(worldPos, 1, Color.Lerp(Color.black, Color.blue, maskValue), 30);

                    var heightDelta = heightCurve.Evaluate(normalizedFlatDistToSpline);

                    float existingStencilStrength;
                    int existingStencilKey;
                    MiscUtilities.DecompressStencil(layer.Stencil[coordX, coordZ], out existingStencilKey, out existingStencilStrength);

                    if (existingStencilKey != stencilKey &&
                        existingStencilKey > stencilKey &&
                        !(existingStencilStrength < maskValue && maskValue > 0))
                    {
                        continue;
                    }

                    // Refine our worldposition to be on the same XZ plane as the spline point
                    worldPos = new Vector3(worldPos.x, closestOnSpline.y, worldPos.z);

                    // Find the point on the spline closest to this given point
                    var naturalT = mainSpline.UniformToNaturalTime(uniformT);

                    // Get the upvec from the natural time
                    var up = mainSpline.GetUpVector(naturalT).normalized;

                    // Create a plane and cast against it
                    var plane = new Plane(up, closestOnSpline);

                    float dist = 0;
                    var castRay = new Ray(worldPos, Vector3.down);
                    plane.Raycast(castRay, out dist);
                    var castPoint = castRay.GetPoint(dist);
                    var heightAtPoint = (castPoint.y + heightDelta);

                    //DebugHelper.DrawPoint(worldPos.xz().x0z(heightAtPoint), .05f, Color.cyan, 20);
                    heightAtPoint -= terrainPos.y;
                    heightAtPoint /= terrainSize.y;
                    heightAtPoint = MiscUtilities.FloorToUshort(heightAtPoint);

                    var existingHeight = layerHeights[dx, dz];
                    layerHeights[dx, dz] = Mathf.Lerp(existingHeight, heightAtPoint, Mathf.Clamp01(maskValue));

                    /*if (existingStencilStrength > 0)
                    {
                        DebugHelper.DrawPoint(worldPos, 1, Color.yellow, 30);
                    }*/

                    var newRawStencilValue = MiscUtilities.CompressStencil(maskValue > existingStencilStrength ? stencilKey : existingStencilKey,
                        maskValue + existingStencilStrength);
                    layer.Stencil[coordX, coordZ] = newRawStencilValue;

                    /* MiscUtilities.DecompressStencil(layer.Stencil[coordX, coordZ], out existingStencilKey,
                        out existingStencilStrength);*/
                }
            }
            Profiler.EndSample();

            layer.SetHeights(matrixMin.x, matrixMin.z,
                layerHeights, wrapper.Terrain.terrainData.heightmapResolution);
        }
    }
}