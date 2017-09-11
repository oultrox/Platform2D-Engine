using System;
using UnityEngine;

public class Controller2D : RaycastController {

    //Nos permite conocer la informacion de colisiones del objeto.
    public struct CollisionInfo
    {
        //Controlan por donde colisiona el player con sus raycasts.
        public bool above, below;
        public bool left, right;

        //Wall jump
        public bool isAbleToWallJump;
        public bool isStickedToWall;

        //Slopes
        public bool isClimbingSlope;
        public bool isDescendingSlope;
        public bool isSlidingDownMaxSlope;  
        public float slopeAngle, slopeAngleOld;

        //Direccion hacia donde mira el movimiento del jugador
        public int faceDir;

        public Vector2 velocityOld;
        public Vector2 slopeNormal;
        public Collider2D platformStanding;

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
    
    public CollisionInfo collisionInfo;
    private float playerVerticalInput;



    //----------Metodos API----------
    //Inicialización
    public override void Start()
    {
        base.Start();
        collisionInfo.faceDir = 1;
    }

    //----------Metodos custom----------
    //Metodo de movimiento y sus sobrecargas.
    public void Move(Vector2 moveAmount, bool standingOnPlatform = false)
    {
        Move(moveAmount,0f, standingOnPlatform);
    }

    public void Move(Vector2 moveAmount, float input, bool standingOnPlatform = false)
    {
        //Actualiza y resetea cada frame.
        UpdateRaycastOrigins();
        collisionInfo.Reset();
        collisionInfo.velocityOld = moveAmount;

        //Se usa para atravesar las plataformas.
        playerVerticalInput = input;  

        if (moveAmount.y < 0)
        {
            DescendSlope(ref moveAmount);
        }

        if (moveAmount.x != 0)
        {
            collisionInfo.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        HorizontalCollisions(ref moveAmount);

        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }

        transform.Translate(moveAmount);

        //Condiciona el estado de colisión del objeto si está parado en alguna plataforma.
        if (standingOnPlatform)
        {
            collisionInfo.below = true; 
        }
    }


    //Se chequea las colisiones verticales y horizontales de manera separada para hacer más preciso la calculación en rampas y superficies.
    private void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = collisionInfo.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;

        if (Mathf.Abs(moveAmount.x) < SKIN_WIDTH)
        {
            rayLength = 2 * SKIN_WIDTH;
        }

        //Sistema similar al vertical solo que aquí condicionamos el movimiento en rampas.
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
                    collisionInfo.isAbleToWallJump = true;
                }

                //Checking si está dentro de un obstaculo en movimiento, que no le dificulte moverse saliendo de la iteración.
                if (hit.distance == 0)
                {
                    continue;
                }

                //Para tomar las rampas de manera correcta, consigue el angulo de la normal y pregunta si cumple con el minimo de empinación
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    //Si está descendiendo aplicar la velocidad antigua que tenía.
                    if (collisionInfo.isDescendingSlope)
                    {
                        collisionInfo.isDescendingSlope = false;
                        moveAmount = collisionInfo.velocityOld;
                    }

                    //Si entramos a una nueva rampa, crea el movimiento en X preciso pegado a la rampa de acuerdo a la distancia de este nuevo angulo.
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisionInfo.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - SKIN_WIDTH;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }

                    //Sube los slops en movimiento horizontal.
                    ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }

                //Aplica movimiento horizontal con sus raycasts directo si no está escalando rampas.
                if (!collisionInfo.isClimbingSlope || slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = (hit.distance - SKIN_WIDTH) * directionX;
                    rayLength = hit.distance;

                    //Corrige las colisiones de lado actualizando el movimiento en Y (Si es que está escalando).
                    if (collisionInfo.isClimbingSlope)
                    {
                        moveAmount.y = Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                    }

                    collisionInfo.left = directionX == -1;
                    collisionInfo.right = directionX == 1;
                }
            }
        }
    }

    private void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + SKIN_WIDTH;

        //Por la cantidad de los rayos necesarios verticales, lanzará los raycast para preguntar si colisionó con algo.
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigen.bottomLeft : raycastOrigen.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);

            //BEAM! Lanza el rayo que es el que contiene la colisión que se condicionará dependiendo de lo que pegó.
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin,Vector2.up * directionY, Color.red);

            if (hit)
            {
                //Almacena el collider para atravesar plataformas habilitadas.
                if (hit.collider.tag == "Atravesable")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }

                    //Si oprime hacia abajo, atravesar.
                    if (playerVerticalInput == -1)
                    {
                        if (collisionInfo.platformStanding.Equals(hit.collider))
                        {
                            continue;
                        }
                        collisionInfo.platformStanding = hit.collider;
                    }
                }
                //Almacena la informacion del collider con el que colisiona para preguntar si es el collider con el que se puede bajar.
                collisionInfo.platformStanding = hit.collider;

                //Aplica el movimiento vertical a base del a distancia del hit del raycast y la dirección vertical (directionY)
                moveAmount.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;

                //Corrige las colisiones de arriba actualizando el movimiento en X (Si es que está escalando).
                if (collisionInfo.isClimbingSlope)
                {
                    moveAmount.x = moveAmount.y / Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Math.Sign(moveAmount.x);
                }
                
                //Condiciona sus estados de si está tocando por abajo o por arriba en base a la dirección vertical.
                collisionInfo.below = directionY == -1;
                collisionInfo.above = directionY == 1;
            }
        }

        //Corrige el movimiento en interseccinoes de rampas.
        if (collisionInfo.isClimbingSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigen.bottomLeft : raycastOrigen.bottomRight) + Vector2.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisionInfo.slopeAngle)
                {
                    moveAmount.x = (hit.distance - SKIN_WIDTH) * directionX;
                    collisionInfo.slopeAngle = slopeAngle;
                    collisionInfo.slopeNormal = hit.normal;
                }
            }
        }
    }

    //Zona de slopes
    private void ClimbSlope(ref Vector2 velocity, float slopeAngle, Vector2 slopeNormal)
    {
        //consigue la cantidad que se va a mover
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisionInfo.below = true;
            collisionInfo.isClimbingSlope = true;
            collisionInfo.slopeAngle = slopeAngle;
            collisionInfo.slopeNormal = slopeNormal;
        }
    }

    private void DescendSlope (ref Vector2 moveAmount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigen.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigen.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
        SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);

        //Si no se está deslizando 
        if (!collisionInfo.isSlidingDownMaxSlope)
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

                            collisionInfo.slopeAngle = slopeAngle;
                            collisionInfo.isDescendingSlope = true;
                            collisionInfo.below = true;
                            collisionInfo.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    //Método que permite el deslize en rampas tan empinadas que no son escalables.
    private void SlideDownMaxSlope( RaycastHit2D hit, ref Vector2 MoveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                MoveAmount.x = Mathf.Sign(hit.normal.x) *(Mathf.Abs(MoveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);
                collisionInfo.slopeAngle = slopeAngle;
                collisionInfo.isSlidingDownMaxSlope = true;
                collisionInfo.slopeNormal = hit.normal;
            }
        }
    }
}
