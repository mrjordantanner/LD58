using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [Header("SpawnerController Override")]
    public bool useSpawnerControllerSettings = true;

    [Header("Spawner Settings")]
    public GameObject ObjectPrefab;
    public bool isEnabled = false;
    public bool isTimed = true;
    public float spawnInterval = 5f;
 
    [Header("Spawn Options")]
    public bool useRandomInterval = false;
    public float minInterval = 3f;
    public float maxInterval = 7f;
    public bool spawnOnStart = false;
    
    [Header("Physics Settings")]
    public bool randomizeVelocity = true;
    public float minVelocity = 3f;
    public float maxVelocity = 8f;
    public float minMass = 0.5f;
    public float maxMass = 2f;
    public float minBounciness = 0.5f;
    public float maxBounciness = 1.2f;
    public float minGravityScale = 0.5f;
    public float maxGravityScale = 1.5f;
    
    [Header("Direction Settings")]
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
    [ReadOnly] public float spawnTimer = 0f;
    [ReadOnly] public int totalSpawned = 0;

    private void Start()
    {
        // Register with SpawnerController
        if (SpawnerController.Instance != null)
        {
            SpawnerController.Instance.RegisterSpawner(this);
        }
        
        if (spawnOnStart)
        {
            SpawnObject();
        }
        
        // Set initial random interval if using random timing
        if (useRandomInterval)
        {
            SetRandomInterval();
        }
    }

    private void OnDestroy()
    {
        // Unregister from SpawnerController
        if (SpawnerController.Instance != null)
        {
            SpawnerController.Instance.UnregisterSpawner(this);
        }
    }

    private void Update()
    {
        if (!isEnabled || !isTimed) return;
        
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval)
        {
            SpawnObject();
            spawnTimer = 0f; // Reset timer after spawning
            
            // Set new random interval if using random timing
            if (useRandomInterval)
            {
                SetRandomInterval();
            }
        }
    }

    public void SpawnObject()
    {
        if (ObjectPrefab == null)
        {
            Debug.LogWarning($"Spawner: ObjectPrefab is null on {gameObject.name}");
            return;
        }
        
        var newObject = Instantiate(ObjectPrefab, transform.position, Quaternion.identity, gameObject.transform);
        newObject.name = $"{ObjectPrefab.name}_{totalSpawned}"; // Add count for uniqueness
        totalSpawned++;
        
        // Configure the spawned object's movement
        ConfigureSpawnedObject(newObject);
    }

    /// <summary>
    /// Configures the physics properties for a spawned object
    /// </summary>
    private void ConfigureSpawnedObject(GameObject spawnedObject)
    {
        BallMovement ballMovement = spawnedObject.GetComponent<BallMovement>();
        
        if (ballMovement == null)
        {
            Debug.LogWarning($"Spawner: No BallMovement component found on {spawnedObject.name}");
            return;
        }
        
        // Use SpawnerController settings if available and enabled
        if (useSpawnerControllerSettings && SpawnerController.Instance != null)
        {
            // SpawnerController will handle the configuration
            SpawnerController.Instance.ConfigureBall(spawnedObject);
        }
        else
        {
            // Use local spawner settings
            ConfigureWithLocalSettings(ballMovement);
        }
        
        Debug.Log($"Spawner: Spawned {spawnedObject.name} with physics-based movement");
    }

    /// <summary>
    /// Configures BallMovement using local spawner settings
    /// </summary>
    private void ConfigureWithLocalSettings(BallMovement ballMovement)
    {
        // Generate random properties
        Vector2 direction = GetRandomDirection();
        float velocity = randomizeVelocity ? Random.Range(minVelocity, maxVelocity) : minVelocity;
        float mass = Random.Range(minMass, maxMass);
        float bounciness = Random.Range(minBounciness, maxBounciness);
        float gravityScale = Random.Range(minGravityScale, maxGravityScale);

        // Initialize the ball
        ballMovement.Initialize(ballMovement.transform.position, direction, velocity, mass, bounciness, gravityScale);
    }

    /// <summary>
    /// Get a random direction from preferred directions with variance
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
    /// Sets a random spawn interval between min and max
    /// </summary>
    private void SetRandomInterval()
    {
        spawnInterval = Random.Range(minInterval, maxInterval);
    }

    /// <summary>
    /// Resets the spawner
    /// </summary>
    public void ResetSpawner()
    {
        spawnTimer = 0f;
        totalSpawned = 0;
        if (useRandomInterval)
        {
            SetRandomInterval();
        }
    }

    /// <summary>
    /// Manually trigger a spawn
    /// </summary>
    public void TriggerSpawn()
    {
        SpawnObject();
    }

    private void OnDrawGizmos()
    {
        // Draw spawner position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Draw preferred directions
        if (preferredDirections.Length > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector2 direction in preferredDirections)
            {
                Gizmos.DrawRay(transform.position, direction * 1f);
            }
        }
    }
}