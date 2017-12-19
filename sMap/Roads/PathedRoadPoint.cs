using System;
using sMap.Roads;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;

[StripComponentOnBuild]
class PathedRoadPoint : HurtMonoBehavior
{
    public PathedRoadPoint[] Connections = new PathedRoadPoint[0];
    public ConnectionConfiguration Configuration;

    private Node _myNode;

#if UNITY_EDITOR
    [MenuItem("Tools/Make Pathed Road Connections")]
    public static void MakeConnections()
    {
        /*
        var newSettings = NavMesh.CreateSettings();
        newSettings.agentRadius = 5;
        newSettings.agentClimb = 1.3f;
        newSettings.agentSlope = 33f;
        newSettings.agentHeight = 3f;
        newSettings.
         * */
        var nodes = FindObjectsOfType<PathedRoadPoint>();
        foreach (var pathedRoadPoint in nodes)
        {
            foreach (var connection in pathedRoadPoint.Connections)
            {
                pathedRoadPoint.PathToNode(connection);
            }
        }

    }

    public void PathToNode(PathedRoadPoint endPoint)
    {
        NavMeshPath path = new NavMeshPath();
        NavMeshHit hit;
        
        var queryFilter = new NavMeshQueryFilter {agentTypeID = GetAgentId(), areaMask = ~0};

        var localPoint = transform.position;
        NavMesh.SamplePosition(localPoint, out hit, 999, queryFilter);
        localPoint = hit.position;

        var remotePoint = endPoint.transform.position;
        NavMesh.SamplePosition(remotePoint, out hit, 999, queryFilter);
        remotePoint = hit.position;

        if (!NavMesh.CalculatePath(localPoint, remotePoint, queryFilter, path))
        {
            Debug.Log("No Path");
            return;
        }

        var lastNode = GetNode();

        for (int i = 0; i < path.corners.Length; i++)
        {
            var corner = path.corners[i];
            if (Vector3.Distance(corner, lastNode.transform.position) < 80f)
            {
                continue;
            }
            if (i == path.corners.Length - 1)
            {
                RoadNetwork.LevelInstance.ConnectNodes(endPoint.GetNode(), lastNode, Configuration);
                break;
            }
            //TODO: Set these to the terrain normal
            lastNode = RoadNetwork.LevelInstance.CreateNewNode(corner, Vector3.up, lastNode, Configuration, null);
        }
    }

    private int GetAgentId()
    {
        var count = NavMesh.GetSettingsCount();
        for (var i = 0; i < count; i++)
        {
            var settings = NavMesh.GetSettingsByIndex(i);

            if (settings.agentRadius >= 5)
            {
                return settings.agentTypeID;
            }
        }

        throw new Exception("No agent found matching settings");
    }

    public Node GetNode()
    {
        if (_myNode == null)
        {
            _myNode = RoadNetwork.LevelInstance.CreateNewNode(transform.position, transform.up, null, Configuration, null);
        }
        return _myNode;
    }
#endif
}

