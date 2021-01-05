using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public struct BaseNodeData
{
    public string guid;
    public string nodeName;
    public Vector2 position;
    public NodeType nodeType;
}

[System.Serializable]
public enum NodeType
{
    StartNode,
    EndNode,
    DialogueNode,
    ChoiceNode,
}

[System.Serializable]
public struct DialogueCharacter
{
    public string characterName;
}

public class FlowEdge : EdgeConnector
{
    protected override void RegisterCallbacksOnTarget()
    {
        Debug.Log("Ayyyy");
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        
    }

    public override EdgeDragHelper edgeDragHelper { get; }
}
