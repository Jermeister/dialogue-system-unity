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
        
        Initialize("Dialogue Node", Guid.NewGuid().ToString());
        SetupPorts();

        SetupCharacterSelection(0);

        RefreshExpandedState();
        RefreshPorts();
        SetPosition(new Rect(_position, defaultNodeSize));
        
        AddTextField();
    }

    public DialogueNode(DialogueGraphView _graphView, BaseNodeData baseData, DialogueNodeData dialogueData)
    {
        graphView = _graphView;
        
        Initialize(baseData.nodeName, baseData.guid);
        SetupPorts();

        var propertyIndex = _graphView.exposedProperties.FindIndex(x => x.PropertyName == dialogueData.speaker);
        SetupCharacterSelection(propertyIndex);
        
        SetPosition(new Rect(baseData.position, defaultNodeSize));
        RefreshExpandedState();
        RefreshPorts();
        

        foreach (var text in dialogueData.dialogueTexts)
        {
            AddTextField(text);
        }
    }

    public new DialogueNodeData CopyData(bool keepGuid = true)
    {
        var textList = dialogueTexts.Select(textObject => textObject.text).ToList();
        
        var data = new DialogueNodeData
        {
            guid = keepGuid ? guid : Guid.NewGuid().ToString(),
            speaker = characterDropdown.value.PropertyName,
            dialogueTexts = textList,
        };
        
        return data;
    }

    private void Initialize(string nName = "Dialogue Node", string nodeGuid = null)
    {
        nodeName = nName;
        title = nodeName;
        guid = nodeGuid;
        nodeType = NodeType.DialogueNode;
        
        dialogueTexts = new List<TextField>();
        deleteButtons = new List<Button>();

        styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        
        var button = new Button(AddTextField) {text = "Add Text"};
        mainContainer.Add(button);
    }

    private void SetupPorts()
    {
        inputPoint = true;
        outputPoint = true;
        
        // Generate Input Port
        var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);
        
        // Generate Output Port
        var outputPort = GeneratePort(Direction.Output, Port.Capacity.Single);
        outputPort.portName = "Output";
        outputContainer.Add(outputPort);
    }

    private void SetupCharacterSelection(int propertyIndex)
    {
        characterDropdown = new PopupField<ExposedProperty>(graphView.exposedProperties, graphView.exposedProperties[propertyIndex]);
        inputContainer.Add(characterDropdown);
    }
    
    private void AddTextField()
    {
        AddTextField("Insert text..");
    }
    
    private void AddTextField(string textFieldText)
    {
        var textField = new TextField(string.Empty);
        textField.value = textFieldText;

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
