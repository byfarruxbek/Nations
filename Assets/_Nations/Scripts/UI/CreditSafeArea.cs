using CesiumForUnity;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nations.UI
{
    // Keep Cesium's native renderer, provider logos, links and changing credits intact.
    [DefaultExecutionOrder(300)]
    public sealed class CreditSafeArea : MonoBehaviour
    {
        UIDocument document;
        public float ReservedHeight { get; private set; } = 100;
        public bool CreditsPositioned { get; private set; }
        void LateUpdate()
        {
            if(!document)
                document=CesiumCreditSystem.GetDefaultCreditSystem().GetComponent<UIDocument>();
            if(!document || document.rootVisualElement?.panel==null)return;
            var root=document.rootVisualElement;
            root.pickingMode=PickingMode.Ignore;
            // UIDocument's root can size to its contents; explicit viewport sizing makes safe-area offsets reliable.
            root.style.position=Position.Absolute;root.style.left=0;root.style.top=0;root.style.right=0;root.style.bottom=0;
            var safe=GlobeSafeArea.Current;
            Vector2 min=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(safe.xMin,Screen.height-safe.yMax));
            Vector2 max=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(safe.xMax,Screen.height-safe.yMin));
            var credits=root.Q("OnScreenCredits");
            if(credits==null)return;
            for(var ancestor=credits.parent;ancestor!=null;ancestor=ancestor.parent)ancestor.pickingMode=PickingMode.Ignore;
            credits.style.position=Position.Absolute;credits.style.width=StyleKeyword.Auto;
            credits.style.left=min.x+12;credits.style.right=root.resolvedStyle.width-max.x+12;
            credits.style.bottom=root.resolvedStyle.height-max.y+10;
            credits.style.flexWrap=Wrap.Wrap;credits.style.backgroundColor=new Color(0,0,0,0.7f);
            CreditsPositioned=true;
            var popup=root.Q("PopupCredits");
            if(popup!=null)
            {
                popup.style.left=min.x+12;popup.style.top=min.y+70;
                popup.style.width=Mathf.Max(100,max.x-min.x-24);popup.style.maxHeight=Mathf.Max(100,max.y-min.y-150);
            }
        }
    }
}
