#if VEGETATION_STUDIO
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies;
using AwesomeTechnologies.Common;
using AwesomeTechnologies.Billboards;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using MadMaps.Common;
using MadMaps.Common.Collections;
using MadMaps.Common.GenericEditor;
using MadMaps.Common.Serialization;
using MadMaps.Terrains.Lookups;
using MadMaps.WorldStamp;
using UnityEngine;
using System.Linq;

namespace MadMaps.Terrains
{
    [Serializable]
    public class VegetationStudioInstance
    {
        public string Guid;
        public string VSID;
        public Vector3 Position;
        public Vector3 Scale = Vector3.one;
        public Vector3 Rotation;

        public VegetationStudioInstance Clone()
        {
            return new VegetationStudioInstance()
            {
                Guid = this.Guid,
                VSID = this.VSID,
                Position = this.Position,
                Scale = this.Scale,
                Rotation = this.Rotation,
            };
        }
    }

    [Serializable]
    public class VegetationStudioLookup : CompositionDictionary<string, VegetationStudioInstance>
    {
        private const int Partitioning = 256;

        [Serializable]
        public class SpatialMapping : CompositionDictionary<Coord, List<string>>{}

        [SerializeField]
        private SpatialMapping _mapping = new SpatialMapping();

        public Coord PositionToCoord(Vector3 pos)
        {
            int x = Mathf.FloorToInt(Mathf.Clamp01(pos.x)*Partitioning);
            int z = Mathf.FloorToInt(Mathf.Clamp01(pos.z)*Partitioning);
            return new Coord(x, z);
        }

        public void AppendPartitionList(Coord coord, List<string> toAppend)
        {
            if (toAppend == null)
            {
                toAppend = new List<string>();
            }
            List<string> partitionList;
            if (_mapping.TryGetValue(coord, out partitionList))
            {
                toAppend.AddRange(partitionList);
            }
        } 

        public override void Add(string key, VegetationStudioInstance value)
        {
            var coord = PositionToCoord(value.Position);
            List<string> partitionList;
            if (!_mapping.TryGetValue(coord, out partitionList))
            {
                partitionList = new List<string>();
                _mapping[coord] = partitionList;
            }
            base.Add(key, value);
            if (partitionList.Contains(value.Guid))
            {
                throw new Exception("Same tree GUID in spatial mapping???");
            }
            partitionList.Add(value.Guid);
        }

        public override bool Remove(string key)
        {
            VegetationStudioInstance value;
            if (TryGetValue(key, out value))
            {
                var coord = PositionToCoord(value.Position);
                List<string> partitionList;
                if (_mapping.TryGetValue(coord, out partitionList))
                {
                    partitionList.Remove(key);
                }
            }
            return base.Remove(key);
        }

        public override void Clear()
        {
            _mapping.Clear();
            base.Clear();
        }
    }

    public static class VegetationStudioUtilities
    {
        public static List<VegetationSystem> GetVegetationSystemsForTerrain(Terrain terrain)
        {
            var result = new List<VegetationSystem>();
            var vegSystems = UnityEngine.Object.FindObjectsOfType<VegetationSystem>();
            foreach(var vegSys in vegSystems)
            {
                if(vegSys.GetTerrain() == terrain)
                {
                    result.Add(vegSys);
                }
            }
            return result;
        }

        public static void SnapshotVegetationStudioData(this TerrainLayer layer, Terrain terrain)
        {
            layer.VSRemovals.Clear();
            layer.VSInstances.Clear();

            var vegStorages = UnityEngine.Object.FindObjectsOfType<PersistentVegetationStorage>();
            foreach(var vegStorage in vegStorages)
            {
                var vegSys = vegStorage.GetComponent<VegetationSystem>();
                if(vegSys.GetTerrain() != terrain)
                {
                    continue;
                }
                foreach(var cell in vegStorage.PersistentVegetationStoragePackage.PersistentVegetationCellList)
                {
                    foreach(var info in cell.PersistentVegetationInfoList)
                    {
                        foreach(var item in info.VegetationItemList)
                        {
                            layer.VSInstances.Add(new VegetationStudioInstance()
                            {
                                Guid = System.Guid.NewGuid().ToString(),
                                Position = terrain.WorldToTreePos(terrain.GetPosition() + item.Position),
                                Rotation = item.Rotation.eulerAngles,
                                Scale = item.Scale,
                                VSID = info.VegetationItemID,
                            });
                        }
                    }
                }
            }
        }
    }
  
    public partial class TerrainLayer : LayerBase
    {
        const byte VS_ID = 123;
        public List<VegetationStudioInstance> VSInstances = new List<VegetationStudioInstance>();
        public List<string> VSRemovals = new List<string>();

        private void WriteVegetationStudioToTerrain(TerrainWrapper wrapper, Bounds bounds)
        {
            var existingVSData = wrapper.CompoundTerrainData.VegetationStudio;
            if (BlendMode == ETerrainLayerBlendMode.Set)
            {
                existingVSData.Clear();
            }
            for (var i = 0; i < VSInstances.Count; i++)
            {
                existingVSData.Add(VSInstances[i].Guid, VSInstances[i]);
            }
            foreach (var treeRemoval in TreeRemovals)
            {
                existingVSData.Remove(treeRemoval);
            }
        }

        public override List<VegetationStudioInstance> GetVegetationStudioData()
        {
            return VSInstances;
        }

        public override List<string> GetVegetationStudioRemovals()
        {
            return VSRemovals;
        }

    }
}
#endif