using UnityEngine;
using UnityEngine.UI;

namespace Nations.UI
{
    public sealed class LoadingSpinner : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            const int segments = 32;
            float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments, angle = t * Mathf.PI * 1.6f;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Color c = color; c.a *= t;
                helper.AddVert(direction * radius, c, Vector2.zero);
                helper.AddVert(direction * (radius - 2), c, Vector2.zero);
                if (i == 0) continue;
                int n = i * 2;
                helper.AddTriangle(n - 2, n, n - 1); helper.AddTriangle(n - 1, n, n + 1);
            }
        }
    }
}
