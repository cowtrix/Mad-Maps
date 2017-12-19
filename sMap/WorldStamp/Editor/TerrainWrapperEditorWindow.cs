using sMap.Common;
using sMap.Terrains;
using UnityEditor;

namespace sMap.Terrains
{
    public class TerrainWrapperEditorWindow : EditorWindow
    {
        public TerrainWrapperGUI GUI;
        public TerrainWrapper Wrapper;

        /*[MenuItem("Window/Terrain Wrapper Inspector")]
        public static void OpenWindow()
        {
            var w = GetWindow<TerrainWrapperEditorWindow>();
        }*/

        void OnGUI()
        {
            if (GUI == null)
            {
                GUI = CreateInstance<TerrainWrapperGUI>();
                GUI.Wrapper = Wrapper ?? (TerrainWrapper)FindObjectOfType(typeof(TerrainWrapper));
                GUI.OnEnable();
            }

            GUI.Wrapper = Wrapper;
            GUI.IsPopout = true;
            ((Editor) GUI).OnInspectorGUI();
        }
    }
}