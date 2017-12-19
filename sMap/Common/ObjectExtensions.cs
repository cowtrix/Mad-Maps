using UnityEngine;

namespace sMap.Common
{
    public static class ColorExtensions
    {
        public static string ToHexString(this Color color)
        {
            return string.Format(
                "#{0}{1}{2}{3}",
                ((int)(color.r * 255f)).ToString("X2"),
                ((int)(color.g * 255f)).ToString("X2"),
                ((int)(color.b * 255f)).ToString("X2"),
                ((int)(color.a * 255f)).ToString("X2")
            );
        }

        public static string ToHexString(this Color32 color)
        {
            return ((Color)color).ToHexString();
        }
    }

    public static class ObjectExtensions
    {
        public static T JSONClone<T>(this T o)
        {
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(o));
        }
    }
}