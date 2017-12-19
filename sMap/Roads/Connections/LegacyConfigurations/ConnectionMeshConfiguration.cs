using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace sMap.Roads
{
    //[CreateAssetMenu(menuName = "sRoads/Connection Mesh Configuration")]
    public class ConnectionMeshConfiguration : ConnectionMeshBase
    {
        public bool OverrideNormal;
        public Vector3 NormalOverride = Vector3.up;

        public ShadowCastingMode ShadowCastingMode = ShadowCastingMode.On;

        public bool Static;
        public bool Mirror;
        public Material Material;

        [Range(0, 180)]
        public float PhongBreakAngle = 45;
        
        public bool CopyToCollider;
        public int Layer = 21;

        public float LoDDistance = 1f;
        
        public override Type GetMonoType()
        {
            throw new NotImplementedException();
        }
    }
}