using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BaseNode : Node
{
    public string guid;
    public string nodeName;
    public NodeType nodeType;
    public Vector2 position;
    public bool inputPoint = false;
    public bool outputPoint = false;
    
    public readonly Vector2 defaultNodeSize = new Vector2(200,150);
    public readonly Vector2 defaultPosition = new Vector2(300, 250);
    protected DialogueGraphView graphView;

    public BaseNode(NodeType _nodeType, DialogueGraphView _graphView, Vector2 position = default)
    {
        graphView = _graphView;

        if (position == default)
            position = defaultPosition;

        switch (_nodeType)
        {
            case NodeType.StartNode:
                SetupStartNode(position);
                break;
            case NodeType.EndNode:
                SetupEndNode(position);
                break;
        }
    }

    public BaseNode() { }

    protected void SetupStartNode(Vector2 position)
    {
        title = "Start";
        guid = Guid.NewGuid().ToString();
        nodeType = NodeType.StartNode;
        outputPoint = true;
        inputPoint = false;
        
        SetPosition(new Rect(position.x, position.y, defaultNodeSize.x, defaultNodeSize.y));
        
        var outputPort = GeneratePort(Direction.Output);
        outputPort.portName = "Start";
        outputContainer.Add(outputPort);
        
        capabilities &= ~Capabilities.Deletable;
        
        RefreshExpandedState();
        RefreshPorts();
    }

    protected void SetupEndNode(Vector2 position)
    {
        title = "End";
        guid = Guid.NewGuid().ToString();
        nodeType = NodeType.EndNode;
        outputPoint = false;
        inputPoint = true;
        
        SetPosition(new Rect(position.x, position.y, defaultNodeSize.x, defaultNodeSize.y));
        
        var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "End";
        outputContainer.Add(inputPort);

        RefreshExpandedState();
        RefreshPorts();
    }
    
    protected Port GeneratePort(Direction portDirection, Port.Capacity capacity=Port.Capacity.Single)
    {
        return InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(FlowEdge));
    }
    
    protected Port GeneratePortOfType(Direction portDirection, Type type, Port.Capacity capacity=Port.Capacity.Single)
    {
        return InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
    }
}
