using System;
using UnityEngine;

public class PlatformMotor2D : RaycastMotor2D
{
    // Stores collision state information of the object.
    public struct CollisionInfo
    {
        // Track collisions around the player using raycasts.
        public bool above, below;
        public bool left, right;

        // Wall jump support
        public bool isAbleToWallJump;
        public bool isStickedToWall;

        // Slopes
        public bool isClimbingSlope;
        public bool isDescendingSlope;
        public bool isSlidingDownMaxSlope;
        public float slopeAngle, slopeAngleOld;

        // Direction the player is facing based on movement
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
    
    public float maxSlopeAngle = 80;
    public CollisionInfo collisionInfo;
    private float playerVerticalInput;
    
    
    public override void Start()
    {
        base.Start();
        collisionInfo.faceDir = 1;
    }
    
    public void Move(Vector2 moveAmount, bool standingOnPlatform = false)
    {
        Move(moveAmount, 0f, standingOnPlatform);
    }

    public void Move(Vector2 moveAmount, float input, bool standingOnPlatform = false)
    {
        // Update and reset each frame.
        UpdateRaycastOrigins();
        collisionInfo.Reset();
        collisionInfo.velocityOld = moveAmount;

        // Used for passing through platforms.
        playerVerticalInput = input;

        if (moveAmount.y < 0)
        {
            DescendSlope(ref moveAmount);
        }

        if (moveAmount.x != 0)
        {
            collisionInfo.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        CheckHorizontalCollisions(ref moveAmount);

        if (moveAmount.y != 0)
        {
            CheckVerticalCollisions(ref moveAmount);
        }

        transform.Translate(moveAmount);

        // Marks the object as standing on a platform if specified.
        if (standingOnPlatform)
        {
            collisionInfo.below = true;
        }
    }

    /// <summary>
    /// Checks horizontal and vertical collisions separately for more precise slope/ground calculations.
    /// </summary>
    /// <param name="moveAmount"></param>
    private void CheckHorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = collisionInfo.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;

        if (Mathf.Abs(moveAmount.x) < SKIN_WIDTH)
        {
            rayLength = 2 * SKIN_WIDTH;
        }

        // Similar system to vertical checks, but adapted for slope handling.
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigen.bottomLeft : raycastOrigen.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D raycastHit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (!raycastHit)
            {
                continue;
            }

            // Check if the wall is climbable (wall jump allowed).
            if (raycastHit.collider.CompareTag("Escalable"))
            {
                collisionInfo.isAbleToWallJump = true;
            }

            // Skip if colliding with a moving obstacle at distance 0 to allow smooth exit.
            if (raycastHit.distance == 0)
            {
                continue;
            }

            // Handle slope climbing: check angle of surface and compare with slope limits.
            float slopeAngle = Vector2.Angle(raycastHit.normal, Vector2.up);
            if (i == 0 && slopeAngle <= maxSlopeAngle)
            {
                // Reset descending state when transitioning onto a climbable slope.
                if (collisionInfo.isDescendingSlope)
                {
                    collisionInfo.isDescendingSlope = false;
                    moveAmount = collisionInfo.velocityOld;
                }

                // Adjust movement when entering a new slope.
                float distanceToSlopeStart = 0;
                if (slopeAngle != collisionInfo.slopeAngleOld)
                {
                    distanceToSlopeStart = raycastHit.distance - SKIN_WIDTH;
                    moveAmount.x -= distanceToSlopeStart * directionX;
                }

                // Apply climbing movement.
                ClimbSlope(ref moveAmount, slopeAngle, raycastHit.normal);
                moveAmount.x += distanceToSlopeStart * directionX;
            }

            // Apply horizontal collisions when not climbing or slope is too steep.
            if (!collisionInfo.isClimbingSlope || slopeAngle > maxSlopeAngle)
            {
                moveAmount.x = (raycastHit.distance - SKIN_WIDTH) * directionX;
                rayLength = raycastHit.distance;

                // Correct vertical movement when climbing.
                if (collisionInfo.isClimbingSlope)
                {
                    moveAmount.y = Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                }

                collisionInfo.left = directionX == -1;
                collisionInfo.right = directionX == 1;
            }

        }
    }

    private void CheckVerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + SKIN_WIDTH;
        Vector2 rayOrigin;
        RaycastHit2D raycastHit;

        // Cast vertical rays to detect collisions above/below.
        for (int i = 0; i < verticalRayCount; i++)
        {
            rayOrigin = (directionY == -1) ? raycastOrigen.bottomLeft : raycastOrigen.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);

            raycastHit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (!raycastHit)
            {
                continue;
            }
            // Handle passthrough platforms.
            if (raycastHit.collider.CompareTag("Atravesable"))
            {
                if (directionY == 1 || raycastHit.distance == 0)
                {
                    continue;
                }

                // Allow descending through platform when pressing down.
                if (playerVerticalInput == -1)
                {
                    if (collisionInfo.platformStanding.Equals(raycastHit.collider))
                    {
                        continue;
                    }
                    collisionInfo.platformStanding = raycastHit.collider;
                }
            }
            // Store reference to standing platform collider.
            collisionInfo.platformStanding = raycastHit.collider;

            // Apply vertical adjustment.
            moveAmount.y = (raycastHit.distance - SKIN_WIDTH) * directionY;
            rayLength = raycastHit.distance;

            // Correct horizontal movement when climbing.
            if (collisionInfo.isClimbingSlope)
            {
                moveAmount.x = moveAmount.y / Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Math.Sign(moveAmount.x);
            }

            // Update collision state above/below.
            collisionInfo.below = directionY == -1;
            collisionInfo.above = directionY == 1;
        }

        if (!collisionInfo.isClimbingSlope)
        {
            return;
        }

        float directionX = Mathf.Sign(moveAmount.x);
        rayLength = Mathf.Abs(moveAmount.x) + SKIN_WIDTH;
        rayOrigin = ((directionX == -1) ? raycastOrigen.bottomLeft : raycastOrigen.bottomRight) + Vector2.up * moveAmount.y;
        raycastHit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
        if (!raycastHit)
        {
            return;
        }

        // Correct movement when transitioning between slopes.
        float slopeAngle = Vector2.Angle(raycastHit.normal, Vector2.up);
        if (slopeAngle != collisionInfo.slopeAngle)
        {
            moveAmount.x = (raycastHit.distance - SKIN_WIDTH) * directionX;
            collisionInfo.slopeAngle = slopeAngle;
            collisionInfo.slopeNormal = raycastHit.normal;
        }
    }


    // Slope climbing logic
    private void ClimbSlope(ref Vector2 velocity, float slopeAngle, Vector2 slopeNormal)
    {
        // Calculate movement along the slope
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

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
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigen.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigen.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + SKIN_WIDTH, collisionMask);
        SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
        SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);

        if (collisionInfo.isSlidingDownMaxSlope)
        {
            return;
        }

        // Handle normal slope descent
        float directionX = Mathf.Sign(moveAmount.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigen.bottomRight : raycastOrigen.bottomLeft;
        RaycastHit2D raycastHit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
        if (!raycastHit)
        {
            return;
        }

        float slopeAngle = Vector2.Angle(raycastHit.normal, Vector2.up);
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

        float moveDistance = Mathf.Abs(moveAmount.x);
        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
        moveAmount.y -= descendVelocityY;

        collisionInfo.slopeAngle = slopeAngle;
        collisionInfo.isDescendingSlope = true;
        collisionInfo.below = true;
        collisionInfo.slopeNormal = raycastHit.normal;
    }

    /// <summary>
    /// Handles sliding down steep slopes that cannot be climbed based on the angle criteria.
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="MoveAmount"></param>
    private void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 MoveAmount)
    {
        if (!hit)
        {
            return;
        }

        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
        bool isTooSteep = (slopeAngle > maxSlopeAngle);
        if(!isTooSteep)
        {
            return;
        }

        MoveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(MoveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);
        collisionInfo.slopeAngle = slopeAngle;
        collisionInfo.isSlidingDownMaxSlope = true;
        collisionInfo.slopeNormal = hit.normal;
    }
}
