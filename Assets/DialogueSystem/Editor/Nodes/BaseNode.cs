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
    protected GraphView graphView;

    public BaseNode(NodeType _nodeType, GraphView _graphView)
    {
        graphView = _graphView;
        
        switch (_nodeType)
        {
            case NodeType.StartNode:
                SetupStartNode();
                break;
            case NodeType.EndNode:
                SetupEndNode();
                break;
        }
    }

    public BaseNode() { }

    protected void SetupStartNode()
    {
        title = "Start";
        guid = Guid.NewGuid().ToString();
        outputPoint = true;
        inputPoint = false;
        
        SetPosition(new Rect(250, 300, defaultNodeSize.x, defaultNodeSize.y));
        
        var outputPort = GeneratePort(Direction.Output);
        outputPort.portName = "Start";
        outputContainer.Add(outputPort);
        
        capabilities &= ~Capabilities.Deletable;
        
        RefreshExpandedState();
        RefreshPorts();
    }

    protected void SetupEndNode()
    {
        title = "End";
        guid = Guid.NewGuid().ToString();
        outputPoint = false;
        inputPoint = true;
        
        SetPosition(new Rect(550, 300, defaultNodeSize.x, defaultNodeSize.y));
        
        var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "End";
        outputContainer.Add(inputPort);

        RefreshExpandedState();
        RefreshPorts();
    }
    
    protected Port GeneratePort(Direction portDirection, Port.Capacity capacity=Port.Capacity.Single)
    {
        return InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }
    
    protected Port GeneratePortOfType(Direction portDirection, Type type, Port.Capacity capacity=Port.Capacity.Single)
    {
        return InstantiatePort(Orientation.Horizontal, portDirection, capacity, type);
    }
}
