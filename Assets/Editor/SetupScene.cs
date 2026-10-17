#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetupScene
{
    [MenuItem("Nations/Build World Map Scene")]
    public static void CreateSceneAndPrefabs()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Color Palette
        Color bgDark = HexToColor("030712");          // Deep space navy
        Color cyanAccent = HexToColor("38BDF8");      // Glowing cyan
        Color blueAccent = HexToColor("0284C7");      // Sky blue
        Color glassBg = new Color(0.043f, 0.082f, 0.157f, 0.92f); // Semi-transparent glass
        Color glassBorder = new Color(0.22f, 0.74f, 0.97f, 0.45f); // Neon cyan border
        Color textPrimary = Color.white;
        Color textSecondary = HexToColor("94A3B8");

        // High-res Procedural Textures (0% Checkerboards, AAA Vector Visuals)
        Sprite markerSprite = GenerateMarkerSprite();
        Sprite mapSprite = GenerateMapSprite();
        Sprite glassSprite = GenerateGlassPanelSprite(512, 256, glassBg, glassBorder);
        Sprite buttonSprite = GenerateButtonSprite(512, 128, HexToColor("0284C7"), HexToColor("06B6D4"));

        // --- MAIN CAMERA ---
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.5f;
        cam.backgroundColor = bgDark;
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.transform.position = new Vector3(0f, 0f, -10f);
        camObj.AddComponent<AudioListener>();
        camObj.tag = "MainCamera";

        camObj.AddComponent<Physics2DRaycaster>();

        // --- EVENT SYSTEM ---
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();

        // --- BACKGROUND ---
        GameObject bgParent = new GameObject("Background");
        
        GameObject bgGrad = new GameObject("GradientBackground");
        bgGrad.transform.SetParent(bgParent.transform);
        SpriteRenderer bgSr = bgGrad.AddComponent<SpriteRenderer>();
        bgSr.color = bgDark;

        // --- WORLD MAP ---
        GameObject worldParent = new GameObject("World");
        
        GameObject worldMapObj = new GameObject("WorldMap");
        worldMapObj.transform.SetParent(worldParent.transform);
        SpriteRenderer mapSr = worldMapObj.AddComponent<SpriteRenderer>();
        mapSr.sprite = mapSprite;
        worldMapObj.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
        worldMapObj.transform.position = new Vector3(0f, 0.5f, 0f);

        // --- CANVAS (UI) ---
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2400);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // --- HEADER ---
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform headerRt = headerObj.AddComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1);
        headerRt.anchoredPosition = new Vector2(0, -120);
        headerRt.sizeDelta = new Vector2(0, 240);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(headerObj.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.45f);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.anchoredPosition = Vector2.zero;
        Text titleTxt = titleObj.AddComponent<Text>();
        titleTxt.text = "NATIONS";
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.fontSize = 78;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = textPrimary;

        GameObject subObj = new GameObject("Subtitle");
        subObj.transform.SetParent(headerObj.transform, false);
        RectTransform subRt = subObj.AddComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0, 0);
        subRt.anchorMax = new Vector2(1, 0.45f);
        subRt.anchoredPosition = Vector2.zero;
        Text subTxt = subObj.AddComponent<Text>();
        subTxt.text = "EXPLORE  •  LEARN  •  DISCOVER";
        subTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subTxt.fontSize = 32;
        subTxt.alignment = TextAnchor.MiddleCenter;
        subTxt.color = cyanAccent;

        // --- COUNTRY INFO CARD ---
        GameObject cardObj = new GameObject("CountryInfoCard");
        cardObj.transform.SetParent(canvasObj.transform, false);
        RectTransform cardRt = cardObj.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0f);
        cardRt.anchorMax = new Vector2(0.5f, 0f);
        cardRt.pivot = new Vector2(0.5f, 0f);
        cardRt.anchoredPosition = new Vector2(0, 360);
        cardRt.sizeDelta = new Vector2(920, 540);

        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.sprite = glassSprite;
        cardBg.type = Image.Type.Sliced;
        cardBg.color = Color.white;
        
        CanvasGroup cardCg = cardObj.AddComponent<CanvasGroup>();
        CountryInfoCard infoCardScript = cardObj.AddComponent<CountryInfoCard>();
        infoCardScript.canvasGroup = cardCg;
        infoCardScript.cardTransform = cardRt;

        // Card Header Text: 🇺🇿  O‘zbekiston
        GameObject cardHeaderObj = new GameObject("CardHeader");
        cardHeaderObj.transform.SetParent(cardObj.transform, false);
        RectTransform cardHeaderRt = cardHeaderObj.AddComponent<RectTransform>();
        cardHeaderRt.anchorMin = new Vector2(0.06f, 0.72f);
        cardHeaderRt.anchorMax = new Vector2(0.94f, 0.95f);
        Text cardHeaderTxt = cardHeaderObj.AddComponent<Text>();
        cardHeaderTxt.text = "🇺🇿  O‘zbekiston";
        cardHeaderTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        cardHeaderTxt.fontSize = 54;
        cardHeaderTxt.fontStyle = FontStyle.Bold;
        cardHeaderTxt.alignment = TextAnchor.MiddleLeft;
        cardHeaderTxt.color = textPrimary;
        infoCardScript.countryHeaderText = cardHeaderTxt;

        // Capital Text
        GameObject capObj = new GameObject("CapitalText");
        capObj.transform.SetParent(cardObj.transform, false);
        RectTransform capRt = capObj.AddComponent<RectTransform>();
        capRt.anchorMin = new Vector2(0.06f, 0.52f);
        capRt.anchorMax = new Vector2(0.94f, 0.72f);
        Text capTxt = capObj.AddComponent<Text>();
        capTxt.text = "Poytaxt:  Toshkent";
        capTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        capTxt.fontSize = 38;
        capTxt.alignment = TextAnchor.MiddleLeft;
        capTxt.color = textSecondary;
        infoCardScript.capitalText = capTxt;

        // Continent Text
        GameObject contObj = new GameObject("ContinentText");
        contObj.transform.SetParent(cardObj.transform, false);
        RectTransform contRt = contObj.AddComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0.06f, 0.35f);
        contRt.anchorMax = new Vector2(0.94f, 0.52f);
        Text contTxt = contObj.AddComponent<Text>();
        contTxt.text = "Qit'a:  Osiyo";
        contTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        contTxt.fontSize = 38;
        contTxt.alignment = TextAnchor.MiddleLeft;
        contTxt.color = textSecondary;
        infoCardScript.continentText = contTxt;

        // Button: Kashf etish
        GameObject buttonObj = new GameObject("ExploreButton");
        buttonObj.transform.SetParent(cardObj.transform, false);
        RectTransform btnRt = buttonObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.06f, 0.08f);
        btnRt.anchorMax = new Vector2(0.94f, 0.30f);
        Image btnImg = buttonObj.AddComponent<Image>();
        btnImg.sprite = buttonSprite;
        btnImg.type = Image.Type.Sliced;
        btnImg.color = Color.white;
        btnImg.raycastTarget = true;
        Button btn = buttonObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.98f, 1f);
        colors.pressedColor = new Color(0.7f, 0.9f, 1f);
        btn.colors = colors;

        GameObject btnTxtObj = new GameObject("ButtonText");
        btnTxtObj.transform.SetParent(buttonObj.transform, false);
        RectTransform btnTxtRt = btnTxtObj.AddComponent<RectTransform>();
        btnTxtRt.anchorMin = Vector2.zero;
        btnTxtRt.anchorMax = Vector2.one;
        Text btnTxt = btnTxtObj.AddComponent<Text>();
        btnTxt.text = "Kashf etish";
        btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnTxt.fontSize = 42;
        btnTxt.fontStyle = FontStyle.Bold;
        btnTxt.alignment = TextAnchor.MiddleCenter;
        btnTxt.color = Color.white;
        btnTxt.raycastTarget = false;

        infoCardScript.exploreButton = btn;

        // --- OBJECT 1: CountryMarker (Uzbekistan) ---
        GameObject objectsParent = new GameObject("Objects");
        GameObject markerObj = new GameObject("CountryMarker");
        markerObj.transform.SetParent(objectsParent.transform);
        markerObj.transform.position = new Vector3(0.45f, 0.80f, 0f);
        markerObj.transform.localScale = new Vector3(0.60f, 0.60f, 1f);

        SpriteRenderer markerSr = markerObj.AddComponent<SpriteRenderer>();
        markerSr.sprite = markerSprite;
        markerSr.color = Color.white;

        CircleCollider2D col = markerObj.AddComponent<CircleCollider2D>();
        col.radius = 1.4f;

        CountryMarker markerScript = markerObj.AddComponent<CountryMarker>();
        markerScript.infoCard = infoCardScript;

        // --- BOTTOM PANEL ---
        GameObject bottomPanelObj = new GameObject("BottomPanel");
        bottomPanelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bottomRt = bottomPanelObj.AddComponent<RectTransform>();
        bottomRt.anchorMin = new Vector2(0, 0);
        bottomRt.anchorMax = new Vector2(1, 0);
        bottomRt.pivot = new Vector2(0.5f, 0);
        bottomRt.anchoredPosition = new Vector2(0, 40);
        bottomRt.sizeDelta = new Vector2(-80, 180);

        Image bottomImg = bottomPanelObj.AddComponent<Image>();
        bottomImg.sprite = glassSprite;
        bottomImg.type = Image.Type.Sliced;
        bottomImg.color = Color.white;

        HorizontalLayoutGroup hlg = bottomPanelObj.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.padding = new RectOffset(20, 20, 15, 15);
        hlg.spacing = 20;

        CreateNavButton(bottomPanelObj, "World", "World", cyanAccent, true);
        CreateNavButton(bottomPanelObj, "Explore", "Explore", textSecondary, false);
        CreateNavButton(bottomPanelObj, "Profile", "Profile", textSecondary, false);

        // Save Prefabs & Scene
        Directory.CreateDirectory("Assets/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(markerObj, "Assets/Prefabs/CountryMarker.prefab");
        PrefabUtility.SaveAsPrefabAsset(cardObj, "Assets/Prefabs/CountryInfoCard.prefab");

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/WorldMapScene.unity");

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/WorldMapScene.unity", true)
        };

        Debug.Log("High-end AAA WorldMapScene built successfully!");
    }

    private static Sprite GenerateMarkerSprite()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color cyan = HexToColor("38BDF8");
        Color blue = HexToColor("0284C7");
        Color transparent = new Color(0, 0, 0, 0);

        float center = size / 2f;
        float outerR = size * 0.45f;
        float midR = size * 0.28f;
        float innerR = size * 0.14f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= innerR)
                {
                    tex.SetPixel(x, y, Color.white);
                }
                else if (dist >= midR - 6 && dist <= midR + 6)
                {
                    tex.SetPixel(x, y, cyan);
                }
                else if (dist >= outerR - 8 && dist <= outerR + 8)
                {
                    float a = Mathf.Clamp01(1f - Mathf.Abs(dist - outerR) / 8f);
                    tex.SetPixel(x, y, new Color(cyan.r, cyan.g, cyan.b, a * 0.85f));
                }
                else
                {
                    tex.SetPixel(x, y, transparent);
                }
            }
        }
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        Directory.CreateDirectory("Assets/Sprites");
        File.WriteAllBytes("Assets/Sprites/country_marker.png", png);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/country_marker.png");
    }

    private static Sprite GenerateMapSprite()
    {
        int w = 2048;
        int h = 1024;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color spaceNavy = HexToColor("030712");
        Color landNavy = HexToColor("0B192C");
        Color cyanGlow = HexToColor("38BDF8");
        Color gridLine = new Color(0.22f, 0.74f, 0.97f, 0.08f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x % 96 == 0 || y % 96 == 0)
                {
                    tex.SetPixel(x, y, gridLine);
                    continue;
                }

                float nx = (float)x / w * 2f - 1f;
                float ny = (float)y / h * 2f - 1f;

                bool isLand = (Mathf.Sin(nx * 4f) * Mathf.Cos(ny * 3f) > 0.12f) ||
                              (Mathf.Abs(nx - 0.25f) < 0.4f && Mathf.Abs(ny - 0.1f) < 0.45f) ||
                              (Mathf.Abs(nx + 0.5f) < 0.3f && Mathf.Abs(ny + 0.3f) < 0.4f);

                if (isLand)
                {
                    // Glowing cyan coastline
                    bool isCoast = (x % 4 == 0 || y % 4 == 0);
                    tex.SetPixel(x, y, isCoast ? cyanGlow : landNavy);
                }
                else
                {
                    tex.SetPixel(x, y, spaceNavy);
                }
            }
        }
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes("Assets/Sprites/world_map.png", png);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/world_map.png");
    }

    private static Sprite GenerateGlassPanelSprite(int w, int h, Color bg, Color border)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        int bw = 4;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x < bw || x >= w - bw || y < bw || y >= h - bw)
                {
                    tex.SetPixel(x, y, border);
                }
                else
                {
                    tex.SetPixel(x, y, bg);
                }
            }
        }
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes("Assets/Sprites/glass_panel.png", png);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/glass_panel.png");
    }

    private static Sprite GenerateButtonSprite(int w, int h, Color c1, Color c2)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        int bw = 3;
        Color borderColor = HexToColor("38BDF8");

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            Color lineCol = Color.Lerp(c1, c2, t);

            for (int x = 0; x < w; x++)
            {
                if (x < bw || x >= w - bw || y < bw || y >= h - bw)
                {
                    tex.SetPixel(x, y, borderColor);
                }
                else
                {
                    tex.SetPixel(x, y, lineCol);
                }
            }
        }
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes("Assets/Sprites/button_bg.png", png);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/button_bg.png");
    }

    [MenuItem("Nations/Build macOS App")]
    public static void BuildMacApp()
    {
        CreateSceneAndPrefabs();

        Directory.CreateDirectory("Builds/Mac");
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds/Mac/Nations.app");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/WorldMapScene.unity" },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Mac Build Result: " + report.summary.result);
    }

    [MenuItem("Nations/Build Android APK")]
    public static void BuildAndroidAPK()
    {
        CreateSceneAndPrefabs();

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        Directory.CreateDirectory("Builds/Android");
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds/Android/Nations.apk");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/WorldMapScene.unity" },
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("APK Build Result: " + report.summary.result);
    }

    private static void CreateNavButton(GameObject parent, string name, string label, Color textColor, bool isSelected)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = isSelected ? new Color(0.22f, 0.74f, 0.97f, 0.25f) : Color.clear;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRt = txtObj.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        Text txt = txtObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 36;
        txt.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = textColor;
        txt.raycastTarget = false;

        NavButtonHandler nav = btnObj.AddComponent<NavButtonHandler>();
        nav.buttonText = txt;
        nav.backgroundImage = img;
        nav.SetState(isSelected);
    }

    private static Color HexToColor(string hex)
    {
        Color col;
        if (ColorUtility.TryParseHtmlString("#" + hex, out col))
            return col;
        return Color.white;
    }
}
#endif
