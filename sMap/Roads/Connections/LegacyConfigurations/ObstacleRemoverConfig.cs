using System;
using UnityEngine;

namespace sMap.Roads
{
    public class ObstacleRemoverConfig : ConnectionComponentConfiguration
    {
        [Header("Trees")]
        public float TreeRemoveDistance = 1;

        [Header("Objects")]
        public LayerMask ObjectRemovalMask = 1 << 21;
        public float ObjectRemoveDistance = 1;
        public string RegexMatch;

        public override Type GetMonoType()
        {
            return typeof (RemoveTerrainObjects);
        }
    }
}