using Unity.Entities;
using UnityEngine;

namespace Alexnown.Screenshot
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ScreenshotSystem : ComponentSystem
    {
        private EntityQuery _requests;
        private EntityQueryBuilder.F_ESD<ScreenshotResult, ScreenshotRequest> _cachedForEach;
        private Camera _cachedMainCamera;

        protected override void OnCreate()
        {
            _requests = GetEntityQuery(
                ComponentType.ReadOnly<ScreenshotRequest>(),
                ComponentType.ReadOnly<ScreenshotResult>(),
                ComponentType.Exclude<ScreenshotTaken>());
            RequireForUpdate(_requests);
            _cachedForEach = TakeScreenshot;
        }

        private Camera GetMainCamera()
        {
            if (_cachedMainCamera == null) _cachedMainCamera = Camera.main;
            return _cachedMainCamera;
        }

        private void TakeScreenshot(Entity entity, ScreenshotResult result, ref ScreenshotRequest request)
        {
            Camera cam = request.TargetCamera != Entity.Null
                ? EntityManager.GetComponentObject<Camera>(request.TargetCamera)
                : GetMainCamera();
            int camCullMask = cam.cullingMask;
            float camSize = cam.orthographicSize;
            var camPos = cam.transform.position;
            var clearFlags = cam.clearFlags;
            //for screenshot with transparency on an android clearFlags must be NOT Depth or Nothing
            bool requareChangeClearFlagsForScreenshot = clearFlags == CameraClearFlags.Depth ||
                                                        clearFlags == CameraClearFlags.Nothing;
            if (requareChangeClearFlagsForScreenshot) cam.clearFlags = CameraClearFlags.SolidColor;
            cam.cullingMask = request.CullingMask;
            var screenshotSize = request.Size;
            var tempRt = RenderTexture.GetTemporary(screenshotSize.x, screenshotSize.y, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = tempRt;
            if (request.OverridedSize > 0) cam.orthographicSize = request.OverridedSize;
            if (request.OverridePosition) cam.transform.position = request.Position;

            cam.Render();
            cam.clearFlags = clearFlags;
            cam.cullingMask = camCullMask;
            if (request.OverridedSize > 0) cam.orthographicSize = camSize;
            if (request.OverridePosition) cam.transform.position = camPos;
            cam.targetTexture = null;
            RenderTexture.active = tempRt;
            Texture2D screenshot = result.Texture ?? new Texture2D(screenshotSize.x, screenshotSize.y, TextureFormat.RGBA32, false);
            screenshot.ReadPixels(new Rect(0, 0, screenshot.width, screenshot.height), 0, 0);
            screenshot.Apply(false);
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tempRt);
            if (result.Texture == null) PostUpdateCommands.SetSharedComponent(entity, new ScreenshotResult { Texture = screenshot });
        }

        protected override void OnUpdate()
        {
            PostUpdateCommands.AddComponent(_requests, ComponentType.ReadOnly<ScreenshotTaken>());
            Entities.With(_requests).ForEach(_cachedForEach);
        }
    }
}