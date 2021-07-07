using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodesContainer : ScriptableObject
{
    public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
    public List<BaseNodeData> baseNodesData = new List<BaseNodeData>();
    public List<DialogueNodeData> dialogueNodesData = new List<DialogueNodeData>();
    public List<ChoiceNodeData> choiceNodesData = new List<ChoiceNodeData>();
    public List<ExposedPropertyData> exposedProperties = new List<ExposedPropertyData>();
}
