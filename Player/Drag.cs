using UnityEngine;

public class Drag : MonoBehaviour
{
    // Drag parameters are now managed by PlayerManager.Instance
    // This allows for easy tweaking during play
    
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
    private float originalMass;
    private float originalDrag;
    private float originalAngularDrag;
    private bool wasKinematic;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        if (rb == null)
        {
            Debug.LogError($"Drag: No Rigidbody2D found on {name}. Drag component requires a Rigidbody2D.");
            enabled = false;
            return;
        }
        
        // Store original physics values
        originalMass = rb.mass;
        originalDrag = rb.drag;
        originalAngularDrag = rb.angularDrag;
        wasKinematic = rb.isKinematic;
        
        // Create drag line if enabled
        if (PlayerManager.Instance.showDragLine)
        {
            CreateDragLine();
        }
    }

    private void Update()
    {
        if (!PlayerManager.Instance.enableDrag) return;

        HandleMouseInput();
        UpdateDragPhysics();
        UpdateDragLine();
        
        // Apply continuous deceleration if not dragging and moving
        if (!isDragging && PlayerManager.Instance.useDeceleration && rb != null && rb.velocity.magnitude > 0.1f)
        {
            ApplyContinuousDeceleration();
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
                
                // Apply weight effects
                ApplyWeightEffects();
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
            
            // Restore original physics
            RestorePhysics();
        }
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
            // Object is not at rest - apply forces
            isAtRest = false;
            
            // Calculate drag force based on distance and weight
            currentDragForce = Mathf.Min(PlayerManager.Instance.dragForce * distance * PlayerManager.Instance.mouseSensitivity, PlayerManager.Instance.maxDragForce);
            
            // Apply weight multiplier
            if (PlayerManager.Instance.usePhysicsWeight)
            {
                currentDragForce *= PlayerManager.Instance.weight * PlayerManager.Instance.massMultiplier;
            }
            
            // Apply force towards target
            Vector2 force = direction.normalized * currentDragForce;
            rb.AddForce(force);
            
            // Apply damping to prevent oscillation
            rb.velocity *= PlayerManager.Instance.dragDamping;
        }
    }

    private void ApplyWeightEffects()
    {
        if (!PlayerManager.Instance.usePhysicsWeight) return;
        
        // Increase mass based on weight
        rb.mass = originalMass * PlayerManager.Instance.weight * PlayerManager.Instance.massMultiplier;
        
        // Apply drag for resistance
        rb.drag = PlayerManager.Instance.physicsDrag;
        rb.angularDrag = PlayerManager.Instance.angularDrag;
        
        // Make sure it's not kinematic for physics to work
        rb.isKinematic = false;
    }

    private void RestorePhysics()
    {
        // Restore original physics values
        rb.mass = originalMass;
        rb.drag = originalDrag;
        rb.angularDrag = originalAngularDrag;
        rb.isKinematic = wasKinematic;
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

    private void CreateDragLine()
    {
        GameObject lineObj = new GameObject("DragLine");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        
        dragLine = lineObj.AddComponent<LineRenderer>();
        dragLine.material = new Material(Shader.Find("Sprites/Default"));
        //dragLine.color = PlayerManager.Instance.dragLineColor;
        dragLine.startWidth = PlayerManager.Instance.dragLineWidth;
        dragLine.endWidth = PlayerManager.Instance.dragLineWidth;
        dragLine.positionCount = 2;
        dragLine.enabled = false;
        dragLine.sortingOrder = 10; // Render on top
    }

    private void UpdateDragLine()
    {
        if (dragLine == null) return;
        
        if (isDragging && PlayerManager.Instance.showDragLine)
        {
            dragLine.enabled = true;
            dragLine.SetPosition(0, transform.position);
            dragLine.SetPosition(1, mouseWorldPosition);
            
            // Update line color based on drag force and rest state
            Color lineColor = PlayerManager.Instance.dragLineColor;
            if (isAtRest)
            {
                lineColor = Color.green; // Green when at rest
                lineColor.a = 0.5f;
            }
            else
            {
                lineColor.a = Mathf.Clamp01(currentDragForce / PlayerManager.Instance.maxDragForce);
            }
            //dragLine.color = lineColor;
        }
        else
        {
            dragLine.enabled = false;
        }
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
        if (isDragging)
        {
            ApplyWeightEffects();
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

    private void OnDrawGizmosSelected()
    {
        if (!PlayerManager.Instance.enableDrag) return;
        
        // Draw drag range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, PlayerManager.Instance.maxDragDistance);
        
        // Draw drag line if dragging
        if (isDragging && Application.isPlaying)
        {
            Gizmos.color = PlayerManager.Instance.dragLineColor;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }

    private void OnDestroy()
    {
        // Clean up drag line
        if (dragLine != null && dragLine.gameObject != null)
        {
            Destroy(dragLine.gameObject);
        }
    }
}
