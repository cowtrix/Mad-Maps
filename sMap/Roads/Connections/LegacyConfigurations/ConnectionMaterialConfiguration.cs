using System;
using UnityEngine;

namespace sMap.Roads
{
    //[CreateAssetMenu(menuName = "sRoads/Connection Material Configuration")]
    public class ConnectionMaterialConfiguration : ConnectionComponentConfiguration
    {
        public EMaterialType Material;

        public override Type GetMonoType()
        {
            throw new NotImplementedException();
        }
    }
}