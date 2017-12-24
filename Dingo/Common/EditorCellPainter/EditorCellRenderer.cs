using System.Collections.Generic;
using UnityEngine;

namespace Dingo.Common.Painter
{
#if UNITY_EDITOR && !HURTWORLDSDK
public class EditorCellRenderer : MonoBehaviour
{
    public int VertCount { get { return _verts.Count; } }

    public Mesh Mesh;
    public MeshFilter MeshFilter;
    public MeshRenderer Renderer;
    private List<Vector3> _verts = new List<Vector3>();
    private List<int> _tris = new List<int>();
    private List<Color> _colors = new List<Color>();

    public void Initialise()
    {
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
        EditorCellHelper.Register(this);
        gameObject.hideFlags = HideFlags.HideAndDontSave;
    }

    public void QueueCell(EditorCellHelper.Cell cell)
    {
        _verts.Add(cell.Center);
        _tris.Add(_verts.Count - 1);
        _tris.Add(_verts.Count - 1);
        _tris.Add(_verts.Count - 1);
        _colors.Add(cell.Color);
    }

    public void Clear()
    {
        if (Mesh)
        {
            Mesh.Clear();
        }
        _verts.Clear();
        _tris.Clear();
        _colors.Clear();
    }

    public void Finalise()
    {
        Initialise();
        Mesh.Clear();
        Mesh.SetVertices(_verts);
        Mesh.SetTriangles(_tris, 0);
        Mesh.SetColors(_colors);
    }

    public void SetData(Matrix4x4 trs, Material material)
    {
        Initialise();
        transform.ApplyTRSMatrix(trs);
        Renderer.sharedMaterial = material;
    }

    public void Update()
    {
        EditorCellHelper.Register(this);
    }

    public void OnDestroy()
    {
        Mesh.TryDestroyImmediate();
    }
}
#endif
}