using UnityEngine;

//Motor físico basado en Raycasts.
[RequireComponent(typeof(BoxCollider2D))]
public class RaycastMotor2D : MonoBehaviour {
    
    //Nos facilita los limites de el origen del raycast via el collider.
    public struct RaycastOrigin
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    //Variables editables vía editor
    public LayerMask collisionMask;

    //Heredado
    protected const float SKIN_WIDTH = 0.015f;
    protected const float DISTANCE_BETWEEN_RAYS = 0.1f;

    protected int horizontalRayCount;
    protected int verticalRayCount;
    protected float horizontalRaySpacing;
    protected float verticalRaySpacing;

    protected BoxCollider2D colliderObj;
    protected RaycastOrigin raycastOrigen;

    //-----------Metodos API-----------
    // Use this for initialization
    public virtual void Start()
    {
        colliderObj = this.GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    //-----------Metodos custom-----------
    //Actualiza las lineas del raycast del origen en base al grosor del collider del jugador.
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = colliderObj.bounds;
        bounds.Expand(SKIN_WIDTH * -2); // esto permite que tenga un pequeño del raycast dentro del jugador, para evitar bugs.

        raycastOrigen.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigen.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigen.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigen.topRight = new Vector2(bounds.max.x, bounds.max.y);

    }

    //calcula el espacio entre las lineas del raycasting. se hace 1 sóla vez.
    private void CalculateRaySpacing()
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
