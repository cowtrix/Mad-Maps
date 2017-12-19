using sMap.Common;
using UnityEngine;

namespace sMap.WorldStamp
{
    public class WorldStampPreview
    {
        private Mesh _mesh;
        private Material _material;
        private WorldStamp _stamp;
        private int _lastFrame;

        public void Invalidate(WorldStamp stamp)
        {
            _stamp = stamp;
            if (_mesh == null)
            {
                _mesh = new Mesh();
            }
            if (_material == null)
            {
                _material = Resources.Load<Material>("WorldStamp/WorldStampPreviewMaterial");
            }
            var heights = stamp.Data.Heights;
            const int res = 32;
            var verts = new Vector3[(res+1) * (res+1)];
            var uv = new Vector2[verts.Length];
            var colors = new Color[verts.Length];
            int counter = 0;
            for (int u = 0; u <= res; u++)
            {
                var uF = u/(float) res;
                for (int v = 0; v <= res; v++)
                {
                    var vF = v / (float)res;
                    var samplePoint = stamp.HaveHeightsBeenFlipped ? new Vector2(uF, vF) : new Vector2(vF, uF);
                    var height = heights.BilinearSample(samplePoint);


                    var pos = new Vector3(uF * stamp.Data.Size.x, height * stamp.Data.Size.y, vF * stamp.Data.Size.z) - stamp.Data.Size.xz().x0z() / 2;
                    var val = stamp.GetMask().GetBilinear(stamp.Data.GridManager, new Vector3(pos.x, 0, pos.z) + stamp.Data.Size.xz().x0z() / 2);
                    pos.y *= val;

                    verts[counter] = pos;
                    uv[counter] = new Vector2(uF, vF);
                    colors[counter] = Color.Lerp(Color.clear, Color.white, val);
                    counter++;
                }
            }
            
            _mesh.vertices = verts;
            _mesh.uv = uv;
            _mesh.colors = colors;

            var tris = new int[((res+1) * (res)) * 6];
            for (var i = 0; i < tris.Length; i += 6)
            {
                var vIndex = i/6;

                var t1 = vIndex + 0;
                var t2 = vIndex + 1;
                var t3 = vIndex + 2 + (res-1);

                var t1r = t1 / (res + 1);
                var t2r = t2 / (res + 1);
                var t3r = (t3 / (res + 1)) - 1;

                if (t1r == t2r && t2r == t3r && t3r == t1r)
                {
                    tris[i + 0] = t1;
                    tris[i + 1] = t2;
                    tris[i + 2] = t3;
                }

                var t4 = vIndex + 2 + (res - 1);
                var t5 = vIndex + 1 + (res - 1);
                var t6 = vIndex + 0;

                var t4r = (t4 / (res + 1)) - 1;
                var t5r = (t5 / (res + 1)) - 1;
                var t6r = t6 / (res + 1);

                if (t4r == t5r && t5r == t6r && t6r == t4r)
                {
                    tris[i + 3] = t4;
                    tris[i + 4] = t5;
                    tris[i + 5] = t6;
                }
            }

            _mesh.triangles = tris;

            _mesh.RecalculateNormals(0);

#if UNITY_EDITOR
            UnityEditor.SceneView.onSceneGUIDelegate -= OnSceneGUIDelegate;
            UnityEditor.SceneView.onSceneGUIDelegate += OnSceneGUIDelegate;
#endif
        }

#if UNITY_EDITOR
        private void OnSceneGUIDelegate(UnityEditor.SceneView sceneView)
        {
            if (!_stamp || _stamp.Equals(null) || _stamp.Data == null)
            {
                UnityEditor.SceneView.onSceneGUIDelegate -= OnSceneGUIDelegate;
                return;
            }

            if (!_stamp.gameObject.activeInHierarchy)
            {
                return;
            }

            if (_lastFrame >= Time.renderedFrameCount)
            {
                return;
            }
            _lastFrame = Time.renderedFrameCount;

            var scale = new Vector3(_stamp.Size.x/_stamp.Data.Size.x, _stamp.Size.y/_stamp.Data.Size.y,
                _stamp.Size.z/_stamp.Data.Size.z);
            scale = new Vector3(_stamp.transform.lossyScale.x * scale.x, _stamp.transform.lossyScale.y * scale.y, _stamp.transform.lossyScale.z * scale.z);
            var mat = Matrix4x4.TRS(_stamp.transform.position, _stamp.transform.rotation, scale);
            Graphics.DrawMesh(_mesh, mat, _material, 0);
        }
#endif

        public void Dispose()
        {
#if UNITY_EDITOR
            UnityEditor.SceneView.onSceneGUIDelegate -= OnSceneGUIDelegate;
#endif
            UnityEngine.Object.DestroyImmediate(_mesh);
        }
    }
}