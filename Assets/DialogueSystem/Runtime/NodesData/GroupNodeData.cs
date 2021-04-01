using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CommentBlockData
{
    public List<string> childNodes = new List<string>();
    public Vector2 position;
    public string title = "Comment Block";
}