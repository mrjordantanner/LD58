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

    public IEnumerator Init()
    {
        yield return new WaitForSecondsRealtime(0);
    }
    
    private void Update()
    {
        // Handle round timer (just for tracking, no limit)
        if (isRoundActive)
        {
            roundTimer += Time.deltaTime;
        }
    }


    /// <summary>
    /// Spawns a ball at a random position within the play area
    /// </summary>
    public GameObject SpawnBall()
    {
        // Don't spawn if game is in menu state
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.MainMenu)
        {
            Debug.LogWarning("SpawnerController: Cannot spawn ball - game is in menu state");
            return null;
        }
        
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
        
        // Apply current theme accent color to the ball
        ApplyThemeToBall(ball);

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
        totalBallsSpawned = 0;
        Debug.Log("SpawnerController: Reset spawn system");
    }
    

    /// <summary>
    /// Starts a new round with anticipation and VFX
    /// </summary>
    public void StartRound()
    {
        // Don't start round if game is in menu state
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.MainMenu)
        {
            Debug.LogWarning("SpawnerController: Cannot start round - game is in menu state");
            return;
        }
        
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
    /// Gets the current round timer value
    /// </summary>
    public float GetRoundTimer()
    {
        return isRoundActive ? roundTimer : 0f;
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

    /// <summary>
    /// Applies the current theme accent color to a ball
    /// </summary>
    /// <param name="ball">The ball to apply theme to</param>
    private void ApplyThemeToBall(GameObject ball)
    {
        if (ThemeController.Instance != null)
        {
            SpriteRenderer ballRenderer = ball.GetComponent<SpriteRenderer>();
            if (ballRenderer != null)
            {
                ballRenderer.color = ThemeController.Instance.GetAccentColor();
            }
        }
    }
}