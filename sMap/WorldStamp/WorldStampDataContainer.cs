using UnityEngine;

namespace sMap.WorldStamp
{
    [StripComponentOnBuild]
    public class WorldStampDataContainer : MonoBehaviour
    {
        public WorldStampData GetData()
        {
            if (Redirect == this)
            {
                Redirect = null;
            }
            if (Redirect != null && Redirect.Data != null)
            {
                return Redirect.GetData();
            }
            return Data;
        }

        [HideInInspector]
        public WorldStampData Data;
        public WorldStampDataContainer Redirect;

        public void LinkToPrefab()
        {
#if UNITY_EDITOR
            var prefabParent = UnityEditor.PrefabUtility.GetPrefabParent(transform.parent.gameObject);
            if (prefabParent == null)
            {
                //Debug.LogError("Unable to find prefab");
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
            Redirect = prefabWsdc;
            Data = null;
#endif
        }
    }
}