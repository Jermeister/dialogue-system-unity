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
    private List<Node> nodes => _targetGraphView.nodes.ToList();
    
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
        var exposedPropertiesData = new List<ExposedPropertyData>();

        foreach (var property in _targetGraphView.exposedProperties)
        {
            exposedPropertiesData.Add(new ExposedPropertyData()
            {
                propertyType = property.propertyType,
                propertyName = property.PropertyName,
                propertyValue = property.PropertyValue,
            });
        }

        nodesContainer.exposedProperties = exposedPropertiesData;
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
                thisNodeGuid = outputNode.guid,
                portName = connectedPorts[i].output.portName,
                nextNodeGuid = inputNode.guid
            });
        }

        foreach (var node in nodes)
        {
            var baseNode = node as BaseNode;
            nodesContainer.baseNodesData.Add(new BaseNodeData()
            {
                guid = baseNode.guid,
                nodeName = baseNode.nodeName,
                position = baseNode.GetPosition().position,
                nodeType = baseNode.nodeType,
            });

            switch (baseNode.nodeType)
            {
                case NodeType.DialogueNode:
                    var dialogueNode = node as DialogueNode;
                    var textsList = new List<string>();
                    
                    foreach (var textField in dialogueNode.dialogueTexts)
                    {
                        textsList.Add(textField.text);
                    }

                    nodesContainer.dialogueNodesData.Add(new DialogueNodeData()
                    {
                        guid = baseNode.guid,
                        speaker = dialogueNode?.characterDropdown.value.PropertyName,
                        dialogueTexts = textsList,
                    });
                    break;
                case NodeType.ChoiceNode:
                    var choiceNode = node as ChoiceNode;

                    nodesContainer.choiceNodesData.Add(new ChoiceNodeData()
                    {
                        guid = baseNode.guid,
                        speaker = choiceNode.characterDropdown.value.PropertyName,
                        dialogueText = choiceNode.dialogueTextField.text,
                    });
                    break;
            }
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
        CreateExposedProperties();
        CreateNodes();
        ConnectNodes();
    }

    private void CreateExposedProperties()
    {
        // Clear existing properties and create new ones from data
        _targetGraphView.ClearExposedProperties();
        _targetGraphView.AddExposedPropertiesFromData(_containerCache.exposedProperties);
    }

    private void ConnectNodes()
    {
        var baseNodes = nodes.Cast<BaseNode>().ToList();
        for (int i = 0; i < baseNodes.Count; i++)
        {
            var connections = _containerCache.nodeLinks.Where(x => x.thisNodeGuid == baseNodes[i].guid).ToList();

            for (int j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].nextNodeGuid;
                var targetNode = baseNodes.First(x => x.guid == targetNodeGuid);
                
                if (targetNode == null || targetNode.inputContainer.childCount == 0)
                    continue;
                
                LinkNodes(baseNodes[i].outputContainer[j].Q<Port>(), (Port) targetNode.inputContainer[0]);
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
            switch (nodeData.nodeType)
            {
                case NodeType.StartNode:
                    var startNode = new BaseNode(nodeData.nodeType, _targetGraphView, nodeData.position, nodeData.guid);
                    _targetGraphView.AddElement(startNode);
                    break;
                case NodeType.DialogueNode:
                    DialogueNodeData dialogueData = _containerCache.dialogueNodesData.Find(x => x.guid == nodeData.guid);
                    var dialogueNode = new DialogueNode(_targetGraphView, nodeData, dialogueData);
                    _targetGraphView.AddElement(dialogueNode);
                    break;
                case NodeType.ChoiceNode:
                    ChoiceNodeData choiceData = _containerCache.choiceNodesData.Find(x => x.guid == nodeData.guid);
                    var nodePorts = _containerCache.nodeLinks.Where(x => x.thisNodeGuid == nodeData.guid).ToList();
                    var choiceNode = new ChoiceNode(_targetGraphView, nodeData, choiceData, nodePorts);
                    _targetGraphView.AddElement(choiceNode);
                    break;
                case NodeType.EndNode:
                    var endNode = new BaseNode(nodeData.nodeType, _targetGraphView, nodeData.position, nodeData.guid);
                    _targetGraphView.AddElement(endNode);
                    break;
            }
        }
    }

    private void ClearGraph()
    {
        var baseNodes = nodes.Cast<BaseNode>().ToList();

        if (!_containerCache || _containerCache.nodeLinks == null || baseNodes.Count == 0)
            return;
        
        // Set entry points guid back from the save, discard existing guid.
        //baseNodes.Find(x => x.inputPoint).guid = _containerCache.nodeLinks[0].thisNodeGuid;

        foreach (var node in baseNodes)
        {
            if (node.inputPoint) continue;

            // Remove connections associated with the node
            edges.Where(x=>x.input.node == node).ToList().ForEach(edge=>_targetGraphView.RemoveElement(edge));
            
            // Remove the node
            _targetGraphView.RemoveElement(node);
        }
    }
}
