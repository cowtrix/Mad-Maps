using System;
using System.Collections.Generic;
using Dingo.Common;
using Dingo.Terrains;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dingo.WorldStamp.Authoring
{
    [Serializable]
    public class TreeDataCreator : WorldStampCreatorLayer
    {
        [NonSerialized]
        public List<DingoTreeInstance> Trees = new List<DingoTreeInstance>();

        public List<GameObject> IgnoredTrees = new List<GameObject>();

        public override GUIContent Label
        {
            get { return new GUIContent(string.Format("Trees ({0})", Trees.Count));}
        }

        protected override bool HasDataPreview
        {
            get { return false; }
        }

        protected override void CaptureInternal(Terrain terrain, Bounds bounds)
        {
            Trees.Clear();
            var trees = terrain.terrainData.treeInstances;
            var prototypes = new List<TreePrototype>(terrain.terrainData.treePrototypes);
            var expandedBounds = bounds;
            expandedBounds.Expand(Vector3.up * 5000);
            foreach (var tree in trees)
            {
                var worldPos = terrain.TreeToWorldPos(tree.position);
                if (IgnoredTrees.Contains(prototypes[tree.prototypeIndex].prefab))
                {
                    continue;
                }
                if (!expandedBounds.Contains(worldPos))
                {
                    continue;
                }
                var hurtTree = new DingoTreeInstance(tree, prototypes);
                var yDelta = worldPos.y - terrain.SampleHeight(worldPos);
                hurtTree.Position = new Vector3((worldPos.x - bounds.min.x) / bounds.size.x, yDelta, (worldPos.z - bounds.min.z) / bounds.size.z);
                Trees.Add(hurtTree);
            }
        }

        public override void PreviewInDataInspector()
        {
            throw new System.NotImplementedException();
        }

        public override void Clear()
        {
            Trees.Clear();
        }

        protected override void CommitInternal(WorldStampData data, WorldStamp stamp)
        {
            data.Trees.Clear();
            for (int i = 0; i < Trees.Count; i++)
            {
                var dingoTreeInstance = Trees[i];
                data.Trees.Add(dingoTreeInstance.Clone());
            }
        }

#if UNITY_EDITOR
        protected override void PreviewInSceneInternal(WorldStampCreator parent)
        {
            var bounds = parent.Template.Bounds;
            var terrain = parent.Template.Terrain;
            Handles.color = Color.green;
            foreach (var hurtTreeInstance in Trees)
            {
                var pos = new Vector3(hurtTreeInstance.Position.x * bounds.size.x, 0,
                    hurtTreeInstance.Position.z * bounds.size.z) + bounds.min;
                pos.y += terrain.SampleHeight(pos);
                Handles.DrawDottedLine(pos, pos + Vector3.up * 10 * hurtTreeInstance.Scale.x, 1);
            }
        }
#endif
    }
}