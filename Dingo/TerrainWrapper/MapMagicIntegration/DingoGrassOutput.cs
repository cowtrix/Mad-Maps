using System;
using System.Collections;
using System.Collections.Generic;
using MapMagic;
using UnityEngine;

namespace Dingo.Terrains.MapMagic
{
#if MAPMAGIC
    [System.Serializable]
    [GeneratorMenu(menu = "Dingo", name = "Dingo Grass", disengageable = true)]
    public class DingoGrassOutput : OutputGenerator, Layout.ILayered
    {
        public string LayerName = "MapMagic";

        //layer
        public class Layer : Layout.ILayer
        {
            public Input input = new Input(InoutType.Map);
            public Output output = new Output(InoutType.Map);

            public DetailPrototypeWrapper Wrapper;
            public string name;
            public float density = 0.5f;

            public bool pinned { get; set; }
            public int guiHeight { get; set; }

            public void OnCollapsedGUI(Layout layout)
            {
                layout.margin = 20; layout.rightMargin = 20; layout.fieldSize = 1f;
                layout.Par(20);
                input.DrawIcon(layout);
                layout.Label(name, rect: layout.Inset());
                if (output == null) output = new Output(InoutType.Map); //backwards compatibility
                output.DrawIcon(layout);
            }

            public void OnExtendedGUI(Layout layout)
            {
                layout.margin = 20; layout.rightMargin = 20;
                layout.Par(20);

                input.DrawIcon(layout);
                layout.Field(ref name, rect: layout.Inset());
                if (output == null) output = new Output(InoutType.Map); //backwards compatibility
                output.DrawIcon(layout);

                layout.margin = 5; layout.rightMargin = 10; layout.fieldSize = 0.6f;
                layout.fieldSize = 0.65f;

                layout.Field(ref Wrapper);
                if (Wrapper == null)
                {
                    return;
                }

                Wrapper.RenderMode = layout.Field(Wrapper.RenderMode, "Mode");

                if (Wrapper.RenderMode == DetailRenderMode.VertexLit)
                {
                    Wrapper.Prototype = layout.Field(Wrapper.Prototype, "Object");
                    Wrapper.PrototypeTexture = null; //otherwise this texture will be included to build even if not displayed
                }
                else
                {
                    layout.Par(60); //not 65
                    layout.Inset((layout.field.width - 60) / 2);
                    Wrapper.PrototypeTexture = layout.Field(Wrapper.PrototypeTexture, rect: layout.Inset(60));
                    Wrapper.Prototype = null; //otherwise this object will be included to build even if not displayed
                    layout.Par(2);
                }

                density = layout.Field(density, "Density", max: 50);
                //det.bendFactor = layout.Field(det.bendFactor, "Bend");
                Wrapper.DryColor = layout.Field(Wrapper.DryColor, "Dry");
                Wrapper.HealthyColor = layout.Field(Wrapper.HealthyColor, "Healthy");

                Vector2 temp = new Vector2(Wrapper.MinWidth, Wrapper.MaxWidth);
                layout.Field(ref temp, "Width", max: 10);
                Wrapper.MinWidth = temp.x;
                Wrapper.MaxWidth = temp.y;

                temp = new Vector2(Wrapper.MinHeight, Wrapper.MaxHeight);
                layout.Field(ref temp, "Height", max: 10);
                Wrapper.MinHeight = temp.x;
                Wrapper.MaxHeight = temp.y;

                Wrapper.NoiseSpread = layout.Field(Wrapper.NoiseSpread, "Noise", max: 1);
            }

            public void OnAdd(int n) { name = "Grass"; }
            public void OnRemove(int n) { input.Link(null, null); }
            public void OnSwitch(int o, int n) { }
        }
        public Layer[] baseLayers = new Layer[0];
        public Layout.ILayer[] layers
        {
            get { return baseLayers; }
            set { baseLayers = ArrayTools.Convert<Layer, Layout.ILayer>(value); }
        }

        public int selected { get; set; }
        public int collapsedHeight { get; set; }
        public int extendedHeight { get; set; }
        public Layout.ILayer def { get { return new Layer() { name = "Grass" }; } }

        //params
        public Input maskIn = new Input(Generator.InoutType.Map);
        public static int patchResolution = 16;
        public bool obscureLayers = false;

        //public class GrassTuple { public int[][,] details; public DetailPrototype[] prototypes; }

        //generator
        public override IEnumerable<Input> Inputs()
        {
            if (maskIn == null) maskIn = new Input(InoutType.Map); //for backwards compatibility, input should not be null
            yield return maskIn;

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

            //blending layers
            if (obscureLayers) Matrix.BlendLayers(matrices);

            //masking layers
            Matrix mask = (Matrix)maskIn.GetObject(results);
            if (mask != null)
                for (int i = 0; i < matrices.Length; i++) matrices[i].Multiply(mask);

            //saving outputs
            for (int i = 0; i < baseLayers.Length; i++)
            {
                if (stop != null && stop(0)) return; //do not write object is generating is stopped
                if (baseLayers[i].output == null) baseLayers[i].output = new Output(InoutType.Map); //back compatibility
                baseLayers[i].output.SetObject(results, matrices[i]);
            }
        }

        public static void Process(global::MapMagic.CoordRect rect, Chunk.Results results, GeneratorsAsset gens, Chunk.Size terrainSize, Func<float, bool> stop = null)
        {
            if (stop != null && stop(0)) return;

            //debug timer
            /*			if (MapMagic.instance!=null && MapMagic.instance.guiDebug && worker!=null)
                        {
                            if (worker.timer==null) worker.timer = new System.Diagnostics.Stopwatch(); 
                            else worker.timer.Reset();
                            worker.timer.Start();
                        }*/

            //values to calculate density
            float pixelSize = terrainSize.pixelSize;
            float pixelSquare = pixelSize * pixelSize;

            //a random needed to convert float to int
            InstanceRandom rnd = new InstanceRandom(terrainSize.Seed(rect));

            //calculating the totoal number of prototypes
            int prototypesNum = 0;
            foreach (DingoGrassOutput grassOut in gens.GeneratorsOfType<DingoGrassOutput>())
                prototypesNum += grassOut.baseLayers.Length;

            //preparing results
            List<int[,]> detailsList = new List<int[,]>();
            List<DetailPrototypeWrapper> prototypesList = new List<DetailPrototypeWrapper>();

            //filling result
            foreach (DingoGrassOutput gen in gens.GeneratorsOfType<DingoGrassOutput>(onlyEnabled: true, checkBiomes: true))
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
                    if (stop != null && stop(0)) return;

                    //loading objects from input
                    //Matrix matrix = (Matrix)gen.baseLayers[i].input.GetObject(chunk);
                    //if (matrix == null) continue;

                    //reading output directly
                    Output output = gen.baseLayers[i].output;
                    if (stop != null && stop(0)) return; //checking stop before reading output
                    if (!results.results.ContainsKey(output)) continue;
                    Matrix matrix = (Matrix)results.results[output];

                    //filling array
                    int[,] detail = new int[matrix.rect.size.x, matrix.rect.size.z];
                    for (int x = 0; x < matrix.rect.size.x; x++)
                        for (int z = 0; z < matrix.rect.size.z; z++)
                        {
                            float val = matrix[x + matrix.rect.offset.x, z + matrix.rect.offset.z];
                            float biomeVal = 1;
                            if (gen.biome != null)
                            {
                                if (biomeMask == null) biomeVal = 0;
                                else biomeVal = biomeMask[x + matrix.rect.offset.x, z + matrix.rect.offset.z];
                            }
                            detail[z, x] = rnd.RandomToInt(val * gen.baseLayers[i].density * pixelSquare * biomeVal);
                        }

                    //adding to arrays
                    detailsList.Add(detail);
                    prototypesList.Add(gen.baseLayers[i].Wrapper);
                }
            }

            //pushing to apply
            if (stop != null && stop(0)) return;

            TupleSet<int[][,], DetailPrototypeWrapper[]> grassTuple = new TupleSet<int[][,], DetailPrototypeWrapper[]>(detailsList.ToArray(), prototypesList.ToArray());

#if UN_MapMagic
            if (FoliageCore_MainManager.instance != null)
            {
                float resolutionDifferences = (float)MapMagic.instance.terrainSize / terrainSize.resolution;

                var uNatureTuple = new uNatureGrassTuple(grassTuple, new Vector3(rect.Min.x * resolutionDifferences, 0, rect.Min.z * resolutionDifferences)); // transform coords
                results.apply.CheckAdd(typeof(GrassOutput), uNatureTuple, replace: true);
            }
            else
            {
                //Debug.LogError("uNature_MapMagic extension is enabled but no foliage manager exists on the scene.");
                //return;
				results.apply.CheckAdd(typeof(GrassOutput), grassTuple, replace: true);
            }
#else
            results.apply.CheckAdd(typeof(DingoGrassOutput), grassTuple, replace: true);
#endif
        }


        public IEnumerator Apply(global::MapMagic.CoordRect rect, Terrain terrain, object dataBox, Func<float, bool> stop = null)
        {
            var wrapper = terrain.gameObject.GetOrAddComponent<TerrainWrapper>();
            var terrainLayer = wrapper.GetLayer<TerrainLayer>(LayerName, false, true);
            terrainLayer.DetailData.Clear();

            var grassTuple = (TupleSet<int[][,], DetailPrototypeWrapper[]>)dataBox;
            int[][,] details = grassTuple.item1;
            DetailPrototypeWrapper[] prototypes = grassTuple.item2;

            //resolution
            if (details.Length != 0)
            {
                int resolution = details[0].GetLength(1);
                terrain.terrainData.SetDetailResolution(resolution, patchResolution);
            }

            //prototypes
            //terrain.terrainData.detailPrototypes = prototypes;
            for (int i = 0; i < details.Length; i++)
            {
                //terrain.terrainData.SetDetailLayer(0, 0, i, details[i]);
                terrainLayer.SetDetailMap(prototypes[i], 0, 0, details[i], global::MapMagic.MapMagic.instance.resolution);
            }
            
            yield return null;
            global::MapMagic.MapMagic.OnApplyCompleted += MapMagicOnOnApplyCompleted;
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
            terrainLayer.DetailData.Clear();
            wrapper.Dirty = true;
        }

        public override void OnGUI(GeneratorsAsset gens)
        {
            layout.fieldSize = .6f;
            layout.Field(ref LayerName, "Layer");
            layout.fieldSize = .5f;

            layout.Par(20); maskIn.DrawIcon(layout, "Mask");

            layout.Field(ref patchResolution, "Patch Res", min: 4, max: 64, fieldSize: 0.35f);
            patchResolution = Mathf.ClosestPowerOfTwo(patchResolution);
            layout.Field(ref obscureLayers, "Obscure Layers", fieldSize: 0.35f);
            layout.Par(3);
            layout.DrawLayered(this, "Layers:");

            layout.fieldSize = 0.4f; layout.margin = 10; layout.rightMargin = 10;
            layout.Par(5);
        }

    }
#endif
}