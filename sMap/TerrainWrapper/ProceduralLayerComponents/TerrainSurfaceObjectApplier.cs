namespace sMap.Terrains
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
        }
    }
}