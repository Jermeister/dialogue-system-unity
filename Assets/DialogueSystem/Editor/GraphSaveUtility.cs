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
    private DialogueContainer _containerCache;

    private List<Edge> edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();
    
    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        // No connections, don't save anything
        if (!edges.Any()) return;

        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

        var connectedPorts = edges.Where(x => x.input.node != null).ToArray();

        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;
            
            dialogueContainer.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGuid = outputNode.guid,
                portName = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.guid
            });
        }

        foreach (var dialogueNode in nodes.Where(node => !node.entryPoint))
        {
            dialogueContainer.dialogueNodeData.Add(new DialogNodeData
            {
                guid = dialogueNode.guid,
                dialogueName = dialogueNode.nodeName,
                dialogueText = dialogueNode.nodeText,
                position = dialogueNode.GetPosition().position,
            });
        }

        if (!Directory.Exists("Assets/Resources"))
            Directory.CreateDirectory("Assets/Resources");
        
        if (!Directory.Exists("Assets/Resources/Dialogues"))
            Directory.CreateDirectory("Assets/Resources/Dialogues");
        
        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/Dialogues/{fileName}.asset");
        AssetDatabase.SaveAssets();

    }

    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<DialogueContainer>($"Dialogues/{fileName}");

        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File not found!", "Target dialogue graph file does not exist.", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
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
                
                targetNode.SetPosition(new Rect(_containerCache.dialogueNodeData.First(x => x.guid == targetNodeGuid).position, _targetGraphView.defaultNodeSize));
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
        foreach (var nodeData in _containerCache.dialogueNodeData)
        {
            var tempNode = _targetGraphView.CreateDialogueNode(nodeData.dialogueName, nodeData.dialogueText, Vector2.zero);
            tempNode.guid = nodeData.guid;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodeData.guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }

    private void ClearGraph()
    {
        // Set entry points guid back from the save, discard existing guid.
        nodes.Find(x => x.entryPoint).guid = _containerCache.nodeLinks[0].baseNodeGuid;

        foreach (var node in nodes)
        {
            if (node.entryPoint) continue;

            // Remove connections associated with the node
            edges.Where(x=>x.input.node == node).ToList().ForEach(edge=>_targetGraphView.RemoveElement(edge));
            
            // Remove the node
            _targetGraphView.RemoveElement(node);
        }
    }
}
