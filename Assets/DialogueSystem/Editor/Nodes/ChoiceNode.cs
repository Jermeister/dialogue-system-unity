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
    public string speaker;

    public ChoiceNode(Vector3 _position, DialogueGraphView _graphView)
    {
        choiceTexts = new List<TextField>();
        graphView = _graphView;
        
        nodeName = "Choice Node";
        title = nodeName;
        guid = Guid.NewGuid().ToString();
        inputPoint = true;
        outputPoint = true;

        styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        // Generate Input Port
        var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);
        
        // Generate dropdown for Character
        characterDropdown = new PopupField<ExposedProperty>( _graphView.exposedProperties, 0);
        inputContainer.Add(characterDropdown);
        
        RefreshExpandedState();
        RefreshPorts();
        SetPosition(new Rect(_position, defaultNodeSize));
        
        var button = new Button(() => { AddChoicePort(); });
        button.text = "Add Choice";
        titleContainer.Add(button);
        
        dialogueTextField = new TextField(string.Empty);
        dialogueTextField.value = "Insert text..";

        dialogueTextField.RegisterValueChangedCallback(evt =>
        {
            dialogueTextField.value = evt.newValue;
        });

        mainContainer.Add(dialogueTextField);
        
        AddChoicePort();
    }
    
    private void AddChoicePort(string overridePortName = "")
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
