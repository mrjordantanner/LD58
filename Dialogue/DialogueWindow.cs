using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Loading;

public class DialogueWindow : MonoBehaviour
{
    public Image speakerImage;
    public TextMeshProUGUI speakerNameLabel;
    public TextMeshProUGUI dialogueText;
    //public Button continueButton;

    private void Awake()
    {
        // Flatten the box on startup
        //gameObject.transform.DOScaleY(0f, 0f);

        speakerNameLabel.text = "";
        dialogueText.text = "";

        //if (!continueButton) continueButton = GetComponentInChildren<Button>();
        //ShowContinueButton(false);
    }

    //public void ShowContinueButton(bool value)
    //{
    //    continueButton.gameObject.SetActive(value);
    //}

    // Callback
    public void DialogueWindowOnClick()
    {
        if (DialogueManager.Instance.isTyping)
        {
            DialogueManager.Instance.InterruptDialogue();
        }
        else
        {
            DialogueManager.Instance.ContinueButtonOnClick();
        }
    }
}

