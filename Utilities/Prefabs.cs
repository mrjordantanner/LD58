using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Singleton.  Reference class for Prefabs and other misc materials, sprites, etc.
/// </summary>
[ExecuteInEditMode]
public class Prefabs : MonoBehaviour
{
    #region EditMode Singleton
    public static Prefabs Instance;
    private void Awake()
    {
        // Play Mode
        if (Application.isPlaying)
        {
            // Ensure there's only one instance in play mode
            if (Instance != null && Instance != this)
            {
                Debug.Log($"Destroying duplicate instance of {name}.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // Edit Mode
        else
        {
            if (Instance == null) Instance = this;
        }
    }
    #endregion

    // Editor-only Update method
    void Update()
    {
        if (Application.isPlaying) return;
        Instance = this;
    }

    [Header("Map")]
    public GameObject BlankBlockPrefab;
    public GameObject OutOfBoundsAreaPrefab;
    public GameObject TransitionRoomPrefab;
    public GameObject FinishRoomPrefab;
    public GameObject CheckpointPrefab;
    public GameObject SequenceTriggerPrefab;
    public GameObject WallBlockContainer;
    public GameObject ObjectContainer;

    [Header("Tilemap")]
    public GameObject BlankTilemapPrefab;
    public GameObject PlaceholderBlockPrefab;
    public GameObject DestroyVFX;

    [Header("Materials")]
    public Material blockHitFlashMaterial;
    public Material blockHitFlashMaterial2;

    [Header("Sprites")]
    public Sprite transitionRoomWallBlockSprite;

    [Header("VFX / Particles")]
    public GameObject PlayerDeathParticles;
    public GameObject PlayerDeathProjectile;
    public GameObject CollectGemSmallVFX;
    public GameObject CollectGemLargeVFX;
    public GameObject BlockHitVFX;
    public GameObject RectAoeObject, CircleAoeObject;
}

