using System;
using System.Collections.Generic;
using Dingo.Common;
using Dingo.Terrains;
using Dingo.WorldStamp;
using Dingo.Common.GenericEditor;
using UnityEngine;
using Random = System.Random;

namespace Dingo.Roads.Connections
{
    [Serializable]
    public class PlaceTerrainObjects : ConnectionComponent, IOnBakeCallback
    {
        [Name("Terrain/Place Objects")]
        public class Config : ConnectionConfigurationBase
        {
            public List<GameObject> Objects = new List<GameObject>();
            public FloatMinMax Distance = new FloatMinMax(1, 1);
            public float InitialOffset = 0;
            public int MaxInstancesPerConnection = 10;

            public FloatMinMax SplineOffset = new FloatMinMax(0, 0);
            public Vec3MinMax Scale = new Vec3MinMax(Vector3.one, Vector3.one);
            public bool UniformScale = true;
            public Vec3MinMax Rotation;
            public Vec3MinMax LocalOffset;
            public Vector3 SplineRotation;
            public bool Mirror;
            public float Chance = 1;
            public bool AbsoluteHeight = true;

            public override Type GetMonoType()
            {
                return typeof(PlaceTerrainObjects);
            }
        }

        public void OnBake()
        {
            var config = Configuration.GetConfig<Config>();
            if (config == null)
            {
                Debug.LogError("Invalid config for ConnectionObjectComponent " + name, this);
                return;
            }

            if (config.Objects == null || config.Objects.Count == 0)
            {
                return;
            }

            var seed = NodeConnection.ThisNode.Seed;
            var rand = new Random(seed);
            
            var wrappers = TerrainLayerUtilities.CollectWrappers(NodeConnection.GetSpline().GetApproximateXZObjectBounds());
            foreach (var terrainWrapper in wrappers)
            {
                ProcessWrapper(terrainWrapper, config, rand);
            }
        }

        private void ProcessWrapper(TerrainWrapper wrapper, Config config, Random rand)
        {
            var spline = NodeConnection.GetSpline();
            var length = spline.Length;
            var step = config.Distance;
            var layer = Network.GetLayer(wrapper);
            var tSize = wrapper.Terrain.terrainData.size;

            for (var i = config.InitialOffset; i < length; i += step.GetRand(rand))
            {
                var randIndex = Mathf.FloorToInt((float)rand.NextDouble()*config.Objects.Count);
                var prefab = config.Objects[randIndex];

                var roll = (float) rand.NextDouble();
                if (roll > config.Chance)
                {
                    continue;
                }

                var offset = config.LocalOffset.GetRand(rand);
                var scale = config.Scale.GetRand(rand);
                if (config.UniformScale)
                {
                    scale = Vector3.one*scale.x;
                }

                var uniformT = i/length;
                var splineOffset = config.SplineOffset.GetRand(rand) * (rand.Flip() ? -1 : 1);
                var tangent = spline.GetTangent(uniformT).normalized;
                var rotation = Quaternion.LookRotation(tangent) * Quaternion.Euler(config.Rotation.GetRand(rand));

                var worldPos = spline.GetUniformPointOnSpline(uniformT);
                worldPos += tangent * splineOffset;
                worldPos += offset;

                if (!config.AbsoluteHeight)
                {
                    worldPos.y = offset.y;
                }

                var tPos = worldPos - wrapper.transform.position;
                tPos = new Vector3(tPos.x / tSize.x, tPos.y, tPos.z / tSize.z);
                
                var prefabObject = new PrefabObjectData()
                {
                    AbsoluteHeight = config.AbsoluteHeight,
                    Guid = Guid.NewGuid().ToString(),
                    Position = tPos,
                    Scale = scale,
                    Rotation = rotation.eulerAngles, 
                    Prefab = prefab
                };

                layer.Objects.Add(prefabObject);
            }
        }
    }
}