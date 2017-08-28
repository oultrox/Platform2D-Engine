using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Motor de física basado en Raycasts.

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {
    
    //Nos facilita los limites de el origen del raycast via el collider.
    public struct RaycastOrigins
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
    protected RaycastOrigins raycastOrigins;


    // Use this for initialization
    public virtual void Start()
    {
        colliderObj = this.GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    //Actualiza las lineas del raycast del origen en base al grosor del collider del jugador.
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = colliderObj.bounds;
        bounds.Expand(SKIN_WIDTH * -2); // esto permite que tenga un pequeño del raycast dentro del jugador, para evitar bugs.

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

    }

    //calcula el espacio entre las lineas del raycasting. se hace 1 sóla vez.
    public void CalculateRaySpacing()
    {
        Bounds bounds = colliderObj.bounds;
        bounds.Expand(SKIN_WIDTH * -2); 

        //Consigue el ancho y largo del collider.
        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        //Setea el numero de raycast que debe lanzar en base el espaciado del lado contrario entre la distancia entre ellos.
        horizontalRayCount = Mathf.RoundToInt(boundsHeight / DISTANCE_BETWEEN_RAYS);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / DISTANCE_BETWEEN_RAYS);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
}
