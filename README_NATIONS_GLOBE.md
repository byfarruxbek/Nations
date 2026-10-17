# NATIONS — Phase 1 globe

This implementation adds only the globe explorer. Existing `Assets/Scenes/WorldMapScene.unity`, scripts and prefabs are preserved. The new default scene is `Assets/_Nations/Scenes/Globe.unity`. Project-wide rendering changes to URP; previous project settings are backed up in `Documentation/*.before-phase1.*`.

## Open and run

1. In Unity Hub, open this `Nations` folder using **Unity 6000.6.2f1** (the project's existing version).
2. Let Package Manager resolve packages and finish importing. The Cesium native package is a large download.
3. Open `Assets/_Nations/Scenes/Globe.unity`. If configuring a fresh checkout before the generated scene is available, choose **Nations → Phase 1 → Configure and Open Globe**. This creates the scene only if it does not exist; it does not overwrite an existing globe scene.
4. Press Play. With no credentials, a dark blue gridded WGS84 setup ellipsoid and a setup message appear. This is deliberately identified as a setup view, not satellite imagery.
5. Add a local Google key as below, stop and restart Play, and the scene will stream real Google Photorealistic 3D Tiles. Alternatively, a Cesium ion token enables Cesium World Terrain (asset 1); imagery is not configured for that optional terrain-only fallback.

## Required packages

Pinned in `Packages/manifest.json`:

- `com.cesium.unity`: **1.26.0**
- `com.unity.render-pipelines.universal`: **17.6.0**
- `com.unity.inputsystem`: **1.20.0**
- `com.unity.mathematics`: **1.3.3**
- Existing UGUI and Unity Test Framework remain installed; other pre-existing dependencies are retained.

The configured scoped registry is:

```json
{"name":"Cesium","url":"https://unity.pkg.cesium.com","scopes":["com.cesium.unity"]}
```

The registry's `latest` tag reported 1.10.0 during setup, although the versions list includes 1.26.0. Use the explicit version. The official [release package](https://github.com/CesiumGS/cesium-unity/releases/tag/v1.26.0) is the reference.

## Google Cloud setup and local key

1. Open [Google Cloud Console](https://console.cloud.google.com/), create or select a project, and attach an active billing account.
2. Go to **APIs & Services → Library**, search **Map Tiles API**, and enable it for that project. This is separate from Maps SDK for Android/iOS.
3. Go to **APIs & Services → Credentials → Create credentials → API key**.
4. Edit the key. Under **API restrictions**, restrict it to **Map Tiles API**. Configure application restrictions appropriate to your actual request transport. Cesium sends native HTTPS requests; Android package/SHA restrictions or iOS bundle restrictions should not be assumed to work just because the player runs on those platforms. Validate the supported restriction scheme before distributing a client. Use a separate development key and configure quotas/billing alerts in Google Cloud.
5. Copy `Assets/_Nations/Config/MapConfig.example.json` to:

   `Assets/_Nations/Config/Resources/NationsMapConfig.local.json`

6. Replace `YOUR_API_KEY` in that local file:

```json
{
  "GOOGLE_MAP_TILES_API_KEY": "YOUR_API_KEY",
  "CESIUM_ION_ACCESS_TOKEN": ""
}
```

7. Restart Play. `GlobeRuntime` constructs:

   `https://tile.googleapis.com/v1/3dtiles/root.json?key=<encoded-key>`

The local JSON and its `.meta` are ignored by Git. Never paste a real key into the scene, tracked config asset, scripts, example, or README. The JSON is included in built players through Resources; this keeps secrets out of source control but does **not** make a distributed API key secret. Inject the file securely in CI when building. Do not share raw native/network logs without checking them for request URLs.

See Google's [Map Tiles API setup](https://developers.google.com/maps/documentation/tile/get-api-key) and [Photorealistic 3D Tiles guide](https://developers.google.com/maps/documentation/tile/3d-tiles). No 2D session-token request is needed for this 3D root URL workflow.

## Attribution

`showCreditsOnScreen` is set in the scene and enforced at runtime. Cesium's official credit UI displays the returned copyright strings, images and links. `CreditSafeArea` moves its on-screen container within `Screen.safeArea` and permits wrapping without replacing its contents. The map itself remains full-screen.

Do not disable Cesium's credit system, cover it with future UI, or crop screenshots that require attribution. Validate credits with real Google data on notched devices in portrait and landscape. A separate **Google Maps** text attribution appears at the safe area's top-right while Google tiles are active; it does not replace or overlap the data credits below. Review both with real data before release. See [Google's attribution policy](https://developers.google.com/maps/documentation/tile/policies).

## Controls

| Input | Action |
|---|---|
| One-finger drag | Grab/orbit the ellipsoid; brief decaying inertia on release |
| Two-finger spread | Zoom in toward the midpoint |
| Two-finger pinch together | Zoom out from the midpoint |
| Two-finger twist | Rotate heading after a 5-degree threshold |
| Two fingers moving vertically together | Tilt after a 12-pixel threshold, near the surface |
| Left mouse drag | Orbit |
| Wheel / trackpad scrolling | Smooth zoom toward pointer |
| Right mouse drag / Alt or Shift + left drag | Tilt and heading |

Touch identity/count changes reset gesture baselines, preventing jumps when fingers are added or removed. UI interactions are excluded. Touch wins over mouse while active; desktop mouse is disabled on mobile players. Phase 1 uses the Input System backend exclusively, including Android. The old scene and scripts are preserved, but its legacy controls require temporarily selecting Both input backends when revisiting that scene.

## Camera implementation

`GlobeMath` contains double-precision WGS84 ellipsoid intersection, geodetic surface positions and stable axis-angle transport. `GlobeCameraController` maintains ECEF navigation state and rebases the Cesium origin when the camera moves more than 1 km from it. No float Euler latitude/longitude orbit accumulates at the poles.

- Initial focus: approximately 38°E, 30°N (Europe / North Africa / western Asia), from space.
- Framing fits the globe to the viewport; portrait necessarily has less Earth coverage vertically when the whole disk must fit horizontally.
- Zoom uses logarithmic distance: large moves in space and fine moves close to the ground.
- Focus, tilt, heading and zoom are damped. Angular swipe velocity decays exponentially.
- Pitch is measured from nadir: 0° looks down, up to 68° near the surface. Tilt smoothly reduces at space scale.
- Ellipsoid altitude floor: 80 m. Maximum orbit distance: 35,000 km. Near/far planes adapt to altitude.
- Zoom anchoring retains a fixed geographic ellipsoid point across the gesture and applies two corrective rotations after each eased zoom step. It falls back to centered zoom when the ray misses Earth. This is an isolated ellipsoid approximation; it does not ray-pick individual rooftops.
- Physics meshes are enabled for streamed tiles. A radial clearance ray from above terrestrial relief lifts the camera above loaded geometry, with eased descent afterward.

Cesium's Dynamic Camera was evaluated as the reusable reference: its altitude-aware free-flight and origin/anchor concepts are useful, but its standard controls do not implement the requested mobile grab/pinch/twist behavior. Phase 1 therefore uses a dedicated orbit controller with Cesium georeference transforms, without attaching a competing Dynamic Camera or origin-shift controller.

Inspector tuning lives on `GlobeCameraController`: MinAltitude, MaxAltitude, MinPitch, MaxPitch, OrbitSensitivity, ZoomSensitivity, TiltSensitivity, RotationSensitivity, Inertia, Damping, and TerrainClearance.

## Rendering and quality

`Assets/_Nations/Settings/NationsURP.asset` uses forward URP, no HDR/post-processing, no realtime shadows, and 2x MSAA. The space skybox uses a sparse procedural star field. The credential-free ellipsoid has restrained edge shading and a coordinate grid. Real tiles retain their imagery/materials; no fake night overlay or heavy atmosphere is applied. Photogrammetry is often prelit, so a physically changing day/night terminator is not promised.

`Assets/_Nations/Config/MapConfig.asset` exposes the quality profile without a menu:

| Profile | Screen-space error | Concurrent loads | Tile cache budget |
|---|---:|---:|---:|
| Low | 24 | 6 | 128 MiB |
| Medium (default) | 16 | 10 | 256 MiB |
| High | 8 | 16 | 512 MiB |

The target frame rate is 60. Budgets are tile-cache limits, not total process memory caps. The 30–60 FPS target requires device profiling; it is not a measured performance guarantee.

## Android build

1. Install Unity Hub's **Android Build Support**, SDK/NDK and OpenJDK modules for 6000.6.2f1.
2. Add the local map config if real tiles are desired in the build.
3. Choose **Nations → Phase 1 → Build Android APK**. Output: `Builds/Android/NationsGlobe.apk`.
4. Alternatively use File → Build Profiles → Android. The globe is the only enabled scene. Use a development build for the diagnostic overlay, and turn Development Build off for production.
5. Settings enforced by the setup script: **IL2CPP**, **ARM64 only**, **Internet Access Require**, minimum Android API **26**, portrait, linear color, URP.
6. The included APK was successfully built and inspected for ARM64-only Cesium/IL2CPP libraries, internet permission, minimum SDK 26 and target SDK 36. It has not been run on a physical device. The prior `Builds/Android/Nations.apk` is preserved.
7. For store release, configure your own application ID, signing, version, target SDK and AAB settings. The sample identifier is `com.nations.globe`.

Cesium's [supported-platform requirements](https://cesium.com/learn/cesium-unity/ref-doc/supported-platforms.html) require IL2CPP and a supported 64-bit Android architecture.

## iOS

Install iOS Build Support for this Unity version, switch to iOS in Build Profiles, and export an Xcode project. Configure your team/signing and bundle ID in Xcode. Setup selects IL2CPP and iOS 15 minimum; modern Unity iOS builds target ARM64. iOS tooling was not installed on this machine during implementation. Verify device performance, credits and touch gestures before distribution.

## Loading and failures

A small spinner fades once view-load progress reaches 95% and a renderable tile has been created, a setup view is selected, or a timeout/failure is reported. The renderer and controls remain responsive while streaming. A small status line explains missing configuration, timeout or root-request failure. Failed API access does not intentionally throw; native initialization exceptions are caught. Diagnostic status is separate from map attribution.

Development builds and Editor display FPS, ellipsoid altitude, latitude/longitude, pitch, heading and view-load percentage. The diagnostic object is disabled in non-development players. View-load percentage is not a loaded-tile count.

## Verification

- **Nations → Phase 1 → Run Geometry and Configuration Checks** checks WGS84 positions, ray intersections, pole transport, long-orbit numerical stability and Android settings.
- `Nations.Editor.GlobeSmokeTest.Run` enters Play Mode and exercises the controller: startup, drag, inertia, off-center zoom to city scale, tilt/heading, and zoom limits. Reports and screenshots are written under `Documentation/`.
- See `Documentation/VALIDATION.md` for the actual execution results and remaining acceptance tests. Controller simulation does not replace physical multitouch, Google streaming or Android/iOS device tests.

Example batch setup:

```sh
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -projectPath "$PWD" \
  -executeMethod Nations.Editor.GlobeSetup.ConfigureBatch -quit \
  -logFile Logs/nations-setup.log
```

## Current limits and next step

No key is included, so real terrain/imagery, Google authorization and live attribution require your credentials. Terrain avoidance can only respond to loaded colliders; it cannot guarantee clearance over geometry that has not streamed yet. Zoom anchoring uses the ellipsoid rather than the terrain surface. Gesture thresholds and touch feel require physical-device tuning. Landscape safe-area and aspect logic are implemented, but launch orientation is portrait. No separate atmospheric shell is added to the real tiles.

Recommended next work remains within Phase 1: test real streaming on an Android device and an iPhone, profile memory/frame time, then refine terrain-aware camera clearance and touch anchoring with actual streamed height queries. Do not add gameplay until the acceptance matrix passes on devices.

## File inventory

See `Documentation/FILES.md` for all added and modified paths, including Unity-generated scene/material/config assets and `.meta` files.


## Exploration interface and localization

The existing Globe scene now includes local country details, three-language settings and the compact × menu. Setup, data licensing, limitations and validation: [Documentation/DATA_LOCALIZATION.md](Documentation/DATA_LOCALIZATION.md). The working Cesium ion configuration is preserved.
