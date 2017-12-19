using System;
using System.Collections.Generic;
using sMap.Common;
using UnityEngine;

namespace sMap.Roads
{
    //[CreateAssetMenu(menuName = "sRoads/Connection Object Configuration")]
    public class ConnectionObjectConfiguration : ConnectionComponentConfiguration
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
            throw new NotImplementedException();
        }
    }
}