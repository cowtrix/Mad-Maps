using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using sMap.Common.Collections;
using UnityEngine;
using sMap.WorldStamp;

namespace sMap.Terrains
{
    public abstract class LayerBase : ScriptableObject
    {
        [HideInInspector]
        public bool Enabled = true;
        [HideInInspector]
        public Serializable2DFloatArray Stencil;

        public virtual bool UserOwned
        {
            get { return true; }
        }

        public virtual void Dispose(TerrainWrapper wrapper, bool destroyObjects)
        {
        }

        public virtual void WriteToTerrain(TerrainWrapper wrapper)
        {
        }

        public virtual void Clear(TerrainWrapper wrapper)
        {
        }

        [CanBeNull]
        public virtual Serializable2DFloatArray GetHeights(int x, int z, int xSize, int zSize, int hRes)
        {
            return null;
        }

        [CanBeNull]
        public virtual Serializable2DByteArray GetSplatmap(SplatPrototypeWrapper prototype, int x, int z, int width,
            int height, int splatResolution)
        {
            return null;
        }

        [CanBeNull]
        public virtual Serializable2DByteArray GetDetailMap(DetailPrototypeWrapper detailWrapper, int x, int z,
            int width, int height, int detailResolution)
        {
            return null;
        }

        public virtual float SampleHeightNormalized(Vector2 normalizedPos)
        {
            return 0;
        }

        public virtual float SampleHeight(TerrainWrapper wrapper, Vector3 worldPos)
        {
            return 0;
        }

        [CanBeNull]
        public virtual List<string> GetTreeRemovals()
        {
            return null;
        }

        [CanBeNull]
        public virtual List<HurtTreeInstance> GetTrees()
        {
            return null;
        }

        [CanBeNull]
        public virtual List<string> GetObjectRemovals()
        {
            return null;
        }

        [CanBeNull]
        public virtual List<PrefabObjectData> GetObjects()
        {
            return null;
        }

        [CanBeNull]
        public virtual List<SplatPrototypeWrapper> GetSplatPrototypeWrappers()
        {
            return null;
        }

        [CanBeNull]
        public virtual List<DetailPrototypeWrapper> GetDetailPrototypeWrappers()
        {
            return null;
        }

        public virtual float BlendHeight(float sum, Vector3 worldPos, TerrainWrapper wrapper)
        {
            return sum;
        }

        public virtual Serializable2DFloatArray BlendHeights(int x, int z, int width, int height, int heightRes, Serializable2DFloatArray result)
        {
            return result;
        }
        
        public virtual float GetStencilStrength(Vector2 vector2, int stencilKey)
        {
            return 0;
        }

        public virtual float GetStencilStrength(Vector2 vector2, bool ignoreNegativeKeys = true)
        {
            return 0;
        }

        public virtual Serializable2DByteArray BlendSplats(SplatPrototypeWrapper splat,
            int x, int z, int width, int height, int aRes, Serializable2DByteArray result)
        {
            //throw new NotImplementedException();
            //Debug.LogErrorFormat("BlendSplats is not implemented for {0} ({1})", name, GetType());
            return result;
        }

        public virtual Serializable2DByteArray BlendDetails(DetailPrototypeWrapper detail,
            int x, int z, int width, int height, int dRes, Serializable2DByteArray result)
        {
           // throw new NotImplementedException();
            //Debug.LogErrorFormat("BlendDetails is not implemented for {0} ({1})", name, GetType());
            return result;
        }

        public virtual Dictionary<SplatPrototypeWrapper, Serializable2DByteArray> GetSplatMaps(int x, int z, int width,
            int height, int splatResolution)
        {
            var ret = new Dictionary<SplatPrototypeWrapper, Serializable2DByteArray>();
            var prototypeWrappers = GetSplatPrototypeWrappers();
            if (prototypeWrappers != null)
            {
                foreach (var splatPrototypeWrapper in prototypeWrappers)
                {
                    var result = GetSplatmap(splatPrototypeWrapper, x, z, width, height, splatResolution);
                    if (result != null)
                    {
                        ret.Add(splatPrototypeWrapper, result);
                    }
                }
            }
            return ret;
        }

        public virtual Dictionary<DetailPrototypeWrapper, Serializable2DByteArray> GetDetailMaps(int x, int z, int width,
            int height, int detailResolution)
        {
            var ret = new Dictionary<DetailPrototypeWrapper, Serializable2DByteArray>();
            var prototypeWrappers = GetDetailPrototypeWrappers();
            if (prototypeWrappers != null)
            {
                foreach (var detailPrototypeWrapper in prototypeWrappers)
                {
                    var result = GetDetailMap(detailPrototypeWrapper, x, z, width, height, detailResolution);
                    if (result != null)
                    {
                        ret.Add(detailPrototypeWrapper, result);
                    }
                }
            }
            return ret;
        }

        public virtual void PrepareApply(TerrainWrapper terrainWrapper)
        {
        }
    }
}