using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodesContainer : ScriptableObject
{
    public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
    public List<BaseNodeData> baseNodesData = new List<BaseNodeData>();
    //public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
}
