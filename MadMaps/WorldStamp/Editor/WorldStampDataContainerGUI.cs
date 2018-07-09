using UnityEditor;
using UnityEngine;

namespace MadMaps.WorldStamps
{
    [CustomEditor(typeof(WorldStampDataContainer))]
    [CanEditMultipleObjects]
    public class WorldStampDataContainerGUI : Editor
    {
        /*public override void OnInspectorGUI()
        {
            var wsdc = target as WorldStampDataContainer;
            DoDataGUI(wsdc);
        }
        
        public static void DoDataGUI(WorldStampDataContainer wsdc)
        {
            if (PrefabUtility.GetPrefabType(wsdc) == PrefabType.Prefab)
            {
                return;
            }
            if (wsdc.Redirect == null)
            {
                wsdc.Redirect = null;

                var prefabParent = PrefabUtility.GetPrefabParent(wsdc.transform.parent.gameObject);
                if (prefabParent == null)
                {
                    EditorGUILayout.HelpBox("Null Prefab!", MessageType.Warning);
                    return;
                }
                var prefabGo = prefabParent as GameObject;
                if (prefabGo == null)
                {
                    return;
                }
                var prefabWsdc = prefabGo.GetComponentInChildren<WorldStampDataContainer>();
                if (prefabWsdc == null)
                {
                    EditorGUILayout.HelpBox("Null Data On Prefab!", MessageType.Warning);
                    return;
                }
                if (GUILayout.Button("Turn Into Proxy"))
                {
                    TurnIntoProxy(wsdc);
                }
            }
            else
            {
                wsdc.Data = null;
                if (GUILayout.Button("Turn Into Instance"))
                {
                    TurnIntoInstance(wsdc);
                }
            }
        }
        */
        public static void TurnIntoProxy(WorldStampDataContainer wsdc)
        {
            if (!EditorUtility.DisplayDialog("Are you sure?",
                "This will delete any instance data you changed on this object.", "Yes", "No"))
            {
                return;
            }

            var prefabParent = PrefabUtility.GetPrefabParent(wsdc.transform.parent.gameObject);
            if (prefabParent == null)
            {
                Debug.LogError("Unable to find prefab");
                return;
            }
            var prefabGo = prefabParent as GameObject;
            if (prefabGo == null)
            {
                Debug.LogError("Unable to find prefab");
                return;
            }
            var prefabWsdc = prefabGo.GetComponentInChildren<WorldStampDataContainer>();
            if (prefabWsdc == null)
            {
                Debug.LogError("Unable to find data container on prefab");
                return;
            }
            wsdc.Redirect = prefabWsdc;
            wsdc.Data = null;
        }

        public static void TurnIntoInstance(WorldStampDataContainer wsdc)
        {
            wsdc.Data = JsonUtility.FromJson<WorldStampData>(JsonUtility.ToJson(wsdc.Redirect.Data));
            wsdc.Redirect = null;
        }

        protected override void OnHeaderGUI()
        {
        }
    }
}