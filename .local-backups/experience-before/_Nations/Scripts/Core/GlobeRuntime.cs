using System;
using CesiumForUnity;
using UnityEngine;

namespace Nations
{
    [DefaultExecutionOrder(-100)]
    public sealed class GlobeRuntime : MonoBehaviour
    {
        public MapConfig Config;
        public Cesium3DTileset Tileset;
        public GameObject SetupGlobe;
        public bool GoogleActive { get; private set; }
        public bool Streaming { get; private set; }
        public string Status { get; private set; } = "Starting globe…";
        public float LoadProgress { get; private set; }
        public bool Ready { get; private set; }
        float started, nextPoll;
        bool failed, hasContent;

        void Start()
        {
            Application.targetFrameRate = Config.TargetFrameRate;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            Tileset.gameObject.SetActive(false);
            SetupGlobe.SetActive(true);
            try
            {
                var local = Resources.Load<TextAsset>(Config.LocalConfigResource);
                var credentials = local ? JsonUtility.FromJson<MapCredentials>(local.text) : new MapCredentials();
                string key = credentials?.GOOGLE_MAP_TILES_API_KEY?.Trim();
                GoogleActive = !string.IsNullOrEmpty(key) && key != "YOUR_API_KEY";
                Tileset.showCreditsOnScreen = true;
                Tileset.maximumScreenSpaceError = Config.Quality == MobileQuality.Low ? 24 : Config.Quality == MobileQuality.High ? 8 : 16;
                Tileset.maximumSimultaneousTileLoads = Config.Quality == MobileQuality.Low ? 6u : Config.Quality == MobileQuality.High ? 16u : 10u;
                Tileset.maximumCachedBytes = Config.Quality == MobileQuality.Low ? 128L * 1024 * 1024 : Config.Quality == MobileQuality.High ? 512L * 1024 * 1024 : 256L * 1024 * 1024;
                Tileset.createPhysicsMeshes = true;
                if (GoogleActive)
                {
                    Tileset.tilesetSource = CesiumDataSource.FromUrl;
                    Tileset.url = "https://tile.googleapis.com/v1/3dtiles/root.json?key=" + Uri.EscapeDataString(key);
                }
                else if (!string.IsNullOrWhiteSpace(credentials?.CESIUM_ION_ACCESS_TOKEN))
                {
                    Tileset.tilesetSource = CesiumDataSource.FromCesiumIon;
                    Tileset.ionAssetID = 1;
                    Tileset.ionAccessToken = credentials.CESIUM_ION_ACCESS_TOKEN;
                }
                else
                {
                    Status = "Setup globe · add a map key to stream Earth";
                    Ready = true; return;
                }
                Cesium3DTileset.OnCesium3DTilesetLoadFailure += OnFailure;
                Tileset.OnTileGameObjectCreated += OnTileCreated;
                Tileset.gameObject.SetActive(true);
                Streaming = true; started = Time.unscaledTime;
                Status = "Streaming Earth…";
            }
            catch (Exception ex)
            {
                Status = "Map initialization failed · check local configuration";
                Debug.LogWarning("NATIONS: Map initialization failed (" + ex.GetType().Name + "). No credentials logged.");
                Ready = true; failed = true;
            }
        }
        void OnTileCreated(GameObject tile) { if (tile.GetComponentInChildren<Renderer>()) hasContent = true; }
        void OnFailure(Cesium3DTilesetLoadFailureDetails details)
        {
            if (details.tileset != Tileset) return;
            failed = true; Ready = true;
            Status = "Map unavailable · check network, API access and billing";
            Debug.LogWarning("NATIONS: Tileset request failed. Check internet, Map Tiles API access, key restrictions and billing. Request URL omitted to protect credentials.");
        }
        void Update()
        {
            if (!Streaming || Time.unscaledTime < nextPoll) return;
            nextPoll = Time.unscaledTime + 0.5f;
            LoadProgress = Tileset.ComputeLoadProgress();
            if (LoadProgress >= 95 && hasContent && !failed)
            {
                Ready = true; SetupGlobe.SetActive(false); Status = "";
            }
            else if (!Ready && Time.unscaledTime - started > Config.LoadingTimeout)
            {
                Ready = true; Status = "Still connecting · check network and map configuration";
            }
        }
        void OnDestroy()
        {
            Cesium3DTileset.OnCesium3DTilesetLoadFailure -= OnFailure;
            if (Tileset) Tileset.OnTileGameObjectCreated -= OnTileCreated;
        }
    }
}
