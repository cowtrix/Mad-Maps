using System;
using System.Collections.Generic;
using sMap.Terrains;
using UnityEngine;

namespace sMap.Roads
{
    //[CreateAssetMenu(menuName = "sRoads/Connection Terrain Height Configuration")]
    public class ConnectionTerrainConfiguration : ConnectionComponentConfiguration
    {
        [Serializable]
        public class SplatConfig
        {
            public SplatPrototypeWrapper SplatPrototype;
            public float SplatStrength = 1;
        }

        /*public bool UseThreshold;
        [RequireBool("UseThreshold")]
        public float Threshold = float.MaxValue;*/

        //public bool SetHeight = true;
        public AnimationCurve HeightFalloff = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public AnimationCurve Height = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        public float Radius = 10;

        public bool RemoveTrees = true;
        public float TreeRemoveDistance = 1;

        public bool RemoveGrass = true;
        public AnimationCurve GrassFalloff = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        public bool SetSplat = false;
        public AnimationCurve SplatFalloff = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public List<SplatConfig> SplatConfigurations = new List<SplatConfig>();
        
        public bool RemoveObjects = true;
        public LayerMask ObjectRemovalMask = 1 << 21;
        public float ObjectRemoveDistance = 1;
        public string RegexMatch;

        public override Type GetMonoType()
        {
            throw new NotImplementedException();
        }
    }
}

