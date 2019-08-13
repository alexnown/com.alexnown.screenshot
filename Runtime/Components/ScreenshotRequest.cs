using Unity.Entities;
using Unity.Mathematics;

namespace Alexnown.Screenshot
{
    public struct ScreenshotRequest : IComponentData
    {
        public Entity TargetCamera;
        public int2 Size;
        public bool OverridePosition;
        public float3 Position;
        public float OverridedSize;
        public int CullingMask;
    }
}
