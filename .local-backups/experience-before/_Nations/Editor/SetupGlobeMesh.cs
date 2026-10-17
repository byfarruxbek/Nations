using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nations.Editor
{
    public static class SetupGlobeMesh
    {
        public static void Refresh()
        {
            EditorSceneManager.OpenScene(GlobeSetup.ScenePath);
            const string path = "Assets/_Nations/Materials/SetupEllipsoidMesh.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!mesh)
            {
                const int longitudeSteps = 128, latitudeSteps = 64;
                var vertices = new Vector3[(longitudeSteps + 1) * (latitudeSteps + 1)];
                var normals = new Vector3[vertices.Length]; var uv = new Vector2[vertices.Length];
                var triangles = new int[longitudeSteps * latitudeSteps * 6];
                int index = 0;
                for (int y = 0; y <= latitudeSteps; y++)
                for (int x = 0; x <= longitudeSteps; x++)
                {
                    float latitude = Mathf.PI * (y / (float)latitudeSteps - 0.5f), longitude = 2 * Mathf.PI * x / longitudeSteps;
                    int i = y * (longitudeSteps + 1) + x;
                    normals[i] = new Vector3(Mathf.Cos(latitude) * Mathf.Sin(longitude), Mathf.Sin(latitude), Mathf.Cos(latitude) * Mathf.Cos(longitude));
                    vertices[i] = normals[i] * 0.5f; uv[i] = new Vector2(x / (float)longitudeSteps, y / (float)latitudeSteps);
                    if (x == longitudeSteps || y == latitudeSteps) continue;
                    int next = i + longitudeSteps + 1;
                    triangles[index++] = i; triangles[index++] = i + 1; triangles[index++] = next;
                    triangles[index++] = i + 1; triangles[index++] = next + 1; triangles[index++] = next;
                }
                mesh = new Mesh { name = "WGS84 setup sphere mesh", vertices = vertices, normals = normals, uv = uv, triangles = triangles };
                mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path);
            }
            var globe = Object.FindAnyObjectByType<Nations.Globe.SetupEllipsoid>();
            globe.GetComponent<MeshFilter>().sharedMesh = mesh;
            globe.Refresh();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }
    }
}
