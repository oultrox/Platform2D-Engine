using System;
using UnityEngine;

public class PlatformMotor2D : RaycastMotor2D
{

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
            left = right = false;

            isClimbingSlope = false;
            isDescendingSlope = false;
            isSlidingDownMaxSlope = false;
            isAbleToWallJump = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0f;
            slopeNormal = Vector2.zero;
        }
    }

    //Variables editables via inspector
    public float maxSlopeAngle = 80;
    public CollisionInfo collisionInfo;

    //Cached variables
    private float playerVerticalInput;
    private float rayLength;
    private float directionX;
    private float directionY;
    private float slopeAngle;
    private float distanceToSlopeStart;
    private float descendVelocityY;
    private float moveDistance;
    private float climbVelocityY;

    private Vector2 rayOrigin;
    private RaycastHit2D raycastHit;
    private RaycastHit2D maxSlopeHitLeft;
    private RaycastHit2D maxSlopeHitRight;



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
        Move(moveAmount, 0f, standingOnPlatform);
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
        directionX = collisionInfo.faceDir;
        rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;

        if (Mathf.Abs(moveAmount.x) < SKIN_WIDTH)
        {
            rayLength = 2 * SKIN_WIDTH;
        }

        //Sistema similar al vertical solo que aquí condicionamos el movimiento en rampas.
        for (int i = 0; i < horizontalRayCount; i++)
        {
            rayOrigin = (directionX == -1) ? raycastOrigen.bottomLeft : raycastOrigen.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            raycastHit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (!raycastHit)
            {
                continue;
            }

            //Checking si el muro con el cual está colisionando es escalable.
            if (raycastHit.collider.CompareTag("Escalable"))
            {
                collisionInfo.isAbleToWallJump = true;
            }

            //Checking si está dentro de un obstaculo en movimiento, que no le dificulte moverse saliendo de la iteración.
            if (raycastHit.distance == 0)
            {
                continue;
            }

            //Para tomar las rampas de manera correcta, consigue el angulo de la normal y pregunta si cumple con el minimo de empinación
            slopeAngle = Vector2.Angle(raycastHit.normal, Vector2.up);
            if (i == 0 && slopeAngle <= maxSlopeAngle)
            {
                //Si está descendiendo aplicar la velocidad antigua que tenía.
                if (collisionInfo.isDescendingSlope)
                {
                    collisionInfo.isDescendingSlope = false;
                    moveAmount = collisionInfo.velocityOld;
                }

                //Si entramos a una nueva rampa, crea el movimiento en X preciso pegado a la rampa de acuerdo a la distancia de este nuevo angulo.
                distanceToSlopeStart = 0;
                if (slopeAngle != collisionInfo.slopeAngleOld)
                {
                    distanceToSlopeStart = raycastHit.distance - SKIN_WIDTH;
                    moveAmount.x -= distanceToSlopeStart * directionX;
                }

                //Sube los slops en movimiento horizontal.
                ClimbSlope(ref moveAmount, slopeAngle, raycastHit.normal);
                moveAmount.x += distanceToSlopeStart * directionX;
            }

            //Aplica movimiento horizontal con sus raycasts directo si no está escalando rampas.
            if (!collisionInfo.isClimbingSlope || slopeAngle > maxSlopeAngle)
            {
                moveAmount.x = (raycastHit.distance - SKIN_WIDTH) * directionX;
                rayLength = raycastHit.distance;

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

    private void VerticalCollisions(ref Vector2 moveAmount)
    {
        directionY = Mathf.Sign(moveAmount.y);
        rayLength = Mathf.Abs(moveAmount.y) + SKIN_WIDTH;

        //Por la cantidad de los rayos necesarios verticales, lanzará los raycast para preguntar si colisionó con algo.
        for (int i = 0; i < verticalRayCount; i++)
        {
            rayOrigin = (directionY == -1) ? raycastOrigen.bottomLeft : raycastOrigen.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);

            //BEAM! Lanza el rayo que es el que contiene la colisión que se condicionará dependiendo de lo que pegó.
            raycastHit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (!raycastHit)
            {
                continue;
            }
            //Almacena el collider para atravesar plataformas habilitadas.
            if (raycastHit.collider.CompareTag("Atravesable"))
            {
                if (directionY == 1 || raycastHit.distance == 0)
                {
                    continue;
                }

                //Si oprime hacia abajo, atravesar.
                if (playerVerticalInput == -1)
                {
                    if (collisionInfo.platformStanding.Equals(raycastHit.collider))
                    {
                        continue;
                    }
                    collisionInfo.platformStanding = raycastHit.collider;
                }
            }
            //Almacena la informacion del collider con el que colisiona para preguntar si es el collider con el que se puede bajar.
            collisionInfo.platformStanding = raycastHit.collider;

            //Aplica el movimiento vertical a base del a distancia del hit del raycast y la dirección vertical (directionY)
            moveAmount.y = (raycastHit.distance - SKIN_WIDTH) * directionY;
            rayLength = raycastHit.distance;

            //Corrige las colisiones de arriba actualizando el movimiento en X (Si es que está escalando).
            if (collisionInfo.isClimbingSlope)
            {
                moveAmount.x = moveAmount.y / Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Math.Sign(moveAmount.x);
            }

            //Condiciona sus estados de si está tocando por abajo o por arriba en base a la dirección vertical.
            collisionInfo.below = directionY == -1;
            collisionInfo.above = directionY == 1;
        }

        //Corrige el movimiento en interseccinoes de rampas.
        if (collisionInfo.isClimbingSlope)
        {
            directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;
            rayOrigin = ((directionX == -1) ? raycastOrigen.bottomLeft : raycastOrigen.bottomRight) + Vector2.up * moveAmount.y;
            raycastHit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (raycastHit)
            {
                slopeAngle = Vector2.Angle(raycastHit.normal, Vector2.up);
                if (slopeAngle != collisionInfo.slopeAngle)
                {
                    moveAmount.x = (raycastHit.distance - SKIN_WIDTH) * directionX;
                    collisionInfo.slopeAngle = slopeAngle;
                    collisionInfo.slopeNormal = raycastHit.normal;
                }
            }
        }
    }


    //Rampas
    private void ClimbSlope(ref Vector2 velocity, float slopeAngle, Vector2 slopeNormal)
    {
        //consigue la cantidad que se va a mover
        moveDistance = Mathf.Abs(velocity.x);
        climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        bool isActualYVelocityOutdated = (velocity.y <= climbVelocityY);
        if (!isActualYVelocityOutdated)
        {
            return;
        }

        velocity.y = climbVelocityY;
        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
        collisionInfo.below = true;
        collisionInfo.isClimbingSlope = true;
        collisionInfo.slopeAngle = slopeAngle;
        collisionInfo.slopeNormal = slopeNormal;

    }


    private void DescendSlope(ref Vector2 moveAmount)
    {
        maxSlopeHitLeft = Physics2D.Raycast(raycastOrigen.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        maxSlopeHitRight = Physics2D.Raycast(raycastOrigen.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
        SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);

        if (collisionInfo.isSlidingDownMaxSlope)
        {
            return;
        }

        //desciende normalmente por la rampa.
        directionX = Mathf.Sign(moveAmount.x);
        rayOrigin = (directionX == -1) ? raycastOrigen.bottomRight : raycastOrigen.bottomLeft;
        raycastHit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
        if (!raycastHit)
        {
            return;
        }

        slopeAngle = Vector2.Angle(raycastHit.normal, Vector2.up);
        bool isAngleDescendable = slopeAngle != 0 && slopeAngle <= maxSlopeAngle;
        if (!isAngleDescendable)
        {
            return;
        }

        bool isSameDirection = Mathf.Sign(raycastHit.normal.x) == directionX;
        if (!isSameDirection)
        {
            return;
        }

        bool isNeededToUpdateMovement = raycastHit.distance - SKIN_WIDTH <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
        if (!isNeededToUpdateMovement)
        {
            return;
        }

        moveDistance = Mathf.Abs(moveAmount.x);
        descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
        moveAmount.y -= descendVelocityY;

        collisionInfo.slopeAngle = slopeAngle;
        collisionInfo.isDescendingSlope = true;
        collisionInfo.below = true;
        collisionInfo.slopeNormal = raycastHit.normal;
    }

    //Método que permite el deslize en rampas tan empinadas que no son escalables.
    private void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 MoveAmount)
    {
        if (!hit)
        {
            return;
        }

        slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        bool isDeslizable = (slopeAngle > maxSlopeAngle);
        if(!isDeslizable)
        {
            return;
        }
        
        MoveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(MoveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);
        collisionInfo.slopeAngle = slopeAngle;
        collisionInfo.isSlidingDownMaxSlope = true;
        collisionInfo.slopeNormal = hit.normal;
    }
}
