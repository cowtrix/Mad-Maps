using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using sMap.Common;
using sMap.Roads;
using sMap.Terrains.Lookups;
using sMap.WorldStamp;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace sMap.Terrains
{
    public abstract class ProceduralLayerComponent
    {
        public enum ApplyTiming
        {
            Instant,
            OnPreFinalise,
            OnPreRecalculate,
            OnPostFinalise,
            OnFrameAfterPostFinalise,
        }

        public abstract ApplyTiming Timing { get; }
        public bool Enabled = true;
        public abstract void Apply(ProceduralLayer layer, TerrainWrapper wrapper);
    }

    public class ProceduralLayer : LayerBase, ISerializationCallbackReceiver
    {
        [AllowDerived]
        public List<ProceduralLayerComponent> Components = new List<ProceduralLayerComponent>();

        [HideInInspector]
        public List<DerivedComponentJsonDataRow> ComponentsJSON = new List<DerivedComponentJsonDataRow>();

        [HideInInspector]
        public List<string> TreeRemovals = new List<string>();
        public List<PrefabObjectData> Objects = new List<PrefabObjectData>();

        [Seed]
        public int Seed;

        public override void PrepareApply(TerrainWrapper terrainWrapper)
        {
            if (Seed == 0)
            {
                Seed = Random.Range(int.MinValue, int.MaxValue);
            }

            terrainWrapper.OnPreRecalculate -= OnPreRecalculate;
            terrainWrapper.OnPreFinalise -= OnPreFinalise;
            terrainWrapper.OnPostFinalise -= OnPostFinalise;
            terrainWrapper.OnFrameAfterPostFinalise -= OnFrameAfterPostFinalise;

            if (Enabled)
            {
                terrainWrapper.OnPreRecalculate += OnPreRecalculate;
                terrainWrapper.OnPreFinalise += OnPreFinalise;
                terrainWrapper.OnPostFinalise += OnPostFinalise;
                terrainWrapper.OnFrameAfterPostFinalise += OnFrameAfterPostFinalise;
            }
        }

        private void OnFrameAfterPostFinalise(TerrainWrapper wrapper)
        {
            if (!Enabled)
            {
                return;
            }
            for (int i = 0; i < Components.Count; i++)
            {
                var proceduralLayerComponent = Components[i];
                if (!proceduralLayerComponent.Enabled)
                {
                    continue;
                }
                if (proceduralLayerComponent.Timing == ProceduralLayerComponent.ApplyTiming.OnFrameAfterPostFinalise)
                {
                    MiscUtilities.ProgressBar(
                        string.Format("Procedural Layer {0}: Processing {1}", name, proceduralLayerComponent.GetType()),
                        string.Format("{0}/{1}", i + 1, Components.Count), i / (float)Components.Count);
                    proceduralLayerComponent.Apply(this, wrapper);
                }
            }
            MiscUtilities.ClearProgressBar();
        }

        private void OnPostFinalise(TerrainWrapper wrapper)
        {
            if (!Enabled)
            {
                return;
            }
            for (int i = 0; i < Components.Count; i++)
            {
                var proceduralLayerComponent = Components[i];
                if (!proceduralLayerComponent.Enabled)
                {
                    continue;
                }
                if (proceduralLayerComponent.Timing == ProceduralLayerComponent.ApplyTiming.OnPostFinalise)
                {
                    MiscUtilities.ProgressBar(
                        string.Format("Procedural Layer {0}: Processing {1}", name, proceduralLayerComponent.GetType()),
                        string.Format("{0}/{1}", i + 1, Components.Count), i/(float) Components.Count);
                    proceduralLayerComponent.Apply(this, wrapper);
                }
            }
        }

        private void OnPreFinalise(TerrainWrapper wrapper)
        {
            if (!Enabled)
            {
                return;
            }
            for (int i = 0; i < Components.Count; i++)
            {
                var proceduralLayerComponent = Components[i];
                if (!proceduralLayerComponent.Enabled)
                {
                    continue;
                }
                if (proceduralLayerComponent.Timing == ProceduralLayerComponent.ApplyTiming.OnPreFinalise)
                {
                    proceduralLayerComponent.Apply(this, wrapper);
                }
            }
        }

        private void OnPreRecalculate(TerrainWrapper wrapper)
        {
            if (!Enabled)
            {
                return;
            }
            Objects.Clear();
            TreeRemovals.Clear();
            for (int i = 0; i < Components.Count; i++)
            {
                var proceduralLayerComponent = Components[i];
                if (!proceduralLayerComponent.Enabled)
                {
                    continue;
                }
                if (proceduralLayerComponent.Timing == ProceduralLayerComponent.ApplyTiming.OnPreRecalculate)
                {
                    MiscUtilities.ProgressBar(
                        string.Format("Procedural Layer {0}: Processing {1}", name, proceduralLayerComponent.GetType()),
                        string.Format("{0}/{1}", i + 1, Components.Count), i / (float)Components.Count);
                    proceduralLayerComponent.Apply(this, wrapper);
                }
            }
        }

        public override void WriteToTerrain(TerrainWrapper wrapper)
        {
            if (!Enabled)
            {
                return;
            }
            for (int i = 0; i < Components.Count; i++)
            {
                var proceduralLayerComponent = Components[i];
                if (!proceduralLayerComponent.Enabled)
                {
                    continue;
                }
                if (proceduralLayerComponent.Timing == ProceduralLayerComponent.ApplyTiming.Instant)
                {
                    MiscUtilities.ProgressBar(
                        string.Format("Procedural Layer {0}: Processing {1}", name, proceduralLayerComponent.GetType()),
                        string.Format("{0}/{1}", i + 1, Components.Count), i / (float)Components.Count);
                    proceduralLayerComponent.Apply(this, wrapper);
                }
            }

            foreach (var prefabObjectData in Objects)
            {
                wrapper.CompoundTerrainData.Objects.Add(prefabObjectData.Guid,
                    new InstantiatedObjectData(prefabObjectData, this, null));
            }
            
        }

        public override List<string> GetTreeRemovals()
        {
            return TreeRemovals;
        }

        public void OnBeforeSerialize()
        {
            ComponentsJSON.Clear();
            foreach (var proceduralLayerComponent in Components)
            {
                var type = proceduralLayerComponent.GetType();
                var jsonRow = new DerivedComponentJsonDataRow()
                {
                    AssemblyQualifiedName = type.AssemblyQualifiedName,
                    SerializedObjects = new List<Object>(),
                };
                jsonRow.JsonText = JSONSerializer.Serialize(type, proceduralLayerComponent, false,
                    jsonRow.SerializedObjects);
                ComponentsJSON.Add(jsonRow);
            }
        }

        public void OnAfterDeserialize()
        {
            Components.Clear();
            foreach (var component in ComponentsJSON)
            {
                Components.Add(
                (ProceduralLayerComponent)
                    JSONSerializer.Deserialize(Type.GetType(component.AssemblyQualifiedName), component.JsonText,
                        component.SerializedObjects));
            }
        }

        public override float BlendHeight(float sum, Vector3 worldPos, TerrainWrapper wrapper)
        {
            return sum;
        }

        public override List<PrefabObjectData> GetObjects()
        {
            return Objects;
        }
    }
}