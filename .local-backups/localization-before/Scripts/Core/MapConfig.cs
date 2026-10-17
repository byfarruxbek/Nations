using UnityEngine;

namespace Nations
{
    public enum MobileQuality { Low, Medium, High }
    [CreateAssetMenu(menuName = "Nations/Map Configuration")]
    public sealed class MapConfig : ScriptableObject
    {
        public MobileQuality Quality = MobileQuality.Medium;
        [Tooltip("Resources-relative JSON path. The real file is ignored by Git.")]
        public string LocalConfigResource = "NationsMapConfig.local";
        public int TargetFrameRate = 60;
        public float LoadingTimeout = 30;
    }
    [System.Serializable]
    public sealed class MapCredentials
    {
        public string GOOGLE_MAP_TILES_API_KEY = "YOUR_API_KEY";
        public string CESIUM_ION_ACCESS_TOKEN = "";
    }
}
