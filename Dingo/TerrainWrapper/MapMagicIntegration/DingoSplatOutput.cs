using System;
using System.Collections;
using System.Collections.Generic;
using MapMagic;
using UnityEngine;

namespace Dingo.Terrains.MapMagic
{
#if MAPMAGIC
    [System.Serializable]
    [GeneratorMenu(menu = "Dingo", name = "Dingo Textures", disengageable = true)]
    public class DingoSplatOutput : OutputGenerator, Layout.ILayered
    {
        public string LayerName = "MapMagic";

        //layer
        public class Layer : Layout.ILayer
        {
            public Input input = new Input(InoutType.Map);
            public Output output = new Output(InoutType.Map);
            public string name = "Layer";
            public float opacity = 1;
            public SplatPrototypeWrapper Wrapper;

            public bool pinned { get; set; }
            public int guiHeight { get; set; }

            public void OnCollapsedGUI(Layout layout)
            {
                layout.margin = 20; layout.rightMargin = 20; layout.fieldSize = 1f;
                layout.Par(20);
                if (!pinned) input.DrawIcon(layout);
                layout.Label(name, rect: layout.Inset());
                output.DrawIcon(layout);
            }

            public void OnExtendedGUI(Layout layout)
            {
                layout.margin = 20; layout.rightMargin = 20;
                layout.Par(20);

                if (!pinned) input.DrawIcon(layout);
                layout.Field(ref name, rect: layout.Inset());
                output.DrawIcon(layout);

                layout.Field(ref Wrapper);
                if (!Wrapper)
                {
                    return;
                }

                layout.Par(2);
                layout.Par(60); //not 65
                Wrapper.Texture = layout.Field(Wrapper.Texture, rect: layout.Inset(60));
                Wrapper.NormalMap = layout.Field(Wrapper.NormalMap, rect: layout.Inset(60));
                layout.Par(2);

                layout.margin = 5; layout.rightMargin = 5; layout.fieldSize = 0.6f;
                //layout.SmartField(ref downscale, "Downscale", min:1, max:8); downscale = Mathf.ClosestPowerOfTwo(downscale);
                opacity = layout.Field(opacity, "Opacity", min: 0);
                Wrapper.TileSize = layout.Field(Wrapper.TileSize, "Size");
                Wrapper.TileOffset = layout.Field(Wrapper.TileOffset, "Offset");
                Wrapper.SpecularColor = layout.Field(Wrapper.SpecularColor, "Specular");
                Wrapper.Smoothness = layout.Field(Wrapper.Smoothness, "Smooth", max: 1);
                Wrapper.Metallic = layout.Field(Wrapper.Metallic, "Metallic", max: 1);
            }

            public void OnAdd(int n) { }
            public void OnRemove(int n)
            {
                input.Link(null, null);
                Input connectedInput = output.GetConnectedInput(global::MapMagic.MapMagic.instance.gens.list);
                if (connectedInput != null) connectedInput.Link(null, null);
            }
            public void OnSwitch(int o, int n) { }
        }
        public Layer[] baseLayers = new Layer[] { new Layer() { pinned = true, name = "Background" } };
        public virtual Layout.ILayer[] layers
        {
            get { return baseLayers; }
            set { baseLayers = ArrayTools.Convert<Layer, Layout.ILayer>(value); }
        }

        public int selected { get; set; }
        public int collapsedHeight { get; set; }
        public int extendedHeight { get; set; }

        public Layout.ILayer def
        {
            get { return new Layer(); }
        }

        //generator
        public override IEnumerable<Input> Inputs()
        {
            if (baseLayers == null) baseLayers = new Layer[0];
            for (int i = 0; i < baseLayers.Length; i++)
                if (baseLayers[i].input != null)
                    yield return baseLayers[i].input;
        }
        public override IEnumerable<Output> Outputs()
        {
            if (baseLayers == null) baseLayers = new Layer[0];
            for (int i = 0; i < baseLayers.Length; i++)
                if (baseLayers[i].output != null)
                    yield return baseLayers[i].output;
        }

        //get static actions using instance
        public override Action<global::MapMagic.CoordRect, Chunk.Results, GeneratorsAsset, Chunk.Size, Func<float, bool>> GetProces() { return Process; }
        public override Func<global::MapMagic.CoordRect, Terrain, object, Func<float, bool>, IEnumerator> GetApply() { return Apply; }
        public override Action<global::MapMagic.CoordRect, Terrain> GetPurge() { return Purge; }


        public override void Generate(global::MapMagic.CoordRect rect, Chunk.Results results, Chunk.Size terrainSize, int seed, Func<float, bool> stop = null)
        {
            if ((stop != null && stop(0)) || !enabled) return;

            //loading inputs
            Matrix[] matrices = new Matrix[baseLayers.Length];
            for (int i = 0; i < baseLayers.Length; i++)
            {
                if (baseLayers[i].input != null)
                {
                    matrices[i] = (Matrix)baseLayers[i].input.GetObject(results);
                    if (matrices[i] != null) matrices[i] = matrices[i].Copy(null);
                }
                if (matrices[i] == null) matrices[i] = new Matrix(rect);
            }

            //background matrix
            //matrices[0] = terrain.defaultMatrix; //already created
            matrices[0].Fill(1);

            //populating opacity array
            float[] opacities = new float[matrices.Length];
            for (int i = 0; i < baseLayers.Length; i++)
                opacities[i] = baseLayers[i].opacity;
            opacities[0] = 1;

            //blending layers
            Matrix.BlendLayers(matrices, opacities);

            //saving changed matrix results
            for (int i = 0; i < baseLayers.Length; i++)
            {
                if (stop != null && stop(0)) return; //do not write object is generating is stopped
                baseLayers[i].output.SetObject(results, matrices[i]);
            }
        }

        public static void Process(global::MapMagic.CoordRect rect, Chunk.Results results, GeneratorsAsset gens, Chunk.Size terrainSize, Func<float, bool> stop = null)
        {
            if (stop != null && stop(0)) return;

            //gathering prototypes and matrices lists
            List<SplatPrototypeWrapper> prototypesList = new List<SplatPrototypeWrapper>();
            List<float> opacities = new List<float>();
            List<Matrix> matrices = new List<Matrix>();
            List<Matrix> biomeMasks = new List<Matrix>();

            foreach (DingoSplatOutput gen in gens.GeneratorsOfType<DingoSplatOutput>(onlyEnabled: true, checkBiomes: true))
            {
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

                for (int i = 0; i < gen.baseLayers.Length; i++)
                {
                    //reading output directly
                    Output output = gen.baseLayers[i].output;
                    if (stop != null && stop(0)) return; //checking stop before reading output
                    if (!results.results.ContainsKey(output)) continue;
                    Matrix matrix = (Matrix)results.results[output];
                    matrix.Clamp01();

                    //adding to lists
                    matrices.Add(matrix);
                    biomeMasks.Add(gen.biome == null ? null : biomeMask);
                    prototypesList.Add(gen.baseLayers[i].Wrapper);
                    opacities.Add(gen.baseLayers[i].opacity);
                }
            }

            //optimizing matrices list if they are not used
            for (int i = matrices.Count - 1; i >= 0; i--)
                if (opacities[i] < 0.001f || matrices[i].IsEmpty() || (biomeMasks[i] != null && biomeMasks[i].IsEmpty()))
                { prototypesList.RemoveAt(i); opacities.RemoveAt(i); matrices.RemoveAt(i); biomeMasks.RemoveAt(i); }

            //creating array
            float[, ,] splats3D = new float[terrainSize.resolution, terrainSize.resolution, prototypesList.Count];
            if (matrices.Count == 0) { results.apply.CheckAdd(typeof(DingoSplatOutput), new TupleSet<float[, ,], SplatPrototype[]>(splats3D, new SplatPrototype[0]), replace: true); return; }

            //filling array
            if (stop != null && stop(0)) return;

            int numLayers = matrices.Count;
            int maxX = splats3D.GetLength(0); int maxZ = splats3D.GetLength(1); //MapMagic.instance.resolution should not be used because of possible lods
            //global::MapMagic.CoordRect rect =  matrices[0].rect;

            float[] values = new float[numLayers]; //row, to avoid reading/writing 3d array (it is too slow)

            for (int x = 0; x < maxX; x++)
                for (int z = 0; z < maxZ; z++)
                {
                    int pos = rect.GetPos(x + rect.offset.x, z + rect.offset.z);
                    float sum = 0;

                    //getting values
                    for (int i = 0; i < numLayers; i++)
                    {
                        float val = matrices[i].array[pos];
                        if (biomeMasks[i] != null) val *= biomeMasks[i].array[pos]; //if mask is not assigned biome was ignored, so only main outs with mask==null left here
                        if (val < 0) val = 0; if (val > 1) val = 1;
                        sum += val; //normalizing: calculating sum
                        values[i] = val;
                    }

                    //setting color
                    for (int i = 0; i < numLayers; i++) splats3D[z, x, i] = values[i] / sum;
                }

            //pushing to apply
            if (stop != null && stop(0)) return;
            TupleSet<float[, ,], SplatPrototypeWrapper[]> splatsTuple = new TupleSet<float[, ,], SplatPrototypeWrapper[]>(splats3D, prototypesList.ToArray());
            results.apply.CheckAdd(typeof(DingoSplatOutput), splatsTuple, replace: true);
        }

        public IEnumerator Apply(global::MapMagic.CoordRect rect, Terrain terrain, object dataBox, Func<float, bool> stop = null)
        {
            TupleSet<float[, ,], SplatPrototypeWrapper[]> splatsTuple = (TupleSet<float[, ,], SplatPrototypeWrapper[]>)dataBox;
            float[, ,] splats3D = splatsTuple.item1;
            SplatPrototypeWrapper[] prototypes = splatsTuple.item2;

            terrain.terrainData.splatPrototypes = new[] {new SplatPrototype()};    // To stop MapMagic purging what we're doing here alter on...

            var wrapper = terrain.gameObject.GetOrAddComponent<TerrainWrapper>();
            var terrainLayer = wrapper.GetLayer<TerrainLayer>(LayerName, false, true);

            var splatWidth = splats3D.GetLength(0);
            var splatHeight = splats3D.GetLength(1);

            if (terrain.terrainData.alphamapResolution != splatWidth)
            {
                Debug.Log("Set alphamapResolution to " + splatWidth);
                terrain.terrainData.alphamapResolution = splatWidth;
            }

            var data = new float[splatWidth, splatHeight];
            for (int i = 0; i < prototypes.Length; i++)
            {
                var splatPrototypeWrapper = prototypes[i];
                for (var u = 0; u < splatWidth; ++u)
                {
                    for (var v = 0; v < splatHeight; ++v)
                    {
                        data[u, v] = splats3D[u, v, i];
                    }
                }
                terrainLayer.SetSplatmap(splatPrototypeWrapper, 0, 0, data, splatWidth);
            }

            global::MapMagic.MapMagic.OnApplyCompleted += MapMagicOnOnApplyCompleted;
            yield break;
        }

        private void MapMagicOnOnApplyCompleted(Terrain terrain)
        {
            global::MapMagic.MapMagic.OnApplyCompleted -= MapMagicOnOnApplyCompleted;
            var wrapper = terrain.gameObject.GetOrAddComponent<TerrainWrapper>();
            wrapper.Dirty = true;
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
            terrainLayer.SplatData.Clear();
            wrapper.Dirty = true;
        }

        public override void OnGUI(GeneratorsAsset gens)
        {
            layout.fieldSize = .6f;
            layout.Field(ref LayerName, "Layer");
            layout.fieldSize = .5f;

            layout.DrawLayered(this, "Layers:");
        }
    }
#endif
}