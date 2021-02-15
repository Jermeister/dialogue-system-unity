using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueSingle : MonoBehaviour
{
    [Header("Serialized Fields")]
    public TMP_Text dialogueText;
    public TMP_Text speakerNameText;

    private List<string> dialogueTexts;
    private int currentIndex = 0;

    public void SetupDialogue(string speakerName, List<string> dialogues)
    {
        currentIndex = 0;
        dialogueTexts = dialogues;
        dialogueText.text = dialogueTexts[currentIndex];
        speakerNameText.text = speakerName;

        gameObject.SetActive(true);
    }

    public void NextDialogue()
    {
        currentIndex++;

        if (currentIndex >= dialogueTexts.Count)
        {
            gameObject.SetActive(false);
            DialoguesManager.instance.Next();
        }
        else
        {
            dialogueText.text = dialogueTexts[currentIndex];
        }
    }
}
