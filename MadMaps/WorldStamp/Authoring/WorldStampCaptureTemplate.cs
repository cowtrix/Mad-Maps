using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadMaps.WorldStamp.Authoring
{
    [Serializable]
    public class WorldStampCaptureTemplate
    {
#if UNITY_EDITOR
        public Bounds Bounds;
        public Terrain Terrain;

        public int Layer;
        public DetailDataCreator DetailDataCreator = new DetailDataCreator();
        public HeightmapDataCreator HeightmapDataCreator = new HeightmapDataCreator();
        public MaskDataCreator MaskDataCreator = new MaskDataCreator();
        public ObjectDataCreator ObjectDataCreator = new ObjectDataCreator();
        public RoadDataCreator RoadDataCreator = new RoadDataCreator();
        public SplatDataCreator SplatDataCreator = new SplatDataCreator();
        public TreeDataCreator TreeDataCreator = new TreeDataCreator();

        public List<WorldStampCreatorLayer> Creators
        {
            get
            {
                if (__creators == null)
                {
                    __creators = new List<WorldStampCreatorLayer>
                    {
                        HeightmapDataCreator,
                        SplatDataCreator,
                        DetailDataCreator,
                        TreeDataCreator,
                        ObjectDataCreator,
                        RoadDataCreator,
                        MaskDataCreator
                    };
                }
                return __creators;
            }
        }
        private List<WorldStampCreatorLayer> __creators;
#endif
    }
}