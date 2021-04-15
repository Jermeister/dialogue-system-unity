using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueDemo : MonoBehaviour
{
    void Start()
    {
        DialoguesManager.instance.PlayDialogue("Punny");
    }
}
