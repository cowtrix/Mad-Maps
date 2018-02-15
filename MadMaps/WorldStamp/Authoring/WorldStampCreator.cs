#if UNITY_EDITOR

using System.Linq;
using MadMaps.Common;
using UnityEditor;
using UnityEngine;
using EditorGUILayoutX = MadMaps.Common.EditorGUILayoutX;

namespace MadMaps.WorldStamp.Authoring
{
    public class WorldStampCreator : SceneViewEditorWindow
    {
        [MenuItem("Tools/Mad Maps/World Stamp Creator", false, 6)]
        public static void OpenWindow()
        {
            var w = GetWindow<WorldStampCreator>();
            w.titleContent = new GUIContent("Stamp Creator");
        }

        public bool BoundsLocked;
        public WorldStampCaptureTemplate Template = new WorldStampCaptureTemplate();
        public WorldStampCreatorLayer SceneGUIOwner;

        public WorldStamp TargetInjectionStamp;
        public WorldStampTemplate TargetInjectionTemplate;
        /*
        private GUIContent _createStampTemplateContent = new GUIContent("Create Stamp Template", "Create an in-scene object to preserve stamp capture settings.");
        */
        void OnSelectionChange()
        {
            if (!Selection.activeGameObject)
            {
                return;
            }
            var t = Selection.activeGameObject.GetComponent<Terrain>();
            if (!t)
            {
                return;
            }
            Template.Terrain = t;
        }

        void OnEnable()
        {
            if (Template.Terrain != null)
            {
                return;
            }
            var currentTerrain = Terrain.activeTerrain;
            if (!currentTerrain)
            {
                return;
            }
            Template.Terrain = currentTerrain;
            Template.Bounds = Template.Terrain.GetBounds();
        }
        
        void OnDisable()
        {
            for (int i = 0; i < Template.Creators.Count; i++)
            {
                Template.Creators[i].Dispose();
            }
        }

        public T GetCreator<T>() where T: WorldStampCreatorLayer
        {
            return Template.Creators.First(layer => layer is T) as T;
        }
        
        protected void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            Template.Terrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain", Template.Terrain, typeof(Terrain), true);
            EditorExtensions.HelpButton("http://lrtw.net/madmaps/index.php?title=World_Stamp_Creator");
            EditorGUILayout.EndHorizontal();
            if (Template.Terrain == null)
            {
                EditorGUILayout.HelpBox("Please Select a Target Terrain", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Capture Area:", Template.Bounds.size.xz().ToString());
            if (GUILayout.Button(new GUIContent(BoundsLocked ? GUIResources.LockedIcon : GUIResources.UnlockedIcon, "Lock Bounds"), EditorStyles.label,
                GUILayout.Width(18), GUILayout.Height(18)))
            {
                BoundsLocked = !BoundsLocked;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);
            var recaptureContent = new GUIContent("Capture");
            recaptureContent.tooltip = "Recapture this data.";
            GUI.color = Template.Creators.Any(layer => layer.NeedsRecapture) ? Color.Lerp(Color.red, Color.white, .5f) : Color.white;
            if (GUILayout.Button(recaptureContent, EditorStyles.miniButton, GUILayout.Height(16)))
            {
                for (int i = 0; i < Template.Creators.Count; i++)
                {
                    var worldStampCreatorLayer = Template.Creators[i];
                    worldStampCreatorLayer.Capture(Template.Terrain, Template.Bounds);
                }
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            
            for (int i = 0; i < Template.Creators.Count; i++)
            {
                Template.Creators[i].DrawGUI(this);
            }

            EditorGUILayout.LabelField("", GUILayout.ExpandHeight(true));
            EditorExtensions.Seperator();

            if (GUILayout.Button("Create Stamp Template"))
            {
                var mask = GetCreator<MaskDataCreator>();
                var newTemplate = new GameObject("Stamp Template");
                var temp = newTemplate.AddComponent<WorldStampTemplate>();
                temp.transform.position = Template.Bounds.center.xz().x0z(Template.Bounds.min.y);
                temp.Mask = mask.GetArrayFromMask(this);
                temp.Template = Template.JSONClone();
                temp.Size = Template.Bounds.size;
            }

            GUILayout.BeginHorizontal();
            TargetInjectionTemplate = (WorldStampTemplate) EditorGUILayout.ObjectField(TargetInjectionTemplate,
                typeof (WorldStampTemplate), true);
            GUI.enabled = TargetInjectionTemplate;
            if (GUILayout.Button("Replace Existing Stamp Template", GUILayout.Width(220)))
            {
                var mask = GetCreator<MaskDataCreator>();
                var temp = TargetInjectionTemplate;
                temp.transform.position = Template.Bounds.center.xz().x0z(Template.Bounds.min.y);
                temp.Mask = mask.GetArrayFromMask(this);
                temp.Size = Template.Bounds.size;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            EditorExtensions.Seperator();

            Template.Layer = EditorGUILayout.LayerField("Create Stamp On Layer:", Template.Layer);
            if (GUILayout.Button("Create New Stamp"))
            {
                if (Template.Creators.Any(layer => layer.Enabled && layer.NeedsRecapture) && EditorUtility.DisplayDialog("Layer {0} Needs Recapture",
                        string.Format("We need to recapture the terrain. Do this now?"), "Yes", "No"))
                {
                    for (int i = 0; i < Template.Creators.Count; i++)
                    {
                        var layer = Template.Creators[i];
                        if (layer.Enabled && layer.NeedsRecapture)
                        {
                            layer.Capture(Template.Terrain, Template.Bounds);
                        }
                    }
                }

                GameObject go = new GameObject("New WorldStamp");
                
                go.transform.position = Template.Bounds.center.xz().x0z(Template.Bounds.min.y) + Vector3.up * GetCreator<HeightmapDataCreator>().ZeroLevel * Template.Terrain.terrainData.size.y;
                var stamp = go.AddComponent<WorldStamp>();
                var data = new WorldStampData();
                foreach (var layer in Template.Creators)
                {
                    if (layer.Enabled)
                    {
                        layer.Commit(data, stamp);
                    }
                }
                data.Size = Template.Bounds.size;
                stamp.SetData(data);
                stamp.HaveHeightsBeenFlipped = true;
                go.transform.SetLayerRecursive(Template.Layer);
                
                EditorGUIUtility.PingObject(stamp);
            }
            EditorGUILayout.BeginHorizontal();
            TargetInjectionStamp = (WorldStamp) EditorGUILayout.ObjectField(TargetInjectionStamp, typeof (WorldStamp), true);
            GUI.enabled = TargetInjectionStamp;
            if (GUILayout.Button("Replace Existing Stamp", GUILayout.Width(220)))
            {
                var data = new WorldStampData();
                foreach (var worldStampCreatorLayer in Template.Creators)
                {
                    worldStampCreatorLayer.Commit(data, TargetInjectionStamp);
                }
                data.Size = Template.Bounds.size;
                TargetInjectionStamp.SetData(data);
                TargetInjectionStamp.HaveHeightsBeenFlipped = true;
                TargetInjectionStamp.gameObject.layer = Template.Layer;
                EditorGUIUtility.PingObject(TargetInjectionStamp);
                EditorUtility.SetDirty(TargetInjectionStamp);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        protected override void OnSceneGUI(SceneView sceneView)
        {
            if (Template.Terrain == null)
            {
                return;
            }

            if (!BoundsLocked && DoSetArea())
            {
                foreach (var worldStampCreatorLayer in Template.Creators)
                {
                    worldStampCreatorLayer.NeedsRecapture = true;
                }
            }

            if (!Template.Creators.Contains(SceneGUIOwner))
            {
                SceneGUIOwner = null;
            }

            if (SceneGUIOwner != null)
            {
                SceneGUIOwner.PreviewInScene(this);
            }

            Handles.color = Color.white;
            Handles.DrawWireCube(Template.Bounds.center, Template.Bounds.size);
            Handles.color = Color.white.WithAlpha(.5f);
            Handles.DrawWireCube(Template.Terrain.GetPosition() + Template.Terrain.terrainData.size / 2, Template.Terrain.terrainData.size);
        }

        public static Bounds ClampBounds(Terrain Terrain, Bounds b)
        {
            var tb = Terrain.GetBounds();
            
            b.min = Terrain.HeightmapCoordToWorldPos(Terrain.WorldToHeightmapCoord(b.min, TerrainX.RoundType.Round)).xz().x0z(tb.min.y);
            b.max = Terrain.HeightmapCoordToWorldPos(Terrain.WorldToHeightmapCoord(b.max, TerrainX.RoundType.Round)).xz().x0z(tb.min.y);

            b.Encapsulate(b.center.xz().x0z(tb.max.y));
            b.Encapsulate(b.center.xz().x0z(tb.min.y));
            
            return b;
        }
        
        private bool DoSetArea()
        {
            var b = Template.Bounds;
            b.min = Handles.DoPositionHandle(b.min, Quaternion.identity);
            b.max = Handles.DoPositionHandle(b.max.xz().x0z(b.min.y), Quaternion.identity);
            
            b = ClampBounds(Template.Terrain, b);
            if (b != Template.Bounds)
            {
                Template.Bounds = b;
                return true;
            }

            return false;
        }
    }
}

#endif