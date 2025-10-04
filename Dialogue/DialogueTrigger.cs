using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    // Callback
    public void TriggerDialogue()
    {
        StartCoroutine(DialogueManager.Instance.StartDialogue(dialogue));
    }


}

