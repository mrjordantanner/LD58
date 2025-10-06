using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [Header("Physics Settings")]
    public float bounceForce = 1f;
    public float bounceDamping = 0.8f;
    public float minVelocity = 0.1f;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color gizmoColor = Color.yellow;
    
    // Private variables
    private Rigidbody2D rb;
    private bool isInitialized = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure we have a dynamic rigidbody
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Set up physics properties
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f; // Enable gravity
        rb.drag = 0f; // No air resistance
        rb.angularDrag = 0f; // No angular resistance
        
        // Add physics material for bouncing
        PhysicsMaterial2D bounceMaterial = new PhysicsMaterial2D("BallBounce");
        bounceMaterial.bounciness = bounceForce;
        bounceMaterial.friction = 0f;
        
        // Apply to collider if it exists
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.sharedMaterial = bounceMaterial;
        }
    }
    
    private void Start()
    {
        // Ball will be configured by SpawnerController
        isInitialized = true;
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        // Check if ball has stopped moving (very low velocity)
        if (rb.velocity.magnitude < minVelocity)
        {
            // Apply slight damping to prevent infinite tiny movements
            rb.velocity *= 0.99f;
        }
    }
    
    /// <summary>
    /// Initialize the ball with random physics properties and launch it
    /// </summary>
    public void Initialize(Vector2 spawnPosition, Vector2 direction, float velocity, float mass = 1f, float bounciness = 1f, float gravityScale = 1f)
    {
        // Set position
        transform.position = spawnPosition;
        
        // Set physics properties
        rb.mass = mass;
        rb.gravityScale = gravityScale;
        
        // Update bounce material
        PhysicsMaterial2D bounceMaterial = new PhysicsMaterial2D("BallBounce");
        bounceMaterial.bounciness = bounciness;
        bounceMaterial.friction = Random.Range(0f, 0.2f); // Small random friction
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.sharedMaterial = bounceMaterial;
        }
        
        // Launch the ball
        rb.velocity = direction.normalized * velocity;
        
        // Add some random angular velocity for more interesting movement
        rb.angularVelocity = Random.Range(-180f, 180f);
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Apply a force to the ball
    /// </summary>
    public void ApplyForce(Vector2 force)
    {
        if (rb != null)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
    
    /// <summary>
    /// Stop the ball's movement
    /// </summary>
    public void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
    
    /// <summary>
    /// Get the current velocity
    /// </summary>
    public Vector2 GetVelocity()
    {
        return rb != null ? rb.velocity : Vector2.zero;
    }
    
    /// <summary>
    /// Check if the ball is moving
    /// </summary>
    public bool IsMoving()
    {
        return rb != null && rb.velocity.magnitude > minVelocity;
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw velocity vector
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rb.velocity.normalized * 2f);
        }
        
        // Draw ball center
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Play bump sound when ball hits obstacles
        if (collision.gameObject.CompareTag("Obstacles"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("Bump");
            }
        }
    }
    
    private void OnDestroy()
    {
        StopMovement();
    }
}