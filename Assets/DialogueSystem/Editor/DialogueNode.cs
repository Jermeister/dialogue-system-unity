using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class DialogueNode : Node
{
    public string guid;
    public string nodeName;
    public string nodeText;
    public bool entryPoint = false;
}
