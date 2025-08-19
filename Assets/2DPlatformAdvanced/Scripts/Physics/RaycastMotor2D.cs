using UnityEngine;

/// <summary>
/// Physics motor based on raycasts. Used to extend either our player controllers or movable objects in the game.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class RaycastMotor2D : MonoBehaviour 
{
    /// <summary>
    /// Helps define the edges of the raycast origin based on the collider.
    /// </summary>
    protected struct RaycastOrigin
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
    
    public LayerMask collisionMask;
    protected const float SKIN_WIDTH = 0.015f;
    protected const float DISTANCE_BETWEEN_RAYS = 0.1f;
    protected int horizontalRayCount;
    protected int verticalRayCount;
    protected float horizontalRaySpacing;
    protected float verticalRaySpacing;
    protected BoxCollider2D colliderObj;
    protected RaycastOrigin raycastOrigin;
    private Bounds _bounds;

    public virtual void Start()
    {
        colliderObj = this.GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    /// <summary>
    /// Updates the positions of the raycast origins based on the collider size.
    /// </summary>
    protected void UpdateRaycastOrigins()
    {
        _bounds = colliderObj.bounds;
        _bounds.Expand(SKIN_WIDTH * -2); // Shrinks bounds slightly to inset raycasts and prevent bugs.

        raycastOrigin.bottomLeft.x = _bounds.min.x;
        raycastOrigin.bottomLeft.y = _bounds.min.y;

        raycastOrigin.bottomRight.x = _bounds.max.x;
        raycastOrigin.bottomRight.y = _bounds.min.y;

        raycastOrigin.topLeft.x = _bounds.min.x;
        raycastOrigin.topLeft.y = _bounds.max.y;

        raycastOrigin.topRight.x = _bounds.max.x;
        raycastOrigin.topRight.y = _bounds.max.y;
    }

    /// <summary>
    /// Calculates the spacing between raycast lines. Done only once.
    /// </summary>
    private void CalculateRaySpacing()
    {
        _bounds = colliderObj.bounds;
        _bounds.Expand(SKIN_WIDTH * -2); 

        // Get the width and height of the collider.
        float boundsWidth = _bounds.size.x;
        float boundsHeight = _bounds.size.y;

        // Set the number of rays to cast based on the spacing along the opposite side.
        horizontalRayCount = Mathf.RoundToInt(boundsHeight / DISTANCE_BETWEEN_RAYS);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / DISTANCE_BETWEEN_RAYS);

        horizontalRaySpacing = _bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = _bounds.size.x / (verticalRayCount - 1);
    }
}
