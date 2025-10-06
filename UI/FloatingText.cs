using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    [HideInInspector] public TextMeshProUGUI text;
    [HideInInspector] public Image icon;

    private CanvasGroup canvasGroup;
    private float timer = 0f;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        icon = GetComponentInChildren<Image>();

        // Use VFX controller lifespan
        Destroy(gameObject, VFX.Instance.FloatingTextLifespan);
    }

    void Update()
    {
        // Use VFX controller float speed
        transform.Translate(VFX.Instance.FloatingTextFloatSpeed * Time.deltaTime * Vector3.up);
        timer += Time.deltaTime;

        // Fade out over lifespan
        if (canvasGroup != null)
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / VFX.Instance.FloatingTextLifespan);

        if (timer > VFX.Instance.FloatingTextLifespan)
            Destroy(gameObject);
    }

    public IEnumerator FloatAndFade(float duration)
    {
        Vector3 currentScale = text.transform.localScale;
        Vector3 targetScale = currentScale * VFX.Instance.FloatingTextScaleSize;

        // Scale up with elastic easing
        text.transform.DOScale(targetScale, VFX.Instance.FloatingTextScaleUpDuration)
            .SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(VFX.Instance.FloatingTextScaleUpDuration);

        // Scale down with bounce easing back to original scale
        text.transform.DOScale(currentScale, VFX.Instance.FloatingTextScaleDownDuration)
            .SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(VFX.Instance.FloatingTextScaleDownDuration);

        // Hold at original scale
        yield return new WaitForSeconds(VFX.Instance.FloatingTextHoldDuration);

        // Fade out text and icon
        text.DOFade(0, VFX.Instance.FloatingTextFadeDuration).SetEase(Ease.InQuint);
        if (icon != null)
            icon.DOFade(0, VFX.Instance.FloatingTextFadeDuration).SetEase(Ease.InQuint);
    }
}

