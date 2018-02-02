using UnityEditor;
using UnityEngine;

namespace MadMaps.Common
{
    public static class FindProblemMeshes
    {
        [MenuItem("Tools/Mad Maps/Utilities/Find Problem Meshes")]
        public static void Find()
        {
            var allMeshColliders = UnityEngine.Object.FindObjectsOfType<MeshCollider>();
            foreach (var collider in allMeshColliders)
            {
                if (collider.sharedMesh == null)
                {
                    Debug.Log("MeshCollider with null mesh: " + collider.name, collider);
                    continue;
                }
                if (collider.sharedMesh.bounds.size.magnitude == 0)
                {
                    Debug.Log("MeshCollider with bounds size 0 magnitude: " + collider.name, collider);
                    continue;
                }
                if (collider.sharedMesh.bounds.size.x == 0 ||
                    collider.sharedMesh.bounds.size.y == 0 ||
                    collider.sharedMesh.bounds.size.z == 0)
                {
                    Debug.Log("MeshCollider with bounds size 0 component: " + collider.name, collider);
                    continue;
                }
            }
        }
    }
}