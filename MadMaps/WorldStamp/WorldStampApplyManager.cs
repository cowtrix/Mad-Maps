using System;
using System.Collections.Generic;
using System.Linq;
using MadMaps.Common;
using MadMaps.Terrains;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace MadMaps.WorldStamp
{
    /// <summary>
    /// This class sets up the order of a collection of stamps, and then executes them.
    /// </summary>
    public static class WorldStampApplyManager
    {
        public class LayerStampMapping
        {
            public string LayerName;
            public int LayerIndex = -1;
            public List<WorldStamp> Stamps = new List<WorldStamp>();
        }

        public static List<LayerStampMapping> SortStamps(TerrainWrapper wrapper, string layerFilter)
        {
            Profiler.BeginSample("CollectAndOrganise");

            // Collect all WorldStamps that lie within the bounds of the TerrainWrapper and satisfy the layerFilter (if one is specified)
            var allStamps = new List<WorldStamp>(Object.FindObjectsOfType<WorldStamp>());
            var tBounds = new Bounds(wrapper.Terrain.GetPosition() + wrapper.Terrain.terrainData.size / 2,
                wrapper.Terrain.terrainData.size);

            for (var i = allStamps.Count - 1; i >= 0; i--)
            {
                var worldStamp = allStamps[i];

                if (!String.IsNullOrEmpty(layerFilter) && worldStamp.LayerName != layerFilter)
                {
                    allStamps.RemoveAt(i);
                    continue;
                }

                var stampBounds = new ObjectBounds(worldStamp.transform.position, worldStamp.Size / 2,
                    worldStamp.transform.rotation);
                var axisStampBounds = stampBounds.ToAxisBounds();
                if (!tBounds.Intersects(axisStampBounds))
                {
                    allStamps.RemoveAt(i);
                }
            }
            
            // Sort the stamps into what layer they each point to
            List<LayerStampMapping> mappings = new List<LayerStampMapping>();
            for (int i = 0; i < allStamps.Count; i++)
            {
                var stamp = allStamps[i];
                LayerStampMapping mapping = null;
                foreach (var layerStampMapping in mappings)
                {
                    if (layerStampMapping.LayerName == stamp.LayerName)
                    {
                        mapping = layerStampMapping;
                        break;
                    }
                }
                if (mapping == null)
                {
                    mapping = new LayerStampMapping()
                    {
                        LayerIndex = wrapper.GetLayerIndex(stamp.LayerName),
                        LayerName = stamp.LayerName
                    };
                    mappings.Add(mapping);
                }
                mapping.Stamps.Add(stamp);
            }

            // Sort this mapping by layer index, then the contents of the mappings by priority
            mappings = mappings.OrderByDescending(mapping => mapping.LayerIndex).ToList();
            for (int i = 0; i < mappings.Count; i++)
            {
                mappings[i].Stamps = mappings[i].Stamps.OrderBy(stamp => stamp.Priority)
                    .ThenBy(stamp => stamp.transform.GetSiblingIndex())
                    .ToList();
            }

            // Go through and create new layers as needed
            for (int i = mappings.Count - 1; i >= 0; i--)
            {
                var layerStampMapping = mappings[i];
                var layer = wrapper.GetLayer<TerrainLayer>(layerStampMapping.LayerName, false, true);
                if (!layer.UserOwned)
                {
                    mappings.RemoveAt(i);
                    continue;
                }
                if (wrapper.GetLayerIndex(layer) == wrapper.Layers.Count - 1)
                {
                    layer.BlendMode = TerrainLayer.ETerrainLayerBlendMode.Set;
                }
                else
                {
                    layer.BlendMode = TerrainLayer.ETerrainLayerBlendMode.Stencil;
                }
                layer.Clear(wrapper);
            }
            Profiler.EndSample();
            return mappings;
        }

        /// <summary>
        /// TODO: This is a very complicated method. Is there a way to move towards componentisation?
        /// TODO: Layer filter method should be able to include/exclude arbitrary numbers of layers (currently it is all or one)
        /// Generally it doesn't make sense to apply only one stamp for a given layer. While partial recalculation
        /// might be possible, it doesn't currently exist. So when we recalcualte one stamp on a layer, we also
        /// recalculate all other stamps on that layer.
        /// </summary>
        /// <param name="wrapper">The wrapper to recalculate the stamps for</param>
        /// <param name="layerFilter">A name filter for the layers (for recalculating a single layer)</param>
        public static void ApplyAllStamps(TerrainWrapper wrapper, string layerFilter = null)
        {
            Profiler.BeginSample("ApplyAllStamps");
            
            var mappings = SortStamps(wrapper, layerFilter);

            // Okay, we've organised our stamps in such a way so we can iterate through and execute them.
            // Iterating the mappings here is equivalent to iterating through the TerrainWrapper layers and executing all the relevant stamps for each layer.
            for (int i = 0; i < mappings.Count; i++)
            {
                var layerStampMapping = mappings[i];
                var layer = wrapper.GetLayer<TerrainLayer>(layerStampMapping.LayerName, false, true);   // Is this redundant or just cautious? See line 100

                // Here, we copy the flattened information of all layers below this layer to the current layer, to easily blend new data with old data. 
                // A somewhat naive solution, but the best I have found after trying several out.
                wrapper.CopyCompoundToLayer(layer); 

                // Heights are a special type of layer, as WorldStamps have the ability to compete with each other and determine who gets control of a point
                // on the map through their height values. For this reason heights needed to be evaluated first, so winners can be determined.
                for (int j = 0; j < layerStampMapping.Stamps.Count; j++)
                {
                    layerStampMapping.Stamps[j].SnapStamp(true);

                    MiscUtilities.ProgressBar(String.Format("Applying Heights for Stamp {0} : Layer {1}", layerStampMapping.Stamps[j].name, layer.name), String.Format("{0}/{1}", j, layerStampMapping.Stamps.Count), j / (float)layerStampMapping.Stamps.Count);
                    layerStampMapping.Stamps[j].StampHeights(wrapper, layer);
                }
                // The Stencil is a critical component of stamping. Fundament It holds a dual purpose - blending 
                for (int j = 0; j < layerStampMapping.Stamps.Count; j++)
                {
                    MiscUtilities.ProgressBar(String.Format("Applying Stencil for Stamp {0} : Layer {1}", layerStampMapping.Stamps[j].name, layer.name), String.Format("{0}/{1}", j, layerStampMapping.Stamps.Count), j / (float)layerStampMapping.Stamps.Count);
                    layerStampMapping.Stamps[j].StampStencil(wrapper, layer, j + 1);
                }

                MiscUtilities.ClampStencil(layer.Stencil);

                for (int j = 0; j < layerStampMapping.Stamps.Count; j++)
                {
                    var worldStamp = layerStampMapping.Stamps[j];
                    var stencilKey = j + 1;

                    MiscUtilities.ProgressBar(String.Format("Applying Splats for Stamp {0} : Layer {1}", layerStampMapping.Stamps[j].name, layer.name), String.Format("{0}/{1}", j, layerStampMapping.Stamps.Count), j / (float)layerStampMapping.Stamps.Count);
                    worldStamp.StampSplats(wrapper, layer, stencilKey);

                    MiscUtilities.ProgressBar(String.Format("Applying Objects for Stamp {0} : Layer {1}", layerStampMapping.Stamps[j].name, layer.name), String.Format("{0}/{1}", j, layerStampMapping.Stamps.Count), j / (float)layerStampMapping.Stamps.Count);
                    worldStamp.StampObjects(wrapper, layer, stencilKey);

                    MiscUtilities.ProgressBar(String.Format("Applying Trees for Stamp {0} : Layer {1}", layerStampMapping.Stamps[j].name, layer.name), String.Format("{0}/{1}", j, layerStampMapping.Stamps.Count), j / (float)layerStampMapping.Stamps.Count);
                    worldStamp.StampTrees(wrapper, layer, stencilKey);

                    MiscUtilities.ProgressBar(String.Format("Applying Details for Stamp {0} : Layer {1}", layerStampMapping.Stamps[j].name, layer.name), String.Format("{0}/{1}", j, layerStampMapping.Stamps.Count), j / (float)layerStampMapping.Stamps.Count);
                    worldStamp.StampDetails(wrapper, layer, stencilKey);

                    #if VEGETATION_STUDIO
                    MiscUtilities.ProgressBar(String.Format("Applying Details for Stamp {0} : Layer {1}", layerStampMapping.Stamps[j].name, layer.name), String.Format("{0}/{1}", j, layerStampMapping.Stamps.Count), j / (float)layerStampMapping.Stamps.Count);
                    worldStamp.StampVegetationStudio(wrapper, layer, stencilKey);
                    #endif
                }

                MiscUtilities.ColoriseStencil(layer.Stencil);
                wrapper.ClearCompoundCache(layer);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(layer);
#endif
            }
            MiscUtilities.ClearProgressBar();
            Profiler.EndSample();
        }
    }
}