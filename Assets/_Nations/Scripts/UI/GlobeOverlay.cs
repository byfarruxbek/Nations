using Nations.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace Nations.UI
{
    public sealed class GlobeOverlay : MonoBehaviour
    {
        public GlobeRuntime Runtime;
        public GlobeCameraController Controller;
        public RectTransform SafeRoot;
        public CanvasGroup Loading;
        public RectTransform Spinner;
        public Text Status;
        public Text DebugText;
        Rect lastSafe;
        int width, height;
        float nextDebug, frameTime;
        public bool ShowDebug;
        bool debugEnabled;
        void Start()
        {
            Status.font = Nations.Localization.Locale.Font;
            Status.resizeTextForBestFit = true; Status.resizeTextMinSize = 10; Status.resizeTextMaxSize = 13;
debugEnabled = ShowDebug && (Application.isEditor || Debug.isDebugBuild);
            DebugText.gameObject.SetActive(debugEnabled);
        }
        void Update()
        {
            Rect safe = GlobeSafeArea.Current;
            if (safe != lastSafe || width != Screen.width || height != Screen.height)
            {
                width = Screen.width; height = Screen.height; lastSafe = safe;
                SafeRoot.anchorMin = new Vector2(safe.xMin / width, safe.yMin / height);
                SafeRoot.anchorMax = new Vector2(safe.xMax / width, safe.yMax / height);
                SafeRoot.offsetMin = SafeRoot.offsetMax = Vector2.zero;
            }
            Loading.alpha = Mathf.MoveTowards(Loading.alpha, Runtime.Ready ? 0 : 1, Time.unscaledDeltaTime * 2);
            if (Loading.alpha > 0) Spinner.Rotate(0, 0, -180 * Time.unscaledDeltaTime);
            if (Status.text != Runtime.Status) Status.text = Runtime.Status;
if (!debugEnabled) return;
            frameTime = Mathf.Lerp(frameTime, Time.unscaledDeltaTime, 0.05f);
            if (Time.unscaledTime > nextDebug)
            {
                nextDebug = Time.unscaledTime + 0.5f;
                DebugText.text = $"{1 / Mathf.Max(frameTime, 0.001f):0} FPS   {Controller.Altitude / 1000:0.00} km\n{Controller.Latitude:0.000}°, {Controller.Longitude:0.000}°   pitch {Controller.Pitch:0}°   heading {Controller.Heading % 360:0}°\nTiles {Runtime.LoadProgress:0}%";
            }

        }
    }
}
