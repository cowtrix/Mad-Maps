using System;
using UnityEngine;
using Random = System.Random;

namespace Dingo.Common
{
    [Serializable]
    public struct FloatMinMax
    {
        public float Min, Max;

        public FloatMinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float GetRand(Random rand)
        {
            return Mathf.Lerp(Min, Max, (float)rand.NextDouble());
        }
    }
}