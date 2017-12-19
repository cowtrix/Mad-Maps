using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using sMap.Common;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TerrainSurfaceObject))]
public class TerrainSurfaceObjectGUI : Editor
{
    public void OnSceneGUI()
    {
        var tso = target as TerrainSurfaceObject;
        if (!tso.EditingEnabled)
        {
            return;
        }
        var rot = tso.transform.rotation;
        var pos = tso.transform.position;
        for (int i = 0; i < tso.CastPoints.Count; i++)
        {
            var castConfig = tso.CastPoints[i];
            var castOffset = castConfig.Position;
            castOffset = Vector3.Scale(castOffset, tso.transform.localScale);

            var castPos = pos + rot * castOffset;
            castConfig.Position = Vector3.Scale(Quaternion.Inverse(rot) * (Handles.DoPositionHandle(castPos, rot) - pos), tso.transform.localScale.Inverse());
            Handles.DrawDottedLine(castPos, castPos + Vector3.down * castConfig.AcceptableDistance, 1);
            tso.CastPoints[i] = castConfig;
        }
    }
}
#endif

[StripComponentOnBuild]
public class TerrainSurfaceObject : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Level/Iterate Cliff Objects")]
#endif
    public static void DoAll()
    {
        var all = FindObjectsOfType<TerrainSurfaceObject>();
        for (int i = 0; i < all.Length; i++)
        {
            all[i].Recalculate();
        }
    }

    [Serializable]
    public struct CastConfig
    {
        public Vector3 Position;
        public float AcceptableDistance;
    }
    
    public List<CastConfig> CastPoints = new List<CastConfig>();

    public bool EditingEnabled;

    public int RequireCasts = 3;
    public LayerMask Mask;
    public const float CastDist = 100;
    public bool Enabled = true;

    [ContextMenu("Recalculate")]
    public void Recalculate()
    {
        if (!Enabled)
        {
            return;
        }
        
        var pos = transform.position;
        var rot = transform.rotation;
        float minY = transform.position.y;
        int successCount = 0;
        foreach (var castConfig in CastPoints)
        {
            var castOffset = castConfig.Position;
            castOffset = Vector3.Scale(castOffset, transform.localScale);
            var acceptableDist = castConfig.AcceptableDistance*transform.localScale.y;
            var castPos = pos + rot * castOffset;
            var yDelta = castPos.y - transform.position.y;
            var castDist = CastDist*transform.localScale.y;

            var hits = Physics.RaycastAll(castPos, Vector3.down, castDist, Mask);
            if (hits.Length == 0)
            {
                if (EditingEnabled)
                    Debug.DrawLine(castPos, castPos + Vector3.down * castDist, Color.red, 10);
                continue;
            }

            hits = hits.OrderBy(raycastHit => raycastHit.distance).ToArray();
            for (int i = 0; i < hits.Length; i++)
            {
                var raycastHit = hits[i];
                if (raycastHit.collider.transform == transform)
                {
                    continue;
                }
                if (raycastHit.distance <= acceptableDist)
                {
                    if (EditingEnabled)
                        Debug.DrawLine(castPos, castPos + Vector3.down * raycastHit.distance, Color.green, 10);
                    successCount++;
                    break;
                }
                var candidateMinY = raycastHit.point.y + acceptableDist - yDelta;
                if (candidateMinY  < minY)
                {
                    if(EditingEnabled)
                        Debug.DrawLine(castPos, castPos.xz().x0z(candidateMinY), Color.grey, 10);
                    minY = candidateMinY;
                }
                
                break;
            }
        }

        if (successCount >= RequireCasts)
        {
            return;
        }

        transform.position = new Vector3(transform.position.x, minY, transform.position.z);
#if UNITY_EDITOR
        EditorUtility.SetDirty(transform);
#endif

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, CastDist, 1 << 21))
        {
            var mmObjAbove = hit.collider.gameObject.GetComponent<TerrainSurfaceObject>();
            if (mmObjAbove != null && !mmObjAbove.Equals(null))
            {
                if (ReferenceEquals(mmObjAbove, this))
                {
                    Debug.LogError("Hit self");
                    return;
                }
                //mmObjAbove.PlaceCount++;
                mmObjAbove.Recalculate();
            }
        }
    }
    
    public bool Serialize
    {
        get { return true; }
    }
}
