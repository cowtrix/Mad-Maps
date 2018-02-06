#if MAPMAGIC
using System;
using System.Collections;
using System.Collections.Generic;
using MadMaps.Common;
using MapMagic;
using UnityEngine;

namespace MadMaps.Terrains.MapMagic
{
    [System.Serializable]
    [GeneratorMenu(menu = "MadMaps", name = "MadMaps Height", disengageable = true)]
    public class MadMapsHeightOutput : OutputGenerator
    {
        public string LayerName = "MapMagic";

        public Input input = new Input(InoutType.Map);
        public Output output = new Output(InoutType.Map);
        public override IEnumerable<Input> Inputs() { yield return input; }
        public override IEnumerable<Output> Outputs() { if (output == null) output = new Output(InoutType.Map); yield return output; }

        public float layer { get; set; }

        //get static actions using instance
        public override Action<global::MapMagic.CoordRect, Chunk.Results, GeneratorsAsset, Chunk.Size, Func<float, bool>> GetProces() { return Process; }
        public override Func<global::MapMagic.CoordRect, Terrain, object, Func<float, bool>, IEnumerator> GetApply() { return Apply; }
        public override Action<global::MapMagic.CoordRect, Terrain> GetPurge() { return Purge; }

        public static int scale = 1;

        public void Process(global::MapMagic.CoordRect rect, Chunk.Results results, GeneratorsAsset gens, Chunk.Size terrainSize, Func<float, bool> stop = null)
        {
            if (stop != null && stop(0)) return;

            //reading height outputs
            if (results.heights == null || results.heights.rect.size.x != rect.size.x) results.heights = new Matrix(rect);
            results.heights.rect.offset = rect.offset;
            results.heights.Clear();

            //processing main height
            foreach (MadMapsHeightOutput gen in gens.GeneratorsOfType<MadMapsHeightOutput>(onlyEnabled: true, checkBiomes: true))
            {
                //if (stop!=null && stop(0)) return; //do not break while results.heights is empty!

                //loading inputs
                Matrix heights = (Matrix)gen.input.GetObject(results);
                if (heights == null) continue;

                //loading biome matrix
                Matrix biomeMask = null;
                if (gen.biome != null)
                {
                    object biomeMaskObj = gen.biome.mask.GetObject(results);
                    if (biomeMaskObj == null) continue; //adding nothing if biome has no mask
                    biomeMask = (Matrix)biomeMaskObj;
                    if (biomeMask == null) continue;
                    if (biomeMask.IsEmpty()) continue; //optimizing empty biomes
                }

                //adding to final result
                if (gen.biome == null) results.heights.Add(heights);
                else if (biomeMask != null) results.heights.Add(heights, biomeMask);
            }

            //creating 2d array
            if (stop != null && stop(0)) return;
            int heightSize = terrainSize.resolution * scale + 1;
            float[,] heights2D = new float[heightSize, heightSize];
            for (int x = 0; x < heightSize - 1; x++)
                for (int z = 0; z < heightSize - 1; z++)
                {
                    if (scale == 1) heights2D[z, x] += results.heights[x + results.heights.rect.offset.x, z + results.heights.rect.offset.z];
                    else
                    {
                        float fx = 1f * x / scale; float fz = 1f * z / scale;
                        heights2D[z, x] = results.heights.GetInterpolated(fx + results.heights.rect.offset.x, fz + results.heights.rect.offset.z);
                    }
                }

            //blur only original base verts
            if (scale == 2)
            {
                float blurVal = 0.2f;

                for (int z = 0; z < heightSize - 1; z += 2)
                    for (int x = 2; x < heightSize - 1; x += 2)
                        heights2D[x, z] = (heights2D[x - 1, z] + heights2D[x + 1, z]) / 2 * blurVal + heights2D[x, z] * (1 - blurVal);

                for (int x = 0; x < heightSize - 1; x += 2)
                    for (int z = 2; z < heightSize - 1; z += 2)
                        heights2D[x, z] = (heights2D[x, z - 1] + heights2D[x, z + 1]) / 2 * blurVal + heights2D[x, z] * (1 - blurVal);
            }

            //blur high scale values
            if (scale == 4)
            {
                int blurIterations = 2;

                for (int i = 0; i < blurIterations; i++)
                {
                    float prev = 0;
                    float curr = 0;
                    float next = 0;

                    for (int x = 0; x < heightSize; x++)
                    {
                        prev = heights2D[x, 0]; curr = prev;
                        for (int z = 1; z < heightSize - 2; z++)
                        {
                            next = heights2D[x, z + 1];
                            curr = (next + prev) / 2;// * blurVal + curr*(1-blurVal);

                            heights2D[x, z] = curr;
                            prev = curr;
                            curr = next;
                        }

                    }

                    for (int z = 0; z < heightSize; z++)
                    {
                        prev = heights2D[0, z]; curr = prev;
                        for (int x = 1; x < heightSize - 2; x++)
                        {
                            next = heights2D[x + 1, z];
                            curr = (next + prev) / 2;// * blurVal + curr*(1-blurVal);

                            heights2D[x, z] = curr;
                            prev = curr;
                            curr = next;
                        }
                    }
                }
            }

            //processing sides
            for (int x = 0; x < heightSize; x++)
            {
                float prevVal = heights2D[heightSize - 3, x]; //size-2
                float currVal = heights2D[heightSize - 2, x]; //size-1, point on border
                float nextVal = currVal - (prevVal - currVal);
                heights2D[heightSize - 1, x] = nextVal;
            }
            for (int z = 0; z < heightSize; z++)
            {
                float prevVal = heights2D[z, heightSize - 3]; //size-2
                float currVal = heights2D[z, heightSize - 2]; //size-1, point on border
                float nextVal = currVal - (prevVal - currVal);
                heights2D[z, heightSize - 1] = nextVal;
            }
            heights2D[heightSize - 1, heightSize - 1] = heights2D[heightSize - 1, heightSize - 2];


            //pushing to apply
            if (stop != null && stop(0)) return;

#if UN_MapMagic
            if (FoliageCore_MainManager.instance != null)
            {
                float resolutionDifferences = (float)MapMagic.instance.terrainSize / terrainSize.resolution;

                uNatureHeightTuple heightTuple = new uNatureHeightTuple(heights2D, new Vector3(rect.Min.x * resolutionDifferences, 0, rect.Min.z * resolutionDifferences)); // transform coords
                results.apply.CheckAdd(typeof(HeightOutput), heightTuple, replace: true);
            }
            else
            {
                //Debug.LogError("uNature_MapMagic extension is enabled but no foliage manager exists on the scene.");
                //return;
				results.apply.CheckAdd(typeof(HeightOutput), heights2D, replace: true);
            }

#else
            results.apply.CheckAdd(typeof(MadMapsHeightOutput), heights2D, replace: true);
#endif
        }

        public void Purge(global::MapMagic.CoordRect rect, Terrain terrain)
        {
            var wrapper = terrain.GetComponent<TerrainWrapper>();
            if (wrapper == null)
            {
                return;
            }
            var terrainLayer = wrapper.GetLayer<TerrainLayer>(LayerName);
            if (terrainLayer == null || terrainLayer.Heights == null)
            {
                return;
            }
            terrainLayer.Heights.Clear();
            wrapper.Dirty = true;
        }

        public IEnumerator Apply(global::MapMagic.CoordRect rect, Terrain terrain, object dataBox, Func<float, bool> stop = null)
        {
            if (terrain == null || terrain.terrainData == null) yield break; //chunk removed during apply

            float[,] heights2D = (float[,])dataBox;
            heights2D = heights2D.Flip();
            var data = terrain.terrainData;
            Vector3 terrainSize = terrain.terrainData.size; //new Vector3(MapMagic.instance.terrainSize, MapMagic.instance.terrainHeight, MapMagic.instance.terrainSize);
            int terrainResolution = heights2D.GetLength(0); //heights2D[0].GetLength(0);
            if ((data.size - terrainSize).sqrMagnitude > 0.01f || data.heightmapResolution != terrainResolution)
            {
                if (terrainResolution <= 64) //brute force
                {
                    data.heightmapResolution = terrainResolution;
                    data.size = new Vector3(terrainSize.x, terrainSize.y, terrainSize.z);
                }

                else //setting res 64, re-scaling to 1/64, and then changing res
                {
                    data.heightmapResolution = 65;
                    terrain.Flush(); //otherwise unity crushes without an error
                    int resFactor = (terrainResolution - 1) / 64;
                    data.size = new Vector3(terrainSize.x / resFactor, terrainSize.y, terrainSize.z / resFactor);
                    data.heightmapResolution = terrainResolution;
                }
            }
            yield return null;

            var wrapper = terrain.gameObject.GetOrAddComponent<TerrainWrapper>();
            var terrainLayer = wrapper.GetLayer<TerrainLayer>(LayerName, false, true);
            
            terrainLayer.SetHeights(0, 0, heights2D, global::MapMagic.MapMagic.instance.resolution+1);
            yield return null;

            global::MapMagic.MapMagic.OnApplyCompleted += MapMagicOnOnApplyCompleted;
        }

        private void MapMagicOnOnApplyCompleted(Terrain terrain)
        {
            global::MapMagic.MapMagic.OnApplyCompleted -= MapMagicOnOnApplyCompleted;
            var wrapper = terrain.gameObject.GetOrAddComponent<TerrainWrapper>();
            wrapper.Dirty = true;
        }

        public override void OnGUI(GeneratorsAsset gens)
        {
            layout.fieldSize = .6f;
            layout.Field(ref LayerName, "Layer");
            layout.fieldSize = .5f;

            layout.Par(20); input.DrawIcon(layout, "Height");
            layout.Par(5);

            if (output == null) output = new Output(InoutType.Map);

            layout.Field(ref scale, "Scale", min: 1, max: 4f);
            scale = Mathf.NextPowerOfTwo(scale);
        }
    }
}
#endif