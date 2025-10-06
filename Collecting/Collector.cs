using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Collector : MonoBehaviour
{
    [Header("Expansion Settings")]
    public float expandDistance = 1.0f; // How far each quadrant moves outward
    public float expandDuration = 0.2f;
    public float stayOpenDuration = 0.3f;
    public float contractDuration = 0.2f;
    
    [Header("Collection Window")]
    public float preCollectionWindow = 0.1f;
    public float postCollectionWindow = 0.15f;
    
    [Header("Overlap Detection")]
    [Range(0.1f, 1.0f)]
    public float overlapThreshold = 0.7f; // How much of the collectible must be inside (0.7 = 70%)
    public bool useOverlapDetection = true;
    public bool usePreciseDetection = false; // Use sampling method instead of bounds calculation
    [Range(10, 50)]
    public int samplePoints = 20; // Number of points to sample for precise detection
    
    [Header("Visual Effects")]
    public bool useRotation = true;
    public float expandRotationSpeed = 180f;
    public float contractRotationSpeed = 720f;
    public bool useColorChange = true;
    public Color expandColor = Color.clear;
    public Color contractColor = Color.clear;
    public Color originalColor = Color.clear;
    
    [Header("Shake Effects")]
    public bool useShakeOnClose = true;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.15f;
    public int shakeVibrato = 10;
    
    [Header("Quadrant References")]
    public Transform topLeftQuadrant;
    public Transform topRightQuadrant;
    public Transform bottomLeftQuadrant;
    public Transform bottomRightQuadrant;

    [Header("State")]
    [ReadOnly] public bool isExpanded = false;
    [ReadOnly] public bool isExpanding = false;
    [ReadOnly] public bool isHolding = false;

    public GameObject GraphicsObject;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private SpriteRenderer graphicsSpriteRenderer;
    private Tween expansionTween;
    private Tween shakeTween;
    private Coroutine collectionWindowCoroutine;
    public Collider2D coll;

    // Store original positions of quadrants
    private Vector3 topLeftOriginalPos;
    private Vector3 topRightOriginalPos;
    private Vector3 bottomLeftOriginalPos;
    private Vector3 bottomRightOriginalPos;

    // Track collection success during window
    private bool collectionSuccessful = false;

    // Cached Drag component reference
    private Drag playerDragComponent;

    /// <summary>
    /// Checks if there's a ball in play (exists and is moving)
    /// </summary>
    private bool IsBallInPlay()
    {
        // Find all balls in the scene
        BallMovement[] allBalls = FindObjectsOfType<BallMovement>();
        
        // Check if any ball is moving (in play)
        foreach (BallMovement ball in allBalls)
        {
            if (ball != null && ball.IsMoving())
            {
                return true;
            }
        }
        
        return false;
    }

    private void Start()
    {
        originalScale = GraphicsObject != null ? GraphicsObject.transform.localScale : transform.localScale;
        originalRotation = transform.rotation;

        coll = GetComponent<Collider2D>();
        
        // Disable collider initially
        if (coll != null)
        {
            coll.enabled = false;
        }
        
        // If no GraphicsObject is assigned, use this object
        if (GraphicsObject == null)
        {
            GraphicsObject = gameObject;
        }
        
        // Store original positions of quadrants
        StoreQuadrantOriginalPositions();
        
        // Get the SpriteRenderer component for color changes
        graphicsSpriteRenderer = GraphicsObject.GetComponent<SpriteRenderer>();
        if (graphicsSpriteRenderer == null)
        {
            Debug.LogWarning($"Collector: No SpriteRenderer found on GraphicsObject {GraphicsObject.name}");
        }
        
        // Set initial color
        if (graphicsSpriteRenderer != null)
        {
            graphicsSpriteRenderer.color = GetActualColor(originalColor, "primaryLight");
        }

        // Cache the Drag component reference from parent
        playerDragComponent = GetComponentInParent<Drag>();
    }

    private void Update()
    {
        // Check if input should be suspended
        if (GameManager.Instance.ShouldSuspendInput()) return;

        bool isPressed = Input.GetKey(InputManager.Instance.shootButton) || 
                        Input.GetKey(InputManager.Instance.shootKey);

        // Check if player is currently dragging (only allow collector to open when dragging)
        bool isPlayerDragging = playerDragComponent != null && playerDragComponent.isDragging;

        // On button press - start opening if not already open/opening AND player is dragging
        if (isPressed && !isHolding && !isExpanding && !isExpanded && isPlayerDragging)
        {
            isHolding = true;
            StartExpansion();
        }
        // On button release - start closing if currently open
        else if (!isPressed && isHolding && isExpanded && !isExpanding)
        {
            isHolding = false;
            StartContraction();
        }
    }

    /// <summary>
    /// Stores the original positions of all quadrants
    /// </summary>
    private void StoreQuadrantOriginalPositions()
    {
        if (topLeftQuadrant != null)
            topLeftOriginalPos = topLeftQuadrant.localPosition;
        if (topRightQuadrant != null)
            topRightOriginalPos = topRightQuadrant.localPosition;
        if (bottomLeftQuadrant != null)
            bottomLeftOriginalPos = bottomLeftQuadrant.localPosition;
        if (bottomRightQuadrant != null)
            bottomRightOriginalPos = bottomRightQuadrant.localPosition;
    }

    /// <summary>
    /// Starts the expansion sequence using DoTween
    /// </summary>
    private void StartExpansion()
    {
        // Kill any existing tween
        if (expansionTween != null && expansionTween.IsActive())
        {
            expansionTween.Kill();
        }
        
        isExpanding = true;
        isExpanded = false;

        AudioManager.Instance.PlaySound("Open-1");
        
        // Create the expansion sequence - move quadrants outward
        expansionTween = DOTween.Sequence()
            // Move quadrants diagonally outward
            .Join(topLeftQuadrant != null ? 
                topLeftQuadrant.DOLocalMove(topLeftOriginalPos + new Vector3(-expandDistance, expandDistance, 0), expandDuration)
                .SetEase(Ease.OutQuad) : null)
            .Join(topRightQuadrant != null ? 
                topRightQuadrant.DOLocalMove(topRightOriginalPos + new Vector3(expandDistance, expandDistance, 0), expandDuration)
                .SetEase(Ease.OutQuad) : null)
            .Join(bottomLeftQuadrant != null ? 
                bottomLeftQuadrant.DOLocalMove(bottomLeftOriginalPos + new Vector3(-expandDistance, -expandDistance, 0), expandDuration)
                .SetEase(Ease.OutQuad) : null)
            .Join(bottomRightQuadrant != null ? 
                bottomRightQuadrant.DOLocalMove(bottomRightOriginalPos + new Vector3(expandDistance, -expandDistance, 0), expandDuration)
                .SetEase(Ease.OutQuad) : null)
            // Add rotation and color effects during expansion
            .Join(useRotation && GraphicsObject != null ? 
                GraphicsObject.transform.DORotate(new Vector3(0, 0, expandRotationSpeed), expandDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad) : null)
            .Join(useColorChange && graphicsSpriteRenderer != null ? 
                graphicsSpriteRenderer.DOColor(GetActualColor(expandColor, "primaryLightTint"), expandDuration)
                .SetEase(Ease.OutQuad) : null)
            // Set expanded state and stay open
            .AppendCallback(() => {
                isExpanded = true;
                isExpanding = false;
                expansionTween = null;
            });
    }

    /// <summary>
    /// Starts the contraction sequence and collection window
    /// </summary>
    private void StartContraction()
    {
        // Kill any existing tween
        if (expansionTween != null && expansionTween.IsActive())
        {
            expansionTween.Kill();
        }
        
        isExpanding = true;
        isExpanded = false;

        // Start collection window immediately when closing begins
        StartCollectionWindow();

        AudioManager.Instance.PlaySound("Shut-1");

        // Flash player sprites to accent color during contraction
        FlashPlayerSpritesToAccentColor();

        // Create the contraction sequence
        expansionTween = DOTween.Sequence()
            // Move quadrants back to original positions
            .Join(topLeftQuadrant != null ? 
                topLeftQuadrant.DOLocalMove(topLeftOriginalPos, contractDuration)
                .SetEase(Ease.InQuad) : null)
            .Join(topRightQuadrant != null ? 
                topRightQuadrant.DOLocalMove(topRightOriginalPos, contractDuration)
                .SetEase(Ease.InQuad) : null)
            .Join(bottomLeftQuadrant != null ? 
                bottomLeftQuadrant.DOLocalMove(bottomLeftOriginalPos, contractDuration)
                .SetEase(Ease.InQuad) : null)
            .Join(bottomRightQuadrant != null ? 
                bottomRightQuadrant.DOLocalMove(bottomRightOriginalPos, contractDuration)
                .SetEase(Ease.InQuad) : null)
            // Add fast rotation with elastic easing for extra elasticity and snappiness
            .Join(useRotation && GraphicsObject != null ? 
                GraphicsObject.transform.DORotate(new Vector3(0, 0, contractRotationSpeed), contractDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutElastic) : null)
            .Join(useColorChange && graphicsSpriteRenderer != null ? 
                graphicsSpriteRenderer.DOColor(GetActualColor(contractColor, "accent"), contractDuration)
                .SetEase(Ease.InQuad) : null)
            // Clean up and reset visual effects
            .AppendCallback(() => {
                ShakeGraphicsObject();
                ResetVisualEffects();
            })
            .OnComplete(() => {
                isExpanding = false;
                expansionTween = null;
            });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Don't collect while expanded - only during closing animation
        if (isExpanded)
        {
            return;
        }
        
        if (collision.CompareTag("Collectible"))
        {
            // Check if we should collect this item based on overlap detection
            if (ShouldCollectItem(collision))
            {
                // Mark collection as successful
                collectionSuccessful = true;
                collision.GetComponent<Collectible>().Collect();
                
                // Play hit sound when ball is captured
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("Hit-1");
                }
                
                // Flash success color on screen instantly when capture is made
                VFX.Instance.FlashSuccessColor();

                // Create reflex floating text based on capture time
                CreateReflexFloatingText();

                Debug.Log("Collector: Scoring.Instance found, proceeding with bonus calculations");
                    
                // Award reflex bonus for quick capture
                Scoring.Instance.AwardReflexBonus();
                    
                // Award accuracy bonus for precise capture
                float overlapPercentage = usePreciseDetection ? 
                    CalculateOverlapRatioPrecise(collision) : 
                    CalculateOverlapRatio(collision);
                    
                // If overlap calculation failed (0%), use a reasonable fallback for display
                if (overlapPercentage <= 0f)
                {
                    Debug.Log("Collector: Overlap calculation failed, using fallback percentage for display");
                    overlapPercentage = 0.8f; // 80% for display purposes
                }
                    
                Scoring.Instance.AwardAccuracyBonus(overlapPercentage);
                    
                // Create floating text for bonuses
                Debug.Log($"Collector: Creating floating text for overlap: {overlapPercentage:P1} at position: {transform.position}");
                Scoring.Instance.CreateBonusFloatingText(overlapPercentage, transform.position);

            }
            else
            {
                // Only trigger round failure for failed collection attempt if ball is in play
                if (IsBallInPlay() && Progression.Instance != null)
                {
                    if (Progression.Instance.IsInRound())
                    {
                        Progression.Instance.FailRound();
                        Debug.Log("Collector: Round failed due to failed collection attempt");
                        
                        // Test alert for round failure
                        //HUD.Instance.ShowAlertMessage("ROUND FAILED!", 0.3f, 2f, 0.5f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Quickly flashes the player sprites to accent color during contraction
    /// </summary>
    private void FlashPlayerSpritesToAccentColor()
    {
        PlayerCharacter playerCharacter = FindObjectOfType<PlayerCharacter>();
        if (playerCharacter == null) return;

        // Get current theme accent color
        Color accentColor = Color.blue; // Default fallback
        if (ThemeController.Instance != null)
        {
            accentColor = ThemeController.Instance.GetAccentColor();
        }

        // Get current theme foreground color
        Color foregroundColor = Color.white; // Default fallback
        if (ThemeController.Instance != null)
        {
            foregroundColor = ThemeController.Instance.GetForegroundColor();
        }

        // Flash duration should be quick and snappy to match contraction
        float flashDuration = contractDuration * 0.3f; // 30% of contraction duration

        // Flash all 4 player graphics renderers to accent color
        if (playerCharacter.graphicTopLeft != null)
        {
            playerCharacter.graphicTopLeft.DOColor(accentColor, flashDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    playerCharacter.graphicTopLeft.DOColor(foregroundColor, flashDuration * 0.5f)
                        .SetEase(Ease.InQuad);
                });
        }
        if (playerCharacter.graphicTopRight != null)
        {
            playerCharacter.graphicTopRight.DOColor(accentColor, flashDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    playerCharacter.graphicTopRight.DOColor(foregroundColor, flashDuration * 0.5f)
                        .SetEase(Ease.InQuad);
                });
        }
        if (playerCharacter.graphicBottomLeft != null)
        {
            playerCharacter.graphicBottomLeft.DOColor(accentColor, flashDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    playerCharacter.graphicBottomLeft.DOColor(foregroundColor, flashDuration * 0.5f)
                        .SetEase(Ease.InQuad);
                });
        }
        if (playerCharacter.graphicBottomRight != null)
        {
            playerCharacter.graphicBottomRight.DOColor(accentColor, flashDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    playerCharacter.graphicBottomRight.DOColor(foregroundColor, flashDuration * 0.5f)
                        .SetEase(Ease.InQuad);
                });
        }
    }

    /// <summary>
    /// Shakes the graphics object with a quick, tight shudder effect
    /// </summary>
    private void ShakeGraphicsObject()
    {
        if (!useShakeOnClose || GraphicsObject == null)
            return;

        // Kill any existing shake tween
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
        }

        // Store original position
        Vector3 originalPosition = GraphicsObject.transform.localPosition;

        // Create shake tween
        shakeTween = GraphicsObject.transform.DOShakePosition(
            shakeDuration,
            shakeIntensity,
            shakeVibrato,
            fadeOut: true
        ).OnComplete(() => {
            // Ensure we return to exact original position
            GraphicsObject.transform.localPosition = originalPosition;
            shakeTween = null;
        });
    }


    /// <summary>
    /// Determines if a collectible should be collected based on overlap detection
    /// </summary>
    private bool ShouldCollectItem(Collider2D collectibleCollider)
    {
        if (!useOverlapDetection)
        {
            return true; // If overlap detection is disabled, collect everything
        }

        float overlapRatio = usePreciseDetection ? 
            CalculateOverlapRatioPrecise(collectibleCollider) : 
            CalculateOverlapRatio(collectibleCollider);
            
        bool shouldCollect = overlapRatio >= overlapThreshold;
        
        //Debug.Log($"Collector: Overlap ratio for {collectibleCollider.name}: {overlapRatio:F2} (threshold: {overlapThreshold:F2})");
        
        return shouldCollect;
    }

    /// <summary>
    /// Calculates what percentage of the collectible is inside the collector
    /// </summary>
    private float CalculateOverlapRatio(Collider2D collectibleCollider)
    {
        if (coll == null || collectibleCollider == null)
            return 0f;

        // Get bounds of both colliders
        Bounds collectorBounds = coll.bounds;
        Bounds collectibleBounds = collectibleCollider.bounds;

        // Debug.Log($"Collector: Collector bounds: {collectorBounds}, Ball bounds: {collectibleBounds}");
        // Debug.Log($"Collector: Ball collider: {collectibleCollider.name}, enabled: {collectibleCollider.enabled}, isTrigger: {collectibleCollider.isTrigger}");
        // Debug.Log($"Collector: Ball collider size: {collectibleCollider.bounds.size}, center: {collectibleCollider.bounds.center}");

        // Calculate the intersection bounds
        Vector3 intersectionMin = Vector3.Max(collectorBounds.min, collectibleBounds.min);
        Vector3 intersectionMax = Vector3.Min(collectorBounds.max, collectibleBounds.max);

        // Debug.Log($"Collector: Intersection min: {intersectionMin}, max: {intersectionMax}");

        // Check if there's any intersection
        if (intersectionMin.x >= intersectionMax.x || intersectionMin.y >= intersectionMax.y)
        {
            Debug.Log("Collector: No intersection detected");
            return 0f; // No intersection
        }

        // Calculate intersection area
        float intersectionArea = (intersectionMax.x - intersectionMin.x) * (intersectionMax.y - intersectionMin.y);
        
        // Calculate collectible area
        float collectibleArea = collectibleBounds.size.x * collectibleBounds.size.y;

        float ratio = Mathf.Clamp01(intersectionArea / collectibleArea);
        // Debug.Log($"Collector: Intersection area: {intersectionArea}, Ball area: {collectibleArea}, Ratio: {ratio:P1}");

        // Return the ratio (0.0 to 1.0)
        return ratio;
    }

    /// <summary>
    /// Alternative method using Physics2D.OverlapArea for more precise detection
    /// This method samples points within the collectible bounds to check overlap
    /// </summary>
    private float CalculateOverlapRatioPrecise(Collider2D collectibleCollider)
    {
        if (coll == null || collectibleCollider == null)
            return 0f;

        Bounds collectibleBounds = collectibleCollider.bounds;
        
        // Sample points within the collectible bounds
        int pointsInside = 0;
        
        for (int i = 0; i < samplePoints; i++)
        {
            // Generate random point within collectible bounds
            Vector2 samplePoint = new Vector2(
                Random.Range(collectibleBounds.min.x, collectibleBounds.max.x),
                Random.Range(collectibleBounds.min.y, collectibleBounds.max.y)
            );
            
            // Check if this point is inside the collector
            if (coll.OverlapPoint(samplePoint))
            {
                pointsInside++;
            }
        }
        
        return (float)pointsInside / samplePoints;
    }

    /// <summary>
    /// Draws debug visualization of the overlap detection (only in Scene view)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!useOverlapDetection || coll == null)
            return;

        // Draw collector bounds in green
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(coll.bounds.center, coll.bounds.size);

        // Draw threshold indicator
        Gizmos.color = Color.yellow;
        Vector3 thresholdSize = coll.bounds.size * overlapThreshold;
        Gizmos.DrawWireCube(coll.bounds.center, thresholdSize);
    }

    /// <summary>
    /// Starts the extended collection window with forgiveness
    /// </summary>
    private void StartCollectionWindow()
    {
        if (collectionWindowCoroutine != null)
        {
            StopCoroutine(collectionWindowCoroutine);
        }
        
        // Reset collection success flag for this window
        collectionSuccessful = false;
        
        collectionWindowCoroutine = StartCoroutine(CollectionWindowSequence());
    }

    /// <summary>
    /// Handles the extended collection window timing
    /// </summary>
    private IEnumerator CollectionWindowSequence()
    {
        // Pre-collection window (before contraction)
        yield return new WaitForSeconds(preCollectionWindow);
        
        // Enable collider during contraction
        if (coll != null)
        {
            coll.enabled = true;
        }
        
        // Wait for contraction duration
        yield return new WaitForSeconds(contractDuration);
        
        // Post-collection window (after contraction)
        yield return new WaitForSeconds(postCollectionWindow);
        
        // Disable collider
        if (coll != null)
        {
            coll.enabled = false;
        }

        // Check if collection was successful during this window
        if (!collectionSuccessful)
        {
            // Only fail the round if ball is in play and no collectible was collected during the window
            if (IsBallInPlay() && Progression.Instance != null && Progression.Instance.IsInRound())
            {
                Progression.Instance.FailRound();
                CameraShaker.Instance.Shake(CameraShaker.ShakeStyle.Large);

            }
        }

        collectionWindowCoroutine = null;
    }

    /// <summary>
    /// Gets the actual color to use, falling back to theme if color is clear
    /// </summary>
    private Color GetActualColor(Color fallbackColor, string themeColorName)
    {
        if (fallbackColor.a > 0f) // If alpha > 0, use the fallback color
            return fallbackColor;
        
        // Otherwise use theme color
        return ThemeController.Instance.GetColor(themeColorName);
    }

    /// <summary>
    /// Resets visual effects to original state
    /// </summary>
    private void ResetVisualEffects()
    {
        if (GraphicsObject != null)
        {
            GraphicsObject.transform.rotation = originalRotation;
        }
        
        if (graphicsSpriteRenderer != null)
        {
            graphicsSpriteRenderer.color = GetActualColor(originalColor, "primaryLight");
        }

        // Reset quadrant positions
        ResetQuadrantPositions();
    }

    /// <summary>
    /// Resets all quadrants to their original positions
    /// </summary>
    private void ResetQuadrantPositions()
    {
        if (topLeftQuadrant != null)
            topLeftQuadrant.localPosition = topLeftOriginalPos;
        if (topRightQuadrant != null)
            topRightQuadrant.localPosition = topRightOriginalPos;
        if (bottomLeftQuadrant != null)
            bottomLeftQuadrant.localPosition = bottomLeftOriginalPos;
        if (bottomRightQuadrant != null)
            bottomRightQuadrant.localPosition = bottomRightOriginalPos;
    }

    /// <summary>
    /// Force stop any ongoing expansion sequence
    /// </summary>
    public void StopExpansion()
    {
        if (expansionTween != null && expansionTween.IsActive())
        {
            expansionTween.Kill();
            expansionTween = null;
        }
        
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
            shakeTween = null;
        }
        
        if (collectionWindowCoroutine != null)
        {
            StopCoroutine(collectionWindowCoroutine);
            collectionWindowCoroutine = null;
        }
        
        isExpanding = false;
        isExpanded = false;
        isHolding = false;
        if (GraphicsObject != null)
        {
            GraphicsObject.transform.localScale = originalScale;
            GraphicsObject.transform.localPosition = Vector3.zero; // Reset position
        }
        
        // Disable collider
        if (coll != null)
        {
            coll.enabled = false;
        }
        
        ResetVisualEffects();
    }

    /// <summary>
    /// Reset to original state (useful for game resets)
    /// </summary>
    public void ResetCollector()
    {
        StopExpansion();
        if (GraphicsObject != null)
        {
            GraphicsObject.transform.localScale = originalScale;
            GraphicsObject.transform.localPosition = Vector3.zero; // Reset position
        }
        
        // Disable collider
        if (coll != null)
        {
            coll.enabled = false;
        }
        
        ResetVisualEffects();
    }

    /// <summary>
    /// Clean up tweens and coroutines when object is destroyed
    /// </summary>
    /// <summary>
    /// Creates floating text showing reflex rating and points based on capture time
    /// </summary>
    private void CreateReflexFloatingText()
    {
        if (VFX.Instance == null || HUD.Instance == null || Scoring.Instance == null)
        {
            Debug.LogWarning("Collector: Required instances not found for reflex floating text");
            return;
        }

        // Calculate capture time
        float captureTime = Time.time - Scoring.Instance.roundStartTime;
        
        // Get reflex rating and points from Scoring system
        string reflexRating = GetReflexRating(captureTime);
        int reflexPoints = GetReflexPoints(captureTime);
        
        if (reflexPoints > 0)
        {
            string reflexText = $"{reflexRating} +{reflexPoints}";
            Vector3 textPosition = transform.position + Vector3.up * 1f; // 1 unit above collector
            
            Debug.Log($"Collector: Creating reflex floating text: '{reflexText}' at {textPosition}");
            VFX.Instance.CreateFloatingText(
                reflexText,
                textPosition,
                HUD.Instance.successColor, // Green success color
                1.5f, // duration
                1f // scale
            );
        }
    }

    /// <summary>
    /// Gets reflex rating based on capture time
    /// </summary>
    private string GetReflexRating(float captureTime)
    {
        if (captureTime <= 2f) return "Fast!";
        else if (captureTime <= 4f) return "Quick";
        else if (captureTime <= 6f) return "Slow";
        else return "Slow";
    }

    /// <summary>
    /// Gets reflex points based on capture time
    /// </summary>
    private int GetReflexPoints(float captureTime)
    {
        if (captureTime <= 2f) return 500; // High reflex bonus
        else if (captureTime <= 4f) return 300; // Medium reflex bonus
        else if (captureTime <= 6f) return 100; // Low reflex bonus
        else return 0; // No bonus
    }

    private void OnDestroy()
    {
        if (expansionTween != null && expansionTween.IsActive())
        {
            expansionTween.Kill();
        }
        
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
        }
        
        if (collectionWindowCoroutine != null)
        {
            StopCoroutine(collectionWindowCoroutine);
        }
    }
}
