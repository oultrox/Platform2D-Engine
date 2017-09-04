using System;
using UnityEngine;


public class Controller2D : RaycastController {

    //Nos permite conocer la informacion sobre donde está colisionando.
    public struct CollisionState
    {
        //Controlan por donde colisiona el player con sus raycasts.
        public bool above, below;
        public bool left, right;

        //Wall Jump
        public bool isAbleToWallJump;

        //Slopes
        public bool isClimbingSlope;
        public bool isDescendingSlope;
        public bool isSlidingDownMaxSlope;

        public int faceDir;
        public float slopeAngle, slopeAngleOld;
        public Vector2 velocityOld;
        public Vector2 slopeNormal;
        public Collider2D fallThroughPlatform;

        //Metodo de reinicio de las variables.
        public void Reset()
        {
            above = below = false;
            left  = right = false;

            isClimbingSlope       = false;
            isDescendingSlope     = false;
            isSlidingDownMaxSlope = false;
            isAbleToWallJump      = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0f;
            slopeNormal = Vector2.zero;
        }
    }

    //Variables editables via inspector
    public float maxSlopeAngle = 80;
    
    public CollisionState collisionState;
    private float playerVerticalInput;



    //----------Metodos API----------
    //Inicialización de su base y la dirección inicial la cual mira el jugador es hacia la derecha.
    public override void Start()
    {
        base.Start();
        collisionState.faceDir = 1;
    }

    //----------Metodos custom----------
    //Sobrecarga de el metodo de movimiento
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
    //Chequeo de Raycastar en colisiones verticales.
    public void VerticalCollisions(ref Vector2 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

        //Por la cantidad de los rayos necesarios verticales, lanzará los raycast para preguntar si colisionó con algo.
        for (int i = 0; i < verticalRayCount; i++)
        {
            //Acá pregunta hacia donde estamos mirando básicamente en nuestra dirección vertical
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigen.bottomLeft : raycastOrigen.topLeft;
            //Le suma el espaciado entre rayos
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            //y BEAM! Lanza el rayo que es el que contiene la colisión que se condicionará dependiendo de lo que pegó.
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin,Vector2.up * directionY, Color.red);

            //Si pegó con algo entonces detener y revisar con qué colisionó
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

                //Aplica el movimiento vertical a base del a distancia del hit del raycast y la dirección vertical (directionY)
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;

                //Corrige el movimiento horizontal en las empinadas o rampas.
                if (collisionState.isClimbingSlope)
                {
                    velocity.x =velocity.y / Mathf.Tan(collisionState.slopeAngle * Mathf.Deg2Rad) * Math.Abs(velocity.x);
                }
                
                //Condiciona sus estados de si está tocando por abajo o por arriba en base a la dirección vertical.
                collisionState.below = directionY == -1;
                collisionState.above = directionY == 1;
            }
        }
    }

    //Chequeo de raycaster en colisiones horizontales 
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
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigen.bottomLeft : raycastOrigen.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (hit)
            {
                //Checking si el muro con el cual está colisionando es escalable.
                if (hit.collider.CompareTag("Escalable"))
                {
                    collisionState.isAbleToWallJump = true;
                }

                //Checking si está dentro de un obstaculo en movimiento, que no le dificulte moverse.
                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i==0 && slopeAngle <= maxSlopeAngle)
                {
                    if (collisionState.isDescendingSlope)
                    {
                        collisionState.isDescendingSlope = false;
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

                if (!collisionState.isClimbingSlope || slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = (hit.distance - SKIN_WIDTH) * directionX;
                    rayLength = hit.distance;

                    if (collisionState.isClimbingSlope)
                    {
                        moveAmount.y = Mathf.Tan(collisionState.slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs(moveAmount.x);
                    }
                    collisionState.left = directionX == -1;
                    collisionState.right = directionX == 1;
                }
                
            }
        }
        if (collisionState.isClimbingSlope)
        {
            directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigen.bottomLeft : raycastOrigen.bottomRight) + Vector2.up * moveAmount.y;
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
            collisionState.isClimbingSlope = true;
            collisionState.slopeAngle = slopeAngle;
            collisionState.slopeNormal = slopeNormal;
        }
    }

    private void DescendSlope (ref Vector2 moveAmount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigen.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigen.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
        SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);

        //Si no se está deslizando 
        if (!collisionState.isSlidingDownMaxSlope)
        {
            //desciende normalmente por la rampa.
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigen.bottomRight : raycastOrigen.bottomLeft;
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
                            collisionState.isDescendingSlope = true;
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
                collisionState.isSlidingDownMaxSlope = true;
                collisionState.slopeNormal = hit.normal;
            }
        }
    }
}
