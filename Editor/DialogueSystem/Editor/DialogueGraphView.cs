using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(200,150);
    public readonly Vector2 defaultCommentBlockSize = new Vector2(300, 200);

    public Blackboard blackboard;
    public EnumField typeEnum;
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

        typeEnum = new EnumField(BlackboardType.Character);
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
        if (node == null)
            return;
        AddElement(node);
    }


    private BaseNode CreateNewNode(NodeType _nodeType, Vector2 _position)
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
                return new BaseNode(_nodeType, this, _position);
            case NodeType.GroupNode:
                CreateGroupBlock(new Rect(_position, defaultCommentBlockSize));
                return null;
            default:
                return null;
        }
    }

    public void CreateGroupBlock(Rect rect, GroupNodeData groupNodeData = null)
    {
        if (groupNodeData == null)
            groupNodeData = new GroupNodeData();
        var group = new Group
        {
            autoUpdateGeometry = true,
            title = groupNodeData.title
        };
        AddElement(group);
        group.SetPosition(rect);
    }

    public void AddPropertyToBlackboard()
    {
        switch ((BlackboardType) typeEnum.value)
        {
            case BlackboardType.None:
                if (exposedProperties.Count > 0)
                    return;
                
                var noneProperty = new NoneProperty() {PropertyName = "None", propertyType = BlackboardType.None, PropertyValue = "None"};
                exposedProperties.Add(noneProperty);
                break;
            
            case BlackboardType.Character:
                var propertyName = "New_Character";
                CheckPropertyNameAvailability(ref propertyName);

                var characterProperty = new CharacterProperty(propertyName, "New Value", this);

                blackboard.Add(characterProperty.propertyElement);
                exposedProperties.Add(characterProperty);
                break;
            default:
                Debug.Log("Default should not be hit");
                break;
        }
    }

    public void AddExposedPropertiesFromData(List<ExposedPropertyData> propertiesData)
    {
        foreach (var property in propertiesData)
        {
            switch (property.propertyType)
            {
                case BlackboardType.None:
                    var noneProperty = new NoneProperty() {PropertyName = "None", propertyType = BlackboardType.None, PropertyValue = "None"};
                    exposedProperties.Add(noneProperty);
                    break;
                case BlackboardType.Character:
                    var characterProperty = new CharacterProperty(property.propertyName, property.propertyValue, this);
                    exposedProperties.Add(characterProperty);
                    break;
            }
        }

        RepaintBlackboardNoCheck();
    }

    public void RepaintBlackboardNoCheck()
    {
        blackboard.Clear();
        
        blackboard.Add(typeEnum);
        blackboard.Add(new BlackboardSection{title="Exposed properties"});
        
        foreach (var property in exposedProperties)
        {
            if (property.propertyType != BlackboardType.None)
                blackboard.Add(property.propertyElement);
        }
        
        blackboard.MarkDirtyRepaint();
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
    }

    void PopulateDeleteOption(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Delete", DeletePropertyFromBlackboard, DropdownMenuAction.AlwaysEnabled, ((BlackboardField)evt.target).text);
    }

    public void RemovePropertyFromBlackboard(string propertyName)
    {
        var propertyToRemoveId = exposedProperties.FindIndex(prop => prop.PropertyName == propertyName);
        exposedProperties.RemoveAt(propertyToRemoveId);
        
        RepaintBlackboardNoCheck();
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

