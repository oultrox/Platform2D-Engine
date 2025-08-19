using UnityEngine;

[RequireComponent(typeof(PlatformMotor2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = 0.4f;
    public float moveSpeed = 6;
    public float accelerationOnGround = 0.03f;
    public float accelerationOnAir = 0.1f;

    [Header("Wall Settings")]
    public float wallsSlideSpeedMax = 3;
    public float wallStickTime = 0.25f;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    private int wallDirX;
    private float timeWallUnstick;
    private float maxJumpVelocity;
    private float minJumpVelocity;
    private float gravity;

    private float targetVelocityX;
    private float velocityXSmoothing;
    private Vector3 velocity;

    // Components
    private PlatformMotor2D playerMotor;
    private PlayerInput playerInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerMotor = GetComponent<PlatformMotor2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        SetGravity();
    }
    
    private void Update()
    {
        HandleMovement();
        HandleWallSliding();
        HandleJumping();

        playerMotor.Move(velocity * Time.deltaTime, playerInput.MoveInput.y);

        HandleCollisions();
        HandleAnimations();

        // consume one-shot inputs so they don’t get reused this frame
        playerInput.ConsumeJumpInputs();
    }

    private void SetGravity()
    {
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity * timeToJumpApex);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }
    
    private void HandleMovement()
    {
        float horizontalInput = playerInput.MoveInput.x;
        targetVelocityX = horizontalInput * moveSpeed;
        velocity.x = Mathf.SmoothDamp(
            velocity.x,
            targetVelocityX,
            ref velocityXSmoothing,
            (playerMotor.collisionInfo.below ? accelerationOnGround : accelerationOnAir)
        );

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleWallSliding()
    {
        wallDirX = (playerMotor.collisionInfo.left) ? -1 : 1;

        if (playerMotor.collisionInfo.isAbleToWallJump)
        {
            if (((playerMotor.collisionInfo.left && velocity.x < 0) ||
                (playerMotor.collisionInfo.right && velocity.x > 0)) &&
                !playerMotor.collisionInfo.isSlidingDownMaxSlope &&
                !playerMotor.collisionInfo.below)
            {
                playerMotor.collisionInfo.isStickedToWall = true;
            }
        }

        if (!playerMotor.collisionInfo.isStickedToWall)
            return;

        if (!playerMotor.collisionInfo.left && !playerMotor.collisionInfo.right)
            playerMotor.collisionInfo.isStickedToWall = false;

        if (velocity.y < -wallsSlideSpeedMax)
            velocity.y = -wallsSlideSpeedMax;

        float horizontalInput = playerInput.MoveInput.x;

        if (timeWallUnstick > 0)
        {
            velocityXSmoothing = 0;
            velocity.x = 0;

            if (horizontalInput != wallDirX || horizontalInput == 0)
            {
                timeWallUnstick -= Time.deltaTime;
                if (timeWallUnstick <= 0)
                    playerMotor.collisionInfo.isStickedToWall = false;
            }
            else if (horizontalInput == wallDirX)
            {
                timeWallUnstick = wallStickTime;
            }
        }
        else
        {
            timeWallUnstick = wallStickTime;
        }
    }

    private void HandleJumping()
    {
        if (playerInput.JumpPressed)
            JumpRelease();

        if (playerInput.JumpReleased)
            JumpUp();
    }

    void JumpRelease()
    {
        float horizontalInput = playerInput.MoveInput.x;

        // Wall jump
        if (playerMotor.collisionInfo.isStickedToWall)
        {
            if (wallDirX == horizontalInput)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (horizontalInput == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
            playerMotor.collisionInfo.isStickedToWall = false;
        }

        // Normal jump
        if (playerMotor.collisionInfo.below)
        {
            if (playerMotor.collisionInfo.isSlidingDownMaxSlope)
            {
                if (horizontalInput != -Mathf.Sign(playerMotor.collisionInfo.slopeNormal.x))
                {
                    velocity.y = maxJumpVelocity * playerMotor.collisionInfo.slopeNormal.y;
                    velocity.x = maxJumpVelocity * playerMotor.collisionInfo.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    void JumpUp()
    {
        if (velocity.y > minJumpVelocity)
            velocity.y = minJumpVelocity;
    }

    private void HandleCollisions()
    {
        if (playerMotor.collisionInfo.above || playerMotor.collisionInfo.below)
        {
            if (playerMotor.collisionInfo.isSlidingDownMaxSlope)
            {
                velocity.y += playerMotor.collisionInfo.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }

    private void HandleAnimations()
    {
        if (velocity.x != 0)
            spriteRenderer.flipX = velocity.x < 0;

        animator.SetBool("grounded", playerMotor.collisionInfo.below);
        animator.SetFloat("velocityX", Mathf.Abs(targetVelocityX));
    }
}
