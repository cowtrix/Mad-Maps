using System;
using System.Collections.Generic;
using UnityEngine;

namespace sMap.Roads
{
    //[CreateAssetMenu(menuName = "sRoads/Component Collection Configuration")]
    public class ConnectionComponentCollectionConfiguration : ConnectionComponentConfiguration
    {
        public List<ConnectionComponentConfiguration> Components = new List<ConnectionComponentConfiguration>();
        public override Type GetMonoType()
        {
            throw new NotImplementedException();
            //return typeof(ConnectionComponentCollection);
        }
    }
}