using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ChoiceNode : BaseNode
{
    public List<TextField> choiceTexts;
    public TextField dialogueTextField;
    public PopupField<ExposedProperty> characterDropdown;

    public ChoiceNode(Vector3 _position, DialogueGraphView _graphView)
    {
        graphView = _graphView;
        
        Initialize();
        SetupInputPort();
        
        // Generate dropdown for Character
        SetupCharacterSelection(0);

        RefreshExpandedState();
        RefreshPorts();
        SetPosition(new Rect(_position, defaultNodeSize));

        AddChoicePort();
    }

    public ChoiceNode(DialogueGraphView _graphView, BaseNodeData baseData, ChoiceNodeData choiceNodeData, List<NodeLinkData> choicePorts)
    {
        graphView = _graphView;
        
        Initialize(baseData.nodeName, baseData.guid, choiceNodeData.dialogueText);
        SetupInputPort();
        
        // Generate dropdown for Character
        var propertyIndex = _graphView.exposedProperties.FindIndex(x => x.PropertyName == choiceNodeData.speaker);
        SetupCharacterSelection(propertyIndex);

        RefreshExpandedState();
        RefreshPorts();
        SetPosition(new Rect(baseData.position, defaultNodeSize));

        foreach (var port in choicePorts)
        {
            AddChoicePort(port.portName);
        }
    }

    private void Initialize(string nName = "Choice Node", string nodeGuid = null, string dialogueText = "Insert text..")
    {
        choiceTexts = new List<TextField>();

        nodeName = nName;
        title = nodeName;
        
        if (nodeGuid != null)
            guid = nodeGuid;
        else
            guid = Guid.NewGuid().ToString();
        
        nodeType = NodeType.ChoiceNode;
        inputPoint = true;
        outputPoint = true;

        styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        
        var button = new Button(() => { AddChoicePort(); });
        button.text = "Add Choice";
        titleContainer.Add(button);
        
        dialogueTextField = new TextField(string.Empty);
        dialogueTextField.value = dialogueText;

        dialogueTextField.RegisterValueChangedCallback(evt =>
        {
            dialogueTextField.value = evt.newValue;
        });

        mainContainer.Add(dialogueTextField);
    }

    private void SetupInputPort()
    {
        // Generate Input Port
        var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);
    }
    
    private void SetupCharacterSelection(int propertyIndex)
    {
        characterDropdown = new PopupField<ExposedProperty>(graphView.exposedProperties, graphView.exposedProperties[propertyIndex]);
        inputContainer.Add(characterDropdown);
    }
    
    private void AddChoicePort(string overridePortName = null)
    {
        var generatedPort = GeneratePort(Direction.Output);

        // Removing default label
        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        oldLabel.visible = false;
        oldLabel.style.flexBasis = 0;

        var outputPortCount = outputContainer.childCount;
        var choicePortName = string.IsNullOrEmpty(overridePortName) ? $"Choice {outputPortCount}" : overridePortName;

        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
        textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        
        generatedPort.contentContainer.Add(textField);

        var deleteButton = new Button(()=> RemovePort(generatedPort))
        {
            text = "X",
        };
        
        generatedPort.contentContainer.Add(deleteButton);
        
        generatedPort.portName = choicePortName;
        generatedPort.MarkDirtyRepaint();
        
        outputContainer.Add(generatedPort);
        RefreshExpandedState();
        RefreshPorts();
    }
    
    private void RemovePort(Port generatedPort)
    {
        var targetEdge = graphView.edges.ToList().Where(x =>
            x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            graphView.RemoveElement(edge);
        }

        outputContainer.Remove(generatedPort);
        RefreshPorts();
        RefreshExpandedState();
    }
}
