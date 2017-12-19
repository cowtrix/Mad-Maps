using System;
using System.Collections.Generic;
using System.Linq;

namespace sMap.Common
{
    public static class ListExtensions
    {
        
        public static T Random<T>(this IList<T> array)
        {
            if (array.Count == 0)
            {
                throw new Exception("Check for empty arrays before calling this!");
            }
            if (array.Count == 1)
            {
                return array[0];
            }
            return array[UnityEngine.Random.Range(0, array.Count())];
        }

        public static IOrderedEnumerable<T> Randomize<T>(this IList<T> source, int seed = 1324)
        {
            Random rnd = new Random(seed);
            return source.OrderBy((item) => rnd.Next());
        }

        public static void Fill<T>(this IList<T> array, T obj, int count)
        {
            array.Clear();
            for (var i = 0; i < count; ++i)
            {
                array.Add(obj);
            }
        }

        public static void FillRandom(this float[] array)
        {
            for (var i = 0; i < array.Length; ++i)
            {
                array[i] = UnityEngine.Random.value;
            }
        }
    }
}