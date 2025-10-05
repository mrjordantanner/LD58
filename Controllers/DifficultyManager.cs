using UnityEngine;

/// <summary>
/// Singleton that manages difficulty scaling based on current level
/// Provides smooth difficulty curves for ball spawning parameters
/// </summary>
public class DifficultyManager : MonoBehaviour, IInitializable
{
    #region Singleton
    public static DifficultyManager Instance;
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

    public string Name { get { return "DifficultyManager"; } }

    [Header("Difficulty Curve Settings")]
    public bool enableDifficultyScaling = true;
    public int maxLevel = 20;
    public AnimationCurve velocityCurve = AnimationCurve.EaseInOut(0, 2f, 1, 15f);
    public AnimationCurve anticipationCurve = AnimationCurve.EaseInOut(0, 1.8f, 1, 0.2f);
    public AnimationCurve roundDurationCurve = AnimationCurve.EaseInOut(0, 45f, 1, 15f);
    public AnimationCurve bouncinessRangeCurve = AnimationCurve.EaseInOut(0, 0.4f, 1, 1.8f);
    public AnimationCurve gravityRangeCurve = AnimationCurve.EaseInOut(0, 0.4f, 1, 1.6f);

    [Header("Current Difficulty Parameters")]
    [ReadOnly] public int currentLevel = 1;
    [ReadOnly] public float currentMinVelocity = 2f;
    [ReadOnly] public float currentMaxVelocity = 4f;
    [ReadOnly] public float currentAnticipationMin = 1.2f;
    [ReadOnly] public float currentAnticipationMax = 1.8f;
    [ReadOnly] public float currentRoundDuration = 45f;
    [ReadOnly] public float currentMinBounciness = 0.8f;
    [ReadOnly] public float currentMaxBounciness = 1.2f;
    [ReadOnly] public float currentMinGravityScale = 0.8f;
    [ReadOnly] public float currentMaxGravityScale = 1.2f;

    [Header("Base Values (Level 1)")]
    public float baseMinVelocity = 2f;
    public float baseMaxVelocity = 4f;
    public float baseAnticipationMin = 1.2f;
    public float baseAnticipationMax = 1.8f;
    public float baseRoundDuration = 45f;
    public float baseMinBounciness = 0.8f;
    public float baseMaxBounciness = 1.2f;
    public float baseMinGravityScale = 0.8f;
    public float baseMaxGravityScale = 1.2f;

    [Header("Max Values (Final Level)")]
    public float maxMinVelocity = 8f;
    public float maxMaxVelocity = 15f;
    public float maxAnticipationMin = 0.2f;
    public float maxAnticipationMax = 0.5f;
    public float maxRoundDuration = 15f;
    public float maxMinBounciness = 0.2f;
    public float maxMaxBounciness = 2.0f;
    public float maxMinGravityScale = 0.2f;
    public float maxMaxGravityScale = 1.8f;

    private void Start()
    {
        // Initialize with level 1 difficulty
        UpdateDifficultyParameters(1);
    }

    public System.Collections.IEnumerator Init()
    {
        // Subscribe to progression events
        if (Progression.Instance != null)
        {
            // We'll add event subscription when we know the event system
            Debug.Log("DifficultyManager: Initialized and ready for progression events");
        }
        else
        {
            Debug.LogWarning("DifficultyManager: No Progression instance found");
        }

        yield return new WaitForSecondsRealtime(0);
    }

    /// <summary>
    /// Updates difficulty parameters based on the current level
    /// </summary>
    public void UpdateDifficultyParameters(int level)
    {
        if (!enableDifficultyScaling)
        {
            SetToBaseValues();
            return;
        }

        currentLevel = Mathf.Clamp(level, 1, maxLevel);
        
        // Calculate normalized level (0-1)
        float normalizedLevel = (float)(currentLevel - 1) / (maxLevel - 1);
        
        // Apply curves to get current values
        currentMinVelocity = Mathf.Lerp(baseMinVelocity, maxMinVelocity, velocityCurve.Evaluate(normalizedLevel));
        currentMaxVelocity = Mathf.Lerp(baseMaxVelocity, maxMaxVelocity, velocityCurve.Evaluate(normalizedLevel));
        
        currentAnticipationMin = Mathf.Lerp(baseAnticipationMin, maxAnticipationMin, anticipationCurve.Evaluate(normalizedLevel));
        currentAnticipationMax = Mathf.Lerp(baseAnticipationMax, maxAnticipationMax, anticipationCurve.Evaluate(normalizedLevel));
        
        currentRoundDuration = Mathf.Lerp(baseRoundDuration, maxRoundDuration, roundDurationCurve.Evaluate(normalizedLevel));
        
        float bouncinessRange = Mathf.Lerp(0.4f, 1.8f, bouncinessRangeCurve.Evaluate(normalizedLevel));
        currentMinBounciness = Mathf.Max(0.1f, 1f - bouncinessRange);
        currentMaxBounciness = 1f + bouncinessRange;
        
        float gravityRange = Mathf.Lerp(0.4f, 1.6f, gravityRangeCurve.Evaluate(normalizedLevel));
        currentMinGravityScale = Mathf.Max(0.1f, 1f - gravityRange);
        currentMaxGravityScale = 1f + gravityRange;

        // Log the current difficulty parameters
        LogCurrentDifficultyParameters();
    }

    /// <summary>
    /// Sets all parameters to base values (level 1)
    /// </summary>
    private void SetToBaseValues()
    {
        currentLevel = 1;
        currentMinVelocity = baseMinVelocity;
        currentMaxVelocity = baseMaxVelocity;
        currentAnticipationMin = baseAnticipationMin;
        currentAnticipationMax = baseAnticipationMax;
        currentRoundDuration = baseRoundDuration;
        currentMinBounciness = baseMinBounciness;
        currentMaxBounciness = baseMaxBounciness;
        currentMinGravityScale = baseMinGravityScale;
        currentMaxGravityScale = baseMaxGravityScale;
    }

    /// <summary>
    /// Logs the current difficulty parameters to console
    /// </summary>
    private void LogCurrentDifficultyParameters()
    {
        Debug.Log($"=== DIFFICULTY LEVEL {currentLevel} ===");
        Debug.Log($"Velocity Range: {currentMinVelocity:F1} - {currentMaxVelocity:F1}");
        Debug.Log($"Anticipation Time: {currentAnticipationMin:F1}s - {currentAnticipationMax:F1}s");
        Debug.Log($"Round Duration: {currentRoundDuration:F1}s");
        Debug.Log($"Bounciness Range: {currentMinBounciness:F1} - {currentMaxBounciness:F1}");
        Debug.Log($"Gravity Range: {currentMinGravityScale:F1} - {currentMaxGravityScale:F1}");
        Debug.Log("================================");
    }

    /// <summary>
    /// Applies current difficulty parameters to SpawnerController
    /// </summary>
    public void ApplyDifficultyToSpawnerController()
    {
        if (SpawnerController.Instance == null)
        {
            Debug.LogWarning("DifficultyManager: No SpawnerController found to apply difficulty to");
            return;
        }

        SpawnerController spawnerController = SpawnerController.Instance;
        
        // Apply velocity settings
        spawnerController.minVelocity = currentMinVelocity;
        spawnerController.maxVelocity = currentMaxVelocity;
        
        // Apply anticipation settings
        spawnerController.anticipationMinDuration = currentAnticipationMin;
        spawnerController.anticipationMaxDuration = currentAnticipationMax;
        
        // Apply round duration
        spawnerController.SetRoundDuration(currentRoundDuration);
        
        // Apply physics settings
        spawnerController.minBounciness = currentMinBounciness;
        spawnerController.maxBounciness = currentMaxBounciness;
        spawnerController.minGravityScale = currentMinGravityScale;
        spawnerController.maxGravityScale = currentMaxGravityScale;

        Debug.Log($"DifficultyManager: Applied Level {currentLevel} difficulty to SpawnerController");
    }

    /// <summary>
    /// Called when a new level starts
    /// </summary>
    public void OnLevelStart(int level)
    {
        UpdateDifficultyParameters(level);
        ApplyDifficultyToSpawnerController();
    }

    /// <summary>
    /// Gets the current difficulty level
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    /// <summary>
    /// Gets the normalized difficulty (0-1)
    /// </summary>
    public float GetNormalizedDifficulty()
    {
        return (float)(currentLevel - 1) / (maxLevel - 1);
    }

    /// <summary>
    /// Resets difficulty to level 1
    /// </summary>
    public void ResetDifficulty()
    {
        UpdateDifficultyParameters(1);
        ApplyDifficultyToSpawnerController();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw difficulty visualization in scene view
        Gizmos.color = Color.Lerp(Color.green, Color.red, GetNormalizedDifficulty());
        Gizmos.DrawWireCube(transform.position, Vector3.one * (1f + GetNormalizedDifficulty()));
    }
}
