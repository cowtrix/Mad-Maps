using System;
using System.Collections.Generic;
using sMap.Common;
using UnityEngine;

namespace sMap.Roads
{
    //[CreateAssetMenu(menuName = "sRoads/Tree Planter")]
    public class TreePlanterConfig : ConnectionComponentConfiguration
    {
        public AnimationCurve ProbablityAlongCurve = new AnimationCurve()
        {
            keys = new Keyframe[]
            {
                new Keyframe(0, 1), 
                new Keyframe(1, 1), 
            }
        };
        public AnimationCurve ProbablityThroughCurve = new AnimationCurve()
        {
            keys = new Keyframe[]
            {
                new Keyframe(0, -1), 
                new Keyframe(0.5f, 0), 
                new Keyframe(1, 1),
            }
        };


        public FloatMinMax Size = new FloatMinMax(1,1);
        public ColorMinMax Color = new ColorMinMax(UnityEngine.Color.white, UnityEngine.Color.white);
        public List<GameObject> Trees = new List<GameObject>();
        public float ProbabilityMultiplier = 1;
        public FloatMinMax StepDistance = new FloatMinMax(1,1);
        public float OffsetMultiplier = 5f;

        public override Type GetMonoType()
        {
            return typeof(PlaceTerrainTrees);
        }
    }
}