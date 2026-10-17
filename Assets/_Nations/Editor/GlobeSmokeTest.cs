using System;
using System.IO;
using Nations.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Nations.Editor
{
    [InitializeOnLoad]
    public static class GlobeSmokeTest
    {
        const string Running = "Nations.Smoke.Running";
        static double start, longitude, releaseLongitude, initialAltitude;
        static int stage;
        static GlobeCameraController camera;
        static bool errors;
        static Mouse testMouse;
        static InputSettings previousInputSettings;
        static Unity.Mathematics.double3 zoomPoint;
        static GlobeSmokeTest() { EditorApplication.update += Tick; Application.logMessageReceived += Log; }
        [MenuItem("Nations/Phase 1/Run Play Mode Smoke Test %#t")]
        public static void Run()
        {
            stage = 0; camera = null; errors = false;
            GlobeChecks.Run();
            EditorSceneManager.OpenScene(GlobeSetup.ScenePath);
            SessionState.SetBool(Running, true);
            SessionState.SetBool("Nations.Smoke.Failed", false);
            EditorApplication.isPlaying = true;
        }
        static void Log(string message, string trace, LogType type)
        {
            if (EditorApplication.isPlaying && SessionState.GetBool(Running, false) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
            { errors = true; SessionState.SetBool("Nations.Smoke.Failed", true); }
        }
        static void Check(bool result, string message) { if (!result) throw new Exception(message); }
        static void Tick()
        {
            if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            try
            {
                if (!camera)
                {
                    camera = UnityEngine.Object.FindAnyObjectByType<GlobeCameraController>();
                    if (!camera) return;
                    start = EditorApplication.timeSinceStartup;
                    return;
                }
                double t = EditorApplication.timeSinceStartup - start;
                Vector2 center = new Vector2(camera.View.pixelWidth * 0.5f, camera.View.pixelHeight * 0.5f);
                Check(double.IsFinite(camera.Altitude) && camera.Altitude >= camera.MinAltitude - 1, "Camera altitude invalid");
                if (stage == 0 && t > 2)
                {
                    Check(camera.TryPick(center, out _), "Earth not under center ray");
                    initialAltitude = camera.Altitude; longitude = camera.Longitude;
                    Capture("Documentation/globe-space.png");
                    camera.BeginDrag(); stage = 1; start = EditorApplication.timeSinceStartup;
                }
                else if (stage == 1 && t < 1)
                    camera.Drag(center, center + new Vector2(5, 0), 1f / 60);
                else if (stage == 1)
                {
                    Check(Math.Abs(camera.Longitude - longitude) > 0.1, "Drag did not orbit");
                    releaseLongitude = camera.Longitude; camera.EndDrag(); stage = 2; start = EditorApplication.timeSinceStartup;
                }
                else if (stage == 2 && t > 1)
                {
                    Check(Math.Abs(camera.Longitude - releaseLongitude) > 0.001, "No inertia");
                    camera.StopMotion();
                    Check(camera.TryPick(center + new Vector2(camera.View.pixelWidth * 0.1f, 0), out zoomPoint), "Zoom anchor missed");
                    stage = 3; start = EditorApplication.timeSinceStartup;
                }
                else if (stage == 3 && t < 5)
                    camera.Zoom(-0.15f, center + new Vector2(camera.View.pixelWidth * 0.1f, 0));
                else if (stage == 3)
                {
                    Check(camera.Altitude < 5000 && camera.Altitude < initialAltitude, "City zoom failed");
                    Check(camera.TryPick(center + new Vector2(camera.View.pixelWidth * 0.1f, 0), out var zoomEnd), "Zoom anchor lost");
                    double anchorError = Unity.Mathematics.math.distance(zoomPoint, zoomEnd);
                    Debug.Log("NATIONS: Zoom-anchor error (metres): " + anchorError);
                    Check(anchorError < 1, "Zoom anchor drift exceeds 1 m over world-to-city zoom");
                    camera.TiltRotate(260, 35); stage = 4; start = EditorApplication.timeSinceStartup;
                }
                else if (stage == 4 && t > 2)
                {
                    Check(camera.Pitch > 20 && Math.Abs(camera.Heading) > 20, "Tilt/heading failed");
                    Capture("Documentation/globe-close.png"); stage = 5; start = EditorApplication.timeSinceStartup;
                }
                else if (stage == 5 && t < 5) camera.Zoom(0.25f, center);
                else if (stage == 5 && t > 6)
                {
                    Check(camera.Altitude <= camera.MaxAltitude * 1.01, "Maximum altitude exceeded");
                    Check(!errors && !SessionState.GetBool("Nations.Smoke.Failed", false), "Runtime console errors");
                    previousInputSettings = InputSystem.settings;
                    InputSystem.settings = UnityEngine.Object.Instantiate(previousInputSettings);
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    testMouse = InputSystem.AddDevice<Mouse>();
                    longitude = camera.Longitude;
                    InputSystem.QueueStateEvent(testMouse, new MouseState { position = center, buttons = 1 });
                    stage = 6; start = EditorApplication.timeSinceStartup;
                }
                else if (stage == 6 && t < 1.5)
                    InputSystem.QueueStateEvent(testMouse, new MouseState { position = center + new Vector2((float)t * 100, 0), buttons = 1 });
                else if (stage == 6)
                {
                    InputSystem.QueueStateEvent(testMouse, new MouseState { position = center + new Vector2(150, 0) });
                    stage = 7; start = EditorApplication.timeSinceStartup;
                }
                else if (stage == 7 && t > 0.5)
                {
                    Debug.Log($"NATIONS: Mouse drag longitude delta {camera.Longitude - longitude}, current device {Mouse.current?.deviceId}, test device {testMouse.deviceId}, focus {Application.isFocused}");
                    Check(Math.Abs(camera.Longitude - longitude) > 0.1, "Input System mouse drag failed");
                    initialAltitude = camera.Altitude;
                    InputSystem.QueueStateEvent(testMouse, new MouseState { position = center, scroll = new Vector2(0, 120) });
                    stage = 8; start = EditorApplication.timeSinceStartup;
                }
                else if (stage == 8 && t > 1)
                {
                    Check(camera.Altitude < initialAltitude * 0.95, "Input System scroll zoom failed");
                    Check(!errors && !SessionState.GetBool("Nations.Smoke.Failed", false), "Runtime console errors");
                    Finish(true, "PASS: Play Mode startup, center pick, drag, inertia, city zoom, geographic anchor (<1 m drift), tilt/heading, altitude limits, Input System virtual mouse drag/scroll, no runtime errors. Physical touch and streamed map data not tested.");
                }
                if (t > 35) Finish(false, "Smoke test timed out.");
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }
        static void Capture(string path)
        {
            int width = 960, height = Mathf.RoundToInt(960 / camera.View.aspect);
            var target = new RenderTexture(width, height, 24);
            var previous = camera.View.targetTexture; var active = RenderTexture.active;
            camera.View.targetTexture = target; camera.View.Render(); RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            camera.View.targetTexture = previous; RenderTexture.active = active;
            UnityEngine.Object.DestroyImmediate(image); target.Release(); UnityEngine.Object.DestroyImmediate(target);
        }
        static void Finish(bool success, string message)
        {
            SessionState.SetBool(Running, false);
            if (testMouse != null) InputSystem.RemoveDevice(testMouse);
            if (previousInputSettings)
            {
                var temporary = InputSystem.settings;
                InputSystem.settings = previousInputSettings;
                UnityEngine.Object.DestroyImmediate(temporary);
            }
            File.WriteAllText("Documentation/playmode-checks.txt", message + "\n");
            Debug.Log("NATIONS smoke: " + message);
            EditorApplication.isPlaying = false;
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
