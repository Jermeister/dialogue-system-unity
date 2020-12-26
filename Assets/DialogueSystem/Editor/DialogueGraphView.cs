using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(150,200);

    public Blackboard blackboard;
    public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
    
    private NodeSearchWindow _searchWindow;

    public DialogueGraphView(EditorWindow editorWindow)
    {
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphStyleSheet"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        
        var gridBackground = new GridBackground();
        Insert(0, gridBackground);
        gridBackground.StretchToParentSize();

        AddElement(GenerateEntryPoint());
        AddSearchWindow(editorWindow);
    }

    private void AddSearchWindow(EditorWindow editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(editorWindow, this);
        
        nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort!=port && startPort.node!=port.node)
                compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    private Port GeneratePort(DialogueNode targetNode, Direction portDirection, Port.Capacity capacity=Port.Capacity.Single)
    {
        return targetNode.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }

    private DialogueNode GenerateEntryPoint()
    {
        var node = new DialogueNode
        {
            title = "Start",
            guid = Guid.NewGuid().ToString(),
            entryPoint = true
        };

        node.SetPosition(new Rect(100, 200, 100, 150));
        
        var outputPort = GeneratePort(node, Direction.Output);
        outputPort.portName = "Next";
        node.outputContainer.Add(outputPort);
        
        node.capabilities &= ~Capabilities.Deletable;
        
        node.RefreshExpandedState();
        node.RefreshPorts();
        
        return node;
    }
    
    public void CreateNode(string nodeName, string nodeText="", Vector2 position = new Vector2())
    {
        AddElement(CreateDialogueNode(nodeName, nodeText, position));
    }


    public DialogueNode CreateDialogueNode(string nodeName, string dialogueText, Vector2 position)
    {
        var node = new DialogueNode
        {
            title = nodeName,
            nodeName = nodeName,
            nodeText = dialogueText,
            guid = Guid.NewGuid().ToString(),
        };

        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);
        
        node.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        
        var button = new Button(() => { AddChoicePort(node); });
        button.text = "Add Choice";
        node.titleContainer.Add(button);
        
        var textField = new TextField(string.Empty);
        if (string.IsNullOrEmpty(dialogueText))
            textField.value = string.Empty;
        else
            textField.value = dialogueText;

        textField.RegisterValueChangedCallback(evt =>
        {
            node.nodeText = evt.newValue;
        });

        node.mainContainer.Add(textField);
        
        node.RefreshPorts();
        node.RefreshExpandedState();

        node.SetPosition(new Rect(position, defaultNodeSize));

        return node;
    }

    public void AddChoicePort(DialogueNode node, string overridePortName = "")
    {
        var generatedPort = GeneratePort(node, Direction.Output);
        
        // Removing default label
        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);
        
        generatedPort.contentContainer.Add(new Label("  "));
        
        var outputPortCount = node.outputContainer.childCount;
        var choicePortName = string.IsNullOrEmpty(overridePortName) ? $"Choice {outputPortCount}" : overridePortName;

        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
        textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.Add(textField);

        var deleteButton = new Button(()=> RemovePort(node, generatedPort))
        {
            text = "X",
        };
        generatedPort.Add(deleteButton);
        
        generatedPort.portName = choicePortName;
        node.outputContainer.Add(generatedPort);
        
        node.RefreshPorts();
        node.RefreshExpandedState();
    }

    private void RemovePort(DialogueNode node, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x =>
            x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(edge);
        }

        node.outputContainer.Remove(generatedPort);
        node.RefreshPorts();
        node.RefreshExpandedState();
    }

    public void AddPropertyToBlackboard(ExposedProperty exposedProperty)
    {
        // Save local / temp values
        var localName = exposedProperty.PropertyName;
        var localValue = exposedProperty.PropertyValue;

        // Find duplicate names and count them
        int tempCounter = 0;
        string tempName = localName;
        while (exposedProperties.Any(x => x.PropertyName == tempName))
        {
            tempCounter++;
            tempName = $"{localName}_{tempCounter}";
        }


        if (tempCounter > 0)
            localName = $"{localName}_{tempCounter}";

        
        var property = new ExposedProperty
        {
            PropertyName = localName, 
            PropertyValue = localValue
        };
        exposedProperties.Add(property);

        var container = new VisualElement();
        var blackboardField = new BlackboardField{text = localName, typeText = "string property"};
        container.Add(blackboardField);

        var propertyValueTextField = new TextField("Value:")
        {
            value = localValue
        };

        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var changingPropertyIndex = exposedProperties.FindIndex(x => x.PropertyName == property.PropertyName);
            exposedProperties[changingPropertyIndex].PropertyName = evt.newValue;
        });
        
        var blackboardValueRow = new BlackboardRow(blackboardField, propertyValueTextField);
        container.Add(blackboardValueRow);
        
        blackboard.Add(container);
    }

    public void ClearExposedProperties()
    {
        exposedProperties.Clear();
        blackboard.Clear();
    }
}
