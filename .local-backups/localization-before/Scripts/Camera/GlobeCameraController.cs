using CesiumForUnity;
using Nations.Globe;
using Unity.Mathematics;
using UnityEngine;

namespace Nations.Navigation
{
    [DefaultExecutionOrder(100)]
    public sealed class GlobeCameraController : MonoBehaviour
    {
        public CesiumGeoreference Georeference;
        public UnityEngine.Camera View;
        [Header("Navigation (metres, degrees, seconds)")]
        [Min(30)] public float MinAltitude = 80;
        public float MaxAltitude = 35000000;
        public float MinPitch = 0;
        [Range(0, 75)] public float MaxPitch = 68;
        [Range(0.1f, 3)] public float OrbitSensitivity = 1;
        [Range(0.1f, 3)] public float ZoomSensitivity = 1;
        public float TiltSensitivity = 0.12f;
        public float RotationSensitivity = 1;
        [Range(0.05f, 2)] public float Inertia = 0.35f;
        [Range(0.02f, 0.5f)] public float Damping = 0.11f;
        public float TerrainClearance = 35;
        [Min(100)] public float OriginRebaseDistance = 1000;
        public double Altitude { get; private set; }
        public double Latitude => GlobeMath.Latitude(normal);
        public double Longitude => GlobeMath.Longitude(normal);
        public float Pitch => pitch;
        public float Heading => heading;
        public double3 Eye { get; private set; }
        public bool IsDragging { get; private set; }

        double3 normal, targetNormal, north, angularVelocity, origin;
        bool hasOrigin;
        float logRange, targetLogRange, zoomVelocity, pitch, targetPitch, pitchVelocity;
        float heading, targetHeading, headingVelocity;
        float terrainLift, liftVelocity;
        Vector2 zoomAnchor;
        double3 zoomPoint;
        bool hasZoomPoint;
        float anchorUntil;
        bool initialized;

        void Start() => Initialize();
        public void Initialize()
        {
            if (initialized) return;
            normal = targetNormal = GlobeMath.Normal(38, 30);
            north = math.normalize(new double3(0, 0, 1) - normal * normal.z);
            // Fit the globe to about 68% of the smaller viewport dimension.
            float halfFov = View.fieldOfView * Mathf.Deg2Rad * 0.5f;
            float angularRadius = Mathf.Atan(Mathf.Tan(halfFov) * 0.68f * Mathf.Min(1, View.aspect));
            logRange = targetLogRange = Mathf.Log((float)(GlobeMath.EquatorialRadius / Mathf.Sin(angularRadius) - GlobeMath.EquatorialRadius));
            logRange = targetLogRange = Mathf.Clamp(logRange, Mathf.Log(MinAltitude), Mathf.Log(MaxAltitude));
            initialized = true;
            PlaceCamera();
        }
        public void BeginDrag() { Initialize(); IsDragging = true; angularVelocity = default; }
        public void EndDrag(bool allowInertia = true) { IsDragging = false; if (!allowInertia) angularVelocity = default; }
        public void StopMotion() { angularVelocity = default; IsDragging = false; }
        public void Drag(Vector2 previous, Vector2 current, float dt)
        {
            double3 axis; double angle;
            if (TryPick(previous, out var a) && TryPick(current, out var b))
                GlobeMath.Between(GlobeMath.SurfaceNormal(b), GlobeMath.SurfaceNormal(a), out axis, out angle);
            else
            {
                Vector2 delta = current - previous;
                double scale = math.min(math.exp(logRange) / GlobeMath.EquatorialRadius, 2.5) / math.max(Screen.height, 1);
                double3 east = math.normalize(math.cross(north, normal));
                double3 rotation = -north * delta.x * scale + east * delta.y * scale;
                angle = math.length(rotation); axis = angle > 1e-12 ? rotation / angle : normal;
            }
            angle = math.min(angle * OrbitSensitivity, 0.2);
            targetNormal = math.normalize(GlobeMath.Rotate(targetNormal, axis, angle));
            double3 velocity = axis * angle / math.max(dt, 1.0 / 240);
            angularVelocity = math.lerp(angularVelocity, velocity, 1 - math.exp(-dt / 0.045));
        }
        public void Zoom(float logarithmicDelta, Vector2 screenAnchor)
        {
            Initialize();
            targetLogRange = Mathf.Clamp(targetLogRange + Mathf.Clamp(logarithmicDelta, -0.35f, 0.35f) * ZoomSensitivity,
                Mathf.Log(MinAltitude), Mathf.Log(MaxAltitude));
            if (Time.unscaledTime >= anchorUntil || Vector2.Distance(zoomAnchor, screenAnchor) > 4)
                hasZoomPoint = TryPick(screenAnchor, out zoomPoint);
            zoomAnchor = screenAnchor; anchorUntil = Time.unscaledTime + 0.65f;
        }
        public void TiltRotate(float verticalPixels, float rotationDegrees)
        {
            targetPitch = Mathf.Clamp(targetPitch + verticalPixels * TiltSensitivity, MinPitch, MaxPitch);
            targetHeading += rotationDegrees * RotationSensitivity;
            angularVelocity = default;
        }
        public bool TryPick(Vector2 screen, out double3 point)
        {
            Ray ray = View.ScreenPointToRay(screen);
            double3 direction = math.normalize(Georeference.TransformUnityDirectionToEarthCenteredEarthFixed((double3)(float3)ray.direction));
            return GlobeMath.Intersect(Eye, direction, out point);
        }
        void LateUpdate()
        {
            Initialize();
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            if (!IsDragging)
            {
                double speed = math.length(angularVelocity);
                if (speed > 1e-10)
                    targetNormal = math.normalize(GlobeMath.Rotate(targetNormal, angularVelocity / speed, speed * dt));
                angularVelocity *= math.exp(-dt / math.max(Inertia, 0.01));
            }
            GlobeMath.Between(normal, targetNormal, out var axis, out double angle);
            Transport(axis, angle * (1 - math.exp(-dt / Damping)));
            bool anchored = hasZoomPoint && Time.unscaledTime < anchorUntil && !IsDragging;
            logRange = Mathf.SmoothDamp(logRange, targetLogRange, ref zoomVelocity, Damping, Mathf.Infinity, dt);
            float altitudePitchLimit = Mathf.Lerp(MaxPitch, 0, Mathf.InverseLerp(250000, 4000000, Mathf.Exp(logRange)));
            pitch = Mathf.SmoothDamp(pitch, Mathf.Clamp(targetPitch, MinPitch, altitudePitchLimit), ref pitchVelocity, Damping, Mathf.Infinity, dt);
            heading = Mathf.SmoothDamp(heading, targetHeading, ref headingVelocity, Damping, Mathf.Infinity, dt);
            PlaceCamera();
            // Retain one geographic anchor across the entire eased zoom; two corrections reduce ellipsoid error.
            for (int correction = 0; anchored && correction < 2; correction++)
            {
                if (!TryPick(zoomAnchor, out var after)) break;
                GlobeMath.Between(GlobeMath.SurfaceNormal(after), GlobeMath.SurfaceNormal(zoomPoint), out axis, out angle);
                angle = math.min(angle, 0.2);
                Transport(axis, angle);
                targetNormal = math.normalize(GlobeMath.Rotate(targetNormal, axis, angle));
                PlaceCamera();
            }
        }
        void Transport(double3 axis, double angle)
        {
            normal = math.normalize(GlobeMath.Rotate(normal, axis, angle));
            north = GlobeMath.Rotate(north, axis, angle);
            north = math.normalize(north - normal * math.dot(north, normal));
        }
        void PlaceCamera()
        {
            double3 forward = GlobeMath.Rotate(north, normal, -math.radians(heading));
            double tilt = math.radians(pitch), range = math.exp(logRange);
            double3 offset = normal * math.cos(tilt) - forward * math.sin(tilt);
            Eye = GlobeMath.Surface(normal) + offset * range;
            double3 eyeLlh = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(Eye);
            if (eyeLlh.z < MinAltitude)
                Eye = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(new double3(eyeLlh.x, eyeLlh.y, MinAltitude));
            if (!hasOrigin || math.distance(origin, Eye) > OriginRebaseDistance)
            {
                origin = Eye; hasOrigin = true;
                Georeference.SetOriginEarthCenteredEarthFixed(Eye.x, Eye.y, Eye.z);
            }
            View.transform.position = (Vector3)(float3)Georeference.TransformEarthCenteredEarthFixedPositionToUnity(Eye);
            Vector3 direction = (Vector3)(float3)Georeference.TransformEarthCenteredEarthFixedDirectionToUnity(-offset);
            double3 cameraUp = normal * math.sin(tilt) + forward * math.cos(tilt);
            Vector3 up = (Vector3)(float3)Georeference.TransformEarthCenteredEarthFixedDirectionToUnity(cameraUp);
            View.transform.rotation = Quaternion.LookRotation(direction, up);
            Altitude = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(Eye).z;
            // Query already streamed colliders from above the maximum terrestrial relief.
            float requiredLift = 0;
            if (Altitude < 15000)
            {
                Physics.SyncTransforms();
                Vector3 radial = (Vector3)(float3)Georeference.TransformEarthCenteredEarthFixedDirectionToUnity(GlobeMath.Normal(eyeLlh.x, eyeLlh.y));
                if (Physics.Raycast(View.transform.position + radial * 16000, -radial, out var hit, 32000, ~0, QueryTriggerInteraction.Ignore))
                {
                    requiredLift = Mathf.Max(0, 16000 - hit.distance + TerrainClearance);
                }
            }
            if (requiredLift >= terrainLift) { terrainLift = requiredLift; liftVelocity = 0; }
            else terrainLift = Mathf.SmoothDamp(terrainLift, requiredLift, ref liftVelocity, 0.3f);
            if (terrainLift > 0.01f)
            {
                Vector3 radial = (Vector3)(float3)Georeference.TransformEarthCenteredEarthFixedDirectionToUnity(GlobeMath.Normal(eyeLlh.x, eyeLlh.y));
                View.transform.position += radial * terrainLift;
                Eye = Georeference.TransformUnityPositionToEarthCenteredEarthFixed((double3)(float3)View.transform.position);
                Altitude += terrainLift;
            }
            View.nearClipPlane = Mathf.Clamp((float)Altitude * 0.0001f, 0.5f, 2000);
            View.farClipPlane = (float)math.max(20000, math.length(Eye) + GlobeMath.EquatorialRadius * 1.2);
        }
        void OnApplicationFocus(bool focus) { if (!focus) StopMotion(); }
    }
}
