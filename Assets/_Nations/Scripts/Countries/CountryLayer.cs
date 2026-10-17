using System;
using Nations.Localization;
using Nations.Globe;
using Nations.Navigation;
using Nations.Input;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Nations.Countries
{
    [Serializable] public class CountryData { public Country[] countries; }
    [Serializable] public class Country { public string id, name; public int rank; public double lon, lat; public Polygon[] polygons; }
    [Serializable] public class Polygon { public Ring[] rings; }
    [Serializable] public class Ring { public double[] points; [NonSerialized] public double3[] ecef; }

    [RequireComponent(typeof(CanvasRenderer))]
    [DefaultExecutionOrder(200)]
    public sealed class CountryLayer : MaskableGraphic
    {
        public GlobeCameraController Controller;
        public GlobeInputManager Input;
        public bool Borders = true, Labels = true;
        public CountryData Data { get; private set; }
        public int Selected { get; private set; } = -1;
        public event Action<string> SelectionChanged;
        readonly Text[] labels = new Text[16];
        readonly Rect[] occupied = new Rect[16];
        float nextLabels;
        double4x4 ecefToLocal;
        Matrix4x4 localToWorld;
        readonly UIVertex[] quad = new UIVertex[4];
        protected override void Start()
        {
            base.Start(); raycastTarget = false;
            var source = Resources.Load<TextAsset>("NationsCountries");
            if (!source) { Debug.LogWarning("NATIONS: Country data missing."); return; }
            Data = JsonUtility.FromJson<CountryData>(source.text);
            Borders = PlayerPrefs.GetInt("Nations.Borders", 1) != 0;
            Labels = PlayerPrefs.GetInt("Nations.Labels", 1) != 0;
            foreach (var c in Data.countries)
                foreach (var p in c.polygons)
                    foreach (var ring in p.rings)
                    {
                        ring.ecef = new double3[ring.points.Length / 2];
                        for (int i = 0; i < ring.ecef.Length; i++)
                            ring.ecef[i] = GlobeMath.Surface(GlobeMath.Normal(ring.points[i * 2], ring.points[i * 2 + 1]));
                    }
            for (int i = 0; i < labels.Length; i++)
            {
                var go = new GameObject("Country label", typeof(RectTransform), typeof(Text), typeof(Shadow)); go.transform.SetParent(transform, false);
                var label = go.GetComponent<Text>(); label.font = Locale.Font; label.fontSize = 12;
                label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false; label.color = new Color(0.9f, 0.95f, 1, 0.9f);
                go.GetComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.9f); labels[i] = label;
            }
            Input.Tapped += Select;
            Locale.Changed += LanguageChanged;
        }
        void LanguageChanged() { nextLabels = 0; }
        public void SetBorders(bool value) { Borders = value; PlayerPrefs.SetInt("Nations.Borders", value ? 1 : 0); PlayerPrefs.Save(); SetVerticesDirty(); }
        public void SetLabels(bool value) { Labels = value; PlayerPrefs.SetInt("Nations.Labels", value ? 1 : 0); PlayerPrefs.Save(); nextLabels = 0; }
        void LateUpdate()
        {
            if (Data == null) return;
            ecefToLocal = Controller.Georeference.ecefToLocalMatrix;
            localToWorld = Controller.Georeference.transform.localToWorldMatrix;
            SetVerticesDirty(); // Projection must follow the moving/rebased camera without lag.
            if (Time.unscaledTime < nextLabels) return;
            nextLabels = Time.unscaledTime + 0.08f;
            int used = 0;
            if (Labels && Controller.Altitude > 80000)
                for (int pass = -1; pass < Data.countries.Length && used < labels.Length; pass++)
                {
                    int index = pass < 0 ? Selected : pass;
                    if (index < 0 || pass >= 0 && index == Selected) continue;
                    Country c = Data.countries[index];
                    if (Controller.Altitude > 8000000 && c.rank > 3 && index != Selected) continue;
                    if (!Project(GlobeMath.Surface(GlobeMath.Normal(c.lon, c.lat)), out Vector2 pt)) continue;
                    var label = labels[used]; label.text = CountryCatalog.Name(c);
                    float width = Mathf.Min(180, label.preferredWidth + 14);
                    Rect box = new Rect(pt.x - width / 2, pt.y - 13, width, 26);
                    var screen = RectTransformUtility.WorldToScreenPoint(null, rectTransform.TransformPoint(pt));
                    Rect safe = Nations.UI.GlobeSafeArea.Current; safe.yMin += 120 * canvas.scaleFactor; safe.yMax -= 85 * canvas.scaleFactor;
                    if (!safe.Contains(screen - new Vector2(width * canvas.scaleFactor / 2,13 * canvas.scaleFactor)) || !safe.Contains(screen + new Vector2(width * canvas.scaleFactor / 2,13 * canvas.scaleFactor))) continue;
                    bool overlap = false;
                    for (int j = 0; j < used; j++) if (occupied[j].Overlaps(box)) { overlap = true; break; }
                    if (overlap) continue;
                    occupied[used] = box; label.rectTransform.sizeDelta = box.size; label.rectTransform.anchoredPosition = pt;
                    label.color = index == Selected ? new Color(0.48f, 1, 0.83f) : new Color(0.9f, 0.95f, 1, 0.9f);
                    label.gameObject.SetActive(true); used++;
                }
            for (int i = used; i < labels.Length; i++) labels[i].gameObject.SetActive(false);
        }
        bool Project(double3 point, out Vector2 local)
        {
            local = default;
            if (math.dot(GlobeMath.SurfaceNormal(point), Controller.Eye - point) <= 0) return false;
            Vector3 world = localToWorld.MultiplyPoint3x4((Vector3)(float3)math.transform(ecefToLocal, point));
            Vector3 screen = Controller.View.WorldToScreenPoint(world);
            if (screen.z <= 0) return false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screen, null, out local); return true;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if (Data == null || !Controller || Controller.Altitude < 80000) return;
            float alpha = Mathf.InverseLerp(80000, 300000, (float)Controller.Altitude);
            for (int ci = 0; ci < Data.countries.Length; ci++)
            {
                bool selected = ci == Selected;
                if (!Borders && !selected) continue;
                Color tint = selected ? new Color(0.45f, 1, 0.8f, alpha) : new Color(0.85f, 0.94f, 1, alpha * 0.42f);
                foreach (var polygon in Data.countries[ci].polygons)
                    foreach (var ring in polygon.rings)
                    {
                        if (ring.ecef == null) continue;
                        int stride = Controller.Altitude > 12000000 && !selected ? 2 : 1;
                        for (int i = 0; i < ring.ecef.Length - 1; i += stride)
                        {
                            int j = Mathf.Min(i + stride, ring.ecef.Length - 1);
                            if (!Project(ring.ecef[i], out Vector2 a) || !Project(ring.ecef[j], out Vector2 b)) continue;
                            if (!rectTransform.rect.Overlaps(Rect.MinMaxRect(Mathf.Min(a.x,b.x)-2,Mathf.Min(a.y,b.y)-2,Mathf.Max(a.x,b.x)+2,Mathf.Max(a.y,b.y)+2))) continue;
                            Vector2 normal = new Vector2(-(b-a).y, (b-a).x).normalized * (selected ? 1.1f : 0.45f);
                            AddVertex(0, a-normal, tint); AddVertex(1, a+normal, tint); AddVertex(2, b+normal, tint); AddVertex(3, b-normal, tint); vh.AddUIVertexQuad(quad);
                        }
                    }
            }
        }
        void AddVertex(int i, Vector2 point, Color tint) { quad[i] = UIVertex.simpleVert; quad[i].position = point; quad[i].color = tint; }
        public int FindCountry(double longitude, double latitude)
        {
            if (Data == null) return -1;
            for (int i = 0; i < Data.countries.Length; i++)
                foreach (var polygon in Data.countries[i].polygons)
                {
                    if (!Inside(polygon.rings[0].points, longitude, latitude)) continue;
                    bool hole = false;
                    for (int r = 1; r < polygon.rings.Length; r++) if (Inside(polygon.rings[r].points, longitude, latitude)) { hole = true; break; }
                    if (!hole) return i;
                }
            return -1;
        }
        static bool Inside(double[] ring, double lon, double lat)
        {
            bool inside = false;
            double previousX = ring[0], previousY = ring[1];
            lon = previousX + Wrap(lon - previousX);
            for (int i = 2; i < ring.Length; i += 2)
            {
                double x = previousX + Wrap(ring[i] - previousX), y = ring[i+1];
                if ((y > lat) != (previousY > lat) && lon < (previousX-x)*(lat-y)/(previousY-y)+x) inside = !inside;
                previousX = x; previousY = y;
            }
            return inside;
        }
        static double Wrap(double x) { while (x > 180) x -= 360; while (x < -180) x += 360; return x; }
        public bool SelectCountry(string id)
        {
            if(Data==null)return false;
            for(int i=0;i<Data.countries.Length;i++)
                if(Data.countries[i].id==id)
                {
                    Selected=i;SelectionChanged?.Invoke(Data.countries[i].name);nextLabels=0;return true;
                }
            return false;
        }
        void Select(Vector2 screen)
        {
            if (!Controller.TryPick(screen, out double3 point)) return;
            var normal = GlobeMath.SurfaceNormal(point);
            Selected = FindCountry(GlobeMath.Longitude(normal), GlobeMath.Latitude(normal));
            SelectionChanged?.Invoke(Selected < 0 ? "" : Data.countries[Selected].name); nextLabels = 0;
        }
        protected override void OnDestroy() { if (Input) Input.Tapped -= Select; Locale.Changed -= LanguageChanged; base.OnDestroy(); }
    }
}
