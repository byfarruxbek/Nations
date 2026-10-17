# 🌍 NATIONS

**UZ:** Unity va Cesium asosida yaratilayotgan interaktiv 3D dunyo xaritasi va geografik platforma.

**EN:** An interactive 3D world exploration and geography platform built with Unity and Cesium.

---

### Languages

[🇺🇿 O‘zbekcha](#-ozbekcha) • [🇬🇧 English](#-english)

---

# 🇺🇿 O‘zbekcha

## Loyiha haqida

**NATIONS** — Unity va Cesium yordamida yaratilayotgan interaktiv 3D dunyo loyihasi.

Hozirgi **Phase 1** bosqichida loyiha asosan 3D globus va dunyoni o‘rganish interfeysiga qaratilgan.

Mavjud:

- Interaktiv 3D Yer
- Cesium for Unity integratsiyasi
- Google Photorealistic 3D Tiles
- Mobil va desktop kamera boshqaruvi
- Pinch-to-zoom
- Orbit
- Tilt
- Heading rotation
- Touch gesture tizimi
- WGS84 koordinata tizimi
- Android build
- URP rendering
- Country exploration interfeysi
- Lokalizatsiya
- Uch tilli sozlamalar

Asosiy sahna:

```text
Assets/_Nations/Scenes/Globe.unity
```

Eski:

```text
Assets/Scenes/WorldMapScene.unity
```

sahna, scriptlar va prefablar saqlab qolingan.

---

## 🚀 Loyihani ishga tushirish

### 1. Unity Hub orqali ochish

`Nations` papkasini:

```text
Unity 6000.6.2f1
```

versiyasida oching.

### 2. Package Manager

Unity barcha package va dependency'larni yuklab bo‘lishini kuting.

Cesium native package hajmi katta bo‘lgani sababli birinchi import biroz vaqt olishi mumkin.

### 3. Globe sahnasini oching

```text
Assets/_Nations/Scenes/Globe.unity
```

Agar sahna hali generatsiya qilinmagan bo‘lsa:

```text
Nations → Phase 1 → Configure and Open Globe
```

buyrug‘idan foydalaning.

### 4. Play tugmasini bosing

API credentials bo‘lmasa, loyiha WGS84 asosidagi ko‘k setup globe'ni ko‘rsatadi.

Haqiqiy 3D Yer ma'lumotlari uchun Google Map Tiles API key yoki Cesium ion konfiguratsiyasi kerak.

---

## 📦 Asosiy package'lar

`Packages/manifest.json`:

| Package | Version |
|---|---|
| Cesium for Unity | 1.26.0 |
| Universal Render Pipeline | 17.6.0 |
| Unity Input System | 1.20.0 |
| Unity Mathematics | 1.3.3 |

Cesium registry:

```json
{
  "name": "Cesium",
  "url": "https://unity.pkg.cesium.com",
  "scopes": ["com.cesium.unity"]
}
```

---

# 🗺 Google Photorealistic 3D Tiles

## Google Cloud sozlamalari

Google Cloud Console'da yangi loyiha yarating yoki mavjud loyihani tanlang.

So‘ng:

```text
APIs & Services
→ Library
→ Map Tiles API
→ Enable
```

Keyin:

```text
APIs & Services
→ Credentials
→ Create credentials
→ API key
```

API key'ni imkon qadar faqat:

```text
Map Tiles API
```

uchun cheklash tavsiya qilinadi.

---

## 🔑 Local API konfiguratsiyasi

Quyidagi faylni:

```text
Assets/_Nations/Config/MapConfig.example.json
```

nusxalang va:

```text
Assets/_Nations/Config/Resources/NationsMapConfig.local.json
```

nomi bilan saqlang.

Ichiga:

```json
{
  "GOOGLE_MAP_TILES_API_KEY": "YOUR_API_KEY",
  "CESIUM_ION_ACCESS_TOKEN": ""
}
```

yozing.

### ⚠️ Muhim

Real API key'ni:

- GitHub repository'ga
- README'ga
- Scene ichiga
- Script ichiga
- public config faylga

joylamang.

Local konfiguratsiya Git orqali ignore qilinishi kerak.

---

# © Attribution

Google va Cesium ma'lumotlaridan foydalanishda attribution ko‘rsatilishi kerak.

Loyiha Cesium'ning rasmiy credit tizimidan foydalanadi.

```text
showCreditsOnScreen = true
```

Google tiles ishlayotganida Google Maps attribution'i ham ko‘rinadi.

Credit'larni:

- yashirmang;
- boshqa UI bilan yopmang;
- screenshotlardan kesib tashlamang.

---

# 🎮 Controls

## 📱 Mobile

| Gesture | Harakat |
|---|---|
| Bir barmoq bilan drag | Globe orbit |
| Ikki barmoqni yoyish | Zoom in |
| Ikki barmoqni yaqinlashtirish | Zoom out |
| Ikki barmoq bilan twist | Heading rotation |
| Ikki barmoqni vertikal harakatlantirish | Tilt |

Orbit harakatidan keyin qisqa inertia ishlaydi.

---

## 🖥 Desktop

| Input | Harakat |
|---|---|
| Left mouse drag | Orbit |
| Mouse wheel / trackpad | Smooth zoom |
| Right mouse drag | Tilt / heading |
| Alt + Left Drag | Tilt / heading |
| Shift + Left Drag | Tilt / heading |

Touch faol bo‘lganida touch input mouse input'dan ustun turadi.

---

# 📷 Kamera tizimi

Kamera tizimi:

```text
GlobeCameraController
```

orqali boshqariladi.

`GlobeMath` quyidagi matematik funksiyalarni bajaradi:

- Double precision WGS84
- Ellipsoid intersection
- Geodetic surface positions
- Axis-angle transport

Kamera ECEF koordinata tizimidan foydalanadi.

Cesium Origin kamera undan taxminan:

```text
1 km
```

uzoqlashganda qayta joylashtiriladi.

---

## Kamera parametrlar

### Boshlang‘ich hudud

Taxminan:

```text
38° E
30° N
```

Ya'ni:

- Yevropa
- Shimoliy Afrika
- G‘arbiy Osiyo

tomoni.

### Pitch

```text
0° → Nadir
68° → Maksimal surface tilt
```

### Minimal altitude

```text
80 m
```

### Maksimal orbit distance

```text
35,000 km
```

Zoom logarithmic usulda ishlaydi:

- kosmosda katta qadamlar;
- Yerga yaqinlashganda aniqroq harakat.

---

# 🎨 Rendering

Loyiha:

```text
Universal Render Pipeline
```

dan foydalanadi.

Asosiy URP asset:

```text
Assets/_Nations/Settings/NationsURP.asset
```

Hozirgi sozlamalar:

- Forward URP
- 2x MSAA
- HDR yo‘q
- Post Processing yo‘q
- Realtime shadows yo‘q
- Procedural star field
- 60 FPS target

Google Photogrammetry o‘z material va imagery'sini saqlaydi.

Sun'iy night overlay hozircha qo‘llanilmagan.

---

# ⚙️ Quality Profiles

```text
Assets/_Nations/Config/MapConfig.asset
```

| Profile | Screen-space error | Concurrent loads | Tile cache |
|---|---:|---:|---:|
| Low | 24 | 6 | 128 MiB |
| Medium | 16 | 10 | 256 MiB |
| High | 8 | 16 | 512 MiB |

Default:

```text
Medium
```

Target:

```text
60 FPS
```

Real performance qurilma kuchiga bog‘liq.

---

# 🤖 Android build

Unity Hub orqali quyidagilarni o‘rnating:

```text
Android Build Support
Android SDK
Android NDK
OpenJDK
```

Keyin:

```text
Nations
→ Phase 1
→ Build Android APK
```

Build:

```text
Builds/Android/NationsGlobe.apk
```

ga yoziladi.

---

## Android sozlamalari

```text
Scripting Backend: IL2CPP
Architecture: ARM64
Minimum Android API: 26
Color Space: Linear
Internet Access: Require
Rendering: URP
```

Sample identifier:

```text
com.nations.globe
```

Release versiyada o‘zingizning:

- Application ID
- Signing
- Version
- Target SDK
- AAB

sozlamalaringizni o‘rnatishingiz kerak.

---

# 🍎 iOS

Unity Hub'dan:

```text
iOS Build Support
```

o‘rnating.

Build Profiles orqali iOS'ga switch qilib Xcode project export qiling.

Xcode'da:

- Team
- Signing
- Bundle ID

sozlanadi.

Minimal iOS versiya:

```text
iOS 15
```

Architecture:

```text
ARM64
```

---

# ⏳ Loading

Tiles yuklanayotganda kichik loading spinner ko‘rinadi.

Spinner quyidagi holatlardan birida yo‘qoladi:

- View load 95% ga yetganda
- Render qilinadigan tile paydo bo‘lganda
- Setup view ishga tushganda
- Timeout yuz berganda
- Failure qayd etilganda

Streaming paytida kamera va boshqaruv ishlashda davom etadi.

---

# 🛠 Diagnostics

Development build va Unity Editor'da quyidagilar ko‘rsatiladi:

- FPS
- Altitude
- Latitude
- Longitude
- Pitch
- Heading
- View Load %

Production build'da diagnostic overlay o‘chiriladi.

---

# ✅ Verification

Unity menyusidan:

```text
Nations
→ Phase 1
→ Run Geometry and Configuration Checks
```

quyidagilar tekshiriladi:

- WGS84 position
- Ray intersection
- Pole transport
- Numerical stability
- Android settings

---

## Smoke Test

```text
Nations.Editor.GlobeSmokeTest.Run
```

quyidagilarni sinaydi:

- Startup
- Drag
- Inertia
- Off-center zoom
- City-scale zoom
- Tilt
- Heading
- Zoom limits

Hisobotlar:

```text
Documentation/
```

papkasiga yoziladi.

---

# 🧪 Batch Setup

```sh
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -projectPath "$PWD" \
  -executeMethod Nations.Editor.GlobeSetup.ConfigureBatch -quit \
  -logFile Logs/nations-setup.log
```

---

# ⚠️ Hozirgi cheklovlar

Hozirgi Phase 1 versiyada:

- API key repository ichida yo‘q
- Real Google streaming uchun local credentials kerak
- Terrain clearance faqat yuklangan collider'larga bog‘liq
- Zoom anchoring terrain emas, ellipsoid asosida ishlaydi
- Gesture tuning real qurilmada tekshirilishi kerak
- Landscape safe-area mavjud
- Default launch orientation portrait
- Alohida atmospheric shell yo‘q

---

# 🔜 Keyingi bosqich

Tavsiya qilinadigan ishlar:

1. Android qurilmada real Google streaming'ni test qilish
2. iPhone'da test qilish
3. FPS profiling
4. Memory profiling
5. Touch gesture'larni yaxshilash
6. Terrain-aware camera clearance
7. Real height query integratsiyasi

Shundan keyin gameplay tizimlariga o‘tish mumkin.

---

# 🌐 Exploration va Localization

Globe sahnasida:

- Country details
- Uch tilli sozlamalar
- Compact `×` menu
- Localization tizimi

mavjud.

Qo‘shimcha hujjat:

```text
Documentation/DATA_LOCALIZATION.md
```

---

# 📁 File Inventory

Barcha qo‘shilgan va o‘zgartirilgan fayllar:

```text
Documentation/FILES.md
```

ichida ko‘rsatilgan.

---

---

# 🇬🇧 English

## About

**NATIONS** is an interactive 3D world exploration project built with Unity and Cesium.

The current **Phase 1** implementation focuses on the globe explorer.

It currently includes:

- Interactive 3D Earth
- Cesium for Unity
- Google Photorealistic 3D Tiles
- Mobile and desktop camera controls
- Pinch-to-zoom
- Orbit controls
- Tilt and heading
- Touch gesture system
- WGS84 navigation
- Android build support
- Universal Render Pipeline
- Country exploration interface
- Localization
- Three-language settings

The main scene is:

```text
Assets/_Nations/Scenes/Globe.unity
```

The existing:

```text
Assets/Scenes/WorldMapScene.unity
```

scene, scripts and prefabs are preserved.

---

# 🚀 Open and Run

## 1. Open the project

Open the `Nations` folder in Unity Hub using:

```text
Unity 6000.6.2f1
```

## 2. Package Manager

Allow Unity Package Manager to resolve all dependencies and finish importing.

The Cesium native package is a large download and may take some time during the first import.

## 3. Open the Globe scene

```text
Assets/_Nations/Scenes/Globe.unity
```

For a fresh checkout where the scene has not yet been generated:

```text
Nations → Phase 1 → Configure and Open Globe
```

## 4. Press Play

Without credentials, the application displays a dark-blue gridded WGS84 setup ellipsoid.

To stream real-world 3D data, configure Google Map Tiles API or Cesium ion.

---

# 📦 Required Packages

Pinned in:

```text
Packages/manifest.json
```

| Package | Version |
|---|---|
| Cesium for Unity | 1.26.0 |
| Universal Render Pipeline | 17.6.0 |
| Unity Input System | 1.20.0 |
| Unity Mathematics | 1.3.3 |

Cesium scoped registry:

```json
{
  "name": "Cesium",
  "url": "https://unity.pkg.cesium.com",
  "scopes": ["com.cesium.unity"]
}
```

---

# 🗺 Google Photorealistic 3D Tiles

## Google Cloud Setup

Open Google Cloud Console and create or select a project.

Enable:

```text
APIs & Services
→ Library
→ Map Tiles API
```

Then create an API key:

```text
APIs & Services
→ Credentials
→ Create credentials
→ API key
```

Restrict the key to:

```text
Map Tiles API
```

where appropriate.

---

# 🔑 Local API Configuration

Copy:

```text
Assets/_Nations/Config/MapConfig.example.json
```

to:

```text
Assets/_Nations/Config/Resources/NationsMapConfig.local.json
```

and configure:

```json
{
  "GOOGLE_MAP_TILES_API_KEY": "YOUR_API_KEY",
  "CESIUM_ION_ACCESS_TOKEN": ""
}
```

### Important

Never commit a real API key to:

- GitHub
- README files
- scenes
- scripts
- tracked configuration assets

The local configuration and its `.meta` file should remain ignored by Git.

---

# © Attribution

The project uses Cesium's official credit system.

```text
showCreditsOnScreen = true
```

While Google tiles are active, Google Maps attribution is also shown.

Do not:

- disable credits;
- cover them with UI;
- crop required attribution from screenshots.

---

# 🎮 Controls

## Mobile

| Gesture | Action |
|---|---|
| One-finger drag | Orbit the globe |
| Two-finger spread | Zoom in |
| Two-finger pinch | Zoom out |
| Two-finger twist | Rotate heading |
| Two fingers moving vertically | Tilt |

A short decaying inertia is applied after orbit gestures.

---

## Desktop

| Input | Action |
|---|---|
| Left mouse drag | Orbit |
| Mouse wheel / trackpad | Smooth zoom |
| Right mouse drag | Tilt / heading |
| Alt + Left Drag | Tilt / heading |
| Shift + Left Drag | Tilt / heading |

Touch input takes priority over mouse input while active.

---

# 📷 Camera System

The camera is controlled by:

```text
GlobeCameraController
```

`GlobeMath` provides:

- Double-precision WGS84 calculations
- Ellipsoid intersection
- Geodetic surface positions
- Stable axis-angle transport

The camera maintains an ECEF navigation state.

The Cesium origin is rebased when the camera moves more than approximately:

```text
1 km
```

from it.

---

## Initial Camera

Initial focus:

```text
38° E
30° N
```

covering approximately:

- Europe
- North Africa
- Western Asia

---

## Camera Limits

Pitch:

```text
0° = nadir
68° = maximum surface tilt
```

Minimum altitude:

```text
80 m
```

Maximum orbit distance:

```text
35,000 km
```

Zoom uses logarithmic distance for large movements in space and precise motion near the surface.

---

# 🎨 Rendering and Quality

Main URP asset:

```text
Assets/_Nations/Settings/NationsURP.asset
```

Current settings include:

- Forward URP
- 2x MSAA
- No HDR
- No post-processing
- No realtime shadows
- Sparse procedural star field
- Target 60 FPS

Real tiles retain their original imagery and materials.

No artificial night overlay is currently applied.

---

# ⚙️ Quality Profiles

Configured in:

```text
Assets/_Nations/Config/MapConfig.asset
```

| Profile | Screen-space error | Concurrent loads | Tile cache |
|---|---:|---:|---:|
| Low | 24 | 6 | 128 MiB |
| Medium | 16 | 10 | 256 MiB |
| High | 8 | 16 | 512 MiB |

Default profile:

```text
Medium
```

Target frame rate:

```text
60 FPS
```

Actual performance depends on the target device.

---

# 🤖 Android Build

Install through Unity Hub:

```text
Android Build Support
Android SDK
Android NDK
OpenJDK
```

Then use:

```text
Nations
→ Phase 1
→ Build Android APK
```

Output:

```text
Builds/Android/NationsGlobe.apk
```

---

## Android Configuration

```text
Scripting Backend: IL2CPP
Architecture: ARM64
Minimum Android API: 26
Internet Access: Require
Color Space: Linear
Rendering: URP
```

Sample identifier:

```text
com.nations.globe
```

For production releases, configure your own:

- Application ID
- Signing
- Version
- Target SDK
- AAB settings

---

# 🍎 iOS

Install:

```text
iOS Build Support
```

Switch to iOS in Build Profiles and export an Xcode project.

Configure:

- Team
- Signing
- Bundle ID

Minimum version:

```text
iOS 15
```

Architecture:

```text
ARM64
```

---

# ⏳ Loading and Failures

A small loading spinner is displayed while map data streams.

It fades when:

- view-load reaches approximately 95%;
- a renderable tile is available;
- setup mode is selected;
- a timeout occurs;
- a loading failure is reported.

Camera controls remain responsive while data is streaming.

---

# 🛠 Diagnostics

Development builds and Unity Editor display:

- FPS
- Ellipsoid altitude
- Latitude
- Longitude
- Pitch
- Heading
- View-load percentage

The diagnostic overlay is disabled in non-development builds.

---

# ✅ Verification

Run:

```text
Nations
→ Phase 1
→ Run Geometry and Configuration Checks
```

It verifies:

- WGS84 positions
- Ray intersections
- Pole transport
- Numerical stability
- Android configuration

---

# 🧪 Smoke Test

Run:

```text
Nations.Editor.GlobeSmokeTest.Run
```

The test exercises:

- Startup
- Drag
- Inertia
- Off-center zoom
- City-scale zoom
- Tilt
- Heading
- Zoom limits

Reports and screenshots are written to:

```text
Documentation/
```

---

# Batch Setup

```sh
"/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -projectPath "$PWD" \
  -executeMethod Nations.Editor.GlobeSetup.ConfigureBatch -quit \
  -logFile Logs/nations-setup.log
```

---

# ⚠️ Current Limitations

The current Phase 1 implementation has several limitations:

- No API key is included in the repository
- Real Google streaming requires local credentials
- Terrain avoidance depends on loaded colliders
- Zoom anchoring uses the ellipsoid rather than terrain
- Gesture thresholds require physical-device tuning
- Landscape safe-area support exists
- Default launch orientation is portrait
- No separate atmospheric shell is used

---

# 🔜 Recommended Next Steps

1. Test real streaming on an Android device
2. Test on an iPhone
3. Profile frame time
4. Profile memory usage
5. Refine touch controls
6. Improve terrain-aware camera clearance
7. Integrate real streamed height queries

Gameplay development should continue after the globe implementation has been properly validated on physical devices.

---

# 🌐 Exploration Interface and Localization

The Globe scene currently includes:

- Local country information
- Three-language settings
- Compact `×` menu
- Localization support

Additional documentation:

```text
Documentation/DATA_LOCALIZATION.md
```

---

# 📁 File Inventory

For a list of added and modified project files, see:

```text
Documentation/FILES.md
```

---

## Tech Stack

- Unity 6
- C#
- Cesium for Unity
- Google Map Tiles API
- Google Photorealistic 3D Tiles
- Universal Render Pipeline
- Unity Input System
- WGS84 / ECEF

---

## Project Status

🚧 **In Development — Phase 1**

The current focus is building a stable, performant and intuitive 3D globe exploration system before adding the full Nations gameplay layer.
