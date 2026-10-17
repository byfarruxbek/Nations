using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attached to 3D Country Markers on the 3D Globe surface.
/// Handles pulse animation, selection highlights, sphere alignment,
/// and notifying the CountryInfoCard with country data.
/// </summary>
public class GlobeMarker : MonoBehaviour, IPointerClickHandler
{
    [System.Serializable]
    public class CountryData
    {
        public string flagEmoji = "🇺🇿";
        public string countryName = "O‘zbekiston";
        public string capitalName = "Toshkent";
        public string continentName = "Osiyo";
    }

    [Header("Country Info")]
    public CountryData countryInfo = new CountryData();

    [Header("UI Reference")]
    public CountryInfoCard infoCard;
    public GlobeRotator globeRotator;

    [Header("Pulse Animation")]
    public float pulseSpeed = 2.5f;
    public float pulseAmount = 0.12f;
    public float clickScaleMultiplier = 1.4f;

    private Vector3 originalScale;
    private bool isSelected = false;
    private SpriteRenderer spriteRenderer;
    private static GlobeMarker currentlySelectedMarker;

    private void Start()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure marker faces outward from sphere center
        if (transform.parent != null)
        {
            Vector3 dirFromCenter = (transform.position - transform.parent.position).normalized;
            if (dirFromCenter != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dirFromCenter);
            }
        }
    }

    private void Update()
    {
        if (!isSelected)
        {
            PerformSubtlePulse();
        }
    }

    private void PerformSubtlePulse()
    {
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * (1.0f + scaleOffset);
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
        // Unselect previous
        if (currentlySelectedMarker != null && currentlySelectedMarker != this)
        {
            currentlySelectedMarker.DeselectMarker();
        }

        currentlySelectedMarker = this;
        isSelected = true;

        StopAllCoroutines();
        StartCoroutine(AnimateHighlight());

        // Focus globe on this country
        if (globeRotator != null)
        {
            globeRotator.FocusOnMarker(transform);
        }

        // Display info card
        if (infoCard != null)
        {
            infoCard.DisplayCountry(countryInfo.flagEmoji, countryInfo.countryName, countryInfo.capitalName, countryInfo.continentName);
        }
    }

    public void DeselectMarker()
    {
        isSelected = false;
        transform.localScale = originalScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private IEnumerator AnimateHighlight()
    {
        Vector3 targetScale = originalScale * clickScaleMultiplier;
        float elapsed = 0f;
        float dur = 0.20f;

        // Bounce UP
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t * t * (3f - 2f * t));
            yield return null;
        }

        // Highlight color glow
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.258f, 0.851f, 1.0f, 1.0f);
        }
    }
}
