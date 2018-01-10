using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    public class WorldStampCaptureTemplateContainer : MonoBehaviour
    {
        public WorldStampCaptureTemplate Template;

        public void OnDrawGizmosSelected()
        {
            GizmoExtensions.DrawWireCube(Template.Bounds.center, Template.Bounds.size/2, Quaternion.identity, Color.white);
        }
    }
}