using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;

namespace Nations.Globe
{
    [DefaultExecutionOrder(200)]
    public sealed class SetupEllipsoid : MonoBehaviour
    {
        public CesiumGeoreference Georeference;
        void LateUpdate() => Refresh();
        public void Refresh()
        {
            transform.position = (Vector3)(float3)Georeference.TransformEarthCenteredEarthFixedPositionToUnity(double3.zero);
            Vector3 up = (Vector3)(float3)Georeference.TransformEarthCenteredEarthFixedDirectionToUnity(new double3(0, 0, 1));
            Vector3 forward = (Vector3)(float3)Georeference.TransformEarthCenteredEarthFixedDirectionToUnity(new double3(0, 1, 0));
            transform.rotation = Quaternion.LookRotation(forward, up);
            transform.localScale = new Vector3((float)GlobeMath.EquatorialRadius * 2, (float)GlobeMath.PolarRadius * 2, (float)GlobeMath.EquatorialRadius * 2);
        }
    }
}
