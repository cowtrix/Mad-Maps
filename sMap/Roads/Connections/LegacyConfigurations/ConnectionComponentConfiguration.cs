using System;
using UnityEngine;

namespace sMap.Roads
{
    [Obsolete]
    public abstract class ConnectionComponentConfiguration : ScriptableObject
    {
        public int Priority;
        public abstract Type GetMonoType();
    }
}