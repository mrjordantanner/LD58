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

    [Header("Difficulty Settings")]
    public bool enableDifficultyScaling = true;
    public int maxLevel = 20;

    [Header("Current Difficulty Parameters")]
    [ReadOnly] public int currentLevel = 1;
    [ReadOnly] public float currentMinVelocity = 2f;
    [ReadOnly] public float currentMaxVelocity = 4f;
    [ReadOnly] public float currentAnticipationMin = 1.2f;
    [ReadOnly] public float currentAnticipationMax = 1.8f;
    [ReadOnly] public float currentMinBounciness = 0.8f;
    [ReadOnly] public float currentMaxBounciness = 1.2f;
    [ReadOnly] public float currentMinGravityScale = 0.8f;
    [ReadOnly] public float currentMaxGravityScale = 1.2f;

    [Header("Base Values (Level 1)")]
    public float baseMinVelocity = 2f;
    public float baseMaxVelocity = 4f;
    public float baseAnticipationMin = 1.2f;
    public float baseAnticipationMax = 1.8f;
    public float baseMinBounciness = 0.8f;
    public float baseMaxBounciness = 1.2f;
    public float baseMinGravityScale = 0.8f;
    public float baseMaxGravityScale = 1.2f;

    [Header("Max Values (Final Level)")]
    public float maxMinVelocity = 8f;
    public float maxMaxVelocity = 15f;
    public float maxAnticipationMin = 0.2f;
    public float maxAnticipationMax = 0.5f;
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
        
        // Use simple linear interpolation instead of curves
        currentMinVelocity = Mathf.Lerp(baseMinVelocity, maxMinVelocity, normalizedLevel);
        currentMaxVelocity = Mathf.Lerp(baseMaxVelocity, maxMaxVelocity, normalizedLevel);
        
        currentAnticipationMin = Mathf.Lerp(baseAnticipationMin, maxAnticipationMin, normalizedLevel);
        currentAnticipationMax = Mathf.Lerp(baseAnticipationMax, maxAnticipationMax, normalizedLevel);
        
        // Calculate bounciness using direct base to max interpolation
        currentMinBounciness = Mathf.Lerp(baseMinBounciness, maxMinBounciness, normalizedLevel);
        currentMaxBounciness = Mathf.Lerp(baseMaxBounciness, maxMaxBounciness, normalizedLevel);
        
        // Calculate gravity using direct base to max interpolation
        currentMinGravityScale = Mathf.Lerp(baseMinGravityScale, maxMinGravityScale, normalizedLevel);
        currentMaxGravityScale = Mathf.Lerp(baseMaxGravityScale, maxMaxGravityScale, normalizedLevel);

        // Log the current difficulty parameters
        LogCurrentDifficultyParameters();
        
        // Force inspector refresh
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
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

    /// <summary>
    /// Tests and logs difficulty scaling across all levels
    /// </summary>
    [ContextMenu("Test Difficulty Scaling")]
    public void TestDifficultyScaling()
    {
        Debug.Log("=== TESTING DIFFICULTY SCALING ACROSS ALL LEVELS ===");
        
        for (int level = 1; level <= maxLevel; level++)
        {
            UpdateDifficultyParameters(level);
            Debug.Log($"Level {level,2}: Vel({currentMinVelocity:F1}-{currentMaxVelocity:F1}) " +
                     $"Ant({currentAnticipationMin:F1}-{currentAnticipationMax:F1}s) " +
                     $"Bounce({currentMinBounciness:F1}-{currentMaxBounciness:F1}) " +
                     $"Grav({currentMinGravityScale:F1}-{currentMaxGravityScale:F1})");
        }
        
        Debug.Log("=== END DIFFICULTY SCALING TEST ===");
    }

    /// <summary>
    /// Simple test to verify the method is working
    /// </summary>
    [ContextMenu("Test Simple Values")]
    public void TestSimpleValues()
    {
        Debug.Log("=== SIMPLE TEST ===");
        Debug.Log($"Before: currentMinVelocity = {currentMinVelocity}, currentMaxVelocity = {currentMaxVelocity}");
        
        // Force set some test values
        currentMinVelocity = 999f;
        currentMaxVelocity = 888f;
        
        Debug.Log($"After manual set: currentMinVelocity = {currentMinVelocity}, currentMaxVelocity = {currentMaxVelocity}");
        
        // Now test the method
        UpdateDifficultyParameters(5);
        
        Debug.Log($"After UpdateDifficultyParameters(5): currentMinVelocity = {currentMinVelocity}, currentMaxVelocity = {currentMaxVelocity}");
        Debug.Log("=== END SIMPLE TEST ===");
    }



    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw difficulty visualization in scene view
        Gizmos.color = Color.Lerp(Color.green, Color.red, GetNormalizedDifficulty());
        Gizmos.DrawWireCube(transform.position, Vector3.one * (1f + GetNormalizedDifficulty()));
    }
}
