using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dingo.WorldStamp.Authoring
{
    [Serializable]
    public class WorldStampCaptureTemplate //: ISerializationCallbackReceiver
    {
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
    }
}