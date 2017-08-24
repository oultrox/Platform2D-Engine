using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    public LayerMask collisionMask;

    protected const float SKIN_WIDTH = 0.015f;
    public int horizontalRayCount = 2;
    public int verticalRayCount = 4;


    protected float horizontalRaySpacing;
    protected float verticalRaySpacing;

    protected BoxCollider2D coll;
    protected RaycastOrigins raycastOrigins;


    // Use this for initialization
    public virtual void Start()
    {
        coll = this.GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    //Nos facilita los limites de el origen del raycast via el collider.
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    //Lanza las lineas del raycast del origen en base al grosor del collider del jugador.
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

    }

    //calcula el espacio entre las lineas del raycasting.
    public void CalculateRaySpacing()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.y / (verticalRayCount - 1);
    }
}
