using System.IO;
using CesiumForUnity;
using Nations.Globe;
using Nations.Input;
using Nations.Navigation;
using Nations.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Nations.Editor
{
    public static class GlobeSetup
    {
        public const string ScenePath = "Assets/_Nations/Scenes/Globe.unity";
        const string Root = "Assets/_Nations/";
        [MenuItem("Nations/Phase 1/Configure and Open Globe")]
        public static void Configure()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ConfigureProject();
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
            CreateScene();
        }
        public static void ConfigureBatch() { Configure(); SetupGlobeMesh.Refresh(); GlobeChecks.Run(); }
        static void ConfigureProject()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Root + "Settings/NationsURP.asset");
            if (!pipeline)
            {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, Root + "Settings/NationsRenderer.asset");
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.supportsHDR = false;
                pipeline.msaaSampleCount = 2;
                pipeline.renderScale = 1;
                pipeline.shadowDistance = 0;
                pipeline.supportsCameraDepthTexture = false;
                pipeline.supportsCameraOpaqueTexture = false;
                AssetDatabase.CreateAsset(pipeline, Root + "Settings/NationsURP.asset");
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            MoveGeneratedAsset("Assets/UniversalRenderPipelineGlobalSettings.asset", Root + "Settings/UniversalRenderPipelineGlobalSettings.asset");
            MoveGeneratedAsset("Assets/DefaultVolumeProfile.asset", Root + "Settings/DefaultVolumeProfile.asset");
            int originalQuality = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false); QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(originalQuality, false);
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.nations.globe");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.nations.globe");
            // Android uses the current Input System backend exclusively.
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            settings.FindProperty("activeInputHandler").intValue = 1;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var existing = EditorBuildSettings.scenes;
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
            foreach (var scene in existing) if (scene.path != ScenePath) scenes.Add(new EditorBuildSettingsScene(scene.path, false));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }
        static void MoveGeneratedAsset(string source, string destination)
        {
            if (AssetDatabase.LoadMainAssetAtPath(source) && !AssetDatabase.LoadMainAssetAtPath(destination))
                AssetDatabase.MoveAsset(source, destination);
        }
        static GameObject Child(string name, Transform parent)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); return go;
        }
        static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("NATIONS");
            var geo = Child("CesiumGeoreference", root.transform).AddComponent<CesiumGeoreference>();
            geo.SetOriginLongitudeLatitudeHeight(38, 30, 0);
            var tileObject = Child("EarthTileset", geo.transform); tileObject.SetActive(false);
            var tiles = tileObject.AddComponent<Cesium3DTileset>();
            tiles.showCreditsOnScreen = true; tiles.updateInEditor = false;
            tiles.tilesetSource = CesiumDataSource.FromUrl; tiles.url = "";
            tiles.maximumScreenSpaceError = 16; tiles.maximumSimultaneousTileLoads = 10;
            tiles.maximumCachedBytes = 256L * 1024 * 1024; tiles.createPhysicsMeshes = true;
            tiles.preloadSiblings = false;
            var rig = Child("CameraRig", geo.transform);
            var cam = Child("Main Camera", rig.transform).AddComponent<UnityEngine.Camera>();
            cam.tag = "MainCamera"; cam.fieldOfView = 45; cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.001f, 0.002f, 0.004f);
            cam.allowHDR = false; cam.gameObject.AddComponent<AudioListener>();
            cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            var controller = Child("GlobeCameraController", root.transform).AddComponent<GlobeCameraController>();
            controller.Georeference = geo; controller.View = cam;
            var input = Child("GlobeInputManager", root.transform).AddComponent<GlobeInputManager>(); input.Controller = controller;
            var sun = Child("Lighting", root.transform).AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 1; sun.shadows = LightShadows.None;
            sun.transform.rotation = Quaternion.Euler(35, -35, 0);
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.3f);
            var sky = new Material(Shader.Find("Nations/Quiet Space")); AssetDatabase.CreateAsset(sky, Root + "Materials/QuietSpace.mat"); RenderSettings.skybox = sky;
            var setup = GameObject.CreatePrimitive(PrimitiveType.Sphere); setup.name = "Setup Ellipsoid (no map credentials)";
            Object.DestroyImmediate(setup.GetComponent<Collider>()); setup.transform.SetParent(root.transform, false);
            var setupMaterial = new Material(Shader.Find("Nations/Setup Globe"));
            AssetDatabase.CreateAsset(setupMaterial, Root + "Materials/SetupGlobe.mat");
            setup.GetComponent<Renderer>().sharedMaterial = setupMaterial;
            setup.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            setup.AddComponent<SetupEllipsoid>().Georeference = geo;
            var config = ScriptableObject.CreateInstance<MapConfig>();
            AssetDatabase.CreateAsset(config, Root + "Config/MapConfig.asset");
            var runtime = root.AddComponent<GlobeRuntime>(); runtime.Config = config; runtime.Tileset = tiles; runtime.SetupGlobe = setup;
            var events = Child("EventSystem", root.transform); events.AddComponent<EventSystem>();
            events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            BuildUI(root.transform, runtime, controller);
            root.AddComponent<CreditSafeArea>();
            controller.Initialize();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("NATIONS: Globe scene configured and saved.");
        }
        static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.sizeDelta = size; rect.anchoredPosition = position;
            return rect;
        }
        static Text Label(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 position, int fontSize)
        {
            var rect = Rect(name, parent, anchor, anchor, size, position);
            var text = rect.gameObject.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize; text.color = new Color(0.72f, 0.8f, 0.88f); text.raycastTarget = false;
            text.alignment = TextAnchor.MiddleCenter; return text;
        }
        static void BuildUI(Transform parent, GlobeRuntime runtime, GlobeCameraController controller)
        {
            var ui = new GameObject("UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            ui.transform.SetParent(parent, false); ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = ui.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(430, 932); scaler.matchWidthOrHeight = 0.5f;
            var safe = Rect("SafeArea", ui.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var loading = Rect("LoadingIndicator", safe, new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(20, 20), Vector2.zero);
            var spinner = loading.gameObject.AddComponent<LoadingSpinner>(); spinner.color = new Color(0.7f, 0.82f, 0.92f); spinner.raycastTarget = false;
            var group = loading.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false;
            var status = Label("Status", safe, new Vector2(0.5f, 0.24f), new Vector2(340, 44), Vector2.zero, 13);
            var debug = Label("DebugPanel", safe, new Vector2(0.5f, 1), new Vector2(390, 70), new Vector2(0, -90), 11);
            var overlay = ui.AddComponent<GlobeOverlay>(); overlay.Runtime = runtime; overlay.Controller = controller;
            overlay.SafeRoot = safe; overlay.Loading = group; overlay.Spinner = loading; overlay.Status = status; overlay.DebugText = debug;
        }
        [MenuItem("Nations/Phase 1/Build Android APK")]
        public static void BuildAndroid()
        {
            ConfigureProject();
            Directory.CreateDirectory("Builds/Android");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = "Builds/Android/NationsGlobe.apk",
                target = BuildTarget.Android, options = BuildOptions.None
            });
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("NATIONS Android build failed: " + result.summary.result);
        }
    }
}
