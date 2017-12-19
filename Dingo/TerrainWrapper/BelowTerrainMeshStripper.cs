using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dingo.Terrains
{
    public class BelowTerrainMeshStripper : MonoBehaviour
#if HURTWORLDSDK
        , ILevelPreBuildStepCallback
#endif
    {
        [ContextMenu("Strip")]
        public void Strip()
        {
            var t = Terrain.activeTerrain;
            var mf = GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in mf)
            {
                meshFilter.sharedMesh = StripMesh(t, meshFilter.sharedMesh, meshFilter.transform);
                if (meshFilter.sharedMesh.triangles.Length == 0)
                {
                    DestroyImmediate(meshFilter.sharedMesh);
                    DestroyImmediate(meshFilter);
                }
            }

            var mc = GetComponentsInChildren<MeshCollider>();
            for (int i = 0; i < mc.Length; i++)
            {
                var meshCollider = mc[i];
                meshCollider.sharedMesh = StripMesh(t, meshCollider.sharedMesh, meshCollider.transform);
                if (meshCollider.sharedMesh.triangles.Length == 0)
                {
                    DestroyImmediate(meshCollider.sharedMesh);
                    DestroyImmediate(meshCollider);
                }
            }
        }

        [Pure]
        private Mesh StripMesh(Terrain t, Mesh sourceMesh, Transform transform)
        {
            var mesh = sourceMesh.Clone();

            List<Vector3> verts = new List<Vector3>();
            mesh.GetVertices(verts);

            HashSet<int> validVertices = new HashSet<int>();
            for (int i = 0; i < verts.Count; i++)
            {
                var vector3 = verts[i];
                var wPos = transform.TransformPoint(vector3);
                var tHeight = t.transform.position.y + t.SampleHeight(wPos);
                if (wPos.y > tHeight)
                {
                    validVertices.Add(i);
                }
            }

            List<int> tris = new List<int>();
            List<int> trianglesToRemove = new List<int>();
            mesh.GetTriangles(tris, 0);
            for (int i = 0; i < tris.Count; i += 3)
            {
                var tri1 = tris[i + 0];
                var tri2 = tris[i + 1];
                var tri3 = tris[i + 2];
                if (!validVertices.Contains(tri1) && !validVertices.Contains(tri2) && !validVertices.Contains(tri3))
                {
                    trianglesToRemove.Add(i);
                }
            }

            trianglesToRemove.Sort();
            for (int i = trianglesToRemove.Count - 1; i >= 0; i--)
            {
                var triIndex = trianglesToRemove[i];
                //var offset = i*3;
                var offset = 0;
                try
                {
                    tris.RemoveAt(triIndex + 2 - offset);
                    tris.RemoveAt(triIndex + 1 - offset);
                    tris.RemoveAt(triIndex + 0 - offset);
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw;
                }
            }

            mesh.SetTriangles(tris, 0, true);
#if UNITY_EDITOR
            MeshUtility.SetMeshCompression(mesh, ModelImporterMeshCompression.High);
#endif
           
            return mesh;
        }

        public void OnLevelPreBuildStep()
        {
            Strip();
            DestroyImmediate(this);
        }
    }
}