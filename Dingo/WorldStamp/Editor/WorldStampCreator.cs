using System.Linq;
using Dingo.Common;
using UnityEditor;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    public class WorldStampCreator : SceneViewEditorWindow
    {
        [MenuItem("Tools/Dingo/World Stamp Creator", false, 6)]
        [MenuItem("Tools/Dingo/World Stamp Creator", false, 6)]
        public static void OpenWindow()
        {
            var w = GetWindow<WorldStampCreator>();
            w.titleContent = new GUIContent("World Stamp Creator");
        }

        public Bounds Bounds;
        public Terrain Terrain;

        private WorldStampCreatorLayer[] _creators = new WorldStampCreatorLayer[]
        {
            new HeightmapLayer(), 
            new SplatDataCreator(), 
            new DetailDataCreator(), 
            new TreeDataCreator(), 
            new ObjectDataCreator(),
            new MaskDataCreator(),
        };

        public WorldStampCreatorLayer SceneGUIOwner;

        void OnEnable()
        {
            if (Terrain != null)
            {
                return;
            }
            var currentTerrain = Terrain.activeTerrain;
            if (!currentTerrain)
            {
                return;
            }
            Terrain = currentTerrain;
            Bounds = Terrain.GetBounds();
        }

        public T GetCreator<T>() where T: WorldStampCreatorLayer
        {
            return _creators.First(layer => layer is T) as T;
        }
        
        protected void OnGUI()
        {
            Terrain = (Terrain) EditorGUILayout.ObjectField("Target Terrain", Terrain, typeof (Terrain), true);
            if (Terrain == null)
            {
                EditorGUILayout.HelpBox("Please Select a Target Terrain", MessageType.Info);
                return;
            }

            for (int i = 0; i < _creators.Length; i++)
            {
                _creators[i].DrawGUI(this);
                if (_creators[i].NeedsRecapture)
                {
                    _creators[i].Capture(Terrain, Bounds);
                }
            }

            EditorGUILayout.LabelField("", GUILayout.ExpandHeight(true));
            EditorExtensions.Seperator();
            if (GUILayout.Button("Create New Stamp"))
            {
                GameObject go = new GameObject("New WorldStamp");
                go.transform.position = Bounds.center + Vector3.up * GetCreator<HeightmapLayer>().HeightMin * Terrain.terrainData.size.y;
                var stamp = go.AddComponent<WorldStamp>();
                var data = new WorldStampData();
                foreach (var worldStampCreatorLayer in _creators)
                {
                    worldStampCreatorLayer.Commit(data);
                }
                stamp.SetData(data);
                stamp.HaveHeightsBeenFlipped = true;
                EditorGUIUtility.PingObject(stamp);
            }
        }

        protected override void OnSceneGUI(SceneView sceneView)
        {
            if (Terrain == null)
            {
                return;
            }

            if (DoSetArea())
            {
                foreach (var worldStampCreatorLayer in _creators)
                {
                    worldStampCreatorLayer.NeedsRecapture = true;
                }
            }

            if (SceneGUIOwner != null)
            {
                SceneGUIOwner.PreviewInScene(this);
            }

            Handles.color = Color.white;
            Handles.DrawWireCube(Bounds.center, Bounds.size);
            Handles.DrawWireCube(Terrain.GetPosition() + Terrain.terrainData.size / 2, Terrain.terrainData.size);
        }
        
        private bool DoSetArea()
        {
            var b = Bounds;
            var tb = Terrain.GetBounds();

            b.min = Handles.DoPositionHandle(b.min, Quaternion.identity).Flatten();
            b.max = Handles.DoPositionHandle(b.max.xz().x0z(b.min.y), Quaternion.identity).Flatten();
            b.min = Terrain.HeightmapCoordToWorldPos(Terrain.WorldToHeightmapCoord(b.min, TerrainX.RoundType.Floor)).Flatten();
            b.max = Terrain.HeightmapCoordToWorldPos(Terrain.WorldToHeightmapCoord(b.max, TerrainX.RoundType.Floor)).Flatten();

            b.Encapsulate(b.center.xz().x0z(tb.max.y));
            b.Encapsulate(b.center.xz().x0z(tb.min.y));
            if (b != Bounds)
            {
                Bounds = b;
                return true;
            }
            return false;
        }
    }
}
