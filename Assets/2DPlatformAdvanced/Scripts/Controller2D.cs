using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {

    const float SKIN_WIDTH = 0.015f;
    public int horizontalRayCount = 2;
    public int verticalRayCount = 4;
    public LayerMask collisionMask;
    public CollisionInfo collisions;
    float maxClimbAngle = 80;
    float maxDescendAngle = 75;

    private float horizontalRaySpacing;
    private float verticalRaySpacing;

    BoxCollider2D coll;
    RaycastOrigins raycastOrigins;

    //Nos facilita los limites de el origen del raycast via el collider.
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    //Nos permite conocer la informacion sobre donde está colisionando.
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        
        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }

    // Use this for initialization
    void Start ()
    {
        coll = this.GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    //Chequea la colisión separadamente como en los clásicos donde utiliza los raycastr para saber si colisióno con un sólido.
    //Modificando así el vector de velocidad que es el que permitirá movernos o detenernos con los sólidos.
    public void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin,Vector2.up * directionY * rayLength, Color.red);
            if (hit)
            {
                if (collisions.climbingSlope)
                {
                    velocity.x =velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Math.Abs(velocity.x);
                }
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }
    }

    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i==0 && slopeAngle <= maxClimbAngle)
                {
                    float distanceToSlopeStart = 0;
                    if (slopeAngle!= collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - SKIN_WIDTH;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs(velocity.x);
                    }
                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
                
            }
        }
        if (collisions.climbingSlope)
        {
            directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                }
            }
        }
    }

    private void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    private void DescendSlope (ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity,collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - SKIN_WIDTH <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

    //Lanza las lineas del raycast del origen en base al grosor del collider del jugador.
    void UpdateRaycastOrigins()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

    }

    //calcula el espacio entre las lineas del raycasting.
    void CalculateRaySpacing()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.y / (verticalRayCount - 1);
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        collisions.Reset();

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }
        if (velocity.x!=0)
        {
            HorizontalCollisions(ref velocity);
        }

        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }
        transform.Translate(velocity);
    }
}
