using System;
using Dingo.Terrains;

namespace Dingo.Common.Serialization
{
    public static class LegacyManager
    {
        public static Type GetTypeFromLegacy(string name)
        {
            if (name.Contains("HurtTreeInstance"))
            {
                return typeof(DingoTreeInstance);
            }
            var t = Type.GetType(name.Replace("sMap", "Dingo"));
            return t;
        }
    }
}