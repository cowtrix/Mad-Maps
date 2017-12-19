using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EditorCellPainter;
using sMap.Common;
using sMap.Common.Collections;
using sMap.Terrains;
using sMap.Terrains.Lookups;
using UnityEditor;
using UnityEngine;

namespace sMap.WorldStamp
{
    public class WorldStampCreator : SceneViewEditorWindow
    {
        private static float MaskResolution = 128;
        private static float MinMaskRes = 4;
        private static Painter _maskPainter;
        private bool _painting;

        [MenuItem("Window/World Stamp Creator", false, 6)]
        public static void OpenWindow()
        {
            GetWindow<WorldStampCreator>();
        }

        enum Step
        {
            SettingArea,
            Finalizing,
        }

        enum SceneGUIOwner
        {
            None,
            Heights,
            Splats,
            Trees,
            Details,
            Objects,
            Mask,
        }

        private SceneGUIOwner _currentSceneGUIOwner = SceneGUIOwner.None;
        private Step _currentStep;
        private Bounds _currentBounds;
        private Terrain _focusedTerrain;
        private WorldStampData _tempData;
        
        private bool _treesExpanded = false;
        private bool _treesEnabled = true;

        private bool _heightsExpanded = false;
        private bool _heightsEnabled = true;
        private bool _autoHeightMin = true;
        private float _heightMin = 0;
        private int _displayRes = 16;

        private bool _objectsExpanded = false;
        private bool _objectsEnabled = true;
        private LayerMask _objectLayerMask = ~0;
        private Dictionary<PrefabObjectData, Bounds> _objInSceneMapping = new Dictionary<PrefabObjectData, Bounds>();

        private bool _splatsExpanded = false;
        private bool _splatsEnabled = true;
        private List<SplatPrototypeWrapper> _ignoredSplats = new List<SplatPrototypeWrapper>();
        private bool _ignoredSplatsExpanded;

        private bool _detailsExpanded = false;
        private bool _detailsEnabled = true;
        private List<DetailPrototypeWrapper> _ignoredDetails = new List<DetailPrototypeWrapper>();
        private bool _ignoredDetailsExpanded; 

        private bool _maskPainterExpanded = false;

        protected void OnGUI()
        {
            titleContent = new GUIContent("World Stamp Creator");
            _focusedTerrain = (Terrain) EditorGUILayout.ObjectField("Target Terrain", _focusedTerrain, typeof (Terrain), true);
            if (_focusedTerrain == null)
            {
                EditorGUILayout.HelpBox("Please Select a Target Terrain", MessageType.Info);
                return;
            }
            
            if (GUILayout.Button("Reset"))
            {
                _tempData = null;
                _currentStep = Step.SettingArea;
                if (_currentBounds.min == Vector3.zero && _currentBounds.size == Vector3.zero)
                {
                    _currentBounds = new Bounds(_focusedTerrain.GetPosition(), Vector3.one * 100);
                }
            }
            
            if (_currentStep == Step.SettingArea)
            {
                EditorGUILayout.HelpBox("Please Select a Target Area", MessageType.Info);
                if (GUILayout.Button("Done"))
                {
                    DoStep();
                }
                return;
            }
            
            if (_currentStep == Step.Finalizing)
            {
                if (GUILayout.Button("Collect"))
                {
                    CollectAll();
                }
                if (GUILayout.Button("Create"))
                {
                    CollectAll();
                    GameObject go = new GameObject("New WorldStamp");
                    go.transform.position = _currentBounds.center + Vector3.up * _heightMin * _focusedTerrain.terrainData.size.y;
                    var stamp = go.AddComponent<WorldStamp>();
                    stamp.SetData(_tempData);
                    stamp.HaveHeightsBeenFlipped = true;
                    EditorGUIUtility.PingObject(stamp);
                }

                var heightsHeader = string.Format("Heights ({0})",
                    _tempData.Heights != null
                        ? string.Format("{0}x{1}", _tempData.Heights.Width, _tempData.Heights.Height)
                        : "Null");
                if (DoSectionHeader(heightsHeader, ref _heightsExpanded, ref _currentSceneGUIOwner,
                    SceneGUIOwner.Heights, ref _heightsEnabled))
                {
                    DataInspector.SetData(_tempData.Heights);
                }
                if (_heightsExpanded)
                {
                    EditorGUI.indentLevel++;
                    _autoHeightMin = EditorGUILayout.Toggle("Auto Min Height", _autoHeightMin);
                    if (!_autoHeightMin)
                    {
                        var newHeightMin = EditorGUILayout.Slider("Min Height", _heightMin, 0, 1);
                        if (newHeightMin != _heightMin)
                        {
                            _heightMin = newHeightMin;
                            CollectHeights();
                            SceneView.RepaintAll();
                        }
                    }
                    int maxRes =
                        Mathf.CeilToInt(Mathf.Max(
                            ((_currentBounds.size.x / _focusedTerrain.terrainData.size.x ) * _focusedTerrain.terrainData.alphamapResolution),
                            _focusedTerrain.terrainData.size.z/_currentBounds.size.z)
                            );
                    _displayRes = EditorGUILayout.IntSlider("Preview Resolution", _displayRes, 8, maxRes);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                DoSectionHeader(string.Format("Objects ({0})", _tempData.Objects.Count), ref _objectsExpanded, ref _currentSceneGUIOwner, SceneGUIOwner.Objects, ref _objectsEnabled, false);
                if (_objectsExpanded)
                {
                    EditorGUI.indentLevel++;
                    _objectLayerMask = LayerMaskFieldUtility.LayerMaskField("Mask", _objectLayerMask, false);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                DoSectionHeader(string.Format("Trees ({0})", _tempData.Trees.Count), ref _treesExpanded, ref _currentSceneGUIOwner, SceneGUIOwner.Trees, ref _treesEnabled, false);
                if (_treesExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Nothing here yet...");
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                if (DoSectionHeader(string.Format("Splats ({0})", _tempData.SplatData.Count), ref _splatsExpanded,
                    ref _currentSceneGUIOwner, SceneGUIOwner.Splats, ref _splatsEnabled))
                {
                    DataInspector.SetData(_tempData.SplatData.Select(x => x.Data).ToList(), _tempData.SplatData.Select(x => x.Wrapper).ToList());
                }
                if (_splatsExpanded)
                {
                    EditorGUI.indentLevel++;
                    _ignoredSplatsExpanded = EditorGUILayout.Foldout(_ignoredSplatsExpanded, string.Format("Ignored Splats ({0})", _ignoredSplats.Count));
                    if (_ignoredSplatsExpanded)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayoutX.DrawList(_ignoredSplats, null, DrawSplatPrototypeWrapperGUI, null, false, true);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                if (DoSectionHeader(string.Format("Details ({0})", _tempData.DetailData.Count), ref _detailsExpanded,
                    ref _currentSceneGUIOwner, SceneGUIOwner.Details, ref _detailsEnabled))
                {
                    DataInspector.SetData(_tempData.DetailData.Select(x => x.Data).ToList(), _tempData.DetailData.Select(x => x.Wrapper).ToList());
                }
                if (_detailsExpanded)
                {
                    EditorGUI.indentLevel++;
                    _ignoredDetailsExpanded = EditorGUILayout.Foldout(_ignoredDetailsExpanded, string.Format("Ignored Details ({0})", _ignoredDetails.Count));
                    if (_ignoredDetailsExpanded)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayoutX.DrawList(_ignoredDetails, null, DrawDetailPrototypeWrapperGUI, null, false, true);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                bool maskDummyRef = true;
                DoSectionHeader("Mask", ref _maskPainterExpanded, ref _currentSceneGUIOwner, SceneGUIOwner.Mask, ref maskDummyRef, false);
                if (_maskPainterExpanded)
                {
                    EditorGUI.indentLevel++;
                    if (EditorGUILayoutX.IndentedButton("Reset Mask"))
                    {
                        ResetMask();
                    }
                    if (EditorGUILayoutX.IndentedButton("Fill From Min Y"))
                    {
                        FillMaskFromMinY();
                    }
                    if (EditorGUILayoutX.IndentedButton("Load From Texture"))
                    {
                        var path = EditorUtility.OpenFilePanel("Load Texture Into Mask", "Assets", "png");
                        if (!string.IsNullOrEmpty(path))
                        {
                            var tex = new Texture2D(0, 0);
                            tex.LoadImage(File.ReadAllBytes(path));

                            _tempData.GridSize = Math.Max(MinMaskRes, Math.Max(_currentBounds.size.x, _currentBounds.size.z) / MaskResolution);
                            _tempData.Mask.Clear();
                            for (var u = 0f; u < _currentBounds.size.x; u += _tempData.GridSize)
                            {
                                for (var v = 0f; v < _currentBounds.size.z; v += _tempData.GridSize)
                                {
                                    var cell = _tempData.GridManager.GetCell(new Vector3(u, 0, v));
                                    var cellMax = _tempData.GridManager.GetCellMax(cell).x0z() + _currentBounds.min;
                                    var cellMin = _tempData.GridManager.GetCellCenter(cell).x0z() + _currentBounds.min;
                                    if (!_currentBounds.Contains(cellMax) || !_currentBounds.Contains(cellMin))
                                    {
                                        continue;
                                    }
                                    var val = tex.GetPixelBilinear(u/_currentBounds.size.x, v/_currentBounds.size.z).grayscale;
                                    _tempData.Mask.SetValue(cell, val);
                                }
                            }

                            DestroyImmediate(tex);
                        }
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
            }
        }

        private SplatPrototypeWrapper DrawSplatPrototypeWrapperGUI(SplatPrototypeWrapper splatPrototypeWrapper)
        {
            return (SplatPrototypeWrapper) EditorGUILayout.ObjectField(splatPrototypeWrapper, typeof (SplatPrototypeWrapper), false);
        }

        private DetailPrototypeWrapper DrawDetailPrototypeWrapperGUI(DetailPrototypeWrapper detailPrototypeWrapper)
        {
            return (DetailPrototypeWrapper)EditorGUILayout.ObjectField(detailPrototypeWrapper, typeof(DetailPrototypeWrapper), false);
        }

        private static bool DoSectionHeader(string label, ref bool expanded, ref SceneGUIOwner sceneGUIowner, SceneGUIOwner thisSceneGuiID, ref bool enabled, bool canDataPreview = true)
        {
            EditorExtensions.Seperator();

            EditorGUILayout.BeginHorizontal();
            expanded = EditorGUILayout.Foldout(expanded, label);

            bool dataResult = false;
            if (canDataPreview)
            {
                var previewContent = EditorGUIUtility.IconContent("ClothInspector.ViewValue");
                previewContent.tooltip = "Preview In Window";
                dataResult = GUILayout.Button(previewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(16));
            }

            enabled = EditorGUILayout.Toggle(new GUIContent(string.Empty,  enabled ? "Enable Capture" : "Disable Capture"), enabled, GUILayout.Width(20));
            
            var sceneviewContent = EditorGUIUtility.IconContent("Terrain Icon");
            sceneviewContent.tooltip = sceneGUIowner == thisSceneGuiID ? "Clear Preview" : "Preview On Terrain";
            GUI.color = sceneGUIowner == thisSceneGuiID ? Color.white : Color.gray;
            if (GUILayout.Button(sceneviewContent, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (sceneGUIowner == thisSceneGuiID)
                {
                    sceneGUIowner = SceneGUIOwner.None;
                }
                else
                {
                    sceneGUIowner = thisSceneGuiID;
                }
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            return dataResult;
        }

        void DoStep()
        {
            if (_currentStep == Step.SettingArea)
            {
                if (_currentBounds.size == Vector3.zero)
                {
                    Debug.Log("Invalid size!");
                    return;
                }

                CollectAll();
            }
            _currentStep++;
        }

        void CollectAll()
        {
            if (_tempData == null)
            {
                _tempData = new WorldStampData()
                {
                    GridSize = Math.Max(MinMaskRes, Math.Max(_currentBounds.size.x, _currentBounds.size.z) / MaskResolution),
                    Size = new Vector3(_currentBounds.size.x, _focusedTerrain.terrainData.size.y, _currentBounds.size.z)
                };
                ResetMask();
            }
            
            CollectObjects();
            CollectHeights();
            CollectTrees();
            CollectSplats();
            CollectDetails();
        }

        private void ResetMask()
        {
            _tempData.GridSize = Math.Max(MinMaskRes, Math.Max(_currentBounds.size.x, _currentBounds.size.z) / MaskResolution);
            _tempData.Mask.Clear();
            for (var u = 0f; u < _currentBounds.size.x; u += _tempData.GridSize)
            {
                for (var v = 0f; v < _currentBounds.size.z; v += _tempData.GridSize)
                {
                    var cell = _tempData.GridManager.GetCell(new Vector3(u, 0, v));
                    var cellMax = _tempData.GridManager.GetCellMax(cell).x0z() + _currentBounds.min;
                    var cellMin = _tempData.GridManager.GetCellCenter(cell).x0z() + _currentBounds.min;
                    if (!_currentBounds.Contains(cellMax) || !_currentBounds.Contains(cellMin))
                    {
                        continue;
                    }
                    _tempData.Mask.SetValue(cell, 1);
                }
            }
        }

        private void FillMaskFromMinY()
        {
            _tempData.Mask.Clear();
            for (var u = 0f; u < _currentBounds.size.x; u += _tempData.GridSize)
            {
                for (var v = 0f; v < _currentBounds.size.z; v += _tempData.GridSize)
                {
                    var cell = _tempData.GridManager.GetCell(new Vector3(u, 0, v));
                    var cellMax = _tempData.GridManager.GetCellMax(cell).x0z() + _currentBounds.min;
                    var cellMin = _tempData.GridManager.GetCellCenter(cell).x0z() + _currentBounds.min;
                    if (!_currentBounds.Contains(cellMax) || !_currentBounds.Contains(cellMin))
                    {
                        continue;
                    }

                    var h = _tempData.Heights.BilinearSample(new Vector2(v / _currentBounds.size.z, u / _currentBounds.size.x));
                    _tempData.Mask.SetValue(cell,h > 0 ? 1:0);
                }
            }
        }

        void CollectObjects()
        {
            _objInSceneMapping.Clear();
            _tempData.Objects.Clear();
            
            if (!_objectsEnabled)
            {
                return;
            }

            var expandedBounds = _currentBounds;
            expandedBounds.Expand(Vector3.up * 5000);

            var terrainPos = _focusedTerrain.GetPosition();

            var allTransforms = FindObjectsOfType<Transform>();
            var done = new HashSet<Transform>();
            allTransforms = allTransforms.OrderBy(transform => transform.GetHierarchyDepth()).ToArray();
            HashSet<Transform> ignores = new HashSet<Transform>();
            for (int i = 0; i < allTransforms.Length; i++)
            {
                var transform = allTransforms[i];
                if (done.Contains(transform) || ignores.Contains(transform))
                {
                    continue;
                }
                if (transform.GetComponent<TerrainCollider>())
                {
                    continue;
                }
                if (!expandedBounds.Contains(transform.position))
                {
                    continue;
                }
                if (transform.GetComponentInChildren<WorldStamp>())
                {
                    Debug.Log(string.Format("Ignored {0} as it had a stamp component on it.", transform), transform);
                    ignores.Add(transform);
                    var children = transform.GetComponentsInChildren<Transform>(true);
                    foreach (var ignore in children)
                    {
                        ignores.Add(ignore);
                    }
                    continue;
                }

                var go = transform.gameObject;
                if (_objectLayerMask != (_objectLayerMask | (1 << go.layer)))
                {
                    continue;
                }

                var prefabRoot = PrefabUtility.GetPrefabObject(go);
                if (prefabRoot == null)
                {
                    //DebugHelper.DrawCube(collider.bounds.center, collider.bounds.extents, Quaternion.identity, Color.red, 30);
                    continue;
                }

                done.Add(transform);
                var subTransforms = transform.GetComponentsInChildren<Transform>();
                foreach (var childTransform in subTransforms)
                {
                    done.Add(childTransform);
                }

                var prefabAsset = PrefabUtility.GetPrefabParent(go) as GameObject;
                var root = PrefabUtility.FindPrefabRoot(go);

                var relativePos = root.transform.position - _currentBounds.min;
                relativePos = new Vector3(relativePos.x / _currentBounds.size.x, 0, relativePos.z / _currentBounds.size.z);
                var terrainHeightAtPoint = (_focusedTerrain.SampleHeight(root.transform.position) + terrainPos.y);
                relativePos.y = (root.transform.position.y - terrainHeightAtPoint);

                var newData = new PrefabObjectData()
                {
                    Prefab = prefabAsset,
                    Position = relativePos,
                    Rotation = root.transform.rotation.eulerAngles,
                    Scale = root.transform.lossyScale,
                    Guid = GUID.Generate().ToString(),
                };

                var c = transform.GetComponentInChildren<Collider>();
                if (c)
                {
                    _objInSceneMapping[newData] = c.bounds;
                }
                else
                {
                    var r = transform.GetComponentInChildren<Renderer>();
                    if (r)
                    {
                        _objInSceneMapping[newData] = r.bounds;
                    }
                }
                _tempData.Objects.Add(newData);
            }
        }

        void CollectHeights()
        {
            if (!_heightsEnabled)
            {
                _tempData.Heights = null;
                return;
            }

            var min = _focusedTerrain.WorldToHeightmapCoord(_currentBounds.min, TerrainX.RoundType.Floor);
            var max = _focusedTerrain.WorldToHeightmapCoord(_currentBounds.max, TerrainX.RoundType.Floor);

            int width = max.x - min.x;
            int height = max.z - min.z;

            float minHeight = float.MaxValue;
            _tempData.Heights = new Serializable2DFloatArray(width + 1, height + 1);

            var sampleHeights = _focusedTerrain.terrainData.GetHeights(min.x, min.z, width + 1, height + 1);
            //sampleHeights = sampleHeights.Flip();

            for (var dx = 0; dx <= width; ++dx)
            {
                for (var dz = 0; dz <= height; ++dz)
                {
                    var sample = sampleHeights[dz, dx];
                    if (sample < minHeight)
                    {
                        minHeight = sample;
                    }
                    _tempData.Heights[dx, dz] = sample;
                }
            }
            if (_autoHeightMin)
            {
                _heightMin = minHeight;
            }
            if (_heightMin > 0)
            {
                for (var dx = 0; dx <= width; ++dx)
                {
                    for (var dz = 0; dz <= height; ++dz)
                    {
                        _tempData.Heights[dx, dz] -= _heightMin;
                    }
                }
            }
        }

        void CollectTrees()
        {
            _tempData.Trees.Clear();
            if (!_treesEnabled)
            {
                return;
            }

            var trees = _focusedTerrain.terrainData.treeInstances;
            var prototypes = new List<TreePrototype>(_focusedTerrain.terrainData.treePrototypes);
            var expandedBounds = _currentBounds;
            expandedBounds.Expand(Vector3.up * 5000);
            foreach (var tree in trees)
            {
                var worldPos = _focusedTerrain.TreeToWorldPos(tree.position);
                if (!expandedBounds.Contains(worldPos))
                {
                    continue;
                }
                var hurtTree = new HurtTreeInstance(tree, prototypes);
                var yDelta = worldPos.y - _focusedTerrain.SampleHeight(worldPos);
                hurtTree.Position = new Vector3((worldPos.x - _currentBounds.min.x) / _currentBounds.size.x, yDelta, (worldPos.z - _currentBounds.min.z) / _currentBounds.size.z);
                _tempData.Trees.Add(hurtTree);
            }
        }

        void CollectSplats()
        {
            _tempData.SplatData.Clear();
            if (!_splatsEnabled)
            {
                return;
            }

            var min = _focusedTerrain.WorldToSplatCoord(_currentBounds.min);
            var max = _focusedTerrain.WorldToSplatCoord(_currentBounds.max);

            int width = max.x - min.x;
            int height = max.z - min.z;

            //float minHeight = float.MaxValue;

            var prototypes = _focusedTerrain.terrainData.splatPrototypes;
            var wrappers = TerrainLayerUtilities.ResolvePrototypes(prototypes);

            if (prototypes.Length != wrappers.Count)
            {
                Debug.LogError("Failed to collect splats - possibly you have splat configs that aren't wrapper assets?");
                return;
            }

            var sampleSplats = _focusedTerrain.terrainData.GetAlphamaps(min.x, min.z, width, height);

            for (var i = 0; i < prototypes.Length; ++i)
            {
                var wrapper = wrappers[prototypes[i]];
                if (wrapper == null || _ignoredSplats.Contains(wrapper))
                {
                    continue;
                }

                var data = new byte[width, height];
                float sum = 0;
                for (var dx = 0; dx < width; ++dx)
                {
                    for (var dz = 0; dz < height; ++dz)
                    {
                        var val = sampleSplats[dz, dx, i];
                        data[dx, dz] = (byte)Mathf.Clamp(val * 255f, 0, 255);
                        sum += val;
                    }
                }
                if (sum < 0.01f)
                {
                    Debug.Log(string.Format("Ignored splat {0} as it appeared to be empty.", wrapper.name));
                    continue;
                }

                _tempData.SplatData.Add(new CompressedSplatData{ Wrapper = wrapper, Data = new Serializable2DByteArray(data)});
            }
        }

        void CollectDetails()
        {
            _tempData.DetailData.Clear();
            if (!_detailsEnabled)
            {
                return;
            }

            var min = _focusedTerrain.WorldToDetailCoord(_currentBounds.min);
            var max = _focusedTerrain.WorldToDetailCoord(_currentBounds.max);

            int width = max.x - min.x;
            int height = max.z - min.z;

            var prototypes = _focusedTerrain.terrainData.detailPrototypes;
            var wrappers = TerrainLayerUtilities.ResolvePrototypes(prototypes);

            if (prototypes.Length != wrappers.Count)
            {
                Debug.LogError("Failed to collect details - possibly you have detail configs that aren't wrapper assets?");
                return;
            }

            for (var i = 0; i < prototypes.Length; ++i)
            {
                var wrapper = wrappers[prototypes[i]];
                if (_ignoredDetails.Contains(wrapper))
                {
                    continue;
                }

                var sample = _focusedTerrain.terrainData.GetDetailLayer(min.x, min.z, width, height, i);
                var data = new byte[width, height];
                int sum = 0;
                for (var dx = 0; dx < width; ++dx)
                {
                    for (var dz = 0; dz < height; ++dz)
                    {
                        var sampleData = sample[dz, dx];
                        data[dx, dz] = (byte)sampleData;
                        sum += sampleData;
                    }
                }
                if (sum > 0)
                {
                    _tempData.DetailData.Add(new CompressedDetailData {Wrapper = wrapper, Data = new Serializable2DByteArray(data)});
                }
                else
                {
                    Debug.Log(string.Format("Ignored detail {0} as it appeared to be empty.", wrapper.name));
                }
            }
        }

        protected override void OnSceneGUI(SceneView sceneView)
        {
            if (_focusedTerrain == null)
            {
                return;
            }

            var max = _currentBounds.max;
            _currentBounds = new Bounds(_currentBounds.min, Vector3.zero);
            _currentBounds.Encapsulate(max);

            if (_currentStep == Step.SettingArea)
            {
                DoSetArea();
            }
            if (_currentStep == Step.Finalizing)
            {
                if (_tempData != null)
                {
                    DoMaskSceneGUI();

                    _tempData.Size = new Vector3(_currentBounds.size.x, _focusedTerrain.terrainData.size.y,
                        _currentBounds.size.z);

                    DoObjectSceneGUI();
                    DoHeightsSceneGUI();
                    DoTreesSceneGUI();
                    DoSplatsSceneGUI();
                    DoDetailsSceneGUI();
                }
            }

            Handles.color = Color.white;
            Handles.DrawWireCube(_currentBounds.center, _currentBounds.size);
            Handles.DrawWireCube(_focusedTerrain.GetPosition() + _focusedTerrain.terrainData.size / 2, _focusedTerrain.terrainData.size);
        }

        private void DoMaskSceneGUI()
        {
            if (_currentSceneGUIOwner != SceneGUIOwner.Mask)
            {
                if (_maskPainter != null)
                {
                    _maskPainter.Destroy();
                    _maskPainter = null;
                }
                return;
            }
            if (_maskPainter == null)
            {
                CreatePainter();
            }
            else
            {
                _tempData.GridSize = Math.Max(MinMaskRes, Math.Max(_currentBounds.size.x, _currentBounds.size.z) / MaskResolution);
                _maskPainter.GridManager = _tempData.GridManager;
                _maskPainter.Canvas = _tempData.Mask;
                _maskPainter.MaxValue = 1;
                _maskPainter.MinValue = 0;
                _maskPainter.Rect = new Rect(Vector2.zero, _currentBounds.size.xz());
                _maskPainter.TRS = Matrix4x4.TRS(_currentBounds.min, Quaternion.identity, Vector3.one);
                _maskPainter.Repaint();
                _maskPainter.PaintingEnabled = true;
                _maskPainter.OnSceneGUI();
            }
        }

        private void CreatePainter()
        {
            if (_tempData == null)
            {
                return;
            }
            _maskPainter = new Painter(_tempData.Mask, _tempData.GridManager);
            _maskPainter.Ramp = new Gradient()
            {
                colorKeys = new[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.black, 0.001f), new GradientColorKey(Color.black, 1), },
                alphaKeys = new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1), }
            };
            _maskPainter.Rect = new Rect(_currentBounds.min.xz(), _currentBounds.size.xz());
            //_maskPainter.Offset = -_currentBounds.min;
        }

        private void DoTreesSceneGUI()
        {
            if (_currentSceneGUIOwner != SceneGUIOwner.Trees)
            {
                return;
            }

            if (_tempData.Trees != null)
            {
                Handles.color = Color.green;
                foreach (var hurtTreeInstance in _tempData.Trees)
                {
                    var pos = new Vector3(hurtTreeInstance.Position.x * _currentBounds.size.x, 0,
                                  hurtTreeInstance.Position.z * _currentBounds.size.z) + _currentBounds.min;
                    pos.y += _focusedTerrain.SampleHeight(pos);
                    Handles.DrawDottedLine(pos, pos + Vector3.up * 10 * hurtTreeInstance.Scale.x, 1);
                }
            }
        }

        private void DoObjectSceneGUI()
        {
            if (_currentSceneGUIOwner != SceneGUIOwner.Objects)
            {
                return;
            }

            Handles.color = Color.white.WithAlpha(.5f);
            for (int i = 0; i < _tempData.Objects.Count; i++)
            {
                var b = _tempData.Objects[i];
                Bounds bounds;
                if(_objInSceneMapping.TryGetValue(b, out bounds))
                //if (!_tempData.IsMasked(_tempData.Objects[i]))
                {
                    Handles.DrawWireCube(bounds.center, bounds.size);
                }
            }
        }

        private void DoHeightsSceneGUI()
        {
            if (_currentSceneGUIOwner != SceneGUIOwner.Heights)
            {
                return;
            }

            var boundsMin = _currentBounds.min;
            var boundsSize = _currentBounds.size;
            var tHeight = _focusedTerrain.terrainData.size.y;
            var tYOffset = _focusedTerrain.GetPosition().y;

            var heightMin = _heightMin * tHeight + tYOffset;
            Handles.color = Color.blue;
            Handles.DrawWireCube(_currentBounds.center.xz().x0z(heightMin), _currentBounds.size.xz().x0z());

            var step = (1 / (float)_displayRes);
            var aspect = _currentBounds.size.x / _currentBounds.size.z;
            var stepX = step;
            var stepZ = step * aspect;
            if (_currentBounds.size.x > _currentBounds.size.z)
            {
                stepX = step * aspect;
                stepZ = step;
            }
            
            Handles.color = Color.white.WithAlpha(.4f);
            for (var dz = 0f; dz <= 1; dz += stepZ)
            {
                if (dz > 1)
                {
                    dz = 1;
                }
                var pz = dz * boundsSize.z;
                var pNeighbour1z = Mathf.Clamp01(dz + stepZ);

                for (var dx = 0f; dx <= 1; dx += stepX)
                {
                    if (dx > 1)
                    {
                        dx = 1;
                    }
                    var px = dx * boundsSize.x;
                    var pNeighbour1x = Mathf.Clamp01(dx + stepX);

                    var height = tYOffset + ((_tempData.Heights.BilinearSample(new Vector2(dx, dz)) + _heightMin) * _focusedTerrain.terrainData.size.y);
                    var maskVal = _tempData.Mask.GetBilinear(_tempData.GridManager, new Vector3(px, 0, pz));
                    height = Mathf.Lerp(heightMin, height, maskVal);

                    var p = new Vector3(boundsMin.x + px, height, boundsMin.z + pz);
                    Handles.CubeCap(-1, p, Quaternion.identity, 1);
                    
                    if (dz > 0)
                    {
                        var heightN1 = tYOffset + (_tempData.Heights.BilinearSample(new Vector2(dx, dz + stepZ)) + _heightMin) * tHeight;
                        maskVal = _tempData.Mask.GetBilinear(_tempData.GridManager, new Vector3(pNeighbour1x * boundsSize.x, 0, pz));
                        heightN1 = Mathf.Lerp(heightMin, heightN1, maskVal);
                        var p1 = new Vector3(boundsMin.x + pNeighbour1x * boundsSize.x, heightN1, boundsMin.z + pz);
                        Handles.DrawLine(p, p1);
                    }
                    if (dx > 0)
                    {
                        var heightN3 = tYOffset + (_tempData.Heights.BilinearSample(new Vector2(dx + stepZ, dz)) + _heightMin) * tHeight;
                        maskVal = _tempData.Mask.GetBilinear(_tempData.GridManager, new Vector3(px, 0, pNeighbour1z * boundsSize.z));
                        heightN3 = Mathf.Lerp(heightMin, heightN3, maskVal);
                        var p3 = new Vector3(boundsMin.x + px, heightN3, boundsMin.z + pNeighbour1z * boundsSize.z);
                        Handles.DrawLine(p, p3);
                    }
                }
            }
        }

        private void DoSplatsSceneGUI()
        {
            if (_currentSceneGUIOwner != SceneGUIOwner.Splats)
            {
                return;
            }

            int counter = 0;
            int res = 32;
            foreach (var kvp in _tempData.SplatData)
            {
                int xStep = kvp.Data.Width / res;
                int zStep = kvp.Data.Height / res;
                Vector2 cellSize = new Vector2((xStep / (float)kvp.Data.Width) * _currentBounds.size.x, (zStep / (float)kvp.Data.Height) * _currentBounds.size.z);
                for (var u = 0; u < kvp.Data.Width; u += xStep)
                {
                    var fU = u / (float)kvp.Data.Width;
                    var wU = _currentBounds.min.x + fU*_currentBounds.size.x;
                    for (var v = 0; v < kvp.Data.Height; v += zStep)
                    {
                        var fV = v / (float)kvp.Data.Height;
                        var wV = _currentBounds.min.z + fV * _currentBounds.size.z;

                        var val = kvp.Data[u, v] / 255f;
                        HandleExtensions.DrawXZCell(new Vector3(wU, counter, wV), cellSize,
                            Quaternion.identity, ColorUtils.GetIndexColor(counter).WithAlpha(val));
                    }
                }
                counter++;
            }
            
        }

        private void DoDetailsSceneGUI()
        {
            if (_currentSceneGUIOwner != SceneGUIOwner.Details)
            {
                return;
            }

            int counter = 0;
            int res = 32;
            foreach (var kvp in _tempData.DetailData)
            {
                int xStep = kvp.Data.Width / res;
                int zStep = kvp.Data.Height / res;
                Vector2 cellSize = new Vector2((xStep / (float)kvp.Data.Width) * _currentBounds.size.x, (zStep / (float)kvp.Data.Height) * _currentBounds.size.z);
                for (var u = 0; u < kvp.Data.Width; u += xStep)
                {
                    var fU = u / (float)kvp.Data.Width;
                    var wU = _currentBounds.min.x + fU * _currentBounds.size.x;
                    for (var v = 0; v < kvp.Data.Height; v += zStep)
                    {
                        var fV = v / (float)kvp.Data.Height;
                        var wV = _currentBounds.min.z + fV * _currentBounds.size.z;

                        var val = kvp.Data[u, v] / 255f;
                        HandleExtensions.DrawXZCell(new Vector3(wU, counter, wV), cellSize,
                            Quaternion.identity, ColorUtils.GetIndexColor(counter).WithAlpha(val));
                    }
                }
                counter++;
            }

        }

        private void DoSetArea()
        {
            _currentBounds.min = Handles.DoPositionHandle(_currentBounds.min, Quaternion.identity).Flatten();
            _currentBounds.max = Handles.DoPositionHandle(_currentBounds.max, Quaternion.identity).Flatten();
            _currentBounds.min = _focusedTerrain.HeightmapCoordToWorldPos(_focusedTerrain.WorldToHeightmapCoord(_currentBounds.min, TerrainX.RoundType.Floor)).Flatten();
            _currentBounds.max = _focusedTerrain.HeightmapCoordToWorldPos(_focusedTerrain.WorldToHeightmapCoord(_currentBounds.max, TerrainX.RoundType.Floor)).Flatten();
        }
    }
}
