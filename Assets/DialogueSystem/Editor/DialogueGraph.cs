using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        GenerateMinimap();
        GenerateBlackBoard();
    }

    private void GenerateBlackBoard()
    {
        var blackboard = new Blackboard(_graphView);
        blackboard.Add(new BlackboardSection{title="Properties"});
        blackboard.addItemRequested = _blackboard =>
        {
            _graphView.AddPropertyToBlackboard();
        };
        
        blackboard.SetPosition(new Rect(10, 30, 200, 300));
        
        _graphView.Add(blackboard);
    }

    private void GenerateMinimap()
    {
        var minimap = new MiniMap {anchored = true};
        
        // 10 px offset from the left
        var coords = _graphView.contentViewContainer.WorldToLocal(new Vector2(maxSize.x - 10, 30));
        
        minimap.SetPosition((new Rect(coords.x, coords.y, 200, 140)));
        _graphView.Add(minimap);
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

        var fileNameTextField = new TextField("File Name: ");
        fileNameTextField.SetValueWithoutNotify("New Dialogue");
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterCallback((EventCallback<ChangeEvent<string>>)(evt => _fileName = evt.newValue));
        
        toolbar.Add(fileNameTextField);
        
        toolbar.Add(new Button(() => RequestDataOperation(true)){text = "Save Asset"});
        toolbar.Add(new Button(() => RequestDataOperation(false)){text = "Load Asset"});
        
        rootVisualElement.Add(toolbar);
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
        }
    }
    

    public void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
}
