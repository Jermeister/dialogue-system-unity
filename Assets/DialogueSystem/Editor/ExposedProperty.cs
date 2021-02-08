using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public abstract class ExposedProperty
{
    public string PropertyName = "New String";
    public string PropertyValue = "New Value";
    public BlackboardType propertyType;

    protected DialogueGraphView graphView;
    public VisualElement propertyElement;

    public ExposedProperty() { }
    
    protected void PopulateDeleteOption(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Delete", DeletePropertyFromBlackboard, DropdownMenuAction.AlwaysEnabled, ((BlackboardField)evt.target).text);
    }
    
    protected void DeletePropertyFromBlackboard(DropdownMenuAction dropdownMenuAction)
    {
        if (dropdownMenuAction.name == "Delete")
        {
            graphView.RemovePropertyFromBlackboard(dropdownMenuAction.userData.ToString());
        }
    }
}


public class CharacterProperty : ExposedProperty
{
    public CharacterProperty(string propertyName, string propertyValue, DialogueGraphView dialogueGraphView)
    {
        graphView = dialogueGraphView;
        propertyType = BlackboardType.Character;
        PropertyName = propertyName;
        PropertyValue = propertyValue;
        
        propertyElement = new VisualElement();
        var blackboardField = new BlackboardField{text = propertyName, typeText = "Character"};

        blackboardField.Q<Label>("typeLabel").style.flexBasis = StyleKeyword.Auto;
        blackboardField.capabilities &= ~Capabilities.Deletable;
        
        blackboardField.RegisterCallback<ContextualMenuPopulateEvent>(PopulateDeleteOption);
        blackboardField.Add(new Button(() => { graphView.RemovePropertyFromBlackboard(this.PropertyName); }) { text = "X" });
        
        propertyElement.Add(blackboardField);

        var propertyValueTextField = new TextField("Value:")
        {
            value = PropertyValue
        };
        propertyValueTextField.Q<Label>().style.minWidth = StyleKeyword.Auto;

        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var changingPropertyIndex = graphView.exposedProperties.FindIndex(x => x.PropertyName == this.PropertyName);
            
            if (changingPropertyIndex < 0)
                return;
            
            graphView.exposedProperties[changingPropertyIndex].PropertyValue = evt.newValue;
            propertyValueTextField.value = evt.newValue;
        });
        
        var blackboardValueRow = new BlackboardRow(blackboardField, propertyValueTextField);
        propertyElement.Add(blackboardValueRow);
    }
    
    public override string ToString()
    {
        return PropertyName;
    }
}

[System.Serializable]
public enum BlackboardType
{
    Character,
}
