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
    private DialogueGraphView targetGraphView;
    private NodesContainer containerCache;

    private List<Edge> edges => targetGraphView.edges.ToList();
    private List<Node> nodes => targetGraphView.nodes.ToList();
    
    public static GraphSaveUtility GetInstance(DialogueGraphView graphView)
    {
        return new GraphSaveUtility
        {
            targetGraphView = graphView
        };
    }

    public void SaveGraph(string fileName)
    {
        var dialogueContainer = ScriptableObject.CreateInstance<NodesContainer>();

        // No connections, don't save anything
        if (!edges.Any())
        {
            EditorUtility.DisplayDialog("Error", "No connections. Please connect some nodes to save.", "OK");
            return;
        }
        
        // No connections from Start Node
        var entryPoint = edges.Find(x => x.output.node.title == "Start");        
        if(entryPoint == null)
        {
            EditorUtility.DisplayDialog("Error", "Start Node must be connected before saving.", "OK");
            return;
        }
        
        if (!BuildNodesContainer(dialogueContainer))
        {
            return;
        }
        
        SaveExposedProperties(dialogueContainer);


        if (!Directory.Exists("Assets/Resources"))
            Directory.CreateDirectory("Assets/Resources");
        
        if (!Directory.Exists("Assets/Resources/Dialogues"))
            Directory.CreateDirectory("Assets/Resources/Dialogues");
        
        if (Directory.Exists($"Assets/Resources/Dialogues/{fileName}.asset"))
        {
            var outputContainer = AssetDatabase.LoadMainAssetAtPath ($"Assets/Resources/Dialogues/{fileName}.asset") as NodesContainer;
            EditorUtility.CopySerialized(dialogueContainer, outputContainer);
        }
        else
        {
            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/Dialogues/{fileName}.asset");
        }
        
        AssetDatabase.SaveAssets();
        Selection.activeObject=AssetDatabase.LoadMainAssetAtPath($"Assets/Resources/Dialogues/{fileName}.asset");
    }

    private void SaveExposedProperties(NodesContainer nodesContainer)
    {
        var exposedPropertiesData = new List<ExposedPropertyData>();

        foreach (var property in targetGraphView.exposedProperties)
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

    public NodesContainer GetNodesContainer()
    {
        var dialogueContainer = ScriptableObject.CreateInstance<NodesContainer>();
        
        if (!BuildNodesContainer(dialogueContainer))
        {
            return null;
        }
        
        return dialogueContainer;
    }

    public bool BuildNodesContainer(NodesContainer nodesContainer)
    {
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
            nodesContainer.baseNodesData.Add(baseNode.CopyData(true));

            switch (baseNode.nodeType)
            {
                case NodeType.DialogueNode:
                    var dialogueNode = node as DialogueNode;
                    nodesContainer.dialogueNodesData.Add(dialogueNode.CopyData(true));
                    break;
                case NodeType.ChoiceNode:
                    var choiceNode = node as ChoiceNode;
                    nodesContainer.choiceNodesData.Add(choiceNode.CopyData(true));
                    break;
            }
        }

        return true;
    }

    public void LoadGraph(string fileName)
    {
        containerCache = Resources.Load<NodesContainer>($"Dialogues/{fileName}");

        if (containerCache == null)
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
        targetGraphView.ClearExposedProperties();
        targetGraphView.AddExposedPropertiesFromData(containerCache.exposedProperties);
    }

    private void ConnectNodes()
    {
        var baseNodes = nodes.Cast<BaseNode>().ToList();
        for (int i = 0; i < baseNodes.Count; i++)
        {
            var connections = containerCache.nodeLinks.Where(x => x.thisNodeGuid == baseNodes[i].guid).ToList();

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
        
        targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (var nodeData in containerCache.baseNodesData)
        {
            switch (nodeData.nodeType)
            {
                case NodeType.StartNode:
                    var startNode = new BaseNode(nodeData.nodeType, targetGraphView, nodeData.position, nodeData.guid);
                    targetGraphView.AddElement(startNode);
                    break;
                case NodeType.DialogueNode:
                    DialogueNodeData dialogueData = containerCache.dialogueNodesData.Find(x => x.guid == nodeData.guid);
                    var dialogueNode = new DialogueNode(targetGraphView, nodeData, dialogueData);
                    targetGraphView.AddElement(dialogueNode);
                    break;
                case NodeType.ChoiceNode:
                    ChoiceNodeData choiceData = containerCache.choiceNodesData.Find(x => x.guid == nodeData.guid);
                    var nodePorts = containerCache.nodeLinks.Where(x => x.thisNodeGuid == nodeData.guid).ToList();
                    var choiceNode = new ChoiceNode(targetGraphView, nodeData, choiceData, nodePorts);
                    targetGraphView.AddElement(choiceNode);
                    break;
                case NodeType.EndNode:
                    var endNode = new BaseNode(nodeData.nodeType, targetGraphView, nodeData.position, nodeData.guid);
                    targetGraphView.AddElement(endNode);
                    break;
            }
        }
    }

    private void ClearGraph()
    {
        edges?.ToList().ForEach(edge=>targetGraphView.RemoveElement(edge));
        nodes?.ToList().ForEach(node=> targetGraphView.RemoveElement(node));
    }
}