using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the Uzbekistan map marker with a radar pulse animation
/// and smooth scale feedback when clicked/tapped.
/// </summary>
public class CountryMarker : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Reference")]
    public CountryInfoCard infoCard;

    [Header("Pulse Animation Settings")]
    public float pulseSpeed = 3.0f;
    public float pulseAmount = 0.12f;

    [Header("Click Feedback Settings")]
    public float clickScaleMultiplier = 1.35f;
    public float clickAnimationDuration = 0.20f;

    private Vector3 originalScale;
    private bool isAnimatingClick = false;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isAnimatingClick)
        {
            PerformRadarPulse();
        }
    }

    private void PerformRadarPulse()
    {
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * (1.0f + scaleOffset);

        if (spriteRenderer != null)
        {
            float alpha = 0.85f + Mathf.Sin(Time.time * pulseSpeed * 0.5f) * 0.15f;
            Color col = spriteRenderer.color;
            col.a = alpha;
            spriteRenderer.color = col;
        }
    }

    private void OnMouseDown()
    {
        SelectMarker();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectMarker();
    }

    public void SelectMarker()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateTapFeedback());

        if (infoCard != null)
        {
            infoCard.ShowCard();
        }
    }

    private IEnumerator AnimateTapFeedback()
    {
        isAnimatingClick = true;
        Vector3 targetScale = originalScale * clickScaleMultiplier;
        float elapsed = 0f;

        // Scale UP with EaseOutBack
        while (elapsed < clickAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / clickAnimationDuration;
            // Overshoot bounce equation
            float bounce = Mathf.Sin(t * Mathf.PI * 0.75f) * 1.15f;
            transform.localScale = Vector3.LerpUnclamped(originalScale, targetScale, bounce);
            yield return null;
        }

        elapsed = 0f;
        // Scale back down
        while (elapsed < clickAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / clickAnimationDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t * t * (3f - 2f * t));
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimatingClick = false;
    }
}
