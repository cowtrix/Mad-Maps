using System;
using sMap.Roads.Connections;
using UnityEngine;

namespace sMap.Roads
{
    //[CreateAssetMenu(menuName = "sRoads/Spline Deformer")]
    public class SplineDeformerConfig : ConnectionComponentConfiguration
    {
        public AnimationCurve Amplitude = new AnimationCurve()
        {
            keys = new Keyframe[]
            {
                new Keyframe(0, 0), 
                new Keyframe(0.2f, 1), 
                new Keyframe(0.8f, 1), 
                new Keyframe(1, 0), 
            }
        };
        public float AmplitudeCoefficient = 1;
        public float Frequency = .1f;

        public override Type GetMonoType()
        {
            return typeof (SplineDeformer);
        }
    }
}