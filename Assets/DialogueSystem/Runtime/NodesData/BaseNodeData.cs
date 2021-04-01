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
    GroupNode,
}

[System.Serializable]
public struct DialogueCharacter
{
    public string characterName;
}


public struct FlowEdge {}

public struct CharacterEdge {}
