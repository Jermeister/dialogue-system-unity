using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueOptions : MonoBehaviour
{
    [Header("Serialized Fields")] 
    public List<GameObject> optionObjects;
    public List<TMP_Text> optionTexts;

    public TMP_Text dialogueText;
    public TMP_Text speakerNameText;

    public void SetupDialogue(string speakerName, string dialogue, List<NodeLinkData> options)
    {
        dialogueText.text = dialogue;
        speakerNameText.text = speakerName;
        
        SetupOptions(options);

        gameObject.SetActive(true);
    }

    private void SetupOptions(List<NodeLinkData> options)
    {
        int count = options.Count;
        
        for (int i = 0; i < optionObjects.Count; i++)
        {
            if (count > 0)
            {
                optionObjects[i].SetActive(true);
                optionTexts[i].text = options[i].portName;
            }
            else
            {
                optionObjects[i].SetActive(false);
            }
            
            count--;
        }
    }
    
    public void NextDialogue(int index)
    {
        gameObject.SetActive(false);
        Debug.Log($"Selected option: {index}");
        DialoguesManager.instance.Next(index);
    }
}