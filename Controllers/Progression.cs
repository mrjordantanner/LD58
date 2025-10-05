using System;
using UnityEngine;

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
            // Normal mode: initialize first level
            InitializeLevel(1);
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
        
        // Apply theme for this level
        if (ThemeController.Instance != null)
        {
            ThemeController.Instance.ApplyThemeForLevel(levelNumber);
            Debug.Log($"Progression: Applied theme for level {levelNumber}");
        }
        else
        {
            Debug.LogWarning("Progression: No ThemeController found! Theme will not be applied.");
        }
        
        Debug.Log($"Progression: Initialized Level {levelNumber} (Difficulty: {levelDifficulty:F2})");

        // Trigger level started event
        EventManager.Instance.TriggerEvent(EventManager.LEVEL_STARTED, currentLevelData);
        
        // Start first round
        StartRound(1);
    }

    /// <summary>
    /// Complete the current level
    /// </summary>
    public void CompleteLevel()
    {
        if (!isLevelActive) return;
        
        isLevelActive = false;
        totalLevelsCompleted++;
        
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
        
        // 2) Wait 2 seconds to show level complete message
        Debug.Log("Progression: Waiting 2 seconds...");
        yield return new WaitForSeconds(2f);
        
        // 3) Despawn the player
        Debug.Log("Progression: Despawning player...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.DespawnPlayer();
        }
        
        // 4) Wait 1.5 seconds
        Debug.Log("Progression: Waiting 1.5 seconds...");
        yield return new WaitForSeconds(1.5f);
        
        // 5) Increment level and reset round
        Debug.Log("Progression: Advancing to next level...");
        currentLevel++;
        currentRound = 1;
        roundsCompletedInCurrentLevel = 0;
        
        // 6) Wait 1 second
        Debug.Log("Progression: Waiting 1 second...");
        yield return new WaitForSeconds(1f);
        
        // 7) Spawn the player
        Debug.Log("Progression: Respawning player...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SpawnPlayer();
        }
        
        // 8) Wait 0.5 seconds
        Debug.Log("Progression: Waiting 0.5 seconds...");
        yield return new WaitForSeconds(0.5f);
        
        // 9) Start the new level
        Debug.Log("Progression: Starting new level...");
        InitializeLevel(currentLevel);
        
        Debug.Log("Progression: Level completion sequence complete");
    }

    /// <summary>
    /// Fail the current level
    /// </summary>
    public void FailLevel()
    {
        if (!isLevelActive) return;
        
        isLevelActive = false;
        
        // End the SpawnerController round if active
        if (isRoundActive && SpawnerController.Instance != null)
        {
            SpawnerController.Instance.EndRound();
            isRoundActive = false;
            Debug.Log("Progression: Ended SpawnerController round due to level failure");
        }
        
        Debug.Log($"Progression: Level {currentLevel} failed!");
        
        // Trigger level failed event
        EventManager.Instance.TriggerEvent(EventManager.LEVEL_FAILED, currentLevelData);
    }
    #endregion

    #region Round Management
    /// <summary>
    /// Start a new round within the current level
    /// </summary>
    public void StartRound(int roundNumber)
    {
        //Debug.Log($"Progression: StartRound({roundNumber}) called. isLevelActive: {isLevelActive}");
        
        if (!isLevelActive) 
        {
            Debug.LogWarning("Progression: Cannot start round - no active level!");
            return;
        }
        
        currentRound = roundNumber;
        
        // Create round data
        currentRoundData = new Round(roundNumber, currentLevel, currentLevelData.difficulty);
        isRoundActive = true;
        
        Debug.Log($"Progression: Round state set. isRoundActive: {isRoundActive}");
        
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
        
        // Start the success sequence coroutine
        StartCoroutine(RoundSuccessSequence());
    }

    /// <summary>
    /// Handles the paced sequence for round success and progression
    /// </summary>
    private System.Collections.IEnumerator RoundSuccessSequence()
    {
        // 1) Trigger round completed event
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
        
        // 3) Wait 1.5 seconds
        Debug.Log("Progression: Waiting 1.5 seconds...");
        yield return new WaitForSeconds(1.5f);
        
        // 4) Despawn the player
        Debug.Log("Progression: Despawning player...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.DespawnPlayer();
        }
        
        // 5) Wait 1.5 seconds
        Debug.Log("Progression: Waiting 1.5 seconds...");
        yield return new WaitForSeconds(1.5f);
        
        // 6) Show new level-round text (this happens automatically via events)
        Debug.Log("Progression: Showing progression...");
        
        // 7) Wait 1 second
        Debug.Log("Progression: Waiting 1 second...");
        yield return new WaitForSeconds(1f);
        
        // 8) Spawn the player
        Debug.Log("Progression: Respawning player...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SpawnPlayer();
        }
        
        // 9) Wait 0.5 seconds
        Debug.Log("Progression: Waiting 0.5 seconds...");
        yield return new WaitForSeconds(0.5f);
        
        // 10) Start next round or level
        Debug.Log("Progression: Starting next round/level...");
        if (roundsCompletedInCurrentLevel >= roundsPerLevel)
        {
            CompleteLevel();
        }
        else
        {
            // Start next round
            StartRound(currentRound + 1);
        }
        
        Debug.Log("Progression: Round success sequence complete");
    }

    /// <summary>
    /// Fail the current round and restart it
    /// </summary>
    public void FailRound()
    {
        // Skip progression logic if disabled (testing mode)
        if (disableProgression) return;

        if (!isRoundActive) return;
        
        isRoundActive = false;
        
        Debug.Log($"Progression: Round {currentLevel}-{currentRound} failed! Starting reset sequence...");
        
        // Play round failure sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("Bad-2");
        }
        
        // Start the reset sequence coroutine
        StartCoroutine(RoundFailureSequence());
    }

    /// <summary>
    /// Handles the paced sequence for round failure and restart
    /// </summary>
    private System.Collections.IEnumerator RoundFailureSequence()
    {
        // Suspend input during failure sequence
        GameManager.Instance.inputSuspended = true;
        
        // 1) Immediately destroy collectibles with VFX
        Debug.Log("Progression: Destroying collectibles...");
        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        foreach (Collectible collectible in collectibles)
        {
            if (collectible != null)
            {
                collectible.DestroyMe();
            }
        }
        
        // Trigger round failed event
        EventManager.Instance.TriggerEvent(EventManager.ROUND_FAILED, currentRoundData);
        
        // 2) Wait 1 second
        Debug.Log("Progression: Waiting 1 second...");
        yield return new WaitForSeconds(1f);
        
        // 3) Despawn the player
        Debug.Log("Progression: Despawning player...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.DespawnPlayer();
        }
        
        // 4) Wait 1 second
        Debug.Log("Progression: Waiting 1 second...");
        yield return new WaitForSeconds(1f);
        
        // 5) Respawn the player and start the round over
        Debug.Log("Progression: Respawning player and restarting round...");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SpawnPlayer();
        }
        
        // Start the round over
        StartRound(currentRound);
        
        // Restore input - round failure sequence is complete
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
