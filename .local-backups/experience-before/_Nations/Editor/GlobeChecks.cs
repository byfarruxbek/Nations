using System;
using System.IO;
using Nations.Globe;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Nations.Editor
{
    public static class GlobeChecks
    {
        static void Require(bool valid, string message) { if (!valid) throw new Exception("NATIONS check failed: " + message); }
        [MenuItem("Nations/Phase 1/Run Geometry and Configuration Checks")]
        public static void Run()
        {
            var n = GlobeMath.Normal(64.6, 41.3);
            var surface = GlobeMath.Surface(n);
            Require(math.abs(math.length(surface / GlobeMath.Radii) - 1) < 1e-12, "WGS84 surface");
            Require(GlobeMath.Intersect(surface + n * 20000000, -n, out var hit), "space ray hit");
            Require(math.distance(surface, hit) < 0.001, "space ray precision");
            Require(GlobeMath.Intersect(surface + n * 80, -n, out hit) && math.distance(surface, hit) < 0.001, "city ray precision");
            Require(!GlobeMath.Intersect(surface + n * 80, n, out hit), "outward ray misses");
            Require(!GlobeMath.Intersect(new double3(0, 0, 20000000), new double3(1, 0, 0), out hit), "space miss");
            var p = GlobeMath.Normal(0, 89.9999);
            var q = GlobeMath.Normal(180, 89.9999);
            GlobeMath.Between(p, q, out var axis, out var angle);
            Require(math.distance(GlobeMath.Rotate(p, axis, angle), q) < 1e-10, "pole transport");
            for (int i = 0; i < 10000; i++) p = GlobeMath.Rotate(p, new double3(1, 0, 0), 0.001);
            Require(math.abs(math.length(p) - 1) < 1e-9, "long orbit stability");
            Require(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP, "Android IL2CPP");
            Require(PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64, "Android ARM64 only");
            Require(PlayerSettings.Android.forceInternetPermission, "Internet permission");
            Require(EditorBuildSettings.scenes[0].path == GlobeSetup.ScenePath, "main scene");
            Directory.CreateDirectory("Documentation");
            File.WriteAllText("Documentation/geometry-checks.txt", "PASS: WGS84 surface, space/city ray precision, ray misses, pole transport, 10000-step orbit stability, Android IL2CPP/ARM64/internet, main scene.\n");
            Debug.Log("NATIONS: Geometry and configuration checks PASS.");
        }
    }
}
