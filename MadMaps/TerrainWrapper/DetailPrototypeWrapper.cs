using System.Collections.Generic;
using UnityEngine;

namespace MadMaps.Terrains
{
    [CreateAssetMenu(menuName = "Dingo/Terrain/Detail Prototype")]
    public class DetailPrototypeWrapper : ScriptableObject
    {
        public float BendFactor;
        public Color DryColor;
        public Color HealthyColor;
        public float MaxHeight;
        public float MaxWidth;
        public float MinHeight;
        public float MinWidth;
        public float NoiseSpread;
        public GameObject Prototype;
        public Texture2D PrototypeTexture;
        public DetailRenderMode RenderMode;

        public DetailPrototype GetPrototype()
        {
            return new DetailPrototype()
            {
                bendFactor = BendFactor,
                dryColor = DryColor,
                healthyColor = HealthyColor,
                maxHeight = MaxHeight,
                maxWidth = MaxWidth,
                minHeight = MinHeight,
                minWidth = MinWidth,
                noiseSpread = NoiseSpread,
                prototype = Prototype,
                prototypeTexture = PrototypeTexture,
                renderMode = RenderMode,
                usePrototypeMesh = (RenderMode == DetailRenderMode.VertexLit)
            };
        }

        public class DetailPrototypeComparer : IEqualityComparer<DetailPrototype>
        {
            public bool Equals(DetailPrototype x, DetailPrototype y)
            {
                return x.prototypeTexture == y.prototypeTexture && x.prototype == y.prototype && x.renderMode == y.renderMode && x.healthyColor == y.healthyColor && x.dryColor == y.dryColor;
            }

            public int GetHashCode(DetailPrototype obj)
            {
                int hashCode = 0;
                hashCode = (hashCode * 397) ^ (obj.prototypeTexture != null ? obj.prototypeTexture.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.prototype != null ? obj.prototype.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.bendFactor.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.dryColor.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.healthyColor.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.maxHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.minHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.maxWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.minWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.noiseSpread.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.renderMode.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.usePrototypeMesh.GetHashCode();
                return hashCode;
            }
        }

        public void SetFromPrototype(DetailPrototype prototype)
        {
            BendFactor = prototype.bendFactor;
            DryColor = prototype.dryColor;
            HealthyColor = prototype.healthyColor;
            MaxHeight = prototype.maxHeight;
            MinHeight = prototype.minHeight;
            MaxWidth = prototype.maxWidth;
            MinWidth = prototype.minWidth;
            NoiseSpread = prototype.noiseSpread;
            Prototype = prototype.prototype;
            PrototypeTexture = prototype.prototypeTexture;
            RenderMode = prototype.renderMode;
        }
    }
}