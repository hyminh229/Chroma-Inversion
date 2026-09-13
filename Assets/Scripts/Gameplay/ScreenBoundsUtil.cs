using UnityEngine;

public static class ScreenBoundsUtil
{
    public static void GetWorldBounds(out float minX, out float maxX, out float bottomY, out float topY)
    {
        Camera cam = Camera.main;
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane));

        minX = bottomLeft.x;
        maxX = topRight.x;
        bottomY = bottomLeft.y;
        topY = topRight.y;
    }
}