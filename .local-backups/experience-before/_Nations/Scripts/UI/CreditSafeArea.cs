using CesiumForUnity;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nations.UI
{
    // Keep Cesium's own credit renderer, links, images and dynamic data attribution intact.
    public sealed class CreditSafeArea : MonoBehaviour
    {
        UIDocument document;
        Label googleAttribution;
        GlobeRuntime runtime;
        void Start()
        {
            document = CesiumCreditSystem.GetDefaultCreditSystem().GetComponent<UIDocument>();
            runtime = GetComponent<GlobeRuntime>();
            if (!document) return;
            googleAttribution = new Label("Google Maps");
            googleAttribution.pickingMode = PickingMode.Ignore;
            googleAttribution.style.position = Position.Absolute;
            googleAttribution.style.fontSize = 16;
            googleAttribution.style.color = Color.white;
            googleAttribution.style.backgroundColor = new Color(0, 0, 0, 0.65f);
            googleAttribution.style.paddingLeft = googleAttribution.style.paddingRight = 6;
            document.rootVisualElement.Add(googleAttribution);
        }
        void LateUpdate()
        {
            if (!document || document.rootVisualElement?.panel == null) return;
            var root = document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            var safe = Screen.safeArea;
            Vector2 min = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(safe.xMin, Screen.height - safe.yMax));
            Vector2 max = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(safe.xMax, Screen.height - safe.yMin));
            var credits = root.Q("OnScreenCredits");
            if (credits != null)
            {
                for (var ancestor = credits.parent; ancestor != null; ancestor = ancestor.parent)
                    ancestor.pickingMode = PickingMode.Ignore;
                credits.style.width = StyleKeyword.Auto;
                credits.style.backgroundColor = new Color(0, 0, 0, 0.65f);
                credits.style.left = min.x + 12;
                credits.style.right = root.resolvedStyle.width - max.x + 12;
                credits.style.bottom = root.resolvedStyle.height - max.y + 10;
                credits.style.flexWrap = Wrap.Wrap;
            }
            if (googleAttribution != null)
            {
                googleAttribution.style.display = runtime && runtime.GoogleActive ? DisplayStyle.Flex : DisplayStyle.None;
                googleAttribution.style.right = root.resolvedStyle.width - max.x + 12;
                googleAttribution.style.top = min.y + 12;
            }
            var popup = root.Q("PopupCredits");
            if (popup != null)
            {
                popup.style.maxWidth = Mathf.Max(100, max.x - min.x - 24);
                popup.style.maxHeight = Mathf.Max(100, max.y - min.y - 80);
            }
        }
    }
}
