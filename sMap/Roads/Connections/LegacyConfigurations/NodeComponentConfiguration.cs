using System;
using UnityEngine;

namespace sMap.Roads
{
    public abstract class NodeComponentConfiguration : ScriptableObject
    {
        public abstract Type GetMonoType();
    }

    
}