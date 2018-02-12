using MadMaps.Common;
using UnityEngine;

namespace MadMaps.Roads
{
    /// <summary>
    /// A solution to the problem of embedding procedural meshes into prefabs.
    /// This will attempt to mirror the serialization of any assigned mesh
    /// and put it back when it detects that the mesh reference has been lost.
    /// </summary>
    [ExecuteInEditMode]
#if HURTWORLDSDK
    [StripComponentOnBuild]
#endif
    public class ProceduralMeshContainer : MonoBehaviour
    {
        public bool Collider;
        public SerializedMesh Mesh = new SerializedMesh();

        [SerializeField]
        [HideInInspector]
        private MeshFilter _meshFilter;

        [SerializeField]
        [HideInInspector]
        private MeshCollider _meshCollider;

        [SerializeField]
        [HideInInspector]
        private Mesh _lastMesh;

        public void Update()
        {
            if (_meshFilter == null)
            {
                _meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            }
            if (_meshCollider == null)
            {
                _meshCollider = gameObject.GetOrAddComponent<MeshCollider>();
            }

            if (_meshFilter.sharedMesh != null && _meshFilter.sharedMesh != _lastMesh)
            {
                Mesh.FromMesh(_meshFilter.sharedMesh);
                _lastMesh = _meshFilter.sharedMesh;
            }

            if (Mesh != null && !Mesh.IsEmpty && _meshFilter.sharedMesh == null)
            {
                _meshFilter.sharedMesh = Mesh.ToMesh(_meshFilter.sharedMesh);
                _lastMesh = _meshFilter.sharedMesh;
            }

            if (Collider)
            {
                _meshCollider.sharedMesh = _meshFilter.sharedMesh;
            }
        }
    }
}