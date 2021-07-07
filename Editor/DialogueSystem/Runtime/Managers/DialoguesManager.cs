using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguesManager : MonoBehaviour
{
    public static DialoguesManager instance
    {
        get
        {
            if (_instance == null)
                _instance = new DialoguesManager();
            return _instance;
        }
    }

    public DialoguesManager() { }

    private static DialoguesManager _instance;

    [Header("Serialized Objects")]
    [SerializeField] private List<DialogueContainer> dialoguesContainer;
    [SerializeField] private DialogueSingle dialogueSingle;
    [SerializeField] private DialogueOptions dialogueOptions;

    private BaseNodeData currentNode;
    private NodeLinkData currentNodeLink;
    private DialogueContainer currentContainer;

    private void Awake()
    {
        _instance = this;
    }

    public void PlayDialogue(string dialogueName)
    {
        var dialogueIndex =  dialoguesContainer.FindIndex(x => x.dialogueName.Equals(dialogueName));
        Debug.Log($"Playing Dialogue {dialogueName} ({dialogueIndex})");
        
        if (dialogueIndex == -1)
            return;
        
        currentContainer = dialoguesContainer[dialogueIndex];
        currentNode = currentContainer.nodesContainer.baseNodesData.Find(x => x.nodeType == NodeType.StartNode);
        currentNodeLink = currentContainer.nodesContainer.nodeLinks.Find(x => x.thisNodeGuid == currentNode.guid);
        Debug.Log($"Current Node: {currentNode.nodeType} {currentNode.guid}");

        Next();
    }

    public void Next(int selectedId = -1)
    {
        Debug.Log("Going next..");
        if (currentNodeLink.nextNodeGuid == null)
        {
            Debug.Log("Dialogue Ended");
            return;
        }

        // If pressing Next on DialogueSingle (no multiple options - one outcome)
        if (selectedId == -1)
        {
            currentNode = currentContainer.nodesContainer.baseNodesData.Find(x => x.guid == currentNodeLink.nextNodeGuid);
            currentNodeLink = currentContainer.nodesContainer.nodeLinks.Find(x => x.thisNodeGuid == currentNode.guid);
        }
        else // If pressing Next on DialogueOptions (need to find a correspondent option)
        {
            var allNextLinks = currentContainer.nodesContainer.nodeLinks.FindAll(x => x.thisNodeGuid == currentNodeLink.thisNodeGuid);
            currentNodeLink = currentContainer.nodesContainer.nodeLinks.Find(x => x.thisNodeGuid == allNextLinks[selectedId].nextNodeGuid);
            currentNode = currentContainer.nodesContainer.baseNodesData.Find(x => x.guid == currentNodeLink.thisNodeGuid);
        }
        
        Debug.Log($"Current Node: {currentNode.nodeType} {currentNode.guid}");
        
        switch (currentNode.nodeType)
        {
            case NodeType.ChoiceNode:
                var currentNodeDataChoice = currentContainer.nodesContainer.choiceNodesData.Find(x => x.guid == currentNode.guid);
                var allNextLinks = currentContainer.nodesContainer.nodeLinks.FindAll(x => x.thisNodeGuid == currentNodeLink.thisNodeGuid);
                dialogueOptions.SetupDialogue(currentNodeDataChoice.speaker, currentNodeDataChoice.dialogueText, allNextLinks);
                break;
            case NodeType.DialogueNode:
                var currentNodeDataDialogue = currentContainer.nodesContainer.dialogueNodesData.Find(x => x.guid == currentNode.guid);
                dialogueSingle.SetupDialogue(currentNodeDataDialogue.speaker, currentNodeDataDialogue.dialogueTexts);
                break;
            case NodeType.EndNode:
                Debug.Log("Dialogue Ended");
                break;
            case NodeType.StartNode:
                Debug.Log("Dialogue Started, but how..");
                break;
        }
    }
}

[System.Serializable]
public struct DialogueContainer
{
    public string dialogueName;
    public NodesContainer nodesContainer;
}
