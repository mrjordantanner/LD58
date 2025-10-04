using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class RadialProgressBar : MonoBehaviour
{
    [HideInInspector]
    public CanvasGroup canvasGroup;
    public Image fillImage;
    public Image deltaImage;
    public float fullOpacity = 0.75f;

    // Delta
    float changeDuration = 0.35f;
    float timer = 0f;
    float timerDuration = 0.75f;

    // TODO fades
    //public float hideDelayDuration = 1f;
    //public float hideFadeDuration = 0.5f;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        ResetTimer();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            ResetTimer();

            if (fillImage.fillAmount != deltaImage.fillAmount)
            {
                TweenDeltaBar();
            }
        }
    }

    public void ResetTimer()
    {
        timer = timerDuration;
    }

    public void SetBar(float amount)
    {
        fillImage.fillAmount = amount;
        deltaImage.fillAmount = amount;
    }

    public void TweenDeltaBar()
    {
        var target = fillImage.fillAmount;
        var tween = DOTween.To(() =>
            deltaImage.fillAmount, x => deltaImage.fillAmount = x, target, changeDuration)
                .SetEase(Ease.OutSine);
    }


    public void Show()
    {
        canvasGroup.alpha = fullOpacity;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
    }

    public void UpdateProgressBar(float percentageAmount)
    {
        fillImage.fillAmount = percentageAmount / 1;
    }

    // TODO fades
    //IEnumerator HideProgressBarWithDelay()
    //{
    //    yield return new WaitForSeconds(hideDelayDuration);
    //}
}
