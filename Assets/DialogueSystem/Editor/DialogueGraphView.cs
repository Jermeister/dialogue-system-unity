using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(200,150);

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

    private BaseNode GenerateEntryPoint()
    {
        var node = new BaseNode(NodeType.StartNode, this);
        return node;
    }
    
    public void CreateNode(NodeType _nodeType, Vector2 _position = new Vector2())
    {
        AddElement(CreateDialogueNode(_nodeType, _position));
    }


    private BaseNode CreateDialogueNode(NodeType _nodeType, Vector2 _position)
    {
        switch (_nodeType)
        {
            case NodeType.StartNode:
                return new BaseNode(_nodeType, this);
            case NodeType.DialogueNode:
                return new DialogueNode(_position, this);
            case NodeType.ChoiceNode:
                return new ChoiceNode(_position, this);
            case NodeType.EndNode:
                return new BaseNode(_nodeType, this);
            default:
                return null;
        }
    }

    public void AddPropertyToBlackboard(ExposedProperty exposedProperty, bool noCheck = false)
    {
        // Save local / temp values
        var localName = exposedProperty.PropertyName;
        var localValue = exposedProperty.PropertyValue;

        if (!noCheck)
        {
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
        }
        
        var container = new VisualElement();
        var blackboardField = new BlackboardField{text = localName, typeText = "string"};
        blackboardField.Q<Label>("typeLabel").style.flexBasis = StyleKeyword.Auto;
        blackboardField.capabilities &= ~Capabilities.Deletable;
        
        blackboardField.RegisterCallback<ContextualMenuPopulateEvent>(PopulateDeleteOption);
        blackboardField.Add(new Button(() => { RemovePropertyFromBlackboard(localName); }) { text = "X" });
        
        container.Add(blackboardField);

        var propertyValueTextField = new TextField("Value:")
        {
            value = localValue
        };
        propertyValueTextField.Q<Label>().style.minWidth = StyleKeyword.Auto;

        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var changingPropertyIndex = exposedProperties.FindIndex(x => x.PropertyName == localName);
            exposedProperties[changingPropertyIndex].PropertyName = evt.newValue;
        });
        
        var blackboardValueRow = new BlackboardRow(blackboardField, propertyValueTextField);
        container.Add(blackboardValueRow);
        
        blackboard.Add(container);
    }


    void PopulateDeleteOption(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Delete", DeletePropertyFromBlackboard, DropdownMenuAction.AlwaysEnabled, ((BlackboardField)evt.target).text);
    }
    
    private void RemovePropertyFromBlackboard(string propertyName)
    {
        var propertyToRemove = exposedProperties.Find(prop => prop.PropertyName == propertyName);
        exposedProperties.Remove(propertyToRemove);
 
        blackboard.Clear();
        
        //Add properties from data
        foreach (var exposedProperty in exposedProperties)
        {
            AddPropertyToBlackboard(exposedProperty, true);
        }
    }

    void DeletePropertyFromBlackboard(DropdownMenuAction dropdownMenuAction)
    {
        if (dropdownMenuAction.name == "Delete")
        {
            RemovePropertyFromBlackboard(dropdownMenuAction.userData.ToString());
        }
    }

    public void ClearExposedProperties()
    {
        exposedProperties.Clear();
        blackboard.Clear();
    }
}
