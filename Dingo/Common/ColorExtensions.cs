using UnityEngine;

namespace Dingo.Common
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
}