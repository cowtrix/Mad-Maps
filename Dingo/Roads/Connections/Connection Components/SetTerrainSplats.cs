using System;
using System.Collections.Generic;
using Dingo.Common;
using Dingo.Common.Collections;
using Dingo.Terrains;
using ParadoxNotion.Design;
using UnityEngine;

namespace Dingo.Roads.Connections
{
    public class SetTerrainSplats : ConnectionComponent, IOnBakeCallback
    {
        [Name("Terrain/Set Splats")]
        public class Config : ConnectionConfigurationBase
        {
            [Serializable]
            public class SplatConfig
            {
                public SplatPrototypeWrapper SplatPrototype;
                public float SplatStrength = 1;
            }

            public float Radius = 10;
            public AnimationCurve SplatFalloff = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 1), new Keyframe(1, 0));
            public List<SplatConfig> SplatConfigurations = new List<SplatConfig>();

            public override Type GetMonoType()
            {
                return typeof(SetTerrainSplats);
            }
        }

        public const int SplatOffset = 2;

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
                ProcessTerrainSplat(config, wrapper, layer);
            }
        }
        
        private void ProcessTerrainSplat(Config config, TerrainWrapper wrapper, TerrainLayer layer)
        {
            var terrain = wrapper.Terrain;
            var splatRes = wrapper.Terrain.terrainData.alphamapResolution;
            var mainSpline = NodeConnection.GetSpline();
            var radius = config.Radius;

            // Create bounds to encapsulate spline (axis aligned)
            var bounds = mainSpline.GetApproximateBounds();
            bounds.Expand(radius * 2 * Vector3.one);

            // Create object bounds
            var objectBounds = mainSpline.GetApproximateXZObjectBounds();
            objectBounds.Expand(radius * 2 * Vector3.one);
            objectBounds.Expand(Vector3.up * 10000);

            // Early cull
            var axisBounds = objectBounds.ToAxisBounds();
            var terrainBounds = wrapper.Terrain.GetComponent<Collider>().bounds;
            if (!terrainBounds.Intersects(axisBounds))
            {
                return;
            }

            float planeGive = -(wrapper.Terrain.terrainData.size.x / wrapper.Terrain.terrainData.alphamapResolution) * SplatOffset;
            Plane startPlane, endPlane;
            GenerateSplinePlanes(planeGive, mainSpline, out startPlane, out endPlane);

            // Get matrix space min/max
            var matrixMin = terrain.WorldToSplatCoord(bounds.min);
            var matrixMax = terrain.WorldToSplatCoord(bounds.max);

            matrixMin = new Common.Coord(Mathf.Clamp(matrixMin.x, 0, terrain.terrainData.alphamapResolution), Mathf.Clamp(matrixMin.z, 0, terrain.terrainData.alphamapResolution));
            matrixMax = new Common.Coord(Mathf.Clamp(matrixMax.x, 0, terrain.terrainData.alphamapResolution), Mathf.Clamp(matrixMax.z, 0, terrain.terrainData.alphamapResolution));

            var floatArraySize = new Common.Coord(matrixMax.x - matrixMin.x, matrixMax.z - matrixMin.z);

            // Get all the existing compound splats
            var currentPrototypes = wrapper.GetCompoundSplatPrototypes(layer, true);
            var baseData = layer.GetSplatMaps(matrixMin.x, matrixMin.z, floatArraySize.x, floatArraySize.z, splatRes);

            var stencilKey = GetStencilKey();
            Serializable2DFloatArray thisPatchStencil = new Serializable2DFloatArray(floatArraySize.x,
                floatArraySize.z);
            foreach (var splatConfiguration in config.SplatConfigurations)
            {
                var splatPrototypeWrapper = splatConfiguration.SplatPrototype;
                Serializable2DByteArray baseLayerSplat;
                if (!baseData.TryGetValue(splatPrototypeWrapper, out baseLayerSplat))
                {
                    baseLayerSplat = new Serializable2DByteArray(floatArraySize.x, floatArraySize.z);
                    baseData[splatPrototypeWrapper] = baseLayerSplat;
                }

                for (var dz = 0; dz < floatArraySize.z; ++dz)
                {
                    for (var dx = 0; dx < floatArraySize.x; ++dx)
                    {
                        var coordX = matrixMin.x + dx;
                        var coordZ = matrixMin.z + dz;

                        float thisComponentStencilStrength;

                        var stencilCoord = new Vector2(coordX / (float)splatRes, coordZ / (float)splatRes);
                        layer.Stencil.StencilBilinearSample(stencilCoord, stencilKey, out thisComponentStencilStrength);
                        thisPatchStencil[dx, dz] = thisComponentStencilStrength > 0 ? 1 : 0;
                        if (thisComponentStencilStrength <= 0.01f || config.SplatConfigurations.Count == 0)
                        {
                            continue;
                        }

                        var worldPos = terrain.SplatCoordToWorldPos(new Common.Coord(coordX, coordZ));
                        worldPos = new Vector3(worldPos.x, objectBounds.center.y, worldPos.z);

                        if (terrain.ContainsPointXZ(worldPos) &&
                            objectBounds.Contains(worldPos) &&
                            GeometryExtensions.BetweenPlanes(worldPos, startPlane, endPlane))
                        {
                            var uniformT = mainSpline.GetClosestUniformTimeOnSplineXZ(worldPos.xz()); // Expensive!
                            var closestOnSpline = mainSpline.GetUniformPointOnSpline(uniformT);
                            var normalizedFlatDistToSpline =
                                Mathf.Clamp01((worldPos - closestOnSpline).xz().magnitude / (config.Radius));
                            var maskValue = config.SplatFalloff.Evaluate(normalizedFlatDistToSpline);

                            if (maskValue <= 0)
                            {
                                continue;
                            }

                            var writeFloatValue = splatConfiguration.SplatStrength * maskValue;
                            var writeValue = (byte)Mathf.Clamp(writeFloatValue * 255f, 0, 255);
                            var mainRead = baseLayerSplat[dx, dz];
                            writeValue = (byte)Mathf.Max(writeValue, mainRead);

                            foreach (var currentPrototype in currentPrototypes)
                            {
                                if (!baseData.ContainsKey(currentPrototype))
                                {
                                    continue;
                                }
                                var otherSplatFloatValue = baseData[currentPrototype][dx, dz] / 255f;
                                var otherSplatFloatWriteVal = (otherSplatFloatValue * (1 - writeFloatValue));
                                var write = (byte)Mathf.Clamp(otherSplatFloatWriteVal * 255, 0, 255);
                                baseData[currentPrototype][dx, dz] = write;
                            }

                            var newVal = (byte)Mathf.Clamp(mainRead + writeValue, 0, 255);
                            baseLayerSplat[dx, dz] = newVal;
                        }
                        else
                        {
                            thisPatchStencil[dx, dz] = 0;
                        }
                    }
                }
            }

            foreach (var existingSplatPrototype in baseData)
            {
                var splat = existingSplatPrototype.Key;
                var data = existingSplatPrototype.Value;
                layer.SetSplatmap(splat, matrixMin.x, matrixMin.z, data, wrapper.Terrain.terrainData.alphamapResolution, thisPatchStencil);
            }
        }

        private int GetStencilKey()
        {
            return GetPriority();
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

    }
}