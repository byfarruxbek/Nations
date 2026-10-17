using System;
using Unity.Mathematics;

namespace Nations.Globe
{
    public static class GlobeMath
    {
        public const double EquatorialRadius = 6378137.0;
        public const double PolarRadius = 6356752.314245;
        public static readonly double3 Radii = new double3(EquatorialRadius, EquatorialRadius, PolarRadius);

        public static double3 Normal(double longitude, double latitude)
        {
            double lon = math.radians(longitude), lat = math.radians(latitude);
            return new double3(math.cos(lat) * math.cos(lon), math.cos(lat) * math.sin(lon), math.sin(lat));
        }
        public static double3 Surface(double3 normal)
        {
            double3 k = Radii * Radii * normal;
            return k / math.sqrt(math.dot(k, normal));
        }
        public static double3 SurfaceNormal(double3 p) => math.normalize(p / (Radii * Radii));
        public static double3 Rotate(double3 vector, double3 axis, double angle)
        {
            return vector * math.cos(angle) + math.cross(axis, vector) * math.sin(angle)
                + axis * math.dot(axis, vector) * (1 - math.cos(angle));
        }
        public static void Between(double3 from, double3 to, out double3 axis, out double angle)
        {
            double3 cross = math.cross(from, to);
            double length = math.length(cross);
            angle = math.atan2(length, math.clamp(math.dot(from, to), -1, 1));
            axis = length > 1e-12 ? cross / length : math.normalize(math.cross(from,
                math.abs(from.z) < 0.9 ? new double3(0, 0, 1) : new double3(1, 0, 0)));
        }
        public static bool Intersect(double3 origin, double3 direction, out double3 point)
        {
            double3 o = origin / Radii, d = direction / Radii;
            double a = math.dot(d, d), b = math.dot(o, d), c = math.dot(o, o) - 1;
            double discriminant = b * b - a * c;
            if (discriminant < 0 || a < 1e-30) { point = default; return false; }
            double root = math.sqrt(discriminant);
            double t = (-b - root) / a;
            if (t < 0) t = (-b + root) / a;
            point = origin + direction * t;
            return t >= 0 && math.all(math.isfinite(point));
        }
        public static double Longitude(double3 normal) => math.degrees(math.atan2(normal.y, normal.x));
        public static double Latitude(double3 normal) => math.degrees(math.asin(math.clamp(normal.z, -1, 1)));
    }
}
