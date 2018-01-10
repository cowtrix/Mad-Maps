using System;
using System.Collections;
using MapMagic;
using UnityEngine;

namespace Dingo.Terrains.MapMagic
{
    [System.Serializable]
    [GeneratorMenu(menu = "Output", name = "Terrain Wrapper", disengageable = true)]
    public class TerrainWrapperOutput : OutputGenerator
    {
        public enum ELayerSelectionMode
        {
            Name,
            Reference,
        }

        public ELayerSelectionMode LayerSelectionMode;
        public string LayerName = "MapMagic Layer";
        public TerrainLayer LayerRef;

        public override void OnGUI(GeneratorsAsset gens)
        {
            layout.Field(ref LayerSelectionMode, "Layer Mode");
            if (LayerSelectionMode == ELayerSelectionMode.Name)
            {
                layout.Field(ref LayerName, "Layer");
            }
            else
            {
                layout.Field(ref LayerRef, "Layer");
            }
        }

        public override Action<global::MapMagic.CoordRect, Chunk.Results, GeneratorsAsset, Chunk.Size, Func<float, bool>> GetProces()
        {
            return Process;
        }
        
        public override Func<global::MapMagic.CoordRect, Terrain, object, Func<float, bool>, IEnumerator> GetApply()
        {
            return Apply;
        }
        
        public override Action<global::MapMagic.CoordRect, Terrain> GetPurge()
        {
            return Purge;
        }

        private static void Purge(global::MapMagic.CoordRect coordRect, Terrain terrain)
        {
        }

        private static void Process(global::MapMagic.CoordRect p1, Chunk.Results p2, GeneratorsAsset p3, Chunk.Size p4, Func<float, bool> p5)
        {

        }

        private static IEnumerator Apply(global::MapMagic.CoordRect coordRect, Terrain terrain, object dataBox, Func<float, bool> stop = null)
        {
            yield break;
        }
    }
}