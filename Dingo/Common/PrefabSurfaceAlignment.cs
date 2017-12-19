#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dingo.Common
{
    [Serializable]
    public class PointConfiguration
    {
        public Vector3 PointA = Vector3.forward + Vector3.left;
        public Vector3 PointB = Vector3.forward + Vector3.right;
        public Vector3 PointC = Vector3.back;
    }
    [StripComponentOnBuild]
    public class PrefabSurfaceAlignment : MonoBehaviour
    {
        public int SelectedIndex = 0;
        public List<PointConfiguration> Configurations = new List<PointConfiguration>(); 

        public Vector3 UpVector = Vector3.up;

        public void OnDrawGizmosSelected()
        {
            if (SelectedIndex >= 0 && SelectedIndex < Configurations.Count && Configurations[SelectedIndex] != null)
            {
                var item = Configurations[SelectedIndex];
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position + transform.rotation * item.PointA, 0.5f);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + transform.rotation * item.PointB, 0.5f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position + transform.rotation * item.PointC, 0.5f);
                Gizmos.color = Color.white;
            }

        }

        [MenuItem("Tools/Level/ExecuteSurfaceAlignment")]
        public static void AlignAll()
        {
            var items = FindObjectsOfType<PrefabSurfaceAlignment>();
            foreach (var prefabSurfaceAlignment in items)
            {
                prefabSurfaceAlignment.Align();
            }
        }
        [ContextMenu("Solve")]
        public void Solve()
        {
            Align(SelectedIndex);
        }

        
        public void Align(int index = -1)
        {
            var item = index == -1 ? Configurations.Random() : Configurations[index];

            var PointA = item.PointA;
            var PointB = item.PointB;
            var PointC = item.PointC;

            var terrain = FindObjectOfType<Terrain>();
            var newPosition = transform.position;

            //Debug.DrawLine(newPosition,hit.point,Color.red,10f);
            var newRotation = transform.rotation;

            //Debug.Log("Hit.distance " + hit.distance);
            newPosition = newPosition + Vector3.down * TerrainHeightDelta(newPosition + newRotation * PointA,terrain);

            var pointAWorldSpace = newPosition + newRotation * PointA;

            float increment = 10f;
            float lastSign = -1;
            int count = 0;
            
            while (true)
            {
                var pointBworldSpace = newPosition + newRotation * PointB;

                var delta = TerrainHeightDelta(pointBworldSpace, terrain);
                var sign = Mathf.Sign(delta);
                if (sign != lastSign)
                {
                    increment = increment/2f;
                    if (increment < 0.001f)
                    {
                        break;
                    }
                }

                if (count > 500)
                {
                    Debug.LogError("Exceeded search count");
                    break;
                }
                count++;
                lastSign = sign;
                //Debug.Log("Rotating " + (sign > 0 ? "Down" : "Up") + " by increment " + increment);
                var rotationAngle = Vector3.Cross(newRotation * Vector3.up, newRotation * PointB - newRotation * PointA);
                //Debug.DrawLine(pointAWorldSpace, pointAWorldSpace + rotationAngle, Color.green, 10f);
                newPosition = RotatePointAroundPivot(ref newRotation, newPosition, pointAWorldSpace, rotationAngle, increment * sign);
            }
            var abVector = (newPosition + newRotation*PointB) - pointAWorldSpace;
            var abCenter = (abVector) / 2 + pointAWorldSpace;
            increment = 10;
            while (true)
            {
                var pointCworldSpace = newPosition + newRotation * PointC;

                var delta = TerrainHeightDelta(pointCworldSpace, terrain);
                var sign = Mathf.Sign(delta);
                if (sign != lastSign)
                {
                    increment = increment / 2f;
                    if (increment < 0.001f)
                    {
                        break;
                    }
                }

                if (count > 500)
                {
                    Debug.LogError("Exceeded search count");
                    break;
                }
                count++;
                lastSign = sign;
                //Debug.Log("Rotating " + (sign > 0 ? "Down" : "Up") + " by increment " + increment);
                var rotationAngle = abVector;
                //Debug.DrawLine(abCenter, abCenter + rotationAngle, Color.magenta, 10f);
                newPosition = RotatePointAroundPivot(ref newRotation, newPosition, abCenter, rotationAngle, -increment * sign);
            }
            
            //Debug.DrawLine(pointAWorldSpace, pointAWorldSpace + Vector3.Cross(hit.normal, (midPoint - newPosition) - newRotation * PointA), Color.green, 10f);
            //newPosition = RotatePointAroundPivot(ref newRotation, newPosition, pointAWorldSpace, Vector3.Cross(hit.normal, (midPoint - newPosition) - newRotation * PointA), -30);
            transform.position = newPosition;
            transform.rotation = newRotation;
        }

        public Vector3 RotatePointAroundPivot(ref Quaternion startRotation, Vector3 point, Vector3 pivot, Vector3 axis, float degrees)
        {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            var rot = Quaternion.AngleAxis(degrees, axis);
            dir = rot * dir; // rotate it
            startRotation = rot*startRotation;
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }

        public float TerrainHeightDelta(Vector3 point, Terrain terrain)
        {
            var sample = terrain.SampleHeight(point) + terrain.GetPosition().y;
            return point.y - sample;
        }
    }
}
#endif