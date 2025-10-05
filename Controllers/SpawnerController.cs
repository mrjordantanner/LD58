using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour, IInitializable
{
    #region Singleton
    public static SpawnerController Instance;
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

    public string Name { get { return "SpawnerController"; } }

    public GameObject BallSpawnVFXObject;

    [Header("Play Area")]
    public BoxCollider2D playAreaCollider;

    [Header("Round Flow Settings")]
    public float anticipationMinDuration = 0.5f;
    public float anticipationMaxDuration = 1.5f;
    public float ballSpawnVFXLifespan = 1f;
    public bool isRoundActive = false;
    public float roundTimer = 0f;
    public float roundDuration = 30f; // Default round duration, can be configured per round

    [Header("Spawner Management")]
    public List<Spawner> activeSpawners = new List<Spawner>();
    public bool autoFindSpawners = true;
    public bool autoEnableSpawners = false;
    
    [Header("Global Spawn Settings")]
    public bool globalSpawnEnabled = true;
    public float globalSpawnInterval = 5f;
    public bool useRandomInterval = false;
    public float minInterval = 3f;
    public float maxInterval = 7f;
    public bool spawnOnStart = false;
    
    [Header("Ball Physics Settings")]
    public GameObject ballPrefab;
    public float minVelocity = 3f;
    public float maxVelocity = 8f;
    public float minMass = 0.5f;
    public float maxMass = 2f;
    public float minBounciness = 0.5f;
    public float maxBounciness = 1.2f;
    public float minGravityScale = 0.5f;
    public float maxGravityScale = 1.5f;
    
    [Header("Spawn Direction Settings")]
    public bool randomizeDirection = true;
    public Vector2[] preferredDirections = {
        Vector2.right,
        Vector2.left,
        Vector2.up,
        Vector2.down,
        new Vector2(1, 1).normalized,
        new Vector2(-1, 1).normalized,
        new Vector2(1, -1).normalized,
        new Vector2(-1, -1).normalized
    };
    public float directionVariance = 30f; // Degrees of variance from preferred directions
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    [ReadOnly] public int totalBallsSpawned = 0;
    [ReadOnly] public float spawnTimer = 0f;

    public IEnumerator Init()
    {
        // Find all spawners in the scene (for backward compatibility)
        if (autoFindSpawners)
        {
            FindAllSpawners();
        }
        
        // Auto-enable spawning if requested
        if (spawnOnStart)
        {
            globalSpawnEnabled = true;
        }
        
        // Set initial random interval if using random timing
        if (useRandomInterval)
        {
            SetRandomInterval();
        }
        
        yield return new WaitForSecondsRealtime(0);
    }
    
    private void Update()
    {
        // Handle round timer
        if (isRoundActive)
        {
            roundTimer += Time.deltaTime;
            
            // Check if round should end
            if (roundTimer >= roundDuration)
            {
                EndRound();
            }
        }
        
        // Handle automatic spawning (only when not in round mode)
        if (!globalSpawnEnabled || isRoundActive) return;
        
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= globalSpawnInterval)
        {
            SpawnBall();
            spawnTimer = 0f; // Reset timer after spawning
            
            // Set new random interval if using random timing
            if (useRandomInterval)
            {
                SetRandomInterval();
            }
        }
    }

    /// <summary>
    /// Finds all Spawner components in the scene
    /// </summary>
    public void FindAllSpawners()
    {
        activeSpawners.Clear();
        Spawner[] foundSpawners = FindObjectsOfType<Spawner>();
        activeSpawners.AddRange(foundSpawners);
        
        Debug.Log($"SpawnerController: Found {activeSpawners.Count} spawners in scene");
    }

    /// <summary>
    /// Adds a spawner to the active list
    /// </summary>
    public void RegisterSpawner(Spawner spawner)
    {
        if (!activeSpawners.Contains(spawner))
        {
            activeSpawners.Add(spawner);
            Debug.Log($"SpawnerController: Registered spawner {spawner.name}");
        }
    }

    /// <summary>
    /// Removes a spawner from the active list
    /// </summary>
    public void UnregisterSpawner(Spawner spawner)
    {
        if (activeSpawners.Contains(spawner))
        {
            activeSpawners.Remove(spawner);
            Debug.Log($"SpawnerController: Unregistered spawner {spawner.name}");
        }
    }

    /// <summary>
    /// Enables or disables spawning
    /// </summary>
    public void SetGlobalSpawnEnabled(bool enabled)
    {
        globalSpawnEnabled = enabled;
        Debug.Log($"SpawnerController: Global spawn enabled set to {enabled}");
    }

    /// <summary>
    /// Sets the spawn interval
    /// </summary>
    public void SetGlobalSpawnInterval(float interval)
    {
        globalSpawnInterval = interval;
        Debug.Log($"SpawnerController: Spawn interval set to {interval}");
    }
    
    /// <summary>
    /// Sets a random spawn interval between min and max
    /// </summary>
    private void SetRandomInterval()
    {
        globalSpawnInterval = Random.Range(minInterval, maxInterval);
    }

    /// <summary>
    /// Spawns a ball at a random position within the play area
    /// </summary>
    public GameObject SpawnBall()
    {
        if (ballPrefab == null)
        {
            Debug.LogError("SpawnerController: Ball prefab is not assigned!");
            return null;
        }

        // Get random spawn position within play area
        Vector2 spawnPosition = GetRandomSpawnPosition();
        
        // Create the ball
        GameObject ball = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
        ball.name = $"Ball_{totalBallsSpawned}";
        totalBallsSpawned++;

        // Configure the ball with random physics properties
        ConfigureBall(ball);

        Debug.Log($"SpawnerController: Spawned ball at {spawnPosition}");
        return ball;
    }

    /// <summary>
    /// Gets a random spawn position within the play area collider
    /// </summary>
    private Vector2 GetRandomSpawnPosition()
    {
        if (playAreaCollider == null)
        {
            Debug.LogWarning("SpawnerController: No play area collider assigned, using default position");
            return Vector2.zero;
        }

        Bounds bounds = playAreaCollider.bounds;
        
        // Add some margin from the edges to prevent spawning too close to walls
        float margin = 0.5f;
        Vector2 min = new Vector2(bounds.min.x + margin, bounds.min.y + margin);
        Vector2 max = new Vector2(bounds.max.x - margin, bounds.max.y - margin);
        
        return new Vector2(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y)
        );
    }

    /// <summary>
    /// Gets a random direction for ball movement
    /// </summary>
    private Vector2 GetRandomDirection()
    {
        if (!randomizeDirection || preferredDirections.Length == 0)
        {
            return Vector2.right; // Default direction
        }

        // Pick a random preferred direction
        Vector2 baseDirection = preferredDirections[Random.Range(0, preferredDirections.Length)];
        
        // Add random variance
        float randomAngle = Random.Range(-directionVariance, directionVariance);
        return Quaternion.AngleAxis(randomAngle, Vector3.forward) * baseDirection;
    }

    /// <summary>
    /// Configures a ball with random physics properties and launches it
    /// </summary>
    public void ConfigureBall(GameObject ball)
    {
        BallMovement ballMovement = ball.GetComponent<BallMovement>();
        if (ballMovement == null)
        {
            Debug.LogWarning($"SpawnerController: No BallMovement component found on ball {ball.name}");
            return;
        }

        // Generate random properties
        Vector2 direction = GetRandomDirection();
        float velocity = Random.Range(minVelocity, maxVelocity);
        float mass = Random.Range(minMass, maxMass);
        float bounciness = Random.Range(minBounciness, maxBounciness);
        float gravityScale = Random.Range(minGravityScale, maxGravityScale);

        // Initialize the ball
        ballMovement.Initialize(ball.transform.position, direction, velocity, mass, bounciness, gravityScale);
    }

    /// <summary>
    /// Clears all spawned balls
    /// </summary>
    public void ClearAllBalls()
    {
        BallMovement[] allBalls = FindObjectsOfType<BallMovement>();
        foreach (BallMovement ball in allBalls)
        {
            if (ball.name.StartsWith("Ball_"))
            {
                DestroyImmediate(ball.gameObject);
            }
        }
        totalBallsSpawned = 0;
        Debug.Log("SpawnerController: Cleared all balls");
    }

    /// <summary>
    /// Gets statistics about spawned balls
    /// </summary>
    public string GetSpawnStatistics()
    {
        BallMovement[] allBalls = FindObjectsOfType<BallMovement>();
        return $"Total Balls: {allBalls.Length}";
    }

    /// <summary>
    /// Resets spawning system
    /// </summary>
    public void ResetSpawnSystem()
    {
        spawnTimer = 0f;
        totalBallsSpawned = 0;
        if (useRandomInterval)
        {
            SetRandomInterval();
        }
        Debug.Log("SpawnerController: Reset spawn system");
    }
    
    /// <summary>
    /// Manually trigger a spawn
    /// </summary>
    public void TriggerSpawn()
    {
        SpawnBall();
    }

    /// <summary>
    /// Starts a new round with anticipation and VFX
    /// </summary>
    public void StartRound()
    {
        if (isRoundActive)
        {
            Debug.LogWarning("SpawnerController: Round is already active!");
            return;
        }

        Debug.Log("SpawnerController: Starting new round");
        
        // Apply current difficulty settings if DifficultyManager is available
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ApplyDifficultyToSpawnerController();
        }
        
        // Reset round state
        isRoundActive = true;
        roundTimer = 0f;
        
        // Start the round flow
        StartCoroutine(RoundFlow());
    }

    /// <summary>
    /// Ends the current round
    /// </summary>
    public void EndRound()
    {
        if (!isRoundActive)
        {
            Debug.LogWarning("SpawnerController: No active round to end!");
            return;
        }

        Debug.Log($"SpawnerController: Round ended after {roundTimer:F1} seconds");
        
        isRoundActive = false;
        roundTimer = 0f;
        
        // Trigger round end event (for other systems to listen to)
        // EventManager.Instance?.TriggerEvent("RoundEnded");
    }

    /// <summary>
    /// Gets the current round progress (0-1)
    /// </summary>
    public float GetRoundProgress()
    {
        if (!isRoundActive) return 0f;
        return Mathf.Clamp01(roundTimer / roundDuration);
    }

    /// <summary>
    /// Gets the remaining round time
    /// </summary>
    public float GetRemainingRoundTime()
    {
        if (!isRoundActive) return 0f;
        return Mathf.Max(0f, roundDuration - roundTimer);
    }

    /// <summary>
    /// Sets the round duration
    /// </summary>
    public void SetRoundDuration(float duration)
    {
        roundDuration = duration;
        Debug.Log($"SpawnerController: Round duration set to {duration} seconds");
    }

    /// <summary>
    /// Coroutine that handles the round flow: anticipation -> VFX -> ball spawn
    /// </summary>
    private System.Collections.IEnumerator RoundFlow()
    {
        // Step 1: Choose spawn point
        Vector2 spawnPosition = GetRandomSpawnPosition();
        Debug.Log($"SpawnerController: Round starting - ball will spawn at {spawnPosition}");

        // Step 2: Anticipation pause
        float anticipationDuration = Random.Range(anticipationMinDuration, anticipationMaxDuration);
        Debug.Log($"SpawnerController: Anticipation pause for {anticipationDuration:F1} seconds");
        yield return new WaitForSeconds(anticipationDuration);

        // Step 3: Spawn VFX
        if (BallSpawnVFXObject != null)
        {
            GameObject vfx = Instantiate(BallSpawnVFXObject, spawnPosition, Quaternion.identity);
            vfx.name = $"BallSpawnVFX_{Time.time:F1}";
            
            // Destroy VFX after lifespan
            Destroy(vfx, ballSpawnVFXLifespan);
            
            Debug.Log($"SpawnerController: Spawned VFX at {spawnPosition}, will destroy in {ballSpawnVFXLifespan} seconds");
        }
        else
        {
            Debug.LogWarning("SpawnerController: BallSpawnVFXObject is not assigned!");
        }

        // Step 4: Wait for VFX duration, then spawn ball
        yield return new WaitForSeconds(ballSpawnVFXLifespan);

        // Step 5: Spawn and launch the ball
        GameObject ball = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
        ball.name = $"Ball_{totalBallsSpawned}";
        totalBallsSpawned++;

        // Configure the ball with random physics properties
        ConfigureBall(ball);

        Debug.Log($"SpawnerController: Ball spawned and launched at {spawnPosition}");
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // Draw play area bounds
        if (playAreaCollider != null)
        {
            Gizmos.color = Color.green;
            Bounds bounds = playAreaCollider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            
            // Draw spawn area with margin
            Gizmos.color = Color.yellow;
            float margin = 0.5f;
            Vector3 marginSize = bounds.size - Vector3.one * margin * 2;
            Gizmos.DrawWireCube(bounds.center, marginSize);
        }
    }
}