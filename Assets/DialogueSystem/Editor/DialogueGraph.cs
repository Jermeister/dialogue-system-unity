using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _fileName;

    [MenuItem("Graph/Dialogue Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph Editor");
    }

    public void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateBlackBoard();
        GenerateMinimap();
    }
    

    private void GenerateBlackBoard()
    {
        var blackboard = new Blackboard(_graphView);
        blackboard.Add(new BlackboardSection{title="Exposed properties"});
        blackboard.addItemRequested = _blackboard =>
        {
            _graphView.AddPropertyToBlackboard(new ExposedProperty());
        };

        blackboard.editTextRequested = (blackboard1, element, newValue) =>
        {
            var oldPropertyName = ((BlackboardField) element).text;
            
            var tempCounter = 0;
            string tempName = newValue;
            while (_graphView.exposedProperties.Any(x => x.PropertyName == tempName))
            {
                tempCounter++;
                tempName = $"{newValue}_{tempCounter}";
            }
            
            if (tempCounter > 0)
                newValue = $"{newValue}_{tempCounter}";

            var propertyIndex = _graphView.exposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
            _graphView.exposedProperties[propertyIndex].PropertyName = newValue;

            ((BlackboardField) element).text = newValue;
        };
        
        blackboard.SetPosition(new Rect(10, 180, 200, 300));
        
        _graphView.Add(blackboard);

        _graphView.blackboard = blackboard;
    }

    private void GenerateMinimap()
    {
        var minimap = new MiniMap {anchored = true};
        
        var coords = new Rect(10, 30, 200, 140);

        minimap.SetPosition(coords);
        _graphView.Add(minimap);

        _graphView.Q<MiniMap>().zoomFactorTextChanged = delegate(string s) { 
            TestingRandomStuff();
         };
    }

    private void TestingRandomStuff()
    {
        _graphView.Q<MiniMap>().Q<Label>().text = "Hello World";
    }


    private void ConstructGraphView()
    {
        _graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph Editor"
        };
        
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name:");
        fileNameTextField.SetValueWithoutNotify("New_Dialogue");
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterCallback((EventCallback<ChangeEvent<string>>)(evt => _fileName = evt.newValue));
        
        toolbar.Add(fileNameTextField);
        
        toolbar.Add(new Button(() => RequestDataOperation(true)){text = "Save Asset"});
        toolbar.Add(new Button(() => RequestDataOperation(false)){text = "Load Asset"});
        
        rootVisualElement.Add(toolbar);
    }

    private void UpdateFileNameTextField()
    {
        try
        {
            rootVisualElement.Q<Toolbar>().Q<TextField>().value = _fileName;
        }
        catch
        {
            Debug.Log("No TextField found, could not load the object.");
        }
    }

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(_graphView);

        if (save)
        {
            saveUtility.SaveGraph(_fileName);
        }
        else
        {
            saveUtility.LoadGraph(_fileName);
            UpdateFileNameTextField();
        }
    }
    

    public void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
    
    /* OnOpenAssetAttribute has an option to provide an order index in the callback, starting at 0. 
    * This is useful if you have more than one OnOpenAssetAttribute callback, 
    * and you would like them to be called in a certain order. Callbacks are called in order, starting at zero.
    *
    * Must return true if you handled the opening of the asset or false if an external tool should open it.
    * The method with this attribute must use at least these two parameters!
    */
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        //Get the instanceID of the DialogueGraphContainer to find it in the project.
        string assetPath = AssetDatabase.GetAssetPath(instanceID);
        DialogueContainer dialogueContainer = AssetDatabase.LoadAssetAtPath<DialogueContainer>(assetPath);

        if (dialogueContainer != null)
        {
            DialogueGraph window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent($"{dialogueContainer.name} (Dialogue Graph)");
            
            //Debug.Log($"Dialogue Container name: {dialogueContainer.name}");

            //Once the window is opened, we load the content of the scriptable object.
            //Even if the new name doesn't show up in the TextField, we need to assign the _fileName
            //to load the appropriate file.
            window._fileName = dialogueContainer.name;
            window.RequestDataOperation(false);
            return true;
        }

        //If object not found, won't open anything since we need the object to draw the window.
        return false; 
    }
}