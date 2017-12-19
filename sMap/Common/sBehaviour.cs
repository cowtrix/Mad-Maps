using UnityEngine;

namespace sMap.Common
{
#if HURTWORLDSDK
    [StripComponentOnBuild]
#endif
    public class sBehaviour : MonoBehaviour, ISerializationCallbackReceiver
    {
        protected bool HasDeserialized { get; private set; }
        
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            HasDeserialized = true;
        }
    }
}