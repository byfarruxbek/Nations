using UnityEngine;
namespace Nations.UI
{
    public static class GlobeSafeArea
    {
#if UNITY_EDITOR
        public static Rect? TestOverride;
#endif
        public static Rect Current
        {
            get
            {
#if UNITY_EDITOR
                if(TestOverride.HasValue)return TestOverride.Value;
#endif
                return Screen.safeArea;
            }
        }
    }
}
