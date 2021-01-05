using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueNode : BaseNode
{
    public List<TextField> dialogueTexts;
    public string speaker;

    private List<Button> deleteButtons;

    public DialogueNode(Vector3 _position, GraphView _graphView)
    {
        graphView = _graphView;
        
        nodeName = "Dialogue Node";
        title = nodeName;
        guid = Guid.NewGuid().ToString();
        inputPoint = true;
        outputPoint = true;
        
        dialogueTexts = new List<TextField>();
        deleteButtons = new List<Button>();

        styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        // Generate Input Port
        var inputPort = GeneratePortOfType(Direction.Input, typeof(FlowEdge), Port.Capacity.Multi);
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);
        
        // Generate Input Port for Character
        var inputCharacterPort = GeneratePortOfType(Direction.Input, typeof(DialogueCharacter));
        inputCharacterPort.portName = "Character";
        inputContainer.Add(inputCharacterPort);
        
        // Generate Output Port
        var outputPort = GeneratePortOfType(Direction.Output, typeof(FlowEdge));
        outputPort.portName = "Output";
        outputContainer.Add(outputPort);
        
        RefreshExpandedState();
        RefreshPorts();
        SetPosition(new Rect(_position, defaultNodeSize));
        
        var button = new Button(() => { AddTextField(); });
        button.text = "Add Text";
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
