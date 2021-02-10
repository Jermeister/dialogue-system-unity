using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class DialogueNode : BaseNode
{
    public List<TextField> dialogueTexts;
    public PopupField<ExposedProperty> characterDropdown;
    private List<Button> deleteButtons;

    public DialogueNode(Vector3 _position, DialogueGraphView _graphView)
    {
        graphView = _graphView;
        
        nodeName = "Dialogue Node";
        title = nodeName;
        guid = Guid.NewGuid().ToString();
        nodeType = NodeType.DialogueNode;
        inputPoint = true;
        outputPoint = true;
        
        dialogueTexts = new List<TextField>();
        deleteButtons = new List<Button>();

        styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        // Generate Input Port
        var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);

        characterDropdown = new PopupField<ExposedProperty>(graphView.exposedProperties, graphView.exposedProperties[0]);
        inputContainer.Add(characterDropdown);

        // Generate Output Port
        var outputPort = GeneratePort(Direction.Output, Port.Capacity.Single);
        outputPort.portName = "Output";
        outputContainer.Add(outputPort);
        
        RefreshExpandedState();
        RefreshPorts();
        SetPosition(new Rect(_position, defaultNodeSize));

        var button = new Button(AddTextField) {text = "Add Text"};
        mainContainer.Add(button);
        
        AddTextField();
    }

    private void AddTextField()
    {
        var textField = new TextField(string.Empty);
        textField.value = "Insert text..";

        textField.RegisterValueChangedCallback(evt =>
        {
            int index = dialogueTexts.FindIndex(x => x == (TextField) evt.currentTarget);
            dialogueTexts[index].value = evt.newValue;
        });
        
        var deleteButton = new Button()
        {
            text = "X",
        };

        deleteButton.clicked += () =>
        {
            int index = deleteButtons.FindIndex(x => x == deleteButton );
            RemoveTextField(index);
        };

        dialogueTexts.Add(textField);
        deleteButtons.Add(deleteButton);
        mainContainer.Add(textField);
        mainContainer.Add(deleteButton);
        
        RefreshExpandedState();
        RefreshPorts();
    }
    
    private void RemoveTextField(int index)
    {
        mainContainer.Remove(deleteButtons[index]);
        mainContainer.Remove(dialogueTexts[index]);
        
        dialogueTexts.RemoveAt(index);
        deleteButtons.RemoveAt(index);
        
        RefreshPorts();
        RefreshExpandedState();
    }
}
