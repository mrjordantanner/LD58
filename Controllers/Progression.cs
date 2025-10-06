using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Singleton controller that manages game progression through Levels and Rounds.
/// Levels act as checkpoints with increased difficulty, Rounds are quick challenges within each Level.
/// </summary>
public class Progression : MonoBehaviour, IInitializable
{
    #region Singleton
    public static Progression Instance;
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
    public string Name { get { return "Progression Controller"; } }

    [Header("Current Progress")]
    [ReadOnly] public int currentLevel = 1;
    [ReadOnly] public int currentRound = 1;
    [ReadOnly] public int totalLevelsCompleted = 0;
    [ReadOnly] public int totalRoundsCompleted = 0;

    [Header("Level Settings")]
    public int roundsPerLevel = 3;
    public float baseDifficulty = 1f;
    public float difficultyIncreasePerLevel = 0.2f;
    
    [Header("Failure Tracking")]
    public int failureLimit = 10;
    [ReadOnly] public int failuresThisLevel = 0;

    [Header("State")]
    [ReadOnly] public bool isLevelActive = false;
    [ReadOnly] public bool isRoundActive = false;
    [ReadOnly] public Level currentLevelData;
    [ReadOnly] public Round currentRoundData;

    [Header("Testing")]
    public bool disableProgression = false;

    // Spawner is now handled by SpawnerController

    // Events (using EventManager)
    // Note: We'll trigger events through EventManager.Instance.TriggerEvent()

    private int roundsCompletedInCurrentLevel = 0;
    #endregion

    #region Initialization
    public System.Collections.IEnumerator Init()
    {
        Debug.Log("Progression: Initializing...");

        // Subscribe to events
        SubscribeToEvents();
        
        if (disableProgression)
        {
            // Testing mode: progression disabled, SpawnerController handles spawning manually
            Debug.Log("Progression: Testing mode enabled - progression disabled");
        }
        else
        {
            // Normal mode: level initialization will happen when game flow starts
            Debug.Log("Progression: Ready for level initialization when game flow starts");
        }
        
        Debug.Log("Progression: Initialized");
        yield return new WaitForSecondsRealtime(0);
    }


    private void SubscribeToEvents()
    {
        // Subscribe to any events that might trigger progression changes
        // These will be implemented when we know what triggers them
        Debug.Log("Progression: Subscribed to events");
    }
    #endregion

    #region Level Management
    /// <summary>
    /// Initialize a new level with the given level number
    /// </summary>
    public void InitializeLevel(int levelNumber)
    {
        currentLevel = levelNumber;
        currentRound = 1;
        roundsCompletedInCurrentLevel = 0;
        failuresThisLevel = 0; // Reset failure counter for new level
        
        // Calculate difficulty for this level
        float levelDifficulty = baseDifficulty + (levelNumber - 1) * difficultyIncreasePerLevel;
        
        // Create level data
        currentLevelData = new Level(levelNumber, roundsPerLevel, levelDifficulty);
        isLevelActive = true;
        
        // Update difficulty parameters for this level
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.OnLevelStart(levelNumber);
        }
        else
        {
            Debug.LogWarning("Progression: No DifficultyManager found! Difficulty will not be scaled.");
        }
        
        // Apply theme for this level with smooth transitions
        if (ThemeController.Instance != null)
        {
            ThemeController.Instance.ApplyThemeForLevelWithTween(levelNumber, 1.5f, Ease.OutQuart);
            Debug.Log($"Progression: Applied theme for level {levelNumber} with smooth transition");
        }
        else
        {
            Debug.LogWarning("Progression: No ThemeController found! Theme will not be applied.");
        }
        
        // Start level tracking in scoring system
        if (Scoring.Instance != null)
        {
            Scoring.Instance.StartLevel(levelNumber);
            Debug.Log($"Progression: Started level tracking for level {levelNumber}");
        }
        
        Debug.Log($"Progression: Initialized Level {levelNumber} (Difficulty: {levelDifficulty:F2})");

        // Trigger level started event
        EventManager.Instance.TriggerEvent(EventManager.LEVEL_STARTED, currentLevelData);
        
        // Start first round
        StartRound(1);
    }

    /// <summary>
    /// Initialize a level without starting the first round
    /// </summary>
    /// <param name="levelNumber">The level number to initialize</param>
    public void InitializeLevelWithoutStarting(int levelNumber)
    {
        Debug.Log($"Progression: InitializeLevelWithoutStarting({levelNumber}) called");
        
        // Reset level state
        isLevelActive = true;
        isRoundActive = false;
        roundsCompletedInCurrentLevel = 0;
        failuresThisLevel = 0; // Reset failure counter for new level
        currentLevel = levelNumber;
        
        // Create level data
        float levelDifficulty = baseDifficulty + (levelNumber - 1) * difficultyIncreasePerLevel;
        currentLevelData = new Level(levelNumber, roundsPerLevel, levelDifficulty);
        
        // Update difficulty parameters for this level
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.OnLevelStart(levelNumber);
        }
        else
        {
            Debug.LogWarning("Progression: No DifficultyManager found! Difficulty will not be scaled.");
        }
        
        // Apply theme for this level with smooth transitions
        if (ThemeController.Instance != null)
        {
            ThemeController.Instance.ApplyThemeForLevelWithTween(levelNumber, 1.5f, Ease.OutQuart);
            Debug.Log($"Progression: Applied theme for level {levelNumber} with smooth transition");
        }
        
        // Trigger level started event
        EventManager.Instance.TriggerEvent(EventManager.LEVEL_STARTED, currentLevelData);
        
        Debug.Log($"Progression: Initialized Level {levelNumber} without starting round (Difficulty: {levelDifficulty:F2})");
    }

    /// <summary>
    /// Complete the current level
    /// </summary>
    public void CompleteLevel()
    {
        if (!isLevelActive) return;
        
        isLevelActive = false;
        totalLevelsCompleted++;
        
        // Update scoring system for level completion
        if (Scoring.Instance != null)
        {
            Scoring.Instance.CompleteLevel(currentLevel);
            Debug.Log($"Progression: Updated scoring for level {currentLevel} completion");
        }
        
        Debug.Log($"Progression: Level {currentLevel} completed! Starting level completion sequence...");
        
        // Start the level completion sequence coroutine
        StartCoroutine(LevelCompletionSequence());
    }

    /// <summary>
    /// Handles the paced sequence for level completion and progression
    /// </summary>
    private System.Collections.IEnumerator LevelCompletionSequence()
    {
        // 1) Trigger level completed event
        Debug.Log("Progression: Triggering level completed event...");
        EventManager.Instance.TriggerEvent(EventManager.LEVEL_COMPLETED, currentLevelData);
        
        // 2) Wait 1.5 seconds to show level complete message
        Debug.Log("Progression: Waiting 1.5 seconds...");
        yield return new WaitForSeconds(1.5f);
        
        // 3) Despawn the player
        Debug.Log("Progression: Despawning player...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.DespawnPlayer();
        }
        
        // 4) Wait 1 second
        Debug.Log("Progression: Waiting 1 second...");
        yield return new WaitForSeconds(1f);
        
        // 5) Check if we've reached max level (win condition)
        int maxLevel = DifficultyManager.Instance != null ? DifficultyManager.Instance.maxLevel : 20;
        if (currentLevel >= maxLevel)
        {
            Debug.Log("Progression: Max level reached! Player has won the game!");

            HUD.Instance.ShowAlertMessage("Congratulations, you've won!", 0.5f, 5f, 1f);
            GameManager.Instance.GameOver();
            yield break; 
        }
        
        // 6) Increment level and reset round
        Debug.Log("Progression: Advancing to next level...");
        currentLevel++;
        currentRound = 1;
        roundsCompletedInCurrentLevel = 0;
        
        // 7) Wait 0.5 seconds
        Debug.Log("Progression: Waiting 0.5 seconds...");
        yield return new WaitForSeconds(0.5f);
        
        // 8) Spawn the player
        Debug.Log("Progression: Respawning player...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SpawnPlayer();
        }
        
        // 9) Wait 0.5 seconds
        Debug.Log("Progression: Waiting 0.5 seconds...");
        yield return new WaitForSeconds(0.5f);
        
        // 10) Start the new level
        Debug.Log("Progression: Starting new level...");
        InitializeLevel(currentLevel);
        
        Debug.Log("Progression: Level completion sequence complete");
    }

    #endregion

    #region Round Management
    /// <summary>
    /// Start a new round within the current level
    /// </summary>
    public void StartRound(int roundNumber)
    {
        Debug.Log($"Progression: StartRound({roundNumber}) called. isLevelActive: {isLevelActive}");
        
        if (!isLevelActive) 
        {
            Debug.LogWarning("Progression: Cannot start round - no active level!");
            return;
        }
        
        currentRound = roundNumber;
        
        // Create round data
        currentRoundData = new Round(roundNumber, currentLevel, currentLevelData.difficulty);
        isRoundActive = true;
        
        // Start round tracking in scoring system
        if (Scoring.Instance != null)
        {
            Scoring.Instance.StartRound();
            Debug.Log($"Progression: Started round tracking for round {currentLevel}-{roundNumber}");
        }
        
        Debug.Log($"Progression: Round state set. isRoundActive: {isRoundActive}");
        
        // Spawn player at the beginning of each new round
        Debug.Log($"Progression: Starting round {roundNumber} - spawning player...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SpawnPlayer();
        }
        
        // Start the SpawnerController round flow (anticipation -> VFX -> ball spawn)
        if (SpawnerController.Instance != null)
        {
            SpawnerController.Instance.StartRound();
            Debug.Log($"Progression: Started SpawnerController round flow for Round {currentLevel}-{roundNumber}");
        }
        else
        {
            Debug.LogWarning("Progression: No SpawnerController found! Cannot start round flow.");
        }

        // Trigger round started event
        EventManager.Instance.TriggerEvent(EventManager.ROUND_STARTED, currentRoundData);
    }

    /// <summary>
    /// Complete the current round
    /// </summary>
    public void CompleteRound()
    {
        print("Progression: CompleteRound()");

        // Skip progression logic if disabled (testing mode)
        if (disableProgression) return;

        if (!isRoundActive) return;
        
        isRoundActive = false;
        roundsCompletedInCurrentLevel++;
        totalRoundsCompleted++;
        
        // End the SpawnerController round
        if (SpawnerController.Instance != null)
        {
            SpawnerController.Instance.EndRound();
            Debug.Log("Progression: Ended SpawnerController round");
        }
        
        Debug.Log($"Progression: Round {currentLevel}-{currentRound} completed! Starting success sequence...");
        
        // Update LastRoundCompleted in PlayerData and save to cloud
        if (PlayerData.Instance != null)
        {
            string roundProgress = $"{currentLevel}-{currentRound}";
            PlayerData.Instance.UpdateLastRoundCompleted(roundProgress);
            Debug.Log($"Progression: Updated LastRoundCompleted to '{roundProgress}'");
        }
        
        // Update scoring system
        if (Scoring.Instance != null)
        {
            Scoring.Instance.CompleteRound(currentLevel, currentRound);
            Debug.Log($"Progression: Updated scoring for round {currentLevel}-{currentRound}");
        }
        
        // Start the success sequence coroutine
        StartCoroutine(RoundSuccessSequence());
    }

    /// <summary>
    /// Handles the paced sequence for round success and progression
    /// </summary>
    private System.Collections.IEnumerator RoundSuccessSequence()
    {
        // Wait 0.5 seconds before showing the round complete alert
        yield return new WaitForSeconds(0.5f);
        
        // 1) Show success alert message
        Debug.Log("Progression: Showing round success message...");
        HUD.Instance.ShowAlertMessage("Round Complete!", 0.3f, 2f, 0.5f, HUD.Instance.successColor);
        AudioManager.Instance.PlaySound("Chime-1");
        
        // 2) Trigger round completed event
        Debug.Log("Progression: Triggering round completed event...");
        EventManager.Instance.TriggerEvent(EventManager.ROUND_COMPLETED, currentRoundData);
        
        // 2) Destroy any remaining collectibles (shouldn't be any, but just in case)
        Debug.Log("Progression: Cleaning up any remaining collectibles...");
        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        foreach (Collectible collectible in collectibles)
        {
            if (collectible != null)
            {
                collectible.DestroyMe();
            }
        }
        
        // 3) Wait 1 second
        Debug.Log("Progression: Waiting 1 second...");
        yield return new WaitForSeconds(1f);
        
        // 4) Despawn the player
        Debug.Log("Progression: Despawning player...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.DespawnPlayer();
        }
        
        // 5) Wait 1 second
        Debug.Log("Progression: Waiting 1 second...");
        yield return new WaitForSeconds(1f);
        
        // 6) Show new level-round text (this happens automatically via events)
        Debug.Log("Progression: Showing progression...");
        
        // 7) Wait 0.5 seconds
        Debug.Log("Progression: Waiting 0.5 seconds...");
        yield return new WaitForSeconds(0.5f);
        
        // 8) Spawn the player (only if not completing the level)
        if (roundsCompletedInCurrentLevel < roundsPerLevel)
        {
            Debug.Log("Progression: Respawning player for next round...");
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.SpawnPlayer();
            }
            
            // 9) Wait 0.5 seconds
            Debug.Log("Progression: Waiting 0.5 seconds...");
            yield return new WaitForSeconds(0.5f);
            
            // 10) Start next round
            Debug.Log("Progression: Starting next round...");
            StartRound(currentRound + 1);
        }
        else
        {
            Debug.Log("Progression: Level completed - player will be spawned when new level starts");
            // 10) Complete the level (player will be spawned in LevelCompletionSequence)
            CompleteLevel();
        }
        
        Debug.Log("Progression: Round success sequence complete");
    }

    /// <summary>
    /// Fail the current round and restart it
    /// </summary>
    public void FailRound()
    {
        Debug.Log($"****** Progression: FailRound() called - disableProgression: {disableProgression}, isRoundActive: {isRoundActive}");
        
        // Skip progression logic if disabled (testing mode)
        if (disableProgression) 
        {
            Debug.Log("!!!! Progression: Round failure skipped - progression is disabled !!!!!!");
            return;
        }

        if (!isRoundActive) 
        {
            Debug.Log("!!!! Progression: Round failure skipped - no active round !!!!!!!");
            return;
        }
        
        isRoundActive = false;
        
        // Track failure for this level
        failuresThisLevel++;
        Debug.Log($"Progression: Round {currentLevel}-{currentRound} failed! Failures this level: {failuresThisLevel}/{failureLimit}");
        
        // Check if failure limit exceeded
        if (failuresThisLevel >= failureLimit)
        {
            Debug.Log($"Progression: Failure limit exceeded ({failuresThisLevel}/{failureLimit})! Triggering game over...");
            Collectible[] collectibles = FindObjectsOfType<Collectible>();
            foreach (Collectible collectible in collectibles)
            {
                if (collectible != null)
                {
                    collectible.DestroyMe();
                }
            }
            PlayerManager.Instance.DespawnPlayer();
            GameManager.Instance.GameOver();
            return;
        }
        
        Debug.Log($"Progression: Starting reset sequence...");
        
        // Play round failure sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("Bad-2");
        }
        
        // Start the reset sequence coroutine
        Debug.Log("Progression: Starting RoundFailureSequence coroutine...");
        StartCoroutine(RoundFailureSequence());
    }

    /// <summary>
    /// Handles the paced sequence for round failure and restart
    /// </summary>
    private IEnumerator RoundFailureSequence()
    {
        Debug.Log("======= Progression: RoundFailureSequence started  ====");

        VFX.Instance.FlashAlertColor();

        GameManager.Instance.inputSuspended = true;
        SpawnerController.Instance.EndRound();

        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        foreach (Collectible collectible in collectibles)
        {
            if (collectible != null)
            {
                collectible.DestroyMe();
            }
        }
        PlayerManager.Instance.DespawnPlayer();

        // Trigger round failed event
        Scoring.Instance.FailRound();
        EventManager.Instance.TriggerEvent(EventManager.ROUND_FAILED, currentRoundData);
        
        yield return new WaitForSeconds(0.5f);
        PlayerManager.Instance.DespawnPlayer();

        yield return new WaitForSeconds(0.5f);

        PlayerManager.Instance.SpawnPlayer();

        // Start the round over
        Debug.Log($"Progression: Starting round {currentRound} over...");
        StartRound(currentRound);

        GameManager.Instance.inputSuspended = false;
        Debug.Log("Progression: Round reset sequence complete");
    }
    #endregion

    #region Public API
    /// <summary>
    /// Get the current progression as a formatted string (e.g., "1-3")
    /// </summary>
    public string GetCurrentProgressionString()
    {
        return $"{currentLevel}-{currentRound}";
    }

    /// <summary>
    /// Get the current difficulty multiplier
    /// </summary>
    public float GetCurrentDifficulty()
    {
        return currentLevelData?.difficulty ?? baseDifficulty;
    }

    /// <summary>
    /// Check if we're in the middle of a level
    /// </summary>
    public bool IsInLevel()
    {
        return isLevelActive;
    }

    /// <summary>
    /// Check if we're in the middle of a round
    /// </summary>
    public bool IsInRound()
    {
        // If progression is disabled, we're not in a round
        if (disableProgression) return false;
        
        return isRoundActive;
    }


    /// <summary>
    /// Get progress within current level (0-1)
    /// </summary>
    public float GetLevelProgress()
    {
        if (!isLevelActive) return 1f;
        return (float)roundsCompletedInCurrentLevel / roundsPerLevel;
    }

    /// <summary>
    /// Advance to next level (useful for testing or special cases)
    /// </summary>
    public void AdvanceToNextLevel()
    {
        if (isLevelActive)
        {
            CompleteLevel();
        }
        InitializeLevel(currentLevel + 1);
    }

    /// <summary>
    /// Reset progression to level 1
    /// </summary>
    public void ResetProgression()
    {
        isLevelActive = false;
        isRoundActive = false;
        currentLevel = 1;
        currentRound = 1;
        totalLevelsCompleted = 0;
        totalRoundsCompleted = 0;
        roundsCompletedInCurrentLevel = 0;
        failuresThisLevel = 0; // Reset failure counter
        
        Debug.Log("Progression: Reset to beginning");
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // Unsubscribe from events if needed
        Debug.Log("Progression: Destroyed");
    }
    #endregion
}

/// <summary>
/// Represents a Level in the game progression
/// </summary>
[System.Serializable]
public class Level
{
    public int levelNumber;
    public int totalRounds;
    public float difficulty;
    public DateTime startTime;
    public DateTime? endTime;
    public bool isCompleted;

    public Level(int levelNumber, int totalRounds, float difficulty)
    {
        this.levelNumber = levelNumber;
        this.totalRounds = totalRounds;
        this.difficulty = difficulty;
        this.startTime = DateTime.Now;
        this.endTime = null;
        this.isCompleted = false;
    }

    public void Complete()
    {
        this.endTime = DateTime.Now;
        this.isCompleted = true;
    }

    public TimeSpan GetDuration()
    {
        if (endTime.HasValue)
            return endTime.Value - startTime;
        return DateTime.Now - startTime;
    }
}

/// <summary>
/// Represents a Round within a Level
/// </summary>
[System.Serializable]
public class Round
{
    public int roundNumber;
    public int levelNumber;
    public float difficulty;
    public DateTime startTime;
    public DateTime? endTime;
    public bool isCompleted;

    public Round(int roundNumber, int levelNumber, float difficulty)
    {
        this.roundNumber = roundNumber;
        this.levelNumber = levelNumber;
        this.difficulty = difficulty;
        this.startTime = DateTime.Now;
        this.endTime = null;
        this.isCompleted = false;
    }

    public void Complete()
    {
        this.endTime = DateTime.Now;
        this.isCompleted = true;
    }

    public TimeSpan GetDuration()
    {
        if (endTime.HasValue)
            return endTime.Value - startTime;
        return DateTime.Now - startTime;
    }
}
