using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;


public class HUD : MenuPanel, IInitializable
{
    #region Singleton
    public static HUD Instance;
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

    public string Name { get { return "User Interface"; } }

    public TextMeshProUGUI scoreLabel, 
        bestScoreLabel, 
        gameTimerLabel, 
        progressLabel;

    [Header("Cursors")]
    public CustomCursor customCursor;
    public Texture2D pointerCursor;

    [Header("Misc")]
    public CanvasGroup HUDButtonsGroup;
    public CanvasGroup worldUI;
    public ScreenFader screenFader;
    public GameObject screenFlash;
    CanvasGroup screenFlashCanvas;

    [Header("Objectives")]
    public CanvasGroup objectivesUI;

    [Header("Message Text")]
    public CanvasGroup messageCanvasGroup;
    public TextMeshProUGUI messageText;
    public BlinkingText messageBlinkingText;
    public CanvasGroup alertMessageCanvasGroup;
    public TextMeshProUGUI alertMessageText;

    public IEnumerator Init()
    {
        Hide();
        screenFlashCanvas = screenFlash.GetComponent<CanvasGroup>();
        scoreLabel.text = "0";

        // Hide default cursor
        //Cursor.visible = false;
        //ShowPointerCursor();

        yield return new WaitForSecondsRealtime(0);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameRunning() && !GameManager.Instance.IsGamePaused())
        {
            if (GameManager.Instance.gameTimerEnabled) gameTimerLabel.text = Utils.TimeFormatHundreds(GameManager.Instance.gameTimer);
            if (PlayerData.Instance) UpdatePlayerScore();
        }
    }

    public void UpdatePlayerScore()
    {
        scoreLabel.text = PlayerData.Instance.Data.PlayerScore.ToString();
        string bestScore = PlayerData.Instance.Data.PlayerBestScore > 0 ? PlayerData.Instance.Data.PlayerBestScore.ToString() : "---";
        bestScoreLabel.text = bestScore;
    }

    public void UpdateProgress(string progressText)
    {
        progressLabel.text = progressText;
    }


    // MESSAGE
    private Sequence currentMessageSequence;
    public void ShowMessage(string message, float fadeInDuration, float displayDuration, float fadeOutDuration, bool blink = false)
    {
        messageText.text = "";
        //messageBlinkingText.blink = blink;
        if (!blink) messageText.color = Color.white;

        if (currentMessageSequence != null)
        {
            // If a message is already displaying, interrupt it and fade it out immediately
            currentMessageSequence.Kill();
            messageCanvasGroup.DOFade(0, 0.05f).OnComplete(() => DisplayNewMessage(message, fadeInDuration, displayDuration, fadeOutDuration));
        }
        else
        {
            DisplayNewMessage(message, fadeInDuration, displayDuration, fadeOutDuration);
        }
    }

    void DisplayNewMessage(string message, float fadeInDuration, float displayDuration, float fadeOutDuration)
    {
        messageText.text = message;
        currentMessageSequence = DOTween.Sequence()
            .Append(messageCanvasGroup.DOFade(1, fadeInDuration).SetEase(Ease.OutQuint))
            .AppendInterval(displayDuration)
            .Append(messageCanvasGroup.DOFade(0, fadeOutDuration).SetEase(Ease.InQuint))
            .OnComplete(() => currentMessageSequence = null);
    }

    // ALERT MESSAGE
    private Sequence currentAlertMessageSequence;
    public void ShowAlertMessage(string message, float fadeInDuration, float displayDuration, float fadeOutDuration, bool blink = false)
    {
        alertMessageText.text = "";
        if (currentAlertMessageSequence != null)
        {
            // If a message is already displaying, interrupt it and fade it out immediately
            currentAlertMessageSequence.Kill();
            alertMessageCanvasGroup.DOFade(0, 0.05f).OnComplete(() => DisplayNewAlertMessage(message, fadeInDuration, displayDuration, fadeOutDuration));
        }
        else
        {
            DisplayNewAlertMessage(message, fadeInDuration, displayDuration, fadeOutDuration);
        }
    }

    void DisplayNewAlertMessage(string message, float fadeInDuration, float displayDuration, float fadeOutDuration)
    {
        alertMessageText.text = message;
        currentAlertMessageSequence = DOTween.Sequence()
            .Append(alertMessageCanvasGroup.DOFade(1, fadeInDuration).SetEase(Ease.OutQuint))
            .AppendInterval(displayDuration)
            .Append(alertMessageCanvasGroup.DOFade(0, fadeOutDuration).SetEase(Ease.InQuint))
            .OnComplete(() => currentAlertMessageSequence = null);
    }

    public void ScreenFlash()
    {
        StartCoroutine(StartScreenFlash());
    }

    IEnumerator StartScreenFlash()
    {
        screenFlashCanvas.alpha = 1;
        yield return new WaitForSeconds(0.1f);
        screenFlashCanvas.DOFade(0, 0.5f);
        yield return new WaitForSeconds(0.5f);
        screenFlashCanvas.alpha = 0;
    }

    public IEnumerator TextPop(TextMeshProUGUI text)
    {
        var normalScale = new Vector3(1, 1, 1);
        text.rectTransform.localScale = normalScale;

        var scale = new Vector3(1.35f, 1.35f, 1);
        text.rectTransform.DOScale(scale, 0.02f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.02f);

        text.rectTransform.DOScale(normalScale, 0.2f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.2f);
    }

    //public void ShowPointerCursor()
    //{
    //    Cursor.visible = false;
    //    customCursor.cursorTexture = pointerCursor;
    //    customCursor.hotspot = Vector2.zero;
    //}

}
