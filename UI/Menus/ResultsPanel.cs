using UnityEngine;
using System.Linq;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;


public class ResultsPanel : MenuPanel
{
    public GameObject ReplayButtonObject;
    public GameObject BackButtonObject;

    public MenuPanel scoreCardPanel, leaderboardPanel;

    [Header("Labels")]
    public TextMeshProUGUI newBestNotification;
    public TextMeshProUGUI 
        highestRoundLabel, 
        accuracyLabel,
        scoreThisRunLabel,
        bestScoreLabel;

    public override void Show(float fadeDuration = 0.2f, bool setActivePanel = true)
    {
        newBestNotification.enabled = false;
        BackButtonObject.SetActive(false);
        ReplayButtonObject.SetActive(false);

        base.Show(fadeDuration);
    }

    public IEnumerator ShowResults()
    {
        scoreCardPanel.Show(0.4f);

        // Update score labels
        scoreThisRunLabel.text = Utils.FormatNumberWithCommas(PlayerData.Instance.Data.PlayerScore);
        bestScoreLabel.text = Utils.FormatNumberWithCommas(PlayerData.Instance.Data.PlayerBestScore);
        newBestNotification.enabled = PlayerData.Instance.Data.newHighScore;

        // Update highest round label
        if (highestRoundLabel != null)
        {
            highestRoundLabel.text = PlayerData.Instance.Data.LastRoundCompleted;
        }

        // Update accuracy label (if we have accuracy data)
        if (accuracyLabel != null && Scoring.Instance != null)
        {
            // For now, show a placeholder or calculate accuracy if we have the data
            accuracyLabel.text = "N/A"; // Placeholder until accuracy tracking is implemented
        }

        yield return new WaitForSecondsRealtime(2f);

        ShowLeaderboard();

        ReplayButtonObject.SetActive(true);
    }

    public void ShowLeaderboard(bool showBackButton = false)
    {
        LeaderboardController.Instance.CreateRows();
        LeaderboardController.Instance.ScrollToRow(LeaderboardController.Instance.currentUserIndex);
        leaderboardPanel.Show(0.4f);

        if (showBackButton) BackButtonObject.SetActive(true);
    }

    // Callbacks
    public void OnReplayButtonClick()
    {
        GameManager.Instance.ReplayGame();
        Hide();
    }

    public void OnBackButtonClick()
    {
        Menu.Instance.TitleScreenPanel.Show();
        Hide();

    }

    public override void Hide(float fadeDuration = 0.1f)
    {
        base.Hide(fadeDuration);
        ReplayButtonObject.SetActive(false);
        BackButtonObject.SetActive(false);

        scoreCardPanel.Hide();
        leaderboardPanel.Hide();
    }

}
