using System;
using UnityEngine;

namespace sMap.Roads
{
    //[CreateAssetMenu(menuName = "sRoads/Connection Collider Configuration")]
    public class ConnectionColliderConfiguration : ConnectionMeshBase
    {
        public int Layer;
        public PhysicMaterial Material;
        public bool Convex;
        
        public override Type GetMonoType()
        {
            throw new NotImplementedException();
        }
    }
}