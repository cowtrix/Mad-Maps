using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using sMap.Common;
using UnityEngine;

namespace sMap.Roads.Connections
{
    /*[Serializable]
    public class CreateMeshCollider : ConnectionComponent, IOnBakeCallback
    {
        [Name("Mesh/Collider")]
        public class Config : ConnectionMeshConfigBase
        {
            public int Layer;
            public PhysicMaterial Material;
            public bool Convex;

            public override Type GetMonoType()
            {
                return typeof(CreateMeshCollider);
            }
        }

        public Vector2 SplineRange = new Vector2(0, 1);
        private MeshCollider _meshCollider;
        private Mesh _meshResult;

        public void OnRebake()
        {
            if (Configuration == null)
            {
                return;
            }

            var config = Configuration.GetConfig<Config>();
            if (config == null)
            {
                Debug.LogError("Invalid configuration! Expected RoadMeshObjectConfiguration");
                return;
            }

            if (_meshResult != null)
            {
                DestroyImmediate(_meshResult);
            }
            if (config.SourceMesh == null)
            {
                return;
            }

            var dataContainer = NodeConnection.GetDataContainer();
            _meshCollider = dataContainer.GetOrAddComponent<MeshCollider>();
            _meshCollider.enabled = false;

            List<Mesh> workingMeshes = new List<Mesh>();

            var spline = NodeConnection.GetSpline();
            var totalSplineLength = spline.Length;
            var scale = config.SourceMesh.bounds.size;
            scale.Scale(config.Scale);

            var sourceMeshAxisLength = MeshTools.GetScalarFromAxis(scale, config.Axis);
            var perMeshLength = Math.Min(totalSplineLength, sourceMeshAxisLength);
            var perMeshPercentage = perMeshLength / totalSplineLength;
            var matrix = Matrix4x4.TRS(config.Offset, Quaternion.identity, Vector3.one);

            for (var i = SplineRange.x; i < 1; i += perMeshPercentage)
            {
                var verts = new List<Vector3>(config.SourceMesh.vertices);
                var tris = new List<int>(config.SourceMesh.GetTriangles(0));
                var uvs = new List<Vector4>();
                config.SourceMesh.GetUVs(0, uvs);

                HashSet<int> removedVerts = new HashSet<int>();
                MeshTools.ProcessRemovedVerts(removedVerts, tris, uvs);
                foreach (var removedVert in removedVerts)
                {
                    verts[removedVert] = spline.GetApproximateBounds().center;
                }

                var workingMesh = new Mesh();
                workingMesh.vertices = verts.ToArray();
                workingMesh.triangles = tris.ToArray();
                workingMesh.SetUVs(0, uvs);

                workingMesh.RecalculateNormals();
                workingMesh.RecalculateBounds();

                workingMeshes.Add(workingMesh);
            }

            CombineInstance[] combineInstances = new CombineInstance[workingMeshes.Count];
            for (int i = 0; i < workingMeshes.Count; i++)
            {
                combineInstances[i] = new CombineInstance()
                {
                    mesh = workingMeshes[i],
                    transform = matrix,
                };
            }
            
            _meshResult = new Mesh();
            _meshResult.CombineMeshes(combineInstances);
            _meshResult.RecalculateBounds();

            for (int i = 0; i < workingMeshes.Count; i++)
            {
                DestroyImmediate(workingMeshes[i]);
            }

            _meshCollider.sharedMesh = _meshResult;
        }

        public override void Destroy()
        {
            DestroyImmediate(_meshResult);
            DestroyImmediate(_meshCollider);
            base.Destroy();
        }
    }*/
}