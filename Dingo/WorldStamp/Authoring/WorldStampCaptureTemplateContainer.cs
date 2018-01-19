using Dingo.Common.Collections;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
#if HURTWORLDSDK
    [StripComponentOnBuild(DestroyGameObject = true)]
#endif
    public class WorldStampCaptureTemplateContainer : MonoBehaviour
    {
        [HideInInspector]
        public Serializable2DFloatArray Mask;
        public Vector3 Size;
        public WorldStampCaptureTemplate Template;

        public void OnDrawGizmos()
        {
            GizmoExtensions.DrawWireCube(transform.position + Vector3.up * (Size.y/2), Size / 2, Quaternion.identity, Color.white);
        }
    }
}