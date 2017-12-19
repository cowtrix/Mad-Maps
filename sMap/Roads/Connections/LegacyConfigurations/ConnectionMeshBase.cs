using sMap.Common;
using UnityEngine;

namespace sMap.Roads
{
    public abstract class ConnectionMeshBase : ConnectionComponentConfiguration
    {
        public Mesh SourceMesh;
        [Range(0, 1)]
        public float SnapStrength = 0.05f;
        public MeshTools.Axis Axis;
        public Vector3 Scale = new Vector3(1, 1, 1);
        public Vector3 Offset = new Vector3(0, 0, 0);
        [Tooltip("Rotation around the spline")]
        public Vector3 Rotation = new Vector3(0, 0, 0);
    }
}