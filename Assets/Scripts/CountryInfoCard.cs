using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the translucent glass UI card with smooth spring slide-up transitions.
/// </summary>
public class CountryInfoCard : MonoBehaviour
{
    [Header("Animation Settings")]
    public float slideDistance = 80f;
    public float animDuration = 0.45f;

    [Header("UI Text Components")]
    public Text countryHeaderText;
    public Text capitalText;
    public Text continentText;

    [Header("UI Components")]
    public CanvasGroup canvasGroup;
    public RectTransform cardTransform;
    public Button exploreButton;

    private Vector2 hiddenPosition;
    private Vector2 targetPosition;

    private void Start()
    {
        if (cardTransform == null)
            cardTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        targetPosition = cardTransform.anchoredPosition;
        hiddenPosition = targetPosition - new Vector2(0, slideDistance);

        cardTransform.anchoredPosition = hiddenPosition;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.60f;
        }

        if (exploreButton != null)
        {
            exploreButton.onClick.AddListener(OnExploreButtonClicked);
        }
    }

    public void DisplayCountry(string flag, string countryName, string capital, string continent)
    {
        if (countryHeaderText != null)
        {
            countryHeaderText.text = flag + "  " + countryName;
        }

        if (capitalText != null)
        {
            capitalText.text = "Poytaxt:  " + capital;
        }

        if (continentText != null)
        {
            continentText.text = "Qit'a:  " + continent;
        }

        ShowCard();
    }

    public void ShowCard()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateSlideUp());
    }

    private IEnumerator AnimateSlideUp()
    {
        Vector2 startPos = cardTransform.anchoredPosition;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 0.60f;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;
            // Smooth spring ease-out equation
            float easeSpring = Mathf.Sin(t * Mathf.PI * 0.5f);

            cardTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, easeSpring);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1.0f, t);
            }
            yield return null;
        }

        cardTransform.anchoredPosition = targetPosition;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f;
        }
    }

    public void OnExploreButtonClicked()
    {
        Debug.Log("Kashf etish (Explore) button clicked!");
        StartCoroutine(AnimateButtonPulse());
    }

    private IEnumerator AnimateButtonPulse()
    {
        if (exploreButton != null)
        {
            Transform btnTr = exploreButton.transform;
            Vector3 origScale = Vector3.one;
            Vector3 targetScale = new Vector3(1.06f, 1.06f, 1f);
            float dur = 0.12f;
            float el = 0f;

            while (el < dur)
            {
                el += Time.deltaTime;
                btnTr.localScale = Vector3.Lerp(origScale, targetScale, el / dur);
                yield return null;
            }

            el = 0f;
            while (el < dur)
            {
                el += Time.deltaTime;
                btnTr.localScale = Vector3.Lerp(targetScale, origScale, el / dur);
                yield return null;
            }

            btnTr.localScale = origScale;
        }
    }
}
