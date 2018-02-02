using MadMaps.Common;
using UnityEngine;

namespace MadMaps.Roads
{
    public abstract class NodeComponent : sBehaviour
    {
        [HideInInspector]
        public Node Node;

        public RoadNetwork Network
        {
            get
            {
                if (!Node)
                {
                    Node = GetComponent<Node>();
                }
                return Node.Network;
            }
        }

        public virtual void Think()
        {
        }

        public virtual void Strip()
        {
            DestroyImmediate(this);
        }

        public void Reset()
        {
            Node = GetComponent<Node>();
        }
    }
}