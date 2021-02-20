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
    private float letterRevealTime = 0.05f;

    private bool revealing = false;
    private Coroutine revealCoroutine = null;

    public void SetupDialogue(string speakerName, List<string> dialogues)
    {
        currentIndex = 0;
        dialogueTexts = dialogues;
        dialogueText.text = dialogueTexts[currentIndex];
        speakerNameText.text = speakerName;

        gameObject.SetActive(true);
        
        revealCoroutine = StartCoroutine(RevealText());
    }

    public void NextDialogue()
    {
        if (revealing)
        {
            StopCoroutine(revealCoroutine);
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            revealing = false;
            return;
        }

        currentIndex++;

        if (currentIndex >= dialogueTexts.Count)
        {
            gameObject.SetActive(false);
            DialoguesManager.instance.Next();
        }
        else
        {
            dialogueText.text = dialogueTexts[currentIndex];
            revealCoroutine = StartCoroutine(RevealText());
        }
    }

    private IEnumerator RevealText()
    {
        int letterCount = dialogueText.text.Length;
        revealing = true;

        for (int i = 0; i <= letterCount; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(letterRevealTime);
        }

        revealing = false;
    }
}
