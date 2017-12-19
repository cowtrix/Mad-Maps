using UnityEditor;
using UnityEngine;

namespace Dingo.Roads
{
    [CustomEditor(typeof(RoadNetwork))]
    [CanEditMultipleObjects]
    public class RoadNetworkGUI : Editor
    {
        private RoadNetworkWindow _currentWindow;

        [MenuItem("GameObject/3D Object/sRoad Network")]
        public static void CreateInstance()
        {
            var go = new GameObject("sRoad Network");
            go.transform.position = Vector3.zero;
            go.AddComponent<RoadNetwork>();
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor Window"))
            {
                _currentWindow = EditorWindow.GetWindow<RoadNetworkWindow>();
                _currentWindow.name = "sRoads";
            }
        }
    }
}