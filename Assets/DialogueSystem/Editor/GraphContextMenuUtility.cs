using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphContextMenuUtility
{
    private DialogueGraphView targetGraphView;
    private List<GraphElement> nodeCopyCache = new List<GraphElement>();

    private float nodePasteOffset = 50f;
    
    public static GraphContextMenuUtility GetInstance(DialogueGraphView graphView)
    {
        return new GraphContextMenuUtility
        {
            targetGraphView = graphView
        };
    }
    
    public string OnCopyElementsOption(IEnumerable<GraphElement> elements)
    {
        if (elements == null)
        {
            nodeCopyCache.Clear();
            return "Empty";
        }

        nodeCopyCache.Clear();
        
        foreach (var element in elements)
        {
            if (element.GetType() == typeof(Edge) || element.GetType() == typeof(Group))
                continue;

            nodeCopyCache.Add(element);
        }
        
        return "Success";
    }

    public void OnPasteElementsOption(string a, string b)
    {
        if (nodeCopyCache.Count == 0)
            return;

        var graphContainer = GraphSaveUtility.GetInstance(targetGraphView).GetNodesContainer();

        foreach (var node in nodeCopyCache)
        {
            var baseNode = (BaseNode) node;
            var nodeData = baseNode.CopyData(false);

            // Offset pasted node
            nodeData.position.y += baseNode.GetPosition().height + nodePasteOffset;
            
            switch (nodeData.nodeType)
            {
                case NodeType.DialogueNode:
                    var dialogueNodeOriginal = node as DialogueNode;
                    DialogueNodeData dialogueData = dialogueNodeOriginal.CopyData();
                    var dialogueNode = new DialogueNode(targetGraphView, nodeData, dialogueData);
                    targetGraphView.AddElement(dialogueNode);
                    break;
                case NodeType.ChoiceNode:
                    var choiceNodeOriginal = node as ChoiceNode;
                    ChoiceNodeData choiceData = choiceNodeOriginal.CopyData();
                    var choicePorts = graphContainer.nodeLinks.ToList().Where(x => x.thisNodeGuid == nodeData.guid).ToList();
                    var choiceNode = new ChoiceNode(targetGraphView, nodeData, choiceData, choicePorts);
                    targetGraphView.AddElement(choiceNode);
                    break;
                case NodeType.EndNode:
                    var endNode = new BaseNode(nodeData.nodeType, targetGraphView, nodeData.position, nodeData.guid);
                    targetGraphView.AddElement(endNode);
                    break;
            }
        }
    }

    public bool OnPasteValidation(string data)
    {
        return true;
    }
    
}