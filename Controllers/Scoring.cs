using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton. Handles score calculations and leaderboard integration.
/// </summary>
public class Scoring : MonoBehaviour, IInitializable
{
    #region Singleton
    public static Scoring Instance;
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

    public string Name { get { return "Scoring"; } }

    [Header("Score Configuration")]
    [SerializeField] private int perfectLevelBonus = 1000;
    
    [Header("Reflex Bonus Configuration")]
    [SerializeField] private int highReflexBonus = 500;
    [SerializeField] private int mediumReflexBonus = 300;
    [SerializeField] private int lowReflexBonus = 100;
    [SerializeField] private float highReflexThreshold = 2f;
    [SerializeField] private float mediumReflexThreshold = 4f;
    [SerializeField] private float lowReflexThreshold = 6f;
    
    [Header("Accuracy Bonus Configuration")]
    [SerializeField] private int perfectAccuracyBonus = 1000;
    [SerializeField] private int greatAccuracyBonus = 500;
    [SerializeField] private int goodAccuracyBonus = 200;
    [SerializeField] private float perfectAccuracyThreshold = 0.95f; // 95%
    [SerializeField] private float greatAccuracyThreshold = 0.85f;   // 85%
    [SerializeField] private float goodAccuracyThreshold = 0.70f;   // 70%
    
    [Header("Current Score")]
    [ReadOnly] public int currentScore = 0;
    [ReadOnly] public int bestScore = 0;
    [ReadOnly] public int roundsCompleted = 0;
    [ReadOnly] public int levelsCompleted = 0;
    [ReadOnly] public int perfectLevels = 0;
    [ReadOnly] public float totalTimeElapsed = 0f;

    [Header("Round Tracking")]
    [ReadOnly] public float roundStartTime = 0f;
    [ReadOnly] public float roundCompletionTime = 0f;
    [ReadOnly] public bool isRoundActive = false;
    
    [Header("Level Tracking")]
    [ReadOnly] public int currentLevel = 1;
    [ReadOnly] public int roundsCompletedInCurrentLevel = 0;
    [ReadOnly] public int roundsFailedInCurrentLevel = 0;
    [ReadOnly] public bool isLevelActive = false;

    public IEnumerator Init()
    {
        // Load best score from PlayerData
        if (PlayerData.Instance != null)
        {
            bestScore = PlayerData.Instance.Data.PlayerBestScore;
            Debug.Log($"Scoring: Loaded best score from PlayerData: {bestScore}");
        }
        
        // Reset current session
        ResetCurrentSession();
        
        yield return new WaitForSecondsRealtime(0);
    }

    /// <summary>
    /// Resets the current session score and tracking
    /// </summary>
    public void ResetCurrentSession()
    {
        currentScore = 0;
        roundsCompleted = 0;
        levelsCompleted = 0;
        perfectLevels = 0;
        totalTimeElapsed = 0f;
        isRoundActive = false;
        isLevelActive = false;
        currentLevel = 1;
        roundsCompletedInCurrentLevel = 0;
        roundsFailedInCurrentLevel = 0;
        
        Debug.Log("Scoring: Current session reset");
        
        // Update HUD score labels
        UpdateHUDScoreLabels();
    }

    /// <summary>
    /// Starts tracking a new level
    /// </summary>
    public void StartLevel(int levelNumber)
    {
        currentLevel = levelNumber;
        isLevelActive = true;
        roundsCompletedInCurrentLevel = 0;
        roundsFailedInCurrentLevel = 0;
        
        Debug.Log($"Scoring: Started tracking level {levelNumber}");
    }

    /// <summary>
    /// Starts tracking a new round
    /// </summary>
    public void StartRound()
    {
        roundStartTime = Time.time;
        isRoundActive = true;
        
        Debug.Log($"Scoring: Started tracking round at time {roundStartTime:F2}");
    }

    /// <summary>
    /// Completes a round and calculates score
    /// </summary>
    /// <param name="levelNumber">Current level number</param>
    /// <param name="roundNumber">Current round number</param>
    public void CompleteRound(int levelNumber, int roundNumber)
    {
        if (!isRoundActive)
        {
            Debug.LogWarning("Scoring: CompleteRound called but no round is active");
            return;
        }

        roundCompletionTime = Time.time;
        float roundTime = roundCompletionTime - roundStartTime;
        isRoundActive = false;

        // Calculate round score (no speed rewards)
        int roundScore = CalculateRoundScore(levelNumber, roundNumber);
        
        // Add to current score
        currentScore += roundScore;
        roundsCompleted++;
        roundsCompletedInCurrentLevel++;
        
        // Update total time
        totalTimeElapsed += roundTime;
        
        // Log score change
        LogScoreChange(roundScore, $"Round {levelNumber}-{roundNumber} completed");
        
        Debug.Log($"Scoring: Round {levelNumber}-{roundNumber} completed! " +
                  $"Round Time: {roundTime:F2}s");
        
        // Check for new best score
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            UpdateBestScoreInPlayerData();
            Debug.Log($"Scoring: New best score! {bestScore}");
        }
        
        // Save to cloud and sync leaderboard after round completion
        SaveAndSyncAfterRound();
    }

    /// <summary>
    /// Tracks a round failure
    /// </summary>
    public void FailRound()
    {
        if (isLevelActive)
        {
            roundsFailedInCurrentLevel++;
            Debug.Log($"Scoring: Round failed. Failures in current level: {roundsFailedInCurrentLevel}");
        }
    }

    /// <summary>
    /// Awards reflex bonus based on how quickly the ball was captured
    /// </summary>
    public void AwardReflexBonus()
    {
        if (!isRoundActive)
        {
            Debug.LogWarning("Scoring: AwardReflexBonus called but no round is active");
            return;
        }

        float captureTime = Time.time - roundStartTime;
        int reflexBonus = CalculateReflexBonus(captureTime);
        
        if (reflexBonus > 0)
        {
            currentScore += reflexBonus;
            
            // Log score change
            LogScoreChange(reflexBonus, $"REFLEX BONUS! Capture time: {captureTime:F2}s");
            
            // Check for new best score
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                UpdateBestScoreInPlayerData();
                Debug.Log($"Scoring: New best score with reflex bonus! {bestScore}");
            }
        }
        else
        {
            Debug.Log($"Scoring: No reflex bonus - Capture time: {captureTime:F2}s (over {lowReflexThreshold}s threshold)");
        }
    }

    /// <summary>
    /// Awards accuracy bonus based on how precisely the ball was captured
    /// </summary>
    /// <param name="overlapPercentage">Percentage of ball that was in contact with collector (0.0 to 1.0)</param>
    public void AwardAccuracyBonus(float overlapPercentage)
    {
        if (!isRoundActive)
        {
            Debug.LogWarning("Scoring: AwardAccuracyBonus called but no round is active");
            return;
        }

        // If overlap calculation failed, use a reasonable fallback for scoring
        if (overlapPercentage <= 0f)
        {
            Debug.Log("Scoring: Using fallback accuracy percentage for scoring");
            overlapPercentage = 0.8f; // 80% for scoring purposes
        }

        int accuracyBonus = CalculateAccuracyBonus(overlapPercentage);
        
        if (accuracyBonus > 0)
        {
            currentScore += accuracyBonus;
            
            string accuracyLevel = GetAccuracyLevel(overlapPercentage);
            
            // Log score change
            LogScoreChange(accuracyBonus, $"{accuracyLevel} capture! Overlap: {overlapPercentage:P1}");
            
            // Check for new best score
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                UpdateBestScoreInPlayerData();
                Debug.Log($"Scoring: New best score with accuracy bonus! {bestScore}");
            }
        }
        else
        {
            Debug.Log($"Scoring: No accuracy bonus - Overlap: {overlapPercentage:P1} (below {goodAccuracyThreshold:P0} threshold)");
        }
    }

    /// <summary>
    /// Calculates reflex bonus based on capture time
    /// </summary>
    /// <param name="captureTime">Time taken to capture the ball</param>
    /// <returns>Reflex bonus points</returns>
    private int CalculateReflexBonus(float captureTime)
    {
        if (captureTime <= highReflexThreshold)
        {
            return highReflexBonus;
        }
        else if (captureTime <= mediumReflexThreshold)
        {
            return mediumReflexBonus;
        }
        else if (captureTime <= lowReflexThreshold)
        {
            return lowReflexBonus;
        }
        else
        {
            return 0; // No bonus for slow captures
        }
    }

    /// <summary>
    /// Calculates accuracy bonus based on overlap percentage
    /// </summary>
    /// <param name="overlapPercentage">Percentage of ball that was in contact with collector (0.0 to 1.0)</param>
    /// <returns>Accuracy bonus points</returns>
    private int CalculateAccuracyBonus(float overlapPercentage)
    {
        if (overlapPercentage >= perfectAccuracyThreshold)
        {
            return perfectAccuracyBonus;
        }
        else if (overlapPercentage >= greatAccuracyThreshold)
        {
            return greatAccuracyBonus;
        }
        else if (overlapPercentage >= goodAccuracyThreshold)
        {
            return goodAccuracyBonus;
        }
        else
        {
            return 0; // No bonus for poor accuracy
        }
    }

    /// <summary>
    /// Gets the accuracy level string based on overlap percentage
    /// </summary>
    /// <param name="overlapPercentage">Percentage of ball that was in contact with collector (0.0 to 1.0)</param>
    /// <returns>Accuracy level string</returns>
    private string GetAccuracyLevel(float overlapPercentage)
    {
        if (overlapPercentage >= perfectAccuracyThreshold)
        {
            return "PERFECT";
        }
        else if (overlapPercentage >= greatAccuracyThreshold)
        {
            return "GREAT";
        }
        else if (overlapPercentage >= goodAccuracyThreshold)
        {
            return "GOOD";
        }
        else
        {
            return "POOR";
        }
    }

    /// <summary>
    /// Completes a level and adds level completion bonus
    /// </summary>
    /// <param name="levelNumber">Level that was completed</param>
    public void CompleteLevel(int levelNumber)
    {
        levelsCompleted++;
        isLevelActive = false;
        
        // Check if this was a perfect level (no failures)
        bool isPerfectLevel = roundsFailedInCurrentLevel == 0;
        
        // Level completion bonus
        int levelBonus = levelNumber * 100; // 100 points per level
        currentScore += levelBonus;
        
        // Log level completion bonus
        LogScoreChange(levelBonus, $"Level {levelNumber} completed");
        
        // Perfect level bonus
        if (isPerfectLevel)
        {
            currentScore += perfectLevelBonus;
            perfectLevels++;
            
            // Log perfect level bonus
            LogScoreChange(perfectLevelBonus, $"PERFECT LEVEL! Level {levelNumber} completed with no failures");
        }
        
        Debug.Log($"Scoring: Level {levelNumber} completed! " +
                  $"Perfect Level: {isPerfectLevel}, Failures: {roundsFailedInCurrentLevel}");
        
        // Check for new best score
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            UpdateBestScoreInPlayerData();
            Debug.Log($"Scoring: New best score after level completion! {bestScore}");
        }
    }

    /// <summary>
    /// Calculates the score for a completed round
    /// </summary>
    /// <param name="levelNumber">Current level number</param>
    /// <param name="roundNumber">Current round number</param>
    /// <returns>Calculated score for the round</returns>
    private int CalculateRoundScore(int levelNumber, int roundNumber)
    {
        // Round score = round number × level number × 500
        int roundScore = roundNumber * levelNumber * 500;
        
        Debug.Log($"Scoring: Round score breakdown - " +
                  $"Round: {roundNumber}, Level: {levelNumber}, " +
                  $"Formula: {roundNumber} × {levelNumber} × 500 = {roundScore}");
        
        return roundScore;
    }

    /// <summary>
    /// Updates the best score in PlayerData
    /// </summary>
    private void UpdateBestScoreInPlayerData()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.Data.PlayerBestScore = bestScore;
            PlayerData.Instance.Data.newHighScore = true;
            Debug.Log($"Scoring: Updated PlayerData best score to {bestScore}");
        }
        
        // Update HUD best score label
        UpdateHUDScoreLabels();
    }

    /// <summary>
    /// Saves current score to PlayerData and updates cloud
    /// </summary>
    public async void SaveCurrentScore()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.Data.PlayerScore = currentScore;
            await PlayerData.Instance.SaveAllAsync();
            Debug.Log($"Scoring: Saved current score {currentScore} to PlayerData and cloud");
        }
    }

    /// <summary>
    /// Gets the current score for leaderboard submission
    /// </summary>
    /// <returns>Current score to submit to leaderboard</returns>
    public int GetScoreForLeaderboard()
    {
        return currentScore;
    }

    /// <summary>
    /// Gets the best score for display
    /// </summary>
    /// <returns>Best score achieved</returns>
    public int GetBestScore()
    {
        return bestScore;
    }

    /// <summary>
    /// Gets current session statistics
    /// </summary>
    /// <returns>String containing session stats</returns>
    public string GetSessionStats()
    {
        return $"Score: {currentScore} | Rounds: {roundsCompleted} | Levels: {levelsCompleted} | Perfect Levels: {perfectLevels} | Time: {totalTimeElapsed:F1}s";
    }

    /// <summary>
    /// Logs score changes with consistent formatting
    /// </summary>
    /// <param name="pointsAdded">Points added to score</param>
    /// <param name="reason">Reason for the score change</param>
    private void LogScoreChange(int pointsAdded, string reason)
    {
        Debug.Log($"Scoring: {reason}. {pointsAdded} pts. New score: {currentScore:N0}");
        
        // Update HUD score labels
        UpdateHUDScoreLabels();
    }

    /// <summary>
    /// Updates the HUD score and high score labels
    /// </summary>
    private void UpdateHUDScoreLabels()
    {
        if (HUD.Instance != null)
        {
            // Update current score
            if (HUD.Instance.scoreLabel != null)
            {
                HUD.Instance.scoreLabel.text = currentScore.ToString("N0");
            }
            
            // Update best score
            if (HUD.Instance.bestScoreLabel != null)
            {
                HUD.Instance.bestScoreLabel.text = bestScore.ToString("N0");
            }
            
            Debug.Log($"Scoring: Updated HUD - Score: {currentScore:N0}, Best: {bestScore:N0}");
        }
        else
        {
            Debug.LogWarning("Scoring: HUD.Instance not found, cannot update score labels");
        }
    }

    /// <summary>
    /// Saves current score to PlayerData and syncs with leaderboard after round completion
    /// </summary>
    private async void SaveAndSyncAfterRound()
    {
        Debug.Log("Scoring: Starting cloud save and leaderboard sync after round completion");
        
        // Save current score to PlayerData
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.Data.PlayerScore = currentScore;
            PlayerData.Instance.Data.PlayerBestScore = bestScore;
            
            try
            {
                await PlayerData.Instance.SaveAllAsync();
                Debug.Log($"Scoring: Successfully saved score {currentScore} and best score {bestScore} to cloud");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Scoring: Failed to save to cloud: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Scoring: PlayerData.Instance not found, cannot save to cloud");
        }
        
        // Sync with leaderboard
        if (LeaderboardService.Instance != null)
        {
            try
            {
                await LeaderboardService.Instance.PutPlayerScoreAsync(currentScore);
                Debug.Log($"Scoring: Successfully synced score {currentScore} to leaderboard");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Scoring: Failed to sync with leaderboard: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Scoring: LeaderboardService.Instance not found, cannot sync with leaderboard");
        }
    }

    /// <summary>
    /// Creates floating text for accuracy and reflex bonuses
    /// </summary>
    /// <param name="overlapPercentage">Percentage of ball that was in contact with collector (0.0 to 1.0)</param>
    /// <param name="collectorPosition">Position of the collector for floating text placement</param>
    public void CreateBonusFloatingText(float overlapPercentage, Vector3 collectorPosition)
    {
        Debug.Log($"Scoring: CreateBonusFloatingText called - overlap: {overlapPercentage:P1}, position: {collectorPosition}");
        Debug.Log($"Scoring: Method entry point reached successfully!");
        
        if (VFX.Instance == null || HUD.Instance == null || ThemeController.Instance == null)
        {
            Debug.LogWarning("Scoring: Required instances not found for floating text creation");
            return;
        }

        // Calculate positions
        Vector3 accuracyTextPosition = collectorPosition + Vector3.up * 1f; // 1 unit above collector
        Vector3 reflexTextPosition = collectorPosition + Vector3.up * 2f; // 2 units above collector (above accuracy text)

        // Get accuracy rating and points
        string accuracyRating = GetAccuracyRating(overlapPercentage);
        int accuracyPoints = CalculateAccuracyPoints(overlapPercentage);
        
        // Get reflex rating and points
        string reflexRating = GetReflexRating();
        int reflexPoints = CalculateReflexPoints();

        Debug.Log($"Scoring: Accuracy - {accuracyRating} +{accuracyPoints}, Reflex - {reflexRating} +{reflexPoints}");
        Debug.Log($"Scoring: Overlap: {overlapPercentage:P1}, Capture time: {Time.time - roundStartTime:F2}s");

        // Create reflex floating text (using same pattern as accuracy text)
        Debug.Log($"Scoring: Reflex floating text check - reflexPoints: {reflexPoints}, reflexRating: {reflexRating}");
        
        if (reflexPoints > 0)
        {
            string reflexText = $"{reflexRating} +{reflexPoints}";
            Debug.Log($"Scoring: Creating reflex floating text: '{reflexText}' at {accuracyTextPosition}");
            
            
            Debug.Log($"Scoring: About to call VFX.Instance.CreateFloatingText with text: '{reflexText}'");
            VFX.Instance.CreateFloatingText(
                reflexText,
                accuracyTextPosition, // Use same position as accuracy text
                HUD.Instance.successColor, // Use same color as accuracy text
                1.5f, // duration
                1f // scale
            );
            Debug.Log("Scoring: VFX.Instance.CreateFloatingText call completed");
        }
    }

    /// <summary>
    /// Gets the accuracy rating string based on overlap percentage
    /// </summary>
    /// <param name="overlapPercentage">Percentage of ball that was in contact with collector (0.0 to 1.0)</param>
    /// <returns>Accuracy rating string</returns>
    private string GetAccuracyRating(float overlapPercentage)
    {
        if (overlapPercentage >= perfectAccuracyThreshold)
        {
            return "Perfect!";
        }
        else if (overlapPercentage >= greatAccuracyThreshold)
        {
            return "Great";
        }
        else if (overlapPercentage >= goodAccuracyThreshold)
        {
            return "Good";
        }
        else
        {
            return "Poor";
        }
    }

    /// <summary>
    /// Calculates accuracy points based on overlap percentage
    /// </summary>
    /// <param name="overlapPercentage">Percentage of ball that was in contact with collector (0.0 to 1.0)</param>
    /// <returns>Accuracy points</returns>
    private int CalculateAccuracyPoints(float overlapPercentage)
    {
        if (overlapPercentage >= perfectAccuracyThreshold)
        {
            return perfectAccuracyBonus;
        }
        else if (overlapPercentage >= greatAccuracyThreshold)
        {
            return greatAccuracyBonus;
        }
        else if (overlapPercentage >= goodAccuracyThreshold)
        {
            return goodAccuracyBonus;
        }
        else
        {
            return 0; // Poor
        }
    }

    /// <summary>
    /// Gets the reflex rating string based on capture time
    /// </summary>
    /// <returns>Reflex rating string</returns>
    private string GetReflexRating()
    {
        if (!isRoundActive)
        {
            return "Slow";
        }

        float captureTime = Time.time - roundStartTime;
        Debug.Log($"Scoring: Capture time: {captureTime:F2}s");
        
        if (captureTime <= highReflexThreshold)
        {
            return "Fast!";
        }
        else if (captureTime <= mediumReflexThreshold)
        {
            return "Quick";
        }
        else if (captureTime <= lowReflexThreshold)
        {
            return "Slow";
        }
        else
        {
            return "Slow";
        }
    }

    /// <summary>
    /// Calculates reflex points based on capture time
    /// </summary>
    /// <returns>Reflex points</returns>
    private int CalculateReflexPoints()
    {
        if (!isRoundActive)
        {
            return 0;
        }

        float captureTime = Time.time - roundStartTime;
        
        if (captureTime <= highReflexThreshold)
        {
            return highReflexBonus;
        }
        else if (captureTime <= mediumReflexThreshold)
        {
            return mediumReflexBonus;
        }
        else if (captureTime <= lowReflexThreshold)
        {
            return lowReflexBonus;
        }
        else
        {
            return 0; // No bonus
        }
    }

    /// <summary>
    /// Resets to a specific score (useful for testing)
    /// </summary>
    /// <param name="score">Score to set</param>
    public void SetScore(int score)
    {
        int oldScore = currentScore;
        currentScore = score;
        int change = currentScore - oldScore;
        LogScoreChange(change, $"Score set to {score}");
        
        // Update HUD score labels
        UpdateHUDScoreLabels();
    }

    /// <summary>
    /// Adds a custom bonus to the current score
    /// </summary>
    /// <param name="bonus">Bonus points to add</param>
    /// <param name="reason">Reason for the bonus (for logging)</param>
    public void AddBonus(int bonus, string reason = "Custom Bonus")
    {
        currentScore += bonus;
        
        // Log score change
        LogScoreChange(bonus, reason);
        
        // Check for new best score
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            UpdateBestScoreInPlayerData();
        }
    }
}
