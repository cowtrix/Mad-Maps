using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace MadMaps.Common.Painter
{

#if UNITY_EDITOR

    [ExecuteInEditMode]
    public class EditorCellRenderer : MonoBehaviour
    {
        public int VertCount
        {
            get { return _verts.Count; }
        }

        public Mesh Mesh;
        public MeshFilter MeshFilter;
        public MeshRenderer Renderer;
        private List<Vector3> _verts = new List<Vector3>();
        private List<int> _tris = new List<int>();
        private List<Color> _colors = new List<Color>();
        private List<Vector2> _uvs = new List<Vector2>();
        double _lastAliveTime;

        public void SetAlive()
        {
            #if UNITY_EDITOR
            _lastAliveTime = UnityEditor.EditorApplication.timeSinceStartup;
            #else
            _lastAliveTime = (double)Time.time;
            #endif
        }

        public void Initialise()
        {
            Profiler.BeginSample("Initialise");
            if (Mesh == null)
            {
                Mesh = new Mesh();
                Mesh.hideFlags = HideFlags.DontSave;
                Mesh.MarkDynamic();
            }
            if (MeshFilter == null)
            {
                MeshFilter = gameObject.AddComponent<MeshFilter>();
                MeshFilter.sharedMesh = Mesh;
            }
            if (Renderer == null)
            {
                Renderer = gameObject.AddComponent<MeshRenderer>();
            }
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            EditorCellHelper.Register(this);
            Profiler.EndSample();
        }

        public void QueueCell(EditorCellHelper.Cell cell)
        {
            Profiler.BeginSample("QueueCell");
            if(EditorCellHelper.UseCPU)
            {
                var cellSize = EditorCellHelper.CellSize;
                int index = _verts.Count;
                _verts.Add(cell.Center + new Vector3(cellSize/2, 0, cellSize/2));
                _verts.Add(cell.Center + new Vector3(-cellSize/2, 0, cellSize/2));
                _verts.Add(cell.Center + new Vector3(cellSize/2, 0, -cellSize/2));
                _verts.Add(cell.Center + new Vector3(-cellSize/2, 0, -cellSize/2));
                _uvs.Add(new Vector2(1, 1));
                _uvs.Add(new Vector2(0, 1));
                _uvs.Add(new Vector2(1, 0));
                _uvs.Add(new Vector2(0, 0));
                _colors.Add(cell.Color);
                _colors.Add(cell.Color);
                _colors.Add(cell.Color);
                _colors.Add(cell.Color);
                _tris.Add(index + 0);
                _tris.Add(index + 1);
                _tris.Add(index + 2);
                _tris.Add(index + 1);
                _tris.Add(index + 2);
                _tris.Add(index + 3);
            }
            else
            {
                _verts.Add(cell.Center);
                _tris.Add(_verts.Count - 1);
                _tris.Add(_verts.Count - 1);
                _tris.Add(_verts.Count - 1);
                _colors.Add(cell.Color);
            }
            Profiler.EndSample();
        }

        public void Clear()
        {
            Profiler.BeginSample("Clear");
            if (Mesh)
            {
                Mesh.Clear();
            }
            _verts.Clear();
            _tris.Clear();
            _colors.Clear();
            _uvs.Clear();
            Profiler.EndSample();
        }

        public void Finalise()
        {
            Profiler.BeginSample("Finalise");
            Initialise();
            Mesh.Clear();
            Mesh.SetVertices(_verts);
            Mesh.SetTriangles(_tris, 0);
            Mesh.SetColors(_colors);
            if(EditorCellHelper.UseCPU)
            {
                Mesh.SetUVs(0, _uvs);
            }
            Profiler.EndSample();
        }

        public void SetData(Matrix4x4 trs, Material material)
        {
            Profiler.BeginSample("SetData");
            Initialise();
            transform.ApplyTRSMatrix(trs);
            Renderer.sharedMaterial = material;
            Profiler.EndSample();
            //Debug.LogFormat("Position: {0} Scale {1} Rotation {2}", trs.GetPosition(), trs.GetScale(), trs.GetRotation().eulerAngles);
        }

        public void Update()
        {
            Profiler.BeginSample("Update");
            EditorCellHelper.Register(this);
        #if UNITY_EDITOR
            var t = UnityEditor.EditorApplication.timeSinceStartup;
        #else
            var t = (double)Time.time;
        #endif
            if (_lastAliveTime + EditorCellHelper.AutoClearTime < t)
            {
                if(Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
            Profiler.EndSample();
        }

        public void OnDestroy()
        {
            Mesh.TryDestroyImmediate();
        }
    }
#endif
}