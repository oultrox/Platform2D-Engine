using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Controller2D : RaycastController {

    //Nos permite conocer la informacion sobre donde está colisionando.
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector2 velocityOld;
        public int faceDir;

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

    private float maxClimbAngle = 80;
    private float maxDescendAngle = 75;
    public CollisionInfo collisionInfo;

    //----Metodos API-----
    public override void Start()
    {
        base.Start();
        collisionInfo.faceDir = 1;
    }

    //-----Metodos custom------
    //Funcion de movimiento de el player.
    public void Move(Vector2 moveAmount, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        collisionInfo.Reset();
        collisionInfo.velocityOld = moveAmount;

        if (moveAmount.x != 0)
        {
            collisionInfo.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        if (moveAmount.y < 0)
        {
            DescendSlope(ref moveAmount);
        }

        //quitado del if para chequearsiempre, si quieres puedes devolver esto así no toca siempre el muro.
        HorizontalCollisions(ref moveAmount);

        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }
        transform.Translate(moveAmount);
        if (standingOnPlatform)
        {
            collisionInfo.below = true; 
        }
    }

    //Chequea la colisión separadamente como en los clásicos donde utiliza los raycastr para saber si colisióno con un sólido.
    //Modificando así el vector de velocidad que es el que permitirá movernos o detenernos con los sólidos.
    public void VerticalCollisions(ref Vector2 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin,Vector2.up * directionY, Color.red);

            if (hit)
            {
                //Atravesar suelos hacia arriba
                if (hit.collider.tag == "Atravesable")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                }

                if (collisionInfo.climbingSlope)
                {
                    velocity.x =velocity.y / Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Math.Abs(velocity.x);
                }
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;

                collisionInfo.below = directionY == -1;
                collisionInfo.above = directionY == 1;
            }
        }
    }

    void HorizontalCollisions(ref Vector2 velocity)
    {
        //cambiado de mathf.sign(velocity.x), si quieres puedes devolver esto así no toca siempre
        float directionX = collisionInfo.faceDir;
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

        if (Mathf.Abs(velocity.x) < SKIN_WIDTH){
            rayLength = 2 * SKIN_WIDTH;
        }
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (hit)
            {
                //Checking si está dentro de un obstaculo en movimiento, que no le dificulte moverse.
                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i==0 && slopeAngle <= maxClimbAngle)
                {
                    if (collisionInfo.descendingSlope)
                    {
                        collisionInfo.descendingSlope = false;
                        velocity = collisionInfo.velocityOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle!= collisionInfo.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - SKIN_WIDTH;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                if (!collisionInfo.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                    rayLength = hit.distance;

                    if (collisionInfo.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs(velocity.x);
                    }
                    collisionInfo.left = directionX == -1;
                    collisionInfo.right = directionX == 1;
                }
                
            }
        }
        if (collisionInfo.climbingSlope)
        {
            directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisionInfo.slopeAngle)
                {
                    velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                }
            }
        }
    }

    private void ClimbSlope(ref Vector2 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisionInfo.below = true;
            collisionInfo.climbingSlope = true;
            collisionInfo.slopeAngle = slopeAngle;
        }
    }

    private void DescendSlope (ref Vector2 velocity)
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

                        collisionInfo.slopeAngle = slopeAngle;
                        collisionInfo.descendingSlope = true;
                        collisionInfo.below = true;
                    }
                }
            }
        }
    }
}
