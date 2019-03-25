using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MadMaps.Terrains
{
    public class TerrainSplatsDrawer
    {
        public ReorderableList List;
        private TerrainWrapper _wrapper;

        public TerrainSplatsDrawer(TerrainWrapper wrapper)
        {
            _wrapper = wrapper;
#if UNITY_2018_3_OR_NEWER
            List = new ReorderableList(wrapper.TerrainLayerSplatPrototypes, typeof(TerrainLayer), false, false, true, false);
#else
            List = new ReorderableList(wrapper.SplatPrototypes, typeof(SplatPrototypeWrapper), false, false, true, false);
#endif
            List.drawHeaderCallback += DrawLayerHeaderCallback;
            List.drawElementCallback += DrawLayerElementCallback;
            List.elementHeightCallback += LayerElementHeightCallback;
            List.onAddCallback += OnLayerAddCallback;
            List.onRemoveCallback += OnLayerRemoveCallback;
            List.drawFooterCallback += DrawLayerFooterCallback;
            List.onChangedCallback += RefreshSplats;
        }

        private void RefreshSplats(ReorderableList list)
        {
            _wrapper.RefreshSplats();
        }

        private void OnLayerRemoveCallback(ReorderableList list)
        {
#if UNITY_2018_3_OR_NEWER
            _wrapper.TerrainLayerSplatPrototypes.RemoveAt(list.index);
#else
            _wrapper.SplatPrototypes.RemoveAt(list.index);
#endif
        }

        private void OnLayerAddCallback(ReorderableList list)
        {
#if UNITY_2018_3_OR_NEWER
            _wrapper.TerrainLayerSplatPrototypes.Add(null);
#else
            _wrapper.SplatPrototypes.Add(null);
#endif
        }

        private float LayerElementHeightCallback(int index)
        {
            return 64;
        }

        private void DrawLayerElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            rect.height -= 2;

            float headerHeight = 18;
            float previewTexSize = 57;
            var objFieldWidth = rect.width - previewTexSize - previewTexSize / 2;

            var layerNumberRect = new Rect(rect.x, rect.y, 10, headerHeight);
            EditorGUI.LabelField(layerNumberRect, index.ToString());

            var nameRect = new Rect(layerNumberRect.xMax + 4, rect.y, objFieldWidth, headerHeight);
#if UNITY_2018_3_OR_NEWER
            var newKey = (TerrainLayer)EditorGUI.ObjectField(nameRect, _wrapper.TerrainLayerSplatPrototypes[index], typeof(TerrainLayer), false);
            if (newKey != _wrapper.TerrainLayerSplatPrototypes[index])
            {
                _wrapper.TerrainLayerSplatPrototypes[index] = newKey;
                _wrapper.RefreshSplats();
                //_wrapper.Dirty = true;
            }

            if (_wrapper.TerrainLayerSplatPrototypes[index] == null)
            {
                return;
            }
            var previewRect = new Rect(layerNumberRect.xMax + 4 + objFieldWidth + 4, rect.y, previewTexSize, previewTexSize);
            var tex = _wrapper.TerrainLayerSplatPrototypes[index] != null ? _wrapper.TerrainLayerSplatPrototypes[index].diffuseTexture : null;
#else
            var newKey = (SplatPrototypeWrapper)EditorGUI.ObjectField(nameRect, _wrapper.SplatPrototypes[index], typeof(SplatPrototypeWrapper), false);
            if (newKey != _wrapper.SplatPrototypes[index])
            {
                _wrapper.SplatPrototypes[index] = newKey;
                _wrapper.RefreshSplats();
                //_wrapper.Dirty = true;
            }

            if (_wrapper.SplatPrototypes[index] == null)
            {
                return;
            }
            var previewRect = new Rect(layerNumberRect.xMax + 4 + objFieldWidth + 4, rect.y, previewTexSize, previewTexSize);
            var tex = _wrapper.SplatPrototypes[index] != null ? _wrapper.SplatPrototypes[index].Texture : null;
#endif
            GUI.DrawTexture(previewRect, tex);
            var infoRect = new Rect(layerNumberRect.xMax + 4, rect.y + headerHeight, objFieldWidth, rect.height - headerHeight);
            var sb = new StringBuilder("Written to by: ");
            var any = false;
            foreach (var layer in _wrapper.Layers)
            {
                var wrappers = layer.GetSplatPrototypeWrappers();
#if UNITY_2018_3_OR_NEWER
                if (wrappers != null && wrappers.Contains(_wrapper.TerrainLayerSplatPrototypes[index]))
#else
                if (wrappers != null && wrappers.Contains(_wrapper.SplatPrototypes[index]))
#endif
                {
                    sb.Append(layer.name);
                    sb.Append("    ");
                    any = true;
                }
            }
            if (any)
            {
                var style = EditorStyles.label;
                style.wordWrap = true;
                EditorGUI.LabelField(infoRect, sb.ToString(), style);
            }
        }

        private void DrawLayerHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Splats");
        }

        private void DrawLayerFooterCallback(Rect rect)
        {
            float buttonWidth = 25;
            var xMax = rect.xMax;
            var num = xMax - 58f - buttonWidth * 1;
            rect = new Rect(num, rect.y, xMax - num, rect.height);
            var rect2 = new Rect(rect.xMax - 50, rect.y - 3f, 25f, 16f);
            var position = new Rect(xMax - 29f, rect.y - 3f, 25f, 16f);
            if (Event.current.type == EventType.Repaint)
            {
                GUIStyle footerBackground = "RL Footer";
                footerBackground.Draw(rect, false, false, false, false);
            }
            if (GUI.Button(rect2, EditorGUIUtility.IconContent("Toolbar Plus", "|Add to list"), "RL FooterButton"))
            {
                List.onAddCallback(List);
                if (List.onChangedCallback != null)
                {
                    List.onChangedCallback(List);
                }
            }
            using (new EditorGUI.DisabledScope(List.index < 0 || List.index >= List.count || (List.onCanRemoveCallback != null && !List.onCanRemoveCallback(List))))
            {
                if (GUI.Button(position, EditorGUIUtility.IconContent("Toolbar Minus", "|Remove selection from list"), "RL FooterButton"))
                {
                    List.onRemoveCallback(List);
                    if (List.onChangedCallback != null)
                    {
                        List.onChangedCallback(List);
                    }
                }
            }
            var reapplyRect = new Rect(rect.xMax - 50 - buttonWidth, rect.y - 3f, buttonWidth, 16f);
            if (GUI.Button(reapplyRect, EditorGUIUtility.IconContent("TreeEditor.Refresh", "Refresh All Layers"), "RL FooterButton"))
            {
                _wrapper.RefreshSplats();
            }
#if UNITY_2018_3_OR_NEWER
            List.list = _wrapper.TerrainLayerSplatPrototypes;
#else
            List.list = _wrapper.SplatPrototypes;
#endif
        }

    }
}