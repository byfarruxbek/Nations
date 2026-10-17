# Exploration menu simplification — 2026-10-17

Updated the existing runtime interface, without replacing the Globe scene or map credentials.

- Removed the introductory heading, scroll helper and large resume button in Uzbek, English and Russian.
- Added a fixed 44-unit × button at the top-right; settings move upward and retain the vertical ScrollRect for constrained screens.
- Preserved language selection, music/SFX controls, layer switches, country card, safe-area layout and native map attribution.

Implementation: `Assets/_Nations/Scripts/UI/ExplorationUI.cs`; removed strings in `Assets/_Nations/Resources/NationsLocalization.json` and `Tools/prepare_localization.py`. Regression checks: `Assets/_Nations/Editor/LocalizationChecks.cs`, `ExperienceChecks.cs`, `ExperienceNavigationChecks.cs`, `ExperienceUpgrade.cs`.

Verified in Unity 6000.6.2f1 portrait Play Mode at 1080×1920: native menu open/× close and language/music clicks; all three language variants; input unlock; Input System drag, zoom and country selection; UI raycast blocking; setting persistence; simulated notched safe-area containment. Earth streaming reached 100%, fallback disabled, required credits visible. Clean Play Mode restart Console showed 0 errors; existing Cesium coarse collision-mesh and editor account API timeout warnings remain. Physical mobile touch was not tested. Scrolling is retained by implementation; a physical short-screen scroll test remains advisable.

Additional localization/data work from the ongoing request is documented in DATA_LOCALIZATION.md, including licensing and known missing metadata.
