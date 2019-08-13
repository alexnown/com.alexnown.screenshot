using System;
using Unity.Entities;
using UnityEngine;

namespace Alexnown.Screenshot
{
    public struct ScreenshotResult : ISharedComponentData, IEquatable<ScreenshotResult>
    {
        public Texture2D Texture;

        public bool Equals(ScreenshotResult other)
        {
            if (Texture == null) return other.Texture == null;
            return Texture.Equals(other.Texture);
        }

        public override int GetHashCode()
        {
            return Texture == null ? -1 : Texture.GetHashCode();
        }
    }
}
