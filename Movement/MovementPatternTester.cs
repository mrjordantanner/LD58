using UnityEngine;

/// <summary>
/// Simple tester script to demonstrate the physics-based ball movement
/// </summary>
public class MovementPatternTester : MonoBehaviour
{
    [Header("Test Settings")]
    public GameObject ballPrefab;
    public Transform spawnPoint;
    public float testInterval = 2f;
    
    [Header("Physics Variation")]
    public float minVelocity = 3f;
    public float maxVelocity = 8f;
    public float minMass = 0.5f;
    public float maxMass = 2f;
    public float minBounciness = 0.5f;
    public float maxBounciness = 1.2f;
    
    [Header("Direction Settings")]
    public Vector2[] testDirections = {
        Vector2.right,
        Vector2.left,
        Vector2.up,
        Vector2.down,
        new Vector2(1, 1).normalized,
        new Vector2(-1, 1).normalized,
        new Vector2(1, -1).normalized,
        new Vector2(-1, -1).normalized
    };
    public float directionVariance = 30f;
    
    private float timer = 0f;
    private int currentDirectionIndex = 0;
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= testInterval)
        {
            SpawnTestBall();
            timer = 0f;
        }
    }
    
    private void SpawnTestBall()
    {
        if (ballPrefab == null || spawnPoint == null) return;
        
        // Spawn ball
        GameObject testBall = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        testBall.name = $"TestBall_{Time.time:F1}";
        
        // Get BallMovement component
        BallMovement ballMovement = testBall.GetComponent<BallMovement>();
        if (ballMovement != null)
        {
            // Generate random properties
            Vector2 direction = GetRandomDirection();
            float velocity = Random.Range(minVelocity, maxVelocity);
            float mass = Random.Range(minMass, maxMass);
            float bounciness = Random.Range(minBounciness, maxBounciness);

            // Initialize the ball
            ballMovement.Initialize(testBall.transform.position, direction, velocity, mass, bounciness);
            
            Debug.Log($"Spawned test ball with velocity: {velocity}, mass: {mass}, bounciness: {bounciness}");
        }
        
        // Move to next direction
        currentDirectionIndex = (currentDirectionIndex + 1) % testDirections.Length;
    }
    
    private Vector2 GetRandomDirection()
    {
        if (testDirections.Length == 0)
        {
            // If no test directions, use completely random
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        
        // Pick a random test direction
        Vector2 baseDirection = testDirections[currentDirectionIndex];
        
        // Add random variance
        float randomAngle = Random.Range(-directionVariance, directionVariance);
        return Quaternion.AngleAxis(randomAngle, Vector3.forward) * baseDirection;
    }
    
    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
        }
        
        // Draw test directions
        if (testDirections.Length > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector2 direction in testDirections)
            {
                Gizmos.DrawRay(spawnPoint.position, direction * 1f);
            }
        }
    }
}