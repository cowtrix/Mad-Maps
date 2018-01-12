using UnityEngine;

namespace Dingo.Common
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

        public static Texture2D LockedIcon
        {
            get
            {
                if (__lockedIcon == null)
                {
                    __lockedIcon = Resources.Load<Texture2D>("LockedIcon");
                }
                return __lockedIcon;
            }
        }
        private static Texture2D __lockedIcon;

        public static Texture2D UnlockedIcon
        {
            get
            {
                if (__unlockedIcon == null)
                {
                    __unlockedIcon = Resources.Load<Texture2D>("UnlockedIcon");
                }
                return __unlockedIcon;
            }
        }
        private static Texture2D __unlockedIcon;
    }
}