using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles interaction and selection highlights for Bottom Panel navigation buttons.
/// </summary>
public class NavButtonHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Settings")]
    public Text buttonText;
    public Image backgroundImage;

    public static NavButtonHandler activeSelection;

    private Color activeColor = new Color(0.258f, 0.851f, 1.0f, 1.0f);
    private Color inactiveColor = new Color(0.627f, 0.698f, 0.808f, 1.0f);
    private Color activeBgColor = new Color(0.258f, 0.851f, 1.0f, 0.20f);

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectTab();
    }

    public void SelectTab()
    {
        NavButtonHandler[] allButtons = FindObjectsOfType<NavButtonHandler>();
        foreach (var btn in allButtons)
        {
            btn.SetState(false);
        }

        SetState(true);
        activeSelection = this;
        Debug.Log("Navigated to tab: " + gameObject.name);
    }

    public void SetState(bool isSelected)
    {
        if (buttonText != null)
        {
            buttonText.color = isSelected ? activeColor : inactiveColor;
            buttonText.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? activeBgColor : Color.clear;
        }
    }
}
