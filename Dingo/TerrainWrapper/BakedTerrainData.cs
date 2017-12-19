using System.Collections.Generic;
using Dingo.Common.Collections;
using Dingo.Terrains.Lookups;
using Dingo.WorldStamp;
using Dingo.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dingo.Terrains
{
    /// <summary>
    /// Basically the combined data of a series of layers
    /// TODO: we could possibly store a compound for EVERY layer, and then recalc off that
    /// These maps are getting very large very fast, however
    /// We put this data in an in-scene scriptable object
    /// Otherwise we get some very expensive OnBeforeSerialize with the terrainWrapper OnGUI
    /// </summary>
    public class BakedTerrainData : ScriptableObject
    {
        // Baked data
        //public List<GameObject> OwnedInstantiatedObjects = new List<GameObject>();
        public ObjectPrefabDataLookup Objects = new ObjectPrefabDataLookup();
        public TreeLookup Trees = new TreeLookup();
        public CompressedSplatDataLookup SplatData = new CompressedSplatDataLookup();
        public CompressedDetailDataLookup DetailData = new CompressedDetailDataLookup();
        public Serializable2DFloatArray Heights;

        public void Clear(TerrainWrapper terrainWrapper)
        {
            if (terrainWrapper.WriteObjects)
            {
                Objects.Clear();
            }
            if (terrainWrapper.WriteTrees)
            {
                Trees.Clear();
            }
            if (terrainWrapper.WriteDetails)
            {
                DetailData.Clear();
            }
            if (terrainWrapper.WriteSplats)
            {
                SplatData.Clear();
            }
            if (terrainWrapper.WriteHeights && Heights != null)
            {
                Heights.Clear();
            }
        }
    }

    public class CompoundTerrainLayer
    {
        public Serializable2DFloatArray Heights;
        public List<PrefabObjectData> Objects = new List<PrefabObjectData>();
        public Dictionary<SplatPrototypeWrapper, Serializable2DByteArray> SplatData = new Dictionary<SplatPrototypeWrapper, Serializable2DByteArray>();
        public Dictionary<DetailPrototypeWrapper, Serializable2DByteArray> DetailData = new Dictionary<DetailPrototypeWrapper, Serializable2DByteArray>();
        public List<DingoTreeInstance> Trees = new List<DingoTreeInstance>();
    }
}