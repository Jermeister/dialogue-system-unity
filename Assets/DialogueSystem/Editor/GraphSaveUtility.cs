using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private DialogueGraphView _targetGraphView;
    private NodesContainer _containerCache;

    private List<Edge> edges => _targetGraphView.edges.ToList();
    private List<BaseNode> nodes => _targetGraphView.nodes.ToList().Cast<BaseNode>().ToList();
    
    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        var dialogueContainer = ScriptableObject.CreateInstance<NodesContainer>();

        if (!SaveNodes(dialogueContainer))
        {
            return;
        }
        
        SaveExposedProperties(dialogueContainer);


        if (!Directory.Exists("Assets/Resources"))
            Directory.CreateDirectory("Assets/Resources");
        
        if (!Directory.Exists("Assets/Resources/Dialogues"))
            Directory.CreateDirectory("Assets/Resources/Dialogues");
        
        AssetDatabase.DeleteAsset($"Assets/Resources/Dialogues/{fileName}.asset");
        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/Dialogues/{fileName}.asset");
        AssetDatabase.SaveAssets();
        
        Selection.activeObject=AssetDatabase.LoadMainAssetAtPath($"Assets/Resources/Dialogues/{fileName}.asset");
    }

    private void SaveExposedProperties(NodesContainer nodesContainer)
    {
        nodesContainer.exposedProperties.AddRange(_targetGraphView.exposedProperties);
    }


    public bool SaveNodes(NodesContainer nodesContainer)
    {
        // No connections, don't save anything
        if (!edges.Any())
        {
            EditorUtility.DisplayDialog("Error", "No connections. Please connect some nodes to save.", "OK");
            return false;
        }
        
        // No connections from Start Node
        var entryPoint = edges.Find(x => x.output.node.title == "Start");        
        if(entryPoint == null)
        {
            EditorUtility.DisplayDialog("Error", "Start Node must be connected before saving.", "OK");
            return false;
        }

        var connectedPorts = edges.Where(x => x.input.node != null).OrderByDescending(x => ((BaseNode)(x.output.node)).inputPoint).ToArray();

        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as BaseNode;
            var inputNode = connectedPorts[i].input.node as BaseNode;
            
            nodesContainer.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGuid = outputNode.guid,
                portName = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.guid
            });
        }

        foreach (var node in nodes.Where(node => !node.inputPoint))
        {
            nodesContainer.baseNodesData.Add(new BaseNodeData()
            {
                guid = node.guid,
                nodeName = node.nodeName,
                position = node.GetPosition().position,
            });
        }

        return true;
    }

    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<NodesContainer>($"Dialogues/{fileName}");

        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File not found!", "Target dialogue graph file does not exist.", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
        CreateExposedProperties();
    }

    private void CreateExposedProperties()
    {
        _targetGraphView.ClearExposedProperties();
        // Clear existing properties and create new ones from data
        foreach (var property in _containerCache.exposedProperties)
        {
            _targetGraphView.AddPropertyToBlackboard(property);
        }
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var connections = _containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodes[i].guid).ToList();

            for (int j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].targetNodeGuid;
                var targetNode = nodes.First(x => x.guid == targetNodeGuid);
                LinkNodes(nodes[i].outputContainer[j].Q<Port>(), (Port) targetNode.inputContainer[0]);
                
                targetNode.SetPosition(new Rect(_containerCache.baseNodesData.First(x => x.guid == targetNodeGuid).position, _targetGraphView.defaultNodeSize));
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };
        
        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        
        _targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (var nodeData in _containerCache.baseNodesData)
        {
            // TODO: Make separate Creates for all types of Nodes
            switch (nodeData.nodeType)
            {
                case NodeType.StartNode:
                    break;
                case NodeType.DialogueNode:
                    //var tempNode = _targetGraphView.CreateDialogueNode(nodeData.dialogueName, nodeData.dialogueText, Vector2.zero);
                    //tempNode.guid = nodeData.guid;
                    //_targetGraphView.AddElement(tempNode);
                    //var nodePorts = _containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodeData.guid).ToList();
                    //nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
                    break;
                case NodeType.ChoiceNode:
                    break;
                case NodeType.EndNode:
                    break;
            }
        }
    }

    private void ClearGraph()
    {
        // Set entry points guid back from the save, discard existing guid.
        nodes.Find(x => x.inputPoint).guid = _containerCache.nodeLinks[0].baseNodeGuid;

        foreach (var node in nodes)
        {
            if (node.inputPoint) continue;

            // Remove connections associated with the node
            edges.Where(x=>x.input.node == node).ToList().ForEach(edge=>_targetGraphView.RemoveElement(edge));
            
            // Remove the node
            _targetGraphView.RemoveElement(node);
        }
    }
}
