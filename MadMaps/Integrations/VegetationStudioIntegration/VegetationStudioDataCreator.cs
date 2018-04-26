using MadMaps.Terrains;
using System;
using System.Collections.Generic;
using MadMaps.Common;
using UnityEngine;

#if VEGETATION_STUDIO

using AwesomeTechnologies.Vegetation.PersistentStorage;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MadMaps.WorldStamp.Authoring
{
    [Serializable]
    public class VegetationStudioDataCreator : WorldStampCreatorLayer
    {
        [NonSerialized]
        public List<VegetationStudioInstance> VSData = new List<VegetationStudioInstance>();
        //public List<GameObject> Prototypes = new List<GameObject>();
        //public List<GameObject> IgnoredTrees = new List<GameObject>();

        public override GUIContent Label
        {
            get { return new GUIContent(string.Format("Vegetation Studio ({0})", VSData.Count));}
        }

        protected override bool HasDataPreview
        {
            get { return false; }
        }

        protected override void CaptureInternal(Terrain terrain, Bounds bounds)
        {
            //Prototypes.Clear();
            VSData.Clear();

            var vegSystems = VegetationStudioUtilities.GetVegetationSystemsForTerrain(terrain);

            var expandedBounds = bounds;
            expandedBounds.Expand(Vector3.up * 5000);

            foreach(var vegSys in vegSystems)
            {
                var vegStorage = vegSys.GetComponent<PersistentVegetationStorage>();
                if(!vegStorage)
                {
                    continue;
                }
                foreach(var cell in vegStorage.PersistentVegetationStoragePackage.PersistentVegetationCellList)
                {
                    foreach(var info in cell.PersistentVegetationInfoList)
                    {
                        foreach(var item in info.VegetationItemList)
                        {
                            var worldPos = terrain.transform.position + item.Position;                
                            if (!expandedBounds.Contains(worldPos))
                            {
                                continue;
                            }

                            VSData.Add(new VegetationStudioInstance()
                            {
                                Guid = System.Guid.NewGuid().ToString(),
                                Position = terrain.WorldToTreePos(worldPos),
                                Rotation = item.Rotation.eulerAngles,
                                Scale = item.Scale,
                                VSID = info.VegetationItemID,
                            });
                        }
                    }
                }
            }
        }

        public override void PreviewInDataInspector()
        {
            throw new System.NotImplementedException();
        }

        public override void Clear()
        {
            VSData.Clear();
        }

        protected override void CommitInternal(WorldStampData data, WorldStamp stamp)
        {
            data.VSData.Clear();
            data.VSData.AddRange(VSData);
            /*data..Clear();
            for (int i = 0; i < Trees.Count; i++)
            {
                var treeInstance = Trees[i];
                if (IgnoredTrees.Contains(treeInstance.Prototype))
                {
                    continue;
                }
                data.Trees.Add(treeInstance.Clone());
            }*/
        }

#if UNITY_EDITOR
        protected override void PreviewInSceneInternal(WorldStampCreator parent)
        {
            var bounds = parent.Template.Bounds;
            var terrain = parent.Template.Terrain;
            Handles.color = Color.green;
            foreach (var vsInstance in VSData)
            {
                var pos = terrain.transform.position + vsInstance.Position;
                Handles.DrawDottedLine(pos, pos + Vector3.up * 10 * vsInstance.Scale.x, 1);
            }
        }

        protected override void OnExpandedGUI(WorldStampCreator parent)
        {
            if (VSData.Count == 0)
            {
                EditorGUILayout.HelpBox("No VegetationStudio Data Found", MessageType.Info);
                return;
            }
            /*for (int i = 0; i < Prototypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                Prototypes[i] = (GameObject)EditorGUILayout.ObjectField(Prototypes[i],
                    typeof (GameObject), false);
                GUI.color = Prototypes[i] != null && IgnoredTrees.Contains(Prototypes[i])
                    ? Color.red
                    : Color.white;
                GUI.enabled = Prototypes[i] != null;
                if (GUILayout.Button("Ignore", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    if (IgnoredTrees.Contains(Prototypes[i]))
                    {
                        IgnoredTrees.Remove(Prototypes[i]);
                    }
                    else
                    {
                        IgnoredTrees.Add(Prototypes[i]);
                    }
                }
                GUI.enabled = true;
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }*/
        }
#endif
    }
}
#endif