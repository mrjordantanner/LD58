using UnityEngine;

public class PhysicsDrag : MonoBehaviour
{
    // Drag parameters are managed by PlayerManager.Instance
    
    [Header("State")]
    [ReadOnly] public bool isDragging = false;
    [ReadOnly] public Vector2 dragOffset;
    [ReadOnly] public Vector2 targetPosition;
    [ReadOnly] public float currentDragForce;
    [ReadOnly] public bool isAtRest = false;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private Vector2 mouseWorldPosition;
    private Vector2 lastMousePosition;
    private LineRenderer dragLine;
    
    // Store original physics values
    private float originalMass;
    private float originalDrag;
    private float originalAngularDrag;
    private bool wasKinematic;
    private float originalGravityScale;
    
    // Physics state tracking
    private bool isTransitioning = false;
    private float transitionTimer = 0f;
    private Vector2 lastVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        if (rb == null)
        {
            Debug.LogError($"PhysicsDrag: No Rigidbody2D found on {name}. PhysicsDrag component requires a Rigidbody2D.");
            enabled = false;
            return;
        }
        
        // Store original physics values
        originalMass = rb.mass;
        originalDrag = rb.drag;
        originalAngularDrag = rb.angularDrag;
        wasKinematic = rb.isKinematic;
        originalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (!PlayerManager.Instance.enableDrag) return;

        HandleMouseInput();
        UpdateDragPhysics();

        // Apply continuous deceleration if not dragging and moving
        if (!isDragging && PlayerManager.Instance.useDeceleration && rb != null && rb.velocity.magnitude > 0.1f)
        {
            ApplyContinuousDeceleration();
        }
    }

    private void FixedUpdate()
    {
        // Handle physics-based dragging in FixedUpdate for better physics integration
        if (isDragging && !isTransitioning)
        {
            ApplyPhysicsDrag();
        }
    }

    private void HandleMouseInput()
    {
        // Don't allow dragging when input is suspended
        if (GameManager.Instance.ShouldSuspendInput())
        {
            return;
        }

        // Get mouse world position
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // Check for mouse down
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        // Check for mouse up
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        // Check if mouse is over this object
        Vector2 mousePos = mouseWorldPosition;
        float distance = Vector2.Distance(mousePos, transform.position);
        
        if (distance <= PlayerManager.Instance.maxDragDistance)
        {
            // Check layer mask
            if (IsInDragLayer())
            {
                isDragging = true;
                dragOffset = (Vector2)transform.position - mousePos;
                lastMousePosition = mousePos;
                isAtRest = false; // Reset rest state when starting drag
                
                // Transition to dynamic physics
                StartCoroutine(TransitionToDynamic());
            }
        }
    }

    private void EndDrag()
    {
        if (isDragging)
        {
            isDragging = false;
            isAtRest = false; // Reset rest state when ending drag
            
            // Apply deceleration to bring object to rest
            ApplyDeceleration();
            
            // Transition back to kinematic
            StartCoroutine(TransitionToKinematic());
        }
    }

    private System.Collections.IEnumerator TransitionToDynamic()
    {
        isTransitioning = true;
        transitionTimer = 0f;
        
        // Store current velocity for smooth transition
        lastVelocity = rb.velocity;
        
        while (transitionTimer < PlayerManager.Instance.physicsKinematicTransitionTime)
        {
            transitionTimer += Time.deltaTime;
            float t = transitionTimer / PlayerManager.Instance.physicsKinematicTransitionTime;
            
            // Gradually transition physics properties
            rb.isKinematic = false;
            rb.gravityScale = Mathf.Lerp(0f, originalGravityScale, t);
            rb.drag = Mathf.Lerp(originalDrag, PlayerManager.Instance.physicsDrag, t);
            rb.angularDrag = Mathf.Lerp(originalAngularDrag, PlayerManager.Instance.angularDrag, t);
            
            // Apply weight effects gradually
            if (PlayerManager.Instance.usePhysicsWeight)
            {
                rb.mass = Mathf.Lerp(originalMass, originalMass * PlayerManager.Instance.weight * PlayerManager.Instance.massMultiplier, t);
            }
            
            yield return null;
        }
        
        // Ensure final state
        rb.isKinematic = false;
        rb.gravityScale = originalGravityScale;
        rb.drag = PlayerManager.Instance.physicsDrag;
        rb.angularDrag = PlayerManager.Instance.angularDrag;
        
        if (PlayerManager.Instance.usePhysicsWeight)
        {
            rb.mass = originalMass * PlayerManager.Instance.weight * PlayerManager.Instance.massMultiplier;
        }
        
        isTransitioning = false;
    }

    private System.Collections.IEnumerator TransitionToKinematic()
    {
        isTransitioning = true;
        transitionTimer = 0f;
        
        // Store current velocity for smooth transition
        lastVelocity = rb.velocity;
        
        while (transitionTimer < PlayerManager.Instance.physicsKinematicTransitionTime)
        {
            transitionTimer += Time.deltaTime;
            float t = transitionTimer / PlayerManager.Instance.physicsKinematicTransitionTime;
            
            // Gradually reduce physics influence
            rb.drag = Mathf.Lerp(PlayerManager.Instance.physicsDrag, originalDrag, t);
            rb.angularDrag = Mathf.Lerp(PlayerManager.Instance.angularDrag, originalAngularDrag, t);
            rb.gravityScale = Mathf.Lerp(originalGravityScale, 0f, t);
            
            // Restore original mass gradually
            if (PlayerManager.Instance.usePhysicsWeight)
            {
                rb.mass = Mathf.Lerp(originalMass * PlayerManager.Instance.weight * PlayerManager.Instance.massMultiplier, originalMass, t);
            }
            
            yield return null;
        }
        
        // Final transition to kinematic
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true; // Force kinematic state
        rb.gravityScale = 0f;
        rb.drag = originalDrag;
        rb.angularDrag = originalAngularDrag;
        rb.mass = originalMass;
        
        isTransitioning = false;
    }

    private void UpdateDragPhysics()
    {
        if (!isDragging || rb == null) return;

        // Calculate target position
        targetPosition = mouseWorldPosition + dragOffset;
        
        // Calculate direction and distance to target
        Vector2 direction = (targetPosition - (Vector2)transform.position);
        float distance = direction.magnitude;
        
        // Check if object is within rest threshold
        if (distance <= PlayerManager.Instance.restThreshold)
        {
            // Object is at rest - stop all movement
            isAtRest = true;
            currentDragForce = 0f;
            
            // Stop the rigidbody completely
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            // Object is not at rest - prepare for physics application
            isAtRest = false;
            
            // Calculate drag force based on distance and weight
            currentDragForce = Mathf.Min(PlayerManager.Instance.dragForce * distance * PlayerManager.Instance.mouseSensitivity, PlayerManager.Instance.maxDragForce);
            
            // Apply weight multiplier
            if (PlayerManager.Instance.usePhysicsWeight)
            {
                currentDragForce *= PlayerManager.Instance.weight * PlayerManager.Instance.massMultiplier;
            }
        }
    }

    private void ApplyPhysicsDrag()
    {
        if (!isDragging || rb == null || isTransitioning) return;

        // Calculate direction to target
        Vector2 direction = (targetPosition - (Vector2)transform.position);
        float distance = direction.magnitude;
        
        if (distance > PlayerManager.Instance.restThreshold)
        {
            // Apply physics-based following with slight weight resistance
            Vector2 followForce = direction.normalized * PlayerManager.Instance.physicsFollowStrength * distance;
            
            // Add weight resistance for challenge
            followForce *= (1f - PlayerManager.Instance.physicsWeightResistance);
            
            // Clamp force to prevent excessive acceleration
            followForce = Vector2.ClampMagnitude(followForce, PlayerManager.Instance.physicsMaxFollowForce);
            
            // Apply the force
            rb.AddForce(followForce);
            
            // Apply damping to prevent oscillation
            rb.velocity *= PlayerManager.Instance.physicsFollowDamping;
            
            // Apply additional weight-based resistance
            if (PlayerManager.Instance.usePhysicsWeight)
            {
                Vector2 resistance = -rb.velocity * PlayerManager.Instance.physicsWeightResistance * PlayerManager.Instance.weight;
                rb.AddForce(resistance);
            }
        }
    }

    private void ApplyDeceleration()
    {
        if (!PlayerManager.Instance.useDeceleration || rb == null) return;
        
        // Apply opposing force to current velocity
        Vector2 opposingForce = -rb.velocity.normalized * PlayerManager.Instance.decelerationForce;
        rb.AddForce(opposingForce);
        
        // Apply additional damping
        rb.velocity *= PlayerManager.Instance.decelerationDamping;
        
        // Ensure we don't overshoot and create oscillation
        if (rb.velocity.magnitude < 0.5f)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void ApplyContinuousDeceleration()
    {
        if (!PlayerManager.Instance.useDeceleration || rb == null) return;
        
        // Apply gentle opposing force
        Vector2 opposingForce = -rb.velocity.normalized * (PlayerManager.Instance.decelerationForce * 0.5f);
        rb.AddForce(opposingForce);
        
        // Apply damping
        rb.velocity *= PlayerManager.Instance.decelerationDamping;
        
        // Stop if velocity is very low
        if (rb.velocity.magnitude < 0.1f)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private bool IsInDragLayer()
    {
        // Simple layer check - you can expand this for more complex detection
        return (PlayerManager.Instance.dragLayerMask.value & (1 << gameObject.layer)) != 0;
    }

    /// <summary>
    /// Force start dragging (useful for programmatic control)
    /// </summary>
    public void ForceStartDrag()
    {
        if (PlayerManager.Instance.enableDrag)
        {
            StartDrag();
        }
    }

    /// <summary>
    /// Force stop dragging (useful for programmatic control)
    /// </summary>
    public void ForceEndDrag()
    {
        EndDrag();
    }

    /// <summary>
    /// Set the weight of the object (affects drag resistance)
    /// </summary>
    public void SetWeight(float newWeight)
    {
        PlayerManager.Instance.weight = Mathf.Max(0.1f, newWeight);
        
        // If currently dragging, update physics
        if (isDragging && !isTransitioning)
        {
            if (PlayerManager.Instance.usePhysicsWeight)
            {
                rb.mass = originalMass * PlayerManager.Instance.weight * PlayerManager.Instance.massMultiplier;
            }
        }
    }

    /// <summary>
    /// Enable or disable dragging
    /// </summary>
    public void SetDragEnabled(bool enabled)
    {
        PlayerManager.Instance.enableDrag = enabled;
        
        // If disabling while dragging, stop the drag
        if (!enabled && isDragging)
        {
            EndDrag();
        }
    }

    /// <summary>
    /// Get the current drag state
    /// </summary>
    public bool IsDragging()
    {
        return isDragging;
    }

    /// <summary>
    /// Get the current transition state
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    private void OnDrawGizmosSelected()
    {
        if (!PlayerManager.Instance.enableDrag) return;
        
        // Draw drag range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, PlayerManager.Instance.maxDragDistance);
    }

    private void OnDestroy()
    {
        // Stop any running coroutines
        StopAllCoroutines();
    }
}
