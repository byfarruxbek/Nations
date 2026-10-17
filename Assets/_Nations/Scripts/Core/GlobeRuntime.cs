using System;
using Nations.Localization;
using CesiumForUnity;
using UnityEngine;

namespace Nations
{
    [DefaultExecutionOrder(-100)]
    public sealed class GlobeRuntime : MonoBehaviour
    {
        public event Action FirstContentRendered;
        public event Action ConnectionFailed;
        bool contentAnnounced, failureEpisode;
        public MapConfig Config;
        public Cesium3DTileset Tileset;
        public GameObject SetupGlobe;
        public bool GoogleActive { get; private set; }
        public bool Streaming { get; private set; }
        public string Status => Locale.Get(statusKey);
        string statusKey = "status.starting";
        public float LoadProgress { get; private set; }
        public bool Ready { get; private set; }
        float started, nextPoll;
        bool failed, hasContent;
        int retries;
        float retryAt;
        const int MaxRetries = 2;

        void Start()
        {
            Application.targetFrameRate = Config.TargetFrameRate;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            // Prefer the working, scene-authored Google ion tileset. Never change its token.
            foreach (var candidate in FindObjectsByType<Cesium3DTileset>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (candidate.ionAssetID == 2275207 && candidate.tilesetSource == CesiumDataSource.FromCesiumIon)
                    Tileset = candidate;
            bool configured = Tileset && Tileset.gameObject.activeInHierarchy &&
                (Tileset.tilesetSource == CesiumDataSource.FromCesiumIon && Tileset.ionAssetID > 0 ||
                 Tileset.tilesetSource == CesiumDataSource.FromUrl && !string.IsNullOrWhiteSpace(Tileset.url));
            SetFallback(true);
            try
            {
                var local = Resources.Load<TextAsset>(Config.LocalConfigResource);
                var credentials = local ? JsonUtility.FromJson<MapCredentials>(local.text) : new MapCredentials();
                string key = credentials?.GOOGLE_MAP_TILES_API_KEY?.Trim();
                GoogleActive = configured ? Tileset.ionAssetID == 2275207 || (Tileset.url ?? "").Contains("tile.googleapis.com") : !string.IsNullOrEmpty(key) && key != "YOUR_API_KEY";
                if (!Tileset) throw new InvalidOperationException("Missing tileset reference");
                // Subscribe before activation/configuration can start a root request.
                Cesium3DTileset.OnCesium3DTilesetLoadFailure += OnFailure;
                Tileset.OnTileGameObjectCreated += OnTileCreated;
                Tileset.enabled = false;
                Tileset.showCreditsOnScreen = true;
                Tileset.maximumScreenSpaceError = Config.Quality == MobileQuality.Low ? 24 : Config.Quality == MobileQuality.High ? 8 : 16;
                Tileset.maximumSimultaneousTileLoads = Config.Quality == MobileQuality.Low ? 6u : Config.Quality == MobileQuality.High ? 16u : 10u;
                Tileset.maximumCachedBytes = Config.Quality == MobileQuality.Low ? 128L * 1024 * 1024 : Config.Quality == MobileQuality.High ? 512L * 1024 * 1024 : 256L * 1024 * 1024;
                Tileset.createPhysicsMeshes = true;
                if (configured) { /* Scene credentials and source are authoritative. */ }
                else if (GoogleActive)
                {
                    Tileset.tilesetSource = CesiumDataSource.FromUrl;
                    Tileset.url = "https://tile.googleapis.com/v1/3dtiles/root.json?key=" + Uri.EscapeDataString(key);
                }
                else if (!string.IsNullOrWhiteSpace(credentials?.CESIUM_ION_ACCESS_TOKEN))
                {
                    Tileset.tilesetSource = CesiumDataSource.FromCesiumIon;
                    Tileset.ionAssetID = 2275207;
                    GoogleActive = true;
                    Tileset.ionAccessToken = credentials.CESIUM_ION_ACCESS_TOKEN;
                }
                else
                {
                    statusKey = "status.setup";
                    Ready = true; return;
                }
                Tileset.gameObject.SetActive(true);
                Tileset.enabled = true;
                Streaming = true; started = Time.unscaledTime; retryAt = started + Mathf.Max(60, Config.LoadingTimeout * 2);
                statusKey = "status.streaming";
            }
            catch (Exception ex)
            {
                statusKey = "status.initialization";
                Debug.LogWarning("NATIONS: Map initialization failed (" + ex.GetType().Name + "). No credentials logged.");
                Ready = true; failed = true; NotifyFailure();
            }
        }
        void SetFallback(bool visible)
        {
            if (!SetupGlobe) return;
            // This is only a visual placeholder; picking uses the analytical ellipsoid.
            foreach (var collider in SetupGlobe.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            SetupGlobe.SetActive(visible);
            foreach (var renderer in SetupGlobe.GetComponentsInChildren<Renderer>(true)) renderer.enabled = visible;
        }
        void OnTileCreated(GameObject tile)
        {
            if (!tile.GetComponentInChildren<Renderer>()) return;
            hasContent = true;
            SetFallback(false);
        }
        void OnFailure(Cesium3DTilesetLoadFailureDetails details)
        {
            if (details.tileset != Tileset) return;
            NotifyFailure();
            failed = true; Ready = true; retryAt = Time.unscaledTime + 10;
            statusKey = "status.unavailable";
            Debug.LogWarning("NATIONS: Tileset request failed. Check internet, Map Tiles API access, key restrictions and billing. Request URL omitted to protect credentials.");
        }
        void NotifyFailure()
        {
            if (failureEpisode) return;
            failureEpisode = true; ConnectionFailed?.Invoke();
        }
        void Update()
        {
            if (!Streaming || Time.unscaledTime < nextPoll) return;
            nextPoll = Time.unscaledTime + 0.5f;
            LoadProgress = Tileset.ComputeLoadProgress();
            if (!hasContent) hasContent = Tileset.GetComponentInChildren<Renderer>() != null;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                statusKey = "status.offline"; NotifyFailure();
            }
            if (hasContent)
            {
                if (!contentAnnounced)
                {
                    foreach (var renderer in Tileset.GetComponentsInChildren<MeshRenderer>())
                        if (renderer.enabled && renderer.isVisible)
                        { contentAnnounced = true; FirstContentRendered?.Invoke(); break; }
                }
                if (Application.internetReachability != NetworkReachability.NotReachable) failureEpisode = false;
                Ready = true; failed = false; SetFallback(false); statusKey = "";
                return;
            }
            if (Time.unscaledTime >= retryAt && retries < MaxRetries && Application.internetReachability != NetworkReachability.NotReachable)
            {
                retries++;
                failed = false;
                started = Time.unscaledTime;
                retryAt = started + Mathf.Max(60, Config.LoadingTimeout * 2);
                statusKey = "status.reconnecting";
                Tileset.RecreateTileset();
            }
            else if (retries >= MaxRetries && Time.unscaledTime >= retryAt)
            {
                Ready = true;
                statusKey = "status.exhausted"; NotifyFailure();
            }
            else if (!failed && !Ready && Time.unscaledTime - started > Config.LoadingTimeout)
            {
                Ready = true; statusKey = "status.waiting";
            }
        }
        void OnDestroy()
        {
            Cesium3DTileset.OnCesium3DTilesetLoadFailure -= OnFailure;
            if (Tileset) Tileset.OnTileGameObjectCreated -= OnTileCreated;
        }
    }
}
