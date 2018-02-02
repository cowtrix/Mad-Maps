using UnityEngine;

namespace MadMaps.Terrains
{
    public class TerrainSurfaceObjectApplier : ProceduralLayerComponent
    {
        public float RequiredY = .9f;

        public override ApplyTiming Timing
        {
            get { return ApplyTiming.OnFrameAfterPostFinalise; }
        }

        public override void Apply(ProceduralLayer layer, TerrainWrapper wrapper)
        {
            var obj = wrapper.ObjectContainer;
            if (!obj)
            {
                return;
            }

            wrapper.GetComponent<TerrainCollider>().enabled = false;
            wrapper.GetComponent<TerrainCollider>().enabled = true;

            var children = obj.GetComponentsInChildren<TerrainSurfaceObject>();
            for (int i = 0; i < children.Length; i++)
            {
                var terrainSurfaceObject = children[i];
                if (!terrainSurfaceObject)
                {
                    continue;
                }

                if (terrainSurfaceObject.transform.up.y < RequiredY)
                {
                    continue;
                }
                terrainSurfaceObject.Recalculate();
            }
            Debug.Log(string.Format("Recalculated {0} TerrainSurfaceObjects", children.Length));
        }
    }
}