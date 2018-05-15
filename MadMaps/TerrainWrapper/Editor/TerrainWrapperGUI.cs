using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using MadMaps.Common.Painter;
using MadMaps.Common;
using UnityEditor.SceneManagement;
using System.Linq;

namespace MadMaps.Terrains
{
    [CustomEditor(typeof(TerrainWrapper))]
    public class TerrainWrapperGUI : Editor
    {
        [MenuItem("Tools/Mad Maps/Documentation")]
        public static void OpenWindow()
        {
            Help.BrowseURL("http://lrtw.net/madmaps/");
        }

        public static TerrainLayer StencilLayerDisplay
        {
            get { return __stencilLayerDisplay; }
            set
            {
                __stencilLayerDisplay = value;
                _stencilDisplayDirty = true;
            }
        }
        public static TerrainLayer __stencilLayerDisplay;
        public static bool _stencilDisplayDirty;
        
        public int CurrentTab;
        public bool IsPopout;

        public TerrainWrapper Wrapper;

        private TerrainLayerDrawer _layerDrawer;
        private TerrainSplatsDrawer _splatsDrawer;
        private TerrainDetailsDrawer _detailsDrawer;
        private static GUIContent[] _tabs;

        public void OnEnable()
        {
            if (!Wrapper)
            {
                return;
            }
            _layerDrawer = new TerrainLayerDrawer(Wrapper);
            _splatsDrawer = new TerrainSplatsDrawer(Wrapper);
            _detailsDrawer = new TerrainDetailsDrawer(Wrapper);
        }

        void OnDisable()
        {
            StencilLayerDisplay = null;
        }

        public override void OnInspectorGUI()
        {
            if (Wrapper == null)
            {
                Wrapper = target as TerrainWrapper;
                return;
            }
            
            for (int i = Wrapper.Layers.Count - 1; i >= 0; i--)
            {
                if (!Wrapper.Layers[i])
                {
                    Wrapper.Layers.RemoveAt(i);
                }
            }

            /*if (GUILayout.Button("Dirty"))
            {
                foreach (var layerBase in Wrapper.Layers)
                {
                    layerBase.ForceDirty();
                    EditorUtility.SetDirty(layerBase);
                }
                EditorSceneManager.MarkAllScenesDirty();
            }*/

            if (_tabs == null)
            {
                _tabs = new[]
                {
                    new GUIContent("Layers") {image = EditorGUIUtility.FindTexture("Terrain Icon")}, 
                    new GUIContent("Splats") {image = EditorGUIUtility.FindTexture("TerrainInspector.TerrainToolSplat")},
                    new GUIContent("Details") {image = EditorGUIUtility.FindTexture("TerrainInspector.TerrainToolPlants")},
                    new GUIContent("Info") {image = EditorGUIUtility.FindTexture("_Help")},
                };
            }
            _layerDrawer = _layerDrawer ?? new TerrainLayerDrawer(Wrapper);
            _splatsDrawer = _splatsDrawer ?? new TerrainSplatsDrawer(Wrapper);
            _detailsDrawer = _detailsDrawer ?? new TerrainDetailsDrawer(Wrapper);

            EditorGUILayout.BeginHorizontal();
            CurrentTab = GUILayout.Toolbar(CurrentTab, _tabs, GUILayout.Height(20), GUILayout.Width(EditorGUIUtility.currentViewWidth - (IsPopout ? 12 : 40)));
            
            if (!IsPopout && GUILayout.Button(new GUIContent(GUIResources.PopoutIcon, "Popout Inspector"), 
                EditorStyles.label, GUILayout.Width(18), GUILayout.Height(18)))
            {
                var w = EditorWindow.GetWindow<TerrainWrapperEditorWindow>();
                w.Wrapper = Wrapper;
                Selection.objects = new Object[0];
                return;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);

            var currentTabTitle = _tabs[CurrentTab].text;
            if (currentTabTitle == "Layers")
            {
                if(Wrapper.Dirty)
                {
                    EditorGUILayout.HelpBox("This Wrapper has unnaplied changes. Click \"Reapply All\" to apply them.", MessageType.Info);
                }
                _layerDrawer.List.DoLayoutList();
                if (_layerDrawer.List.index >= 0 && Wrapper.Layers.Count > 0 && _layerDrawer.List.index < Wrapper.Layers.Count)
                {
                    var selected = Wrapper.Layers[_layerDrawer.List.index];
                    Wrapper.Layers[_layerDrawer.List.index] = TerrainLayerDrawer.DrawExpandedGUI(Wrapper, selected);
                }
                else
                {
                    EditorGUILayout.HelpBox("Select a Layer to see information about it here.", MessageType.Info);
                }
                EditorGUILayout.Space();
            }
            else if (currentTabTitle == "Splats")
            { 
                _splatsDrawer.List.DoLayoutList();
            }
            else if (currentTabTitle == "Details")
            {
                _detailsDrawer.List.DoLayoutList();
            }
            else if (currentTabTitle == "Info")
            {
                TerrainWrapper.ComputeShaders = EditorGUILayout.Toggle("Compute Shaders", TerrainWrapper.ComputeShaders);
                Wrapper.WriteHeights = EditorGUILayout.Toggle("Write Heights", Wrapper.WriteHeights);
                Wrapper.WriteSplats = EditorGUILayout.Toggle("Write Splats", Wrapper.WriteSplats);
                Wrapper.WriteTrees = EditorGUILayout.Toggle("Write Trees", Wrapper.WriteTrees);
                Wrapper.WriteDetails = EditorGUILayout.Toggle("Write Details", Wrapper.WriteDetails);
                Wrapper.WriteObjects = EditorGUILayout.Toggle("Write Objects", Wrapper.WriteObjects);
                #if VEGETATION_STUDIO
                Wrapper.WriteVegetationStudio = EditorGUILayout.Toggle("Write Vegetation Studio", Wrapper.WriteVegetationStudio);
                #endif

                EditorExtensions.Seperator();

                var previewContent = new GUIContent(GUIResources.EyeOpenIcon, "Preview");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Compound Heights", Wrapper.CompoundTerrainData.Heights != null ? string.Format("{0}", string.Format("{0}x{1}", Wrapper.CompoundTerrainData.Heights.Width, Wrapper.CompoundTerrainData.Heights.Height)) : "null");
                if (Wrapper.CompoundTerrainData.SplatData != null && GUILayout.Button(previewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(16)))
                {
                    DataInspector.SetData(Wrapper.CompoundTerrainData.Heights);
                }
                EditorGUILayout.EndHorizontal();
                EditorExtensions.Seperator();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Compound Splats", Wrapper.CompoundTerrainData.SplatData != null ? string.Format("{0}", Wrapper.CompoundTerrainData.SplatData.Count) : "null");
                if (Wrapper.CompoundTerrainData.SplatData != null && GUILayout.Button(previewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(16)))
                {
                    List<IDataInspectorProvider> data = new List<IDataInspectorProvider>();
                    List<object> context = new List<object>();
                    foreach (var keyValuePair in Wrapper.CompoundTerrainData.SplatData)
                    {
                        data.Add(keyValuePair.Value);
                        context.Add(keyValuePair.Key);
                    }
                    DataInspector.SetData(data, context);
                }
                EditorGUILayout.EndHorizontal();
                EditorExtensions.Seperator();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Compound Details", Wrapper.CompoundTerrainData.DetailData != null ? string.Format("{0}", Wrapper.CompoundTerrainData.DetailData.Count) : "null");
                if (Wrapper.CompoundTerrainData.DetailData != null && GUILayout.Button(previewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(16)))
                {
                    List<IDataInspectorProvider> data = new List<IDataInspectorProvider>();
                    List<object> context = new List<object>();
                    foreach (var keyValuePair in Wrapper.CompoundTerrainData.DetailData)
                    {
                        data.Add(keyValuePair.Value);
                        context.Add(keyValuePair.Key);
                    }
                    DataInspector.SetData(data, context, true);
                }
                EditorGUILayout.EndHorizontal();
                EditorExtensions.Seperator();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Compound Objects: ", Wrapper.CompoundTerrainData.Objects.Count.ToString());
                if (Wrapper.CompoundTerrainData.DetailData != null && GUILayout.Button(previewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(16)))
                {
                    Dictionary<object, IDataInspectorProvider> data = new Dictionary<object, IDataInspectorProvider>();
                    foreach (var obj in Wrapper.CompoundTerrainData.Objects)
                    {
                        if(!data.ContainsKey(obj.Value.Data.Prefab))
                        {
                            data[obj.Value.Data.Prefab] = new PositionList();
                        }
                        (data[obj.Value.Data.Prefab] as PositionList).Add(obj.Value.Data.Position);
                    }
                    DataInspector.SetData(data.Values.ToList(), data.Keys.ToList(), true);
                }
                EditorGUILayout.EndHorizontal();
                EditorExtensions.Seperator();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Compound Trees: ", Wrapper.CompoundTerrainData.Trees.Count.ToString());
                if (Wrapper.CompoundTerrainData.Trees != null && GUILayout.Button(previewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(16)))
                {
                    Dictionary<object, IDataInspectorProvider> data = new Dictionary<object, IDataInspectorProvider>();
                    foreach (var obj in Wrapper.CompoundTerrainData.Trees)
                    {
                        if(!data.ContainsKey(obj.Value.Prototype))
                        {
                            data[obj.Value.Prototype] = new PositionList();
                        }
                        (data[obj.Value.Prototype] as PositionList).Add(obj.Value.Position);
                    }
                    DataInspector.SetData(data.Values.ToList(), data.Keys.ToList(), true);
                }
                EditorGUILayout.EndHorizontal();
                EditorExtensions.Seperator();

                #if VEGETATION_STUDIO
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Compound Vegetation Studio Data: ", Wrapper.CompoundTerrainData.VegetationStudio.Count.ToString());
                if (Wrapper.CompoundTerrainData.VegetationStudio != null && GUILayout.Button(previewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(16)))
                {
                    Dictionary<object, IDataInspectorProvider> data = new Dictionary<object, IDataInspectorProvider>();
                    foreach (var obj in Wrapper.CompoundTerrainData.VegetationStudio)
                    {
                        if(!data.ContainsKey(obj.Value.VSID))
                        {
                            data[obj.Value.VSID] = new PositionList();
                        }
                        (data[obj.Value.VSID] as PositionList).Add(obj.Value.Position);
                    }
                    DataInspector.SetData(data.Values.ToList(), data.Keys.ToList(), true);
                }
                EditorGUILayout.EndHorizontal();
                EditorExtensions.Seperator();
                #endif
            }
        }
        
        void OnSceneGUI()
        {
            if (StencilLayerDisplay == null)
            {
                return;
            }

            EditorCellHelper.SetAlive();
            if (!_stencilDisplayDirty)
            {
                return;
            }

            var stencil = StencilLayerDisplay.Stencil;
            if (stencil == null)
            {
                return;
            }

            _stencilDisplayDirty = false;

            var wrapper = Wrapper;
            EditorCellHelper.Clear(false);

            int step = wrapper.Terrain.terrainData.heightmapResolution / 256;
            EditorCellHelper.TRS = Matrix4x4.identity;
            EditorCellHelper.CellSize = (wrapper.Terrain.terrainData.size.x/
                                        (float)wrapper.Terrain.terrainData.heightmapResolution) * step;
            //int counter = 0;
            for (var u = 0; u < stencil.Width; u += step)
            {
                for (var v = 0; v < stencil.Height; v += step)
                {
                    var wPos = wrapper.Terrain.HeightmapCoordToWorldPos(new Common.Coord(u, v)).xz().x0z(50);
                    var stencilPos = new Vector2(u/(float) stencil.Width, v/(float) stencil.Height);

                    var stencilKey = Mathf.FloorToInt(stencil.BilinearSample(stencilPos));
                    var strength = StencilLayerDisplay.GetStencilStrength(stencilPos);
                    
                    var stencilColor = ColorUtils.GetIndexColor(stencilKey);
                    if (stencilKey <= 0)
                    {
                        stencilColor = Color.black;
                        strength = 0;
                    }
                    EditorCellHelper.AddCell(wPos, Color.Lerp(Color.black, stencilColor, strength));
                    //counter++;
                }
            }
            EditorCellHelper.Invalidate();
        }

    }
}
