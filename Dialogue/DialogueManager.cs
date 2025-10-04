using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using DG.Tweening;


public class DialogueManager: MonoBehaviour
{
    #region Singleton
    public static DialogueManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion


    public CanvasGroup canvasGroup;

    public DialogueWindow dialogueWindow;
    private Queue<string> dialogueLineQueue = new();

    //[HideInInspector] 
    [TextArea(3, 10)]
    [ReadOnly] public string CurrentDialogueLine;

    [HideInInspector] public Dialogue CurrentDialogue;
    //[HideInInspector] public DialogueWindow dialogueWindow;

    public GameObject DialogueWindowPrefab;
    public SoundEffect speakingSound;

    [Header("Window Settings")]
    public bool isTyping;
    public float typingInterval = 0.01f;
    public float windowTransitionDuration = 0.1f;
    public Ease windowTransitionEasing = Ease.InOutSine;
    public Vector2 dialogueWindowPosition = new Vector2();

    private void Start()
    {
        dialogueWindow.gameObject.SetActive(false);
    }

    public IEnumerator TypeDialogueLine(string line)
    {
        isTyping = true;
        dialogueWindow.dialogueText.text = "";
        CurrentDialogueLine = line;
        foreach (char letter in line.ToCharArray())
        {
            dialogueWindow.dialogueText.text += letter;
            if (speakingSound) speakingSound.Play();
            yield return new WaitForSecondsRealtime(typingInterval * 0.1f);
        }

        isTyping = false;
        //dialogueWindow.ShowContinueButton(true);
    }

    public IEnumerator StartDialogue(Dialogue dialogue)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        GameManager.Instance.inputSuspended = true;
        yield return OpenNewDialogueWindow(dialogue);

        dialogueLineQueue.Clear();
        foreach (string line in dialogue.dialogueLines)
        {
            dialogueLineQueue.Enqueue(line);
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (dialogueLineQueue.Count == 0)
        {
            StartCoroutine(EndDialogue());
            return;
        }

        var line = dialogueLineQueue.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeDialogueLine(line));
    }

    // Callback
    public void ContinueButtonOnClick()
    {
        Menu.Instance.PlayClickSound();
        //dialogueWindow.ShowContinueButton(false);
        DisplayNextLine();
    }

    public void InterruptDialogue()
    {
        isTyping = false;
        StopAllCoroutines();
        dialogueWindow.dialogueText.text = CurrentDialogueLine;
        //dialogueWindow.ShowContinueButton(true);
    }


    IEnumerator OpenNewDialogueWindow(Dialogue dialogue)
    {
        // TODO clean this up
        // var NewDialogueWindow = Instantiate(DialogueWindowPrefab, (Vector3)dialogueWindowPosition, Quaternion.identity, transform);
        //dialogueWindow = NewDialogueWindow.GetComponent<DialogueWindow>();

        dialogueWindow.gameObject.SetActive(true);
        dialogueWindow.speakerNameLabel.text = dialogue.speakerName;
        //dialogueWindow.ShowContinueButton(false);

        CurrentDialogue = dialogue;

        //dialogueWindow.gameObject.transform.DOScaleY(1f, windowTransitionDuration).SetEase(windowTransitionEasing);
        yield return new WaitForSecondsRealtime(windowTransitionDuration);
    }

    IEnumerator EndDialogue()
    {
        //dialogueWindow.gameObject.transform.DOScaleY(0f, windowTransitionDuration).SetEase(windowTransitionEasing);
        yield return new WaitForSecondsRealtime(windowTransitionDuration);

        //Destroy(dialogueWindow.gameObject);
        dialogueWindow.gameObject.SetActive(false);
        //dialogueWindow = null;
        CurrentDialogue = null;
        CurrentDialogueLine = null;

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        GameManager.Instance.inputSuspended = false;

        // TODO hackish
        StartCoroutine(GameManager.Instance.OnIntroDialogueComplete());
    }

}

