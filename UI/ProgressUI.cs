using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

/// <summary>
/// Dedicated UI component for handling progression-related display updates.
/// Subscribes to progression events and updates UI elements accordingly.
/// </summary>
public class ProgressUI : MonoBehaviour, IInitializable
{
    #region Singleton
    public static ProgressUI Instance;
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

    #region Declarations
    public string Name { get { return "Progress UI"; } }

    [Header("UI References")]
    public TextMeshProUGUI progressLabel;
    
    [Header("Message Settings")]
    public float messageFadeInDuration = 0.5f;
    public float messageDisplayDuration = 2f;
    public float messageFadeOutDuration = 0.5f;
    public float alertFadeInDuration = 0.3f;
    public float alertDisplayDuration = 3f;
    public float alertFadeOutDuration = 0.5f;
    
    [Header("Progress Label Settings")]
    public float progressFadeInDuration = 0.5f;
    public float progressFadeOutDuration = 0.3f;
    public float roundFailedDisplayDuration = 1.5f;
    
    // Tween management
    private Tween progressFadeTween;
    #endregion

    #region Initialization
    public IEnumerator Init()
    {
        // Subscribe to progression events
        SubscribeToProgressionEvents();

        yield return new WaitForSecondsRealtime(0);
    }

    /// <summary>
    /// Subscribe to progression events to update the UI
    /// </summary>
    private void SubscribeToProgressionEvents()
    {
        // Subscribe to level events
        EventManager.Instance.Subscribe<Level>(EventManager.LEVEL_STARTED, OnLevelStarted);
        EventManager.Instance.Subscribe<Level>(EventManager.LEVEL_COMPLETED, OnLevelCompleted);
        //EventManager.Instance.Subscribe<Level>(EventManager.LEVEL_FAILED, OnLevelFailed);
        
        // Subscribe to round events
        EventManager.Instance.Subscribe<Round>(EventManager.ROUND_STARTED, OnRoundStarted);
        EventManager.Instance.Subscribe<Round>(EventManager.ROUND_COMPLETED, OnRoundCompleted);
        EventManager.Instance.Subscribe<Round>(EventManager.ROUND_FAILED, OnRoundFailed);
        
        Debug.Log("ProgressUI: Subscribed to progression events");
    }
    #endregion

    #region Level Event Handlers
    /// <summary>
    /// Called when a level starts
    /// </summary>
    private void OnLevelStarted(Level level)
    {
        string progressText = $"{level.levelNumber}";
        //HUD.Instance.ShowMessage(progressText, messageFadeInDuration, messageDisplayDuration, messageFadeOutDuration);
    }

    /// <summary>
    /// Called when a level is completed
    /// </summary>
    private void OnLevelCompleted(Level level)
    {
        //string progressText = $"Level {level.levelNumber} Complete!";
        UpdateProgressLabel("");
        HUD.Instance.ShowMessage($"Level {level.levelNumber} Complete!", messageFadeInDuration, messageDisplayDuration, messageFadeOutDuration);
    }

    /// <summary>
    /// Called when a level fails
    /// </summary>
    //private void OnLevelFailed(Level level)
    //{
    //    string progressText = $"Level {level.levelNumber} Failed";
    //    UpdateProgressLabel(progressText);
    //    HUD.Instance.ShowAlertMessage($"Level {level.levelNumber} Failed!", alertFadeInDuration, alertDisplayDuration, alertFadeOutDuration);
    //}
    #endregion

    #region Round Event Handlers
    /// <summary>
    /// Called when a round starts
    /// </summary>
    private void OnRoundStarted(Round round)
    {
        string progressText = $"{round.levelNumber}-{round.roundNumber}";
        UpdateProgressLabel(progressText);
        //HUD.Instance.ShowMessage(progressText, messageFadeInDuration, messageDisplayDuration, messageFadeOutDuration);
    }

    /// <summary>
    /// Called when a round is completed
    /// </summary>
    private void OnRoundCompleted(Round round)
    {
        //string progressText = $"{round.levelNumber}-{round.roundNumber}";
        UpdateProgressLabel("");
        //HUD.Instance.ShowMessage($"Round Complete!", messageFadeInDuration, messageDisplayDuration * 0.75f, messageFadeOutDuration);
    }

    /// <summary>
    /// Called when a round fails
    /// </summary>
    private void OnRoundFailed(Round round)
    {
        ShowRoundFailedText();
        //HUD.Instance.ShowAlertMessage($"Round {round.levelNumber}-{round.roundNumber} Failed!", alertFadeInDuration, alertDisplayDuration * 0.67f, alertFadeOutDuration);
    }
    #endregion

    #region UI Update Methods
    /// <summary>
    /// Update the progress label text with fade in effect
    /// </summary>
    private void UpdateProgressLabel(string progressText)
    {
        if (progressLabel != null)
        {
            // Kill any existing tween
            if (progressFadeTween != null && progressFadeTween.IsActive())
            {
                progressFadeTween.Kill();
            }
            
            // Set text and fade in
            progressLabel.text = progressText;
            progressLabel.alpha = 0f;
            
            progressFadeTween = progressLabel.DOFade(1f, progressFadeInDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// Show round failed text with fade out before reset
    /// </summary>
    private void ShowRoundFailedText()
    {
        if (progressLabel != null)
        {
            // Kill any existing tween
            if (progressFadeTween != null && progressFadeTween.IsActive())
            {
                progressFadeTween.Kill();
            }
            
            // Set empty text and fade in
            progressLabel.text = "";
            progressLabel.alpha = 0f;
            
            progressFadeTween = progressLabel.DOFade(1f, progressFadeInDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    // After display duration, fade out
                    progressFadeTween = progressLabel.DOFade(0f, progressFadeOutDuration)
                        .SetEase(Ease.InQuad)
                        .SetDelay(roundFailedDisplayDuration);
                });
        }
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// Clean up event subscriptions when ProgressUI is destroyed
    /// </summary>
    private void OnDestroy()
    {
        // Kill any active tweens
        if (progressFadeTween != null && progressFadeTween.IsActive())
        {
            progressFadeTween.Kill();
        }
        
        // Unsubscribe from progression events
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unsubscribe<Level>(EventManager.LEVEL_STARTED, OnLevelStarted);
            EventManager.Instance.Unsubscribe<Level>(EventManager.LEVEL_COMPLETED, OnLevelCompleted);
            //EventManager.Instance.Unsubscribe<Level>(EventManager.LEVEL_FAILED, OnLevelFailed);
            EventManager.Instance.Unsubscribe<Round>(EventManager.ROUND_STARTED, OnRoundStarted);
            EventManager.Instance.Unsubscribe<Round>(EventManager.ROUND_COMPLETED, OnRoundCompleted);
            EventManager.Instance.Unsubscribe<Round>(EventManager.ROUND_FAILED, OnRoundFailed);
        }
        
        Debug.Log("ProgressUI: Destroyed");
    }
    #endregion
}
