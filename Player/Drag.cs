using UnityEngine;

public class Drag : MonoBehaviour
{
    [Header("Drag Settings")]
    public bool enableDrag = true;
    public float dragForce = 10f;
    public float maxDragForce = 50f;
    public float dragDamping = 0.8f;
    
    [Header("Weight & Physics")]
    public float weight = 1f;
    public float massMultiplier = 1f;
    public bool usePhysicsWeight = true;
    public float physicsDrag = 2f;
    public float angularDrag = 5f;
    
    [Header("Deceleration")]
    public float decelerationForce = 15f;
    public float decelerationDamping = 0.9f;
    public bool useDeceleration = true;
    
    [Header("Mouse Settings")]
    public float mouseSensitivity = 1f;
    public float maxDragDistance = 10f;
    public LayerMask dragLayerMask = -1;
    
    [Header("Visual Feedback")]
    public bool showDragLine = true;
    public Color dragLineColor = Color.yellow;
    public float dragLineWidth = 0.1f;
    
    [Header("State")]
    [ReadOnly] public bool isDragging = false;
    [ReadOnly] public Vector2 dragOffset;
    [ReadOnly] public Vector2 targetPosition;
    [ReadOnly] public float currentDragForce;

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
        if (showDragLine)
        {
            CreateDragLine();
        }
    }

    private void Update()
    {
        if (!enableDrag) return;

        HandleMouseInput();
        UpdateDragPhysics();
        UpdateDragLine();
        
        // Apply continuous deceleration if not dragging and moving
        if (!isDragging && useDeceleration && rb != null && rb.velocity.magnitude > 0.1f)
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
        
        if (distance <= maxDragDistance)
        {
            // Check layer mask
            if (IsInDragLayer())
            {
                isDragging = true;
                dragOffset = (Vector2)transform.position - mousePos;
                lastMousePosition = mousePos;
                
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
        
        if (distance > 0.1f) // Small threshold to prevent jitter
        {
            // Calculate drag force based on distance and weight
            currentDragForce = Mathf.Min(dragForce * distance * mouseSensitivity, maxDragForce);
            
            // Apply weight multiplier
            if (usePhysicsWeight)
            {
                currentDragForce *= weight * massMultiplier;
            }
            
            // Apply force towards target
            Vector2 force = direction.normalized * currentDragForce;
            rb.AddForce(force);
            
            // Apply damping to prevent oscillation
            rb.velocity *= dragDamping;
        }
    }

    private void ApplyWeightEffects()
    {
        if (!usePhysicsWeight) return;
        
        // Increase mass based on weight
        rb.mass = originalMass * weight * massMultiplier;
        
        // Apply drag for resistance
        rb.drag = physicsDrag;
        rb.angularDrag = angularDrag;
        
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
        if (!useDeceleration || rb == null) return;
        
        // Apply opposing force to current velocity
        Vector2 opposingForce = -rb.velocity.normalized * decelerationForce;
        rb.AddForce(opposingForce);
        
        // Apply additional damping
        rb.velocity *= decelerationDamping;
        
        // Ensure we don't overshoot and create oscillation
        if (rb.velocity.magnitude < 0.5f)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void ApplyContinuousDeceleration()
    {
        if (!useDeceleration || rb == null) return;
        
        // Apply gentle opposing force
        Vector2 opposingForce = -rb.velocity.normalized * (decelerationForce * 0.5f);
        rb.AddForce(opposingForce);
        
        // Apply damping
        rb.velocity *= decelerationDamping;
        
        // Stop if velocity is very low
        if (rb.velocity.magnitude < 0.1f)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private bool IsInDragLayer()
    {
        // Simple layer check - you can expand this for more complex detection
        return (dragLayerMask.value & (1 << gameObject.layer)) != 0;
    }

    private void CreateDragLine()
    {
        GameObject lineObj = new GameObject("DragLine");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        
        dragLine = lineObj.AddComponent<LineRenderer>();
        dragLine.material = new Material(Shader.Find("Sprites/Default"));
        //dragLine.color = dragLineColor;
        dragLine.startWidth = dragLineWidth;
        dragLine.endWidth = dragLineWidth;
        dragLine.positionCount = 2;
        dragLine.enabled = false;
        dragLine.sortingOrder = 10; // Render on top
    }

    private void UpdateDragLine()
    {
        if (dragLine == null) return;
        
        if (isDragging && showDragLine)
        {
            dragLine.enabled = true;
            dragLine.SetPosition(0, transform.position);
            dragLine.SetPosition(1, mouseWorldPosition);
            
            // Update line color based on drag force
            Color lineColor = dragLineColor;
            lineColor.a = Mathf.Clamp01(currentDragForce / maxDragForce);
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
        if (enableDrag)
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
        weight = Mathf.Max(0.1f, newWeight);
        
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
        enableDrag = enabled;
        
        // If disabling while dragging, stop the drag
        if (!enabled && isDragging)
        {
            EndDrag();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!enableDrag) return;
        
        // Draw drag range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxDragDistance);
        
        // Draw drag line if dragging
        if (isDragging && Application.isPlaying)
        {
            Gizmos.color = dragLineColor;
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
