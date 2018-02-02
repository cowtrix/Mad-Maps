using UnityEngine;

namespace MadMaps.Roads
{
    public class IntersectionMetadata : NodeComponent
    {
        [Header("> Two Node Case")]
        public Vector3 TwoNodeRotation;

        public void OnInsert(Node insertedNode)
        {
            var insertedNodeConnections = insertedNode.GetAllConnections();
            if (insertedNodeConnections.Count == 0)
            {
                Debug.Log("Simple insertion case on no neighbours.");
                Network.RemoveObject(insertedNode, false);
                return;
            }

            if (insertedNodeConnections.Count > 1)
            {
                transform.rotation *= Quaternion.Euler(TwoNodeRotation);
            }

            var localNodes = GetComponents<Node>();
            if (insertedNodeConnections.Count > localNodes.Length)
            {
                Debug.Log("Complex insertion case (more neighbours than local nodes). Please resolve yourself.");
                return;
            }

            for (int i = insertedNodeConnections.Count-1; i >= 0; i--)
            {
                var connection = insertedNodeConnections[i];
                var neighbour = connection.ThisNode == insertedNode ? connection.NextNode : connection.ThisNode;

                Node closestLocalNode = null;
                float closestLocalNodeDist = float.MaxValue;
                for (int j = 0; j < localNodes.Length; j++)
                {
                    var localNode = localNodes[j];
                    var dist = Vector3.Distance(neighbour.NodePosition, localNode.NodePosition);
                    if (dist < closestLocalNodeDist)
                    {
                        closestLocalNodeDist = dist;
                        closestLocalNode = localNode;
                    }
                }
                if (closestLocalNode != null)
                {
                    Network.ConnectNodes(neighbour, closestLocalNode, connection.Configuration);
                    insertedNodeConnections.RemoveAt(i);
                }
            }
            insertedNode.Destroy();
            DestroyImmediate(insertedNode.gameObject);

            /*if (neighbours.Count == 1)
            {
                Debug.Log("Insertion case on 1 neighbour.");

                var singleNeighbour = neighbours[0];
                var firstDist = Vector3.Distance(FirstLinkNode.NodePosition, insertedNode.NodePosition);
                var secondDist = Vector3.Distance(SecondLinkNode.NodePosition, insertedNode.NodePosition);

                Node bestToLink = firstDist < secondDist ? FirstLinkNode : SecondLinkNode;

                insertedNode.Destroy();
                DestroyImmediate(insertedNode.gameObject);

                Network.ConnectNodes(singleNeighbour, bestToLink);

                return;
            }
            if (neighbours.Count == 2)
            {
                Debug.Log("Insertion case on 2 neighbours.");

                transform.rotation *= Quaternion.Euler(TwoNodeRotation);
            }*/
        }
    }
}