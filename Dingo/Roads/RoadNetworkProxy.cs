#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Dingo.Roads
{
#if UNITY_EDITOR
    [CustomEditor(typeof(RoadNetworkProxy))]
    public class RoadNetworkProxyGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            var rnp = target as RoadNetworkProxy;
            var newTarget = EditorGUILayout.ObjectField("Network", rnp.Network, typeof (RoadNetwork), true) as RoadNetwork;
            if (rnp.Network != newTarget)
            {
                rnp.Network = newTarget;
                if (newTarget)
                {
                    newTarget.ForceThink();
                }
            }
        }
    }
#endif

    public class RoadNetworkProxy : MonoBehaviour
    {
        public RoadNetwork Network;
    }
}