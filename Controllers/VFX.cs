using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class VFX : MonoBehaviour
{
    #region Singleton
    public static VFX Instance;
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
        #endregion

        Init();
    }

    [HideInInspector]
    public GameObject VFXContainer;
    public Volume GlobalConstantEffects;
    public Material hitFlashMaterial;
    public Material shootFlashMaterial;
    public Material imageFlashMaterial;

    public GameObject BackgroundGraphics;
    public GameObject iPhoneFrameGraphics;

    [HideInInspector] public ColorAdjustments colorAdjustments;
    [HideInInspector] public SpriteRenderer backgroundSpriteRenderer;
    [HideInInspector] public SpriteRenderer iphoneFrameSpriteRenderer;

    [Header("Floating Text")]
    public Canvas WorldUICanvas;
    public GameObject FloatingTextPrefab;
    
    [Header("Floating Text Animation")]
    [SerializeField] private float floatingTextLifespan = 2f;
    [SerializeField] private float floatingTextFloatSpeed = 40f;
    [SerializeField] private float floatingTextScaleSize = 1.2f;
    [SerializeField] private float floatingTextScaleUpDuration = 0.3f;
    [SerializeField] private float floatingTextScaleDownDuration = 0.2f;
    [SerializeField] private float floatingTextFadeDelay = 0.1f;
    [SerializeField] private float floatingTextFadeDuration = 0.5f;
    [SerializeField] private float floatingTextHoldDuration = 0.5f;

    // Public getters for floating text properties
    public float FloatingTextLifespan => floatingTextLifespan;
    public float FloatingTextFloatSpeed => floatingTextFloatSpeed;
    public float FloatingTextScaleSize => floatingTextScaleSize;
    public float FloatingTextScaleUpDuration => floatingTextScaleUpDuration;
    public float FloatingTextScaleDownDuration => floatingTextScaleDownDuration;
    public float FloatingTextFadeDelay => floatingTextFadeDelay;
    public float FloatingTextFadeDuration => floatingTextFadeDuration;
    public float FloatingTextHoldDuration => floatingTextHoldDuration;

    void Init()
    {
        VFXContainer = new GameObject("VFXContainer");

        backgroundSpriteRenderer = BackgroundGraphics.GetComponent<SpriteRenderer>();
        if (iPhoneFrameGraphics != null)
        {
            iphoneFrameSpriteRenderer = iPhoneFrameGraphics.GetComponent<SpriteRenderer>();
        }
    }

    //public void ScreenFlash()
    //{
    //    backgroundSpriteRenderer.color = Color.white;
    //    backgroundSpriteRenderer.DOColor(Color.clear, 0.02f).SetEase(Ease.Linear);
    //}

    public void FlashSuccessColor()
    {
        // Set background to success color
        backgroundSpriteRenderer.color = HUD.Instance.successColor;
        
        // Fade back to current theme background color
        backgroundSpriteRenderer.DOColor(ThemeController.Instance.GetCurrentTheme().backgroundColor, 0.025f).SetEase(Ease.OutQuad);
    }

    public void FlashAlertColor()
    {
        // Set background to alert color
        backgroundSpriteRenderer.color = new Color(HUD.Instance.alertColor.r, HUD.Instance.alertColor.g, HUD.Instance.alertColor.b, 0.15f);
        
        // Hold for 1 second then fade to current theme background color
        backgroundSpriteRenderer.DOColor(ThemeController.Instance.GetCurrentTheme().backgroundColor, 0.1f).SetDelay(1f).SetEase(Ease.OutQuad);
    }

    public void CreateFloatingText(string text, Vector3 position, Color color, float duration = 1.0f, float scale = 1f, float yOffset = 35f)
    {
        Debug.Log($"VFX: CreateFloatingText called - text: '{text}', position: {position}, color: {color}, duration: {duration}, scale: {scale}, yOffset: {yOffset}");
        
        if (FloatingTextPrefab == null)
        {
            Debug.LogError("VFX: FloatingTextPrefab is null!");
            return;
        }
        
        if (WorldUICanvas == null)
        {
            Debug.LogError("VFX: WorldUICanvas is null!");
            return;
        }
        
        // Apply Y offset to position
        Vector3 adjustedPosition = position + Vector3.up * yOffset;
        
        // Instantiate prefab and parent to world canvas
        GameObject floatingTextObj = Instantiate(FloatingTextPrefab, adjustedPosition, Quaternion.identity, WorldUICanvas.transform);
        FloatingText floatingText = floatingTextObj.GetComponent<FloatingText>();

        if (floatingText == null)
        {
            Debug.LogError("VFX: FloatingText component not found on instantiated object!");
            return;
        }

        floatingText.text.text = text;
        floatingText.text.color = color;
        floatingText.transform.localScale *= scale;

        Debug.Log($"VFX: Starting FloatAndFade coroutine for text: '{text}' at adjusted position: {adjustedPosition}");
        StartCoroutine(floatingText.FloatAndFade(duration));
    }
}
