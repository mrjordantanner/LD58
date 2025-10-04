using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [Range(0, 1)]
    public float speedPercentage = 0.5f;
    public bool useUnscaledDeltaTime;

    //[ReadOnly]
    public float scrollSpeed;

    public MeshRenderer meshRenderer;
    [HideInInspector]
    public Material material;
    Vector2 offset;

    public bool isScrollLocked;
    public bool scrollLockCriteria;


    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
    }

    public void UpdateMaterial(Material mat)
    {
        material = mat;
        meshRenderer.material = material;
    }

    void Update()
    {
        // TODO...
        //isScrollLocked = 
        //    !PlayerManager.Instance.canMove ||
        //    GameManager.Instance.gamePaused ||
        //    PlayerManager.Instance.physics.velocityY == 0 ||
        //    PlayerManager.Instance.physics.velocity.y == 0;

        if (material == null || isScrollLocked) return;
        offset = material.mainTextureOffset;
        var time = useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;
        offset += scrollSpeed * time * Vector2.up / 10;
        material.mainTextureOffset = offset;

    }
}
