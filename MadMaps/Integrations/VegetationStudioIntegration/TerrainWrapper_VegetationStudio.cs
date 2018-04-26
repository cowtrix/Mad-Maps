using System.Linq;
using System.Collections.Generic;
using MadMaps.Common;
using MadMaps.Common.Collections;
using MadMaps.WorldStamp;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MadMaps.Terrains
{
    public partial class TerrainWrapper 
    {

        public List<VegetationStudioInstance> GetCompoundVegetationStudioData(LayerBase terminatingLayer, bool includeTerminatingLayer = false, Bounds? bounds = null)
        {
            if (!includeTerminatingLayer && _compoundDataCache.ContainsKey(terminatingLayer))
            {
                var data = _compoundDataCache[terminatingLayer].VegetationStudio;
                return data; 
            }

            var result = new Dictionary<string, VegetationStudioInstance>();
            for (var i = Layers.Count - 1; i >= 0; i--)
            {
                var layer = Layers[i];
                if (!includeTerminatingLayer && layer == terminatingLayer)
                {
                    break;
                }
                List<VegetationStudioInstance> vsData = layer.GetVegetationStudioData();
                if (vsData != null)
                {
                    for (int j = 0; j < vsData.Count; j++)
                    {
                        var vsdataInstance = vsData[j];
                        if (bounds.HasValue && !bounds.Value.Contains(Terrain.transform.position + vsdataInstance.Position))
                        {
                            continue;
                        }
                        result.Add(vsdataInstance.Guid, vsdataInstance);
                    }
                }
                List<string> removals = layer.GetVegetationStudioRemovals();
                if (removals != null)
                {
                    for (int j = 0; j < removals.Count; j++)
                    {
                        var treeRemoval = removals[j];
                        result.Remove(treeRemoval);
                    }
                }
                if (layer == terminatingLayer)
                {
                    break;
                }
            }
            return result.Values.ToList();
        }
    }
}

