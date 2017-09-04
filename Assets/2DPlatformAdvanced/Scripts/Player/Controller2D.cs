using System;
using UnityEngine;


public class Controller2D : RaycastController {

    //Nos permite conocer la informacion sobre donde está colisionando.
    public struct CollisionState
    {
        public bool above, below;
        public bool left, right;

        public bool isWallJumpable;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector2 velocityOld;
        public Vector2 slopeNormal;
        public int faceDir;
        public Collider2D fallThroughPlatform;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;
            isWallJumpable = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
            slopeNormal = Vector2.zero;
        }
    }

    public float maxSlopeAngle = 80;
    public CollisionState collisionState;
    private float playerVerticalInput;

    //----Metodos API-----
    public override void Start()
    {
        base.Start();
        collisionState.faceDir = 1;
    }

    //-----Metodos custom------
    public void Move(Vector2 moveAmount, bool standingOnPlatform = false)
    {
        Move(moveAmount,0f, standingOnPlatform);
    }

    //Funcion de movimiento de el player.
    public void Move(Vector2 moveAmount, float input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        collisionState.Reset();
        collisionState.velocityOld = moveAmount;
        playerVerticalInput = input;

        if (moveAmount.y < 0)
        {
            DescendSlope(ref moveAmount);
        }

        if (moveAmount.x != 0)
        {
            collisionState.faceDir = (int)Mathf.Sign(moveAmount.x);
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
            collisionState.below = true; 
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

                    if (playerVerticalInput == -1)
                    {
                        if (collisionState.fallThroughPlatform.Equals(hit.collider))
                        {
                            continue;
                        }
                        collisionState.fallThroughPlatform = hit.collider;
                        
                    }
                }
                //Almacena la informacion del collider con el que colisiona para preguntar si es el collider con el que se puede bajar.
                collisionState.fallThroughPlatform = hit.collider;

                //Aplica el movimiento vertical
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;

                //Corrige el movimiento horizontal en las empinadas o rampas.
                if (collisionState.climbingSlope)
                {
                    velocity.x =velocity.y / Mathf.Tan(collisionState.slopeAngle * Mathf.Deg2Rad) * Math.Abs(velocity.x);
                }
                
                //Condiciona sus estados de si está tocando por abajo o por arriba.
                collisionState.below = directionY == -1;
                collisionState.above = directionY == 1;
            }
        }
    }

    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        //cambiado de mathf.sign(velocity.x), si quieres puedes devolver esto así no toca siempre
        float directionX = collisionState.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;

        if (Mathf.Abs(moveAmount.x) < SKIN_WIDTH){
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
                //Checking si el muro con el cual está colisionando es escalable.
                if (hit.collider.CompareTag("Escalable"))
                {
                    collisionState.isWallJumpable = true;
                }

                //Checking si está dentro de un obstaculo en movimiento, que no le dificulte moverse.
                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i==0 && slopeAngle <= maxSlopeAngle)
                {
                    if (collisionState.descendingSlope)
                    {
                        collisionState.descendingSlope = false;
                        moveAmount = collisionState.velocityOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle!= collisionState.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - SKIN_WIDTH;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }

                if (!collisionState.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = (hit.distance - SKIN_WIDTH) * directionX;
                    rayLength = hit.distance;

                    if (collisionState.climbingSlope)
                    {
                        moveAmount.y = Mathf.Tan(collisionState.slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs(moveAmount.x);
                    }
                    collisionState.left = directionX == -1;
                    collisionState.right = directionX == 1;
                }
                
            }
        }
        if (collisionState.climbingSlope)
        {
            directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisionState.slopeAngle)
                {
                    moveAmount.x = (hit.distance - SKIN_WIDTH) * directionX;
                    collisionState.slopeAngle = slopeAngle;
                    collisionState.slopeNormal = hit.normal;
                }
            }
        }
    }

    private void ClimbSlope(ref Vector2 velocity, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisionState.below = true;
            collisionState.climbingSlope = true;
            collisionState.slopeAngle = slopeAngle;
            collisionState.slopeNormal = slopeNormal;
        }
    }

    private void DescendSlope (ref Vector2 moveAmount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
        SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);

        //Si no se está deslizando 
        if (!collisionState.slidingDownMaxSlope)
        {
            //desciende normalmente por la rampa.
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - SKIN_WIDTH <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendVelocityY;

                            collisionState.slopeAngle = slopeAngle;
                            collisionState.descendingSlope = true;
                            collisionState.below = true;
                            collisionState.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope( RaycastHit2D hit, ref Vector2 MoveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                MoveAmount.x = Mathf.Sign(hit.normal.x) *(Mathf.Abs(MoveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);
                collisionState.slopeAngle = slopeAngle;
                collisionState.slidingDownMaxSlope = true;
                collisionState.slopeNormal = hit.normal;
            }
        }
    }
}
