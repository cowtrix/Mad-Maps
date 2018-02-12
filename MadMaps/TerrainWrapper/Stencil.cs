using System;
using MadMaps.Common;
using MadMaps.Common.Collections;
using UnityEngine;

namespace MadMaps.Terrains
{
    [Serializable]
    public class Stencil : Serializable2DFloatArray 
    {
        public Stencil(int width, int data) : base(width, data)
        {
        }

        public Stencil(float[,] data) : base(data)
        {
        }

        protected override Color32 ToColor(float inValue)
        {
            int stencilKey;
            float value;
            MiscUtilities.DecompressStencil(inValue, out stencilKey, out value);
            return Color.Lerp(Color.black, ColorUtils.GetIndexColor(stencilKey), value);
        }
    }
}