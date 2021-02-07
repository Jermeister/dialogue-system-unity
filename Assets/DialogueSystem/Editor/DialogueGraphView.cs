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
    public EnumField typeEnum;
    
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

        typeEnum = new EnumField(new BlackboardType());
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
        var node = CreateNewNode(_nodeType, _position);
        AddElement(node);
    }


    private BaseNode CreateNewNode(NodeType _nodeType, Vector2 _position)
    {
        switch (_nodeType)
        {
            case NodeType.StartNode:
                return new BaseNode(_nodeType, this);
            case NodeType.DialogueNode:
                AddPropertyToBlackboard();
                AddPropertyToBlackboard();
                AddPropertyToBlackboard();
                return new DialogueNode(_position, this);
            case NodeType.ChoiceNode:
                return new ChoiceNode(_position, this);
            case NodeType.EndNode:
                return new BaseNode(_nodeType, this);
            default:
                return null;
        }
    }

    public void AddPropertyToBlackboard()
    {
        switch ((BlackboardType)typeEnum.value)
        {
            case BlackboardType.Character:
                string propertyName = "newCharacter";
                CheckPropertyNameAvailability(ref propertyName);

                var characterProperty = new CharacterProperty(propertyName, "New Value", this);
                
                blackboard.Add(characterProperty.propertyElement);
                exposedProperties.Add(characterProperty);
                break;
            default:
                Debug.Log("BlackboardType.Random not finished");
                break;
        }
    }

    public void CheckPropertyNameAvailability(ref string propertyName)
    {
        // Find duplicate names and count them
        int tempCounter = 0;
        string tempName = propertyName;
        while (exposedProperties.Any(x => x.PropertyName == tempName))
        {
            tempCounter++;
            tempName = $"{propertyName}_{tempCounter}";
        }
        
        if (tempCounter > 0)
            propertyName = $"{propertyName}_{tempCounter}";
        
        Debug.Log(propertyName);
    }

    void PopulateDeleteOption(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Delete", DeletePropertyFromBlackboard, DropdownMenuAction.AlwaysEnabled, ((BlackboardField)evt.target).text);
    }
    
    public void RemovePropertyFromBlackboard(string propertyName)
    {
        var propertyToRemove = exposedProperties.Find(prop => prop.PropertyName == propertyName);
        exposedProperties.Remove(propertyToRemove);
        
        PopulateBlackboardWithProperties(exposedProperties, true);
    }

    void DeletePropertyFromBlackboard(DropdownMenuAction dropdownMenuAction)
    {
        if (dropdownMenuAction.name == "Delete")
        {
            RemovePropertyFromBlackboard(dropdownMenuAction.userData.ToString());
        }
    }

    public void PopulateBlackboardWithProperties(List<ExposedProperty> properties, bool noCheck)
    {
        blackboard.Clear();
        
        blackboard.Add(typeEnum);
        blackboard.Add(new BlackboardSection{title="Exposed properties"});
        
        //Add properties from data
        foreach (var exposedProperty in properties)
        {
            AddPropertyToBlackboard();
        }
    }

    public void ClearExposedProperties()
    {
        exposedProperties.Clear();
        blackboard.Clear();
    }
}

