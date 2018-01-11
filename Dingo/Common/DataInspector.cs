using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dingo.Common.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dingo.Common
{
#if UNITY_EDITOR
    public class DataInspector : EditorWindow
    {
        private static int _textureIndex;
        private static List<Texture2D> _textures = new List<Texture2D>();
        private static List<Object> _context = new List<Object>();
        private Vector2 _scroll;

        public static void SetData(List<Serializable2DByteArray> datas, IEnumerable context)
        {
            _context.Clear();
            if (datas.Count == 0)
            {
                return;
            }

            var window = GetWindow<DataInspector>();
            window.titleContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
            window.minSize = new Vector2(Mathf.Min(datas[0].Width, 600), Mathf.Min(datas[0].Height, 600));

            DisposeTextures();

            foreach (var data in datas)
            {
                var tex = new Texture2D(data.Width, data.Height);
                var colors = new Color[data.Width * data.Height];

                int counter = 0;
                foreach (var val in data.Data)
                {
                    colors[counter] = Color.Lerp(Color.black, Color.white, val / 255f);
                    counter++;
                }

                tex.SetPixels(colors);
                tex.Apply();
                _textures.Add(tex);
            }

            foreach (var item in context)
            {
                var obj = item as Object;
                if (obj != null)
                {
                    _context.Add(obj);
                }
            }
        }

        public static void SetData(List<Serializable2DIntArray> datas)
        {
            _context.Clear();
            var window = GetWindow<DataInspector>();
            window.titleContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
            window.minSize = new Vector2(Mathf.Min(datas[0].Width, 600), Mathf.Min(datas[0].Height, 600));

            DisposeTextures();

            foreach (var data in datas)
            {
                var tex = new Texture2D(data.Width, data.Height);
                var colors = new Color[data.Width * data.Height];

                int counter = 0;
                foreach (var val in data.Data)
                {
                    colors[counter] = Color.Lerp(Color.black, Color.white, val / 16f);
                    counter++;
                }

                tex.SetPixels(colors);
                tex.Apply();
                _textures.Add(tex);
            }
        }

        public static void SetData(float[,] data)
        {
            _context.Clear();
            var window = GetWindow<DataInspector>(true);
            window.titleContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
            window.minSize = new Vector2(Mathf.Min(data.GetLength(0), 600), Mathf.Min(data.GetLength(1), 600) + 100);

            DisposeTextures();

            var tex = new Texture2D(data.GetLength(0), data.GetLength(1));
            var colors = new Color[data.GetLength(0)*data.GetLength(1)];

            int counter = 0;
            foreach (var val in data)
            {
                colors[counter] = Color.Lerp(Color.black, Color.white, val);
                counter++;
            }

            tex.SetPixels(colors);
            tex.Apply();
            _textures.Add(tex);
        }

        public static void SetData(Serializable2DByteArray data)
        {
            _context.Clear();
            var window = GetWindow<DataInspector>(true);
            window.titleContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
            window.minSize = new Vector2(Mathf.Min(data.Width, 600), Mathf.Min(data.Height, 600) + 100);

            DisposeTextures();

            var tex = new Texture2D(data.Width, data.Height);
            var colors = new Color[data.Width * data.Height];

            int counter = 0;
            foreach (var val in data.Data)
            {
                colors[counter] = Color.Lerp(Color.black, Color.white, val);
                counter++;
            }

            tex.SetPixels(colors);
            tex.Apply();
            _textures.Add(tex);
        }

        public static void SetDataStencil(Serializable2DFloatArray data)
        {
            _context.Clear();
            var window = GetWindow<DataInspector>(true);
            window.titleContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
            window.minSize = new Vector2(Mathf.Min(data.Width, 600), Mathf.Min(data.Height, 600) + 100);

            DisposeTextures();

            var tex = new Texture2D(data.Width, data.Height);
            var colors = new Color[data.Width * data.Height];

            var gtex = new Texture2D(data.Width, data.Height);
            var gcolors = new Color[data.Width * data.Height];

            int counter = 0;
            foreach (var val in data.Data)
            {
                int stencilKey;
                float frac;
                MiscUtilities.DecompressStencil(val, out stencilKey, out frac);

                var color = stencilKey > 0 ? ColorUtils.GetIndexColor(stencilKey) : Color.black;
                colors[counter] = Color.Lerp(Color.black, color, frac);
                gcolors[counter] = stencilKey > 0 ? Color.Lerp(Color.black, Color.white, frac) : Color.black;
                counter++;
            }

            tex.filterMode = FilterMode.Point;
            tex.SetPixels(colors);
            tex.Apply();
            _textures.Add(tex);

            gtex.SetPixels(gcolors);
            gtex.Apply();
            _textures.Add(gtex);
        }

        public static void SetData(Serializable2DFloatArray data)
        {
            _context.Clear();
            var window = GetWindow<DataInspector>(true);
            window.titleContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
            window.minSize = new Vector2(Mathf.Min(data.Width, 600), Mathf.Min(data.Height, 600) + 100);

            DisposeTextures();

            var tex = new Texture2D(data.Width, data.Height);
            var colors = new Color[data.Width * data.Height];

            int counter = 0;

            var min = data.Data.Min();
            var max = data.Data.Max();
            
            foreach (var val in data.Data)
            {
                colors[counter] = Color.Lerp(Color.black, Color.white, (val - min)/(max - min));
                /*if (val < 0)
                {
                    colors[counter] = Color.Lerp(Color.black, Color.magenta, -val * 10);
                }*/
                counter++;
            }

            tex.SetPixels(colors);
            tex.Apply();
            _textures.Add(tex);
        }

        public static void SetData(List<Serializable2DFloatArray> datas)
        {
            _context.Clear();
            var window = GetWindow<DataInspector>();
            window.titleContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
            window.minSize = new Vector2(Mathf.Min(datas[0].Width, 600), Mathf.Min(datas[0].Height, 600));

            DisposeTextures();

            foreach (var data in datas)
            {
                var tex = new Texture2D(data.Width, data.Height);
                var colors = new Color[data.Width * data.Height];

                int counter = 0;
                foreach (var val in data.Data)
                {
                    colors[counter] = Color.Lerp(Color.black, Color.white, val);
                    counter++;
                }

                tex.SetPixels(colors);
                tex.Apply();
                _textures.Add(tex);
            }
        }

        public static void SetData(List<Serializable2DByteArray> datas)
        {
            _context.Clear();
            var window = GetWindow<DataInspector>();
            window.titleContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
            window.minSize = new Vector2(Mathf.Min(datas[0].Width, 600), Mathf.Min(datas[0].Height, 600));

            DisposeTextures();

            foreach (var data in datas)
            {
                var tex = new Texture2D(data.Width, data.Height);
                var colors = new Color[data.Width * data.Height];

                int counter = 0;
                foreach (var val in data.Data)
                {
                    colors[counter] = Color.Lerp(Color.black, Color.white, val / 255f);
                    counter++;
                }

                tex.SetPixels(colors);
                tex.Apply();
                _textures.Add(tex);
            }
        }
        
        static void DisposeTextures()
        {
            for (int i = 0; i < _textures.Count; i++)
            {
                var texture2D = _textures[i];
                if (texture2D)
                {
                    DestroyImmediate(texture2D);
                }
            }
            _textures.Clear();
            _textureIndex = 0;
        }

        void OnDisable()
        {
            DisposeTextures();
        }

        void OnGUI()
        {
            if (_textures.IsNullOrEmpty())
            {
                EditorGUILayout.HelpBox("No Data", MessageType.Info);
                return;
            }

            if (_textures.Count > 1)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
                EditorGUILayout.BeginVertical();
                for (int i = 0; i < _textures.Count; i++)
                {
                    if ((i < _context.Count && GUILayout.Button(_context[i].name, GUILayout.Width(100))) || (i >= _context.Count && GUILayout.Button(i.ToString())))
                    {
                        _textureIndex = i;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                _textureIndex = 0;
            }
            var tex = _textures[_textureIndex];

            var aspect = tex.height / (float)tex.width;
            var windowSize = this.position.size;
            
            var size = Math.Min(windowSize.x, windowSize.y);
            var scaledSizeFactor = 1f;
            if (tex.width > tex.height)
            {
                scaledSizeFactor = 1 / (size * (1 / aspect) / windowSize.y);
            }
            else
            {
                scaledSizeFactor = 1 / (size * aspect / windowSize.x);
            }
            size *= scaledSizeFactor;
            
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Width(size), GUILayout.Height(windowSize.y - 60));

            EditorGUILayout.BeginVertical(GUILayout.Width(size), GUILayout.Height(size * aspect));

            GUILayout.Box("", GUILayout.Width(size - 25), GUILayout.Height(size * aspect - 25));
            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect.position += Vector2.one * 4;
            lastRect.size -= Vector2.one * 8;
            GUI.DrawTexture(lastRect, tex);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            if (_textures.Count > 1)
            {
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Normalize Preview"))
            {
                Normalise(tex);
            }
            if (GUILayout.Button("Save"))
            {
                var path = EditorUtility.SaveFilePanel("Save Data", "", "data", "png");
                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllBytes(path, tex.EncodeToPNG());
                }
            }
        }

        void Normalise(Texture2D tex)
        {
            var c = tex.GetPixels();
            var min = float.MaxValue;
            var max = float.MinValue;
            foreach (var color in c)
            {
                //sum += color.grayscale;
                var v = color.grayscale;
                if (v > max)
                {
                    max = color.grayscale;
                }
                if (v < min)
                {
                    min = v;
                }
            }
            for (int i = 0; i < c.Length; i++)
            {
                var factor = (c[i].grayscale - min)/(max - min);
                c[i] = Color.Lerp(Color.black, Color.white, factor);
            }
            tex.SetPixels(c);
            tex.Apply();
        }
        
    }
#endif
}