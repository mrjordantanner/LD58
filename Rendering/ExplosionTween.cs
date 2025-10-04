using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Quickly scales an object up and back down, then optionally destroys it.
/// </summary>
public class ExplosionTween : MonoBehaviour
{
    public bool selfDestructOnCompletion = true;

    // TODO
    //public Light2D light;

    [Header("Expand")]
    public Ease expandEasing = Ease.InSine;
    public float expandDuration = 0.05f;
    public float waitDuration = 0.1f;
    public float contractDuration = 0.25f;

    [Header("Contract")]
    public Ease contractEasing = Ease.InSine;

    void Awake()
    {
        StartCoroutine(ExpandThenContract());
    }

    IEnumerator ExpandThenContract()
    {
        // Start at size 0 and expand out
        var size = transform.localScale;
        //var lightSize = light.pointLightOuterRadius;
        transform.DOScale(Vector2.zero, 0);
        transform.DOScale(size, expandDuration).SetEase(expandEasing);
        yield return new WaitForSeconds(expandDuration + waitDuration);

        transform.DOScale(0, contractDuration).SetEase(expandEasing);
        yield return new WaitForSeconds(contractDuration);

        if (selfDestructOnCompletion)
        {
            Destroy(gameObject);
        }
    }



}
