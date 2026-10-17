# Phase 1 verification record

Unity: 6000.6.2f1 on macOS Apple Silicon. Cesium: official registry 1.26.0. The downloaded release matched registry SHA-1 `d454711fbe38ed70a4a044d50941c76db43009e1`. The final dependency is a registry dependency; no temporary embedded package remains in the project.

## Executed

- C# compilation succeeded. New Phase 1 scripts have no compiler errors.
- Scene generation and serialization succeeded: `Assets/_Nations/Scenes/Globe.unity`.
- Geometry/configuration checks passed: see `geometry-checks.txt`.
- Rendered Play Mode checks passed: see `playmode-checks.txt`.
- World-to-city zoom-anchor error in the final Input System test: approximately **0.00024 metres** in the tested ellipsoid-only scenario. This is a numerical result, not an accuracy claim for real imagery or terrain.
- Input System virtual mouse drag and wheel events passed through the actual `GlobeInputManager`. Batch tests temporarily route input to the unfocused Game view and restore settings afterward.
- `globe-space.png` and `globe-close.png` are camera-only render captures of the **credential-free setup ellipsoid**, not satellite imagery and not screenshots of the loading/credit UI.
- Android ARM64 IL2CPP APK build **succeeded**. Manifest inspection confirmed minimum SDK 26, target SDK 36, internet permission and ARM64-only native libraries. See `android-build-checks.txt`. APK: `Builds/Android/NationsGlobe.apk` (about 46 MiB). Not installed on a physical device.

## Acceptance matrix

| Test | Result / remaining evidence |
|---|---|
| 1. Earth visible from space | Setup ellipsoid renders. Real streamed Earth requires a Google key or ion token; not verified. |
| 2. Smooth horizontal drag | Controller and Input System virtual mouse orbit passed. Physical touch feel pending. |
| 3. Swipe inertia | Controller release/inertia passed. |
| 4. Pinch altitude | Zoom controller passed; two-finger hardware testing pending. |
| 5. World to country | World-to-regional scale simulation passed; geographic imagery unavailable without credentials. |
| 6. City zoom speed | Logarithmic zoom reached city-scale altitude and retained its geographic anchor. |
| 7. Two-finger heading | Heading controller passed; twist gesture hardware testing pending. |
| 8. Two-finger tilt | Tilt controller passed; two-finger gesture hardware testing pending. |
| 9. No Earth penetration | Ellipsoid floor passed. Streamed-terrain collision and unloaded geometry remain unverified. |
| 10. No obvious jitter | Numerical/orbit tests passed; subjective feel and frame pacing need device review. |
| 11. No major console errors | No Phase 1 runtime errors during passing tests. See upstream Editor caveat below. |
| 12. Android ARM64 | Passed configuration checks and actual APK build; ARM64-only native libraries and internet permission verified in APK. |
| 13. Attribution visible | Native credit system retained, show-on-screen enabled and safe-area adapter wired. Live Google attribution needs credentialed device verification. |
| 14. Editor controls | Virtual mouse drag and wheel passed through Input System; physical trackpad testing pending. |

## Known upstream Editor messages

Cesium 1.26's ion Editor integration starts an app-data request even when using the credential-free scene. Entering Play Mode while that request is pending can log two cancellation errors during assembly reload (`Request was canceled because the Unity AppDomain is reloading`, followed by failure to resume the ion connection). They occur before Play Mode, outside the Phase 1 runtime. The tests retain these messages in the log and distinguish them from runtime errors; the package has not been modified to suppress logging.

The project also retains pre-existing deprecated Analytics/IAP packages and legacy script warnings. Cesium itself reports some obsolete Unity API warnings. No unrelated gameplay code was removed to hide those warnings.

## Remaining release checks

Supply a restricted development map key, run on a midrange ARM64 Android phone and an iPhone, verify all one-/two-finger transitions, notched safe areas and live credits, exercise offline/invalid-key/rate-limit conditions, and profile sustained frame time, memory and tile churn. Test high-relief terrain and late-arriving collision meshes. The current collision guard only knows loaded geometry; ellipsoid anchoring does not target individual buildings.

The APK inherits the BILLING permission from the pre-existing Unity Purchasing package. No purchase UI or gameplay was added. Review retained legacy dependencies before publishing. The old APK remains at its original path and is also backed up as Nations.before-phase1.apk.
