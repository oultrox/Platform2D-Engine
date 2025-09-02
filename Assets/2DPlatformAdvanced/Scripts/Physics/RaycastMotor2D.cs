using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles raycast-based physics.
/// </summary>
public class RaycastMotor2D
{
    public const float SKIN_WIDTH = 0.015f;
    public const float DISTANCE_BETWEEN_RAYS = 0.1f;
    protected BoxCollider2D collider;
    protected Bounds bounds;
    protected RaycastOrigin raycastOrigin;
    
    public int HorizontalRayCount { get; private set; }
    public int VerticalRayCount { get; private set; }
    public float HorizontalRaySpacing { get; private set; }
    public float VerticalRaySpacing { get; private set; }
    public Bounds GetBounds() => bounds;
    public Vector2 GetBottomLeft() => raycastOrigin.bottomLeft;
    public Vector2 GetBottomRight() => raycastOrigin.bottomRight;
    public Vector2 GetTopLeft() => raycastOrigin.topLeft;
    public Vector2 GetTopRight() => raycastOrigin.topRight;
    
    protected struct RaycastOrigin
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    
    public RaycastMotor2D(BoxCollider2D collider)
    {
        this.collider = collider;
        CalculateRaySpacing();
        UpdateRaycastOrigins();
    }

    public void UpdateRaycastOrigins()
    {
        bounds = collider.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        raycastOrigin.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigin.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigin.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigin.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        bounds = collider.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        HorizontalRayCount = Mathf.RoundToInt(bounds.size.y / DISTANCE_BETWEEN_RAYS);
        VerticalRayCount = Mathf.RoundToInt(bounds.size.x / DISTANCE_BETWEEN_RAYS);

        HorizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
        VerticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);
    }
}
