# Phase 1 file inventory

Paths are relative to the Nations project. Unity `.meta` files are listed explicitly. Generated Library/Logs/UserSettings files are excluded.

## Added source and documentation

- `.gitignore`
- `Assets/CesiumSettings.meta`
- `Assets/CesiumSettings/Resources.meta`
- `Assets/CesiumSettings/Resources/CesiumIonServers.meta`
- `Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset`
- `Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset.meta`
- `Assets/CesiumSettings/Resources/CesiumRuntimeSettings.asset`
- `Assets/CesiumSettings/Resources/CesiumRuntimeSettings.asset.meta`
- `Assets/_Nations.meta`
- `Assets/_Nations/Config.meta`
- `Assets/_Nations/Config/MapConfig.asset`
- `Assets/_Nations/Config/MapConfig.asset.meta`
- `Assets/_Nations/Config/MapConfig.example.json`
- `Assets/_Nations/Config/MapConfig.example.json.meta`
- `Assets/_Nations/Config/Resources.meta`
- `Assets/_Nations/Editor.meta`
- `Assets/_Nations/Editor/GlobeChecks.cs`
- `Assets/_Nations/Editor/GlobeChecks.cs.meta`
- `Assets/_Nations/Editor/GlobeSetup.cs`
- `Assets/_Nations/Editor/GlobeSetup.cs.meta`
- `Assets/_Nations/Editor/GlobeSmokeTest.cs`
- `Assets/_Nations/Editor/GlobeSmokeTest.cs.meta`
- `Assets/_Nations/Editor/SetupGlobeMesh.cs`
- `Assets/_Nations/Editor/SetupGlobeMesh.cs.meta`
- `Assets/_Nations/Materials.meta`
- `Assets/_Nations/Materials/QuietSpace.mat`
- `Assets/_Nations/Materials/QuietSpace.mat.meta`
- `Assets/_Nations/Materials/QuietSpace.shader`
- `Assets/_Nations/Materials/QuietSpace.shader.meta`
- `Assets/_Nations/Materials/SetupEllipsoidMesh.asset`
- `Assets/_Nations/Materials/SetupEllipsoidMesh.asset.meta`
- `Assets/_Nations/Materials/SetupGlobe.mat`
- `Assets/_Nations/Materials/SetupGlobe.mat.meta`
- `Assets/_Nations/Materials/SetupGlobe.shader`
- `Assets/_Nations/Materials/SetupGlobe.shader.meta`
- `Assets/_Nations/Prefabs.meta`
- `Assets/_Nations/Scenes.meta`
- `Assets/_Nations/Scenes/Globe.unity`
- `Assets/_Nations/Scenes/Globe.unity.meta`
- `Assets/_Nations/Scripts.meta`
- `Assets/_Nations/Scripts/Camera.meta`
- `Assets/_Nations/Scripts/Camera/GlobeCameraController.cs`
- `Assets/_Nations/Scripts/Camera/GlobeCameraController.cs.meta`
- `Assets/_Nations/Scripts/Core.meta`
- `Assets/_Nations/Scripts/Core/GlobeRuntime.cs`
- `Assets/_Nations/Scripts/Core/GlobeRuntime.cs.meta`
- `Assets/_Nations/Scripts/Core/MapConfig.cs`
- `Assets/_Nations/Scripts/Core/MapConfig.cs.meta`
- `Assets/_Nations/Scripts/Globe.meta`
- `Assets/_Nations/Scripts/Globe/GlobeMath.cs`
- `Assets/_Nations/Scripts/Globe/GlobeMath.cs.meta`
- `Assets/_Nations/Scripts/Globe/SetupEllipsoid.cs`
- `Assets/_Nations/Scripts/Globe/SetupEllipsoid.cs.meta`
- `Assets/_Nations/Scripts/Input.meta`
- `Assets/_Nations/Scripts/Input/GlobeInputManager.cs`
- `Assets/_Nations/Scripts/Input/GlobeInputManager.cs.meta`
- `Assets/_Nations/Scripts/UI.meta`
- `Assets/_Nations/Scripts/UI/CreditSafeArea.cs`
- `Assets/_Nations/Scripts/UI/CreditSafeArea.cs.meta`
- `Assets/_Nations/Scripts/UI/GlobeOverlay.cs`
- `Assets/_Nations/Scripts/UI/GlobeOverlay.cs.meta`
- `Assets/_Nations/Scripts/UI/LoadingSpinner.cs`
- `Assets/_Nations/Scripts/UI/LoadingSpinner.cs.meta`
- `Assets/_Nations/Settings.meta`
- `Assets/_Nations/Settings/DefaultVolumeProfile.asset`
- `Assets/_Nations/Settings/DefaultVolumeProfile.asset.meta`
- `Assets/_Nations/Settings/NationsRenderer.asset`
- `Assets/_Nations/Settings/NationsRenderer.asset.meta`
- `Assets/_Nations/Settings/NationsURP.asset`
- `Assets/_Nations/Settings/NationsURP.asset.meta`
- `Assets/_Nations/Settings/UniversalRenderPipelineGlobalSettings.asset`
- `Assets/_Nations/Settings/UniversalRenderPipelineGlobalSettings.asset.meta`
- `Documentation/EditorBuildSettings.before-phase1.txt`
- `Documentation/FILES.md`
- `Documentation/GraphicsSettings.before-phase1.txt`
- `Documentation/ProjectSettings.before-phase1.txt`
- `Documentation/QualitySettings.before-phase1.txt`
- `Documentation/VALIDATION.md`
- `Documentation/android-build-checks.txt`
- `Documentation/geometry-checks.txt`
- `Documentation/globe-close.png`
- `Documentation/globe-space.png`
- `Documentation/manifest.before-phase1.json`
- `Documentation/playmode-checks.txt`
- `ProjectSettings/ShaderGraphSettings.asset`
- `ProjectSettings/URPProjectSettings.asset`
- `README_NATIONS_GLOBE.md`

## Modified

- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `ProjectSettings/EditorBuildSettings.asset`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/QualitySettings.asset`

## Ignored build artifacts

- `Builds/Android/NationsGlobe.apk` — new Phase 1 APK.
- `Builds/Android/Nations.before-phase1.apk` — backup of the original APK; original also restored at `Builds/Android/Nations.apk`.

Original scene, script, prefab and sprite files were not edited. Previous project settings are backed up in Documentation. Cesium generates its own Assets/CesiumSettings directory. URP settings are under Assets/_Nations/Settings.
