using System;
using System.Collections.Generic;
using System.Linq;
using Dingo.Common;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    [Serializable]
    public class WorldStampCaptureTemplate : ISerializationCallbackReceiver
    {
        public bool Dirty = true;

        public Bounds Bounds;
        public Terrain Terrain;
        public List<WorldStampCreatorLayer> Creators = new List<WorldStampCreatorLayer>()
        {
            new HeightmapDataCreator(), 
            new SplatDataCreator(), 
            new DetailDataCreator(), 
            new TreeDataCreator(), 
            new ObjectDataCreator(),
            new RoadDataCreator(), 
            new MaskDataCreator(),
        };

        [SerializeField] private List<Common.Serialization.DerivedComponentJsonDataRow> _creatorJSON;

        public void OnBeforeSerialize()
        {
            if (!Dirty)
            {
                return;
            }
            Dirty = false;
            _creatorJSON = new List<Common.Serialization.DerivedComponentJsonDataRow>();
            for (int i = 0; i < Creators.Count; i++)
            {
                var layer = Creators[i];
                var row = new Common.Serialization.DerivedComponentJsonDataRow()
                {
                    AssemblyQualifiedName = layer.GetType().AssemblyQualifiedName
                };
                row.JsonText = JSONSerializer.Serialize(layer.GetType(), layer, false, row.SerializedObjects);
                _creatorJSON.Add(row);
            }
        }

        public void OnAfterDeserialize()
        {
            if (_creatorJSON == null || _creatorJSON.Count == 0)
            {
                return;
            }
            Creators.Clear();
            foreach (var row in _creatorJSON)
            {
                var creator = JSONSerializer.Deserialize(Type.GetType(row.AssemblyQualifiedName), row.JsonText,
                    row.SerializedObjects) as WorldStampCreatorLayer;
                Creators.Add(creator);
            }
        }

        /// <summary>
        /// We've got to be a bit careful cloning this, as we want to maintain the mask. 
        /// This means we have to manually call the Serialization callbacks to make sure 
        /// that our JSONClone has the most up to date information.
        /// </summary>
        /// <returns>A deep clone of this object.</returns>
        public WorldStampCaptureTemplate Clone()
        {
            var mask = Creators.First(layer => layer is MaskDataCreator) as MaskDataCreator;
            mask.Mask.OnAfterDeserialize();

            Dirty = true;
            OnBeforeSerialize();
            var newTemplate = new WorldStampCaptureTemplate();
            newTemplate._creatorJSON = new List<Common.Serialization.DerivedComponentJsonDataRow>();
            for (int i = 0; i < _creatorJSON.Count; i++)
            {
                newTemplate._creatorJSON.Add(_creatorJSON[i].JSONClone());
            }
            newTemplate.Terrain = Terrain;
            newTemplate.Bounds = Bounds;
            newTemplate.OnAfterDeserialize();

            mask = newTemplate.Creators.First(layer => layer is MaskDataCreator) as MaskDataCreator;
            mask.Mask.OnAfterDeserialize();

            newTemplate.Dirty = true;
            return newTemplate;
        }

        public void Dispose()
        {
            for (int i = 0; i < Creators.Count; i++)
            {
                Creators[i].Dispose();
            }
        }
    }
}