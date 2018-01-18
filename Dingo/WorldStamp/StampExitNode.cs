using Dingo.Common;
using Dingo.Roads;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    [ExecuteInEditMode]
#if HURTWORLDSDK
    [StripComponentOnBuild]
#endif
    public class StampExitNode : NodeComponent
#if HURTWORLDSDK
        , ILevelPreBuildStepCallback
#endif
    {
        public static Texture2D Icon
        {
            get
            {
                if (__icon == null)
                {
                    __icon = Resources.Load<Texture2D>("StampExitNodeIcon");
                }
                return __icon;
            }
        }
        private static Texture2D __icon;

        public static bool IsCommiting;

        [SerializeField] [HideInInspector] private Vector3 _nodeControl;

        public void Update()
        {
            if(!Node)
                return;
            _nodeControl = Node.GetNodeControl(null);
        }

        public override void Strip()
        {
            var n = gameObject.AddComponent<Node>();
            n.Configuration.ExplicitControl = _nodeControl;
            n.Configuration.IsExplicitControl = true;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            EditorExtensions.DrawArrow(transform.position, transform.position + _nodeControl, Color.green, 1);
        }
#endif

#if HURTWORLDSDK
        public void OnLevelPreBuildStep()
        {
            var allMeshColliders = transform.GetComponentsInChildren<MeshCollider>();
            for (int i = 0; i < allMeshColliders.Length; i++)
            {
                allMeshColliders[i].enabled = true;
            }
        }
#endif
    }
}