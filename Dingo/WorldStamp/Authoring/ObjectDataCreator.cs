using System;
using System.Collections.Generic;
using System.Linq;
using Dingo.Common;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dingo.WorldStamp.Authoring
{
    [Serializable]
    public class ObjectDataCreator : WorldStampCreatorLayer
    {
        public LayerMask Mask = ~0;

        [NonSerialized]
        public Dictionary<PrefabObjectData, Bounds> BoundsMapping = new Dictionary<PrefabObjectData, Bounds>();
        [NonSerialized]
        public List<PrefabObjectData> Objects = new List<PrefabObjectData>();

        protected override void CaptureInternal(Terrain terrain, Bounds bounds)
        {
#if UNITY_EDITOR
            BoundsMapping.Clear();
            Objects.Clear();

            var expandedBounds = bounds;
            expandedBounds.Expand(Vector3.up * 5000);

            var terrainPos = terrain.GetPosition();

            var allTransforms = Object.FindObjectsOfType<Transform>();
            var done = new HashSet<Transform>();
            allTransforms = allTransforms.OrderBy(transform => TransformExtensions.GetHierarchyDepth(transform)).ToArray();
            HashSet<Transform> ignores = new HashSet<Transform>();
            for (int i = 0; i < allTransforms.Length; i++)
            {
                var transform = allTransforms[i];
                if (done.Contains(transform) || ignores.Contains(transform))
                {
                    continue;
                }
                if (transform.GetComponent<TerrainCollider>())
                {
                    continue;
                }
                if (!expandedBounds.Contains(transform.position))
                {
                    continue;
                }
                if (transform.GetComponentInChildren<WorldStamp>())
                {
                    Debug.Log(string.Format("WorldStamp Object Capture : Ignored {0} as it contained a WorldStamp. Recursive WorldStamps are currently not supported.", transform), transform);
                    ignores.Add(transform);
                    var children = transform.GetComponentsInChildren<Transform>(true);
                    foreach (var ignore in children)
                    {
                        ignores.Add(ignore);
                    }
                    continue;
                }

                var go = transform.gameObject;
                if (Mask != (Mask | (1 << go.layer)))
                {
                    continue;
                }

                var prefabRoot = PrefabUtility.GetPrefabObject(go);
                if (prefabRoot == null)
                {
                    //DebugHelper.DrawCube(collider.bounds.center, collider.bounds.extents, Quaternion.identity, Color.red, 30);
                    continue;
                }

                done.Add(transform);
                var subTransforms = transform.GetComponentsInChildren<Transform>();
                foreach (var childTransform in subTransforms)
                {
                    done.Add(childTransform);
                }

                var prefabAsset = PrefabUtility.GetPrefabParent(go) as GameObject;
                var root = PrefabUtility.FindPrefabRoot(go);

                var relativePos = root.transform.position - bounds.min;
                relativePos = new Vector3(relativePos.x / bounds.size.x, 0, relativePos.z / bounds.size.z);
                var terrainHeightAtPoint = (terrain.SampleHeight(root.transform.position) + terrainPos.y);
                relativePos.y = (root.transform.position.y - terrainHeightAtPoint);

                var newData = new PrefabObjectData()
                {
                    Prefab = prefabAsset,
                    Position = relativePos,
                    Rotation = root.transform.rotation.eulerAngles,
                    Scale = root.transform.lossyScale,
                    Guid = GUID.Generate().ToString(),
                };

                var c = transform.GetComponentInChildren<Collider>();
                if (c)
                {
                    BoundsMapping[newData] = c.bounds;
                }
                else
                {
                    var r = transform.GetComponentInChildren<Renderer>();
                    if (r)
                    {
                        BoundsMapping[newData] = r.bounds;
                    }
                }
                Objects.Add(newData);
            }
#endif
        }

#if UNITY_EDITOR
        protected override void OnExpandedGUI(WorldStampCreator parent)
        {
            Mask = LayerMaskFieldUtility.LayerMaskField("Layer Mask", Mask, false);
        }

        protected override void PreviewInSceneInternal(WorldStampCreator parent)
        {
            Handles.color = Color.white.WithAlpha(.5f);
            for (int i = 0; i < Objects.Count; i++)
            {
                var b = Objects[i];
                Bounds objBounds;
                if (BoundsMapping.TryGetValue(b, out objBounds))
                {
                    Handles.DrawWireCube(objBounds.center, objBounds.size);
                }
            }
        }
#endif

        public override void PreviewInDataInspector()
        {
            throw new System.NotImplementedException();
        }

        public override void Clear()
        {
            Objects.Clear();
        }

        protected override void CommitInternal(WorldStampData data, WorldStamp stamp)
        {
            data.Objects.Clear();
            foreach (var prefabObjectData in Objects)
            {
                data.Objects.Add(prefabObjectData.JSONClone());
            }
        }

        public override GUIContent Label
        {
            get { return new GUIContent(string.Format("Objects ({0})", Objects.Count));}
        }

        protected override bool HasDataPreview
        {
            get { return false; }
        }
    }
}