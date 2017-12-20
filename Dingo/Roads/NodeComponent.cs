using Dingo.Common;

namespace Dingo.Roads
{
    public abstract class NodeComponent : sBehaviour
    {
        public Node Node
        {
            get
            {
                if (!__node)
                {
                    __node = GetComponent<Node>();
                }
                return __node;
            }
        }
        private Node __node;

        public RoadNetwork Network
        {
            get { return Node.Network; }
        }

        public virtual void Think()
        {
        }

        public virtual void Strip()
        {
            DestroyImmediate(this);
        }
    }
}