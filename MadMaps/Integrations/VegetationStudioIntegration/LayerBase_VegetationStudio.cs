using System.Collections.Generic;
using MadMaps.Common.Collections;
using MadMaps.WorldStamp;
using UnityEngine;

namespace MadMaps.Terrains
{
    public partial class LayerBase
    {
        public virtual List<VegetationStudioInstance> GetVegetationStudioData()
        {
            return null;
        }

        public virtual List<string> GetVegetationStudioRemovals()
        {
            return null;
        }
    }
}