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
    private DialogueGraphView graphView;
    private string fileName;


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

        EnableCopyAndPasteOperation();
        DisableDifferentPortConnections();
    }
    

    private void GenerateBlackBoard()
    {
        var blackboard = new Blackboard(graphView);
        blackboard.scrollable = true;
   
        blackboard.Add(graphView.typeEnum);
        blackboard.Add(new BlackboardSection{title="Exposed properties"});
        blackboard.addItemRequested = _blackboard =>
        {
            graphView.AddPropertyToBlackboard();
        };

        blackboard.editTextRequested = (bb, element, newValue) =>
        {
            var oldPropertyName = ((BlackboardField) element).text;

            if (newValue == null)
                return;

            graphView.CheckPropertyNameAvailability(ref newValue);

            var propertyIndex = graphView.exposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
            graphView.exposedProperties[propertyIndex].PropertyName = newValue;
            ((BlackboardField) element).text = newValue;
        };
        
        blackboard.SetPosition(new Rect(10, 180, 240, 300));
        
        graphView.Add(blackboard);
        graphView.blackboard = blackboard;
        
        graphView.AddPropertyToBlackboard();
    }

    private void GenerateMinimap()
    {
        var minimap = new MiniMap {anchored = true};
        
        var coords = new Rect(10, 30, 200, 140);

        minimap.SetPosition(coords);
        graphView.Add(minimap);

        graphView.Q<MiniMap>().zoomFactorTextChanged = delegate(string s) { 
            TestingRandomStuff();
         };
    }

    private void TestingRandomStuff()
    {
        graphView.Q<MiniMap>().Q<Label>().text = "";
    }


    private void ConstructGraphView()
    {
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph Editor"
        };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void DisableDifferentPortConnections()
    {
        graphView.graphViewChanged += x =>
        {
            if (x.edgesToCreate != null)
            {
                var tempList = new List<Edge>(x.edgesToCreate);
                foreach (var edge in tempList)
                {
                    if (edge.input.portType != edge.output.portType)
                    {
                        x.edgesToCreate.Remove(edge);
                    }
                }
            }
            return default;
        }; 
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name:");
        fileNameTextField.SetValueWithoutNotify("New_Dialogue");
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterCallback((EventCallback<ChangeEvent<string>>)(evt => fileName = evt.newValue));
        
        toolbar.Add(fileNameTextField);
        
        toolbar.Add(new Button(() => RequestDataOperation(true)){text = "Save Asset"});
        toolbar.Add(new Button(() => RequestDataOperation(false)){text = "Load Asset"});
        
        rootVisualElement.Add(toolbar);
    }

    private void UpdateFileNameTextField()
    {
        try
        {
            rootVisualElement.Q<Toolbar>().Q<TextField>().value = fileName;
        }
        catch
        {
            Debug.Log("No TextField found, could not load the object.");
        }
    }

    private void EnableCopyAndPasteOperation()
    {
        var graphCxtMenuUtil = GraphContextMenuUtility.GetInstance(graphView);
        graphView.serializeGraphElements += graphCxtMenuUtil.OnCopyElementsOption;
        graphView.canPasteSerializedData += graphCxtMenuUtil.OnPasteValidation;
        graphView.unserializeAndPaste += graphCxtMenuUtil.OnPasteElementsOption;
    }

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(graphView);

        if (save)
        {
            saveUtility.SaveGraph(fileName);
        }
        else
        {
            saveUtility.LoadGraph(fileName);
            UpdateFileNameTextField();
        }
    }
    

    public void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }
    

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        //Get the instanceID of the DialogueGraphContainer to find it in the project.
        string assetPath = AssetDatabase.GetAssetPath(instanceID);
        NodesContainer nodesContainer = AssetDatabase.LoadAssetAtPath<NodesContainer>(assetPath);

        if (nodesContainer != null)
        {
            DialogueGraph window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent($"{nodesContainer.name} (Dialogue Graph)");
            
            window.fileName = nodesContainer.name;
            window.RequestDataOperation(false);
            return true;
        }
        
        return false; 
    }
}
