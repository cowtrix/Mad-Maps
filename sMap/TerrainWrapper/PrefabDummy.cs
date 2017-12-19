using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace sMap.Terrains
{
    [StripComponentOnBuild]
    public class PrefabDummy : MonoBehaviour, ILevelPreBuildStepCallback
    {
        [Serializable]
        public struct BiomePair
        {
            public HurtBiome Biome;
            public GameObject Prefab;
        }

        public List<BiomePair> BiomePairs = new List<BiomePair>();

        [FormerlySerializedAs("Prefab")] public GameObject DefaultPrefab;

        [ContextMenu("Test Prebuild Step")]
        public void OnLevelPreBuildStep()
        {
            var prefab = DefaultPrefab;
            byte bestWeight = 0;
            if (!BiomePairs.IsNullOrEmpty() && BiomeManager.LevelInstance != null)
            {
                var bm = BiomeManager.LevelInstance;
                if (bm.Data != null)
                {
                    if (bm.GridManager == null)
                    {
                        bm.GridManager = new GridManagerInt(bm.Data.GridSize);
                    }

                    byte weightSum = 0;
                    var cell = bm.GridManager.GetCell(transform.position);
                    foreach (var keyValuePair in bm.Data.BiomeGridDictionary)
                    {
                        var biome = keyValuePair.Key;
                        var biomeData = keyValuePair.Value;

                        /*if (biomeData.BlendMode == HurtBiomeGridMapping.EBiomeBlendMode.Overlay)
                        {
                            continue;
                        }*/

                        byte weight;
                        biomeData.TryGetValue(cell, out weight);
                        weightSum += weight;

                        if (weight == 0)
                        {
                            continue;
                        }

                        foreach (var biomePair in BiomePairs)
                        {
                            if (biomePair.Biome == biome && weight > bestWeight)
                            {
                                bestWeight = weight;
                                prefab = biomePair.Prefab;
                            }
                        }
                    }
                    if (byte.MaxValue - weightSum > weightSum)
                    {
                        foreach (var biomePair in BiomePairs)
                        {
                            if (biomePair.Biome == bm.Data.BaseHurtBiome)
                            {
                                prefab = biomePair.Prefab;
                            }
                        }
                    }
                }
            }

            if (prefab == null)
            {
                Debug.LogError(string.Format("Prefab Dummy {0} had a null reference!", name), this);
            }

            var go = Instantiate(prefab, transform.position, transform.rotation);
            go.transform.localScale = transform.localScale;
            go.transform.SetParent(transform.parent);

            var pbsc = go.GetComponentsByInterface<ILevelPreBuildStepCallback>();
            if (!pbsc.IsNullOrEmpty())
            {
                foreach (var levelPreBuildStepCallback in pbsc)
                {
                    levelPreBuildStepCallback.OnLevelPreBuildStep();
                }
            }

            DestroyImmediate(gameObject);
        }
    }
}