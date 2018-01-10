using UnityEngine;

namespace Dingo.Roads
{
    public static partial class GUIResources
    {
        public static Texture2D EyeOpenIcon
        {
            get
            {
                if (__eyeOpenIcon == null)
                {
                    __eyeOpenIcon = Resources.Load<Texture2D>("EyeOpenIcon");
                }
                return __eyeOpenIcon;
            }
        }
        private static Texture2D __eyeOpenIcon;
    }
}