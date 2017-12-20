using System.Collections.Generic;
using Dingo.Common;
using Dingo.Terrains;
using UnityEditor;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    public class TreeDataCreator : WorldStampCreatorLayer
    {
        public List<DingoTreeInstance> Trees = new List<DingoTreeInstance>();

        protected override GUIContent Label
        {
            get { return new GUIContent("Trees");}
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

        protected override void PreviewInSceneInternal(WorldStampCreator parent)
        {
            var bounds = parent.Bounds;
            var terrain = parent.Terrain;
            Handles.color = Color.green;
            foreach (var hurtTreeInstance in Trees)
            {
                var pos = new Vector3(hurtTreeInstance.Position.x * bounds.size.x, 0,
                    hurtTreeInstance.Position.z * bounds.size.z) + bounds.min;
                pos.y += terrain.SampleHeight(pos);
                Handles.DrawDottedLine(pos, pos + Vector3.up * 10 * hurtTreeInstance.Scale.x, 1);
            }
        }

        public override void PreviewInDataInspector()
        {
            throw new System.NotImplementedException();
        }

        public override void Commit(WorldStampData data)
        {
            data.Trees = Trees.JSONClone();
        }

        protected override void OnExpandedGUI(WorldStampCreator parent)
        {
        }
    }
}